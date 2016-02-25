using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace OpenBve
{
    internal partial class formMain
    {
        /// <summary>Defines an OpenBVE Package</summary>
        internal class Package
        {
            /// <summary>The package version</summary>
            internal Version PackageVersion;
            /// <summary>The package name</summary>
            internal string Name = "";
            /// <summary>The package author</summary>
            internal string Author = "";
            /// <summary>The package website</summary>
            internal string Website = "";
            /// <summary>The GUID for this package</summary>
            internal string GUID = "";
            /// <summary>Stores the package type- 0 for routes and 1 for trains</summary>
            internal int PackageType;
        }

        

        internal static List<Package> InstalledRoutes = new List<Package>();
        internal static List<Package> InstalledTrains = new List<Package>();

        
        internal void RefreshPackages()
        {
            SavePackages();
            LoadRoutePackages();
            PopulatePackageList();
        }

        /// <summary>Call this method to save the package list to disk.</summary>
        internal void SavePackages()
        {
            var PackageDatabase = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "packages.xml");
            File.Delete(PackageDatabase);
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
            var PackageDatabase = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "packages.xml");
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
            var PackageDatabase = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "packages.xml");
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

    }
}