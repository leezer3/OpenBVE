using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace OpenBve {
	internal static partial class ManagedContent {
		internal partial class Database {
			
			/// <summary>Gets all suggestions for the specified list of packages.</summary>
			/// <param name="packages">The list of packages.</param>
			/// <returns>The list of suggestions</returns>
			internal List<ManagedContent.Dependency> GetSuggestions(List<Version> packages) {
				List<Dependency> results = new List<Dependency>();
				foreach (Version package in packages) {
					foreach (Dependency suggestion in package.Suggestions) {
						if (!packages.Exists((Version item) => { return item.Name.Equals(suggestion.Name, StringComparison.OrdinalIgnoreCase); })) {
							Dependency find = results.Find((Dependency item) => { return item.Name.Equals(suggestion.Name, StringComparison.OrdinalIgnoreCase); });
							if (find.Name != null) {
								if (CompareVersions(suggestion.Version, find.Version) > 0) {
									find.Version = suggestion.Version;
								}
							} else {
								string installedVersion = GetInstalledPackageVersion(suggestion.Name);
								if (installedVersion == null || CompareVersions(suggestion.Version, installedVersion) > 0) {
									results.Add(suggestion);
								}
							}
						}
					}
				}
				return results;
			}
			
			/// <summary>Takes a list of packages (which are to be installed) and adds their dependencies to the list.</summary>
			/// <param name="packages">The list of packages.</param>
			/// <returns>Returns false if not all dependencies were found.</returns>
			internal bool AddDependencies(List<Version> packages) {
				/*
				 * Add dependencies.
				 * */
				for (int i = 0; i < packages.Count; i++) {
					foreach (Dependency dependency in packages[i].Dependencies) {
						Version find = packages.Find((Version item) => { return item.Name.Equals(dependency.Name, StringComparison.OrdinalIgnoreCase); });
						if (find != null) {
							if (CompareVersions(dependency.Version, find.Number) > 0) {
								Version version = Dereference(dependency.Name, dependency.Version);
								if (version != null) {
									packages[i] = version;
									i--;
								} else {
									return false;
								}
							}
						} else {
							Version version = Dereference(dependency.Name, dependency.Version);
							if (version != null) {
								packages.Add(version);
							} else {
								return false;
							}
						}
					}
				}
				/*
				 * If the packages to be installed are already installed,
				 * remove them from the list.
				 * */
				for (int i = 0; i < packages.Count; i++) {
					string version = GetInstalledPackageVersion(packages[i].Name);
					if (version != null) {
						if (CompareVersions(packages[i].Number, version) <= 0) {
							packages[i] = packages[packages.Count - 1];
							packages.RemoveAt(packages.Count - 1);
							i--;
						}
					}
				}
				return true;
			}
			
			/// <summary>Installs the specified package, but not its dependencies.</summary>
			/// <param name="version">The specific version of the package.</param>
			/// <param name="size">Accumulates the size of the downloaded data in an interlocked operation. If the operation fails, all accumulated size is subtracted again.</param>
			/// <returns>Whether the package was successfully installed.</returns>
			internal bool InstallPackage(Version version, ref int size) {
				try {
					/*
					 * Check whether the package is protected.
					 * */
					if (IsInstalledPackageProtected(version.Name)) {
						return false;
					}
					/*
					 * Remove existing versions of this package.
					 */
					string packageDirectory = GetInstalledPackageDirectory(version.Name);
					if (packageDirectory != null) {
						RemoveDirectory(packageDirectory);
					} else {
						packageDirectory = Path.Combine(Program.FileSystem.ManagedContentFolders[0], version.Name);
					}
					/*
					 * Install the package.
					 * */
					Source[] sources = version.Sources;
					Shuffle(sources);
					int attemps = sources.Length == 1 ? 3 : sources.Length == 2 ? 2 : 1;
					for (int a = 0; a < attemps; a++) {
						for (int i = 0; i < sources.Length; i++) {
							byte[] bytes;
							if (Internet.TryDownloadBytesFromUrl(sources[i].Url, out bytes, ref size)) {
								if (bytes.Length == sources[i].Size) {
									byte[] md5 = (new MD5CryptoServiceProvider()).ComputeHash(bytes);
									bool add = true;
									for (int k = 0; k < 16; k++) {
										if (md5[k] != sources[i].Md5[k]) {
											add = false;
											break;
										}
									}
									if (add) {
										if (bytes.Length >= 2 && bytes[0] == 0x1F && bytes[1] == 0x8B) {
											/* GZIP-compressed */
											bytes = Gzip.Decompress(bytes);
										}
										if ((bytes.Length & 0x1FF) == 0) {
											/* TAR archive */
											RemoveDirectory(packageDirectory);
											CreateDirectory(packageDirectory);
											Tar.Unpack(bytes, packageDirectory, version.Name);
										} else {
											add = false;
										}
										if (add) {
											/* 
											 * The installation was successful. Save some of
											 * the package information for offline storage.
											 * */
											StringBuilder builder = new StringBuilder();
											builder.AppendLine("name = " + version.Name);
											builder.AppendLine("version = " + version.Number);
											if (version.Sources.Length != 0) {
												builder.Append("sources = ");
												for (int j = 0; j < version.Sources.Length; j++) {
													if (j != 0) {
														builder.Append(',').Append(' ');
													}
													builder.Append(version.Sources[j].Url);
												}
												builder.AppendLine();
											}
											if (version.Dependencies.Length != 0) {
												builder.Append("dependencies = ");
												for (int j = 0; j < version.Dependencies.Length; j++) {
													if (j != 0) {
														builder.Append(',').Append(' ');
													}
													builder.Append(version.Dependencies[j].Name);
													builder.Append(' ').Append('(');
													builder.Append(version.Dependencies[j].Version);
													builder.Append(')');
												}
												builder.AppendLine();
											}
											if (version.Suggestions.Length != 0) {
												builder.Append("suggestions = ");
												for (int j = 0; j < version.Suggestions.Length; j++) {
													if (j != 0) {
														builder.Append(',').Append(' ');
													}
													builder.Append(version.Suggestions[j].Name);
													builder.Append(' ').Append('(');
													builder.Append(version.Suggestions[j].Version);
													builder.Append(')');
												}
												builder.AppendLine();
											}
											builder.AppendLine();
											foreach (KeyValuePair pair in version.Metadata) {
												if (
													!string.Equals(pair.Key, "protected", StringComparison.OrdinalIgnoreCase) &&
													!string.Equals(pair.Key, "wip", StringComparison.OrdinalIgnoreCase)
												) {
													builder.AppendLine(pair.ToString());
												}
											}
											string file = Path.Combine(packageDirectory, "package.cfg");
											File.WriteAllText(file, builder.ToString(), new UTF8Encoding(true));
											return true;
										}
									}
								}
								Interlocked.Add(ref size, -bytes.Length);
							}
						}
						Thread.Sleep(1000);
					}
					return false;
				} catch {
					return false;
				}
			}
			
			internal static void Shuffle<T>(T[] array) {
				Random random = Program.RandomNumberGenerator;
				for (int i = 0; i < array.Length - 1; i++) {
					int index = random.Next(i, array.Length);
					T temp = array[index];
					array[index] = array[i];
					array[i] = temp;
				}
			}
			
		}
	}
}