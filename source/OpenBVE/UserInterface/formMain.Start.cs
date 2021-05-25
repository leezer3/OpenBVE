using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using LibRender2;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
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
		private FileSystemWatcher routeWatcher;
		private FileSystemWatcher trainWatcher;

		private readonly Dictionary<string, string> compatibilitySignals = new Dictionary<string, string>();

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
			while (!Directory.Exists(Folder) && System.IO.Path.IsPathRooted(Folder))
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
				populateRouteList(Folder);
			}
			currentRouteFolder = Folder;
			try
			{
				if (Program.CurrentHost.Platform != HostPlatform.AppleOSX && !String.IsNullOrEmpty(Folder) && Folder.Length > 2)
				{
					//BUG: Mono's filesystem watcher can exceed the OS-X handles limit on some systems
					//Triggered by NWM which has 600+ files in the route folder
					routeWatcher = new FileSystemWatcher
					{
						Path = Folder, 
						NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName, 
						Filter = "*.*"
					};
					routeWatcher.Changed += onRouteFolderChanged;
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

		private void onRouteFolderChanged(object sender, EventArgs e)
		{
			//We need to invoke the control so we don't get a cross thread exception
			if (this.InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker) delegate
				{
					onRouteFolderChanged(this, e);
				});
				return;
			}
			populateRouteList(currentRouteFolder);
			//If this method is triggered whilst the form is disposing, bad things happen...
			if (listviewRouteFiles.Columns.Count > 0)
			{
				listviewRouteFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		/// <summary>Populates the route display list from the selected folder</summary>
		/// <param name="Folder">The folder containing route files</param>
		private void populateRouteList(string Folder)
		{
			try
			{
				if (Folder.Length == 0)
				{
					// drives
					listviewRouteFiles.Items.Clear();
					try
					{
						DriveInfo[] driveInfos = DriveInfo.GetDrives();
						for (int i = 0; i < driveInfos.Length; i++)
						{
							ListViewItem Item = listviewRouteFiles.Items.Add(driveInfos[i].Name);
							Item.ImageKey = Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows ? @"disk" : @"folder";
							Item.Tag = driveInfos[i].RootDirectory.FullName;
							listviewRouteFiles.Tag = null;
						}
					}
					catch
					{
						//Unable to get list of drives
					}
				}
				else if (Directory.Exists(Folder))
				{
					listviewRouteFiles.Items.Clear();
					// parent
					try
					{
						DirectoryInfo Info = Directory.GetParent(Folder);
						if (Info != null)
						{
							ListViewItem Item = listviewRouteFiles.Items.Add("..");
							Item.ImageKey = @"parent";
							Item.Tag = Info.FullName;
							listviewRouteFiles.Tag = Info.FullName;
						}
						else
						{
							ListViewItem Item = listviewRouteFiles.Items.Add("..");
							Item.ImageKey = @"parent";
							Item.Tag = "";
							listviewRouteFiles.Tag = "";
						}
					}
					catch
					{
						//Another permissions issue??
					}
					// folders
					try
					{
						string[] Folders = Directory.GetDirectories(Folder);
						Array.Sort(Folders);
						for (int i = 0; i < Folders.Length; i++)
						{
							DirectoryInfo info = new DirectoryInfo(Folders[i]);
							if ((info.Attributes & FileAttributes.Hidden) == 0)
							{
								string folderName = System.IO.Path.GetFileName(Folders[i]);
								if (!string.IsNullOrEmpty(folderName) && folderName[0] != '.')
								{
									ListViewItem Item = listviewRouteFiles.Items.Add(folderName);
									Item.ImageKey = @"folder";
									Item.Tag = Folders[i];
								}
							}
						}
					}
					catch
					{
						//Another permissions issue??
					}
					// files
					try
					{
						string[] Files = Directory.GetFiles(Folder);
						Array.Sort(Files);
						for (int i = 0; i < Files.Length; i++)
						{ 
							if (string.IsNullOrEmpty(Files[i])) return;
							string Extension = System.IO.Path.GetExtension(Files[i]).ToLowerInvariant();
							switch (Extension)
							{
								case ".rw":
								case ".csv":
									string fileName = System.IO.Path.GetFileName(Files[i]);
									if (!string.IsNullOrEmpty(fileName) && fileName[0] != '.')
									{
										ListViewItem Item = listviewRouteFiles.Items.Add(fileName);
										if (Extension == ".csv")
										{
											try
											{
												using (StreamReader sr = new StreamReader(Files[i], Encoding.UTF8))
												{
													string text = sr.ReadToEnd();
													if (text.IndexOf("With Track", StringComparison.OrdinalIgnoreCase) >= 0 |
													text.IndexOf("Track.", StringComparison.OrdinalIgnoreCase) >= 0 |
													text.IndexOf("$Include", StringComparison.OrdinalIgnoreCase) >= 0)
													{
														Item.ImageKey = @"route";
													}
												}
												
											}
											catch
											{
												//Most likely because the file is locked
											}
										}
										else
										{
											Item.ImageKey = @"route";
										}
										Item.Tag = Files[i];
									}
									break;
							}
						}
					}
					catch
					{
						//Ignore all errors
					}
				}
			}
			catch
			{
				//Ignore all errors
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
				string t = listviewRouteFiles.SelectedItems[0].Tag as string;
				if (t != null)
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
					string t = listviewRouteFiles.Tag as string;
					if (t != null)
					{
						if (t.Length == 0 || Directory.Exists(t))
						{
							textboxRouteFolder.Text = t;
						}
					}
					break;
			}
		}

		// route recently
		private void listviewRouteRecently_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listviewRouteRecently.SelectedItems.Count == 1)
			{
				string t = listviewRouteRecently.SelectedItems[0].Tag as string;
				if (t == null) return;
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
				formImage.ShowImageDialog(pictureboxRouteImage.Image);
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
							Interface.CurrentOptions.RouteEncodings[j].Codepage = EncodingCodepages[i];
							break;
						}
					}
					if (j == Interface.CurrentOptions.RouteEncodings.Length)
					{
						Array.Resize(ref Interface.CurrentOptions.RouteEncodings, j + 1);
						Interface.CurrentOptions.RouteEncodings[j].Codepage = EncodingCodepages[i];
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

		private void textboxTrainFolder_TextChanged(object sender, EventArgs e)
		{
			if (listviewTrainFolders.Columns.Count == 0 || Path.ContainsInvalidChars(textboxTrainFolder.Text))
			{
				return;
			}
			string Folder = textboxTrainFolder.Text;
			while (!Directory.Exists(Folder) && System.IO.Path.IsPathRooted(Folder) && Folder.Length > 2)
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
				populateTrainList(Folder);
			}
			currentTrainFolder = Folder;
			try
			{
				if (Program.CurrentHost.Platform != HostPlatform.AppleOSX && !String.IsNullOrEmpty(Folder) && Folder.Length > 2)
				{
					trainWatcher = new FileSystemWatcher
					{
						Path = Folder, 
						NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName, 
						Filter = "*.*"
					};
					trainWatcher.Changed += onTrainFolderChanged;
					trainWatcher.EnableRaisingEvents = true;
				}
			}
			catch
			{
				//Most likely some sort of permissions issue, only means we can't monitor for new files
			}
			listviewTrainFolders.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void onTrainFolderChanged(object sender, EventArgs e)
		{
			//We need to invoke the control so we don't get a cross thread exception
			if (this.InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker) delegate
				{
					onTrainFolderChanged(this, e);
				});
				return;
			}
			populateTrainList(currentTrainFolder);
			if (listviewTrainFolders.Columns.Count > 0)
			{
				listviewTrainFolders.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		/// <summary>Populates the train display list from the selected folder</summary>
		/// <param name="Folder">The folder containing train folders</param>
		private void populateTrainList(string Folder)
		{
			try
			{
				if (Folder.Length == 0)
				{
					// drives
					listviewTrainFolders.Items.Clear();
					try
					{
						DriveInfo[] driveInfos = DriveInfo.GetDrives();
						for (int i = 0; i < driveInfos.Length; i++)
						{
							ListViewItem Item = listviewTrainFolders.Items.Add(driveInfos[i].Name);
							Item.ImageKey = Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows ? @"disk" : @"folder";
							
							Item.Tag = driveInfos[i].RootDirectory.FullName;
							listviewTrainFolders.Tag = null;
						}
					}
					catch
					{
						//Unable to get list of drives
					}
				}
				else if (Directory.Exists(Folder))
				{
					listviewTrainFolders.Items.Clear();
					// parent
					try
					{
						DirectoryInfo Info = Directory.GetParent(Folder);
						if (Info != null)
						{
							ListViewItem Item = listviewTrainFolders.Items.Add("..");
							Item.ImageKey = @"parent";
							Item.Tag = Info.FullName;
							listviewTrainFolders.Tag = Info.FullName;
						}
						else
						{
							ListViewItem Item = listviewTrainFolders.Items.Add("..");
							Item.ImageKey = @"parent";
							Item.Tag = "";
							listviewTrainFolders.Tag = "";
						}
					}
					catch
					{
						//Another permisions issue?
					}
					// folders
					try
					{
						string[] Folders = Directory.GetDirectories(Folder);
						Array.Sort(Folders);
						for (int i = 0; i < Folders.Length; i++)
						{
							try
							{
								DirectoryInfo info = new DirectoryInfo(Folders[i]);
								if ((info.Attributes & FileAttributes.Hidden) == 0)
								{
									string folderName = System.IO.Path.GetFileName(Folders[i]);
									if (!string.IsNullOrEmpty(folderName) && folderName[0] != '.')
									{
										string File = Path.CombineFile(Folders[i], "train.dat");
										ListViewItem Item = listviewTrainFolders.Items.Add(folderName);
										Item.ImageKey = System.IO.File.Exists(File) ? "train" : "folder";
										Item.Tag = Folders[i];
									}
								}
							}
							catch
							{
								//Most likely permissions
							}
						}
					}
					catch
					{
						//Ignore all errors
					}
				}
			}
			catch
			{
				//Ignore all errors
			}
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
					if (Directory.Exists(t)) {
						try
						{
							string File = Path.CombineFile(t, "train.dat");
							if (System.IO.File.Exists(File))
							{
								Result.TrainFolder = t;
								ShowTrain(false);
								if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
							}
							else
							{
								groupboxTrainDetails.Visible = false;
								buttonStart.Enabled = false;
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
		private void listviewTrainFolders_DoubleClick(object sender, EventArgs e) {
			if (listviewTrainFolders.SelectedItems.Count == 1) {
				string t = listviewTrainFolders.SelectedItems[0].Tag as string;
				if (t != null) {
					if (t.Length == 0)
					{
						//Pop up to parent directory
						textboxTrainFolder.Text = t;
						return;
					}
					if (Directory.Exists(t))
					{
						string[] newDirectories = Directory.EnumerateDirectories(t).ToArray();
						if (newDirectories.Length > 5)
						{
							//More than 5 subdirectories, assume it may be a false positive
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
		private void listviewTrainFolders_KeyDown(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Return:
					listviewTrainFolders_DoubleClick(null, null);
					break;
				case Keys.Back:
					string t = listviewTrainFolders.Tag as string;
					if (t != null) {
						if (t.Length == 0 || Directory.Exists(t)) {
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
					if (Directory.Exists(t)) {
						string File = Path.CombineFile(t, "train.dat");
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
		void CheckboxTrainDefaultCheckedChanged(object sender, EventArgs e) {
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
								Interface.CurrentOptions.TrainEncodings[j].Codepage = EncodingCodepages[i];
								break;
							}
						} if (j == Interface.CurrentOptions.TrainEncodings.Length) {
							Array.Resize(ref Interface.CurrentOptions.TrainEncodings, j + 1);
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
		private readonly object StartGame = new Object();

		private void buttonStart_Click(object sender, EventArgs e) {
			if (Result.RouteFile != null & Result.TrainFolder != null) {
				if (File.Exists(Result.RouteFile) & Directory.Exists(Result.TrainFolder)) {
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

		private BackgroundWorker routeWorkerThread;

		private void routeWorkerThread_doWork(object sender, DoWorkEventArgs e)
		{
			if (string.IsNullOrEmpty(Result.RouteFile))
			{
				return;
			}

			string error; //ignored in this case, background thread
			if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out error, Program.TrainManager, Program.Renderer))
			{
				throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
			}
			Game.Reset(false);
			bool loaded = false;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(Result.RouteFile))
				{
					// ReSharper disable once RedundantCast
					object Route = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
					string RailwayFolder = Loading.GetRailwayFolder(Result.RouteFile);
					string ObjectFolder = Path.CombineDirectory(RailwayFolder, "Object");
					string SoundFolder = Path.CombineDirectory(RailwayFolder, "Sound");
					if (Program.CurrentHost.Plugins[i].Route.LoadRoute(Result.RouteFile, Result.RouteEncoding, null, ObjectFolder, SoundFolder, true, ref Route))
					{
						Program.CurrentRoute = (CurrentRoute) Route;
					}
					else
					{
						if (Program.CurrentHost.Plugins[i].Route.LastException != null)
						{
							throw Program.CurrentHost.Plugins[i].Route.LastException; //Re-throw last exception generated by the route parser plugin so that the UI thread captures it
						}
						else
						{
							throw new Exception("An unknown error was enountered whilst attempting to parser the routefile " + Result.RouteFile);
						}
					}
					loaded = true;
					break;
				}
			}

			if (Loading.Complete)
			{
				Program.CurrentHost.UnloadPlugins(out error);
			}
			
			if (!loaded)
			{
				throw new Exception("No plugins capable of loading routefile " + Result.RouteFile + " were found.");
			}

		}

		private void routeWorkerThread_completed(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null || Program.CurrentRoute == null)
			{
				TryLoadImage(pictureboxRouteImage, "route_error.png");
				if (e.Error != null)
				{
					textboxRouteDescription.Text = e.Error.Message;
				}
				textboxRouteEncodingPreview.Text = "";
				pictureboxRouteMap.Image = null;
				pictureboxRouteGradient.Image = null;
				Result.ErrorFile = Result.RouteFile;
				Result.RouteFile = null;
				checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
				routeWorkerThread.Dispose();
				this.Cursor = System.Windows.Forms.Cursors.Default;
				return;
			}
			try
			{
				lock (BaseRenderer.GdiPlusLock)
				{
					pictureboxRouteMap.Image = Illustrations.CreateRouteMap(pictureboxRouteMap.Width, pictureboxRouteMap.Height, false);
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
					string[] f = {".png", ".bmp", ".gif", ".tiff", ".tif", ".jpeg", ".jpg"};
					int i;
					for (i = 0; i < f.Length; i++)
					{
						string g = Path.CombineFile(System.IO.Path.GetDirectoryName(Result.RouteFile),
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
				string Description = Program.CurrentRoute.Comment.ConvertNewlinesToCrLf();
				textboxRouteDescription.Text = Description.Length != 0 ? Description : System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile);

				textboxRouteEncodingPreview.Text = Description.ConvertNewlinesToCrLf();
				if (Interface.CurrentOptions.TrainName != null)
				{
					checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault") + @" (" + Interface.CurrentOptions.TrainName + @")";
				}
				else
				{
					checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
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
				checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
			}
			

			if (checkboxTrainDefault.Checked)
			{
				ShowDefaultTrain();
			}

			this.Cursor = System.Windows.Forms.Cursors.Default;
			//Deliberately select the tab when the process is complete
			//This hopefully fixes another instance of the 'grey tabs' bug
			
			tabcontrolRouteDetails.SelectedTab = tabpageRouteDescription;

			buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
		}


		// show route
		private void ShowRoute(bool UserSelectedEncoding) {
			if (routeWorkerThread == null)
			{
				return;
			}
			if (Result.RouteFile != null && !routeWorkerThread.IsBusy)
			{
				this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
				TryLoadImage(pictureboxRouteImage, "loading.png");
				groupboxRouteDetails.Visible = true;
				textboxRouteDescription.Text = Translations.GetInterfaceString("start_route_processing");

				// determine encoding
				if (!UserSelectedEncoding) {
					Result.RouteEncoding = TextEncoding.GetSystemEncodingFromFile(Result.RouteFile);
					comboboxRouteEncoding.Tag = new object();
					comboboxRouteEncoding.SelectedIndex = 0;
					comboboxRouteEncoding.Items[0] = $"{Result.RouteEncoding.EncodingName} - {Result.RouteEncoding.CodePage}";
					comboboxRouteEncoding.Tag = null;

					comboboxRouteEncoding.Tag = new object();
					int i;
					for (i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++) {
						if (Interface.CurrentOptions.RouteEncodings[i].Value == Result.RouteFile) {
							int j;
							for (j = 1; j < EncodingCodepages.Length; j++) {
								if (EncodingCodepages[j] == Interface.CurrentOptions.RouteEncodings[i].Codepage) {
									comboboxRouteEncoding.SelectedIndex = j;
									Result.RouteEncoding = Encoding.GetEncoding(EncodingCodepages[j]);
									break;
								}
							}
							if (j == EncodingCodepages.Length) {
								comboboxRouteEncoding.SelectedIndex = 0;
								Result.RouteEncoding = Encoding.UTF8;
							}
							break;
						}
					}
					comboboxRouteEncoding.Tag = null;
				}
				if (!routeWorkerThread.IsBusy)
				{
					//HACK: If clicking very rapidly or holding down an arrow
					//		we can sometimes try to spawn two worker threads
					routeWorkerThread.RunWorkerAsync();
				}
			}
		}

		// show train
		private void ShowTrain(bool UserSelectedEncoding)
		{
			string error; //ignored in this case, background thread
			if (!Program.CurrentHost.LoadPlugins(Program.FileSystem, Interface.CurrentOptions, out error, Program.TrainManager, Program.Renderer))
			{
				throw new Exception("Unable to load the required plugins- Please reinstall OpenBVE");
			}
			bool canLoad = false;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(Result.TrainFolder))
				{
					canLoad = true;
				}
			}

			if (!canLoad)
			{
				groupboxTrainDetails.Visible = false;
				buttonStart.Enabled = false;
				//No plugin capable of loading train found
				return;
			}
			if (!UserSelectedEncoding) {
				Result.TrainEncoding = TextEncoding.GetSystemEncodingFromFile(Result.TrainFolder, "train.txt");
				comboboxTrainEncoding.Tag = new object();
				comboboxTrainEncoding.SelectedIndex = 0;
				comboboxTrainEncoding.Items[0] = $"{Result.TrainEncoding.EncodingName} - {Result.TrainEncoding.CodePage}";

				comboboxTrainEncoding.Tag = null;
				int i;
				for (i = 0; i < Interface.CurrentOptions.TrainEncodings.Length; i++) {
					if (Interface.CurrentOptions.TrainEncodings[i].Value == Result.TrainFolder) {
						int j;
						for (j = 1; j < EncodingCodepages.Length; j++) {
							if (EncodingCodepages[j] == Interface.CurrentOptions.TrainEncodings[i].Codepage) {
								comboboxTrainEncoding.SelectedIndex = j;
								Result.TrainEncoding = Encoding.GetEncoding(EncodingCodepages[j]);
								break;
							}
						}
						if (j == EncodingCodepages.Length) {
							comboboxTrainEncoding.SelectedIndex = 0;
							Result.TrainEncoding = Encoding.UTF8;
						}
						break;
					}
				}
				panelTrainEncoding.Enabled = true;
				comboboxTrainEncoding.Tag = null;
			}
			{
				// train image
				string File = Path.CombineFile(Result.TrainFolder, "train.png");
				if (!System.IO.File.Exists(File)) {
					File = Path.CombineFile(Result.TrainFolder, "train.bmp");
				}

				TryLoadImage(pictureboxTrainImage, System.IO.File.Exists(File) ? File : "train_unknown.png");
			}
			{
				// train description
				string File = Path.CombineFile(Result.TrainFolder, "train.txt");
				if (System.IO.File.Exists(File)) {
					try {
						string trainText = System.IO.File.ReadAllText(File, Result.TrainEncoding);
						trainText = trainText.ConvertNewlinesToCrLf();
						textboxTrainDescription.Text = trainText;
						textboxTrainEncodingPreview.Text = trainText;
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
			if (string.IsNullOrEmpty(Interface.CurrentOptions.TrainName)) {
				return;
			}
			
			string Folder;
			try {
				Folder = System.IO.Path.GetDirectoryName(Result.RouteFile);
				if (Interface.CurrentOptions.TrainName[0] == '$') {
					Folder = Path.CombineDirectory(Folder, Interface.CurrentOptions.TrainName);
					if (Directory.Exists(Folder)) {
						string File = Path.CombineFile(Folder, "train.dat");
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
			bool recursionTest = false;
			string lastFolder = null;
			try
			{
				while (true)
				{
					string TrainFolder = Path.CombineDirectory(Folder, "Train");
					var OldFolder = Folder;
					if (Directory.Exists(TrainFolder))
					{
						try
						{
							Folder = Path.CombineDirectory(TrainFolder, Interface.CurrentOptions.TrainName);
						}
						catch (Exception ex)
						{
							if (ex is ArgumentException)
							{
								break; // Invalid character in path causes infinite recursion
							}

							Folder = null;
						}

						if (Folder != null)
						{
							char c = System.IO.Path.DirectorySeparatorChar;
							if (Directory.Exists(Folder))
							{

								string File = Path.CombineFile(Folder, "train.dat");
								if (System.IO.File.Exists(File))
								{
									// train found
									Result.TrainFolder = Folder;
									ShowTrain(false);
									return;
								}

								if (lastFolder == Folder || recursionTest)
								{
									break;
								}

								lastFolder = Folder;
							}
							else if (Folder.ToLowerInvariant().Contains(c + "railway" + c))
							{
								//If we have a misplaced Train folder in either our Railway\Route
								//or Railway folders, this can cause the train search to fail
								//Detect the presence of a railway folder and carry on traversing upwards if this is the case
								recursionTest = true;
								Folder = OldFolder;
							}
							else
							{
								break;
							}
						}
					}

					if (Folder == null) continue;
					DirectoryInfo Info = Directory.GetParent(Folder);
					if (Info != null)
					{
						Folder = Info.FullName;
					}
					else
					{
						break;
					}
				}
			}
			catch
			{
				//Something broke, but we don't care as it just shows an error below
			}
			// train not found
			Result.TrainFolder = null;
			TryLoadImage(pictureboxTrainImage, "train_error.png");
			textboxTrainDescription.Text = (Translations.GetInterfaceString("start_train_notfound") + Interface.CurrentOptions.TrainName).ConvertNewlinesToCrLf();
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
