﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenTK;

namespace OpenBve {
	/// <summary>Provides methods for starting the program, including the Main procedure.</summary>
	internal static partial class Program {

		/// <summary>Gets the UID of the current user if running on a Unix based system</summary>
		/// <returns>The UID</returns>
		[DllImport("libc")]
		public static extern uint getuid();

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
		internal static Random RandomNumberGenerator = new Random();

		/// <summary>Whether the program will generate a considerably more verbose debug log (WIP)</summary>
		internal static bool GenerateDebugLogging = false;

		public static GameWindow currentGameWindow;
		
		// --- functions ---
		
		/// <summary>Is executed when the program starts.</summary>
		/// <param name="args">The command-line arguments.</param>
		[STAThread]
		private static void Main(string[] args) {

#if !DEBUG            
			// Add handler for UI thread exceptions
			Application.ThreadException += new ThreadExceptionEventHandler(CrashHandler.UIThreadException);

			// Force all WinForms errors to go through handler
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// This handler is for catching non-UI thread exceptions
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler.CurrentDomain_UnhandledException);
#endif

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
			CurrentHost = new Host();
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args);
				FileSystem.CreateFileSystem();
			} catch (Exception ex) {
				MessageBox.Show(Interface.GetInterfaceString("errors_filesystem_invalid") + Environment.NewLine + Environment.NewLine + ex.Message, Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
						"This is a bad idea, please dont!", Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else
			{
				if (!System.IO.File.Exists(System.IO.Path.Combine(Environment.SystemDirectory, "OpenAL32.dll")))
				{
					
					MessageBox.Show(
						"OpenAL was not found on your system, and will now be installed." + System.Environment.NewLine + System.Environment.NewLine +
						"Please follow the install prompts.", Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);

					ProcessStartInfo info = new ProcessStartInfo(System.IO.Path.Combine(FileSystem.DataFolder, "Dependencies\\Win32\\oalinst.exe"));
					info.UseShellExecute = true;
					if (Environment.OSVersion.Version.Major >= 6)
					{
						info.Verb = "runas";
					}
					try
					{
						System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
						p.WaitForExit();
					}
					catch (Win32Exception)
					{
						MessageBox.Show(
						"An error occured during OpenAL installation....", Interface.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
				try
				{
					string[] LanguageFiles = Directory.GetFiles(folder, "*.cfg");
					foreach (var File in LanguageFiles)
					{
						Interface.AddLanguage(File);
					}
				}
				catch
				{
					MessageBox.Show(@"An error occured whilst attempting to load the default language files.");
					//Environment.Exit(0);
				}
			}
			Interface.LoadControls(null, out Interface.CurrentControls);
			{
				string folder = Program.FileSystem.GetDataFolder("Controls");
				string file = OpenBveApi.Path.CombineFile(folder, "Default keyboard assignment.controls");
				Interface.Control[] controls;
				Interface.LoadControls(file, out controls);
				Interface.AddControls(ref Interface.CurrentControls, controls);
			}
			
			// --- check the command-line arguments for route and train ---
			formMain.MainDialogResult result = new formMain.MainDialogResult();
			for (int i = 0; i < args.Length; i++) {
				if (args[i].StartsWith("/route=", StringComparison.OrdinalIgnoreCase)) {
					result.RouteFile = args[i].Substring(7);
					result.RouteEncoding = System.Text.Encoding.UTF8;
					for (int j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
						if (string.Compare(Interface.CurrentOptions.RouteEncodings[j].Value, result.RouteFile, StringComparison.InvariantCultureIgnoreCase) == 0) {
							result.RouteEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.RouteEncodings[j].Codepage);
							break;
						}
					}
				} else if (args[i].StartsWith("/train=", StringComparison.OrdinalIgnoreCase)) {
					result.TrainFolder = args[i].Substring(7);
					result.TrainEncoding = System.Text.Encoding.UTF8;
					for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
						if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
							result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
							break;
						}
					}
				}
			}
			// --- check whether route and train exist ---
			if (result.RouteFile != null) {
				if (!System.IO.File.Exists(result.RouteFile)) {
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
						//Thread.Sleep(20);
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
							MessageBox.Show("The route and train loader encountered the following critical error: " + Environment.NewLine + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
							CrashHandler.LoadingCrash(ex.ToString(), false);
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
					System.Diagnostics.Process.Start(FileSystem.RestartProcess, arguments);
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
			
			if (!Screen.Initialize()) {
				MessageBox.Show("SDL failed to initialize the video subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
			ClearLogFile();
			return true;
		}
		
		/// <summary>Deinitializes the program.</summary>
		private static void Deinitialize() {
			Plugins.UnloadPlugins();
			Sounds.Deinitialize();
			Screen.Deinitialize();
			if (currentGameWindow != null)
			{
				currentGameWindow.Dispose();
			}
		}
				
		/// <summary>Clears the log file.</summary>
		internal static void ClearLogFile() {
			try {
				string file = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "log.txt");
				System.IO.File.WriteAllText(file, @"openBVE Log: " + DateTime.Now + Environment.NewLine + Environment.NewLine, new System.Text.UTF8Encoding(true));
			} catch { }
		}
		
		/// <summary>Appends the specified text to the log file.</summary>
		/// <param name="text">The text.</param>
		internal static void AppendToLogFile(string text) {
			try {
				string file = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "log.txt");
				System.IO.File.AppendAllText(file, DateTime.Now.ToString("HH:mm:ss") + @"  " + text + Environment.NewLine, new System.Text.UTF8Encoding(false));
			} catch { }
		}

	}
}
