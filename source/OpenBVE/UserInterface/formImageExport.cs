using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using RouteManager2;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenBve
{
	internal partial class FormImageExport : Form
	{
		private readonly bool IsMap;
		private readonly string routeFileName;
		internal FormImageExport(bool isMap, string fileName)
		{
			InitializeComponent();
			IsMap = isMap;
			routeFileName = fileName;
			textBoxPath.Text = Path.GetDirectoryName(routeFileName);
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void buttonExport_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(textBoxPath.Text) && Directory.Exists(textBoxPath.Text))
			{
				string finalPath = Path.Combine(textBoxPath.Text) + (IsMap ? Path.GetFileNameWithoutExtension(routeFileName) + " - Map.png" : Path.GetFileNameWithoutExtension(routeFileName) + " - Gradient Profile.png");
				try
				{
					if (IsMap)
					{
						Image map = Illustrations.CreateRouteMap((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value, false, out _);
						map.Save(finalPath);
					}
					else 
					{
						Image gradient = Illustrations.CreateRouteGradientProfile((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value, false);
						gradient.Save(finalPath);
					}
				}
				catch
				{
					MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "errors", "package_file_generic" }) + finalPath, @"Export Image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				Close();
			}
			else
			{
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "errors", "security_checkaccess" }), @"Export Image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void buttonBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fb = new FolderBrowserDialog();
			if (fb.ShowDialog() == DialogResult.OK)
			{
				textBoxPath.Text = fb.SelectedPath;
			}
		}
	}
}
