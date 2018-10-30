using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTK;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;

namespace OpenBve {
	/// <summary>Provides methods for starting the program, including the Main procedure.</summary>
	internal static partial class Program {

		/// <summary>Gets the UID of the current user if running on a Unix based system</summary>
		/// <returns>The UID</returns>
		/// <remarks>Used for checking if we are running as ROOT (don't!)</remarks>
		[DllImport("libc")]
#pragma warning disable IDE1006 // Suppress the VS2017 naming style rule, as this is an external syscall
		private static extern uint getuid();
#pragma warning restore IDE1006

		// --- members ---

		/// <summary>Whether the program is currently running on Mono. This is of interest for the Windows Forms main menu which behaves differently on Mono than on Microsoft .NET.</summary>
		internal static bool CurrentlyRunningOnMono = false;
		
		/// <summary>Whether the program is currently running on Microsoft Windows or compatible. This is of interest for whether running Win32 plugins is possible.</summary>
		internal static bool CurrentlyRunningOnWindows = false;

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

		/// <summary>Whether the program will generate a considerably more verbose debug log (WIP)</summary>
		internal static bool GenerateDebugLogging = false;

		public static GameWindow currentGameWindow;

		internal static JoystickManager Joysticks;
		
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
			//--- determine the running environment ---
			//I wonder if disabling this hack will stop the craashing on Linux....
			CurrentlyRunningOnMono = Type.GetType("Mono.Runtime") != null;
			//Doesn't appear to, but Mono have fixed the button appearance bug
			CurrentlyRunningOnWindows = Environment.OSVersion.Platform == PlatformID.Win32S | Environment.OSVersion.Platform == PlatformID.Win32Windows | Environment.OSVersion.Platform == PlatformID.Win32NT;
			Joysticks = new JoystickManager();
			CurrentHost = new Host();
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args);
				FileSystem.CreateFileSystem();
			} catch (Exception ex) {
				MessageBox.Show(Translations.GetInterfaceString("errors_filesystem_invalid") + Environment.NewLine + Environment.NewLine + ex.Message, Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}

			//Platform specific startup checks
			if (CurrentlyRunningOnMono && !CurrentlyRunningOnWindows)
			{
				// --- Check if we're running as root, and prompt not to ---
				if (getuid() == 0)
				{
					MessageBox.Show(
						"You are currently running as the root user." + System.Environment.NewLine +
						"This is a bad idea, please dont!", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else
			{
				if (!System.IO.File.Exists(System.IO.Path.Combine(Environment.SystemDirectory, "OpenAL32.dll")))
				{
					
					MessageBox.Show(
						"OpenAL was not found on your system, and will now be installed." + System.Environment.NewLine + System.Environment.NewLine +
						"Please follow the install prompts.", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);

					ProcessStartInfo info = new ProcessStartInfo(Path.Combine(FileSystem.DataFolder, "Dependencies\\Win32\\oalinst.exe"));
					info.UseShellExecute = true;
					if (Environment.OSVersion.Version.Major >= 6)
					{
						info.Verb = "runas";
					}
					try
					{
						Process p = Process.Start(info);
						if (p != null)
						{
							p.WaitForExit();
						}
						else
						{
							//For unknown reasons, the process failed to trigger, but did not raise an exception itself
							//Throw one
							throw new Win32Exception();
						}
					}
					catch (Win32Exception)
					{
						MessageBox.Show(
						"An error occured during OpenAL installation....", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					
				}
			}


			// --- load options and controls ---
			Interface.LoadOptions();
			//Switch between SDL2 and native backends; use native backend by default
			var options = new ToolkitOptions();
			if (Interface.CurrentOptions.PreferNativeBackend)
			{
				options.Backend = PlatformBackend.PreferNative;
			}
			Toolkit.Init(options);
			// --- load language ---
			{
				string folder = Program.FileSystem.GetDataFolder("Languages");
				Translations.LoadLanguageFiles(folder);
			}
			Interface.LoadControls(null, out Interface.CurrentControls);
			{
				string folder = Program.FileSystem.GetDataFolder("Controls");
				string file = OpenBveApi.Path.CombineFile(folder, "Default keyboard assignment.controls");
				Interface.Control[] controls;
				Interface.LoadControls(file, out controls);
				Interface.AddControls(ref Interface.CurrentControls, controls);
			}
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
			if (result.RouteFile != null & result.TrainFolder == null) {
				bool isRW = string.Equals(System.IO.Path.GetExtension(result.RouteFile), ".rw", StringComparison.OrdinalIgnoreCase);
				CsvRwRouteParser.ParseRoute(result.RouteFile, isRW, result.RouteEncoding, null, null, null, true);
				if (!string.IsNullOrEmpty(Game.TrainName)) {
					string folder = System.IO.Path.GetDirectoryName(result.RouteFile);
					while (true) {
						string trainFolder = OpenBveApi.Path.CombineDirectory(folder, "Train");
						if (System.IO.Directory.Exists(trainFolder)) {
							folder = OpenBveApi.Path.CombineDirectory(trainFolder, Game.TrainName);
							if (System.IO.Directory.Exists(folder)) {
								string file = OpenBveApi.Path.CombineFile(folder, "train.dat");
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
		private static bool Initialize() {
			if (!Plugins.LoadPlugins()) {
				return false;
			}
			
			Joysticks.RefreshJoysticks();
			// begin HACK //
			
			//One degree in radians
			const double degrees = 0.0174532925199433;
			World.VerticalViewingAngle = 45.0 * degrees;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			World.OriginalVerticalViewingAngle = World.VerticalViewingAngle;
			World.ExtraViewingDistance = 50.0;
			World.ForwardViewingDistance = (double)Interface.CurrentOptions.ViewingDistance;
			World.BackwardViewingDistance = 0.0;
			World.BackgroundImageDistance = (double)Interface.CurrentOptions.ViewingDistance;
			// end HACK //
			FileSystem.ClearLogFile();
			return true;
		}
		
		/// <summary>Deinitializes the program.</summary>
		private static void Deinitialize() {
			Plugins.UnloadPlugins();
			Sounds.Deinitialize();
			if (currentGameWindow != null)
			{
				currentGameWindow.Dispose();
			}
		}

	}
}
