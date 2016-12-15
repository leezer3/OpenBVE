using System;
using System.Collections.Generic;

namespace OpenBve {
	internal static partial class ManagedContent {
		
		/// <summary>Gets all dependent and redundant items for the specified list of packages.</summary>
		/// <param name="packages">The list of packages.</param>
		/// <returns>The list of dependent and redundant items.</returns>
		/// <remarks>Dependent items are packages that depend on any of the packages to be removed.</remarks>
		/// <remarks>Redundant items are libraries or shared libraries that are no longer used by any other package.</remarks>
		internal static List<string> GetDependentAndRedundantPackages(List<string> packages) {
			List<string> results = new List<string>();
			while (true) {
				bool again = false;
				/*
				 * Add all dependent items.
				 * Build a list of dependencies.
				 * */
				List<string> dependencies = new List<string>();
				foreach (string lookupDirectory in Program.FileSystem.ManagedContentFolders) {
					if (System.IO.Directory.Exists(lookupDirectory)) {
						string[] packageDirectories = System.IO.Directory.GetDirectories(lookupDirectory);
						foreach (string packageDirectory in packageDirectories) {
							string package = System.IO.Path.GetFileName(packageDirectory);
							if (
								!packages.Exists((string item) => { return item != null && item.Equals(package, StringComparison.OrdinalIgnoreCase); }) &&
								!results.Exists((string item) => { return item != null && item.Equals(package, StringComparison.OrdinalIgnoreCase); })
							) {
								string file = OpenBveApi.Path.CombineFile(packageDirectory, "package.cfg");
								if (System.IO.File.Exists(file)) {
									string[] lines = System.IO.File.ReadAllLines(file, System.Text.Encoding.UTF8);
									foreach (string line in lines) {
										int equals = line.IndexOf('=');
										if (equals >= 0) {
											string key = line.Substring(0, equals).Trim();
											if (key.Equals("dependencies", StringComparison.OrdinalIgnoreCase)) {
												string[] values = line.Substring(equals + 1).Split(',');
												bool add = false;
												for (int j = 0; j < values.Length; j++) {
													values[j] = values[j].Trim();
													if (values[j].Length != 0) {
														int bracket = Math.Max(values[j].IndexOf('('), values[j].IndexOf('['));
														if (bracket >= 0 && (values[j][values[j].Length - 1] == ')' | values[j][values[j].Length - 1] == ']')) {
															values[j] = values[j].Substring(0, bracket).TrimEnd();
														}
														if (
															packages.Exists((string item) => { return item != null && item.Equals(values[j], StringComparison.OrdinalIgnoreCase); }) ||
															results.Exists((string item) => { return item != null && item.Equals(values[j], StringComparison.OrdinalIgnoreCase); })
														) {
															add = true;
														}
													}
												}
												if (add) {
													results.Add(package);
													again = true;
												} else {
													dependencies.AddRange(values);
												}
											}
										}
									}
								}
							}
						}
					}
				}
				if (again) continue;
				/*
				 * Add all redundant items.
				 * */
				foreach (string lookupDirectory in Program.FileSystem.ManagedContentFolders) {
					if (System.IO.Directory.Exists(lookupDirectory)) {
						string[] packageDirectories = System.IO.Directory.GetDirectories(lookupDirectory);
						foreach (string packageDirectory in packageDirectories) {
							string package = System.IO.Path.GetFileName(packageDirectory);
							if (!IsInstalledPackageProtected(package)) {
								if (
									!packages.Exists((string item) => { return item != null && item.Equals(package, StringComparison.OrdinalIgnoreCase); }) &&
									!results.Exists((string item) => { return item != null && item.Equals(package, StringComparison.OrdinalIgnoreCase); })
								) {
									string file = OpenBveApi.Path.CombineFile(packageDirectory, "package.cfg");
									if (System.IO.File.Exists(file)) {
										bool add = false;
										string[] lines = System.IO.File.ReadAllLines(file, System.Text.Encoding.UTF8);
										foreach (string line in lines) {
											int equals = line.IndexOf('=');
											if (equals >= 0) {
												string key = line.Substring(0, equals).Trim();
												if (key.Equals("type", StringComparison.OrdinalIgnoreCase)) {
													string value = line.Substring(equals + 1).Trim();
													if (value.Equals("library", StringComparison.OrdinalIgnoreCase) | value.Equals("shared library", StringComparison.OrdinalIgnoreCase)) {
														add = !dependencies.Exists((string item) => { return item != null && item.Equals(package, StringComparison.OrdinalIgnoreCase); });
														break;
													}
												}
											}
										}
										if (add) {
											results.Add(package);
											again = true;
										}
									}
								}
							}
						}
					}
				}
				if (!again) break;
			}
			return results;
		}
		
		/// <summary>Removes the specified package, but not dependent packages.</summary>
		/// <param name="package">The package.</param>
		/// <returns>Whether removing the package was successful.</returns>
		internal static bool RemovePackage(string package) {
			try {
				/*
				 * Check whether the package is protected.
				 * */
				if (IsInstalledPackageProtected(package)) {
					return false;
				}
				/*
				 * Remove the package.
				 * */
				while (true) {
					string directory = GetInstalledPackageDirectory(package);
					if (directory != null) {
						RemoveDirectory(directory);
					} else {
						break;
					}
				}
				return true;
			} catch {
				return false;
			}
		}

	}
}