//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Marc Riera, The OpenBVE Project
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
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using OpenTK.Input;
using OpenBveApi.Interface;

namespace DenshaDeGoInput
{
	public partial class Config : Form
	{
		public Config()
		{
			InitializeComponent();
			timer1.Enabled = true;

			// Add connected devices to device list

			ListControllers();

			// Populate command boxes
			buttonaBox.Items.Add(Translations.GetInterfaceString("None"));
			buttonbBox.Items.Add(Translations.GetInterfaceString("None"));
			buttoncBox.Items.Add(Translations.GetInterfaceString("None"));
			buttonstartBox.Items.Add(Translations.GetInterfaceString("None"));
			buttonselectBox.Items.Add(Translations.GetInterfaceString("None"));
			Translations.CommandInfo[] commands = Translations.CommandInfos.OrderBy(o=>o.Command).ToArray();
			for (int i = 0; i < Translations.CommandInfos.Length; i++)
			{
				buttonaBox.Items.Add(commands[i].Name);
				buttonbBox.Items.Add(commands[i].Name);
				buttoncBox.Items.Add(commands[i].Name);
				buttonstartBox.Items.Add(commands[i].Name);
				buttonselectBox.Items.Add(commands[i].Name);
			}
		}

		private void ListControllers()
		{
			for (int i = 0; i < 10; i++)
			{
				JoystickState state = Joystick.GetState(i);
				JoystickCapabilities capabilities = Joystick.GetCapabilities(i);
				InputTranslator.ControllerModels model = InputTranslator.GetControllerModel(state, capabilities);
				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				if (capabilities.ButtonCount > 0 && model != InputTranslator.ControllerModels.Unsupported)
				{
					deviceBox.Items.Add("Joystick " + (i+1));
				}
			}
		}

		private void UpdateInterface()
		{
			label_brakeemg.ForeColor = Color.Black;
			label_brake8.ForeColor = Color.Black;
			label_brake7.ForeColor = Color.Black;
			label_brake6.ForeColor = Color.Black;
			label_brake5.ForeColor = Color.Black;
			label_brake4.ForeColor = Color.Black;
			label_brake3.ForeColor = Color.Black;
			label_brake2.ForeColor = Color.Black;
			label_brake1.ForeColor = Color.Black;
			label_braken.ForeColor = Color.Black;
			label_power5.ForeColor = Color.Black;
			label_power4.ForeColor = Color.Black;
			label_power3.ForeColor = Color.Black;
			label_power2.ForeColor = Color.Black;
			label_power1.ForeColor = Color.Black;
			label_powern.ForeColor = Color.Black;
			label_a.ForeColor = Color.Black;
			label_b.ForeColor = Color.Black;
			label_c.ForeColor = Color.Black;
			label_start.ForeColor = Color.Black;
			label_select.ForeColor = Color.Black;


			if (InputTranslator.IsControllerConnected)
			{
				switch (InputTranslator.BrakeNotch)
				{
					case InputTranslator.BrakeNotches.Emergency:
						label_brakeemg.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B8:
						label_brake8.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B7:
						label_brake7.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B6:
						label_brake6.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B5:
						label_brake5.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B4:
						label_brake4.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B3:
						label_brake3.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B2:
						label_brake2.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.B1:
						label_brake1.ForeColor = Color.White;
						break;
					case InputTranslator.BrakeNotches.Released:
						label_braken.ForeColor = Color.White;
						break;
				}
				switch (InputTranslator.PowerNotch)
				{
					case InputTranslator.PowerNotches.P5:
						label_power5.ForeColor = Color.White;
						break;
					case InputTranslator.PowerNotches.P4:
						label_power4.ForeColor = Color.White;
						break;
					case InputTranslator.PowerNotches.P3:
						label_power3.ForeColor = Color.White;
						break;
					case InputTranslator.PowerNotches.P2:
						label_power2.ForeColor = Color.White;
						break;
					case InputTranslator.PowerNotches.P1:
						label_power1.ForeColor = Color.White;
						break;
					case InputTranslator.PowerNotches.N:
						label_powern.ForeColor = Color.White;
						break;
				}
				if (InputTranslator.ControllerButtons.A == OpenTK.Input.ButtonState.Pressed)
				{
					label_a.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.B == OpenTK.Input.ButtonState.Pressed)
				{
					label_b.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.C == OpenTK.Input.ButtonState.Pressed)
				{
					label_c.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Start == OpenTK.Input.ButtonState.Pressed)
				{
					label_start.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Select == OpenTK.Input.ButtonState.Pressed)
				{
					label_select.ForeColor = Color.White;
				}
			}
		}

		private void Config_Shown(object sender, EventArgs e)
		{
			// Try to select the current device
			if (InputTranslator.activeControllerIndex < deviceBox.Items.Count)
			{
				deviceBox.SelectedIndex = InputTranslator.activeControllerIndex;
			}

			// Set command boxes
			buttonselectBox.SelectedIndex = DenshaDeGoInput.buttonCommands[0];
			buttonstartBox.SelectedIndex = DenshaDeGoInput.buttonCommands[1];
			buttonaBox.SelectedIndex = DenshaDeGoInput.buttonCommands[2];
			buttonbBox.SelectedIndex = DenshaDeGoInput.buttonCommands[3];
			buttoncBox.SelectedIndex = DenshaDeGoInput.buttonCommands[4];

			// Set checkboxes
			convertnotchesCheck.Checked = DenshaDeGoInput.convertNotches;
			minmaxCheck.Checked = DenshaDeGoInput.keepMaxMin;
			holdbrakeCheck.Checked = DenshaDeGoInput.mapHoldBrake;
			minmaxCheck.Enabled = DenshaDeGoInput.convertNotches;
		}

		private void Config_FormClosed(Object sender, FormClosedEventArgs e)
		{
			// Reload the previous config and close the config dialog
			DenshaDeGoInput.LoadConfig();
		}

		private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			InputTranslator.activeControllerIndex = deviceBox.SelectedIndex;
		}

		private void buttonselectBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.buttonCommands[0] = buttonselectBox.SelectedIndex;
		}

		private void buttonstartBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.buttonCommands[1] = buttonstartBox.SelectedIndex;
		}

		private void buttonaBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.buttonCommands[2] = buttonaBox.SelectedIndex;
		}

		private void buttonbBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.buttonCommands[3] = buttonbBox.SelectedIndex;
		}

		private void buttoncBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.buttonCommands[4] = buttoncBox.SelectedIndex;
		}

		private void convertnotchesCheck_CheckedChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.convertNotches = convertnotchesCheck.Checked;
			minmaxCheck.Enabled = DenshaDeGoInput.convertNotches;
		}

		private void minmaxCheck_CheckedChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.keepMaxMin = minmaxCheck.Checked;
		}

		private void holdbrakeCheck_CheckedChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.mapHoldBrake = holdbrakeCheck.Checked;
		}

		private void buttonCalibrate_Click(object sender, EventArgs e)
		{
			timer1.Stop();
			PSController.Calibrate();
			timer1.Start();
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			// Save the config and close the config dialog
			DenshaDeGoInput.SaveConfig();
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			// Reload the previous config and close the config dialog
			DenshaDeGoInput.LoadConfig();
			Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			InputTranslator.Update();
			UpdateInterface();
		}

	}
}
