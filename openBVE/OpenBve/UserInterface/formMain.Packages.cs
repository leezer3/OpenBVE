using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenBveApi.Packages;

namespace OpenBve
{
	internal partial class formMain
	{
		/*
		 * This class contains the drawing and management routines for the
		 * package management tab of the main form.
		 * 
		 * Package manipulation is handled by the OpenBveApi.Packages namespace
		 */
		internal static bool creatingPackage = false;
		internal PackageType newPackageType;
		internal string ImageFile;
		internal BackgroundWorker workerThread = new BackgroundWorker();
		internal bool RemoveFromDatabase = true;
		internal Package dependantPackage;

		


		internal void RefreshPackages()
		{
			if (Database.SaveDatabase() == false)
			{
				MessageBox.Show(Interface.GetInterfaceString("packages_database_save_error"));
			}
			if (Database.LoadDatabase(currentDatabaseFolder, currentDatabaseFile) == true)
			{
				PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages, true);
			}
		}

		private Package currentPackage;
		private Package oldPackage;
		private List<PackageFile> filesToPackage;

		private void buttonInstall_Click(object sender, EventArgs e)
		{
			if (creatingPackage)
			{
				if (textBoxPackageName.Text == Interface.GetInterfaceString("packages_selection_none"))
				{
					MessageBox.Show(Interface.GetInterfaceString("packages_creation_name"));
					return;
				}
				if (textBoxPackageAuthor.Text == Interface.GetInterfaceString("packages_selection_none"))
				{
					MessageBox.Show(Interface.GetInterfaceString("packages_creation_author"));
					return;
				}
				//LINK: Doesn't need checking
				if (textBoxPackageDescription.Text == Interface.GetInterfaceString("packages_selection_none"))
				{
					MessageBox.Show(Interface.GetInterfaceString("packages_creation_description"));
					return;
				}
				if (!Version.TryParse(textBoxPackageVersion.Text, out currentPackage.PackageVersion))
				{
					MessageBox.Show(Interface.GetInterfaceString("packages_creation_version"));
				}
				//Only set properties after making the checks
				currentPackage.Name = textBoxPackageName.Text;
				currentPackage.Author = textBoxPackageAuthor.Text;
				currentPackage.Description = textBoxPackageDescription.Text.Replace("\r\n","\\r\\n");
				HidePanels();
				comboBoxDependancyType.SelectedIndex = 0;
				panelPackageDependsAdd.Show();
				PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages2, false);
				return;
			}
			//Check to see if the package is null- If null, then we haven't loaded a package yet
			if (currentPackage == null)
			{
				if (openPackageFileDialog.ShowDialog() == DialogResult.OK)
				{
					currentPackage = Manipulation.ReadPackage(openPackageFileDialog.FileName);
					if (currentPackage != null)
					{
						textBoxPackageName.Text = currentPackage.Name;
						textBoxPackageAuthor.Text = currentPackage.Author;
						if (currentPackage.Description != null)
						{
							textBoxPackageDescription.Text = currentPackage.Description.Replace("\\r\\n", "\r\n");
						}
						textBoxPackageVersion.Text = currentPackage.PackageVersion.ToString();
						if (currentPackage.Website != null)
						{
							linkLabelPackageWebsite.Links.Clear();
							linkLabelPackageWebsite.Text = currentPackage.Website;
							LinkLabel.Link link = new LinkLabel.Link {LinkData = currentPackage.Website};
							linkLabelPackageWebsite.Links.Add(link);
						}
						else
						{
							linkLabelPackageWebsite.Text = Interface.GetInterfaceString("packages_selection_none_website");
						}
						if (currentPackage.PackageImage != null)
						{
							pictureBoxPackageImage.Image = currentPackage.PackageImage;
						}
						else
						{
							TryLoadImage(pictureBoxPackageImage, currentPackage.PackageType == 0 ? "route_unknown.png" : "train_unknown.png");
						}
						buttonSelectPackage.Text = Interface.GetInterfaceString("packages_install");
					}
					else
					{
						//ReadPackage returns null if the file is not a package.....
						MessageBox.Show(Interface.GetInterfaceString("packages_install_invalid"));
					}
				}

			}
			else
			{
				List<Package> Dependancies = Database.CheckDependancies(currentPackage);
				if (Dependancies != null)
				{
					//We are missing a dependancy
					PopulatePackageList(Dependancies, dataGridViewDependancies, false);
					HidePanels();
					panelDependancyError.Show();
					return;
				}
				VersionInformation Info;
				oldPackage = null;
				switch (currentPackage.PackageType)
				{
					case PackageType.Route:
						Info = Information.CheckVersion(currentPackage, Database.currentDatabase.InstalledRoutes, ref oldPackage);
						break;
					case PackageType.Train:
						Info = Information.CheckVersion(currentPackage, Database.currentDatabase.InstalledTrains, ref oldPackage);
						break;
					default:
						Info = Information.CheckVersion(currentPackage, Database.currentDatabase.InstalledOther, ref oldPackage);
						break;
				}
				if (Info == VersionInformation.NotFound)
				{
					panelPackageInstall.Hide();
					Extract();
				}
				else
				{
					switch (Info)
					{
						case VersionInformation.NewerVersion:
							labelVersionError.Text = Interface.GetInterfaceString("packages_install_version_new");
							textBoxCurrentVersion.Text = oldPackage.PackageVersion.ToString();
							break;
						case VersionInformation.SameVersion:
							labelVersionError.Text = Interface.GetInterfaceString("packages_install_version_same");
							textBoxCurrentVersion.Text = currentPackage.PackageVersion.ToString();
							break;
						case VersionInformation.OlderVersion:
							labelVersionError.Text = Interface.GetInterfaceString("packages_install_version_old");
							textBoxCurrentVersion.Text = oldPackage.PackageVersion.ToString();
							break;
					}
					textBoxNewVersion.Text = currentPackage.PackageVersion.ToString();
					if (currentPackage.Dependancies.Count != 0)
					{
						List<Package> brokenDependancies = OpenBveApi.Packages.Information.UpgradeDowngradeDependancies(currentPackage, Database.currentDatabase.InstalledRoutes, Database.currentDatabase.InstalledTrains);
						if (brokenDependancies != null)
						{
							PopulatePackageList(brokenDependancies, dataGridViewBrokenDependancies, false);
						}
					}
					HidePanels();
					panelVersionError.Show();
				}
			}
				
		}

		private void buttonInstallFinished_Click(object sender, EventArgs e)
		{
			RefreshPackages();
			ResetInstallerPanels();
		}

		internal static readonly string currentDatabaseFolder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase");
		private static readonly string currentDatabaseFile = OpenBveApi.Path.CombineFile(currentDatabaseFolder, "packages.xml");

		private void buttonProceedAnyway_Click(object sender, EventArgs e)
		{
			panelDependancyError.Hide();
			labelMissingDependanciesText1.Text = Interface.GetInterfaceString("packages_install_dependancies_unmet");
			Extract(oldPackage);
		}

		private void Extract(Package packageToReplace = null)
		{
			panelPleaseWait.Show();
			workerThread.DoWork += delegate
			{
				string ExtractionDirectory;
				switch (currentPackage.PackageType)
				{
					case PackageType.Route:
						ExtractionDirectory = Program.FileSystem.RouteInstallationDirectory;
						break;
					case PackageType.Train:
						ExtractionDirectory = Program.FileSystem.TrainInstallationDirectory;
						break;
					default:
						ExtractionDirectory = Program.FileSystem.OtherInstallationDirectory;
						break;
				}
				string PackageFiles = "";
				Manipulation.ExtractPackage(currentPackage, ExtractionDirectory, currentDatabaseFolder, ref PackageFiles);
				textBoxFilesInstalled.Invoke((MethodInvoker) delegate
				{
					textBoxFilesInstalled.Text = PackageFiles;
				});
			};
			workerThread.RunWorkerCompleted += delegate
			{
				switch (currentPackage.PackageType)
				{
					case PackageType.Route:
						if (packageToReplace != null)
						{
							for (int i = Database.currentDatabase.InstalledRoutes.Count; i > 0; i--)
							{
								if (Database.currentDatabase.InstalledRoutes[i - 1].GUID == currentPackage.GUID)
								{
									Database.currentDatabase.InstalledRoutes.Remove(Database.currentDatabase.InstalledRoutes[i - 1]);
								}
							}
						}
						Database.currentDatabase.InstalledRoutes.Add(currentPackage);
						break;
					case PackageType.Train:
						if (packageToReplace != null)
						{
							for (int i = Database.currentDatabase.InstalledTrains.Count; i > 0; i--)
							{
								if (Database.currentDatabase.InstalledTrains[i - 1].GUID == currentPackage.GUID)
								{
									Database.currentDatabase.InstalledTrains.Remove(Database.currentDatabase.InstalledTrains[i - 1]);
								}
							}
						}
						Database.currentDatabase.InstalledTrains.Add(currentPackage);
						break;
				}
				labelInstallSuccess1.Text = Interface.GetInterfaceString("packages_install_success");
				labelInstallSuccess2.Text = Interface.GetInterfaceString("packages_install_success_header");
				labelListFilesInstalled.Text = Interface.GetInterfaceString("packages_install_success_files");
				panelPleaseWait.Hide();
				panelSuccess.Show();
			};
			workerThread.RunWorkerAsync();
		}

		private void OnWorkerProgressChanged(object sender, ProgressReport e)
		{
			//We need to invoke the control so we don't get a cross thread exception
			if (this.InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker)delegate
				{
					OnWorkerProgressChanged(sender, e);
				});
				return;
			}

			//Actually change the controls text
			labelProgressPercent.Text = e.Progress + @"%";
			labelProgressFile.Text = e.CurrentFile;
		}

		/// <summary>This method should be called to populate a datagrid view with a list of packages</summary>
		/// <param name="packageList">The list of packages</param>
		/// <param name="dataGrid">The datagrid view to populate</param>
		/// <param name="simpleList">Whether this is a simple list or a dependancy list</param>
		internal void PopulatePackageList(List<Package> packageList, DataGridView dataGrid, bool simpleList)
		{
			//Clear the package list
			dataGrid.Rows.Clear();
			//We have route packages in our list!
			for (int i = 0; i < packageList.Count; i++)
			{
				//Create row
				object[] packageToAdd;
				if (!simpleList)
				{
					packageToAdd = new object[]
					{
						packageList[i].Name, packageList[i].MinimumVersion, packageList[i].MaximumVersion, packageList[i].Author,
						packageList[i].Website
					};
				}
				else
				{
					packageToAdd = new object[]
					{
						packageList[i].Name, packageList[i].Version, packageList[i].Author, packageList[i].Website
					};
				}
				//Add to the datagrid view
				dataGrid.Rows.Add(packageToAdd);
			}
			dataGrid.ClearSelection();
		}

		/// <summary>This method should be called to uninstall a package</summary>
		internal void UninstallPackage(Package packageToUninstall, ref List<Package> Packages)
		{
			
			string uninstallResults = "";
			List<Package> brokenDependancies = Database.CheckUninstallDependancies(packageToUninstall.Dependancies);
			if (brokenDependancies.Count != 0)
			{
				PopulatePackageList(brokenDependancies, dataGridViewBrokenDependancies, false);
				labelMissingDependanciesText1.Text = Interface.GetInterfaceString("packages_uninstall_broken");
				HidePanels();
				panelDependancyError.Show();
			}
			if (Manipulation.UninstallPackage(packageToUninstall, currentDatabaseFolder ,ref uninstallResults))
			{
				Packages.Remove(packageToUninstall);
				switch (packageToUninstall.PackageType)
				{
					case PackageType.Other:
						DatabaseFunctions.cleanDirectory(Program.FileSystem.OtherInstallationDirectory, ref uninstallResults);
						break;
					case PackageType.Route:
						DatabaseFunctions.cleanDirectory(Program.FileSystem.RouteInstallationDirectory, ref uninstallResults);
						break;
					case PackageType.Train:
						DatabaseFunctions.cleanDirectory(Program.FileSystem.TrainInstallationDirectory, ref uninstallResults);
						break;
				}
				labelUninstallSuccess.Text = Interface.GetInterfaceString("packages_uninstall_success");
				labelUninstallSuccessHeader.Text = Interface.GetInterfaceString("packages_uninstall_success_header");
				textBoxUninstallResult.Text = uninstallResults;
				HidePanels();
				panelUninstallResult.Show();
			}
			else
			{
				labelUninstallSuccess.Text = Interface.GetInterfaceString("packages_uninstall_success");
				labelUninstallSuccessHeader.Text = Interface.GetInterfaceString("packages_uninstall_success_header");
				if (uninstallResults == null)
				{
					//Uninstall requires an XML list of files, and these were missing.......
					textBoxUninstallResult.Text = Interface.GetInterfaceString("packages_uninstall_missing_xml");
					currentPackage = packageToUninstall;
					RemoveFromDatabase = false;
				}
				else
				{
					//Something went wrong in the uninstallation process, so show the log
					//TODO: This doesn't log anything other than a list of files at the minute
					textBoxUninstallResult.Text = uninstallResults;
				}
			}
			HidePanels();
			panelUninstallResult.Show();
		}



		internal void AddDependendsReccomends(Package packageToAdd, ref List<Package> DependsReccomendsList)
		{
			if (currentPackage != null)
			{
				if (DependsReccomendsList == null)
				{
					DependsReccomendsList = new List<Package>();
				}
				DependsReccomendsList.Add(packageToAdd);
			}
		}




		private void dataGridViewPackages_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewPackages.SelectedRows.Count == 0)
			{
				currentPackage = null;
				return;
			}
			var selectedPackageIndex = dataGridViewPackages.SelectedRows[0].Index;
			switch (comboBoxPackageType.SelectedIndex)
			{
				case 0:
					currentPackage = Database.currentDatabase.InstalledRoutes[selectedPackageIndex];
					break;
				case 1:
					currentPackage = Database.currentDatabase.InstalledTrains[selectedPackageIndex];
					break;
				case 2:
					currentPackage = Database.currentDatabase.InstalledOther[selectedPackageIndex];
					break;
			}
		}

		private void buttonUninstallPackage_Click(object sender, EventArgs e)
		{
			if (currentPackage != null)
			{
				switch (currentPackage.PackageType)
				{
					case PackageType.Route:
						UninstallPackage(currentPackage, ref Database.currentDatabase.InstalledRoutes);
						break;
					case PackageType.Train:
						UninstallPackage(currentPackage, ref Database.currentDatabase.InstalledTrains);
						break;
					case PackageType.Other:
						UninstallPackage(currentPackage, ref Database.currentDatabase.InstalledOther);
						break;
				}
			}
			else
			{
				MessageBox.Show(Interface.GetInterfaceString("packages_selection_none"));
			}
		}

		private void buttonUninstallFinish_Click(object sender, EventArgs e)
		{
			if (currentPackage != null)
			{
				//If we were unable to uninstall the package
				//prompt as to whether the user would like to remove the broken package
				if (RemoveFromDatabase == false)
				{
					if (MessageBox.Show(Interface.GetInterfaceString("packages_uninstall_database_remove"), Interface.GetInterfaceString("program_title"), MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RemoveFromDatabase = true;
					}
				}
				if (RemoveFromDatabase)
				{
					switch (currentPackage.PackageType)
					{
						case PackageType.Other:
							Database.currentDatabase.InstalledOther.Remove(currentPackage);
							break;
						case PackageType.Route:
							Database.currentDatabase.InstalledRoutes.Remove(currentPackage);
							break;
						case PackageType.Train:
							Database.currentDatabase.InstalledTrains.Remove(currentPackage);
							break;
					}
				}
				currentPackage = null;
			}
			RefreshPackages();
			ResetInstallerPanels();
		}


		private void buttonInstallPackage_Click(object sender, EventArgs e)
		{
			currentPackage = null;
			labelInstallText.Text = Interface.GetInterfaceString("packages_install_header");
			TryLoadImage(pictureBoxPackageImage, "route_error.png");
			HidePanels();
			panelPackageInstall.Show();
		}

		private void buttonDepends_Click(object sender, EventArgs e)
		{
			if (dependantPackage != null)
			{
				dependantPackage.Version = null;
				switch (dependantPackage.PackageType)
				{
					case PackageType.Route:
						if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion,dependantPackage.Version) == DialogResult.OK)
						{
							AddDependendsReccomends(dependantPackage, ref currentPackage.Dependancies);
							dependantPackage = null;
						}
						break;
					case PackageType.Train:
						if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion,dependantPackage.Version) == DialogResult.OK)
						{
							AddDependendsReccomends(dependantPackage, ref currentPackage.Dependancies);
							dependantPackage = null;
						}
						break;
					case PackageType.Other:
						if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion,dependantPackage.Version) == DialogResult.OK)
						{
							AddDependendsReccomends(dependantPackage, ref currentPackage.Dependancies);
							dependantPackage = null;
						}
						break;
				}
			}
		}

		private void buttonReccomends_Click(object sender, EventArgs e)
		{
			if (dependantPackage != null)
			{
				dependantPackage.Version = null;
				switch (dependantPackage.PackageType)
				{
					case PackageType.Route:
						if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion,dependantPackage.Version) == DialogResult.OK)
						{
							AddDependendsReccomends(dependantPackage, ref currentPackage.Reccomendations);
							dependantPackage = null;
						}
						break;
					case PackageType.Train:
						if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion,dependantPackage.Version) == DialogResult.OK)
						{
							AddDependendsReccomends(dependantPackage, ref currentPackage.Reccomendations);
							dependantPackage = null;
						}
						break;
					case PackageType.Other:
						if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion,dependantPackage.Version) == DialogResult.OK)
						{
							AddDependendsReccomends(dependantPackage, ref currentPackage.Reccomendations);
							dependantPackage = null;
						}
						break;
				}
			}
		}

		private void dataGridViewPackages2_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewPackages2.SelectedRows.Count == 0)
			{
				dependantPackage = null;
				return;
			}
			var selectedPackageIndex = dataGridViewPackages2.SelectedRows[0].Index;
			switch (comboBoxDependancyType.SelectedIndex)
			{
				case 0:
					dependantPackage = Database.currentDatabase.InstalledRoutes[selectedPackageIndex];
					break;
				case 1:
					dependantPackage = Database.currentDatabase.InstalledTrains[selectedPackageIndex];
					break;
				case 2:
					dependantPackage = Database.currentDatabase.InstalledOther[selectedPackageIndex];
					break;
			}
		}


		private void buttonCreatePackage_Click(object sender, EventArgs e)
		{
			HidePanels();
			panelPleaseWait.Show();
			workerThread.DoWork += delegate
			{
				Manipulation.ProgressChanged += OnWorkerProgressChanged;
				Manipulation.CreatePackage(currentPackage, Interface.CurrentOptions.packageCompressionType, currentPackage.FileName, ImageFile, filesToPackage);
				string text = "";
				for (int i = 0; i < filesToPackage.Count; i++)
				{
					text += filesToPackage[i].relativePath + "\r\n";
				}
				textBoxFilesInstalled.Invoke((MethodInvoker) delegate
				{
					textBoxFilesInstalled.Text = text;
				});
				System.Threading.Thread.Sleep(10000);
			};

			workerThread.RunWorkerCompleted += delegate {
				labelInstallSuccess1.Text = Interface.GetInterfaceString("packages_creation_success");
				labelInstallSuccess2.Text = Interface.GetInterfaceString("packages_creation_success_header");
				labelListFilesInstalled.Text = Interface.GetInterfaceString("packages_creation_success_files");
				label1.Text = Interface.GetInterfaceString("packages_success");
				panelPleaseWait.Hide();
				panelSuccess.Show();
			};

			 workerThread.RunWorkerAsync();	
		}

		private void buttonCreateProceed_Click(object sender, EventArgs e)
		{
			if (currentPackage == null || currentPackage.GUID == null)
			{
				//Don't crash if we've clicked on the button without selecting anything
				return;
			}
			currentPackage.FileName = textBoxPackageFileName.Text;
			System.IO.FileInfo fi = null;
			try
			{
				fi = new System.IO.FileInfo(currentPackage.FileName);
			}
			catch
			{
			}
			if (fi == null)
			{
				//The supplied filename was invalid
				MessageBox.Show(Interface.GetInterfaceString("packages_creation_invalid_filename"));
				return;
			}
			try
			{
				System.IO.File.Delete(currentPackage.FileName);
			}
			catch
			{
				//The file is locked or otherwise unavailable
				MessageBox.Show(Interface.GetInterfaceString("packages_creation_invalid_filename"));
				return;
			}
			buttonSelectPackage.Text = Interface.GetInterfaceString("packages_creation_proceed");
			creatingPackage = true;
			switch (newPackageType)
			{
				case PackageType.Route:
					TryLoadImage(pictureBoxPackageImage, "route_unknown.png");
					break;
				case PackageType.Train:
					TryLoadImage(pictureBoxPackageImage, "train_unknown.png");
					break;
				default:
					TryLoadImage(pictureBoxPackageImage, "logo.png");
					break;
			}
			labelInstallText.Text = Interface.GetInterfaceString("packages_creation_header");
			textBoxPackageName.Text = currentPackage.Name;
			textBoxPackageVersion.Text = currentPackage.Version;
			textBoxPackageAuthor.Text = currentPackage.Author;
			if (currentPackage.Description != null)
			{
				textBoxPackageDescription.Text = currentPackage.Description.Replace("\\r\\n", "\r\n");
			}
			HidePanels();
			textBoxPackageDescription.ReadOnly = false;
			textBoxPackageName.ReadOnly = false;
			textBoxPackageVersion.ReadOnly = false;
			textBoxPackageAuthor.ReadOnly = false;
			panelPackageInstall.Show();
		}

		private void createPackageButton_Click(object sender, EventArgs e)
		{
			currentPackage = null;
			HidePanels();
			panelCreatePackage.Show();
		}

		private void pictureBoxPackageImage_Click(object sender, EventArgs e)
		{
			if (creatingPackage)
			{
				if (openPackageFileDialog.ShowDialog() == DialogResult.OK)
				{
					ImageFile = openPackageFileDialog.FileName;
					try
					{
						pictureBoxPackageImage.Image = Image.FromFile(openPackageFileDialog.FileName);
					}
					catch
					{
						//Not an image we can load, so reset the image file
						ImageFile = null;
					}
				}
			}
		}


		private void Q1_CheckedChanged(object sender, EventArgs e)
		{
			filesToPackage = null;
			filesToPackageBox.Text = String.Empty;
			if (radioButtonQ1Yes.Checked == true)
			{
				if (Database.currentDatabase.InstalledRoutes.Count == 0 && Database.currentDatabase.InstalledTrains.Count == 0 && Database.currentDatabase.InstalledOther.Count == 0)
				{
					//There are no packages available to replace....
					string test = Interface.GetInterfaceString("packages_replace_noneavailable");
					MessageBox.Show(test);
					radioButtonQ1No.Checked = true;
					return;
				}
				panelReplacePackage.Show();
				panelNewPackage.Hide();
				switch (newPackageType)
				{
					case PackageType.Route:
						PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewReplacePackage, false);
						break;
					case PackageType.Train:
						PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewReplacePackage, false);
						break;
					case PackageType.Other:
						PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewReplacePackage, false);
						break;
				}
				dataGridViewReplacePackage.ClearSelection();
			}
			else
			{
				panelReplacePackage.Hide();
				panelNewPackage.Show();
				panelNewPackage.Enabled = true;
				string GUID = Guid.NewGuid().ToString();
				currentPackage = new Package
				{
					Name = textBoxPackageName.Text,
					Author = textBoxPackageAuthor.Text,
					Description = textBoxPackageDescription.Text.Replace("\r\n", "\\r\\n"),
					//TODO:
					//Website = linkLabelPackageWebsite.Links[0],
					GUID = GUID,
					PackageVersion = new Version(0, 0, 0, 0),
					PackageType = newPackageType
				};
				textBoxGUID.Text = currentPackage.GUID;
				SaveFileNameButton.Enabled = true;
			}
		}

		private void Q2_CheckedChanged(object sender, EventArgs e)
		{
			filesToPackage = null;
			filesToPackageBox.Text = string.Empty;
			radioButtonQ1Yes.Enabled = true;
			radioButtonQ1No.Enabled = true;
			labelReplacePackage.Enabled = true;
			if (radioButtonQ2Route.Checked)
			{
				newPackageType = PackageType.Route;
			}
			else if (radioButtonQ2Train.Checked)
			{
				newPackageType = PackageType.Train;
			}
			else
			{
				newPackageType = PackageType.Other;
			}
			if (radioButtonQ1Yes.Checked == true)
			{
				panelReplacePackage.Show();
				panelNewPackage.Hide();
				switch (newPackageType)
				{
					case PackageType.Route:
						PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewReplacePackage, false);
						break;
					case PackageType.Train:
						PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewReplacePackage, false);
						break;
					case PackageType.Other:
						PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewReplacePackage, false);
						break;
				}
				dataGridViewReplacePackage.ClearSelection();
			}
			else
			{
				panelReplacePackage.Hide();
				panelNewPackage.Show();
				panelNewPackage.Enabled = false;
				if (radioButtonQ1Yes.Checked || radioButtonQ1No.Checked)
				{
					//Don't generate a new GUID if we haven't decided whether this is a new/ replacement package
					//Pointless & confusing UI update otherwise
					string GUID = Guid.NewGuid().ToString();
					currentPackage = new Package
					{
						Name = textBoxPackageName.Text,
						Author = textBoxPackageAuthor.Text,
						Description = textBoxPackageDescription.Text.Replace("\r\n", "\\r\\n"),
						//TODO:
						//Website = linkLabelPackageWebsite.Links[0],
						GUID = GUID,
						PackageVersion = new Version(0, 0, 0, 0),
						PackageType = newPackageType
					};
					textBoxGUID.Text = currentPackage.GUID;
					SaveFileNameButton.Enabled = true;
				}
			}


		}


		private void dataGridViewReplacePackage_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewReplacePackage.SelectedRows.Count > 0)
			{
				switch (newPackageType)
				{
					case PackageType.Route:
						currentPackage = Database.currentDatabase.InstalledRoutes[dataGridViewReplacePackage.SelectedRows[0].Index];
						currentPackage.PackageType = PackageType.Route;
						break;
					case PackageType.Train:
						currentPackage = Database.currentDatabase.InstalledTrains[dataGridViewReplacePackage.SelectedRows[0].Index];
						currentPackage.PackageType = PackageType.Train;
						break;
					case PackageType.Other:
						currentPackage = Database.currentDatabase.InstalledOther[dataGridViewReplacePackage.SelectedRows[0].Index];
						currentPackage.PackageType = PackageType.Other;
						break;
				}
			}
		}

		private void linkLabelPackageWebsite_Click(object sender, EventArgs e)
		{
			if (creatingPackage)
			{
				if (ShowInputDialog(ref currentPackage.Website) == DialogResult.OK)
				{
					linkLabelPackageWebsite.Text = currentPackage.Website;
					linkLabelPackageWebsite.Links[0].LinkData = currentPackage.Website;
				}
				
			}
			else
			{
				//Launch link in default web-browser
				if (currentPackage != null && currentPackage.Website != null)
				{
					string launchLink = null;
					if (!currentPackage.Website.ToLowerInvariant().StartsWith("http://"))
					{
						launchLink += "http://";
					}
					launchLink += currentPackage.Website;
					Uri URL;
					bool result = Uri.TryCreate(launchLink, UriKind.Absolute, out URL) && (URL.Scheme == Uri.UriSchemeHttp || URL.Scheme == Uri.UriSchemeHttps);
					if (result == true)
					{
						Process.Start(launchLink);
					}
				}
			}
		}


		private static DialogResult ShowInputDialog(ref string input)
		{
			System.Drawing.Size size = new System.Drawing.Size(200, 70);
			Form inputBox = new Form
			{
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
				ClientSize = size,
				Text = Interface.GetInterfaceString("list_name")
			};


			System.Windows.Forms.TextBox textBox = new TextBox
			{
				Size = new System.Drawing.Size(size.Width - 10, 23),
				Location = new System.Drawing.Point(5, 5),
				Text = input
			};
			inputBox.Controls.Add(textBox);

			Button okButton = new Button
			{
				DialogResult = System.Windows.Forms.DialogResult.OK,
				Name = "okButton",
				Size = new System.Drawing.Size(75, 23),
				Text = "&OK",
				Location = new System.Drawing.Point(size.Width - 80 - 80, 39)
			};
			inputBox.Controls.Add(okButton);

			Button cancelButton = new Button
			{
				DialogResult = System.Windows.Forms.DialogResult.Cancel,
				Name = "cancelButton",
				Size = new System.Drawing.Size(75, 23),
				Text = "&Cancel",
				Location = new System.Drawing.Point(size.Width - 80, 39)
			};
			inputBox.Controls.Add(cancelButton);

			inputBox.AcceptButton = okButton;
			inputBox.CancelButton = cancelButton;
			inputBox.AcceptButton = okButton; 
			inputBox.CancelButton = cancelButton; 
			DialogResult result = inputBox.ShowDialog();
			input = textBox.Text;
			return result;
		}

		private static DialogResult ShowVersionDialog(ref Version minimumVersion, ref Version maximumVersion, string currentVersion)
		{
			System.Drawing.Size size = new System.Drawing.Size(200, 80);
			Form inputBox = new Form
			{
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
				ClientSize = size,
				Text = Interface.GetInterfaceString("list_minimum")
			};


			System.Windows.Forms.TextBox textBox = new TextBox
			{
				Size = new System.Drawing.Size(size.Width - 10, 23),
				Location = new System.Drawing.Point(5, 5),
				Text = currentVersion
			};
			inputBox.Controls.Add(textBox);

			System.Windows.Forms.TextBox textBox2 = new TextBox
			{
				Size = new System.Drawing.Size(size.Width - 10, 23),
				Location = new System.Drawing.Point(5, 25),
				Text = currentVersion
			};
			inputBox.Controls.Add(textBox2);

			Button okButton = new Button
			{
				DialogResult = System.Windows.Forms.DialogResult.OK,
				Name = "okButton",
				Size = new System.Drawing.Size(75, 23),
				Text = "&OK",
				Location = new System.Drawing.Point(size.Width - 80 - 80, 49)
			};
			inputBox.Controls.Add(okButton);

			Button cancelButton = new Button
			{
				DialogResult = System.Windows.Forms.DialogResult.Cancel,
				Name = "cancelButton",
				Size = new System.Drawing.Size(75, 23),
				Text = "&Cancel",
				Location = new System.Drawing.Point(size.Width - 80, 49)
			};
			inputBox.Controls.Add(cancelButton);

			inputBox.AcceptButton = okButton;
			inputBox.CancelButton = cancelButton;
			inputBox.AcceptButton = okButton;
			inputBox.CancelButton = cancelButton;
			DialogResult result = inputBox.ShowDialog();
			try
			{
				if (textBox.Text == String.Empty || textBox.Text == @"0")
				{
					minimumVersion = null;
				}
				else
				{
					minimumVersion = Version.Parse(textBox.Text);	
				}
				if (textBox2.Text == String.Empty || textBox2.Text == @"0")
				{
					minimumVersion = null;
				}
				else
				{
					maximumVersion = Version.Parse(textBox2.Text);
				}
			}
			catch
			{
				MessageBox.Show(Interface.GetInterfaceString("packages_creation_version_invalid"));
			}
			return result;
		}

		private void HidePanels()
		{
			panelPackageInstall.Hide();
			panelDependancyError.Hide();
			panelVersionError.Hide();
			panelSuccess.Hide();
			panelUninstallResult.Hide();
			panelPackageDependsAdd.Hide();
			panelCreatePackage.Hide();
			panelReplacePackage.Hide();
			panelPackageList.Hide();
			panelPleaseWait.Hide();
		}

		private void newPackageClearSelectionButton_Click(object sender, EventArgs e)
		{
			if (filesToPackage != null)
			{
				filesToPackage.Clear();
			}
			filesToPackageBox.Clear();
		}

		//This uses a folder picker dialog to add folders
		private void addPackageItemsButton_Click(object sender, EventArgs e)
		{
			var dialog = new FolderSelectDialog();
			if (dialog.Show(Handle))
			{
				var tempList = new List<PackageFile>();
				//This dialog is used elsewhere, so we'd better reset it's properties
				openPackageFileDialog.Multiselect = false;
				if (filesToPackage == null)
				{
					filesToPackage = new List<PackageFile>();
				}
				filesToPackageBox.Text += dialog.FileName + Environment.NewLine;
				//Directory- Get all the files within the directory and add to our list
				string[] files = System.IO.Directory.GetFiles(dialog.FileName, "*.*", System.IO.SearchOption.AllDirectories);
				for (int i = 0; i < files.Length; i++)
				{
					var File = new PackageFile
					{
						absolutePath = files[i],
						relativePath = files[i].Replace(System.IO.Directory.GetParent(dialog.FileName).ToString(), ""),
					};
					tempList.Add(File);
				}

				filesToPackage.AddRange(DatabaseFunctions.FindFileLocations(tempList));
				
			}
		}

		private void replacePackageButton_Click(object sender, EventArgs e)
		{
			if (dataGridViewReplacePackage.SelectedRows.Count == 0)
			{
				//Don't crash if we haven't selected a package to replace....
				return;
			}
			labelNewGUID.Text = Interface.GetInterfaceString("packages_creation_replace_id");
			textBoxGUID.Text = currentPackage.GUID;
			panelNewPackage.Enabled = true;
			addPackageItemsButton.Enabled = true;
			newPackageClearSelectionButton.Enabled = true;
			SaveFileNameButton.Enabled = true;
			panelReplacePackage.Hide();
			panelNewPackage.Show();
		}


		private void SaveFileNameButton_Click(object sender, EventArgs e)
		{
			savePackageDialog = new SaveFileDialog
			{
				Title = Interface.GetInterfaceString("packages_creation_save"),
				CheckPathExists = true,
				DefaultExt = "zip",
				Filter = @"ZIP files (*.zip)|*.zip|All files (*.*)|*.*",
				FilterIndex = 2,
				RestoreDirectory = true
			};

			if (savePackageDialog.ShowDialog() == DialogResult.OK)
			{
				currentPackage.FileName = savePackageDialog.FileName;
				textBoxPackageFileName.Text = savePackageDialog.FileName;
			}
		}

		private void comboBoxPackageType_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (comboBoxPackageType.SelectedIndex)
			{
				case 0:
					PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages, true);
					break;
				case 1:
					PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewPackages, true);
					break;
				case 2:
					PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewPackages, true);
					break;
			}
			currentPackage = null;
			dataGridViewPackages.ClearSelection();
		}

		private void comboBoxDependancyType_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (comboBoxDependancyType.SelectedIndex)
			{
				case 0:
					PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages2, true);
					break;
				case 1:
					PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewPackages2, true);
					break;
				case 2:
					PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewPackages2, true);
					break;
			}
		}


		private void buttonProceedAnyway1_Click(object sender, EventArgs e)
		{
			HidePanels();
			if (radioButtonOverwrite.Checked)
			{
				//Plain overwrite
				Extract();
			}
			else if (radioButtonReplace.Checked)
			{
				//Reinstall
				string result = String.Empty;
				Manipulation.UninstallPackage(currentPackage, currentDatabaseFolder, ref result);
				switch (currentPackage.PackageType)
				{
					case PackageType.Route:
						for (int i = Database.currentDatabase.InstalledRoutes.Count -1; i >= 0; i--)
						{
							if (Database.currentDatabase.InstalledRoutes[i].GUID == currentPackage.GUID)
							{
								Database.currentDatabase.InstalledRoutes.RemoveAt(i);
							}
						}
						DatabaseFunctions.cleanDirectory(Program.FileSystem.RouteInstallationDirectory, ref result);
						break;
					case PackageType.Train:
						for (int i = Database.currentDatabase.InstalledTrains.Count - 1; i >= 0; i--)
						{
							if (Database.currentDatabase.InstalledTrains[i].GUID == currentPackage.GUID)
							{
								Database.currentDatabase.InstalledTrains.RemoveAt(i);
							}
						}
						DatabaseFunctions.cleanDirectory(Program.FileSystem.TrainInstallationDirectory, ref result);
						break;
					case PackageType.Other:
						for (int i = Database.currentDatabase.InstalledOther.Count - 1; i >= 0; i--)
						{
							if (Database.currentDatabase.InstalledOther[i].GUID == currentPackage.GUID)
							{
								Database.currentDatabase.InstalledOther.RemoveAt(i);
							}
						}
						break;
				}
				Extract();
			}
			else if(radioButtonCancel.Checked)
			{
				//Cancel
				ResetInstallerPanels();
			}

		}

		private void dataGridViewDependancies_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			string s = dataGridViewDependancies.SelectedCells[0].Value.ToString();
			//HACK: Multiple cells can be selected, so we should just check the first one
			if (s.StartsWith("www.") || s.StartsWith("http://"))
			{
				Process.Start(s);
			}
		}

		//This method resets the package installer to the default panels when clicking away, or when a creation/ install has finished
		private void ResetInstallerPanels()
		{
			HidePanels();
			panelPackageList.Show();
			creatingPackage = false;
			//Reset radio buttons in the installer
			radioButtonQ1Yes.Checked = false;
			radioButtonQ1No.Checked = false;
			radioButtonQ2Route.Checked = false;
			radioButtonQ2Train.Checked = false;
			radioButtonQ2Other.Checked = false;
			//Reset picturebox
			TryLoadImage(pictureBoxPackageImage, "route_unknown.png");
			TryLoadImage(pictureBoxProcessing, "logo.png");
			//Reset enabled boxes & panels
			textBoxGUID.Text = string.Empty;
			textBoxGUID.Enabled = false;
			SaveFileNameButton.Enabled = false;
			panelReplacePackage.Hide();
			panelNewPackage.Enabled = false;
			panelNewPackage.Show();
			textBoxPackageDescription.ReadOnly = true;
			textBoxPackageName.ReadOnly = true;
			textBoxPackageVersion.ReadOnly = true;
			textBoxPackageAuthor.ReadOnly = true;
			//Set variables to uninitialised states
			creatingPackage = false;
			currentPackage = null;
			dependantPackage = null;
			newPackageType = PackageType.NotFound;
			ImageFile = null;
			RemoveFromDatabase = true;
			//Reset text
			textBoxPackageAuthor.Text = Interface.GetInterfaceString("packages_selection_none");
			textBoxPackageName.Text = Interface.GetInterfaceString("packages_selection_none");
			textBoxPackageDescription.Text = Interface.GetInterfaceString("packages_selection_none");
			textBoxPackageVersion.Text = Interface.GetInterfaceString("packages_selection_none");
			buttonSelectPackage.Text = Interface.GetInterfaceString("packages_install_select");
			labelNewGUID.Text = Interface.GetInterfaceString("packages_creation_new_id");
			linkLabelPackageWebsite.Links.Clear();
			linkLabelPackageWebsite.Text = Interface.GetInterfaceString("packages_selection_none_website");
			LinkLabel.Link link = new LinkLabel.Link { LinkData = null };
			linkLabelPackageWebsite.Links.Add(link);
			//Reset the worker thread
			workerThread = null;
			workerThread = new BackgroundWorker();
		}	
	}
}