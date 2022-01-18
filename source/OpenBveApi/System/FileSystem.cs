using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OpenBveApi.Hosts;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
/*
 * According to MSDN these could only be caused by calling GetExecutingAssembly from unmanaged code
 * We never do this, and it's not worth the mess of null-checking in what is essentially an internal library
 */

namespace OpenBveApi.FileSystem {
	/// <summary>Represents the program's organization of files and folders.</summary>
	public class FileSystem {
		
		
		// --- members ---
		
		/// <summary>The location of the application data, including, among others, Compatibility, Flags and Languages.</summary>
		public string DataFolder;
		
		/// <summary>The locations of managed content.</summary>
		internal string[] ManagedContentFolders;

		/// <summary>Gets the package database folder</summary>
		public string PackageDatabaseFolder => OpenBveApi.Path.CombineDirectory(SettingsFolder, "PackageDatabase");

		/// <summary>The location where to save user settings, including settings.cfg and controls.cfg.</summary>
		public string SettingsFolder;
		
		/// <summary>The initial location of the Railway/Route folder.</summary>
		public string InitialRouteFolder;

		/// <summary>The initial location of the Train folder.</summary>
		public string InitialTrainFolder;
		
		/// <summary>The location of the process to execute on restarting the program.</summary>
		public string RestartProcess;
		
		/// <summary>The arguments to supply to the process on restarting the program.</summary>
		public string RestartArguments;

		/// <summary>The location to which packaged routes will be installed</summary>
		public string RouteInstallationDirectory;

		/// <summary>The location to which packaged trains will be installed</summary>
		public string TrainInstallationDirectory;

		/// <summary>The location to which packaged other items will be installed</summary>
		public string OtherInstallationDirectory;

		/// <summary>The location to which user plugins will be installed</summary>
		public string PluginInstallationDirectory;
		
		/// <summary>The location to which Loksim3D packages will be installed</summary>
		public string LoksimPackageInstallationDirectory;

		/// <summary>Any lines loaded from the filesystem.cfg which were not understood</summary>
		internal string[] NotUnderstoodLines;

		/// <summary>The version of the filesystem</summary>
		internal int Version;

		/// <summary>The host application</summary>
		private readonly HostInterface currentHost;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this class with default locations.</summary>
		internal FileSystem(HostInterface Host)
		{
			currentHost = Host;
			string assemblyFile = Assembly.GetEntryAssembly().Location;
			string assemblyFolder = System.IO.Path.GetDirectoryName(assemblyFile);
			//This copy of openBVE is a special string, and should not be localised
			string userDataFolder = OpenBveApi.Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenBve");
			if (currentHost != null && currentHost.Platform != HostPlatform.MicrosoftWindows)
			{
				/*
				 * Case sensitive platforms, where a file and folder cannot have the same name
				 * https://github.com/leezer3/OpenBVE/issues/571
				 */
				if (File.Exists(userDataFolder))
				{
					int i = 0;
					while (File.Exists(userDataFolder))
					{
						userDataFolder = OpenBveApi.Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenBve-" + i);
						i++;
						if (i == 10)
						{
							//Critical, otherwise we'll be in an infinite loop
							throw new Exception("Failed to find a valid path for UserData folder.");
						}
					}
				}
			}
			this.DataFolder = OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data");
			this.ManagedContentFolders = new string[] { OpenBveApi.Path.CombineDirectory(userDataFolder, "ManagedContent") };
			this.SettingsFolder = OpenBveApi.Path.CombineDirectory(userDataFolder, "Settings");
			this.PluginInstallationDirectory = OpenBveApi.Path.CombineDirectory(userDataFolder, "Plugins");
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
		/// <param name="Host">The host program</param>
		/// <returns>The file system information.</returns>
		public static FileSystem FromCommandLineArgs(string[] args, HostInterface Host) {
			foreach (string arg in args) {
				if (arg.StartsWith("/filesystem=", StringComparison.OrdinalIgnoreCase)) {
					return FromConfigurationFile(arg.Substring(12), Host);
				}
			}
			string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "filesystem.cfg");
			if (File.Exists(configFile)) {
				return FromConfigurationFile(configFile, Host);
			}
			configFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(OpenBveApi.Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenBve"), "Settings"), "filesystem.cfg");
			if (File.Exists(configFile))
			{
				return FromConfigurationFile(configFile, Host);
			}	
			return new FileSystem(Host);
			
		}

		/// <summary>Saves the current file system configuration to disk</summary>
		public void SaveCurrentFileSystemConfiguration()
		{
			string file = OpenBveApi.Path.CombineFile(this.SettingsFolder, "FileSystem.cfg");
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
							string key = line.Substring(0, equals).Trim(new char[] { }).ToLowerInvariant();
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
					newLines.AppendLine("Data=" + this.DataFolder);
					newLines.AppendLine("Settings=" + this.SettingsFolder);
					newLines.AppendLine("InitialRoute=" + this.InitialRouteFolder);
					newLines.AppendLine("InitialTrain=" + this.InitialTrainFolder);
					newLines.AppendLine("RestartProcess=" + this.RestartProcess);
					newLines.AppendLine("RestartArguments=" + this.RestartArguments);
				}
				if (this.RouteInstallationDirectory != null &&
					Directory.Exists(this.RouteInstallationDirectory))
				{
					newLines.AppendLine("RoutePackageInstall=" + ReplacePath(this.RouteInstallationDirectory));
				}

				if (this.TrainInstallationDirectory != null &&
					Directory.Exists(this.TrainInstallationDirectory))
				{
					newLines.AppendLine("TrainPackageInstall=" + ReplacePath(this.TrainInstallationDirectory));
				}
				if (this.OtherInstallationDirectory != null &&
					Directory.Exists(this.OtherInstallationDirectory))
				{
					newLines.AppendLine("OtherPackageInstall=" + ReplacePath(this.OtherInstallationDirectory));
				}
				if (this.LoksimPackageInstallationDirectory != null &&
					Directory.Exists(this.LoksimPackageInstallationDirectory))
				{
					newLines.AppendLine("LoksimPackageInstall=" + ReplacePath(this.LoksimPackageInstallationDirectory));
				}
				if (this.NotUnderstoodLines != null && this.NotUnderstoodLines.Length != 0)
				{
					for (int i = 0; i < this.NotUnderstoodLines.Length; i++)
					{
						newLines.Append(this.NotUnderstoodLines[i]);
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
				line = line.Replace(Assembly.GetEntryAssembly().Location, "$[AssemblyFile]");
				line = line.Replace(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "$[AssemblyFolder]");
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
		public void CreateFileSystem() {
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
		public string GetDataFolder(params string[] subfolders) {
			string folder = this.DataFolder;
			foreach (string subfolder in subfolders) {
				folder = OpenBveApi.Path.CombineDirectory(folder, subfolder);
			}
			return folder;
		}
		
		
		// --- private functions ---

		/// <summary>Creates the file system information from the specified configuration file.</summary>
		/// <param name="file">The configuration file describing the file system.</param>
		/// <param name="Host">The host program</param>
		/// <returns>The file system.</returns>
		private static FileSystem FromConfigurationFile(string file, HostInterface Host) {
			string assemblyFile = Assembly.GetEntryAssembly().Location;
			string assemblyFolder = System.IO.Path.GetDirectoryName(assemblyFile);
			FileSystem system = new FileSystem(Host);
			try
			{
				string[] lines = File.ReadAllLines(file, Encoding.UTF8);
				foreach (string line in lines)
				{
					int equals = line.IndexOf('=');
					if (equals >= 0)
					{
						string key = line.Substring(0, equals).Trim(new char[] { }).ToLowerInvariant();
						string value = line.Substring(equals + 1).Trim(new char[] { });
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
								system.ManagedContentFolders = value.Split(new[] { ',' });
								for (int i = 0; i < system.ManagedContentFolders.Length; i++)
								{
									system.ManagedContentFolders[i] = GetAbsolutePath(system.ManagedContentFolders[i].Trim(new char[] { }), true);
								}
								break;
							case "version":
								int v;
								if (!int.TryParse(value, out v))
								{
									system.AppendToLogFile("WARNING: Invalid filesystem.cfg version detected.");
								}
								if (v <= 1)
								{
									//Silently upgrade to the current config version
									system.Version = 1;
									break;
								}
								if (v > 1)
								{
									system.AppendToLogFile("WARNING: A newer filesystem.cfg version " + v + " was detected. The current version is 1.");
									system.Version = v;
								}
								break;
							case "settings":
								string folder = GetAbsolutePath(value, true);
								if (value.StartsWith(@"$[AssemblyFolder]", StringComparison.InvariantCulture))
								{
									try
									{
										/*
										* Check we have read / write access to the settings file
										* https://bveworldwide.forumotion.com/t1998-access-denied-after-upgrade#20169
										*/
										if (!(Type.GetType("Mono.Runtime") != null))
										{
											//Mono doesn't reliably support AccessControl
											Directory.GetAccessControl(folder);
										}

										string settingsFile = Path.CombineFile(folder, "1.5.0\\options.cfg");
										using (FileStream fs = File.OpenWrite(settingsFile))
										{
											//Write test
										}
									}
									catch
									{
										value = value.Replace("$[AssemblyFolder]", "$[ApplicationData]");
										folder = GetAbsolutePath(value, true);
									}
									
								}
								system.SettingsFolder = folder;
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
								system.AppendToLogFile("WARNING: Unrecognised key " + key + " detected in filesystem.cfg");
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
				folder = folder.Replace('/', System.IO.Path.DirectorySeparatorChar);
				folder = folder.Replace('\\', System.IO.Path.DirectorySeparatorChar);
			}
			folder = folder.Replace("$[AssemblyFile]", Assembly.GetEntryAssembly().Location);
			folder = folder.Replace("$[AssemblyFolder]", System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			folder = folder.Replace("$[ApplicationData]", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			folder = folder.Replace("$[CommonApplicationData]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
			folder = folder.Replace("$[Personal]", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			if (checkIfRooted && !System.IO.Path.IsPathRooted(folder)) {
				throw new InvalidDataException("The folder " + originalFolder + " does not produce an absolute path.");
			}
			return folder;
		}
		
		/// <summary>Clears the log file.</summary>
		public void ClearLogFile(string version) {
			try {
				string file = System.IO.Path.Combine(this.SettingsFolder, "log.txt");
				System.IO.File.WriteAllText(file, @"openBVE Log: " + DateTime.Now + Environment.NewLine + @"Program Version: " + version + Environment.NewLine + Environment.NewLine, new System.Text.UTF8Encoding(true));
			} catch { }
		}

		/// <summary>Appends the specified text to the log file.</summary>
		/// <param name="text">The text.</param>
		public void AppendToLogFile(string text) {
			try {
				string file = System.IO.Path.Combine(this.SettingsFolder, "log.txt");
				System.IO.File.AppendAllText(file, DateTime.Now.ToString("HH:mm:ss") + @"  " + text + Environment.NewLine, new System.Text.UTF8Encoding(false));
			} catch { }
		}
	}
}
