using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using OpenBveApi.Packages;
using System.Text;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

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
		private PackageOperation currentOperation = PackageOperation.None;
		private PackageType newPackageType;
		private string ImageFile;
		private BackgroundWorker workerThread = new BackgroundWorker();
		private bool RemoveFromDatabase = true;
		private Package dependantPackage;
		private List<string> selectedDependacies = new List<string>();
		private bool ProblemEncountered = false;
		private bool listPopulating;

		private void RefreshPackages()
		{
			if (Database.SaveDatabase() == false)
			{
				MessageBox.Show(Translations.GetInterfaceString("packages_database_save_error"));
			}

			string errorMessage;
			if (Database.LoadDatabase(currentDatabaseFolder, currentDatabaseFile, out errorMessage))
			{
				PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages, true, false, false);
				comboBoxPackageType.SelectedIndex = 0;
			}
			if (errorMessage != string.Empty)
			{
				MessageBox.Show(Translations.GetInterfaceString(errorMessage));
			}
			currentPackage = null;
		}

		private Package currentPackage;
		private Package oldPackage;
		private List<PackageFile> filesToPackage;

		private void buttonInstall_Click(object sender, EventArgs e)
		{
			if (currentOperation == PackageOperation.Creating)
			{
				if (textBoxPackageName.Text == Translations.GetInterfaceString("packages_selection_none"))
				{
					MessageBox.Show(Translations.GetInterfaceString("packages_creation_name"));
					return;
				}
				if (textBoxPackageAuthor.Text == Translations.GetInterfaceString("packages_selection_none"))
				{
					MessageBox.Show(Translations.GetInterfaceString("packages_creation_author"));
					return;
				}
				//LINK: Doesn't need checking

				if (textBoxPackageDescription.Text == Translations.GetInterfaceString("packages_selection_none"))
				{
					MessageBox.Show(Translations.GetInterfaceString("packages_creation_description"));
					return;
				}
				if (!Version.TryParse(textBoxPackageVersion.Text, out currentPackage.PackageVersion))
				{
					MessageBox.Show(Translations.GetInterfaceString("packages_creation_version"));
					return;
				}
				//Only set properties after making the checks

				
				if (Program.CurrentHost.MonoRuntime)
				{
					//HACK: Mono's WinForms implementation appears to be setting the encoding for a textbox to be Encoding.Default
					//as opposed to UTF-8 under Microsoft .Net 
					//Convert the string to a byte array first, then get the UTF-8 string from the arrays to make odd characters work
					byte[] byteArray = Encoding.Default.GetBytes(textBoxPackageName.Text);
					string s = Encoding.UTF8.GetString(byteArray);
					currentPackage.Name = s;
					byteArray = Encoding.Default.GetBytes(textBoxPackageAuthor.Text);
					s = Encoding.UTF8.GetString(byteArray);
					currentPackage.Author = s;
					byteArray = Encoding.Default.GetBytes(textBoxPackageDescription.Text);
					s = Encoding.UTF8.GetString(byteArray);
					currentPackage.Description = s.Replace("\r\n", "\\r\\n");
				}
				else
				{
					currentPackage.Name = textBoxPackageName.Text;
					currentPackage.Author = textBoxPackageAuthor.Text;
					currentPackage.Description = textBoxPackageDescription.Text.Replace("\r\n", "\\r\\n");
				}
				HidePanels();
				comboBoxDependancyType.SelectedIndex = 0;
				panelPackageDependsAdd.Show();
				PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages2, true, false, false);
				return;
			}

			List<Package> Dependancies = Database.checkDependsReccomends(currentPackage.Dependancies.ToList());
			if (Dependancies != null)
			{
				//We are missing a dependancy

				labelDependancyErrorHeader.Text = Translations.GetInterfaceString("packages_install_dependancies_unmet_header");
				labelMissingDependanciesText1.Text = Translations.GetInterfaceString("packages_install_dependancies_unmet");
				PopulatePackageList(Dependancies, dataGridViewDependancies, false, false, false);
				HidePanels();
				panelDependancyError.Show();
				return;
			}
			List<Package> Reccomendations = Database.checkDependsReccomends(currentPackage.Reccomendations.ToList());
			if (Reccomendations != null)
			{
				//We are missing a reccomendation

				labelDependancyErrorHeader.Text = Translations.GetInterfaceString("packages_install_reccomends_unmet_header");
				labelMissingDependanciesText1.Text = Translations.GetInterfaceString("packages_install_reccomends_unmet");
				PopulatePackageList(Reccomendations, dataGridViewDependancies, false, false, false);
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
						labelVersionError.Text = Translations.GetInterfaceString("packages_install_version_new");
						labelCurrentVersionNumber.Text = oldPackage.PackageVersion.ToString();
						break;
					case VersionInformation.SameVersion:
						labelVersionError.Text = Translations.GetInterfaceString("packages_install_version_same");
						labelCurrentVersionNumber.Text = currentPackage.PackageVersion.ToString();
						break;
					case VersionInformation.OlderVersion:
						labelVersionError.Text = Translations.GetInterfaceString("packages_install_version_old");
						labelCurrentVersionNumber.Text = oldPackage.PackageVersion.ToString();
						break;
				}
				labelNewVersionNumber.Text = currentPackage.PackageVersion.ToString();
				if (currentPackage.Dependancies.Count != 0)
				{
					List<Package> brokenDependancies = Information.UpgradeDowngradeDependancies(currentPackage,
						Database.currentDatabase.InstalledRoutes, Database.currentDatabase.InstalledTrains);
					if (brokenDependancies != null)
					{
						PopulatePackageList(brokenDependancies, dataGridViewBrokenDependancies, false, false, false);
					}
				}
				HidePanels();
				panelVersionError.Show();
			}
		}

		private void buttonSelectPackage_Click(object sender, EventArgs e)
		{
			if (openPackageFileDialog.ShowDialog() == DialogResult.OK)
			{
				currentPackage = Manipulation.ReadPackage(openPackageFileDialog.FileName);
				if (currentPackage != null)
				{
					buttonNext.Enabled = true;
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
						linkLabelPackageWebsite.Text = Translations.GetInterfaceString("packages_selection_none_website");
					}
					if (currentPackage.PackageImage != null)
					{
						pictureBoxPackageImage.Image = currentPackage.PackageImage;
					}
					else
					{
						TryLoadImage(pictureBoxPackageImage, currentPackage.PackageType == 0 ? "route_unknown.png" : "train_unknown.png");
					}
				}
				else
				{
					//ReadPackage returns null if the file is not a package.....

					MessageBox.Show(Translations.GetInterfaceString("packages_install_invalid"));
				}
			}
		}

		private void buttonInstallFinished_Click(object sender, EventArgs e)
		{
			if (currentOperation != PackageOperation.Creating)
			{
				RefreshPackages();
			}
			ResetInstallerPanels();
		}

		internal static readonly string currentDatabaseFolder =
			OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase");

		private static readonly string currentDatabaseFile = OpenBveApi.Path.CombineFile(currentDatabaseFolder, "packages.xml");

		private void buttonProceedAnyway_Click(object sender, EventArgs e)
		{
			panelDependancyError.Hide();
			if (currentOperation == PackageOperation.Uninstalling)
			{
				UninstallPackage(currentPackage, true);
			}
			else
			{
				labelMissingDependanciesText1.Text = Translations.GetInterfaceString("packages_install_dependancies_unmet");
				Extract(oldPackage);
			}
			
		}

		private void Extract(Package packageToReplace = null)
		{
			ProblemEncountered = false;
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
					case PackageType.Loksim3D:
						ExtractionDirectory = Program.FileSystem.LoksimPackageInstallationDirectory;
						break;
					default:
						ExtractionDirectory = Program.FileSystem.OtherInstallationDirectory;
						break;
				}
				string PackageFiles = "";
				Manipulation.ExtractPackage(currentPackage, ExtractionDirectory, currentDatabaseFolder, ref PackageFiles);
				if (ProblemEncountered == false && PackageFiles != string.Empty)
				{
					textBoxFilesInstalled.Invoke((MethodInvoker) delegate
					{
						textBoxFilesInstalled.Text = PackageFiles;
					});
				}
			};
			workerThread.RunWorkerCompleted += delegate
			{
				if (!ProblemEncountered)
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
						default:
							if (packageToReplace != null)
							{
								for (int i = Database.currentDatabase.InstalledOther.Count; i > 0; i--)
								{
									if (Database.currentDatabase.InstalledOther[i - 1].GUID == currentPackage.GUID)
									{
										Database.currentDatabase.InstalledOther.Remove(Database.currentDatabase.InstalledOther[i - 1]);
									}
								}
							}
							Database.currentDatabase.InstalledOther.Add(currentPackage);
							break;
					}
					Database.currentDatabase.AddDependancies(currentPackage);
					labelInstallSuccess1.Text = Translations.GetInterfaceString("packages_install_success");
					labelInstallSuccess2.Text = Translations.GetInterfaceString("packages_install_success_header");
					labelListFilesInstalled.Text = Translations.GetInterfaceString("packages_install_success_files");
				}
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
				this.BeginInvoke((MethodInvoker) delegate
				{
					OnWorkerProgressChanged(sender, e);
				});
				return;
			}

			//Actually change the controls text
			labelProgressPercent.Text = e.Progress + @"%";
			labelProgressFile.Text = e.CurrentFile;
		}

		private void OnWorkerReportsProblem(object sender, ProblemReport e)
		{
			//We need to invoke the control so we don't get a cross thread exception
			if (this.InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker) delegate
				{
					OnWorkerReportsProblem(sender, e);
				});
				return;
			}
			//Update the text, but don't change the tab- Do this when the worker terminates
			ProblemEncountered = true;
			if (currentOperation != PackageOperation.Creating)
			{
				labelInstallSuccess1.Text = Translations.GetInterfaceString("packages_install_failure");
				labelInstallSuccess2.Text = Translations.GetInterfaceString("packages_install_failure_header");
			}
			else
			{
				labelInstallSuccess1.Text = Translations.GetInterfaceString("packages_creation_failure");
				labelInstallSuccess2.Text = Translations.GetInterfaceString("packages_creation_failure_header");
			}
			labelListFilesInstalled.Text = Translations.GetInterfaceString("packages_creation_failure_error");
			buttonInstallFinish.Text = Translations.GetInterfaceString("packages_success");
			if (e.Exception is UnauthorizedAccessException && currentOperation != PackageOperation.Creating)
			{
				//User attempted to install in a directory which requires UAC access
				textBoxFilesInstalled.Text = e.Exception.Message + Environment.NewLine + Environment.NewLine + Translations.GetInterfaceString("errors_security_checkaccess");
				if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
				{
					textBoxFilesInstalled.Text+= Environment.NewLine + Environment.NewLine + Translations.GetInterfaceString("errors_security_badlocation");
				}
			}
			else
			{
				//Non-localised string as this is a specific error message
				textBoxFilesInstalled.Text = e.Exception + @"\r\n \r\n encountered whilst processing the following file: \r\n\r\n" +
				                             e.CurrentFile + @" at " + e.Progress + @"% completion.";
				//Create crash dump file
				CrashHandler.LogCrash(e.Exception + Environment.StackTrace);
			}
		}

		/// <summary>This method should be called to populate a datagrid view with a list of packages</summary>
		/// <param name="packageList">The list of packages</param>
		/// <param name="dataGrid">The datagrid view to populate</param>
		/// <param name="simpleList">Whether this is a simple list or a dependancy list</param>
		/// <param name="isDependancy">Whether this is a package needed by another or not</param>
		/// <param name="isRecommendation">Whether this is a package recommended by another or not</param>
		internal void PopulatePackageList(List<Package> packageList, DataGridView dataGrid, bool simpleList, bool isDependancy,
			bool isRecommendation)
		{
			listPopulating = true;
			if (this.InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker) delegate
				{
					PopulatePackageList(packageList, dataGrid, simpleList, isDependancy, isRecommendation);
				});
				return;
			}
			//Clear the package list
			// HACK: If we are working with recommendations, do not clear the list to show them together with dependancies
			if (!isRecommendation) dataGrid.Rows.Clear();
			//Add the key column programatically if required
			if (dataGrid.Columns[dataGrid.ColumnCount - 1].Name != "key")
			{
				dataGrid.Columns.Add("key", "key");
				dataGrid.Columns[dataGrid.ColumnCount - 1].Visible = false;
			}
			//We have route packages in our list!
			for (int i = 0; i < packageList.Count; i++)
			{
				//Create row
				object[] packageToAdd;
				if (!simpleList)
				{
					if (isDependancy)
					{
						packageToAdd = new object[]
						{
							packageList[i].Name, packageList[i].MinimumVersion, packageList[i].MaximumVersion,
							packageList[i].PackageType.ToString(),
							Translations.GetInterfaceString("packages_dependancy"), packageList[i].GUID
						};
					}
					else if (isRecommendation)
					{
						packageToAdd = new object[]
						{
							packageList[i].Name, packageList[i].MinimumVersion, packageList[i].MaximumVersion,
							packageList[i].PackageType.ToString(),
							Translations.GetInterfaceString("packages_recommendation"), packageList[i].GUID
						};
					}
					else
					{
						packageToAdd = new object[]
						{
							packageList[i].Name, packageList[i].MinimumVersion, packageList[i].MaximumVersion, packageList[i].Author,
							packageList[i].Website, packageList[i].GUID
						};
					}
				}
				else
				{
					packageToAdd = new object[]
					{
						packageList[i].Name, packageList[i].Version, packageList[i].Author, packageList[i].Website, packageList[i].GUID
					};
				}

				//Add to the datagrid view if not selected as dependancy or recommendation
				if (!selectedDependacies.Contains(packageList[i].GUID) || isDependancy || isRecommendation)
				{
					try
					{
						dataGrid.Rows.Add(packageToAdd);
					}
					catch (Exception ex)
					{
						CrashHandler.LogCrash(ex.ToString());
					}
				}
			}
			dataGrid.ClearSelection();
			listPopulating = false;
		}

		/// <summary>This method should be called to uninstall a package</summary>
		internal void UninstallPackage(Package packageToUninstall, bool force = false)
		{
			currentOperation = PackageOperation.Uninstalling;
			List<Package> Packages;
			switch (packageToUninstall.PackageType)
			{
				case PackageType.Route:
					Packages = Database.currentDatabase.InstalledRoutes;
					break;
				case PackageType.Train:
					Packages = Database.currentDatabase.InstalledTrains;
					break;
				case PackageType.Other:
					Packages = Database.currentDatabase.InstalledOther;
					break;
				default:
					throw new Exception("Unknown package type");
			}
			string uninstallResults = "";
			List<Package> brokenDependancies = Database.CheckUninstallDependancies(packageToUninstall.DependantPackages);
			if (brokenDependancies.Count != 0 && force == false)
			{
				PopulatePackageList(brokenDependancies, dataGridViewDependancies, false, false, false);
				labelMissingDependanciesText1.Text = Translations.GetInterfaceString("packages_uninstall_broken");
				HidePanels();
				panelDependancyError.Show();
				return;
			}
			if (Manipulation.UninstallPackage(packageToUninstall, currentDatabaseFolder, ref uninstallResults))
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
				labelUninstallSuccess.Text = Translations.GetInterfaceString("packages_uninstall_success");
				labelUninstallSuccessHeader.Text = Translations.GetInterfaceString("packages_uninstall_success_header");
				textBoxUninstallResult.Text = uninstallResults;
				HidePanels();
				panelUninstallResult.Show();
			}
			else
			{
				labelUninstallSuccess.Text = Translations.GetInterfaceString("packages_uninstall_success");
				labelUninstallSuccessHeader.Text = Translations.GetInterfaceString("packages_uninstall_success_header");
				if (uninstallResults == null)
				{
					//Uninstall requires an XML list of files, and these were missing.......
					textBoxUninstallResult.Text = Translations.GetInterfaceString("packages_uninstall_missing_xml");
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
			currentOperation = PackageOperation.None;
		}



		internal void AddDependendsReccomends(Package packageToAdd, ref List<Package> DependsReccomendsList,
			bool recommendsOnly)
		{
			try
			{
				var row = dataGridViewPackages2.SelectedRows[0].Index;
				var key = dataGridViewPackages2.Rows[row].Cells[dataGridViewPackages2.ColumnCount - 1].Value.ToString();
				selectedDependacies.Add(key);
				dataGridViewPackages2.Rows.RemoveAt(row);
				if (DependsReccomendsList == null)
				{
					DependsReccomendsList = new List<Package>();
				}
				packageToAdd.PackageVersion = null;
				DependsReccomendsList.Add(packageToAdd);
				dataGridViewPackages2.ClearSelection();
				if (currentPackage.Dependancies != null)
					PopulatePackageList(currentPackage.Dependancies, dataGridViewPackages3, false, true, false);
				if (currentPackage.Reccomendations != null)
					PopulatePackageList(currentPackage.Reccomendations, dataGridViewPackages3, false, false, true);
			}
			catch
			{		
				//Ignored at the minute
			}
		}




		private void dataGridViewPackages_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewPackages.SelectedRows.Count == 0 || listPopulating)
			{
				currentPackage = null;
				return;
			}
			try
			{
				var row = dataGridViewPackages.SelectedRows[0].Index;
				var key = dataGridViewPackages.Rows[row].Cells[dataGridViewPackages.ColumnCount - 1].Value.ToString();

				switch (comboBoxPackageType.SelectedIndex)
				{
					case 0:
						currentPackage = Database.currentDatabase.InstalledRoutes.FirstOrDefault(x => x.GUID == key);
						break;
					case 1:
						currentPackage = Database.currentDatabase.InstalledTrains.FirstOrDefault(x => x.GUID == key);
						break;
					case 2:
						currentPackage = Database.currentDatabase.InstalledOther.FirstOrDefault(x => x.GUID == key);
						break;
				}
			}
			catch
			{
				currentPackage = null;
			}
		}

		private void buttonUninstallPackage_Click(object sender, EventArgs e)
		{
			if (currentPackage != null)
			{
				switch (currentPackage.PackageType)
				{
					case PackageType.Route:
						UninstallPackage(currentPackage);
						break;
					case PackageType.Train:
						UninstallPackage(currentPackage);
						break;
					case PackageType.Other:
					case PackageType.Loksim3D:
						UninstallPackage(currentPackage);
						break;
				}
			}
			else
			{
				MessageBox.Show(Translations.GetInterfaceString("packages_selection_none"));
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
					if (MessageBox.Show(Translations.GetInterfaceString("packages_uninstall_database_remove"), Translations.GetInterfaceString("program_title"), MessageBoxButtons.YesNo) == DialogResult.Yes)
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
			labelInstallText.Text = Translations.GetInterfaceString("packages_install_header");
			buttonBack2.Text = Translations.GetInterfaceString("packages_button_cancel");
			TryLoadImage(pictureBoxPackageImage, "route_error.png");
			HidePanels();
			panelPackageInstall.Show();
		}

		private void buttonDepends_Click(object sender, EventArgs e)
		{
			//dependantPackage.Version = null;
			switch (dependantPackage.PackageType)
			{
				case PackageType.Route:
					if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion, dependantPackage.Version, Translations.GetInterfaceString("packages_creation_dependancies_add")) == DialogResult.OK)
					{
						AddDependendsReccomends(dependantPackage, ref currentPackage.Dependancies, false);
						dependantPackage = null;
					}
					break;
				case PackageType.Train:
					if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion, dependantPackage.Version, Translations.GetInterfaceString("packages_creation_dependancies_add")) == DialogResult.OK)
					{
						AddDependendsReccomends(dependantPackage, ref currentPackage.Dependancies, false);
						dependantPackage = null;
					}
					break;
				case PackageType.Other:
					if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion, dependantPackage.Version, Translations.GetInterfaceString("packages_creation_dependancies_add")) == DialogResult.OK)
					{
						AddDependendsReccomends(dependantPackage, ref currentPackage.Dependancies, false);
						dependantPackage = null;
					}
					break;
			}
		}

		private void buttonReccomends_Click(object sender, EventArgs e)
		{
			switch (dependantPackage.PackageType)
			{
				case PackageType.Route:
					if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion, dependantPackage.Version, Translations.GetInterfaceString("packages_creation_reccomends_add")) == DialogResult.OK)
					{
						AddDependendsReccomends(dependantPackage, ref currentPackage.Reccomendations, true);
						dependantPackage = null;
					}
					break;
				case PackageType.Train:
					if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion, dependantPackage.Version, Translations.GetInterfaceString("packages_creation_reccomends_add")) == DialogResult.OK)
					{
						AddDependendsReccomends(dependantPackage, ref currentPackage.Reccomendations, true);
						dependantPackage = null;
					}
					break;
				case PackageType.Other:
					if (ShowVersionDialog(ref dependantPackage.MinimumVersion, ref dependantPackage.MaximumVersion, dependantPackage.Version, Translations.GetInterfaceString("packages_creation_reccomends_add")) == DialogResult.OK)
					{
						AddDependendsReccomends(dependantPackage, ref currentPackage.Reccomendations, true);
						dependantPackage = null;
					}
					break;
			}
		}

		private void dataGridViewPackages2_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewPackages2.SelectedRows.Count == 0 || listPopulating)
			{
				buttonDepends.Enabled = false;
				buttonReccomends.Enabled = false;
				dependantPackage = null;
				return;
			}
			buttonDepends.Enabled = true;
			buttonReccomends.Enabled = true;
			var row = dataGridViewPackages2.SelectedRows[0].Index;
			try
			{
				var key = dataGridViewPackages2.Rows[row].Cells[dataGridViewPackages2.ColumnCount - 1].Value.ToString();
				switch (comboBoxDependancyType.SelectedIndex)
				{
					case 0:
						dependantPackage = new Package(Database.currentDatabase.InstalledRoutes.FirstOrDefault(x => x.GUID == key), true);
						break;
					case 1:
						dependantPackage = new Package(Database.currentDatabase.InstalledTrains.FirstOrDefault(x => x.GUID == key), true);
						break;
					case 2:
						dependantPackage = new Package(Database.currentDatabase.InstalledOther.FirstOrDefault(x => x.GUID == key), true);
						break;
				}
			}
			catch
			{
				buttonDepends.Enabled = false;
				buttonReccomends.Enabled = false;
				dependantPackage = null;
			}
		}

		private void dataGridViewPackages3_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewPackages3.SelectedRows.Count == 0 || listPopulating)
			{
				buttonRemove.Enabled = false;
				return;
			}
			buttonRemove.Enabled = true;
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			try
			{
				var row = dataGridViewPackages3.SelectedRows[0].Index;
				var key = dataGridViewPackages3.Rows[row].Cells[dataGridViewPackages3.ColumnCount - 1].Value.ToString();
				selectedDependacies.Remove(key);

				if (dataGridViewPackages3.Rows[row].Cells[dataGridViewPackages3.ColumnCount - 2].Value.ToString() == Translations.GetInterfaceString("packages_dependancy"))
				{
					currentPackage.Dependancies.Remove(currentPackage.Dependancies.FirstOrDefault(x => x.GUID == key));
				}
				else
				{
					currentPackage.Reccomendations.Remove(currentPackage.Reccomendations.FirstOrDefault(x => x.GUID == key));
				}
				dataGridViewPackages3.Rows.RemoveAt(row);
				dataGridViewPackages3.ClearSelection();
				switch (comboBoxDependancyType.SelectedIndex)
				{
					case 0:
						PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages2, true, false, false);
						break;
					case 1:
						PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewPackages2, true, false, false);
						break;
					case 2:
						PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewPackages2, true, false, false);
						break;
				}
			}
			catch
			{
				//Ignored at the minute
			}
		}


		private void buttonCreatePackage_Click(object sender, EventArgs e)
		{
			var directory = Path.GetDirectoryName(currentPackage.FileName);
			try
			{
				if (directory == null)
				{
					throw new DirectoryNotFoundException();
				}
				if (!Directory.Exists(directory))
				{
					throw new DirectoryNotFoundException(directory);
				}
				if (!Program.CurrentHost.MonoRuntime)
				{
					//Mono doesn't support System.Security.AccessControl, so this doesn't work....
					Directory.GetAccessControl(directory);
				}
				// ReSharper disable once UnusedVariable
				using (FileStream fs = File.OpenWrite(currentPackage.FileName))
				{
					//Just attempt to open the file with to write as a test
				}
			}
			catch (Exception ex)
			{
				if (ex is DirectoryNotFoundException)
				{
					//We didn't find the directory
					//In theory this shouldn't happen, but handle it just in case
					if (ex.Message == string.Empty)
					{
						//Our filename is blank
						MessageBox.Show(Translations.GetInterfaceString("packages_filename_empty"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					else
					{
						//Directory doesn't exist
						MessageBox.Show(Translations.GetInterfaceString("packages_directory_missing") + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}

				}
				else if (ex is UnauthorizedAccessException)
				{
					//No permissions from access control
					MessageBox.Show(Translations.GetInterfaceString("packages_directory_nowrite") + directory, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				else
				{
					//Generic error
					MessageBox.Show(Translations.GetInterfaceString("packages_file_generic") + currentPackage.FileName, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				return;
			}
			HidePanels();
			panelPleaseWait.Show();
			ProblemEncountered = false;
			workerThread.DoWork += delegate
			{
				Manipulation.CreatePackage(currentPackage, Interface.CurrentOptions.packageCompressionType, currentPackage.FileName, ImageFile, filesToPackage);
			};

			workerThread.RunWorkerCompleted += delegate {
				if (ProblemEncountered == false)
				{
					string text = "";
					if (filesToPackage != null && filesToPackage.Count > 0)
					{
						for (int i = 0; i < filesToPackage.Count; i++)
						{
							text += filesToPackage[i].relativePath + "\r\n";
						}
					}
					textBoxFilesInstalled.Text = text;
					labelInstallSuccess1.Text = Translations.GetInterfaceString("packages_creation_success");
					labelInstallSuccess2.Text = Translations.GetInterfaceString("packages_creation_success_header");
					labelListFilesInstalled.Text = Translations.GetInterfaceString("packages_creation_success_files");
					buttonInstallFinish.Text = Translations.GetInterfaceString("packages_success");
				}
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

			if (filesToPackage == null || filesToPackage.Count == 0)
			{
				MessageBox.Show(Translations.GetInterfaceString("packages_creation_invalid_nofiles"));
				return;
			}
			currentPackage.FileName = textBoxPackageFileName.Text;
			FileInfo fi = null;
			try
			{
				fi = new FileInfo(currentPackage.FileName);
			}
			catch
			{
				//Checked below
			}
			if (fi == null)
			{
				//The supplied filename was invalid
				MessageBox.Show(Translations.GetInterfaceString("packages_creation_invalid_filename"));
				return;
			}
			try
			{
				File.Delete(currentPackage.FileName);
			}
			catch
			{
				//The file is locked or otherwise unavailable
				MessageBox.Show(Translations.GetInterfaceString("packages_creation_invalid_filename"));
				return;
			}
			buttonSelectPackage.Text = Translations.GetInterfaceString("packages_creation_proceed");
			currentOperation = PackageOperation.Creating;
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
			labelInstallText.Text = Translations.GetInterfaceString("packages_creation_header");
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
			buttonSelectPackage.Visible = false;
			buttonNext.Enabled = true;
			panelCreatePackage.Show();
		}

		private void pictureBoxPackageImage_Click(object sender, EventArgs e)
		{
			if (currentOperation == PackageOperation.Creating)
			{
				if (openPackageFileDialog.ShowDialog() == DialogResult.OK)
				{
					ImageFile = openPackageFileDialog.FileName;
					try
					{
						pictureBoxPackageImage.Image = ImageExtensions.FromFile(ImageFile);
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
			filesToPackageBox.Text = string.Empty;
			if (radioButtonQ1Yes.Checked)
			{
				if (Database.currentDatabase.InstalledRoutes.Count == 0 && Database.currentDatabase.InstalledTrains.Count == 0 && Database.currentDatabase.InstalledOther.Count == 0)
				{
					//There are no packages available to replace....
					string test = Translations.GetInterfaceString("packages_replace_noneavailable");
					MessageBox.Show(test);
					radioButtonQ1No.Checked = true;
					return;
				}
				panelReplacePackage.Show();
				panelNewPackage.Hide();
				switch (newPackageType)
				{
					case PackageType.Route:
						PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewReplacePackage, true, false, false);
						break;
					case PackageType.Train:
						PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewReplacePackage, true, false, false);
						break;
					case PackageType.Other:
						PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewReplacePackage, true, false, false);
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
			if (radioButtonQ1Yes.Checked)
			{
				panelReplacePackage.Show();
				panelNewPackage.Hide();
				switch (newPackageType)
				{
					case PackageType.Route:
						PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewReplacePackage, true, false, false);
						break;
					case PackageType.Train:
						PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewReplacePackage, true, false, false);
						break;
					case PackageType.Other:
						PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewReplacePackage, true, false, false);
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
					panelNewPackage.Enabled = true;
				}
			}
		}


		private void dataGridViewReplacePackage_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewReplacePackage.SelectedRows.Count > 0 || listPopulating != true)
			{
				replacePackageButton.Enabled = true;
				var row = dataGridViewReplacePackage.SelectedRows[0].Index;
				string key;
				try
				{
					//BUG:
					//Mono seems to have some sort of race condition, meaning that the update function is called before the datagrid is populated
					//This masks the underlying issue, but it's not a good thing if the key is invalid in the first place either.....
					key = dataGridViewReplacePackage.Rows[row].Cells[dataGridViewReplacePackage.ColumnCount - 1].Value.ToString();
				}
				catch
				{
					return;
				}
				
				switch (newPackageType)
				{
					case PackageType.Route:
						currentPackage = new Package(Database.currentDatabase.InstalledRoutes.FirstOrDefault(x => x.GUID == key), false)
						{
							PackageType = PackageType.Route
						};
						break;
					case PackageType.Train:
						currentPackage = new Package(Database.currentDatabase.InstalledTrains.FirstOrDefault(x => x.GUID == key), false)
						{
							PackageType = PackageType.Train
						};
						break;
					case PackageType.Other:
						currentPackage = new Package(Database.currentDatabase.InstalledOther.FirstOrDefault(x => x.GUID == key), false)
						{
							PackageType = PackageType.Other
						};
						break;
				}
			}
			else replacePackageButton.Enabled = false;
		}

		private void linkLabelPackageWebsite_Click(object sender, EventArgs e)
		{
			if (currentOperation == PackageOperation.Creating)
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
					string link = currentPackage.Website.ToLowerInvariant();
					if (!link.StartsWith("http://") && !link.StartsWith("https://"))
					{
						launchLink += "http://";
					}
					launchLink += currentPackage.Website;
					Uri URL;
					bool result = Uri.TryCreate(launchLink, UriKind.Absolute, out URL) && (URL.Scheme == Uri.UriSchemeHttp || URL.Scheme == Uri.UriSchemeHttps);
					if (result)
					{
						Process.Start(launchLink);
					}
				}
			}
		}

		/// <summary>Shows a popup text input box to add a website link</summary>
		private static DialogResult ShowInputDialog(ref string input)
		{
			Size size = new Size(200, 70);
			Form inputBox = new Form
			{
				FormBorderStyle = FormBorderStyle.FixedDialog,
				MaximizeBox = false,
				MinimizeBox = false,
				StartPosition = FormStartPosition.CenterScreen,
				ClientSize = size,
				Text = Translations.GetInterfaceString("packages_list_website")
			};
			TextBox textBox = new TextBox
			{
				Size = new Size(size.Width - 10, 23),
				Location = new Point(5, 5),
				Text = input
			};
			inputBox.Controls.Add(textBox);

			Button okButton = new Button
			{
				DialogResult = DialogResult.OK,
				Name = "okButton",
				Size = new Size(75, 23),
				Text = Translations.GetInterfaceString("packages_button_ok"),
				Location = new Point(size.Width - 80 - 80, 39)
			};
			inputBox.Controls.Add(okButton);

			Button cancelButton = new Button
			{
				DialogResult = DialogResult.Cancel,
				Name = "cancelButton",
				Size = new Size(75, 23),
				Text = Translations.GetInterfaceString("packages_button_cancel"),
				Location = new Point(size.Width - 80, 39)
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

		private static DialogResult ShowVersionDialog(ref Version minimumVersion, ref Version maximumVersion, string currentVersion, string label)
		{
			Size size = new Size(300, 80);
			
			Form inputBox = new Form
			{
				FormBorderStyle = FormBorderStyle.FixedDialog,
				MaximizeBox = false,
				MinimizeBox = false,
				StartPosition = FormStartPosition.CenterScreen,
				ClientSize = size,
				Text = Translations.GetInterfaceString(label),
			};

			Label minLabel = new Label
			{
				Text = Translations.GetInterfaceString("packages_list_minimum"),
				Location = new Point(5, 6),
			};
			inputBox.Controls.Add(minLabel);

			TextBox textBox = new TextBox
			{
				Size = new Size(size.Width - 110, 23),
				Location = new Point(105, 5),
				Text = currentVersion
			};
			inputBox.Controls.Add(textBox);

			Label maxLabel = new Label
			{
				Text = Translations.GetInterfaceString("packages_list_maximum"),
				Location = new Point(5, 26),
			};
			inputBox.Controls.Add(maxLabel);

			TextBox textBox2 = new TextBox
			{
				Size = new Size(size.Width - 110, 23),
				Location = new Point(105, 25),
				Text = currentVersion
			};
			inputBox.Controls.Add(textBox2);

			Button okButton = new Button
			{
				DialogResult = DialogResult.OK,
				Name = "okButton",
				Size = new Size(75, 23),
				Text = Translations.GetInterfaceString("packages_button_ok"),
				Location = new Point(size.Width - 80 - 80, 49)
			};
			inputBox.Controls.Add(okButton);

			Button cancelButton = new Button
			{
				DialogResult = DialogResult.Cancel,
				Name = "cancelButton",
				Size = new Size(75, 23),
				Text = Translations.GetInterfaceString("packages_button_cancel"),
				Location = new Point(size.Width - 80, 49)
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
					maximumVersion = null;
				}
				else
				{
					maximumVersion = Version.Parse(textBox2.Text);
				}
			}
			catch
			{
				MessageBox.Show(Translations.GetInterfaceString("packages_creation_version_invalid"));
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
			bool DialogOK = false;
			string[] files = null;
			string folder = String.Empty;
			string folderDisplay = String.Empty;
			if (OpenTK.Configuration.RunningOnMacOS || OpenTK.Configuration.RunningOnLinux)
			{
				//Mono doesn't like our fancy folder selector
				//Some versions of OS-X crash, and Linux just falls back- Safer to specifically use the old version on these...
				var MonoDialog = new FolderBrowserDialog();
				if (MonoDialog.ShowDialog() == DialogResult.OK)
				{
					folder = Directory.GetParent(MonoDialog.SelectedPath).ToString();
					folderDisplay = MonoDialog.SelectedPath;
					files = Directory.GetFiles(folderDisplay, "*.*", SearchOption.AllDirectories);
					DialogOK = true;
				}
			}
			else
			{
				//Use the fancy folder selector dialog on Windows
				var dialog = new FolderSelectDialog();
				if (dialog.Show(Handle))
				{
					DialogOK = true;
					folder = Directory.GetParent(dialog.FileName).ToString();
					folderDisplay = dialog.FileName;
					files = Directory.GetFiles(dialog.FileName, "*.*", SearchOption.AllDirectories);
				}

			}

			if (DialogOK && files.Length != 0)
			{

				filesToPackageBox.Text += folderDisplay + Environment.NewLine;
				var tempList = new List<PackageFile>();
				for (int i = 0; i < files.Length; i++)
				{
					var File = new PackageFile
					{
						absolutePath = files[i],
						relativePath = files[i].Replace(folder, ""),
					};
					tempList.Add(File);
				}
				if (filesToPackage == null)
				{
					filesToPackage = new List<PackageFile>();
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
			labelNewGUID.Text = Translations.GetInterfaceString("packages_creation_replace_id");
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
				Title = Translations.GetInterfaceString("packages_creation_save"),
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
					PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages, true, false, false);
					break;
				case 1:
					PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewPackages, true, false, false);
					break;
				case 2:
					PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewPackages, true, false, false);
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
					PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages2, true, false, false);
					break;
				case 1:
					PopulatePackageList(Database.currentDatabase.InstalledTrains, dataGridViewPackages2, true, false, false);
					break;
				case 2:
					PopulatePackageList(Database.currentDatabase.InstalledOther, dataGridViewPackages2, true, false, false);
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
						break;
					case PackageType.Train:
						for (int i = Database.currentDatabase.InstalledTrains.Count - 1; i >= 0; i--)
						{
							if (Database.currentDatabase.InstalledTrains[i].GUID == currentPackage.GUID)
							{
								Database.currentDatabase.InstalledTrains.RemoveAt(i);
							}
						}
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
			currentOperation = PackageOperation.None;
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
			currentOperation = PackageOperation.None;
			currentPackage = null;
			dependantPackage = null;
			newPackageType = PackageType.NotFound;
			ImageFile = null;
			RemoveFromDatabase = true;
			selectedDependacies = new List<string>();
			filesToPackage = null;
			//Reset package lists
			dataGridViewPackages2.Rows.Clear();
			dataGridViewPackages3.Rows.Clear();
			//Reset text
			textBoxPackageAuthor.Text = Translations.GetInterfaceString("packages_selection_none");
			textBoxPackageName.Text = Translations.GetInterfaceString("packages_selection_none");
			textBoxPackageDescription.Text = Translations.GetInterfaceString("packages_selection_none");
			textBoxPackageVersion.Text = Translations.GetInterfaceString("packages_selection_none");
			buttonSelectPackage.Text = Translations.GetInterfaceString("packages_install_select");
			labelNewGUID.Text = Translations.GetInterfaceString("packages_creation_new_id");
			linkLabelPackageWebsite.Links.Clear();
			linkLabelPackageWebsite.Text = Translations.GetInterfaceString("packages_selection_none_website");
			LinkLabel.Link link = new LinkLabel.Link { LinkData = null };
			linkLabelPackageWebsite.Links.Add(link);
			buttonBack2.Text = Translations.GetInterfaceString("packages_button_back");
			buttonNext.Enabled = false;
			buttonSelectPackage.Visible = true;
			//Reset the worker thread
			while (workerThread.IsBusy)
			{
				Thread.Sleep(10);
			}
			workerThread = null;
			workerThread = new BackgroundWorker();
		}	


		private void buttonBack_Click(object sender, EventArgs e)
		{
			HidePanels();
			panelPackageInstall.Show();
		}

		private void buttonBack2_Click(object sender, EventArgs e)
		{
			if (currentOperation != PackageOperation.Creating)
			{
				ResetInstallerPanels();
			}
			else
			{
				HidePanels();
				panelCreatePackage.Show();
			}
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			ResetInstallerPanels();
		}
	}
}
