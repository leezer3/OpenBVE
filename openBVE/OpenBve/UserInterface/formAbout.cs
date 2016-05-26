using System.Drawing;
using System.Windows.Forms;

namespace OpenBve
{
    internal partial class formAbout : Form
    {
        private readonly Image Logo;

        public formAbout()
        {
            InitializeComponent();
            labelProductName.Text = @"openBVE v" + Application.ProductVersion + Program.VersionSuffix;
            try
            {
                string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Menu"), "logo.png");
                if (System.IO.File.Exists(File))
                {
                    try
                    {
                        Logo = Image.FromFile(File);
                    }
                    catch
                    {
                    }
                }
                if (Logo != null)
                {
                    pictureBoxLogo.Image = Logo;
                }
            }
            catch
            { }
            try
            {
                string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico");
                this.Icon = new Icon(File);
            }
            catch { }
            
        }

        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
