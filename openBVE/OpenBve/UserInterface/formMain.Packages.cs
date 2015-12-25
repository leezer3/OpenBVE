using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace OpenBve
{
    internal partial class formMain
    {
        /// <summary>Defines an OpenBVE Package</summary>
        internal class Package
        {
            /// <summary>The overarching version number</summary>
            internal int Version = 0;
            /// <summary>The major version</summary>
            internal int MajorVersion = 0;
            /// <summary>The minor version</summary>
            internal int MinorVersion = 0;
            /// <summary>The revision number</summary>
            internal int Revision = 0;
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

        internal List<Package> InstalledRoutes = new List<Package>();
        internal List<Package> InstalledTrains = new List<Package>();

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
                        switch (currentAttribute.Name)
                        {
                            //Parse attributes
                            case "Version":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.Version);
                                break;
                            case "MajorVersion":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.MajorVersion);
                                break;
                            case "MinorVersion":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.MinorVersion);
                                break;
                            case "Revision":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.Revision);
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
                                int.TryParse(currentAttribute.InnerText, out currentPackage.Version);
                                break;
                            case "MajorVersion":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.MajorVersion);
                                break;
                            case "MinorVersion":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.MinorVersion);
                                break;
                            case "Revision":
                                int.TryParse(currentAttribute.InnerText, out currentPackage.Revision);
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
                object[] Package = { InstalledRoutes[i].Name, InstalledRoutes[i].Version + "." + InstalledRoutes[i].MajorVersion + "." + InstalledRoutes[i].MinorVersion + "." + InstalledRoutes[i].Revision, InstalledRoutes[i].Author, 
                                       InstalledRoutes[i].Website};
                //Add to the datagrid view
                dataGridViewRoutePackages.Rows.Add(Package);
            }

            if (InstalledTrains.Count == 0) return;
            //We have train packages in our list!
            for (int i = 0; i < InstalledTrains.Count; i++)
            {
                //Create row
                object[] Package = { InstalledTrains[i].Name, InstalledTrains[i].Version + "." + InstalledTrains[i].MajorVersion + "." + InstalledTrains[i].MinorVersion + "." + InstalledTrains[i].Revision, InstalledTrains[i].Author, 
                                       InstalledTrains[i].Website};
                //Add to the datagrid view
                dataGridViewTrainPackages.Rows.Add(Package);
            }
        }

    }
}