using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenBve
{
	internal partial class formAbout : Form
	{
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
						using (var fs = new FileStream(File, FileMode.Open, FileAccess.Read))
						{
							pictureBoxLogo.Image = Image.FromStream(fs);
						}
					}
					catch
					{
					}
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
