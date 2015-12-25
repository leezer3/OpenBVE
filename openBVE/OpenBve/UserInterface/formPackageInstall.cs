using System.Drawing;
using System.Windows.Forms;

namespace OpenBve
{
    public partial class formPackageInstall : Form
    {
        public formPackageInstall()
        {
            InitializeComponent();
            pictureBox1.Image = formMain.PackageIcon;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}
