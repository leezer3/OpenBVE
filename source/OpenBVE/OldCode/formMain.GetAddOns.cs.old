using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OpenBve {
	internal partial class formMain : Form {
		
		// --- members ---
		
		/*
		 * There are three kinds of asynchronous operations used in this file.
		 * These operations are mutually exclusive, meaning only one of these
		 * operations may be executed at a time. In order to check whether any
		 * of these operations are performed, check the CurrentDatabaseThread,
		 * CurrentInstallThreads and CurrentRemoveThread members. If one of
		 * these members is not a null reference, the respective operation is
		 * in operation. You can also check the IsBusy function.
		 * */
		
		/// <summary>The thread that currently downloads the database, or a null reference.</summary>
		private Thread CurrentDatabaseThread = null;
		
		/// <summary>The current database, or a null reference.</summary>
		private ManagedContent.Database CurrentDatabase = null;
		
		/// <summary>The threads that currently download and install packages, or a null reference.</summary>
		private Thread[] CurrentInstallThreads = null;
		
		/// <summary>The packages that are left to be downloaded and installed, or a null reference.</summary>
		private ManagedContent.Version[] CurrentInstallPackages = null;
		
		/// <summary>The total size of the download operation.</summary>
		private int CurrentDownloadTotalSize = 0;
		
		/// <summary>The current size of the download operation.</summary>
		private int CurrentDownloadCurrentSize = 0;
		
		/// <summary>The thread that currently removes packages, or a null reference.</summary>
		private Thread CurrentRemoveThread = null;
		
		/// <summary>The current list of screenshots.</summary>
		private string[] Screenshots = null;
		
		/// <summary>The currently displayed screenshot (or thumbnail).</summary>
		private int ScreenshotIndex = 0;

		/// <summary>The number of days screenshots and thumbnails are cached.</summary>
		private const int NumberOfDaysScreenshotsAreCached = 14;

		
		// --- retrieve database ---
		
		/// <summary>Updates the Get add-ons screen when entered.</summary>
		private void EnterGetAddOns() {
			if (!IsBusy()) {
				labelDownloading.Text = Interface.GetInterfaceString("getaddons_connect");
				progressbarDownloading.Style = ProgressBarStyle.Marquee;
				panelPackages.Enabled = false;
				CurrentDatabase = null;
				CurrentDatabaseThread = new Thread(ConnectToServer);
				CurrentDatabaseThread.IsBackground = true;
				CurrentDatabaseThread.Start();
			}
		}
		
		/// <summary>Checks whether any of the asynchronous operations is currenly in operation.</summary>
		/// <returns>A boolean indicating whether any of the asynchronous operations is currenly in operation.</returns>
		private bool IsBusy() {
			return CurrentDatabaseThread != null | CurrentInstallThreads != null | CurrentRemoveThread != null;
		}
		
		/// <summary>Connects to the server and populates the database fields. Intended to be executed from a worker thread.</summary>
		private void ConnectToServer() {
			string[] urls = new string[] {
				"http://odakyufan.zxq.net/packages.dat",
				"http://www.railsimroutes.net/packages.dat"
			};
			string[] names = new string[] {
				"odakyufan.zxq.net",
				"railsimroutes.net"
			};
			string directory = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "Cache");
			string file = System.IO.Path.Combine(directory, "packages.dat");
			if (System.IO.File.Exists(file)) {
				try {
					if ((DateTime.Now - System.IO.File.GetLastWriteTime(file)).TotalMinutes < 10.0) {
						byte[] bytes = System.IO.File.ReadAllBytes(file);
						CurrentDatabase = ManagedContent.Database.Load(bytes);
						this.Invoke(new ThreadStart(
							() => {
								labelDownloading.Text = string.Empty;
							}
						));
					}
				} catch { }
			}
			string error = null;
			if (CurrentDatabase == null) {
				for (int i = 0; i < urls.Length; i++) {
					this.Invoke(new ThreadStart(
						() => {
							labelDownloading.Text = Interface.GetInterfaceString("getaddons_connect") + "\n" + names[i];
						}
					));
					int size = 0;
					byte[] bytes;
					if (Internet.TryDownloadBytesFromUrl(urls[i], out bytes, ref size)) {
						try {
							CurrentDatabase = ManagedContent.Database.Load(bytes);
							this.Invoke(new ThreadStart(
								() => {
									labelDownloading.Text = string.Empty;
								}
							));
							try {
								System.IO.Directory.CreateDirectory(directory);
							} catch { }
							try {
								System.IO.File.WriteAllBytes(file, bytes);
							} catch { }
							break;
						} catch (Exception ex) {
							error = ex.Message;
						}
					}
				}
			}
			CurrentDatabaseThread = null;
			this.Invoke(new ThreadStart(
				() => {
					progressbarDownloading.Style = ProgressBarStyle.Blocks;
					if (CurrentDatabase != null) {
						panelPackages.Enabled = true;
						ManagedContent.Version[] updates = GetAvailableUpdates();
						if (updates != null && updates.Length != 0) {
							textboxFilter.Text = string.Empty;
							checkboxFilterRoutes.Checked = true;
							checkboxFilterTrains.Checked = true;
							checkboxFilterLibraries.Checked = true;
							checkboxFilterSharedLibraries.Checked = true;
							checkboxFilterNoWIPs.Checked = false;
							checkboxFilterUpdates.Checked = true;
							timerFilter.Enabled = false;
							ShowDatabase(true);
							TreeviewPackagesAfterSelect(null, null);
							labelDownloading.Text = Interface.GetInterfaceString("getaddons_updates");
							if (MessageBox.Show(Interface.GetInterfaceString("getaddons_updates_install"), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
								ButtonPackageInstallClick(updates, null);
							} else {
								checkboxFilterLibraries.Checked = false;
								checkboxFilterSharedLibraries.Checked = false;
								checkboxFilterUpdates.Checked = false;
								timerFilter.Enabled = false;
								ShowDatabase(false);
							}
						} else {
							ShowDatabase(false);
						}
					} else if (error != null) {
						labelDownloading.Text = Interface.GetInterfaceString("getaddons_connect_error") + "\n" + error;
					} else {
						labelDownloading.Text = Interface.GetInterfaceString("getaddons_connect_failure");
					}
				}
			));
		}
		
		
		// --- show database ---
		
		private class RemoveIfPossibleAttribute { }
		
		/// <summary>Shows the list of available packages.</summary>
		private void ShowDatabase(bool expand) {
			if (CurrentDatabase != null & !IsBusy()) {
				ClearPackageDetails();
				string[] keywords = textboxFilter.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				/*
				 * Collect packages we want to display.
				 * */
				List<ManagedContent.Version> packages = new List<ManagedContent.Version>();
				if (CurrentDatabase != null) {
					string filter = textboxFilter.Text;
					bool routes = checkboxFilterRoutes.Checked;
					bool trains = checkboxFilterTrains.Checked;
					bool libraries = checkboxFilterLibraries.Checked;
					bool sharedLibraries = checkboxFilterSharedLibraries.Checked;
					bool noWIPs = checkboxFilterNoWIPs.Checked;
					bool onlyUpdates = checkboxFilterUpdates.Checked;
					foreach (ManagedContent.Package package in CurrentDatabase.Packages) {
						if (!onlyUpdates || !ManagedContent.IsInstalledPackageProtected(package.Name)) {
							ManagedContent.Version latestVersion = package.Versions[package.Versions.Length - 1];
							if (!noWIPs || ManagedContent.CompareVersions(latestVersion.Number, "1.0") >= 0 && !latestVersion.GetMetadata("wip", null, "false").Equals("true", StringComparison.OrdinalIgnoreCase)) {
								string type = latestVersion.GetMetadata("type", null, null);
								if (type == "route" & routes | type == "train" & trains | type == "library" & libraries | type == "shared library" & sharedLibraries) {
									string currentVersion = ManagedContent.GetInstalledPackageVersion(package.Name);
									if (!onlyUpdates || currentVersion != null && ManagedContent.CompareVersions(latestVersion.Number, currentVersion) > 0) {
										bool add = true;
										if (keywords.Length != 0) {
											for (int i = 0; i < keywords.Length; i++) {
												if (package.Name.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) < 0 && !latestVersion.ContainsKeyword(keywords[i])) {
													add = false;
													break;
												}
											}
										}
										if (add) {
											packages.Add(latestVersion);
										}
									}
								}
							}
						}
					}
				}
				/*
				 * Group the list of packages by country and city, then display.
				 * */
				treeviewPackages.BeginUpdate();
				treeviewPackages.Nodes.Clear();
				string otherCountries = Interface.GetInterfaceString("getaddons_other_countries");
				string otherCities = Interface.GetInterfaceString("getaddons_other_cities");
				string otherOperators = Interface.GetInterfaceString("getaddons_other_operators");
				foreach (ManagedContent.Version package in packages) {
					string type = package.GetMetadata("type", null, string.Empty);
					if (string.Equals(type, "shared library", StringComparison.OrdinalIgnoreCase)) {
						type = "library";
					}
					string country = package.GetMetadata("country", CurrentLanguageCode, otherCountries);
					string flag = GetFlagFromEnUsCountry(package.GetMetadata("country", "en-US", null), "folder");
					string city = package.GetMetadata("city", CurrentLanguageCode, otherCities);
					string operatorx = package.GetMetadata("operator", CurrentLanguageCode, otherOperators);
					string caption = package.GetMetadata("caption", CurrentLanguageCode, package.Name);
					string currentVersion = ManagedContent.GetInstalledPackageVersion(package.Name);
					TreeNode node;
					node = treeviewPackages.Nodes.Add(country);
					node.ImageKey = flag;
					node.SelectedImageKey = flag;
					node = node.Nodes.Add(city);
					node.ImageKey = "folder";
					node.SelectedImageKey = "folder";
					node = node.Nodes.Add(operatorx);
					node.ImageKey = "folder";
					node.SelectedImageKey = "folder";
					switch (type.ToLowerInvariant()) {
						case "route":
							node = node.Nodes.Add(checkboxFilterRoutes.Text);
							break;
						case "train":
							node = node.Nodes.Add(checkboxFilterTrains.Text);
							break;
						case "library":
							node = node.Nodes.Add(checkboxFilterLibraries.Text);
							break;
						case "shared library":
							node = node.Nodes.Add(checkboxFilterSharedLibraries.Text);
							break;
						default:
							node = node.Nodes.Add(type);
							break;
					}
					node.ImageKey = "folder";
					node.SelectedImageKey = "folder";
					node.Tag = new RemoveIfPossibleAttribute();
					node = node.Nodes.Add(caption);
					string imageKey;
					if (ManagedContent.IsInstalledPackageProtected(package.Name)) {
						imageKey = type + "_protected";
					} else if (currentVersion == null) {
						imageKey = type + "_notinstalled";
					} else if (ManagedContent.CompareVersions(currentVersion, package.Number) < 0) {
						imageKey = type + "_outdatedversion";
					} else {
						imageKey = type + "_latestversion";
					}
					node.ImageKey = imageKey;
					node.SelectedImageKey = imageKey;
					node.Tag = package;
				}
				Group(treeviewPackages.Nodes);
				if (keywords.Length == 0) {
					foreach (TreeNode node in treeviewPackages.Nodes) {
						Flatten(node.Nodes, false);
					}
				} else {
					Flatten(treeviewPackages.Nodes, false);
				}
				treeviewPackages.Sort();
				if (expand) {
					treeviewPackages.ExpandAll();
				} else {
					Expand(treeviewPackages.Nodes, treeviewPackages.Height / treeviewPackages.ItemHeight - 1);
				}
				treeviewPackages.EndUpdate();
				buttonPackageInstall.Enabled = false;
				buttonPackageRemove.Enabled = false;
			}
		}
		
		/// <summary>Gets the flag code for the specified country.</summary>
		/// <param name="country">The country in en-US.</param>
		/// <param name="fallback">The fallback in case the flag is not defined.</param>
		/// <returns>The flag.</returns>
		private static string GetFlagFromEnUsCountry(string country, string fallback) {
			if (country != null) {
				switch (country.ToLowerInvariant()) {
						case "austria": return "AT";
						case "belgium": return "BE";
						case "brazil": return "BR";
						case "switzerland": return "CH";
						case "china": return "CN";
						case "czech republic": return "CZ";
						case "germany": return "DE";
						case "spain": return "ES";
						case "france": return "FR";
						case "united kingdom": return "GB";
						case "hong kong": return "HK";
						case "hungary": return "HU";
						case "italy": return "IT";
						case "japan": return "JP";
						case "south korea": return "KR";
						case "netherlands": return "NL";
						case "poland": return "PL";
						case "portugal": return "PT";
						case "romania": return "RO";
						case "russia": return "RU";
						case "singapore": return "SG";
						case "taiwan": return "TW";
						case "united states": return "US";
						default: return fallback;
				}
			} else {
				return fallback;
			}
		}
		
		private void Group(TreeNodeCollection collection) {
			/* Group folders that have same text */
			for (int i = 1; i < collection.Count; i++) {
				if (collection[i].Tag == null | collection[i].Tag is RemoveIfPossibleAttribute) {
					for (int j = 0; j < i; j++) {
						if (collection[j].Tag == null | collection[j].Tag is RemoveIfPossibleAttribute) {
							if (collection[i].Text == collection[j].Text) {
								TreeNodeCollection elements = collection[i].Nodes;
								collection.RemoveAt(i);
								foreach (TreeNode node in elements) {
									collection[j].Nodes.Add(node);
								}
								i--;
								break;
							}
						}
					}
				}
			}
			/* Recursion */
			foreach (TreeNode node in collection) {
				Group(node.Nodes);
			}
		}
		
		private void Flatten(TreeNodeCollection collection, bool shortify) {
			/* Recursion */
			foreach (TreeNode node in collection) {
				Flatten(node.Nodes, shortify);
			}
			/* Remove empty folders from the collection */
			for (int i = 0; i < collection.Count; i++) {
				if (collection[i].Tag == null && collection[i].Nodes.Count == 0) {
					collection.RemoveAt(i);
					i--;
				}
			}
			if (!shortify) {
				/* If only element is Route/Train/Library/SharedLibrary, then remove it */
				if (collection.Count == 1 && collection[0].Tag is RemoveIfPossibleAttribute) {
					TreeNodeCollection elements = collection[0].Nodes;
					collection.RemoveAt(0);
					foreach (TreeNode element in elements) {
						collection.Add(element);
					}
				}
				/* Expand folders that only contain one element */
				if (collection.Count == 1) {
					if (collection[0].Nodes.Count != 0) {
						collection[0].Expand();
					}
				}
			} else {
				/* Flatten out folders that contain only one element */
				if (collection.Count == 1 && collection[0].Tag == null) {
					TreeNodeCollection elements = collection[0].Nodes;
					collection.RemoveAt(0);
					foreach (TreeNode element in elements) {
						collection.Add(element);
					}
				}
			}
		}
		
		private int Count(TreeNodeCollection collection) {
			int count = collection.Count;
			foreach (TreeNode node in collection) {
				count += Count(node.Nodes);
			}
			return count;
		}
		
		private void Expand(TreeNodeCollection collection, int total) {
			int count = Count(collection);
			if (count <= total) {
				foreach (TreeNode node in collection) {
					node.ExpandAll();
				}
			}
		}
		
		
		// --- functions ---
		
		/// <summary>Gets a textual description of a file size.</summary>
		/// <param name="size">The size in bytes.</param>
		/// <returns>The textual description of the size.</returns>
		private string GetStringFromSize(int size) {
			if (size < 1024) {
				return size.ToString() + " B";
			} else if (size < 1048576) {
				return ((double)size / 1024.0).ToString("0.0") + " KiB";
			} else if (size < 1073741824) {
				return ((double)size / 1048576.0).ToString("0.0") + " MiB";
			} else {
				return ((double)size / 1073741824.0).ToString("0.0") + " GiB";
			}
		}
		
		/// <summary>Checks whether updates are available for any of the installed add-ons.</summary>
		/// <returns>Whether updates are available for any of the installed add-ons.</returns>
		private bool AreUpdatesAvailable() {
			if (CurrentDatabase != null) {
				foreach (ManagedContent.Package package in CurrentDatabase.Packages) {
					string currentVersion = ManagedContent.GetInstalledPackageVersion(package.Name);
					if (currentVersion != null) {
						if (!ManagedContent.IsInstalledPackageProtected(package.Name)) {
							if (ManagedContent.CompareVersions(package.Versions[package.Versions.Length - 1].Number, currentVersion) > 0) {
								return true;
							}
						}
					}
				}
				return false;
			} else {
				return false;
			}
		}
		
		/// <summary>Gets a list of available updates.</summary>
		/// <returns>The list of the latest packages.</returns>
		private ManagedContent.Version[] GetAvailableUpdates() {
			if (CurrentDatabase != null) {
				List<ManagedContent.Version> updates = new List<ManagedContent.Version>();
				foreach (ManagedContent.Package package in CurrentDatabase.Packages) {
					string currentVersion = ManagedContent.GetInstalledPackageVersion(package.Name);
					if (currentVersion != null) {
						if (!ManagedContent.IsInstalledPackageProtected(package.Name)) {
							if (ManagedContent.CompareVersions(package.Versions[package.Versions.Length - 1].Number, currentVersion) > 0) {
								updates.Add(package.Versions[package.Versions.Length - 1]);
							}
						}
					}
				}
				return updates.ToArray();
			} else {
				return null;
			}
		}
		
		
		// --- events ---
		
		private void LinkPackageHomepageLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			if (e.Link.LinkData is string) {
				try {
					System.Diagnostics.Process.Start((string)e.Link.LinkData);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		private void ClearPackageDetails() {
			buttonPackageInstall.Enabled = false;
			buttonPackageRemove.Enabled = false;
			labelPackageInformation.Text = string.Empty;
			textboxPackageDescription.Text = string.Empty;
			linkPackageHomepage.Links.Clear();
			linkPackageHomepage.Text = string.Empty;
			groupboxPackage.Enabled = false;
			pictureboxScreenshot.Image = null;
			pictureboxScreenshot.Cursor = Cursors.Default;
			buttonScreenshotPrevious.Enabled = false;
			buttonScreenshotNext.Enabled = false;
			Screenshots = null;
			ScreenshotIndex = 0;
		}
		
		private void TreeviewPackagesAfterSelect(object sender, TreeViewEventArgs e) {
			if (treeviewPackages.SelectedNode == null || treeviewPackages.SelectedNode.Tag == null) {
				ClearPackageDetails();
			} else if (treeviewPackages.SelectedNode.Tag is ManagedContent.Version) {
				ManagedContent.Version version = (ManagedContent.Version)treeviewPackages.SelectedNode.Tag;
				string currentVersion = ManagedContent.GetInstalledPackageVersion(version.Name);
				string caption = version.GetMetadata("caption", CurrentLanguageCode, null);
				string country = version.GetMetadata("country", CurrentLanguageCode, null);
				string city = version.GetMetadata("city", CurrentLanguageCode, null);
				string operatorx = version.GetMetadata("operator", CurrentLanguageCode, null);
				bool fictional = string.Equals(version.GetMetadata("fictional", null, string.Empty), "true", StringComparison.OrdinalIgnoreCase);
				string author = version.GetMetadata("author", CurrentLanguageCode, null);
				StringBuilder builder = new StringBuilder();
				if (caption != null) {
					builder.AppendLine(caption);
				}
				if (country != null) {
					builder.Append(Interface.GetInterfaceString("getaddons_package_country"));
					builder.AppendLine(country);
				}
				if (city != null) {
					builder.Append(Interface.GetInterfaceString("getaddons_package_city"));
					builder.AppendLine(city);
				}
				if (operatorx != null) {
					builder.Append(Interface.GetInterfaceString("getaddons_package_operator"));
					builder.AppendLine(operatorx);
				}
				if (fictional) {
					builder.AppendLine(Interface.GetInterfaceString("getaddons_package_fictional"));
				}
				builder.AppendLine();
				builder.AppendLine(version.Name);
				if (author != null) {
					builder.Append(Interface.GetInterfaceString("getaddons_package_author"));
					builder.AppendLine(author);
				}
				builder.Append(Interface.GetInterfaceString("getaddons_package_version_latest"));
				builder.AppendLine(version.Number);
				if (currentVersion != null) {
					builder.Append(Interface.GetInterfaceString("getaddons_package_version_installed"));
					builder.AppendLine(currentVersion);
				}
				labelPackageInformation.Text = builder.ToString().Trim();
				textboxPackageDescription.Text = version.GetMetadata("description", CurrentLanguageCode, string.Empty).Replace(@"\n", "\x0D\x0A");
				string[] links = version.GetMetadata("links", CurrentLanguageCode, string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (links.Length != 0) {
					builder = new StringBuilder();
					string[] labels = version.GetMetadata("labels", CurrentLanguageCode, string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					linkPackageHomepage.Links.Clear();
					for (int i = 0; i < links.Length; i++) {
						if (i != 0) {
							builder.Append(' ');
						}
						string label;
						if (i < labels.Length) {
							label = labels[i].Trim();
						} else {
							try {
								label = System.IO.Path.GetFileNameWithoutExtension(links[i]);
							} catch {
								label = "Link" + (i + 1).ToString();
							}
						}
						linkPackageHomepage.Links.Add(builder.Length, label.Length, links[i].Trim());
						builder.Append(label);
					}
					linkPackageHomepage.Text = builder.ToString();
				} else {
					linkPackageHomepage.Links.Clear();
					linkPackageHomepage.Text = string.Empty;
				}
				groupboxPackage.Enabled = true;
				if (ManagedContent.IsInstalledPackageProtected(version.Name)) {
					buttonPackageInstall.Enabled = false;
					buttonPackageRemove.Enabled = false;
				} else {
					if (currentVersion == null) {
						buttonPackageInstall.Enabled = true;
						buttonPackageRemove.Enabled = false;
					} else if (ManagedContent.CompareVersions(currentVersion, version.Number) < 0) {
						buttonPackageInstall.Enabled = true;
						buttonPackageRemove.Enabled = true;
					} else {
						buttonPackageInstall.Enabled = false;
						buttonPackageRemove.Enabled = true;
					}
				}
				/* Prepare screenshots and thumbnails */
				string[] screenshots = version.GetMetadata("screenshots", null, string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				string[] thumbs = version.GetMetadata("thumbnails", null, string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (thumbs.Length < screenshots.Length) {
					int count = thumbs.Length;
					Array.Resize<string>(ref thumbs, screenshots.Length);
					for (int i = count; i < screenshots.Length; i++) {
						thumbs[i] = screenshots[i];
					}
				} else if (thumbs.Length > screenshots.Length) {
					int count = screenshots.Length;
					Array.Resize<string>(ref screenshots, thumbs.Length);
					for (int i = count; i < thumbs.Length; i++) {
						screenshots[i] = thumbs[i];
					}
				}
				for (int i = 0; i < screenshots.Length; i++) {
					screenshots[i] = screenshots[i].Trim();
					thumbs[i] = thumbs[i].Trim();
				}
				pictureboxScreenshot.Image = null;
				pictureboxScreenshot.Cursor = screenshots.Length >= 1 ? Cursors.Hand : Cursors.Default;
				buttonScreenshotPrevious.Enabled = false;
				buttonScreenshotNext.Enabled = screenshots.Length >= 2;
				Screenshots = screenshots;
				ScreenshotIndex = 0;
				string tempDirectory = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "Cache");
				for (int i = 0; i < thumbs.Length; i++) {
					ParameterizedThreadStart callback;
					if (i == 0) {
						callback = ShowThumbnailAsynchronous;
					} else {
						callback = null;
					}
					Internet.DownloadAndSaveAsynchronous(thumbs[i], System.IO.Path.Combine(tempDirectory, version.Name + "_" + version.Number + "_" + i.ToString() + "_thumb"), NumberOfDaysScreenshotsAreCached, callback);
				}
			}
		}
		
		private void PictureboxScreenshotClick(object sender, EventArgs e) {
			if (Screenshots != null && ScreenshotIndex < Screenshots.Length && treeviewPackages.SelectedNode != null && treeviewPackages.SelectedNode.Tag != null) {
				ManagedContent.Version version = (ManagedContent.Version)treeviewPackages.SelectedNode.Tag;
				string tempDirectory = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "Cache");
				for (int i = 0; i < Screenshots.Length; i++) {
					ParameterizedThreadStart callback;
					if (i == ScreenshotIndex) {
						callback = (object data) => {
							string file = (string)data;
							try {
								formImage.ShowImageDialog(Image.FromFile(file));
							} catch {
								System.Media.SystemSounds.Exclamation.Play();
							}
						};
					} else {
						callback = null;
					}
					Internet.DownloadAndSaveAsynchronous(Screenshots[i], System.IO.Path.Combine(tempDirectory, version.Name + "_" + version.Number + "_" + i.ToString()), NumberOfDaysScreenshotsAreCached, callback);
				}
			}
		}

		private void ButtonScreenshotPreviousClick(object sender, EventArgs e) {
			if (ScreenshotIndex > 0) {
				ScreenshotIndex--;
				ShowThumbnail();
			}
		}
		
		private void ButtonScreenshotNextClick(object sender, EventArgs e) {
			if (Screenshots != null && ScreenshotIndex < Screenshots.Length - 1) {
				ScreenshotIndex++;
				ShowThumbnail();
			}
		}
		
		/// <summary>Shows the current thumbnail. Can be called from any thread.</summary>
		private void ShowThumbnailAsynchronous(object data) {
			this.Invoke(new ThreadStart(ShowThumbnail));
		}
		
		/// <summary>Shows the current thumbnail. Must only be called from the main thread.</summary>
		private void ShowThumbnail() {
			if (Screenshots == null || ScreenshotIndex >= Screenshots.Length || treeviewPackages.SelectedNode == null || treeviewPackages.SelectedNode.Tag == null) {
				pictureboxScreenshot.Image = null;
				buttonScreenshotPrevious.Enabled = false;
				buttonScreenshotNext.Enabled = false;
			} else {
				ManagedContent.Version version = (ManagedContent.Version)treeviewPackages.SelectedNode.Tag;
				string directory = System.IO.Path.Combine(Program.FileSystem.SettingsFolder, "Cache");
				string file = System.IO.Path.Combine(directory, version.Name + "_" + version.Number + "_" + ScreenshotIndex.ToString() + "_thumb");
				if (System.IO.File.Exists(file)) {
					try {
						pictureboxScreenshot.Image = Image.FromFile(file);
					} catch {
						pictureboxScreenshot.Image = null;
					}
				}
				buttonScreenshotPrevious.Enabled = ScreenshotIndex > 0;
				buttonScreenshotNext.Enabled = ScreenshotIndex < Screenshots.Length - 1;
			}
		}
		
		
		// --- install ---
		
		/// <summary>Raised when the Install button is clicked.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event args.</param>
		private void ButtonPackageInstallClick(object sender, EventArgs e) {
			if (CurrentDatabase != null & !IsBusy()) {
				if ((treeviewPackages.SelectedNode != null && treeviewPackages.SelectedNode.Tag != null) || sender is ManagedContent.Version[]) {
					/*
					 * Build a list of all selected packages.
					 * */
					List<ManagedContent.Version> packages;
					if (sender is ManagedContent.Version[]) {
						packages = new List<ManagedContent.Version>((ManagedContent.Version[])sender);
					} else {
						packages = new List<ManagedContent.Version>();
						packages.Add((ManagedContent.Version)treeviewPackages.SelectedNode.Tag);
					}
					/*
					 * Go through the list of packages and add
					 * their suggestions if the user confirms.
					 * */
					List<ManagedContent.Dependency> suggestions = CurrentDatabase.GetSuggestions(packages);
					if (suggestions.Count != 0) {
						System.Text.StringBuilder builder = new System.Text.StringBuilder();
						foreach (ManagedContent.Dependency suggestion in suggestions) {
							builder.Append(suggestion.Name).Append(" (").Append(suggestion.Version).AppendLine(")");
							ManagedContent.Version version = CurrentDatabase.Dereference(suggestion.Name, suggestion.Version);
							if (version != null) {
								string caption = version.GetMetadata("caption", CurrentLanguageCode, null);
								if (caption != null) {
									builder.AppendLine(caption);
								}
							}
							builder.AppendLine();
						}
						switch (MessageBox.Show(Interface.GetInterfaceString("getaddons_suggest_1") + "\n\n" + builder.ToString() + Interface.GetInterfaceString("getaddons_suggest_2"), Interface.GetInterfaceString("getaddons_package_install"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
							case DialogResult.Yes:
								foreach (ManagedContent.Dependency suggestion in suggestions) {
									ManagedContent.Version version = CurrentDatabase.Dereference(suggestion.Name, suggestion.Version);
									if (version != null) {
										packages.Add(version);
									}
								}
								break;
							case DialogResult.Cancel:
								return;
						}
					}
					/*
					 * Go through the list of packages and add
					 * their dependencies.
					 * */
					if (!CurrentDatabase.AddDependencies(packages)) {
						if (MessageBox.Show("Some dependencies were not found. This indicates a bug the server that compiled the list of add-ons.", Interface.GetInterfaceString("getaddons_package_install"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel) {
							return;
						}
					}
					if (packages.Count == 0) {
						return;
					}
					/*
					 * Calculate total size of download.
					 * */
					CurrentDownloadCurrentSize = 0;
					CurrentDownloadTotalSize = 0;
					foreach (ManagedContent.Version package in packages) {
						if (package.Sources.Length != 0) {
							CurrentDownloadTotalSize += package.Sources[0].Size;
						}
					}
					/*
					 * Ask for final confirmation.
					 * */
					if (MessageBox.Show(Interface.GetInterfaceString("getaddons_confirmation").Replace("[size]", GetStringFromSize(CurrentDownloadTotalSize)), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
						return;
					}
					/*
					 * Prepare job for asynchronous download.
					 * */
					List<string> unsuccessful = new List<string>();
					ThreadStart job = new ThreadStart(
						() => {
							while (true) {
								ManagedContent.Version version = null;
								lock (this) {
									if (CurrentInstallPackages.Length != 0) {
										version = CurrentInstallPackages[CurrentInstallPackages.Length - 1];
										Array.Resize<ManagedContent.Version>(ref CurrentInstallPackages, CurrentInstallPackages.Length - 1);
									} else {
										break;
									}
								}
								if (version != null) {
									if (!CurrentDatabase.InstallPackage(version, ref CurrentDownloadCurrentSize)) {
										lock (this) {
											unsuccessful.Add(version.Name + " (" + version.Number + ")");
										}
									}
								}
							}
							bool finalize;
							lock (this) {
								for (int i = 0; i < CurrentInstallThreads.Length; i++) {
									if (CurrentInstallThreads[i] == Thread.CurrentThread) {
										CurrentInstallThreads[i] = CurrentInstallThreads[CurrentInstallThreads.Length - 1];
										Array.Resize<Thread>(ref CurrentInstallThreads, CurrentInstallThreads.Length - 1);
									}
								}
								finalize = CurrentInstallThreads.Length == 0;
							}
							if (finalize) {
								this.Invoke(new ThreadStart(
									() => {
										timerInstall.Enabled = false;
										progressbarDownloading.Value = 0;
										progressbarDownloading.Style = ProgressBarStyle.Blocks;
										if (unsuccessful.Count == 0) {
											labelDownloading.Text = string.Empty;
										} else {
											labelDownloading.Text = Interface.GetInterfaceString("getaddons_package_install_failure") + "\n" + string.Join(", ", unsuccessful.ToArray());
										}
										panelPackages.Enabled = true;
										CurrentInstallThreads = null;
										bool noMoreUpdates = checkboxFilterUpdates.Checked && !AreUpdatesAvailable();
										if (noMoreUpdates) {
											checkboxFilterRoutes.Checked = true;
											checkboxFilterTrains.Checked = true;
											checkboxFilterLibraries.Checked = false;
											checkboxFilterSharedLibraries.Checked = false;
											checkboxFilterUpdates.Checked = false;
											timerFilter.Enabled = false;
										}
										ShowDatabase(false);
										if (noMoreUpdates && unsuccessful.Count == 0) {
											labelDownloading.Text = Interface.GetInterfaceString("getaddons_updates_nomore");
										}
										TextboxRouteFilterTextChanged(null, null);
										TextboxTrainFilterTextChanged(null, null);
										Program.SetPackageLookupDirectories();
									}
								));
							}
						}
					);
					/*
					 * Start asynchronous download.
					 * */
					const int numberOfParallelDownloads = 2;
					labelDownloading.Text = Interface.GetInterfaceString("getaddons_package_install_progress");
					progressbarDownloading.Style = ProgressBarStyle.Continuous;
					panelPackages.Enabled = false;
					timerInstall.Enabled = true;
					CurrentInstallPackages = packages.ToArray();
					CurrentInstallThreads = new Thread[numberOfParallelDownloads];
					lock (this) {
						for (int i = 0; i < CurrentInstallThreads.Length; i++) {
							CurrentInstallThreads[i] = new Thread(job);
							CurrentInstallThreads[i].IsBackground = true;
							CurrentInstallThreads[i].Start();
						}
					}
				}
			} else {
				System.Media.SystemSounds.Asterisk.Play();
			}
		}
		
		
		// --- remove ---
		
		/// <summary>Raised when the Remove button is clicked.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event args.</param>
		private void ButtonPackageRemoveClick(object sender, EventArgs e) {
			if (CurrentDatabase != null & !IsBusy()) {
				if (treeviewPackages.SelectedNode != null && treeviewPackages.SelectedNode.Tag != null) {
					/*
					 * Build a list of all selected packages
					 * that are not protected.
					 * */
					List<string> packages = new List<string>();
					packages.Add(((ManagedContent.Version)treeviewPackages.SelectedNode.Tag).Name);
					/*
					 * Add all dependent and redundant items.
					 * */
					List<string> results = ManagedContent.GetDependentAndRedundantPackages(packages);
					if (results.Count != 0) {
						System.Text.StringBuilder builder = new System.Text.StringBuilder();
						foreach (string result in results) {
							string number = ManagedContent.GetInstalledPackageVersion(result);
							if (number != null) {
								builder.Append(result).Append(" (").Append(number).AppendLine(")");
							} else {
								builder.AppendLine(result);
							}
							builder.AppendLine();
						}
						switch (MessageBox.Show(Interface.GetInterfaceString("getaddons_redundant_1") + "\n\n" + builder.ToString() + Interface.GetInterfaceString("getaddons_redundant_2"), Interface.GetInterfaceString("getaddons_remove"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)) {
							case DialogResult.Yes:
								packages.AddRange(results);
								break;
							case DialogResult.Cancel:
								return;
						}
					}
					/*
					 * Remove the packages.
					 * */
					labelDownloading.Text = Interface.GetInterfaceString("getaddons_package_remove_progress");
					progressbarDownloading.Style = ProgressBarStyle.Marquee;
					panelPackages.Enabled = false;
					CurrentRemoveThread = new Thread(
						() => {
							List<string> unsuccessful = new List<string>();
							foreach (string package in packages) {
								if (!ManagedContent.RemovePackage(package)) {
									unsuccessful.Add(package);
								}
							}
							this.Invoke(new ThreadStart(
								() => {
									if (unsuccessful.Count == 0) {
										labelDownloading.Text = string.Empty;
									} else {
										labelDownloading.Text = Interface.GetInterfaceString("getaddons_package_remove_failure") + "\n" + string.Join(", ", unsuccessful.ToArray());
									}
									progressbarDownloading.Style = ProgressBarStyle.Blocks;
									panelPackages.Enabled = true;
									CurrentRemoveThread = null;
									ShowDatabase(false);
									Program.SetPackageLookupDirectories();
									TextboxRouteFilterTextChanged(null, null);
									TextboxTrainFilterTextChanged(null, null);
								}
							));
						}
					);
					CurrentRemoveThread.IsBackground = true;
					CurrentRemoveThread.Start();
				}
			} else {
				System.Media.SystemSounds.Asterisk.Play();
			}
		}
		
		
		// --- cache ---
		
		private void ClearCache(string directory, double days) {
			if (System.IO.Directory.Exists(directory)) {
				string[] files = System.IO.Directory.GetFiles(directory);
				foreach (string file in files) {
					try {
						DateTime lastWrite = System.IO.File.GetLastWriteTime(file);
						TimeSpan span = DateTime.Now - lastWrite;
						if (span.TotalDays > days) {
							System.IO.File.Delete(file);
						}
					} catch { }
				}
			}
		}
		
		
		// --- events ---
		
		/// <summary>Raised every once in a while to update the download progress.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event args.</param>
		private void TimerInstallTick(object sender, EventArgs e) {
			if (CurrentDownloadCurrentSize < CurrentDownloadTotalSize) {
				double fraction = CurrentDownloadTotalSize != 0.0 ? (double)CurrentDownloadCurrentSize / (double)CurrentDownloadTotalSize : 0.0;
				if (fraction < 0.0) fraction = 0.0;
				if (fraction > 1.0) fraction = 1.0;
				progressbarDownloading.Value = progressbarDownloading.Minimum + (int)Math.Floor(fraction * (progressbarDownloading.Maximum - progressbarDownloading.Minimum));
				labelDownloading.Text = Interface.GetInterfaceString("getaddons_package_install_download") + "\n" + GetStringFromSize(CurrentDownloadCurrentSize) + " / " + GetStringFromSize(CurrentDownloadTotalSize);
			} else {
				progressbarDownloading.Value = progressbarDownloading.Maximum;
				labelDownloading.Text = Interface.GetInterfaceString("getaddons_package_install_progress");
			}
		}
		
		private void TextboxFilterTextChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void CheckboxFilterRoutesCheckedChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void CheckboxFilterTrainsCheckedChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void CheckboxFilterLibrariesCheckedChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void CheckboxFilterSharedLibrariesCheckedChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void CheckboxFilterNoWIPsCheckedChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void CheckboxFilterUpdatesCheckedChanged(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			timerFilter.Enabled = true;
		}
		private void TimerFilterTick(object sender, EventArgs e) {
			timerFilter.Enabled = false;
			ShowDatabase(false);
		}
		
	}
}