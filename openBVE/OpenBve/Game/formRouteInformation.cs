using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenTK.Input;

namespace OpenBve
{
    internal partial class formRouteInformation : Form
    {
        public formRouteInformation()
        {
            InitializeComponent();
            try
            {
                pictureBoxRouteMap.Image = OpenBve.Game.RouteInformation.RouteMap;
                pictureBoxRouteMap.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxGradientProfile.Image = OpenBve.Game.RouteInformation.GradientProfile;
                pictureBoxGradientProfile.SizeMode = PictureBoxSizeMode.StretchImage;
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                System.Drawing.Icon ico = new System.Drawing.Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
                this.Icon = ico;
            }
            catch {}
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
    }
}
