using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenBve.Graphics;
using OpenBve.Input;
using OpenTK;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using RouteManager2;
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
#pragma warning restore IDE1006
		
		/// <summary>Stores the current CPU architecture</summary>
		internal static ImageFileMachine CurrentCPUArchitecture;

		/// <summary>The host API used by this program.</summary>
		internal static Host CurrentHost = null;

		/// <summary>Information about the file system organization.</summary>
		internal static FileSystem FileSystem = null;
		
		/// <summary>If the program is to be restarted, this contains the command-line arguments that should be passed to the process, or a null reference otherwise.</summary>
		internal static string RestartArguments = null;

		/// <summary>The random number generator used by this program.</summary>
		internal static readonly Random RandomNumberGenerator = new Random();

		public static GameWindow currentGameWindow;

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
			// Add handler for UI thread exceptions
			Application.ThreadException += (CrashHandler.UIThreadException);

			// Force all WinForms errors to go through handler
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// This handler is for catching non-UI thread exceptions
			AppDomain.CurrentDomain.UnhandledException += (CrashHandler.CurrentDomain_UnhandledException);


			//Determine the current CPU architecture-
			//ARM will generally only support OpenGL-ES
			PortableExecutableKinds peKind;
			typeof(object).Module.GetPEKind(out peKind, out CurrentCPUArchitecture);
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			CurrentHost = new Host();
			if (IntPtr.Size == 4)
			{
				Joysticks = new JoystickManager32();
			}
			else
			{
				Joysticks = new JoystickManager64();	
			}
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args, CurrentHost);
				FileSystem.CreateFileSystem();
			} catch (Exception ex) {
				MessageBox.Show(Translations.GetInterfaceString("errors_filesystem_invalid") + Environment.NewLine + Environment.NewLine + ex.Message, Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}

			Renderer = new NewRenderer();
			Sounds = new Sounds();
			CurrentRoute = new CurrentRoute(CurrentHost, Renderer);
			
			//Platform specific startup checks
			// --- Check if we're running as root, and prompt not to ---
			if (CurrentHost.Platform == HostPlatform.GNULinux && (getuid() == 0 || geteuid() == 0))
			{
				MessageBox.Show(
					"You are currently running as the root user, or via the sudo command." + System.Environment.NewLine +
					"This is a bad idea, please dont!", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}


			// --- load options and controls ---
			try
			{
				Interface.LoadOptions();
			}
			catch
			{
				// ignored
			}
			TrainManager = new TrainManager(CurrentHost, Renderer, Interface.CurrentOptions, FileSystem);
			
			//Switch between SDL2 and native backends; use native backend by default
			var options = new ToolkitOptions();
			if (Interface.CurrentOptions.PreferNativeBackend)
			{
				options.Backend = PlatformBackend.PreferNative;
			}
			Toolkit.Init(options);
			// --- load language ---
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			
			folder = Program.FileSystem.GetDataFolder("Cursors");
			Cursors.LoadCursorImages(folder);
			
			Interface.LoadControls(null, out Interface.CurrentControls);
			folder = Program.FileSystem.GetDataFolder("Controls");
			string file = OpenBveApi.Path.CombineFile(folder, "Default keyboard assignment.controls");
			Control[] controls;
			Interface.LoadControls(file, out controls);
			Interface.AddControls(ref Interface.CurrentControls, controls);
			
			InputDevicePlugin.LoadPlugins(Program.FileSystem);
			
			// --- check the command-line arguments for route and train ---
			formMain.MainDialogResult result = new formMain.MainDialogResult();
			CommandLine.ParseArguments(args, ref result);
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
				string error;
				if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out error, TrainManager, Renderer))
				{
					MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
					throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
				}
				Game.Reset(false);
				bool loaded = false;
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(result.RouteFile))
					{
						object Route = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
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
					MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (!loaded)
				{
					throw new Exception("No plugins capable of loading routefile " + result.RouteFile + " were found.");
				}
				if (!string.IsNullOrEmpty(Interface.CurrentOptions.TrainName)) {
					folder = System.IO.Path.GetDirectoryName(result.RouteFile);
					while (true) {
						string trainFolder = OpenBveApi.Path.CombineDirectory(folder, "Train");
						if (System.IO.Directory.Exists(trainFolder)) {
							try
							{
								folder = OpenBveApi.Path.CombineDirectory(trainFolder, Interface.CurrentOptions.TrainName);
							}
							catch (Exception ex)
							{
								if (ex is ArgumentException)
								{
									break;
								}
							}
							if (System.IO.Directory.Exists(folder)) {
								file = OpenBveApi.Path.CombineFile(folder, "train.dat");
								if (System.IO.File.Exists(file)) {
									result.TrainFolder = folder;
									result.TrainEncoding = System.Text.Encoding.UTF8;
									for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
										if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
											result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
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
			// --- show the main menu if necessary ---
			if (result.RouteFile == null | result.TrainFolder == null) {
				Joysticks.RefreshJoysticks();
				
				// end HACK //
				result = formMain.ShowMainDialog(result);
			} else {
				result.Start = true;
				//Apply translations
				Translations.SetInGameLanguage(Translations.CurrentLanguageCode);
			}
			// --- start the actual program ---
			if (result.Start) {
				if (Initialize()) {
					#if !DEBUG
					try {
						#endif
						MainLoop.StartLoopEx(result);
						#if !DEBUG
					} catch (Exception ex) {
						bool found = false;
						for (int i = 0; i < TrainManager.Trains.Length; i++) {
							if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
								if (TrainManager.Trains[i].Plugin.LastException != null) {
									CrashHandler.LoadingCrash(ex.Message, true);
									MessageBox.Show("The train plugin " + TrainManager.Trains[i].Plugin.PluginTitle + " caused a runtime exception: " + TrainManager.Trains[i].Plugin.LastException.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
									found = true;
									RestartArguments = "";
									break;
								}
							}
						}
						if (!found)
						{
							if (ex is System.DllNotFoundException)
							{
								Interface.AddMessage(MessageType.Critical, false, "The required system library " + ex.Message + " was not found on the system.");
								switch (ex.Message)
								{
									case "libopenal.so.1":
										MessageBox.Show("openAL was not found on this system. \n Please install libopenal1 via your distribtion's package management system.", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
										break;
									default:
										MessageBox.Show("The required system library " + ex.Message + " was not found on this system.", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
#endif
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
					System.Diagnostics.Process.Start(System.IO.File.Exists(FileSystem.RestartProcess) ? FileSystem.RestartProcess : Application.ExecutablePath, arguments);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message + "\n\nProcess = " + FileSystem.RestartProcess + "\nArguments = " + arguments, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		
		/// <summary>Initializes the program. A matching call to deinitialize must be made when the program is terminated.</summary>
		/// <returns>Whether the initialization was successful.</returns>
		private static bool Initialize()
		{
			string error;
			if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out error, TrainManager, Renderer)) {
				MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			Joysticks.RefreshJoysticks();
			// begin HACK //
			Renderer.Camera.VerticalViewingAngle = 45.0.ToRadians();
			Renderer.Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Renderer.Camera.VerticalViewingAngle) * Renderer.Screen.AspectRatio);
			Renderer.Camera.OriginalVerticalViewingAngle = Renderer.Camera.VerticalViewingAngle;
			Renderer.Camera.ExtraViewingDistance = 50.0;
			Renderer.Camera.ForwardViewingDistance = (double)Interface.CurrentOptions.ViewingDistance;
			Renderer.Camera.BackwardViewingDistance = 0.0;
			Program.CurrentRoute.CurrentBackground.BackgroundImageDistance = (double)Interface.CurrentOptions.ViewingDistance;
			// end HACK //
			string programVersion = @"v" + Application.ProductVersion + OpenBve.Program.VersionSuffix;
			FileSystem.ClearLogFile(programVersion);
			return true;
		}
		
		/// <summary>Deinitializes the program.</summary>
		private static void Deinitialize()
		{
			string error;
			Program.CurrentHost.UnloadPlugins(out error);
			Sounds.Deinitialize();
			if (currentGameWindow != null)
			{
				currentGameWindow.Dispose();
			}
		}

	}
}
