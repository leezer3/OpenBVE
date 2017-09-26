using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OpenBve {
	/// <summary>Represents the program's organization of files and folders.</summary>
	internal class FileSystem {
		
		
		// --- members ---
		
		/// <summary>The location of the application data, including, among others, Compatibility, Flags and Languages.</summary>
		internal string DataFolder;
		
		/// <summary>The locations of managed content.</summary>
		internal string[] ManagedContentFolders;
		
		/// <summary>The location where to save user settings, including settings.cfg and controls.cfg.</summary>
		internal string SettingsFolder;
		
		/// <summary>The initial location of the Railway/Route folder.</summary>
		internal string InitialRouteFolder;

		/// <summary>The initial location of the Train folder.</summary>
		internal string InitialTrainFolder;
		
		/// <summary>The location of the process to execute on restarting the program.</summary>
		internal string RestartProcess;
		
		/// <summary>The arguments to supply to the process on restarting the program.</summary>
		internal string RestartArguments;

		/// <summary>The location to which packaged routes will be installed</summary>
		internal string RouteInstallationDirectory;

		/// <summary>The location to which packaged trains will be installed</summary>
		internal string TrainInstallationDirectory;

		/// <summary>The location to which packaged other items will be installed</summary>
		internal string OtherInstallationDirectory;
		
		/// <summary>The location to which Loksim3D packages will be installed</summary>
		internal string LoksimPackageInstallationDirectory;

		/// <summary>Any lines loaded from the filesystem.cfg which were not understood</summary>
		internal string[] NotUnderstoodLines;

		/// <summary>The version of the filesystem</summary>
		internal int Version;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this class with default locations.</summary>
		internal FileSystem() {
			string assemblyFile = Assembly.GetExecutingAssembly().Location;
			string assemblyFolder = Path.GetDirectoryName(assemblyFile);
			//This copy of openBVE is a special string, and should not be localised
			string userDataFolder = OpenBveApi.Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openBVE");
			this.DataFolder = OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data");
			this.ManagedContentFolders = new string[] { OpenBveApi.Path.CombineDirectory(userDataFolder, "ManagedContent") };
			this.SettingsFolder = OpenBveApi.Path.CombineDirectory(userDataFolder, "Settings");
			this.InitialRouteFolder = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Railway"), "Route");
			this.RouteInstallationDirectory = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Railway");
			this.InitialTrainFolder = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Train");
			this.TrainInstallationDirectory = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Train");
			this.OtherInstallationDirectory = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Other");
			this.LoksimPackageInstallationDirectory = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "ManagedContent"), "Loksim3D");
			this.RestartProcess = assemblyFile;
			this.RestartArguments = string.Empty;
		}
		
		
		// --- internal functions ---
		
		/// <summary>Creates the file system information from the command line arguments. If no configuration file is specified in the command line arguments, the default lookup location is used. If no configuration file is found, default values are used.</summary>
		/// <param name="args">The command line arguments.</param>
		/// <returns>The file system information.</returns>
		internal static FileSystem FromCommandLineArgs(string[] args) {
			foreach (string arg in args) {
				if (arg.StartsWith("/filesystem=", StringComparison.OrdinalIgnoreCase)) {
					return FromConfigurationFile(arg.Substring(12));
				}
			}
			string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "filesystem.cfg");
			if (File.Exists(configFile)) {
				return FromConfigurationFile(configFile);
			}
			configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openBVE"), "Settings"), "filesystem.cfg");
			if (File.Exists(configFile))
			{
				return FromConfigurationFile(configFile);
			}	
			return new FileSystem();
			
		}

		internal static void SaveCurrentFileSystemConfiguration()
		{
			string file = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "FileSystem.cfg");
			StringBuilder newLines = new StringBuilder();
			newLines.AppendLine("Version=1");
			try
			{
				if (File.Exists(file))
				{
					string[] lines = File.ReadAllLines(file, Encoding.UTF8);
					for (int i = 0; i < lines.Length; i++)
					{
						string line = ReplacePath(lines[i]);
						
						int equals = line.IndexOf('=');
						if (equals >= 0)
						{
							string key = line.Substring(0, equals).Trim().ToLowerInvariant();
							switch (key)
							{
								case "data":
								case "settings":
								case "initialroute":
								case "initialtrain":
								case "restartprocess":
								case "restartarguments":
									newLines.AppendLine(line);
									break;
							}
						}

					}
				}
				else
				{
					//Create a new filesystem.cfg file using the current filesystem setup as a base
					//Where does the Debian package point it's filesystem.cfg to??
					newLines.AppendLine("Data=" + Program.FileSystem.DataFolder);
					newLines.AppendLine("Settings=" + Program.FileSystem.SettingsFolder);
					newLines.AppendLine("InitialRoute=" + Program.FileSystem.InitialRouteFolder);
					newLines.AppendLine("InitialTrain=" + Program.FileSystem.InitialTrainFolder);
					newLines.AppendLine("RestartProcess=" + Program.FileSystem.RestartProcess);
					newLines.AppendLine("RestartArguments=" + Program.FileSystem.RestartArguments);
				}
				if (Program.FileSystem.RouteInstallationDirectory != null &&
					Directory.Exists(Program.FileSystem.RouteInstallationDirectory))
				{
					newLines.AppendLine("RoutePackageInstall=" + ReplacePath(Program.FileSystem.RouteInstallationDirectory));
				}

				if (Program.FileSystem.TrainInstallationDirectory != null &&
					Directory.Exists(Program.FileSystem.TrainInstallationDirectory))
				{
					newLines.AppendLine("TrainPackageInstall=" + ReplacePath(Program.FileSystem.TrainInstallationDirectory));
				}
				if (Program.FileSystem.OtherInstallationDirectory != null &&
					Directory.Exists(Program.FileSystem.OtherInstallationDirectory))
				{
					newLines.AppendLine("OtherPackageInstall=" + ReplacePath(Program.FileSystem.OtherInstallationDirectory));
				}
				if (Program.FileSystem.LoksimPackageInstallationDirectory != null &&
				    Directory.Exists(Program.FileSystem.LoksimPackageInstallationDirectory))
				{
					newLines.AppendLine("LoksimPackageInstall=" + ReplacePath(Program.FileSystem.LoksimPackageInstallationDirectory));
				}
				if (Program.FileSystem.NotUnderstoodLines != null && Program.FileSystem.NotUnderstoodLines.Length != 0)
				{
					for (int i = 0; i < Program.FileSystem.NotUnderstoodLines.Length; i++)
					{
						newLines.Append(Program.FileSystem.NotUnderstoodLines[i]);
					}
				}
				System.IO.File.WriteAllText(file, newLines.ToString(), new System.Text.UTF8Encoding(true));
			}
			catch
			{
				
			}
		}

		/// <summary> Replaces all instances of absolute paths within a string with their relative equivilants</summary>
		internal static string ReplacePath(string line)
		{
			try
			{
				line = line.Replace(Assembly.GetExecutingAssembly().Location, "$[AssemblyFile]");
				line = line.Replace(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "$[AssemblyFolder]");
				line = line.Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "$[ApplicationData]");
				line = line.Replace(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					"$[CommonApplicationData]");
				line = line.Replace(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "$[Personal]");
			}
			catch
			{
			}
			return line;
		}

		/// <summary>Creates all folders in the file system that can later be written to.</summary>
		internal void CreateFileSystem() {
			try {
				Directory.CreateDirectory(this.SettingsFolder);
			} catch { }
			foreach (string folder in this.ManagedContentFolders) {
				try {
					Directory.CreateDirectory(folder);
				} catch { }
			}
			try {
				Directory.CreateDirectory(this.InitialRouteFolder);
			} catch { }
			try {
				Directory.CreateDirectory(this.InitialTrainFolder);
			} catch { }
		}
		
		/// <summary>Gets the data folder or any specified subfolder thereof.</summary>
		/// <param name="subfolders">The subfolders.</param>
		/// <returns>The data folder or a subfolder thereof.</returns>
		internal string GetDataFolder(params string[] subfolders) {
			string folder = this.DataFolder;
			foreach (string subfolder in subfolders) {
				folder = OpenBveApi.Path.CombineDirectory(folder, subfolder);
			}
			return folder;
		}
		
		
		// --- private functions ---

		/// <summary>Creates the file system information from the specified configuration file.</summary>
		/// <param name="file">The configuration file describing the file system.</param>
		/// <returns>The file system.</returns>
		private static FileSystem FromConfigurationFile(string file) {
			string assemblyFile = Assembly.GetExecutingAssembly().Location;
			string assemblyFolder = Path.GetDirectoryName(assemblyFile);
			FileSystem system = new FileSystem();
			try
			{
				string[] lines = File.ReadAllLines(file, Encoding.UTF8);
				foreach (string line in lines)
				{
					int equals = line.IndexOf('=');
					if (equals >= 0)
					{
						string key = line.Substring(0, equals).Trim().ToLowerInvariant();
						string value = line.Substring(equals + 1).Trim();
						switch (key)
						{
							case "data":

								system.DataFolder = GetAbsolutePath(value, true);
								if (!Directory.Exists(system.DataFolder))
								{
									system.DataFolder = OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data");
									if (!Directory.Exists(system.DataFolder))
									{
										//If we are unable to find the data folder, this is a critical error, as it contains all sorts of essential stuff.....
										MessageBox.Show(@"Critical error:" + Environment.NewLine + @"Unable to find the openBVE data folder.....", @"openBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
										Environment.Exit(0);
									}
								}
								break;
							case "managedcontent":
								system.ManagedContentFolders = value.Split(',');
								for (int i = 0; i < system.ManagedContentFolders.Length; i++)
								{
									system.ManagedContentFolders[i] = GetAbsolutePath(system.ManagedContentFolders[i].Trim(), true);
								}
								break;
							case "version":
								int v;
								if (!int.TryParse(value, out v))
								{
									Program.AppendToLogFile("WARNING: Invalid filesystem.cfg version detected.");
								}
								if (v <= 1)
								{
									//Silently upgrade to the current config version
									system.Version = 1;
									break;
								}
								if (v > 1)
								{
									Program.AppendToLogFile("WARNING: A newer filesystem.cfg version " + v + " was detected. The current version is 1.");
									system.Version = v;
								}
								break;
							case "settings":
								system.SettingsFolder = GetAbsolutePath(value, true);
								break;
							case "initialroute":
								system.InitialRouteFolder = GetAbsolutePath(value, true);
								break;
							case "initialtrain":
								system.InitialTrainFolder = GetAbsolutePath(value, true);
								break;
							case "restartprocess":
								system.RestartProcess = GetAbsolutePath(value, true);
								break;
							case "restartarguments":
								system.RestartArguments = GetAbsolutePath(value, false);
								break;
							case "routepackageinstall":
								system.RouteInstallationDirectory = GetAbsolutePath(value, true);
								break;
							case "trainpackageinstall":
								system.TrainInstallationDirectory = GetAbsolutePath(value, true);
								break;
							case "otherpackageinstall":
								system.OtherInstallationDirectory = GetAbsolutePath(value, true);
								break;
							case "loksimpackageinstall":
								system.LoksimPackageInstallationDirectory = GetAbsolutePath(value, true);
								break;
							default:
								if (system.NotUnderstoodLines == null)
								{
									system.NotUnderstoodLines = new string[0];
								}
								int l = system.NotUnderstoodLines.Length;
								Array.Resize(ref system.NotUnderstoodLines, system.NotUnderstoodLines.Length + 1);
								system.NotUnderstoodLines[l] = line;
								Program.AppendToLogFile("WARNING: Unrecognised key " + key + " detected in filesystem.cfg");
								break;
						}
					}
				}
			}
			catch { }
			return system;
		}

		/// <summary>Gets the absolute path from the specified folder.</summary>
		/// <param name="folder">The folder which may contain special representations of system folders.</param>
		/// <param name="checkIfRooted">Checks if the resulting path is an absolute path.</param>
		/// <returns>The absolute path.</returns>
		private static string GetAbsolutePath(string folder, bool checkIfRooted) {
			string originalFolder = folder;
			if (checkIfRooted) {
				folder = folder.Replace('/', Path.DirectorySeparatorChar);
				folder = folder.Replace('\\', Path.DirectorySeparatorChar);
			}
			folder = folder.Replace("$[AssemblyFile]", Assembly.GetExecutingAssembly().Location);
			folder = folder.Replace("$[AssemblyFolder]", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			folder = folder.Replace("$[ApplicationData]", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			folder = folder.Replace("$[CommonApplicationData]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
			folder = folder.Replace("$[Personal]", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			if (checkIfRooted && !Path.IsPathRooted(folder)) {
				throw new InvalidDataException("The folder " + originalFolder + " does not produce an absolute path.");
			}
			return folder;
		}
		
	}
}