using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using SharpCompress;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace OpenBve
{
    public partial class formPackageInstall : Form
    {
        public formPackageInstall()
        {
            InitializeComponent();
            PackageInstallImage.Image = LoadImage(MenuFolder, "package.png");
            PackageInstallImage.SizeMode = PictureBoxSizeMode.StretchImage;
            PackageTypeImage.Image = LoadImage(MenuFolder, "icon_error.png");
            PackageImage.Image = LoadImage(MenuFolder, "route_error.png");
            PackageImage.SizeMode = PictureBoxSizeMode.StretchImage;
            PackagePanel2.Hide();
            PackagePanel1.Show();
            PackageFilename.Text = CurrentPackage;
            //Create unique temp directory for this run
            TempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(TempDirectory);
        }

        readonly string MenuFolder = Program.FileSystem.GetDataFolder("Menu");
        private string CurrentPackage = "N/A";
        private string TempDirectory;
        private XmlDocument CurrentXML;
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
            //Load the selected package file into a stream
            using (Stream stream = File.OpenRead(@"C:\test.rar"))
            {
                var reader = ReaderFactory.Open(stream);
                while (reader.MoveToNextEntry())
                {
                    //Search for the package.xml file- This must be located in the archive root
                    if (reader.Entry.Key.ToLowerInvariant() == "package.xml")
                    {
                        //Extract the package.xml to the uniquely assigned temp directory
                        CurrentXML = new XmlDocument();
                        reader.WriteEntryToDirectory(TempDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        //Load the XML file
                        CurrentXML.Load(OpenBveApi.Path.CombineDirectory(TempDirectory, "package.xml"));
                    }
                    if (reader.Entry.Key.ToLowerInvariant() == "package.png")
                    {
                        //Extract the package.png to the uniquely assigned temp directory
                        reader.WriteEntryToDirectory(TempDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                    }
                }
            }
        }
        

        private void PackageNextButton_Click(object sender, System.EventArgs e)
        {
            if (PackagePanel1.Visible)
            {
                PackagePanel1.Hide();
                PackagePanel2.Show();
            }
            else
            {
                ReadPackage();
            }
        }

        private void PackageButton_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CurrentPackage = openFileDialog1.FileName;
                PackageFilename.Text = CurrentPackage;
            }
        }
    }
}
