using System;
using System.IO;
using System.Reflection;
using System.Text;

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
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this class with default locations.</summary>
		internal FileSystem() {
			string assemblyFile = Assembly.GetExecutingAssembly().Location;
			string assemblyFolder = Path.GetDirectoryName(assemblyFile);
			string userDataFolder = OpenBveApi.Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "openBVE");
			this.DataFolder = OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data");
			this.ManagedContentFolders = new string[] { OpenBveApi.Path.CombineDirectory(userDataFolder, "ManagedContent") };
			this.SettingsFolder = OpenBveApi.Path.CombineDirectory(userDataFolder, "Settings");
			this.InitialRouteFolder = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Railway"), "Route");
			this.InitialTrainFolder = OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(userDataFolder, "LegacyContent"), "Train");
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
			} else {
				return new FileSystem();
			}
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
			FileSystem system = new FileSystem();
			try {
				string[] lines = File.ReadAllLines(file, Encoding.UTF8);
				foreach (string line in lines) {
					int equals = line.IndexOf('=');
					if (equals >= 0) {
						string key = line.Substring(0, equals).Trim().ToLowerInvariant();
						string value = line.Substring(equals + 1).Trim();
						switch (key) {
							case "data":
								system.DataFolder = GetAbsolutePath(value, true);
								break;
							case "managedcontent":
								system.ManagedContentFolders = value.Split(',');
								for (int i = 0; i < system.ManagedContentFolders.Length; i++) {
									system.ManagedContentFolders[i] = GetAbsolutePath(system.ManagedContentFolders[i].Trim(), true);
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
						}
					}
				}
			} catch { }
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