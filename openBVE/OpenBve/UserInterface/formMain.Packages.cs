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
        internal static List<Package> InstalledRoutes = new List<Package>();
        internal static List<Package> InstalledTrains = new List<Package>();

        
        internal void RefreshPackages()
        {
            SavePackages();
            LoadRoutePackages();
            PopulatePackageList();
        }

        private Package currentPackage;

        private void button2_Click(object sender, EventArgs e)
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
                        button2.Text = "Install";
                    }
                }

            }
            else
            {
                List<Package> Dependancies = Information.CheckDependancies(currentPackage, InstalledRoutes, InstalledTrains);
                if (Dependancies != null)
                {
                    //We are missing a dependancy
                    PopulateDependancyList(Dependancies);
                    panelPackageInstall.Hide();
                    panelDependancyError.Show();
                    return;
                }
                VersionInformation Info;
                Version OldVersion = new Version();
                if (currentPackage.PackageType == 0)
                {
                    Info = Information.CheckVersion(currentPackage, formMain.InstalledRoutes, ref OldVersion);
                }
                else
                {
                    Info = Information.CheckVersion(currentPackage, formMain.InstalledTrains, ref OldVersion);
                }
                switch (Info)
                {
                    case VersionInformation.NotFound:
                        Extract();
                        break;
                    case VersionInformation.NewerVersion:
                        //Newer version than installed, show new version prompt
                        //TODO: Not implemented
                        break;
                    case VersionInformation.SameVersion:
                        //Same version installed, show reinstall prompt
                        //TODO: Not implemented
                        break;
                    case VersionInformation.OlderVersion:
                        //Older version than installed, ERROR
                        
                        break;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RefreshPackages();
            panelSuccess.Hide();
            panelPackageList.Show();
        }

        private readonly string PackageDatabase = OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase");

        private void buttonProceedAnyway_Click(object sender, EventArgs e)
        {
            Extract();
        }

        private void Extract()
        {
            string ExtractionDirectory;
            if (currentPackage.PackageType == 0)
            {
                ExtractionDirectory = Program.FileSystem.InitialRailwayFolder;
            }
            else
            {
                ExtractionDirectory = Program.FileSystem.InitialTrainFolder;
            }
            string PackageFiles = "";
            Manipulation.ExtractPackage(currentPackage, ExtractionDirectory, PackageDatabase, ref PackageFiles);
            formMain.InstalledRoutes.Add(currentPackage);
            textBoxFilesInstalled.Text = PackageFiles;
            panelDependancyError.Hide();
            panelSuccess.Show();
        }

        /// <summary>Call this method to save the package list to disk.</summary>
        internal void SavePackages()
        {
            var PackageDatabase = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase"), "packages.xml");
            if (File.Exists(PackageDatabase))
            {
                File.Delete(PackageDatabase);
            }
            using (StreamWriter sw = new StreamWriter(PackageDatabase))
            {
                sw.WriteLine("<?xml version=\"1.0\"?>");
                sw.WriteLine("<OpenBVE>");
                if (InstalledRoutes.Count > 0)
                {
                    //Write out routes
                    sw.WriteLine("<PackageDatabase id=\"Routes\">");
                    foreach (var Package in InstalledRoutes)
                    {
                        sw.WriteLine("<Package name=\"" + Package.Name +"\" author=\"" + Package.Author + "\" version=\""+Package.PackageVersion + "\" website=\"" + Package.Website + "\" guid=\"" + Package.GUID + "\" type=\"0\"/>");
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

        /// <summary>This method must be called upon first load of the package management tab, in order to load the currently installed packages</summary>
        internal void LoadRoutePackages()
        {
            //Clear the package list
            InstalledRoutes.Clear();
            XmlDocument currentXML = new XmlDocument();
            //Attempt to load the packages database file
            var PackageDatabase = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase"), "packages.xml");
            if (!File.Exists(PackageDatabase))
            {
                //The database file doesn't exist.....
                return;
            }
            try
            {
                currentXML.Load(PackageDatabase);
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
            var PackageDatabase = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Program.FileSystem.SettingsFolder, "PackageDatabase"), "packages.xml");
            if (!File.Exists(PackageDatabase))
            {
                //The database file doesn't exist.....
                return;
            }
            try
            {
                currentXML.Load(PackageDatabase);
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
                        switch (currentAttribute.Name)
                        {
                            //Parse attributes
                            case "Version":
                                currentPackage.PackageVersion = Version.Parse(currentAttribute.InnerText);
                                break;
                            case "Name":
                                currentPackage.Name = currentAttribute.InnerText;
                                break;
                            case "Author":
                                currentPackage.Author = currentAttribute.InnerText;
                                break;
                            case "Website":
                                currentPackage.Website = currentAttribute.InnerText;
                                break;
                            case "GUID":
                                currentPackage.GUID = currentAttribute.InnerText;
                                break;
                        }
                    }
                    //Add to the list of installed packages
                    InstalledTrains.Add(currentPackage);
                }
            }

        }

        /// <summary>This method should be called to populate the list of installed packages </summary>
        internal void PopulatePackageList()
        {
            //Clear the package list
            dataGridViewRoutePackages.Rows.Clear();
            if (InstalledRoutes.Count == 0) return;
            //We have route packages in our list!
            for(int i = 0; i < InstalledRoutes.Count; i++)
            {
                //Create row
                object[] Package = { InstalledRoutes[i].Name, InstalledRoutes[i].PackageVersion, InstalledRoutes[i].Author, 
                                       InstalledRoutes[i].Website};
                //Add to the datagrid view
                dataGridViewRoutePackages.Rows.Add(Package);
            }

            if (InstalledTrains.Count == 0) return;
            //We have train packages in our list!
            for (int i = 0; i < InstalledTrains.Count; i++)
            {
                //Create row
                object[] Package = { InstalledTrains[i].Name, InstalledTrains[i].PackageVersion, InstalledTrains[i].Author, 
                                       InstalledTrains[i].Website};
                //Add to the datagrid view
                dataGridViewTrainPackages.Rows.Add(Package);
            }
        }

        /// <summary>This method should be called to populate the list of unmet dependancies</summary>
        internal void PopulateDependancyList(List<Package> Dependancies)
        {
            //Clear the package list
            dataGridViewDependancies.Rows.Clear();
            //We have route packages in our list!
            for (int i = 0; i < Dependancies.Count; i++)
            {
                //Create row
                object[] Package = { Dependancies[i].Name, Dependancies[i].MinimumVersion, Dependancies[i].MaximumVersion , Dependancies[i].Author, 
                                       Dependancies[i].Website};
                //Add to the datagrid view
                dataGridViewDependancies.Rows.Add(Package);
            }
        }

    }
}