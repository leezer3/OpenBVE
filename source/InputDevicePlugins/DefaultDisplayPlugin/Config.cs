using System;
using System.Windows.Forms;

namespace DefaultDisplayPlugin
{
	public partial class Config : Form
	{
		public Config()
		{
			InitializeComponent();
			checkBoxClock.Checked = DefaultDisplayPlugin.IsDisplayClock;
			checkBoxSpeed.Checked = DefaultDisplayPlugin.IsDisplaySpeed;
			comboBoxSpeed.SelectedIndex = DefaultDisplayPlugin.SpeedDisplayMode;
			checkBoxGradient.Checked = DefaultDisplayPlugin.IsDisplayGradient;
			comboBoxGradient.SelectedIndex = DefaultDisplayPlugin.GradientDisplayMode;
			checkBoxDistNextStation.Checked = DefaultDisplayPlugin.IsDisplayDistNextStation;
			comboBoxDistNextStation.SelectedIndex = DefaultDisplayPlugin.DistNextStationDisplayMode;
			checkBoxFps.Checked = DefaultDisplayPlugin.IsDisplayFps;
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			DefaultDisplayPlugin.IsDisplayClock = checkBoxClock.Checked;
			DefaultDisplayPlugin.IsDisplaySpeed = checkBoxSpeed.Checked;
			DefaultDisplayPlugin.SpeedDisplayMode = comboBoxSpeed.SelectedIndex;
			DefaultDisplayPlugin.IsDisplayGradient = checkBoxGradient.Checked;
			DefaultDisplayPlugin.GradientDisplayMode = comboBoxGradient.SelectedIndex;
			DefaultDisplayPlugin.IsDisplayDistNextStation = checkBoxDistNextStation.Checked;
			DefaultDisplayPlugin.DistNextStationDisplayMode = comboBoxDistNextStation.SelectedIndex;
			DefaultDisplayPlugin.IsDisplayFps = checkBoxFps.Checked;
			DefaultDisplayPlugin.SaveConfig();
			this.Close();
		}
	}
}
