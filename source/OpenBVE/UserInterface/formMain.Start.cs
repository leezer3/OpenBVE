using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LibRender2;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	internal partial class formMain
	{
		// ===============
		// route selection
		// ===============

		/// <summary>The current routefile search folder</summary>
		private string currentRouteFolder;
		private bool populateRouteOnce;
		private FileSystemWatcher routeWatcher;
		private FileSystemWatcher trainWatcher;
		//Separate strings for package browser, so we can go back to the previous location!
		private string currentRoutePackageFolder;
		private string currentTrainPackageFolder;

		private readonly Dictionary<string, string> compatibilitySignals = new Dictionary<string, string>();

		/// <summary>Generation counters used to invalidate stale background list population tasks.</summary>
		private int routeListGeneration;
		private int trainListGeneration;

		/// <summary>A single entry to be displayed in a list view.</summary>
		private class ListItemData
		{
			internal readonly string Name;
			internal readonly string ImageKey;
			internal readonly string Tag;
			internal readonly bool IsParent;
			/// <summary>Whether this entry belongs to the fixed (non-browsing) list view, e.g. the main route or train list.</summary>
			internal readonly bool UseFixedList;

			internal ListItemData(string name, string imageKey, string tag, bool isParent, bool useFixedList)
			{
				Name = name;
				ImageKey = imageKey;
				Tag = tag;
				IsParent = isParent;
				UseFixedList = useFixedList;
			}
		}

		private void LoadCompatibilitySignalSets()
		{
			string[] possibleFiles = Directory.GetFiles(Path.CombineDirectory(Program.FileSystem.GetDataFolder("Compatibility"), "Signals"), "*.xml");
			for (int i = 0; i < possibleFiles.Length; i++)
			{
				XmlDocument currentXML = new XmlDocument();
				try
				{
					currentXML.Load(possibleFiles[i]);
					XmlNode node = currentXML.SelectSingleNode("/openBVE/CompatibilitySignals/SignalSetName");
					if (node != null)
					{
						compatibilitySignals.Add(node.InnerText, possibleFiles[i]);
						comboBoxCompatibilitySignals.Items.Add(node.InnerText);
					}
				}
				catch
				{
					//Ignored
				}

			}
		}

		private void textboxRouteFolder_TextChanged(object sender, EventArgs e)
		{
			if (listviewRouteFiles.Columns.Count == 0 || Path.ContainsInvalidChars(textboxRouteFolder.Text))
			{
				return;
			}
			string Folder = textboxRouteFolder.Text;
			while (!Directory.Exists(Folder) && Path.IsPathRooted(Folder))
			{
				try
				{
					// ReSharper disable once PossibleNullReferenceException
					Folder = Directory.GetParent(Folder).ToString();
				}
				catch
				{
					// Can't get the root of \\ => https://github.com/leezer3/OpenBVE/issues/468
					// Probably safer overall too
					return;
				}
				
			}

			if (currentRouteFolder != Folder)
			{
				PopulateRouteList(Folder, listviewRouteFiles, false);
			}
			currentRouteFolder = Folder;
			try
			{
				if (Program.CurrentHost.Platform != HostPlatform.AppleOSX && !string.IsNullOrEmpty(Folder) && Folder.Length > 2)
				{
					//BUG: Mono's filesystem watcher can exceed the OS-X handles limit on some systems
					//Triggered by NWM which has 600+ files in the route folder
					routeWatcher = new FileSystemWatcher
					{
						Path = Folder, 
						NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName, 
						Filter = "*.*"
					};
					routeWatcher.Changed += OnRouteFolderChanged;
					routeWatcher.EnableRaisingEvents = true;
				}
			}
			catch
			{
				//Most likely some sort of permissions issue, only means we can't monitor for new files
			}
			if (listviewRouteFiles.Columns.Count > 0)
			{
				listviewRouteFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}
		
		private void OnRouteFolderChanged(object sender, EventArgs e)
		{
			//We need to invoke the control so we don't get a cross thread exception
			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker) delegate
				{
					OnRouteFolderChanged(this, e);
				});
				return;
			}
			if (populateRouteOnce)
			{
				/*
				 * If we attempt to browse to the temp folder, we'll likely get an infinite loop
				 * from FS changed events, unless we only populate the folder once
				 */
				if (currentRouteFolder == System.IO.Path.GetTempPath())
				{
					return;
				}
			}
			PopulateRouteList(currentRouteFolder, listviewRouteFiles, false);
			populateRouteOnce = currentRouteFolder == System.IO.Path.GetTempPath();
		}

		/// <summary>Populates the route display list from the selected folder</summary>
		/// <param name="routeFolder">The folder containing route files</param>
		/// <param name="listView">The list view to populate</param>
		/// <param name="packages">Whether this is a packaged content folder</param>
		private void PopulateRouteList(string routeFolder, ListView listView, bool packages)
		{
			int generation = ++routeListGeneration;
			try
			{
				//The directory scan and file sniffing can be slow on large installs,
				//so run it on a background thread and only update the UI when it is complete
				Task.Run(() =>
				{
					List<ListItemData> items = BuildRouteListItems(routeFolder, packages);
					PopulateListView(listView, listviewRouteFiles, items, generation, true, routeFolder.Length == 0);
				});
			}
			catch
			{
				//The background task could not be started
			}
		}

		/// <summary>Builds the route display entries for the selected folder. Runs on a background thread.</summary>
		/// <param name="routeFolder">The folder containing route files</param>
		/// <param name="packages">Whether this is a packaged content folder</param>
		/// <returns>The route entries to display</returns>
		private List<ListItemData> BuildRouteListItems(string routeFolder, bool packages)
		{
			List<ListItemData> items = new List<ListItemData>();
			try
			{
				if (routeFolder.Length == 0)
				{
					// drives
					DriveInfo[] driveInfos = DriveInfo.GetDrives();
					for (int i = 0; i < driveInfos.Length; i++)
					{
						items.Add(new ListItemData(driveInfos[i].Name, Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows ? @"disk" : @"folder", driveInfos[i].RootDirectory.FullName, false, false));
					}
				}
				else if (Directory.Exists(routeFolder))
				{
					if (!packages || routeFolder != Program.FileSystem.RouteInstallationDirectory)
					{
						// Show parent if applicable
						DirectoryInfo info = Directory.GetParent(routeFolder);
						items.Add(new ListItemData("..", @"parent", info != null ? info.FullName : "", true, false));
					}
					
					// folders
					string[] Folders = Directory.GetDirectories(routeFolder);
					Array.Sort(Folders);
					for (int i = 0; i < Folders.Length; i++)
					{
						DirectoryInfo info = new DirectoryInfo(Folders[i]);
						if ((info.Attributes & FileAttributes.Hidden) == 0)
						{
							string folderName = System.IO.Path.GetFileName(Folders[i]);
							if (!string.IsNullOrEmpty(folderName) && folderName[0] != '.')
							{
								items.Add(new ListItemData(folderName, @"folder", Folders[i], false, false));
							}
						}
					}
					// files
					string[] Files = Directory.GetFiles(routeFolder);
					Array.Sort(Files);
					for (int i = 0; i < Files.Length; i++)
					{
						string fileName;
						if (string.IsNullOrEmpty(Files[i])) break;
						string Extension = System.IO.Path.GetExtension(Files[i]).ToLowerInvariant();
						switch (Extension)
						{
							case ".rw":
							case ".csv":
								fileName = System.IO.Path.GetFileName(Files[i]);
								if (!string.IsNullOrEmpty(fileName) && fileName[0] != '.')
								{
									string imageKey;
									if (Extension == ".csv")
									{
										//Only sniff the start of the file rather than reading it entirely,
										//as the route keywords always appear at the top
										imageKey = SniffCsvRouteType(Files[i]) ? @"csvroute" : null;
									}
									else
									{
										imageKey = @"rwroute";
									}
									items.Add(new ListItemData(fileName, imageKey, Files[i], false, false));
								}
								break;
							case ".dat":
								fileName = System.IO.Path.GetFileName(Files[i]);
								if (fileName == null || Path.IsInvalidDatName(Files[i]))
								{
									continue;
								}

								if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
								{
									throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
								}
								for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
								{
									if (Program.CurrentHost.Plugins[j].Route != null && Program.CurrentHost.Plugins[j].Route.CanLoadRoute(Files[i]))
									{
										items.Add(new ListItemData(fileName, @"mechanik", Files[i], false, false));
									}
								}
								break;
							case ".txt":
								if (Path.IsInvalidTxtName(Files[i]))
								{
									continue;
								}
								if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
								{
									throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
								}
								for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
								{
									if (Program.CurrentHost.Plugins[j].Route != null && Program.CurrentHost.Plugins[j].Route.CanLoadRoute(Files[i]))
									{
										//BVE5 routes are always added to the main route list
										items.Add(new ListItemData(System.IO.Path.GetFileName(Files[i]), @"bve5", Files[i], false, true));
									}
								}
								break;
						}
					}
				}
			}
			catch
			{
				//Ignore all errors
			}
			return items;
		}

		/// <summary>Sniffs the start of a CSV file to determine whether it is a route rather than an object.</summary>
		/// <param name="file">The file to sniff</param>
		/// <returns>Whether the file appears to be a route</returns>
		private static bool SniffCsvRouteType(string file)
		{
			try
			{
				using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
				{
					char[] buffer = new char[65536];
					int charsRead = sr.ReadBlock(buffer, 0, buffer.Length);
					string text = new string(buffer, 0, charsRead);
					return text.IndexOf("With Track", StringComparison.OrdinalIgnoreCase) >= 0 |
						text.IndexOf("Track.", StringComparison.OrdinalIgnoreCase) >= 0 |
						text.IndexOf("$Include", StringComparison.OrdinalIgnoreCase) >= 0;
				}
			}
			catch
			{
				//Most likely because the file is locked
				return false;
			}
		}

		/// <summary>Applies the results of a background list population task to the user interface.</summary>
		/// <param name="listView">The list view to populate</param>
		/// <param name="fixedListView">The fixed (non-browsing) list view, if different</param>
		/// <param name="items">The entries to display</param>
		/// <param name="generation">The generation this task belongs to</param>
		/// <param name="isRoute">Whether this task populated the route list rather than the train list</param>
		/// <param name="clearTag">Whether the list view tag should be cleared (drives view)</param>
		private void PopulateListView(ListView listView, ListView fixedListView, List<ListItemData> items, int generation, bool isRoute, bool clearTag)
		{
			try
			{
				listView.BeginInvoke((MethodInvoker)delegate
				{
					try
					{
						int currentGeneration = isRoute ? routeListGeneration : trainListGeneration;
						if (generation != currentGeneration)
						{
							//A newer task has superseded this one
							return;
						}
						bool sameList = fixedListView == listView;
						listView.BeginUpdate();
						try
						{
							listView.Items.Clear();
							if (clearTag)
							{
								listView.Tag = null;
							}
							foreach (ListItemData item in items)
							{
								if (item.UseFixedList && !sameList)
								{
									continue;
								}
								ListViewItem listItem = listView.Items.Add(item.Name);
								listItem.ImageKey = item.ImageKey;
								listItem.Tag = item.Tag;
								if (item.IsParent)
								{
									listView.Tag = item.Tag;
								}
							}
						}
						finally
						{
							listView.EndUpdate();
						}
						if (!sameList)
						{
							fixedListView.BeginUpdate();
							try
							{
								foreach (ListItemData item in items)
								{
									if (!item.UseFixedList)
									{
										continue;
									}
									ListViewItem listItem = fixedListView.Items.Add(item.Name);
									listItem.ImageKey = item.ImageKey;
									listItem.Tag = item.Tag;
								}
							}
							finally
							{
								fixedListView.EndUpdate();
							}
						}
						if (listView.Columns.Count > 0)
						{
							listView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
						}
					}
					catch
					{
						//The form is closing or has been disposed
					}
				});
			}
			catch
			{
				//The form is closing or has been disposed
			}
		}
		
		// route files
		private void listviewRouteFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listviewRouteFiles.SelectedItems.Count == 1)
			{
				string t;
				try
				{
					t = listviewRouteFiles.SelectedItems[0].Tag as string;
				}
				catch
				{

					return;
				}
				if (t != null)
				{

					if (File.Exists(t))
					{
						Result.RouteFile = t;
						ShowRoute(false);
					}
					else
					{
						groupboxRouteDetails.Visible = false;
						buttonStart.Enabled = false;
					}
				}
			}
		}

		private void listviewRouteFiles_DoubleClick(object sender, EventArgs e)
		{
			if (listviewRouteFiles.SelectedItems.Count == 1)
			{
				if (listviewRouteFiles.SelectedItems[0].Tag is string t)
				{
					if (t.Length == 0 || Directory.Exists(t))
					{
						textboxRouteFolder.Text = t;
					}
				}
			}
		}

		private void listviewRouteFiles_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Return:
					listviewRouteFiles_DoubleClick(null, null);
					break;
				case Keys.Back:
					if (listviewRouteFiles.Tag is string t)
					{
						if (t.Length == 0 || Directory.Exists(t))
						{
							textboxRouteFolder.Text = t;
						}
					}
					break;
			}
		}

		private void listViewRoutePackages_DoubleClick(object sender, EventArgs e)
		{
			if (listViewRoutePackages.SelectedItems.Count == 1)
			{
				if (listViewRoutePackages.SelectedItems[0].Tag is string t)
				{
					if (t.Length == 0 || Directory.Exists(t))
					{
						currentRoutePackageFolder = t;
						PopulateRouteList(currentRoutePackageFolder, listViewRoutePackages, true);
					}
				}
			}
		}

		private void listViewRoutePackages_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewRoutePackages.SelectedItems.Count == 1)
			{
				string t;
				try
				{
					t = listViewRoutePackages.SelectedItems[0].Tag as string;
				}
				catch
				{

					return;
				}
				if (t != null)
				{

					if (File.Exists(t))
					{
						Result.RouteFile = t;
						ShowRoute(false);
					}
					else
					{
						groupboxRouteDetails.Visible = false;
						buttonStart.Enabled = false;
					}
				}
			}
		}

		private void tabcontrolRouteSelection_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabcontrolRouteSelection.SelectedIndex == 2)
			{
				PopulateRouteList(currentRoutePackageFolder, listViewRoutePackages, true);
			}
		}

		private void tabcontrolTrainSelection_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabcontrolTrainSelection.SelectedIndex == 3)
			{
				PopulateTrainList(currentTrainPackageFolder, listViewTrainPackages, true);
			}
		}

		// route recently
		private void listviewRouteRecently_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listviewRouteRecently.SelectedItems.Count == 1)
			{
				if (!(listviewRouteRecently.SelectedItems[0].Tag is string t)) return;
				if (!File.Exists(t)) return;
				Result.RouteFile = t;
				ShowRoute(false);
			}
		}

		// =============
		// route details
		// =============

		// route image
		private void pictureboxRouteImage_Click(object sender, EventArgs e)
		{
			if (pictureboxRouteImage.Image != null)
			{
				FormImage.ShowImageDialog(pictureboxRouteImage.Image);
			}
		}

		// route encoding
		private void comboboxRouteEncoding_SelectedIndexChanged(object sender, EventArgs e)
		{
			int i = comboboxRouteEncoding.SelectedIndex;

			if (Result.RouteFile == null && Result.ErrorFile != null)
			{
				//Workaround for the route worker thread
				Result.RouteFile = Result.ErrorFile;
			}
			if (comboboxRouteEncoding.Tag == null)
			{

				if (!(i >= 0 & i < EncodingCodepages.Length)) return;
				Result.RouteEncoding = Encoding.GetEncoding(EncodingCodepages[i]);
				if (i == 0)
				{
					// remove from cache
					for (int j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++)
					{
						if (Interface.CurrentOptions.RouteEncodings[j].Value == Result.RouteFile)
						{
							Interface.CurrentOptions.RouteEncodings[j] =
								Interface.CurrentOptions.RouteEncodings[Interface.CurrentOptions.RouteEncodings.Length - 1];
							Array.Resize(ref Interface.CurrentOptions.RouteEncodings,
								Interface.CurrentOptions.RouteEncodings.Length - 1);
							break;
						}
					}
				}
				else
				{
					// add to cache
					int j;
					for (j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++)
					{
						if (Interface.CurrentOptions.RouteEncodings[j].Value == Result.RouteFile)
						{
							Interface.CurrentOptions.RouteEncodings[j].Codepage = (TextEncoding.Encoding)EncodingCodepages[i];
							break;
						}
					}
					if (j == Interface.CurrentOptions.RouteEncodings.Length)
					{
						Array.Resize(ref Interface.CurrentOptions.RouteEncodings, j + 1);
						Interface.CurrentOptions.RouteEncodings[j].Codepage = (TextEncoding.Encoding)EncodingCodepages[i];
						Interface.CurrentOptions.RouteEncodings[j].Value = Result.RouteFile;
					}
				}
				ShowRoute(true);
			}
		}

		private void buttonRouteEncodingLatin1_Click(object sender, EventArgs e)
		{
			for (int i = 1; i < EncodingCodepages.Length; i++)
			{
				if (EncodingCodepages[i] == 1252)
				{
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		private void buttonRouteEncodingShiftJis_Click(object sender, EventArgs e)
		{
			for (int i = 1; i < EncodingCodepages.Length; i++)
			{
				if (EncodingCodepages[i] == 932)
				{
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		private void buttonRouteEncodingBig5_Click(object sender, EventArgs e)
		{
			for (int i = 1; i < EncodingCodepages.Length; i++)
			{
				if (EncodingCodepages[i] == 950)
				{
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		// ===============
		// train selection
		// ===============

		/// <summary>The current train search folder</summary>
		private string currentTrainFolder;
		private bool populateTrainOnce;

		private void textboxTrainFolder_TextChanged(object sender, EventArgs e)
		{
			if (listviewTrainFolders.Columns.Count == 0 || Path.ContainsInvalidChars(textboxTrainFolder.Text))
			{
				return;
			}
			string Folder = textboxTrainFolder.Text;
			while (!Directory.Exists(Folder) && Path.IsPathRooted(Folder) && Folder.Length > 2)
			{
				try
				{
					// ReSharper disable once PossibleNullReferenceException
					Folder = Directory.GetParent(Folder).ToString();
				}
				catch
				{
					// Can't get the root of \\ => https://github.com/leezer3/OpenBVE/issues/468
					// Probably safer overall too
					return;
				}
			}
			if (currentTrainFolder != Folder)
			{
				PopulateTrainList(Folder, listviewTrainFolders, false);
			}
			currentTrainFolder = Folder;
			try
			{
				if (Program.CurrentHost.Platform != HostPlatform.AppleOSX && !string.IsNullOrEmpty(Folder) && Folder.Length > 2)
				{
					trainWatcher = new FileSystemWatcher
					{
						Path = Folder, 
						NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName, 
						Filter = "*.*"
					};
					trainWatcher.Changed += OnTrainFolderChanged;
					trainWatcher.EnableRaisingEvents = true;
				}
			}
			catch
			{
				//Most likely some sort of permissions issue, only means we can't monitor for new files
			}
		}

		private void OnTrainFolderChanged(object sender, EventArgs e)
		{
			//We need to invoke the control so we don't get a cross thread exception
			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker) delegate
				{
					OnTrainFolderChanged(this, e);
				});
				return;
			}
			if (populateTrainOnce)
			{
				/*
				 * If we attempt to browse to the temp folder, we'll likely get an infinite loop
				 * from FS changed events, unless we only populate the folder once
				 */
				if (currentTrainFolder == System.IO.Path.GetTempPath())
				{
					return;
				}
			}
			PopulateTrainList(currentTrainFolder, listviewTrainFolders, false);
			populateTrainOnce = currentTrainFolder == System.IO.Path.GetTempPath();
		}

		/// <summary>Populates the train display list from the selected folder</summary>
		/// <param name="selectedFolder">The folder containing train folders</param>
		/// <param name="listView">The list view to populate</param>
		/// <param name="packages">Whether this is a packaged content folder</param>
		private void PopulateTrainList(string selectedFolder, ListView listView, bool packages)
		{
			int generation = ++trainListGeneration;
			try
			{
				//The directory scan can be slow on large installs,
				//so run it on a background thread and only update the UI when it is complete
				Task.Run(() =>
				{
					List<ListItemData> items = BuildTrainListItems(selectedFolder, packages);
					PopulateListView(listView, listviewTrainFolders, items, generation, false, selectedFolder.Length == 0);
				});
			}
			catch
			{
				//The background task could not be started
			}
		}

		/// <summary>Builds the train display entries for the selected folder. Runs on a background thread.</summary>
		/// <param name="selectedFolder">The folder containing train folders</param>
		/// <param name="packages">Whether this is a packaged content folder</param>
		/// <returns>The train entries to display</returns>
		private List<ListItemData> BuildTrainListItems(string selectedFolder, bool packages)
		{
			List<ListItemData> items = new List<ListItemData>();
			try
			{
				if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
				{
					return items;
				}
				if (selectedFolder.Length == 0)
				{
					// drives
					DriveInfo[] driveInfos = DriveInfo.GetDrives();
					for (int i = 0; i < driveInfos.Length; i++)
					{
						items.Add(new ListItemData(driveInfos[i].Name, Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows ? @"disk" : @"folder", driveInfos[i].RootDirectory.FullName, false, false));
					}
				}
				else if (Directory.Exists(selectedFolder))
				{
					if (!packages || selectedFolder != Program.FileSystem.TrainInstallationDirectory)
					{
						// parent
						DirectoryInfo info = Directory.GetParent(selectedFolder);
						items.Add(new ListItemData("..", @"parent", info != null ? info.FullName : "", true, false));
					}
					
					// folders
					string[] Folders = Directory.GetDirectories(selectedFolder);
					string[] Files = Directory.GetFiles(selectedFolder);
					Array.Sort(Folders);
					Array.Sort(Files);
					for (int i = 0; i < Folders.Length; i++)
					{
						DirectoryInfo info = new DirectoryInfo(Folders[i]);
						if ((info.Attributes & FileAttributes.Hidden) == 0)
						{
							string folderName = System.IO.Path.GetFileName(Folders[i]);
							if (!string.IsNullOrEmpty(folderName) && folderName[0] != '.')
							{
								string File = Path.CombineFile(Folders[i], "train.dat");
								if (!System.IO.File.Exists(File))
								{
									File = Path.CombineFile(Folders[i], "train.xml");
								}
								items.Add(new ListItemData(folderName, System.IO.File.Exists(File) ? "train" : "folder", Folders[i], false, false));
							}
						}
					}

					for (int i = 0; i < Files.Length; i++)
					{
						for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
						{
							string fileName = Path.GetFileName(Files[i]);
							if (fileName[0] == '#' && fileName.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
							{
								// MSTS / ORTS use a hash at the start of the filename to deliminate AI consists
								// These generally have missing cabviews etc- Hide from visibility in the main menu
								continue;
							}
							if (Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(Files[i]))
							{
								//Consists are always added to the main train list
								items.Add(new ListItemData(System.IO.Path.GetFileName(Files[i]), @"msts", Files[i], false, true));
							}
						}
					}
				}
			}
			catch
			{
				//Ignore all errors
			}
			return items;
		}

		// train folders
		private void listviewTrainFolders_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listviewTrainFolders.SelectedItems.Count == 1)
			{
				string t;
				try
				{
					t = listviewTrainFolders.SelectedItems[0].Tag as string;
				}
				catch (Exception)
				{
					return;
				}
				if (t != null) {
					if (t.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
					{
						Result.TrainFolder = t;
						ShowTrain(false);
					}
					else if (Directory.Exists(t)) 
					{
						try
						{
							string File = Path.CombineFile(t, "train.dat");
							if (!System.IO.File.Exists(File))
							{
								File = Path.CombineFile(t, "train.xml");
							}
							
							if (System.IO.File.Exists(File))
							{
								Result.TrainFolder = t;
								ShowTrain(false);
								if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
							}
							else
							{
								lock (previewLock)
								{
									groupboxTrainDetails.Visible = false;
									buttonStart.Enabled = false;
								}
							}
						}
						catch
						{
							//Ignored
						}
					}
				}
			}
		}
		private void listviewTrainFolders_DoubleClick(object sender, EventArgs e)
		{
			if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
			{
				throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
			}
			if (listviewTrainFolders.SelectedItems.Count == 1) {
				if (listviewTrainFolders.SelectedItems[0].Tag is string t) {
					if (t.Length == 0)
					{
						//Pop up to parent directory
						textboxTrainFolder.Text = t;
						return;
					}
					if (Directory.Exists(t))
					{
						string[] newDirectories = Directory.EnumerateDirectories(t).ToArray();
						bool shouldEnter = true;
						for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
						{
							if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(t))
							{
								shouldEnter = false;
								break;
							}
						}
						if (shouldEnter || newDirectories.Length > 5)
						{
							/*
							 * Either a train folder with more than 5 subdirs (false positive?)
							 * Or a plain folder
							 */
							textboxTrainFolder.Text = t;
							return;
						}

						foreach (string dir in newDirectories)
						{
							for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
							{
								if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(dir))
								{
									textboxTrainFolder.Text = t;
									return;
								}
							}
						}
						string[] splitPath = t.Split('\\', '/');
						if (splitPath.Length < 3)
						{
							//If we're on less than the 3rd level subdir assume it may be a false positive
							textboxTrainFolder.Text = t;
						}
					}
				}
			}
		}

		private void listViewTrainPackages_DoubleClick(object sender, EventArgs e)
		{
			if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
			{
				throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
			}
			if (listViewTrainPackages.SelectedItems.Count == 1) {
				if (listViewTrainPackages.SelectedItems[0].Tag is string t) {
					if (t.Length == 0)
					{
						//Pop up to parent directory
						currentTrainPackageFolder = t;
						PopulateTrainList(currentTrainPackageFolder, listViewTrainPackages, true);
						return;
					}
					if (Directory.Exists(t))
					{
						string[] newDirectories = Directory.EnumerateDirectories(t).ToArray();
						bool shouldEnter = true;
						for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
						{
							if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(t))
							{
								shouldEnter = false;
								break;
							}
						}
						if (shouldEnter || newDirectories.Length > 5)
						{
							/*
							 * Either a train folder with more than 5 subdirs (false positive?)
							 * Or a plain folder
							 */
							currentTrainPackageFolder = t;
							PopulateTrainList(currentTrainPackageFolder, listViewTrainPackages, true);
							return;
						}

						foreach (string dir in newDirectories)
						{
							for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
							{
								if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(dir))
								{
									currentTrainPackageFolder = t;
									PopulateTrainList(currentTrainPackageFolder, listViewTrainPackages, true);
									return;
								}
							}
						}
						string[] splitPath = t.Split('\\', '/');
						if (splitPath.Length < 3)
						{
							//If we're on less than the 3rd level subdir assume it may be a false positive
							currentTrainPackageFolder = t;
							PopulateTrainList(currentTrainPackageFolder, listViewTrainPackages, true);
						}
					}
				}
			}
		}

		private void listViewTrainPackages_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewTrainPackages.SelectedItems.Count == 1)
			{
				string t;
				try
				{
					t = listViewTrainPackages.SelectedItems[0].Tag as string;
				}
				catch (Exception)
				{
					return;
				}
				if (t != null) {
					if (Directory.Exists(t)) {
						try
						{
							string File = Path.CombineFile(t, "train.dat");
							if (!System.IO.File.Exists(File))
							{
								File = Path.CombineFile(t, "train.xml");
							}
							if (System.IO.File.Exists(File))
							{
								Result.TrainFolder = t;
								ShowTrain(false);
								if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
							}
							else
							{
								lock (previewLock)
								{
									groupboxTrainDetails.Visible = false;
									buttonStart.Enabled = false;
								}
							}
						}
						catch
						{
							//Ignored
						}
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
					if (listviewTrainFolders.Tag is string t) {
						if (t.Length == 0 || Directory.Exists(t)) {
							textboxTrainFolder.Text = t;
						}
					} break;
			}
		}

		// train recently
		private void listviewTrainRecently_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewTrainRecently.SelectedItems.Count == 1) {
				if (!(listviewTrainRecently.SelectedItems[0].Tag is string t) || !Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
				{
					return;
				}

				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(t))
					{
						Result.TrainFolder = t;
						ShowTrain(false);
						if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
						return;
					}
				}
				// No plugin reports able to load the selected train
				groupboxTrainDetails.Visible = false;
				buttonStart.Enabled = false;
			}
		}

		// train default
		private void CheckboxTrainDefaultCheckedChanged(object sender, EventArgs e) {
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
				FormImage.ShowImageDialog(pictureboxTrainImage.Image);
			}
		}

		// train encoding
		private void comboboxTrainEncoding_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxTrainEncoding.Tag == null) {
				int i = comboboxTrainEncoding.SelectedIndex;
				if (i >= 0 & i < EncodingCodepages.Length) {
					Result.TrainEncoding = Encoding.GetEncoding(EncodingCodepages[i]);
					if (i == 0) {
						// remove from cache
						for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (Interface.CurrentOptions.TrainEncodings[j].Value == Result.TrainFolder) {
								Interface.CurrentOptions.TrainEncodings[j] = Interface.CurrentOptions.TrainEncodings[Interface.CurrentOptions.TrainEncodings.Length - 1];
								Array.Resize(ref Interface.CurrentOptions.TrainEncodings, Interface.CurrentOptions.TrainEncodings.Length - 1);
								break;
							}
						}
					} else {
						// add to cache
						int j; for (j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (Interface.CurrentOptions.TrainEncodings[j].Value == Result.TrainFolder) {
								Interface.CurrentOptions.TrainEncodings[j].Codepage = (TextEncoding.Encoding)EncodingCodepages[i];
								break;
							}
						} if (j == Interface.CurrentOptions.TrainEncodings.Length) {
							Array.Resize(ref Interface.CurrentOptions.TrainEncodings, j + 1);
							Interface.CurrentOptions.TrainEncodings[j].Codepage = (TextEncoding.Encoding)EncodingCodepages[i];
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
		private readonly object StartGame = new object();

		private void buttonStart_Click(object sender, EventArgs e) {
			if (Result.RouteFile != null & Result.TrainFolder != null)
			{
				bool canLoadRoute = false, canLoadTrain = false;
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (canLoadRoute == false)
					{
						if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(Result.RouteFile))
						{
							canLoadRoute = true;
						}
					}
					if (canLoadTrain == false)
					{
						if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(Result.TrainFolder))
						{
							canLoadTrain = true;
						}
					}
				}

				if (canLoadRoute && canLoadTrain)
				{
					Result.Start = true;
					buttonClose_Click(StartGame, e);
					//HACK: Call Application.DoEvents() to force the message pump to process all pending messages when the form closes
					//This fixes the main form failing to close on Linux
					Application.DoEvents();
				}
			} else {
				System.Media.SystemSounds.Exclamation.Play();
			}
		}
		
		
		// =========
		// functions
		// =========

		private BlockingCollection<LaunchParameters> previewRouteResultQueue;
		private CancellationTokenSource previewRouteCancelTokenSource;
		private Task previewRouteTask;
		private bool previewRouteIsLoading;

		private void InitializePreviewRouteThread()
		{
			previewRouteResultQueue = new BlockingCollection<LaunchParameters>();
			previewRouteCancelTokenSource = new CancellationTokenSource();
			previewRouteTask = Task.Run(() => PreviewRoute(previewRouteCancelTokenSource.Token));
		}

		private void PreviewRoute(CancellationToken cancelToken)
		{
			while (!previewRouteResultQueue.IsCompleted)
			{
				// Wait for a item to be put into the queue.
				LaunchParameters result;
				try
				{
					bool takeSuccess = previewRouteResultQueue.TryTake(out result, Timeout.Infinite, cancelToken);

					if (!takeSuccess)
					{
						continue;
					}
				}
				catch (OperationCanceledException)
				{
					// Preview thread has been cancelled.
					return;
				}

				// Gets the last item put into the queue at this time.
				{
					while (previewRouteResultQueue.TryTake(out LaunchParameters tmp))
					{
						result = tmp;
					}
					// The queue is empty.
				}

				// Load the route.
				Exception ex = null;
				previewRouteIsLoading = true;
				try
				{
					PreviewLoadRoute(result);
				}
				catch (Exception e)
				{
					ex = e;
				}
				

				// If it is not canceled during loading, show the route information.
				if (!Loading.Cancel)
				{
					Invoke((MethodInvoker) delegate
					{
						ShowRouteInformation(ex);
					});
				}
				else
				{
					Loading.Cancel = false;
				}

				previewRouteIsLoading = false;
			}
		}

		private void PreviewLoadRoute(LaunchParameters result)
		{
			if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
			{
				throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
			}

			Game.Reset(false);

			// ReSharper disable once AssignNullToNotNullAttribute - Already checked when loading plugins
			RouteInterface routeInterface = Program.CurrentHost.Plugins.Select(x => x.Route).FirstOrDefault(x => x != null && x.CanLoadRoute(result.RouteFile));

			if (routeInterface == null)
			{
				throw new Exception($"No plugins capable of loading route file {result.RouteFile} were found.");
			}

			// ReSharper disable once RedundantCast
			object route = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
			string railwayFolder = Program.FileSystem.GetRailwayFolder(result.RouteFile, Application.StartupPath);
			string objectFolder = Path.CombineDirectory(railwayFolder, "Object");
			string soundFolder = Path.CombineDirectory(railwayFolder, "Sound");

			if (routeInterface.LoadRoute(result.RouteFile, result.RouteEncoding, null, objectFolder, soundFolder, true, ref route))
			{
				Program.CurrentRoute = (CurrentRoute)route;
				return;
			}

			if (routeInterface.LastException != null)
			{
				throw routeInterface.LastException; //Re-throw last exception generated by the route parser plugin so that the UI thread captures it
			}

			throw new Exception($"An unknown error was encountered whilst attempting to parser the route file {result.RouteFile}");
		}

		/// <summary>Lock to be held when the route / train details are being updated</summary>
		private readonly object previewLock = new object();

		private void ShowRouteInformation(Exception e)
		{
			lock (previewLock)
			{
				try
				{
					if (e != null)
					{
						throw e;
					}

					lock (BaseRenderer.GdiPlusLock)
					{
						pictureboxRouteMap.Image = Illustrations.CreateRouteMap(pictureboxRouteMap.Width, pictureboxRouteMap.Height, false, out _);
						pictureboxRouteGradient.Image = Illustrations.CreateRouteGradientProfile(pictureboxRouteGradient.Width,
							pictureboxRouteGradient.Height, false);
					}

					// image
					if (!string.IsNullOrEmpty(Program.CurrentRoute.Image))
					{
						TryLoadImage(pictureboxRouteImage, Program.CurrentRoute.Image);
					}
					else
					{
						string[] f = { ".png", ".bmp", ".gif", ".tiff", ".tif", ".jpeg", ".jpg" };
						int i;
						for (i = 0; i < f.Length; i++)
						{
							string g = Path.CombineFile(Path.GetDirectoryName(Result.RouteFile),
								System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile) + f[i]);
							if (File.Exists(g))
							{
								try
								{
									using (var fs = new FileStream(g, FileMode.Open, FileAccess.Read))
									{
										pictureboxRouteImage.Image = new Bitmap(fs);
									}
								}
								catch
								{
									pictureboxRouteImage.Image = null;
								}

								break;
							}
						}

						if (i == f.Length)
						{
							TryLoadImage(pictureboxRouteImage, "route_unknown.png");
						}
					}

					// description
					string routeDescription = Program.CurrentRoute.Comment.ConvertNewlinesToCrLf();
					textboxRouteDescription.Text = routeDescription.Length != 0 ? routeDescription : System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile);
					textboxRouteEncodingPreview.Text = routeDescription.ConvertNewlinesToCrLf();
					if (Interface.CurrentOptions.TrainName != null)
					{
						checkboxTrainDefault.Text = $@"{Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_usedefault"})} ({Interface.CurrentOptions.TrainName})";
					}
					else
					{
						checkboxTrainDefault.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_usedefault"});
					}

					Result.ErrorFile = null;
				}
				catch (Exception ex)
				{
					TryLoadImage(pictureboxRouteImage, "route_error.png");
					textboxRouteDescription.Text = ex.Message;
					textboxRouteEncodingPreview.Text = "";
					pictureboxRouteMap.Image = null;
					pictureboxRouteGradient.Image = null;
					Result.ErrorFile = Result.RouteFile;
					Result.RouteFile = null;
					checkboxTrainDefault.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_usedefault"});
				}

				if (checkboxTrainDefault.Checked)
				{
					ShowDefaultTrain();
				}

				Cursor = System.Windows.Forms.Cursors.Default;
				//Deliberately select the tab when the process is complete
				//This hopefully fixes another instance of the 'grey tabs' bug

				tabcontrolRouteDetails.SelectedTab = tabpageRouteDescription;

				buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
			}
		}

		internal void DisposePreviewRouteThread()
		{
			if (previewRouteIsLoading)
			{
				Loading.Cancel = true;
			}

			previewRouteResultQueue?.CompleteAdding();
			previewRouteCancelTokenSource?.Cancel();

			if (previewRouteTask != null && !previewRouteTask.IsCompleted)
			{
				Console.WriteLine(@"Wait for Preview Thread to finish...");
				previewRouteTask.Wait();
				previewRouteTask = null;
				Console.WriteLine(@"Preview Thread finished.");
			}

			previewRouteResultQueue?.Dispose();
			previewRouteCancelTokenSource?.Dispose();

			previewRouteResultQueue = null;
			previewRouteCancelTokenSource = null;
		}


		// show route
		private void ShowRoute(bool userSelectedEncoding)
		{
			Cursor = System.Windows.Forms.Cursors.WaitCursor;
			TryLoadImage(pictureboxRouteImage, "loading.png");
			groupboxRouteDetails.Visible = true;
			textboxRouteDescription.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_processing"});

			// determine encoding
			if (!userSelectedEncoding)
			{
				Result.RouteEncoding = TextEncoding.GetSystemEncodingFromFile(Result.RouteFile);
				comboboxRouteEncoding.Tag = new object();
				comboboxRouteEncoding.SelectedIndex = 0;
				comboboxRouteEncoding.Items[0] = $"{Result.RouteEncoding.EncodingName} - {Result.RouteEncoding.CodePage}";
				comboboxRouteEncoding.Tag = null;

				comboboxRouteEncoding.Tag = new object();
				int i;
				for (i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++)
				{
					if (Interface.CurrentOptions.RouteEncodings[i].Value == Result.RouteFile)
					{
						int j;
						for (j = 1; j < EncodingCodepages.Length; j++)
						{
							if ((TextEncoding.Encoding)EncodingCodepages[j] == Interface.CurrentOptions.RouteEncodings[i].Codepage)
							{
								comboboxRouteEncoding.SelectedIndex = j;
								Result.RouteEncoding = Encoding.GetEncoding(EncodingCodepages[j]);
								break;
							}
						}

						if (j == EncodingCodepages.Length)
						{
							comboboxRouteEncoding.SelectedIndex = 0;
							Result.RouteEncoding = Encoding.UTF8;
						}

						break;
					}
				}

				comboboxRouteEncoding.Tag = null;
			}

			if (previewRouteIsLoading)
			{
				Loading.Cancel = true;
			}

			previewRouteResultQueue.Add(Result);
		}

		// show train
		private void ShowTrain(bool userSelectedEncoding)
		{
			lock (previewLock)
			{
				if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out _, Program.TrainManager, Program.Renderer))
				{
					throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
				}

				bool canLoad = false;
				string trainImage = string.Empty;
				// ReSharper disable once PossibleNullReferenceException - Already checked when loading plugins
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(Result.TrainFolder))
					{
						canLoad = true;
						trainImage = Program.CurrentHost.Plugins[i].Train.GetImage(Result.TrainFolder);
						if (!userSelectedEncoding)
						{
							string descriptionFile = Path.CombineFile(Result.TrainFolder, "train.txt");
							if (!File.Exists(descriptionFile))
							{
								descriptionFile = Path.CombineFile(Result.TrainFolder, "readme.txt");
							}

							if (!File.Exists(descriptionFile))
							{
								descriptionFile = Path.CombineFile(Result.TrainFolder, "read me.txt");
							}
							Result.TrainEncoding = TextEncoding.GetSystemEncodingFromFile(descriptionFile);
							comboboxTrainEncoding.Tag = new object();
							comboboxTrainEncoding.SelectedIndex = 0;
							comboboxTrainEncoding.Items[0] = $"{Result.TrainEncoding.EncodingName} - {Result.TrainEncoding.CodePage}";

							comboboxTrainEncoding.Tag = null;
							for (int k = 0; k < Interface.CurrentOptions.TrainEncodings.Length; k++)
							{
								if (Interface.CurrentOptions.TrainEncodings[k].Value == Result.TrainFolder)
								{
									int j;
									for (j = 1; j < EncodingCodepages.Length; j++)
									{
										if ((TextEncoding.Encoding)EncodingCodepages[j] == Interface.CurrentOptions.TrainEncodings[k].Codepage)
										{
											comboboxTrainEncoding.SelectedIndex = j;
											Result.TrainEncoding = Encoding.GetEncoding(EncodingCodepages[j]);
											break;
										}
									}

									if (j == EncodingCodepages.Length)
									{
										comboboxTrainEncoding.SelectedIndex = 0;
										Result.TrainEncoding = Encoding.UTF8;
									}

									break;
								}
							}

							panelTrainEncoding.Enabled = true;
							comboboxTrainEncoding.Tag = null;
						}

						textboxTrainDescription.Text = Program.CurrentHost.Plugins[i].Train.GetDescription(Result.TrainFolder, Result.TrainEncoding);
						break;
					}
				}

				if (!canLoad)
				{
					groupboxTrainDetails.Visible = false;
					buttonStart.Enabled = false;
					//No plugin capable of loading train found
					return;
				}

				
				if (!string.IsNullOrEmpty(trainImage))
				{
					try
					{
						Image image = Image.FromFile(trainImage);
						pictureboxTrainImage.Image = image;
						pictureboxTrainImage.Enabled = true;
					}
					catch
					{
						TryLoadImage(pictureboxTrainImage, "train_unknown.png");
						pictureboxTrainImage.Enabled = false;
					}
				}
				else
				{
					TryLoadImage(pictureboxTrainImage, "train_unknown.png");
					pictureboxTrainImage.Enabled = false;
				}

				groupboxTrainDetails.Visible = true;
				labelTrainEncoding.Enabled = true;
				labelTrainEncodingPreview.Enabled = true;
				textboxTrainEncodingPreview.Enabled = true;
				buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
			}
		}

		// show default train
		private void ShowDefaultTrain() {
			string trainFolder = Loading.GetDefaultTrainFolder(Result.RouteFile);
			if (!string.IsNullOrEmpty(trainFolder))
			{
				Result.TrainFolder = trainFolder;
				ShowTrain(false);
				return;
			}
			// train not found
			Result.TrainFolder = null;
			lock (previewLock)
			{
				if (!string.IsNullOrEmpty(Interface.CurrentOptions.TrainDownloadLocation))
				{
					string s = (Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "start", "train_notfound_download" }) + Interface.CurrentOptions.TrainDownloadLocation).ConvertNewlinesToCrLf();
					s = s.Replace("[train]", Interface.CurrentOptions.TrainName);
					textboxTrainDescription.Text = s;
					TryLoadImage(pictureboxTrainImage, "train_link.png");
				}
				else
				{
					textboxTrainDescription.Text = (Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "start", "train_notfound" }) + Interface.CurrentOptions.TrainName).ConvertNewlinesToCrLf();
					TryLoadImage(pictureboxTrainImage, "train_error.png");
				}

				
				comboboxTrainEncoding.Tag = new object();
				comboboxTrainEncoding.SelectedIndex = 0;
				comboboxTrainEncoding.Tag = null;
				labelTrainEncoding.Enabled = false;
				panelTrainEncoding.Enabled = false;
				labelTrainEncodingPreview.Enabled = false;
				textboxTrainEncodingPreview.Enabled = false;
				textboxTrainEncodingPreview.Text = "";
				groupboxTrainDetails.Visible = Result.RouteFile != null;
			}
		}

	}
}
