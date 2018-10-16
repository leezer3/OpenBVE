using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			string Folder = Program.FileSystem.GetDataFolder("Flags");
			if (Translations.SelectedLanguage(Folder, ref Interface.CurrentOptions.LanguageCode, comboboxLanguages, pictureboxLanguage)) {
				ApplyLanguage();
			}
		}

		// interpolation
		private void comboboxInterpolation_SelectedIndexChanged(object sender, EventArgs e) {
			int i = comboboxInterpolation.SelectedIndex;
			bool q = i == (int)InterpolationMode.AnisotropicFiltering;
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
