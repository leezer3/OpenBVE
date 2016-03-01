using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
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
        internal static List<Package> InstalledRoutes = new List<Package>();
        internal static List<Package> InstalledTrains = new List<Package>();
        internal int selectedTrainPackageIndex = 0;
        internal int selectedRoutePackageIndex = 0;

        internal void RefreshPackages()
        {
            SavePackages();
            LoadRoutePackages();
            PopulatePackageList();
        }

        private Package currentPackage;
        private Package oldPackage;

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            //Check to see if the package is null- If null, then we haven't loaded a package yet
            if (currentPackage == null)
            {
                if (openPackageFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentPackage = OpenBveApi.Packages.Manipulation.ReadPackage(openPackageFileDialog.FileName);
                    if (currentPackage != null)
                    {
                        textBoxPackageName.Text = currentPackage.Name;
                        textBoxPackageAuthor.Text = currentPackage.Author;
                        textBoxPackageVersion.Text = currentPackage.PackageVersion.ToString();
                        if (currentPackage.Website != null)
                        {
                            linkLabelPackageWebsite.Text = currentPackage.Website;
                            LinkLabel.Link link = new LinkLabel.Link();
                            link.LinkData = currentPackage.Website;
                            linkLabelPackageWebsite.Links.Add(link);
                        }
                        else
                        {
                            linkLabelPackageWebsite.Text = "No website provided.";
                        }
                        if (currentPackage.PackageImage != null)
                        {
                            pictureBoxPackageImage.Image = currentPackage.PackageImage;
                        }
                        else
                        {
                            TryLoadImage(pictureBoxPackageImage, currentPackage.PackageType == 0 ? "route_unknown.png" : "train_unknown.png");
                        }
                        buttonSelectPackage.Text = "Install";
                    }
                }

            }
            else
            {
                List<Package> Dependancies = Information.CheckDependancies(currentPackage, InstalledRoutes, InstalledTrains);
                if (Dependancies != null)
                {
                    //We are missing a dependancy
                    PopulateDependancyList(Dependancies, dataGridViewDependancies);
                    panelPackageInstall.Hide();
                    panelDependancyError.Show();
                    return;
                }
                VersionInformation Info;
                oldPackage = null;
                switch (currentPackage.PackageType)
                {
                    case PackageType.Route:
                        Info = Information.CheckVersion(currentPackage, formMain.InstalledRoutes, ref oldPackage);
                        break;
                    case PackageType.Train:
                        Info = Information.CheckVersion(currentPackage, formMain.InstalledTrains, ref oldPackage);
                        break;
                    default:
                        Info = Information.CheckVersion(currentPackage, formMain.InstalledTrains, ref oldPackage);
                        //TODO: Show appropriate error message....
                        //The current info is temp, as otherwise Info may not be initialised before access
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
                            labelVersionError.Text = "The selected package is already installed, and is a newer version.";
                            textBoxCurrentVersion.Text = oldPackage.PackageVersion.ToString();
                            break;
                        case VersionInformation.SameVersion:
                            labelVersionError.Text = "The selected package is already installed, and is an identical version.";
                            textBoxCurrentVersion.Text = currentPackage.PackageVersion.ToString();
                            break;
                        case VersionInformation.OlderVersion:
                            labelVersionError.Text = "The selected package is already installed, and is an older version.";
                            textBoxCurrentVersion.Text = oldPackage.PackageVersion.ToString();
                            break;
                    }
                    textBoxNewVersion.Text = currentPackage.PackageVersion.ToString();
                    if (currentPackage.Dependancies.Count != 0)
                    {
                        List<Package> brokenDependancies = OpenBveApi.Packages.Information.UpgradeDowngradeDependancies(currentPackage, InstalledRoutes, InstalledTrains);
                        if (brokenDependancies != null)
                        {
                            PopulateDependancyList(brokenDependancies, dataGridViewBrokenDependancies);
                        }
                    }
                    panelDependancyError.Hide();
                    panelSuccess.Hide();
                    panelPackageInstall.Hide();
                    panelVersionError.Show();
                }
            }
                
        }

        private void buttonInstallFinished_Click(object sender, EventArgs e)
        {
            RefreshPackages();
            panelSuccess.Hide();
            panelPackageList.Show();
        }

        private static readonly string PackageDatabase = OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase");
        private static readonly string packageDatabaseFile = OpenBveApi.Path.CombineFile(PackageDatabase, "packages.xml");

        private void buttonProceedAnyway_Click(object sender, EventArgs e)
        {
            Extract(oldPackage);
        }

        private void Extract(Package packageToReplace = null)
        {
            string ExtractionDirectory;
            switch (currentPackage.PackageType)
            {
                case PackageType.Route:
                    ExtractionDirectory = Program.FileSystem.InitialRailwayFolder;
                    break;
                case PackageType.Train:
                default:
                    ExtractionDirectory = Program.FileSystem.InitialTrainFolder;
                    break;
            }
            string PackageFiles = "";
            Manipulation.ExtractPackage(currentPackage, ExtractionDirectory, PackageDatabase, ref PackageFiles);
            switch (currentPackage.PackageType)
            {
                case PackageType.Route:
                    if (packageToReplace != null)
                    {
                        for (int i = InstalledRoutes.Count; i > 0; i--)
                        {
                            if (InstalledRoutes[i -1].GUID == currentPackage.GUID)
                            {
                                InstalledRoutes.Remove(InstalledRoutes[i -1]);
                            }
                        }
                    }
                    formMain.InstalledRoutes.Add(currentPackage);
                    break;
                case PackageType.Train:
                    if (packageToReplace != null)
                    {
                        for (int i = InstalledTrains.Count; i > 0; i--)
                        {
                            if (InstalledTrains[i -1].GUID == currentPackage.GUID)
                            {
                                InstalledTrains.Remove(InstalledTrains[i -1]);
                            }
                        }
                    }
                    formMain.InstalledTrains.Add(currentPackage);
                    break;
            }
            textBoxFilesInstalled.Text = PackageFiles;
            panelDependancyError.Hide();
            panelVersionError.Hide();
            panelSuccess.Show();
        }

        /// <summary>Call this method to save the package list to disk.</summary>
        internal void SavePackages()
        {
            try
            {
                if (!Directory.Exists(PackageDatabase))
                {
                    Directory.CreateDirectory(PackageDatabase);
                }
                if (File.Exists(packageDatabaseFile))
                {
                    File.Delete(packageDatabaseFile);
                }
                //TODO: Can we automatically serialize the XML rather than manually writing?
                using (StreamWriter sw = new StreamWriter(packageDatabaseFile))
                {
                    sw.WriteLine("<?xml version=\"1.0\"?>");
                    sw.WriteLine("<OpenBVE>");
                    if (InstalledRoutes.Count > 0)
                    {
                        //Write out routes
                        sw.WriteLine("<PackageDatabase id=\"Routes\">");
                        foreach (var Package in InstalledRoutes)
                        {
                            sw.WriteLine("<Package name=\"" + Package.Name + "\" author=\"" + Package.Author + "\" version=\"" + Package.PackageVersion + "\" website=\"" + Package.Website + "\" guid=\"" + Package.GUID + "\" type=\"0\"/>");
                        }
                        sw.WriteLine("</PackageDatabase>");
                    }
                    if (InstalledTrains.Count > 0)
                    {
                        //Write out trains
                        sw.WriteLine("<PackageDatabase id=\"Trains\">");
                        foreach (var Package in InstalledTrains)
                        {
                            sw.WriteLine("<Package name=\"" + Package.Name + "\" author=\"" + Package.Author + "\" version=\"" + Package.PackageVersion + "\" website=\"" + Package.Website + "\" guid=\"" + Package.GUID + "\" type=\"1\"/>");
                        }
                        sw.WriteLine("</PackageDatabase>");
                    }
                    sw.WriteLine("</OpenBVE>");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured whilst saving the package database. \r\n Please check for write permissions.");
            }
        }

        /// <summary>This method must be called upon first load of the package management tab, in order to load the currently installed packages</summary>
        internal void LoadRoutePackages()
        {
            //Clear the package list
            InstalledRoutes.Clear();
            XmlDocument currentXML = new XmlDocument();
            //Attempt to load the packages database file
            if (!File.Exists(packageDatabaseFile))
            {
                //The database file doesn't exist.....
                return;
            }
            try
            {
                currentXML.Load(packageDatabaseFile);
            }
            catch
            {
                //Loading the XML barfed.....
                return;
            }
            if (currentXML.DocumentElement == null)
            {
                //Empty XML file
                return;
            }
            //Select the appropriate node
            XmlNodeList DocumentNodes = currentXML.SelectNodes("//OpenBVE/PackageDatabase[@id='Routes']/Package");
            if (DocumentNodes == null)
            {
                //No package nodes in XML file
                return;
            }
            foreach (XmlNode Package in DocumentNodes)
            {
                if (Package.Attributes != null)
                {
                    //This would appear to be a valid package
                    Package currentPackage = new Package();
                    foreach (XmlAttribute currentAttribute in Package.Attributes)
                    {
                        switch (currentAttribute.Name.ToLower())
                        {
                            //Parse attributes
                            case "version":
                                currentPackage.PackageVersion = Version.Parse(currentAttribute.InnerText);
                                break;
                            case "name":
                                currentPackage.Name = currentAttribute.InnerText;
                                break;
                            case "author":
                                currentPackage.Author = currentAttribute.InnerText;
                                break;
                            case "website":
                                currentPackage.Website = currentAttribute.InnerText;
                                break;
                            case "guid":
                                currentPackage.GUID = currentAttribute.InnerText;
                                break;
                        }
                    }
                    if (String.IsNullOrEmpty(currentPackage.GUID))
                    {
                        return;
                    }
                    //Add to the list of installed packages
                    InstalledRoutes.Add(currentPackage);
                }
            }

        }
        /// <summary>This method must be called upon first load of the package management tab, in order to load the currently installed packages</summary>
        internal void LoadTrainPackages()
        {
            //Clear the package list
            InstalledTrains.Clear();
            XmlDocument currentXML = new XmlDocument();
            //Attempt to load the packages database file
            if (!File.Exists(packageDatabaseFile))
            {
                //The database file doesn't exist.....
                return;
            }
            try
            {
                currentXML.Load(packageDatabaseFile);
            }
            catch
            {
                //Loading the XML barfed.....
                return;
            }
            if (currentXML.DocumentElement == null)
            {
                //Empty XML file
                return;
            }
            //Select the appropriate node
            XmlNodeList DocumentNodes = currentXML.SelectNodes("//OpenBVE/PackageDatabase[@id='Trains']/Package");
            if (DocumentNodes == null)
            {
                //No package nodes in XML file
                return;
            }
            foreach (XmlNode Package in DocumentNodes)
            {
                if (Package.Attributes != null)
                {
                    //This would appear to be a valid package
                    Package currentPackage = new Package();
                    foreach (XmlAttribute currentAttribute in Package.Attributes)
                    {
                        switch (currentAttribute.Name.ToLower())
                        {
                            //Parse attributes
                            case "version":
                                currentPackage.PackageVersion = Version.Parse(currentAttribute.InnerText);
                                break;
                            case "name":
                                currentPackage.Name = currentAttribute.InnerText;
                                break;
                            case "author":
                                currentPackage.Author = currentAttribute.InnerText;
                                break;
                            case "website":
                                currentPackage.Website = currentAttribute.InnerText;
                                break;
                            case "guid":
                                currentPackage.GUID = currentAttribute.InnerText;
                                break;
                        }
                    }
                    if (String.IsNullOrEmpty(currentPackage.GUID))
                    {
                        return;
                    }
                    //Add to the list of installed packages
                    InstalledTrains.Add(currentPackage);
                }
            }

        }

        //TODO: Combine two methods into singleton??
        /// <summary>This method should be called to populate the list of installed packages </summary>
        internal void PopulatePackageList()
        {
            //Clear the package list
            dataGridViewRoutePackages.Rows.Clear();
            if (InstalledRoutes.Count != 0)
            {
                //We have route packages in our list!
                for (int i = 0; i < InstalledRoutes.Count; i++)
                {
                    //Create row
                    object[] Package =
                    {
                        InstalledRoutes[i].Name, InstalledRoutes[i].PackageVersion, InstalledRoutes[i].Author,
                        InstalledRoutes[i].Website
                    };
                    //Add to the datagrid view
                    dataGridViewRoutePackages.Rows.Add(Package);
                }

            }
            dataGridViewTrainPackages.Rows.Clear();
            if (InstalledTrains.Count != 0)
            {
                //We have train packages in our list!
                for (int i = 0; i < InstalledTrains.Count; i++)
                {
                    //Create row
                    object[] Package =
                    {
                        InstalledTrains[i].Name, InstalledTrains[i].PackageVersion, InstalledTrains[i].Author,
                        InstalledTrains[i].Website
                    };
                    //Add to the datagrid view
                    dataGridViewTrainPackages.Rows.Add(Package);
                }
            }
        }

        /// <summary>This method should be called to populate the list of unmet dependancies</summary>
        internal void PopulateDependancyList(List<Package> Dependancies, DataGridView dependancyGrid)
        {
            //Clear the package list
            dependancyGrid.Rows.Clear();
            //We have route packages in our list!
            for (int i = 0; i < Dependancies.Count; i++)
            {
                //Create row
                object[] Package = { Dependancies[i].Name, Dependancies[i].MinimumVersion, Dependancies[i].MaximumVersion , Dependancies[i].Author, 
                                       Dependancies[i].Website};
                //Add to the datagrid view
                dependancyGrid.Rows.Add(Package);
            }
        }

        /// <summary>This method should be called to uninstall a package</summary>
        internal void UninstallPackage(int selectedPackageIndex, ref List<Package> Packages)
        {
            //TODO: Requires dependancy checking on uninstall
            string uninstallResults = "";
            Package packageToUninstall = Packages[selectedPackageIndex];
            if (OpenBveApi.Packages.Manipulation.UninstallPackage(packageToUninstall, PackageDatabase,ref uninstallResults))
            {
                Packages.Remove(packageToUninstall);
                textBoxUninstallResult.Text = uninstallResults;
                panelPackageList.Hide();
                panelPackageInstall.Hide();
                panelDependancyError.Hide();
                panelVersionError.Hide();
                panelSuccess.Hide();
            }
            else
            {
                if (uninstallResults == null)
                {
                    //TODO: Requires a specific error for attempting to uninstall a package with the XML file list missing
                }
            }
            panelUninstallResult.Show();
        }

        private void dataGridViewTrainPackages_SelectionChanged(object sender, EventArgs e)
        {
            selectedRoutePackageIndex = -1;
            selectedTrainPackageIndex = -1;
            if (dataGridViewTrainPackages.SelectedRows.Count > 0)
            {
                selectedTrainPackageIndex = dataGridViewTrainPackages.SelectedRows[0].Index;
            }
        }

        private void dataGridViewRoutePackages_SelectionChanged(object sender, EventArgs e)
        {
            selectedTrainPackageIndex = -1;
            selectedRoutePackageIndex = -1;
            if (dataGridViewRoutePackages.SelectedRows.Count > 0)
            {
                selectedRoutePackageIndex = dataGridViewRoutePackages.SelectedRows[0].Index;
            }
        }

        private void buttonUninstallPackage_Click(object sender, EventArgs e)
        {
            if (selectedRoutePackageIndex != -1)
            {
                UninstallPackage(selectedRoutePackageIndex, ref InstalledRoutes);
            }
            if (selectedTrainPackageIndex != -1)
            {
                UninstallPackage(selectedTrainPackageIndex, ref InstalledTrains);
            }
        }

        private void buttonUninstallFinish_Click(object sender, EventArgs e)
        {
            panelUninstallResult.Hide();
            panelPackageList.Show();
        }


        private void buttonInstallPackage_Click(object sender, EventArgs e)
        {
            TryLoadImage(pictureBoxPackageImage, "route_error.png");
            panelPackageList.Hide();
            panelPackageInstall.Show();
        }
    }
}