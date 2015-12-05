using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OpenBve
{
    internal partial class formRouteInformation : Form
    {
        public formRouteInformation()
        {
            InitializeComponent();
            try
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                System.Drawing.Icon ico = new System.Drawing.Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
                this.Icon = ico;
                this.ShowInTaskbar = false;
            }
            catch {}
        }

        internal void UpdateImage(Byte[] RouteMap, Byte[] GradientProfile)
        {
            try
            {
                Bitmap bitmapRouteMap;
                using (var ms = new MemoryStream(RouteMap))
                {
                    bitmapRouteMap = new Bitmap(ms);
                }
                Bitmap bitmapGradientProfile;
                using (var ms = new MemoryStream(GradientProfile))
                {
                    bitmapGradientProfile = new Bitmap(ms);
                }
                pictureBoxRouteMap.Image = bitmapRouteMap;
                pictureBoxRouteMap.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxGradientProfile.Image = bitmapGradientProfile;
                pictureBoxGradientProfile.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch
            { }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Application.DoEvents();
        }
    }
}
