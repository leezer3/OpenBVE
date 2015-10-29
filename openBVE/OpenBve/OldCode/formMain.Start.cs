using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
		// --- route add-ons ---
		
		private void TextboxRouteFilterTextChanged(object sender, EventArgs e) {
			treeviewRouteAddOns.BeginUpdate();
			treeviewRouteAddOns.Nodes.Clear();
			string[] keywords = textboxRouteFilter.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string lookupDirectory in Program.FileSystem.ManagedContentFolders) {
				string[] packageDirectories = Directory.GetDirectories(lookupDirectory);
				foreach (string packageDirectory in packageDirectories) {
					string file = OpenBveApi.Path.CombineFile(packageDirectory, "package.cfg");
					if (File.Exists(file)) {
						string[] lines = File.ReadAllLines(file, Encoding.UTF8);
						List<ManagedContent.KeyValuePair> pairsList = new List<ManagedContent.KeyValuePair>();
						foreach (string line in lines) {
							int equals = line.IndexOf('=');
							if (equals >= 0) {
								string key = line.Substring(0, equals).Trim().ToLowerInvariant();
								string value = line.Substring(equals + 1).Trim();
								pairsList.Add(new ManagedContent.KeyValuePair(key, value));
							}
						}
						ManagedContent.KeyValuePair[] pairs = pairsList.ToArray();
						string type = ManagedContent.GetMetadata(pairs, "type", null, null);
						if (type != null && type.Equals("route", StringComparison.OrdinalIgnoreCase)) {
							string country = ManagedContent.GetMetadata(pairs, "country", CurrentLanguageCode, Interface.GetInterfaceString("getaddons_other_countries"));
							string flag = GetFlagFromEnUsCountry(ManagedContent.GetMetadata(pairs, "country", "en-US", null), "folder");
							string city = ManagedContent.GetMetadata(pairs, "city", CurrentLanguageCode, Interface.GetInterfaceString("getaddons_other_cities"));
							string operatorx = ManagedContent.GetMetadata(pairs, "operator", CurrentLanguageCode, Interface.GetInterfaceString("getaddons_other_operators"));
							string caption = ManagedContent.GetMetadata(pairs, "caption", CurrentLanguageCode, System.IO.Path.GetFileName(packageDirectory));
							string[] metadata = new string[] { country, city, operatorx, caption };
							string entry = ManagedContent.GetMetadata(pairs, "entry", null, string.Empty);
							string path = OpenBveApi.Path.CombineDirectory(packageDirectory, entry);
							TreeNode node = treeviewRouteAddOns.Nodes.Add(country);
							node.ImageKey = flag;
							node.SelectedImageKey = flag;
							node = node.Nodes.Add(city);
							node.ImageKey = "folder";
							node.SelectedImageKey = "folder";
							node = node.Nodes.Add(operatorx);
							node.ImageKey = "folder";
							node.SelectedImageKey = "folder";
							node = node.Nodes.Add(caption);
							node.ImageKey = "folder";
							node.SelectedImageKey = "folder";
							AddRoutes(node.Nodes, path, keywords, metadata);
						}
					}
				}
			}
			Group(treeviewRouteAddOns.Nodes);
			if (keywords.Length == 0) {
				foreach (TreeNode node in treeviewRouteAddOns.Nodes) {
					Flatten(node.Nodes, true);
				}
			} else {
				Flatten(treeviewRouteAddOns.Nodes, true);
			}
			Expand(treeviewRouteAddOns.Nodes, treeviewRouteAddOns.Height / treeviewRouteAddOns.ItemHeight - 1);
			treeviewRouteAddOns.Sort();
			treeviewRouteAddOns.EndUpdate();
		}
		private void AddRoutes(TreeNodeCollection collection, string path, string[] keywords, string[] metadata) {
			if (Directory.Exists(path)) {
				string[] directories = Directory.GetDirectories(path);
				foreach (string directory in directories) {
					TreeNode node = collection.Add(Path.GetFileName(directory));
					node.ImageKey = "folder";
					node.SelectedImageKey = "folder";
					string title = Path.GetFileNameWithoutExtension(directory);
					if (keywords.Length != 0) {
						List<string> remaining = new List<string>();
						foreach (string keyword in keywords) {
							if (title != null && title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) {
								remaining.Add(keyword);
							}
						}
						AddRoutes(node.Nodes, directory, remaining.ToArray(), metadata);
					} else {
						AddRoutes(node.Nodes, directory, keywords, metadata);
					}
				}
				string[] files = Directory.GetFiles(path);
				foreach (string file in files) {
					if (file.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) | file.EndsWith(".rw", StringComparison.OrdinalIgnoreCase)) {
						string title = Path.GetFileNameWithoutExtension(file);
						bool add = true;
						foreach (string keyword in keywords) {
							add = false;
							foreach (string tag in metadata) {
								if (tag.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
									add = true;
									break;
								}
							}
							if (!add) {
								if (title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
									add = true;
								} else {
									break;
								}
							}
						}
						if (add) {
							TreeNode node = collection.Add(title);
							node.Tag = file;
							node.ImageKey = "route";
							node.SelectedImageKey = "route";
						}
					}
				}
			}
		}
		private void TreeviewRouteAddOnsAfterSelect(object sender, TreeViewEventArgs e) {
			if (treeviewRouteAddOns.SelectedNode != null && treeviewRouteAddOns.SelectedNode.Tag != null) {
				Result.RouteFile = (string)treeviewRouteAddOns.SelectedNode.Tag;
				ShowRoute(false);
			}
		}
		
		
		
		// --- train add-ons ---
		
		private void TextboxTrainFilterTextChanged(object sender, EventArgs e) {
			treeviewTrainAddOns.BeginUpdate();
			treeviewTrainAddOns.Nodes.Clear();
			string[] keywords = textboxTrainFilter.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string lookupDirectory in Program.FileSystem.ManagedContentFolders) {
				string[] packageDirectories = Directory.GetDirectories(lookupDirectory);
				foreach (string packageDirectory in packageDirectories) {
					string file = OpenBveApi.Path.CombineFile(packageDirectory, "package.cfg");
					if (File.Exists(file)) {
						string[] lines = File.ReadAllLines(file, Encoding.UTF8);
						List<ManagedContent.KeyValuePair> pairsList = new List<ManagedContent.KeyValuePair>();
						foreach (string line in lines) {
							int equals = line.IndexOf('=');
							if (equals >= 0) {
								string key = line.Substring(0, equals).Trim().ToLowerInvariant();
								string value = line.Substring(equals + 1).Trim();
								pairsList.Add(new ManagedContent.KeyValuePair(key, value));
							}
						}
						ManagedContent.KeyValuePair[] pairs = pairsList.ToArray();
						string type = ManagedContent.GetMetadata(pairs, "type", null, null);
						if (type != null && type.Equals("train", StringComparison.OrdinalIgnoreCase)) {
							string country = ManagedContent.GetMetadata(pairs, "country", CurrentLanguageCode, Interface.GetInterfaceString("getaddons_other_countries"));
							string flag = GetFlagFromEnUsCountry(ManagedContent.GetMetadata(pairs, "country", "en-US", null), "folder");
							string city = ManagedContent.GetMetadata(pairs, "city", CurrentLanguageCode, Interface.GetInterfaceString("getaddons_other_cities"));
							string operatorx = ManagedContent.GetMetadata(pairs, "operator", CurrentLanguageCode, Interface.GetInterfaceString("getaddons_other_operators"));
							string caption = ManagedContent.GetMetadata(pairs, "caption", CurrentLanguageCode, System.IO.Path.GetFileName(packageDirectory));
							string[] metadata = new string[] { country, city, operatorx, caption };
							string entry = ManagedContent.GetMetadata(pairs, "entry", null, string.Empty);
							TreeNode node;
							node = treeviewTrainAddOns.Nodes.Add(country);
							node.ImageKey = flag;
							node.SelectedImageKey = flag;
							node = node.Nodes.Add(city);
							node.ImageKey = "folder";
							node.SelectedImageKey = "folder";
							node = node.Nodes.Add(operatorx);
							node.ImageKey = "folder";
							node.SelectedImageKey = "folder";
							node = node.Nodes.Add(caption);
							node.ImageKey = "folder";
							node.SelectedImageKey = "folder";
							string path = OpenBveApi.Path.CombineDirectory(packageDirectory, entry);
							AddTrains(node.Nodes, path, keywords, metadata);
						}
					}
				}
			}
			Group(treeviewTrainAddOns.Nodes);
			if (keywords.Length == 0) {
				foreach (TreeNode node in treeviewTrainAddOns.Nodes) {
					Flatten(node.Nodes, true);
				}
			} else {
				Flatten(treeviewTrainAddOns.Nodes, true);
			}
			Expand(treeviewTrainAddOns.Nodes, treeviewTrainAddOns.Height / treeviewTrainAddOns.ItemHeight - 1);
			treeviewTrainAddOns.Sort();
			treeviewTrainAddOns.EndUpdate();
		}
		private void AddTrains(TreeNodeCollection collection, string path, string[] keywords, string[] metadata) {
			if (Directory.Exists(path)) {
				string[] directories = Directory.GetDirectories(path);
				foreach (string directory in directories) {
					if (File.Exists(OpenBveApi.Path.CombineFile(directory, "train.dat"))) {
						string title = Path.GetFileName(directory);
						bool add = true;
						foreach (string keyword in keywords) {
							add = false;
							foreach (string tag in metadata) {
								if (tag.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
									add = true;
									break;
								}
							}
							if (!add) {
								if (title != null && title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) {
									add = true;
								} else {
									break;
								}
							}
						}
						if (add) {
							TreeNode node = collection.Add(title);
							node.Tag = directory;
							node.ImageKey = "train";
							node.SelectedImageKey = "train";
						}
					} else {
						TreeNode node = collection.Add(Path.GetFileName(directory));
						node.ImageKey = "folder";
						node.SelectedImageKey = "folder";
						string title = Path.GetFileNameWithoutExtension(directory);
						if (keywords.Length != 0) {
							List<string> remaining = new List<string>();
							foreach (string keyword in keywords) {
								if (title != null && title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) {
									remaining.Add(keyword);
								}
							}
							AddTrains(node.Nodes, directory, remaining.ToArray(), metadata);
						} else {
							AddTrains(node.Nodes, directory, keywords, metadata);
						}
					}
				}
			}
		}
		private void TreeviewTrainAddOnsAfterSelect(object sender, TreeViewEventArgs e) {
			if (treeviewTrainAddOns.SelectedNode != null && treeviewTrainAddOns.SelectedNode.Tag != null) {
				Result.TrainFolder = (string)treeviewTrainAddOns.SelectedNode.Tag;
				ShowTrain(false);
				if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
			}
		}
		


		
		
		// ===============
		// route selection
		// ===============

		// route folder
		private void textboxRouteFolder_TextChanged(object sender, EventArgs e) {
			string Folder = textboxRouteFolder.Text;
			try {
				if (Folder.Length == 0) {
					// drives
					listviewRouteFiles.Items.Clear();
					try { // MoMA says that GetDrives is flagged with [MonoTodo]
						System.IO.DriveInfo[] driveInfos = System.IO.DriveInfo.GetDrives();
						for (int i = 0; i < driveInfos.Length; i++) {
							ListViewItem Item = listviewRouteFiles.Items.Add(driveInfos[i].Name);
							Item.ImageKey = "folder";
							Item.Tag = driveInfos[i].RootDirectory.FullName;
							listviewRouteFiles.Tag = null;
						}
					} catch { }
				} else if (System.IO.Directory.Exists(Folder)) {
					listviewRouteFiles.Items.Clear();
					// parent
					try {
						System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
						if (Info != null) {
							ListViewItem Item = listviewRouteFiles.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = Info.FullName;
							listviewRouteFiles.Tag = Info.FullName;
						} else {
							ListViewItem Item = listviewRouteFiles.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = "";
							listviewRouteFiles.Tag = "";
						}
					} catch { }
					// folders
					try {
						string[] Folders = System.IO.Directory.GetDirectories(Folder);
						Array.Sort<string>(Folders);
						for (int i = 0; i < Folders.Length; i++) {
							System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(Folders[i]);
							if ((info.Attributes & System.IO.FileAttributes.Hidden) == 0) {
								string Name = System.IO.Path.GetFileName(Folders[i]);
								if (!string.IsNullOrEmpty(Name) && Name[0] != '.') {
									ListViewItem Item = listviewRouteFiles.Items.Add(Name);
									Item.ImageKey = "folder";
									Item.Tag = Folders[i];
								}
							}
						}
					} catch { }
					// files
					try {
						string[] Files = System.IO.Directory.GetFiles(Folder);
						Array.Sort<string>(Files);
						for (int i = 0; i < Files.Length; i++) {
							string Extension = System.IO.Path.GetExtension(Files[i]).ToLowerInvariant();
							switch (Extension) {
								case ".rw":
								case ".csv":
									string Name = System.IO.Path.GetFileName(Files[i]);
									if (!string.IsNullOrEmpty(Name) && Name[0] != '.') {
										ListViewItem Item = listviewRouteFiles.Items.Add(Name);
										if (Extension == ".csv") {
											try {
												string text = System.IO.File.ReadAllText(Files[i], Encoding.UTF8);
												if (text.IndexOf("With Track", StringComparison.OrdinalIgnoreCase) >= 0 | text.IndexOf("Track.", StringComparison.OrdinalIgnoreCase) >= 0 | text.IndexOf("$Include", StringComparison.OrdinalIgnoreCase) >= 0) {
													Item.ImageKey = "route";
												}
											} catch { }
										} else {
											Item.ImageKey = "route";
										}
										Item.Tag = Files[i];
									}
									break;
							}
						}
					} catch { }
				}
			} catch { }
			listviewRouteFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		// route files
		private void listviewRouteFiles_SelectedIndexChanged(object sender, EventArgs e) {
            if (listviewRouteFiles.SelectedItems.Count == 1) {
				string t = listviewRouteFiles.SelectedItems[0].Tag as string;
                
				if (t != null)
				{
				    
					if (System.IO.File.Exists(t)) {
						Result.RouteFile = t;
					    //MessageBox.Show(Convert.ToString(t));
						ShowRoute(false);
					}
				}
			}
		}
		private void listviewRouteFiles_DoubleClick(object sender, EventArgs e) {
			if (listviewRouteFiles.SelectedItems.Count == 1) {
				string t = listviewRouteFiles.SelectedItems[0].Tag as string;
				if (t != null) {
					if (t.Length == 0 || System.IO.Directory.Exists(t)) {
						textboxRouteFolder.Text = t;
					}
				}
			}
		}
		private void listviewRouteFiles_KeyDown(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Return:
					listviewRouteFiles_DoubleClick(null, null);
					break;
				case Keys.Back:
					string t = listviewRouteFiles.Tag as string;
					if (t != null) {
						if (t.Length == 0 || System.IO.Directory.Exists(t)) {
							textboxRouteFolder.Text = t;
						}
					} break;
			}
		}

		// route recently
		private void listviewRouteRecently_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewRouteRecently.SelectedItems.Count == 1) {
				string t = listviewRouteRecently.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.File.Exists(t)) {
						Result.RouteFile = t;
					    /*if (Type.GetType("Mono.Runtime") != null)
					    {
					        MessageBox.Show("Please press OK- This works around a mono crash.");
					    } */
					    ShowRoute(false);
					}
				}
			}
		}

		// =============
		// route details
		// =============

		// route image
		private void pictureboxRouteImage_Click(object sender, EventArgs e) {
			if (pictureboxRouteImage.Image != null) {
				formImage.ShowImageDialog(pictureboxRouteImage.Image);
			}
		}

		// route encoding
		private void comboboxRouteEncoding_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxRouteEncoding.Tag == null) {
				int i = comboboxRouteEncoding.SelectedIndex;
				if (i >= 0 & i < EncodingCodepages.Length) {
					Result.RouteEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[i]);
					if (i == 0) {
						// remove from cache
						for (int j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
							if (Interface.CurrentOptions.RouteEncodings[j].Value == Result.RouteFile) {
								Interface.CurrentOptions.RouteEncodings[j] = Interface.CurrentOptions.RouteEncodings[Interface.CurrentOptions.RouteEncodings.Length - 1];
								Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.RouteEncodings, Interface.CurrentOptions.RouteEncodings.Length - 1);
								break;
							}
						}
					} else {
						// add to cache
						int j; for (j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
							if (Interface.CurrentOptions.RouteEncodings[j].Value == Result.RouteFile) {
								Interface.CurrentOptions.RouteEncodings[j].Codepage = EncodingCodepages[i];
								break;
							}
						} if (j == Interface.CurrentOptions.RouteEncodings.Length) {
							Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.RouteEncodings, j + 1);
							Interface.CurrentOptions.RouteEncodings[j].Codepage = EncodingCodepages[i];
							Interface.CurrentOptions.RouteEncodings[j].Value = Result.RouteFile;
						}
					}
					ShowRoute(true);
				}
			}
		}
		private void buttonRouteEncodingLatin1_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 1252) {
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonRouteEncodingShiftJis_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 932) {
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonRouteEncodingBig5_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 950) {
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		// ===============
		// train selection
		// ===============

		// train folder
		private void textboxTrainFolder_TextChanged(object sender, EventArgs e) {
			string Folder = textboxTrainFolder.Text;
			try {
				if (Folder.Length == 0) {
					// drives
					listviewTrainFolders.Items.Clear();
					try { // MoMA says that GetDrives is flagged with [MonoTodo]
						System.IO.DriveInfo[] driveInfos = System.IO.DriveInfo.GetDrives();
						for (int i = 0; i < driveInfos.Length; i++) {
							ListViewItem Item = listviewTrainFolders.Items.Add(driveInfos[i].Name);
							Item.ImageKey = "folder";
							Item.Tag = driveInfos[i].RootDirectory.FullName;
							listviewTrainFolders.Tag = null;
						}
					} catch { }
				} else if (System.IO.Directory.Exists(Folder)) {
					listviewTrainFolders.Items.Clear();
					// parent
					try {
						System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
						if (Info != null) {
							ListViewItem Item = listviewTrainFolders.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = Info.FullName;
							listviewTrainFolders.Tag = Info.FullName;
						} else {
							ListViewItem Item = listviewTrainFolders.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = "";
							listviewTrainFolders.Tag = "";
						}
					} catch { }
					// folders
					try {
						string[] Folders = System.IO.Directory.GetDirectories(Folder);
						Array.Sort<string>(Folders);
						for (int i = 0; i < Folders.Length; i++) {
							try {
								System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(Folders[i]);
								if ((info.Attributes & System.IO.FileAttributes.Hidden) == 0) {
									string Name = System.IO.Path.GetFileName(Folders[i]);
									if (!string.IsNullOrEmpty(Name) && Name[0] != '.') {
										string File = OpenBveApi.Path.CombineFile(Folders[i], "train.dat");
										ListViewItem Item = listviewTrainFolders.Items.Add(Name);
										if (System.IO.File.Exists(File)) {
											Item.ImageKey = "train";
										} else {
											Item.ImageKey = "folder";
										}
										Item.Tag = Folders[i];
									}
								}
							} catch { }
						}
					} catch { }
				}
			} catch { }
			listviewTrainFolders.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		// train folders
		private void listviewTrainFolders_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewTrainFolders.SelectedItems.Count == 1) {
				string t = listviewTrainFolders.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.Directory.Exists(t)) {
						string File = OpenBveApi.Path.CombineFile(t, "train.dat");
						if (System.IO.File.Exists(File)) {
							Result.TrainFolder = t;
							ShowTrain(false);
							if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
						}
					}
				}
			}
		}
		private void listviewTrainFolders_DoubleClick(object sender, EventArgs e) {
			if (listviewTrainFolders.SelectedItems.Count == 1) {
				string t = listviewTrainFolders.SelectedItems[0].Tag as string;
				if (t != null) {
					if (t.Length == 0 || System.IO.Directory.Exists(t)) {
						textboxTrainFolder.Text = t;
					}
				}
			}
		}
		private void listviewTrainFolders_KeyDown(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Return:
					listviewTrainFolders_DoubleClick(null, null);
					break;
				case Keys.Back:
					string t = listviewTrainFolders.Tag as string;
					if (t != null) {
						if (t.Length == 0 || System.IO.Directory.Exists(t)) {
							textboxTrainFolder.Text = t;
						}
					} break;
			}
		}

		// train recently
		private void listviewTrainRecently_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewTrainRecently.SelectedItems.Count == 1) {
				string t = listviewTrainRecently.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.Directory.Exists(t)) {
						string File = OpenBveApi.Path.CombineFile(t, "train.dat");
						if (System.IO.File.Exists(File)) {
							Result.TrainFolder = t;
							ShowTrain(false);
							if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
						}
					}
				}
			}
		}

		// train default
		void CheckboxTrainDefaultCheckedChanged(object sender, System.EventArgs e) {
			if (checkboxTrainDefault.Checked) {
				if (listviewTrainFolders.SelectedItems.Count == 1) {
					listviewTrainFolders.SelectedItems[0].Selected = false;
				}
				if (listviewTrainRecently.SelectedItems.Count == 1) {
					listviewTrainRecently.SelectedItems[0].Selected = false;
				}
				ShowDefaultTrain();
			}
		}
		

		// =============
		// train details
		// =============

		// train image
		private void pictureboxTrainImage_Click(object sender, EventArgs e) {
			if (pictureboxTrainImage.Image != null) {
				formImage.ShowImageDialog(pictureboxTrainImage.Image);
			}
		}

		// train encoding
		private void comboboxTrainEncoding_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxTrainEncoding.Tag == null) {
				int i = comboboxTrainEncoding.SelectedIndex;
				if (i >= 0 & i < EncodingCodepages.Length) {
					Result.TrainEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[i]);
					if (i == 0) {
						// remove from cache
						for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (Interface.CurrentOptions.TrainEncodings[j].Value == Result.TrainFolder) {
								Interface.CurrentOptions.TrainEncodings[j] = Interface.CurrentOptions.TrainEncodings[Interface.CurrentOptions.TrainEncodings.Length - 1];
								Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.TrainEncodings, Interface.CurrentOptions.TrainEncodings.Length - 1);
								break;
							}
						}
					} else {
						// add to cache
						int j; for (j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (Interface.CurrentOptions.TrainEncodings[j].Value == Result.TrainFolder) {
								Interface.CurrentOptions.TrainEncodings[j].Codepage = EncodingCodepages[i];
								break;
							}
						} if (j == Interface.CurrentOptions.TrainEncodings.Length) {
							Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.TrainEncodings, j + 1);
							Interface.CurrentOptions.TrainEncodings[j].Codepage = EncodingCodepages[i];
							Interface.CurrentOptions.TrainEncodings[j].Value = Result.TrainFolder;
						}
					}
					ShowTrain(true);
				}
			}
		}
		private void buttonTrainEncodingLatin1_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 1252) {
					comboboxTrainEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonTrainEncodingShiftJis_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 932) {
					comboboxTrainEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonTrainEncodingBig5_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 950) {
					comboboxTrainEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		
		// =====
		// start
		// =====

		// start
		private void buttonStart_Click(object sender, EventArgs e) {
			if (Result.RouteFile != null & Result.TrainFolder != null) {
				if (System.IO.File.Exists(Result.RouteFile) & System.IO.Directory.Exists(Result.TrainFolder)) {
					Result.Start = true;
					this.Close();
				}
			} else {
				System.Media.SystemSounds.Exclamation.Play();
			}
		}
		
		
		// =========
		// functions
		// =========

		// show route
		private void ShowRoute(bool UserSelectedEncoding) {

			if (Result.RouteFile != null)
			{
			    
				this.Cursor = Cursors.WaitCursor;
				Application.DoEvents();
				// determine encoding
				if (!UserSelectedEncoding) {
					comboboxRouteEncoding.Tag = new object();
					comboboxRouteEncoding.SelectedIndex = 0;
					comboboxRouteEncoding.Items[0] = "(UTF-8)";
					comboboxRouteEncoding.Tag = null;
					Result.RouteEncoding = System.Text.Encoding.UTF8;
					switch (Interface.GetEncodingFromFile(Result.RouteFile)) {
						case Interface.Encoding.Utf8:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-8)";
							Result.RouteEncoding = System.Text.Encoding.UTF8;
							break;
						case Interface.Encoding.Utf16Le:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-16 little endian)";
							Result.RouteEncoding = System.Text.Encoding.Unicode;
							break;
						case Interface.Encoding.Utf16Be:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-16 big endian)";
							Result.RouteEncoding = System.Text.Encoding.BigEndianUnicode;
							break;
						case Interface.Encoding.Utf32Le:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-32 little endian)";
							Result.RouteEncoding = System.Text.Encoding.UTF32;
							break;
						case Interface.Encoding.Utf32Be:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-32 big endian)";
							Result.RouteEncoding = System.Text.Encoding.GetEncoding(12001);
							break;
					}
					panelRouteEncoding.Enabled = true;
					comboboxRouteEncoding.Tag = new object();
                    
					int i;
					for (i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++) {
						if (Interface.CurrentOptions.RouteEncodings[i].Value == Result.RouteFile) {
							int j;
							for (j = 1; j < EncodingCodepages.Length; j++) {
								if (EncodingCodepages[j] == Interface.CurrentOptions.RouteEncodings[i].Codepage) {
									comboboxRouteEncoding.SelectedIndex = j;
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[j]);
									break;
								}
							}
							if (j == EncodingCodepages.Length) {
								comboboxRouteEncoding.SelectedIndex = 0;
								Result.RouteEncoding = System.Text.Encoding.UTF8;
							}
							break;
						}
					}
					comboboxRouteEncoding.Tag = null;
				}
				// parse route
				try {
					Game.Reset(false);
					bool IsRW = string.Equals(System.IO.Path.GetExtension(Result.RouteFile), ".rw", StringComparison.OrdinalIgnoreCase);
					CsvRwRouteParser.ParseRoute(Result.RouteFile, IsRW, Result.RouteEncoding, null, null, null, true);
					pictureboxRouteMap.Image = Illustrations.CreateRouteMap(pictureboxRouteMap.Width, pictureboxRouteMap.Height);
					pictureboxRouteGradient.Image = Illustrations.CreateRouteGradientProfile(pictureboxRouteGradient.Width, pictureboxRouteGradient.Height);
					// image
					if (Game.RouteImage.Length != 0) {
						try {
							pictureboxRouteImage.Image = Image.FromFile(Game.RouteImage);
						} catch {
							TryLoadImage(pictureboxRouteImage, "route_error.png");
						}
					} else {
						//string f = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Result.RouteFile), System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile));
						string[] e = new string[] { ".png", ".bmp", ".gif", ".tiff", ".tif", ".jpeg", ".jpg" };
						int i;
						for (i = 0; i < e.Length; i++) {
							string g = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(Result.RouteFile), System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile) + e[i]);
							if (System.IO.File.Exists(g)) {
								try {
									pictureboxRouteImage.Image = Image.FromFile(g);
								} catch {
									pictureboxRouteImage.Image = null;
								}
								break;
							}
						}
						if (i == e.Length) {
							TryLoadImage(pictureboxRouteImage, "route_unknown.png");
						}
					}
                    
					// description
					string Description = Interface.ConvertNewlinesToCrLf(Game.RouteComment);
					if (Description.Length != 0) {
						textboxRouteDescription.Text = Description;
					} else {
						textboxRouteDescription.Text = System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile);
					}
					textboxRouteEncodingPreview.Text = Interface.ConvertNewlinesToCrLf(Description);
					if (Game.TrainName != null) {
						checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault") + " (" + Game.TrainName + ")";
					} else {
						checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault");
					}
                    
				} catch (Exception ex) {
					// error
					TryLoadImage(pictureboxRouteImage, "route_error.png");
					textboxRouteDescription.Text = ex.Message;
					textboxRouteEncodingPreview.Text = "";
					pictureboxRouteMap.Image = null;
					pictureboxRouteGradient.Image = null;
					Result.RouteFile = null;
					checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault");
				}
				groupboxRouteDetails.Visible = true;
                
				if (checkboxTrainDefault.Checked) {
					ShowDefaultTrain();
				}
			    
				this.Cursor = Cursors.Default;
               
				buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
			}
		}

		// show train
		private void ShowTrain(bool UserSelectedEncoding) {
			if (!UserSelectedEncoding) {
				comboboxTrainEncoding.Tag = new object();
				comboboxTrainEncoding.SelectedIndex = 0;
				comboboxTrainEncoding.Items[0] = "(UTF-8)";
				comboboxTrainEncoding.Tag = null;
				Result.TrainEncoding = System.Text.Encoding.UTF8;
				switch (Interface.GetEncodingFromFile(Result.TrainFolder, "train.txt")) {
					case Interface.Encoding.Utf8:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-8)";
						Result.TrainEncoding = System.Text.Encoding.UTF8;
						break;
					case Interface.Encoding.Utf16Le:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-16 little endian)";
						Result.TrainEncoding = System.Text.Encoding.Unicode;
						break;
					case Interface.Encoding.Utf16Be:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-16 big endian)";
						Result.TrainEncoding = System.Text.Encoding.BigEndianUnicode;
						break;
					case Interface.Encoding.Utf32Le:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-32 little endian)";
						Result.TrainEncoding = System.Text.Encoding.UTF32;
						break;
					case Interface.Encoding.Utf32Be:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-32 big endian)";
						Result.TrainEncoding = System.Text.Encoding.GetEncoding(12001);
						break;
				}
				int i;
				for (i = 0; i < Interface.CurrentOptions.TrainEncodings.Length; i++) {
					if (Interface.CurrentOptions.TrainEncodings[i].Value == Result.TrainFolder) {
						int j;
						for (j = 1; j < EncodingCodepages.Length; j++) {
							if (EncodingCodepages[j] == Interface.CurrentOptions.TrainEncodings[i].Codepage) {
								comboboxTrainEncoding.SelectedIndex = j;
								Result.TrainEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[j]);
								break;
							}
						}
						if (j == EncodingCodepages.Length) {
							comboboxTrainEncoding.SelectedIndex = 0;
							Result.TrainEncoding = System.Text.Encoding.UTF8;
						}
						break;
					}
				}
				panelTrainEncoding.Enabled = true;
				comboboxTrainEncoding.Tag = null;
			}
			{
				// train image
				string File = OpenBveApi.Path.CombineFile(Result.TrainFolder, "train.png");
				if (!System.IO.File.Exists(File)) {
					File = OpenBveApi.Path.CombineFile(Result.TrainFolder, "train.bmp");
				}
				if (System.IO.File.Exists(File)) {
					try {
						pictureboxTrainImage.Image = Image.FromFile(File);
					} catch {
						pictureboxTrainImage.Image = null;
						TryLoadImage(pictureboxTrainImage, "train_error.png");
					}
				} else {
					TryLoadImage(pictureboxTrainImage, "train_unknown.png");
				}
			}
			{
				// train description
				string File = OpenBveApi.Path.CombineFile(Result.TrainFolder, "train.txt");
				if (System.IO.File.Exists(File)) {
					try {
						string Text = System.IO.File.ReadAllText(File, Result.TrainEncoding);
						Text = Interface.ConvertNewlinesToCrLf(Text);
						textboxTrainDescription.Text = Text;
						textboxTrainEncodingPreview.Text = Text;
					} catch {
						textboxTrainDescription.Text = System.IO.Path.GetFileName(Result.TrainFolder);
						textboxTrainEncodingPreview.Text = "";
					}
				} else {
					textboxTrainDescription.Text = System.IO.Path.GetFileName(Result.TrainFolder);
					textboxTrainEncodingPreview.Text = "";
				}
			}
			groupboxTrainDetails.Visible = true;
			labelTrainEncoding.Enabled = true;
			labelTrainEncodingPreview.Enabled = true;
			textboxTrainEncodingPreview.Enabled = true;
			buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
		}

		// show default train
		private void ShowDefaultTrain() {
            
			if (string.IsNullOrEmpty(Result.RouteFile)) {
				return;
			}
			if (string.IsNullOrEmpty(Game.TrainName)) {
				return;
			}
            
			string Folder = null;
			try {
				Folder = System.IO.Path.GetDirectoryName(Result.RouteFile);
				if (Game.TrainName[0] == '$') {
					Folder = OpenBveApi.Path.CombineDirectory(Folder, Game.TrainName);
					if (System.IO.Directory.Exists(Folder)) {
						string File = OpenBveApi.Path.CombineFile(Folder, "train.dat");
						if (System.IO.File.Exists(File)) {
                            
							Result.TrainFolder = Folder;
							ShowTrain(false);
							return;
						}
					}
				}
			} catch {
				Folder = null;
			}
			try {
				while (true) {
					string TrainFolder = OpenBveApi.Path.CombineDirectory(Folder, "Train");
					if (System.IO.Directory.Exists(TrainFolder)) {
						try {
							Folder = OpenBveApi.Path.CombineDirectory(TrainFolder, Game.TrainName);
						} catch {
							Folder = null;
						}
						if (Folder != null && System.IO.Directory.Exists(Folder)) {
							string File = OpenBveApi.Path.CombineFile(Folder, "train.dat");
							if (System.IO.File.Exists(File)) {
								/// train found
								Result.TrainFolder = Folder;
								ShowTrain(false);
								return;
							}
						} break;
					} else {
						System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
						if (Info != null) {
							Folder = Info.FullName;
						} else {
							break;
						}
					}
				}
			} catch { }
			/// train not found
			Result.TrainFolder = null;
			TryLoadImage(pictureboxTrainImage, "train_error.png");
			textboxTrainDescription.Text = Interface.ConvertNewlinesToCrLf(Interface.GetInterfaceString("start_train_notfound") + Game.TrainName);
			comboboxTrainEncoding.Tag = new object();
			comboboxTrainEncoding.SelectedIndex = 0;
			comboboxTrainEncoding.Tag = null;
			labelTrainEncoding.Enabled = false;
			panelTrainEncoding.Enabled = false;
			labelTrainEncodingPreview.Enabled = false;
			textboxTrainEncodingPreview.Enabled = false;
			textboxTrainEncodingPreview.Text = "";
			groupboxTrainDetails.Visible = true;
		}

	}
}