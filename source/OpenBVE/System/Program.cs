using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenBve.Graphics;
using OpenBve.Input;
using OpenBveApi;
using OpenTK;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using RouteManager2;
using System.Diagnostics;
using Control = OpenBveApi.Interface.Control;

namespace OpenBve {
	/// <summary>Provides methods for starting the program, including the Main procedure.</summary>
	internal static partial class Program {
#pragma warning disable IDE1006 // Suppress the VS2017 naming style rule, as this is an external syscall
		/// <summary>Gets the UID of the current user if running on a Unix based system</summary>
		/// <returns>The UID</returns>
		/// <remarks>Used for checking if we are running as ROOT (don't!)</remarks>
		[DllImport("libc")]
		private static extern uint getuid();

		/// <summary>Gets the UID of the current user if running on a Unix based system</summary>
		/// <returns>The UID</returns>
		/// <remarks>Used for checking if we are running as SUDO</remarks>
		[DllImport("libc")]
		private static extern uint geteuid();

		[DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();
#pragma warning restore IDE1006

		/// <summary>Stores the current CPU architecture</summary>
		internal static ImageFileMachine CurrentCPUArchitecture;

		/// <summary>The host API used by this program.</summary>
		internal static Host CurrentHost;

		/// <summary>Information about the file system organization.</summary>
		internal static FileSystem FileSystem;
		
		/// <summary>If the program is to be restarted, this contains the command-line arguments that should be passed to the process, or a null reference otherwise.</summary>
		internal static string RestartArguments;

		/// <summary>The random number generator used by this program.</summary>
		internal static readonly Random RandomNumberGenerator = new Random();

		internal static JoystickManager Joysticks;

		internal static NewRenderer Renderer;

		internal static Sounds Sounds;

		internal static CurrentRoute CurrentRoute;

		internal static TrainManager TrainManager;

		// --- functions ---
		
		/// <summary>Is executed when the program starts.</summary>
		/// <param name="args">The command-line arguments.</param>
		[STAThread]
		private static void Main(string[] args) {
			// --- load options and controls ---
			CurrentHost = new Host();
			try
			{
				FileSystem = FileSystem.FromCommandLineArgs(args, CurrentHost);
				FileSystem.CreateFileSystem();
				Interface.LoadOptions();
			}
			catch
			{
				// ignored
			}
			//Switch between SDL2 and native backends; use native backend by default
			var options = new ToolkitOptions();
			
			if (CurrentHost.Platform == HostPlatform.FreeBSD)
			{
				// The OpenTK X11 backend is broken on FreeBSD, so force SDL2
				options.Backend = PlatformBackend.Default;
			}
			else if (Interface.CurrentOptions.PreferNativeBackend)
			{
				options.Backend = PlatformBackend.PreferNative;
			}
			Toolkit.Init(options);
			
			// Add handler for UI thread exceptions
			Application.ThreadException += (CrashHandler.UIThreadException);

			// Force all WinForms errors to go through handler
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// This handler is for catching non-UI thread exceptions
			AppDomain.CurrentDomain.UnhandledException += (CrashHandler.CurrentDomain_UnhandledException);


			//Determine the current CPU architecture-
			//ARM will generally only support OpenGL-ES
			typeof(object).Module.GetPEKind(out PortableExecutableKinds _, out CurrentCPUArchitecture);
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			if (IntPtr.Size == 4)
			{
				Joysticks = new JoystickManager32();
			}
			else
			{
				Joysticks = new JoystickManager64();	
			}

			if (CurrentHost.Platform == HostPlatform.FreeBSD)
			{
				// BSD seems to need this called at this point to avoid crashing
				// https://github.com/leezer3/OpenBVE/issues/712
				Joysticks.RefreshJoysticks();
			}
			
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args, CurrentHost);
				FileSystem.CreateFileSystem();
			} catch (Exception ex) {
				Program.ShowMessageBox(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","filesystem_invalid"}) + Environment.NewLine + Environment.NewLine + ex.Message, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}));
				return;
			}

			Renderer = new NewRenderer(CurrentHost, Interface.CurrentOptions, FileSystem);
			Sounds = new Sounds(CurrentHost);
			CurrentRoute = new CurrentRoute(CurrentHost, Renderer);
			
			//Platform specific startup checks
			// --- Check if we're running as root, and prompt not to ---
			if ((CurrentHost.Platform == HostPlatform.GNULinux || CurrentHost.Platform == HostPlatform.FreeBSD) && (getuid() == 0 || geteuid() == 0))
			{
				Program.ShowMessageBox(
					@"You are currently running as the root user, or via the sudo command." + Environment.NewLine +
					@"This is a bad idea, please dont!", Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}));
			}


			
			TrainManager = new TrainManager(CurrentHost, Renderer, Interface.CurrentOptions, FileSystem);
			
			// --- load language ---
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			
			folder = Program.FileSystem.GetDataFolder("Cursors");
			LibRender2.AvailableCursors.LoadCursorImages(Program.Renderer, folder);
			
			Interface.LoadControls(null, out Interface.CurrentControls);
			folder = Program.FileSystem.GetDataFolder("Controls");
			string file = Path.CombineFile(folder, "Default keyboard assignment.controls");
			Interface.LoadControls(file, out Control[] controls);
			Interface.AddControls(ref Interface.CurrentControls, controls);
			
			InputDevicePlugin.LoadPlugins(Program.FileSystem);
			
			// --- check the command-line arguments for route and train ---
			LaunchParameters result = CommandLine.ParseArguments(args);
			// --- check whether route and train exist ---
			if (result.RouteFile != null) {
				if (!System.IO.File.Exists(result.RouteFile))
				{
					result.RouteFile = null;
				}
			}
			if (result.TrainFolder != null) {
				if (!System.IO.Directory.Exists(result.TrainFolder)) {
					result.TrainFolder = null;
				}
			}
			// --- if a route was provided but no train, try to use the route default ---
			if (result.RouteFile != null & result.TrainFolder == null)
			{
				if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out string error, TrainManager, Renderer))
				{
					Program.ShowMessageBox(error, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "program", "title" }));
					throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
				}
				Game.Reset(false);
				bool loaded = false;
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(result.RouteFile))
					{
						object Route = Program.CurrentRoute; //must cast to allow us to use the ref keyword.
						Program.CurrentHost.Plugins[i].Route.LoadRoute(result.RouteFile, result.RouteEncoding, null, null, null, true, ref Route);
						Program.CurrentRoute = (CurrentRoute) Route;
						Program.Renderer.Lighting.OptionAmbientColor = CurrentRoute.Atmosphere.AmbientLightColor;
						Program.Renderer.Lighting.OptionDiffuseColor = CurrentRoute.Atmosphere.DiffuseLightColor;
						Program.Renderer.Lighting.OptionLightPosition = CurrentRoute.Atmosphere.LightPosition;
						loaded = true;
						break;
					}
				}

				if (!CurrentHost.UnloadPlugins(out error))
				{
					Program.ShowMessageBox(error, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "program", "title" }));
				}
				if (!loaded)
				{
					throw new Exception("No plugins capable of loading routefile " + result.RouteFile + " were found.");
				}
				if (!string.IsNullOrEmpty(Interface.CurrentOptions.TrainName)) {
					folder = Path.GetDirectoryName(result.RouteFile);
					while (true) {
						string trainFolder = Path.CombineDirectory(folder, "Train");
						if (System.IO.Directory.Exists(trainFolder)) {
							try
							{
								folder = Path.CombineDirectory(trainFolder, Interface.CurrentOptions.TrainName);
							}
							catch (Exception ex)
							{
								if (ex is ArgumentException)
								{
									break;
								}
							}
							if (System.IO.Directory.Exists(folder)) {
								file = Path.CombineFile(folder, "train.dat");
								if (!System.IO.File.Exists(file))
								{
									file = Path.CombineFile(folder, "train.xml");
								}
								if (System.IO.File.Exists(file)) {
									result.TrainFolder = folder;
									result.TrainEncoding = System.Text.Encoding.UTF8;
									for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
										if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
											result.TrainEncoding = System.Text.Encoding.GetEncoding((int)Interface.CurrentOptions.TrainEncodings[j].Codepage);
											break;
										}
									}
								}
							} break;
						}
						if (folder == null) continue;
						System.IO.DirectoryInfo info = System.IO.Directory.GetParent(folder);
						if (info != null) {
							folder = info.FullName;
						} else {
							break;
						}
					}
				}
				Game.Reset(false);
			}
			
			// --- show the main WinForms menu if necessary ---
			if (result.RouteFile == null | result.TrainFolder == null) {
				Joysticks.RefreshJoysticks();

				if (CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
				{
					//WinForms are not supported on 64-bit Apple, so show the experimental GL menu
					result.ExperimentalGLMenu = true;
				}
				else
				{
					if (!result.ExperimentalGLMenu)
					{
						result = formMain.ShowMainDialog(result);
					}
				}
			} else {
				result.Start = true;
				//Apply translations
				Translations.SetInGameLanguage(Translations.CurrentLanguageCode);
			}

			if (CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				// Tell Windows that the main game is managing it's own DPI
				SetProcessDPIAware();
			}

			if (result.ExperimentalGLMenu)
			{
				result.Start = true;
				result.RouteFile = null;
				result.TrainFolder = null;
				Translations.SetInGameLanguage(Translations.CurrentLanguageCode);
			}
			
			// --- start the actual program ---
			if (result.Start) {
				if (Initialize()) {
					try {
						MainLoop.StartLoopEx(result);
					} catch (Exception ex) {
						bool found = false;
						for (int i = 0; i < TrainManager.Trains.Count; i++) {
							if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
								if (TrainManager.Trains[i].Plugin.LastException != null) {
									CrashHandler.LoadingCrash(ex.Message, true);
									Program.ShowMessageBox(@"The train plugin " + TrainManager.Trains[i].Plugin.PluginTitle + @" caused a runtime exception: " + TrainManager.Trains[i].Plugin.LastException.Message, Application.ProductName);
									found = true;
									RestartArguments = "";
									break;
								}
							}
						}
						if (!found)
						{
							if (ex is DllNotFoundException)
							{
								Interface.AddMessage(MessageType.Critical, false, "The required system library " + ex.Message + " was not found on the system.");
								switch (ex.Message)
								{
									case "libopenal.so.1":
										Program.ShowMessageBox(@"openAL was not found on this system. \n Please install libopenal1 via your distribution's package management system.", Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "program", "title" }));
										break;
									default:
										Program.ShowMessageBox(@"The required system library " + ex.Message + @" was not found on this system.", Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "program", "title" }));
										break;
								}
							}
							else
							{
								Interface.AddMessage(MessageType.Critical, false, "The route and train loader encountered the following critical error: " + ex.Message);
								CrashHandler.LoadingCrash(ex + Environment.StackTrace, false);
							}
							RestartArguments = "";
						}
					}
				}
				Deinitialize();
			}
			// --- restart the program if necessary ---
			if (RestartArguments != null) {
				string arguments;
				if (FileSystem.RestartArguments.Length != 0 & RestartArguments.Length != 0) {
					arguments = FileSystem.RestartArguments + " " + RestartArguments;
				} else {
					arguments = FileSystem.RestartArguments + RestartArguments;
				}
				try {
					Process.Start(System.IO.File.Exists(FileSystem.RestartProcess) ? FileSystem.RestartProcess : Application.ExecutablePath, arguments);
					if (CurrentHost.MonoRuntime)
					{
						// Forcefully terminate the original process once the new one has triggered, otherwise we hang around...
						Environment.Exit(0);
					}
				} catch (Exception ex) {
					ShowMessageBox(ex.Message + @"\n\nProcess = " + FileSystem.RestartProcess + @"\nArguments = " + arguments, Application.ProductName);
				}
			}
			else
			{
				Deinitialize();
			}
		}

		
		/// <summary>Initializes the program. A matching call to deinitialize must be made when the program is terminated.</summary>
		/// <returns>Whether the initialization was successful.</returns>
		private static bool Initialize()
		{
			if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out string error, TrainManager, Renderer)) {
				Program.ShowMessageBox(error, @"OpenBVE");
				return false;
			}
			
			Joysticks.RefreshJoysticks();
			// begin HACK //
			Renderer.Camera.VerticalViewingAngle = 45.0.ToRadians();
			Renderer.Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Renderer.Camera.VerticalViewingAngle) * Renderer.Screen.AspectRatio);
			Renderer.Camera.OriginalVerticalViewingAngle = Renderer.Camera.VerticalViewingAngle;
			Renderer.Camera.ExtraViewingDistance = 50.0;
			Renderer.Camera.ForwardViewingDistance = Interface.CurrentOptions.ViewingDistance;
			Renderer.Camera.BackwardViewingDistance = 0.0;
			Program.CurrentRoute.CurrentBackground.BackgroundImageDistance = Interface.CurrentOptions.ViewingDistance;
			// end HACK //
			string programVersion = @"v" + Application.ProductVersion + OpenBve.Program.VersionSuffix;
			FileSystem.ClearLogFile(programVersion);
			return true;
		}
		
		/// <summary>Deinitializes the program.</summary>
		private static void Deinitialize()
		{
			Program.CurrentHost.UnloadPlugins(out _);
			Sounds.DeInitialize();
			Renderer.DeInitialize();
		}

		internal static void ShowMessageBox(string messageText, string captionText)
		{
			if (CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
			{
				// MessageBox.Show does not work on OS-X 64-bit, so let's bodge the System Events dialog
				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/usr/bin/osascript",
						Arguments = $"-e 'tell app \"System Events\" to display dialog \"{messageText}\"'",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				proc.Start();
				proc.WaitForExit();
			}
			else
			{
				MessageBox.Show(messageText, captionText, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

	}
}
