using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace OpenBve
{
    internal partial class formPackageInstall : Form
    {
        
        public formPackageInstall(formMain parentForm)
        {
            InitializeComponent();
            PackageInstallImage.Image = LoadImage(MenuFolder, "package.png");
            PackageInstallImage.SizeMode = PictureBoxSizeMode.StretchImage;
            PackageTypeImage.Image = LoadImage(MenuFolder, "icon_error.png");
            PackageImage.Image = LoadImage(MenuFolder, "route_error.png");
            PackageImage.SizeMode = PictureBoxSizeMode.StretchImage;
            PackagePanel5.Hide();
            PackagePanel4.Hide();
            PackagePanel3.Hide();
            PackagePanel2.Hide();
            PackagePanel1.Show();
            PackageFilename.Text = CurrentPackage;
            //Create unique temp directory for this run
            TempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(TempDirectory);
            this.parentForm = parentForm;
        }

        /// <summary>Pointer to the parent form, updated via the constructor</summary>
        internal formMain parentForm;

        readonly string MenuFolder = Program.FileSystem.GetDataFolder("Menu");
        private string CurrentPackage = "N/A";
        private readonly string TempDirectory;
        private XmlDocument CurrentXML;
        readonly formMain.Package currentPackage = new formMain.Package { Name = "N/A", Author = "N/A", GUID = "000000", PackageVersion = Version.Parse("0.0.0"), Website = "N/A" };
        /// <summary>Attempts to load an image into memory using the OpenBVE path resolution API</summary>
        private Image LoadImage(string Folder, string Title)
        {
            try
            {
                string File = OpenBveApi.Path.CombineFile(Folder, Title);
                if (System.IO.File.Exists(File))
                {
                    try
                    {
                        return Image.FromFile(File);
                    }
                    catch
                    {
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>Reads package information files from the selected archive</summary>
        private void ReadPackage()
        {
            bool InfoFound = false;
            //Load the selected package file into a stream
            using (Stream stream = File.OpenRead(CurrentPackage))
            {

                var reader = ReaderFactory.Open(stream);
                while (reader.MoveToNextEntry())
                {

                    //Search for the package.xml file- This must be located in the archive root
                    if (reader.Entry.Key.ToLowerInvariant() == "package.xml")
                    {
                        MessageBox.Show(reader.Entry.Key.ToLowerInvariant());
                        //Extract the package.xml to the uniquely assigned temp directory
                        CurrentXML = new XmlDocument();
                        reader.WriteEntryToDirectory(TempDirectory,
                            ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        //Load the XML file
                        InfoFound = true;
                        CurrentXML.Load(OpenBveApi.Path.CombineDirectory(TempDirectory, "package.xml"));

                    }
                    if (reader.Entry.Key.ToLowerInvariant() == "package.png")
                    {
                        //Extract the package.png to the uniquely assigned temp directory
                        reader.WriteEntryToDirectory(TempDirectory,ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        PackageImage.Image = LoadImage(TempDirectory, "package.png");
                    }
                    if (reader.Entry.Key.ToLowerInvariant() == "package.rtf")
                    {
                        //Extract the package.rtf description file to the uniquely assigned temp directory
                        reader.WriteEntryToDirectory(TempDirectory,ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        PackageDescription.LoadFile(OpenBveApi.Path.CombineFile(TempDirectory, "package.rtf"));
                    }
                    
                }
            }
            if (!InfoFound)
            {
                //No info found.....
                MessageBox.Show("Package " + CurrentPackage + " is corrupt.");
                return;
            }
            //Read the info
            XmlNodeList DocumentNodes = CurrentXML.DocumentElement.SelectNodes("/openbve/package");
            //Check this file actually contains OpenBVE object nodes
            if (DocumentNodes != null)
            {
                foreach (XmlNode node in DocumentNodes)
                {
                    /*
                     * Read properties
                     */
                    if (node.Attributes != null)
                    {
                        foreach (XmlAttribute Attribute in node.Attributes)
                        {
                            switch (Attribute.Name.ToLowerInvariant())
                            {
                                case "author":
                                    currentPackage.Author = Attribute.InnerText;
                                    break;
                                case "name":
                                    currentPackage.Name = Attribute.InnerText;
                                    break;
                                case "guid":
                                    currentPackage.GUID = Attribute.InnerText;
                                    break;
                                case "website":
                                    currentPackage.Website = Attribute.InnerText;
                                    break;
                                case "version":
                                    currentPackage.PackageVersion = Version.Parse(Attribute.InnerText);
                                    break;
                            }
                        }
                    }
                }
            }
            //The properties have now been read, so update the basic GUI labels
            PackageNameText.Text = currentPackage.Name;
            PackageAuthorText.Text = currentPackage.Author;
            PackageVersionText.Text = currentPackage.PackageVersion.ToString();
        }

        /// <summary>Checks to see if this package is currently installed, and if so whether there is a version conflict</summary>
        private bool VersionCheck(bool IsRoute)
        {
            if (IsRoute)
            {
                foreach (var Package in formMain.InstalledRoutes)
                {
                    //Check GUID
                    if (currentPackage.GUID == Package.GUID)
                    {
                        //GUID found, check versions
                        if (currentPackage.PackageVersion == Package.PackageVersion)
                        {
                            //Identical versions, show prompt 1
                            return false;
                        }
                        else if (currentPackage.PackageVersion > Package.PackageVersion)
                        {
                            //Older version, show prompt 2
                            return false;
                        }
                        else
                        {
                            //OK- new version, show prompt 3
                            return true;
                        }
                    }
                }
            }
            else
            {
                foreach (var Package in formMain.InstalledTrains)
                {
                    //Check GUID
                    if (currentPackage.GUID == Package.GUID)
                    {
                        //GUID found, check versions
                        if (currentPackage.PackageVersion == Package.PackageVersion)
                        {
                            //Identical versions, show prompt 1
                            return false;
                        }
                        else if (currentPackage.PackageVersion > Package.PackageVersion)
                        {
                            return false;
                            //Older version, show prompt 2
                        }
                        else
                        {
                            return false;
                            //OK- new version, show prompt 3
                        }
                    }
                }
            }
            return true;
        }

        private void ExtractPackage(bool IsRoute)
        {
            using (Stream stream = File.OpenRead(CurrentPackage))
            {

                var reader = ReaderFactory.Open(stream);
                List<string> PackageFiles = new List<string>();
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.Key.ToLowerInvariant() == "package.xml" || reader.Entry.Key.ToLowerInvariant() == "package.png" || reader.Entry.Key.ToLowerInvariant() == "package.rtf")
                    {
                        //Skip package information files
                    }
                    else
                    {
                        //Extract everything else, preserving directory structure
                        if (IsRoute)
                        {
                            reader.WriteEntryToDirectory(Program.FileSystem.InitialRailwayFolder,ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                            PackageFiles.Add(reader.Entry.Key);
                        }
                        else
                        {
                            reader.WriteEntryToDirectory(Program.FileSystem.InitialTrainFolder, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                            PackageFiles.Add(reader.Entry.Key);
                        }
                    }
                }
                string Text = "";
                foreach (var FileName in PackageFiles)
                {
                    Text += FileName + "\r\n";
                }
                FilesInstalled.Text = Text;
            }
        }

        private void PackageNextButton_Click(object sender, System.EventArgs e)
        {
            if (PackagePanel1.Visible)
            {
                PackagePanel1.Hide();
                PackagePanel2.Show();
            }
            else if(PackagePanel2.Visible)
            {
                PackagePanel2.Hide();
                if (VersionCheck(true))
                {
                    //Version check has passed, so show the description
                    PackagePanel4.Show();
                }
                else
                {
                    //Show version error page
                    PackagePanel3.Show();
                }
            }
            else if (PackagePanel4.Visible)
            {
                //Extract the package!
                ExtractPackage(true);
                formMain.InstalledRoutes.Add(currentPackage);
                PackagePanel4.Hide();
                PackagePanel5.Show();
                PackageNextButton.Text = "Close";
            }
            else if (PackagePanel5.Visible)
            {
                this.Close();
            }
        }

        private void PackageButton_Click(object sender, System.EventArgs e)
        {
            openRoutePackage = new OpenFileDialog {InitialDirectory = "C:\\"};
            if (openRoutePackage.ShowDialog() == DialogResult.OK)
            {
                CurrentPackage = openRoutePackage.FileName;
                PackageFilename.Text = CurrentPackage;
                try
                {
                    ReadPackage();
                }
                catch (Exception)
                {
                    MessageBox.Show("Package " + CurrentPackage + "is corrupt.");
                }
            }
        }

        private void formPackageInstall_FormClosing(object sender, FormClosingEventArgs e)
        {
            parentForm.RefreshPackages();
        }
    }
}
