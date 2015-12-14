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
                //pictureBoxRouteMap.Image = OpenBve.Game.RouteInformation.RouteMap;
                pictureBoxRouteMap.SizeMode = PictureBoxSizeMode.StretchImage;
                //pictureBoxGradientProfile.Image = OpenBve.Game.RouteInformation.GradientProfile;
                pictureBoxGradientProfile.SizeMode = PictureBoxSizeMode.StretchImage;
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                System.Drawing.Icon ico = new System.Drawing.Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
                this.Icon = ico;
                this.ShowInTaskbar = false;
            }
            catch {}
        }

        internal void UpdateImage(Byte[] RouteMap, Byte[] GradientProfile, string RouteBriefing)
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

                if (!string.IsNullOrEmpty(RouteBriefing))
                {
                    try
                    {
                        if (Path.GetExtension(RouteBriefing) == ".txt")
                        {
                            richTextBoxRouteInformation.LoadFile(RouteBriefing, RichTextBoxStreamType.PlainText);
                        }
                        else
                        {
                            richTextBoxRouteInformation.LoadFile(RouteBriefing, RichTextBoxStreamType.RichText);
                        }
                        
                    }
                    catch
                    {
                        //Hide tab page if we encounter an error loading the briefing file
                        tabControl1.TabPages.Remove(tabPage3);
                    }
                }
                else
                {
                    //Hide tab page if no route briefing is provided
                    tabControl1.TabPages.Remove(tabPage3);
                }
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

        private void richTextBoxRouteInformation_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab.Focus();
        }
    }
}
