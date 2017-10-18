using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			int i = comboboxLanguages.SelectedIndex;
			if (i >= 0 & i < LanguageFiles.Length) {
				string Code = System.IO.Path.GetFileNameWithoutExtension(LanguageFiles[i]);
				string Folder = Program.FileSystem.GetDataFolder("Flags");
				#if !DEBUG
				try {
					#endif
					Interface.CurrentLanguageCode = Interface.AvailableLangauges[i].LanguageCode;
					#if !DEBUG
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				#endif
				#if !DEBUG
				try {
					#endif
					string File = OpenBveApi.Path.CombineFile(Folder, Interface.AvailableLangauges[i].Flag);
					if (!System.IO.File.Exists(File)) {
						File = OpenBveApi.Path.CombineFile(Folder, "unknown.png");
					}
					if (System.IO.File.Exists(File)) {
						using (var fs = new FileStream(File, FileMode.Open, FileAccess.Read))
						{
							pictureboxLanguage.Image = Image.FromStream(fs);
						}
					} else {
						pictureboxLanguage.Image = null;
					}
					CurrentLanguageCode = Code;
					#if !DEBUG
				} catch { }
				#endif
				ApplyLanguage();
			}
		}

		// interpolation
		private void comboboxInterpolation_SelectedIndexChanged(object sender, EventArgs e) {
			int i = comboboxInterpolation.SelectedIndex;
			bool q = i == (int)Interface.InterpolationMode.AnisotropicFiltering;
			labelAnisotropic.Enabled = q;
			updownAnisotropic.Enabled = q;
		}

		private void comboBoxTimeTableDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.TimeTableStyle = (Interface.TimeTableMode)comboBoxTimeTableDisplayMode.SelectedIndex;
		}


		private void trackBarTimeAccelerationFactor_ValueChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.TimeAccelerationFactor = trackBarTimeAccelerationFactor.Value;
		}
		
		// =======
		// options
		// =======

		// joysticks enabled
		private void checkboxJoysticksUsed_CheckedChanged(object sender, EventArgs e) {
			groupboxJoysticks.Enabled = checkboxJoysticksUsed.Checked;
		}

		
		
	}
}