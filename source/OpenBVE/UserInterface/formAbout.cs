using System.Drawing;
using System.Text;
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
						pictureBoxLogo.Image = ImageExtensions.FromFile(File);
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
			
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("AssimpParser:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.AssimpParser);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("CS Script:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.CS_Script);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Cursors:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.Cursors);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine(".NET Runtime:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.dotnet_runtime);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("DotNetZip:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.DotNetZip);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Namotion.Reflection:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.Namotion_Reflection);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NAudio:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.NAudio);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NAudio.Vorbis:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.NAudio_Vorbis);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Newtonsoft.Json:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.Newtonsoft_Json);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NJsonSchema:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.NJsonSchema);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NLayer:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.NLayer);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NVorbis:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.NVorbis);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("OpenAL Soft:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.OpenALSoft);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("OpenTK:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.OpenTK);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Prism:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.Prism);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("ReactiveExtensions:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.ReactiveExtensions);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("ReactiveProperty:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.ReactiveProperty);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("SanYing Input:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.SanYing_Input);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("SharpCompress:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.SharpCompress);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("TrainEditor2 Icons:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.TrainEditor2_Icons);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Ude:");
			builder.AppendLine(OpenBveApi.Resources.Licenses.Ude);

			textBoxOpenSourceLicences.Text = builder.ToString();
		}

		private void buttonClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
