using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
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
		
		/// <summary>Gets the package database folder</summary>
		public string PackageDatabaseFolder => Path.CombineDirectory(SettingsFolder, "PackageDatabase");

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
		
		/// <summary>The location to which Loksim3D packages will be installed</summary>
		public string LoksimPackageInstallationDirectory;

		/// <summary>The Loksim3D data directory</summary>
		public string LoksimDataDirectory;

		/// <summary>The MSTS trainset directory</summary>
		public string MSTSDirectory;

		/// <summary>Any lines loaded from the filesystem.cfg which were not understood</summary>
		internal string[] NotUnderstoodLines;

		/// <summary>The version of the filesystem</summary>
		internal int Version;

		/// <summary>The host application</summary>
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly HostInterface currentHost;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this class with default locations.</summary>
		internal FileSystem(HostInterface Host)
		{
			currentHost = Host;
			string assemblyFile = Assembly.GetEntryAssembly().Location;
			string assemblyFolder = Path.GetDirectoryName(assemblyFile);
			//This copy of openBVE is a special string, and should not be localised
			string userDataFolder = Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenBve");
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
						userDataFolder = Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenBve-" + i);
						i++;
						if (i == 10)
						{
							//Critical, otherwise we'll be in an infinite loop
							throw new Exception("Failed to find a valid path for UserData folder.");
						}
					}
				}
			}
			DataFolder = Path.CombineDirectory(assemblyFolder, "Data");
			SettingsFolder = Path.CombineDirectory(userDataFolder, "Settings");
			InitialRouteFolder = Path.CombineDirectory(Path.CombineDirectory(Path.CombineDirectory(userDataFolder, "LegacyContent"), "Railway"), "Route");
			RouteInstallationDirectory = Path.CombineDirectory(Path.CombineDirectory(userDataFolder, "LegacyContent"), "Railway");
			InitialTrainFolder = Path.CombineDirectory(Path.CombineDirectory(userDataFolder, "LegacyContent"), "Train");
			TrainInstallationDirectory = Path.CombineDirectory(Path.CombineDirectory(userDataFolder, "LegacyContent"), "Train");
			OtherInstallationDirectory = Path.CombineDirectory(Path.CombineDirectory(userDataFolder, "LegacyContent"), "Other");
			LoksimPackageInstallationDirectory = Path.CombineDirectory(Path.CombineDirectory(userDataFolder, "ManagedContent"), "Loksim3D");
			if (currentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				try
				{
					RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Loksim-Group\\Install");
					LoksimDataDirectory = key != null ? key.GetValue("InstallDataDirPath").ToString() : LoksimPackageInstallationDirectory;
				}
				catch
				{
					LoksimDataDirectory = LoksimPackageInstallationDirectory; // minor fudge
				}
			}
			else
			{
				LoksimDataDirectory = LoksimPackageInstallationDirectory; // FIXME: Should this be saved on non Win-32 platforms??
			}
			RestartProcess = assemblyFile;
			RestartArguments = string.Empty;
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
			string assemblyFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string configFile = Path.CombineFile(Path.CombineDirectory(Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "filesystem.cfg");
			if (File.Exists(configFile)) {
				return FromConfigurationFile(configFile, Host);
			}
			configFile = Path.CombineFile(Path.CombineDirectory(Path.CombineDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenBve"), "Settings"), "filesystem.cfg");
			if (File.Exists(configFile))
			{
				return FromConfigurationFile(configFile, Host);
			}	
			return new FileSystem(Host);
			
		}

		/// <summary>Saves the current file system configuration to disk</summary>
		public void SaveCurrentFileSystemConfiguration()
		{
			string file = Path.CombineFile(SettingsFolder, "FileSystem.cfg");
			StringBuilder newLines = new StringBuilder();
			newLines.AppendLine("Version=2");
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
					newLines.AppendLine("Data=" + DataFolder);
					newLines.AppendLine("Settings=" + SettingsFolder);
					newLines.AppendLine("InitialRoute=" + InitialRouteFolder);
					newLines.AppendLine("InitialTrain=" + InitialTrainFolder);
					newLines.AppendLine("RestartProcess=" + RestartProcess);
					newLines.AppendLine("RestartArguments=" + RestartArguments);
				}
				if (RouteInstallationDirectory != null &&
					Directory.Exists(RouteInstallationDirectory))
				{
					newLines.AppendLine("RoutePackageInstall=" + ReplacePath(RouteInstallationDirectory));
				}

				if (TrainInstallationDirectory != null &&
					Directory.Exists(TrainInstallationDirectory))
				{
					newLines.AppendLine("TrainPackageInstall=" + ReplacePath(TrainInstallationDirectory));
				}
				if (OtherInstallationDirectory != null &&
					Directory.Exists(OtherInstallationDirectory))
				{
					newLines.AppendLine("OtherPackageInstall=" + ReplacePath(OtherInstallationDirectory));
				}
				if (LoksimPackageInstallationDirectory != null &&
					Directory.Exists(LoksimPackageInstallationDirectory))
				{
					newLines.AppendLine("LoksimPackageInstall=" + ReplacePath(LoksimPackageInstallationDirectory));
				}
				if (MSTSDirectory != null &&
				    Directory.Exists(OtherInstallationDirectory))
				{
					newLines.AppendLine("MSTSTrainset=" + ReplacePath(MSTSDirectory));
				}
				if (NotUnderstoodLines != null && NotUnderstoodLines.Length != 0)
				{
					for (int i = 0; i < NotUnderstoodLines.Length; i++)
					{
						newLines.Append(NotUnderstoodLines[i]);
					}
				}
				File.WriteAllText(file, newLines.ToString(), new UTF8Encoding(true));
			}
			catch
			{
				// ignored
			}
		}

		/// <summary> Replaces all instances of absolute paths within a string with their relative equivilants</summary>
		internal static string ReplacePath(string line)
		{
			try
			{
				line = line.Replace(Assembly.GetEntryAssembly().Location, "$[AssemblyFile]");
				line = line.Replace(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "$[AssemblyFolder]");
				line = line.Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "$[ApplicationData]");
				line = line.Replace(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					"$[CommonApplicationData]");
				line = line.Replace(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "$[Personal]");
			}
			catch
			{
				// ignored
			}
			return line;
		}

		/// <summary>Creates all folders in the file system that can later be written to.</summary>
		public void CreateFileSystem() {
			try
			{
				if (!string.IsNullOrEmpty(SettingsFolder) && !Directory.Exists(SettingsFolder))
				{
					Directory.CreateDirectory(SettingsFolder);
				}
			}
			catch(Exception ex)
			{
				AppendToLogFile("Failed to create the Settings Folder with exception " + ex);
			}

			try
			{
				if (!string.IsNullOrEmpty(InitialRouteFolder) && !Directory.Exists(InitialRouteFolder))
				{
					Directory.CreateDirectory(InitialRouteFolder);
				}
				
			}
			catch(Exception ex)
			{
				AppendToLogFile("Failed to create the Initial Route Folder with exception " + ex);
			}

			try
			{
				if (!string.IsNullOrEmpty(InitialTrainFolder) && !Directory.Exists(InitialTrainFolder))
				{
					Directory.CreateDirectory(InitialTrainFolder);
				}
			}
			catch(Exception ex)
			{
				AppendToLogFile("Failed to create the Initial Train Folder with exception " + ex);
			}
		}
		
		/// <summary>Gets the data folder or any specified subfolder thereof.</summary>
		/// <param name="subfolders">The subfolders.</param>
		/// <returns>The data folder or a subfolder thereof.</returns>
		public string GetDataFolder(params string[] subfolders) {
			string folder = DataFolder;
			foreach (string subfolder in subfolders) {
				folder = Path.CombineDirectory(folder, subfolder);
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
			string assemblyFolder = Path.GetDirectoryName(assemblyFile);
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
									system.DataFolder = Path.CombineDirectory(assemblyFolder, "Data");
									if (!Directory.Exists(system.DataFolder))
									{
										//If we are still unable to find the default data folder, this is a critical error, as it contains all sorts of essential stuff.....
										MessageBox.Show(@"Unable to find the OpenBVE Data folder." + Environment.NewLine +
										                @"OpenBVE will now exit as the Data folder is required for normal operation." + Environment.NewLine + Environment.NewLine +
										                @"Please consider reinstalling OpenBVE.", @"Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
										Environment.Exit(0);
									}
								}

								break;
							case "version":
								if (!int.TryParse(value, out int v))
								{
									system.AppendToLogFile("WARNING: Invalid filesystem.cfg version detected.");
								}

								if (v <= 2)
								{
									//Silently upgrade to the current config version
									system.Version = 2;
									break;
								}

								system.AppendToLogFile("WARNING: A newer filesystem.cfg version " + v + " was detected. The current version is 2.");
								system.Version = v;

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
										using (FileStream unused = File.OpenWrite(settingsFile))
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
							case "mststrainset":
								system.MSTSDirectory = GetAbsolutePath(value, true);
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
			catch
			{
				// ignored
			}
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
			folder = folder.Replace("$[AssemblyFolder]", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			folder = folder.Replace("$[ApplicationData]", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			folder = folder.Replace("$[CommonApplicationData]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
			folder = folder.Replace("$[Personal]", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			if (checkIfRooted && !Path.IsPathRooted(folder)) {
				throw new InvalidDataException("The folder " + originalFolder + " does not produce an absolute path.");
			}
			return folder;
		}
		
		/// <summary>Clears the log file.</summary>
		public void ClearLogFile(string version) {
			try
			{
				string file = System.IO.Path.Combine(SettingsFolder, "log.txt");
				File.WriteAllText(file, @"OpenBVE Log: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + @"Program Version: " + version + Environment.NewLine + Environment.NewLine, new UTF8Encoding(true));
			}
			catch
			{
				//  ignored
			}
		}

		/// <summary>Appends the specified text to the log file.</summary>
		/// <param name="text">The text.</param>
		/// <param name="addTimestamp">Whether a timestamp should be added to the log file</param>
		public void AppendToLogFile(string text, bool addTimestamp = true) {
			try
			{
				string file = System.IO.Path.Combine(SettingsFolder, "log.txt");
				if (addTimestamp)
				{
					File.AppendAllText(file, DateTime.Now.ToString("HH:mm:ss") + @"  " + text + Environment.NewLine, new UTF8Encoding(false));
				}
				else
				{
					File.AppendAllText(file, text + Environment.NewLine, new UTF8Encoding(false));	
				}
				
			}
			catch
			{
				// ignored
			}
		}
	}
}
