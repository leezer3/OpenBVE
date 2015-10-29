using System;
using System.IO;
using System.Text;

namespace OpenBve {
	internal static partial class ManagedContent {
		
		// --- static functions ---
		
		/// <summary>Gets the directory of the specified package if installed.</summary>
		/// <param name="package">The package name.</param>
		/// <returns>The directory of the specified package, or a null reference if not installed.</returns>
		/// <remarks>If multiple instances of the package exists, this function returns the first one it encounters.</remarks>
		internal static string GetInstalledPackageDirectory(string package) {
			for (int i = 0; i < Program.FileSystem.ManagedContentFolders.Length; i++) {
				string directory = OpenBveApi.Path.CombineDirectory(Program.FileSystem.ManagedContentFolders[i], package);
				if (Directory.Exists(directory)) {
					return directory;
				}
			}
			return null;
		}
		
		/// <summary>Gets the currently installed version of the specified package.</summary>
		/// <param name="package">The package name.</param>
		/// <returns>The currently installed version of the specified package, or a null reference if the package is not installed.</returns>
		internal static string GetInstalledPackageVersion(string package) {
			foreach (string lookupDirectory in Program.FileSystem.ManagedContentFolders) {
				string packageDirectory = OpenBveApi.Path.CombineDirectory(lookupDirectory, package);
				if (Directory.Exists(packageDirectory)) {
					string file = OpenBveApi.Path.CombineFile(packageDirectory, "package.cfg");
					if (File.Exists(file)) {
						string[] lines = File.ReadAllLines(file, Encoding.UTF8);
						for (int j = 0; j < lines.Length; j++) {
							lines[j] = lines[j].Trim();
							int equals = lines[j].IndexOf('=');
							if (equals >= 0) {
								string key = lines[j].Substring(0, equals).TrimEnd();
								if (key.Equals("version", StringComparison.OrdinalIgnoreCase)) {
									string value = lines[j].Substring(equals + 1).TrimStart();
									return value;
								}
							}
						}
					}
					return "0.0";
				}
			}
			return null;
		}
		
		/// <summary>Checks whether the specified package is installed and protected from modification or deletion.</summary>
		/// <param name="package">The package name.</param>
		/// <returns>Whether the specified package is protected.</returns>
		internal static bool IsInstalledPackageProtected(string package) {
			foreach (string lookupDirectory in Program.FileSystem.ManagedContentFolders) {
				string packageDirectory = OpenBveApi.Path.CombineDirectory(lookupDirectory, package);
				if (Directory.Exists(packageDirectory)) {
					string file = OpenBveApi.Path.CombineFile(packageDirectory, "package.cfg");
					if (File.Exists(file)) {
						string[] lines = File.ReadAllLines(file, Encoding.UTF8);
						for (int j = 0; j < lines.Length; j++) {
							lines[j] = lines[j].Trim();
							int equals = lines[j].IndexOf('=');
							if (equals >= 0) {
								string key = lines[j].Substring(0, equals).TrimEnd();
								if (key.Equals("protected", StringComparison.OrdinalIgnoreCase)) {
									string value = lines[j].Substring(equals + 1).TrimStart();
									if (value.Equals("true", StringComparison.OrdinalIgnoreCase)) {
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}
		
		/// <summary>Removes the specified directory if it exists.</summary>
		/// <param name="path">The path to the directory.</param>
		private static void RemoveDirectory(string path) {
			if (Directory.Exists(path)) {
				Directory.Delete(path, true);
			}
		}
		
		/// <summary>Creates the specified directory if it does not already exist.</summary>
		/// <param name="path">The path to the directory.</param>
		private static void CreateDirectory(string path) {
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}
		
	}
}