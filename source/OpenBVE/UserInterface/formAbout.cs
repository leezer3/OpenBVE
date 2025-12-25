using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal partial class FormAbout : Form
	{
		public FormAbout()
		{
			InitializeComponent();
			ApplyLanguage();
			labelProductName.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}) + @" v" + Application.ProductVersion + Program.VersionSuffix;
			try
			{
				string logoFile = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Menu"), "logo.png");
				if (System.IO.File.Exists(logoFile))
				{
					pictureBoxLogo.Image = ImageExtensions.FromFile(logoFile);
				}
			}
			catch
			{
				// Ignored
			}

			try
			{
				string iconFile = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico");
				Icon = new Icon(iconFile);
			}
			catch
			{
				// Ignored
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("CoreFX:");
			builder.AppendLine(OpenBveApi.Resource.CoreFX);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("CS Script:");
			builder.AppendLine(OpenBveApi.Resource.CS_Script);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Cursors:");
			builder.AppendLine(OpenBveApi.Resource.Cursors);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("DotNetZip:");
			builder.AppendLine(OpenBveApi.Resource.DotNetZip);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NAudio:");
			builder.AppendLine(OpenBveApi.Resource.NAudio);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NAudio.Vorbis:");
			builder.AppendLine(OpenBveApi.Resource.NAudio_Vorbis);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NLayer:");
			builder.AppendLine(OpenBveApi.Resource.NLayer);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("NVorbis:");
			builder.AppendLine(OpenBveApi.Resource.NVorbis);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("OpenTK:");
			builder.AppendLine(OpenBveApi.Resource.OpenTK);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Prism:");
			builder.AppendLine(OpenBveApi.Resource.Prism);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("ReactiveExtensions:");
			builder.AppendLine(OpenBveApi.Resource.ReactiveExtensions);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("ReactiveProperty:");
			builder.AppendLine(OpenBveApi.Resource.ReactiveProperty);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("SanYing Input:");
			builder.AppendLine(OpenBveApi.Resource.SanYing_Input);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("SharpCompress:");
			builder.AppendLine(OpenBveApi.Resource.SharpCompress);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("TrainEditor2 Icons:");
			builder.AppendLine(OpenBveApi.Resource.TrainEditor2_Icons);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("Ude:");
			builder.AppendLine(OpenBveApi.Resource.Ude);
			builder.AppendLine();
			builder.AppendLine(new string('-', 80));
			builder.AppendLine("XamlBehaviors for WPF:");
			builder.AppendLine(OpenBveApi.Resource.XamlBehaviorsForWPF);

			Shown += (sender, e) => textBoxOpenSourceLicences.Text = builder.ToString();
		}

		private void ApplyLanguage() {
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"about","title"});
			textBoxMain.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"about","description"});
			label1.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"about","open_source_licenses"});
			labelOpenSourceHeader.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"about","open_source_licenses_header"});
			buttonClose.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"about","close"});
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
			if (Program.CurrentHost.MonoRuntime)
			{
				// Use forceful exit on Mono to stop stuff hanging around
				Environment.Exit(0);
			}
			else
			{
				Close();	
			}
			
		}
	}
}
