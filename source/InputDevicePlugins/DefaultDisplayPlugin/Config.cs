//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
