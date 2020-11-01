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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using OpenTK.Input;
using OpenBveApi.Interface;

namespace DenshaDeGoInput
{
	public partial class Config : Form
	{
		/// <summary>
		/// Internal list of devices.
		/// </summary>
		private List<string> deviceList = new List<string>();

		public Config()
		{
			InitializeComponent();

			// Load language files
			Translations.LoadLanguageFiles(OpenBveApi.Path.CombineDirectory(DenshaDeGoInput.FileSystem.DataFolder, "Languages"));

			// Populate command boxes
			buttonselectBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonstartBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonaBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonbBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttoncBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttondBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonupBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttondownBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonleftBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonrightBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			buttonpedalBox.Items.Add(Translations.GetInterfaceString("denshadego_command_none"));
			Translations.CommandInfo[] commands = Translations.CommandInfos.OrderBy(o => o.Command).ToArray();
			for (int i = 0; i < Translations.CommandInfos.Length; i++)
			{
				buttonselectBox.Items.Add(commands[i].Name);
				buttonstartBox.Items.Add(commands[i].Name);
				buttonaBox.Items.Add(commands[i].Name);
				buttonbBox.Items.Add(commands[i].Name);
				buttoncBox.Items.Add(commands[i].Name);
				buttondBox.Items.Add(commands[i].Name);
				buttonupBox.Items.Add(commands[i].Name);
				buttondownBox.Items.Add(commands[i].Name);
				buttonleftBox.Items.Add(commands[i].Name);
				buttonrightBox.Items.Add(commands[i].Name);
				buttonpedalBox.Items.Add(commands[i].Name);
			}
		}

		/// <summary>
		/// Adds the available controllers to the device dropdown list.
		/// </summary>
		private void ListControllers()
		{
			// Clear the internal and visible lists
			deviceList.Clear();
			deviceBox.Items.Clear();

			for (int i = 0; i < 10; i++)
			{
				InputTranslator.ControllerModels model = InputTranslator.GetControllerModel(i);
				deviceList.Add(Translations.GetInterfaceString("denshadego_joystick").Replace("[index]", (i + 1).ToString()));

				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				JoystickCapabilities capabilities = Joystick.GetCapabilities(i);
				if (capabilities.ButtonCount > 0 && model != InputTranslator.ControllerModels.Unsupported)
				{
					deviceBox.Items.Add(deviceList[i]);
				}
			}
		}

		/// <summary>
		/// Updates the interface to reflect the input from the controller.
		/// </summary>
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
			label_d.ForeColor = Color.Black;
			label_start.ForeColor = Color.Black;
			label_select.ForeColor = Color.Black;
			label_up.ForeColor = Color.Black;
			label_down.ForeColor = Color.Black;
			label_left.ForeColor = Color.Black;
			label_right.ForeColor = Color.Black;
			label_pedal.ForeColor = Color.Black;

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
				if (InputTranslator.ControllerButtons.Select == OpenTK.Input.ButtonState.Pressed)
				{
					label_select.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Start == OpenTK.Input.ButtonState.Pressed)
				{
					label_start.ForeColor = Color.White;
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
				if (InputTranslator.ControllerButtons.D == OpenTK.Input.ButtonState.Pressed)
				{
					label_d.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Up == OpenTK.Input.ButtonState.Pressed)
				{
					label_up.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Down == OpenTK.Input.ButtonState.Pressed)
				{
					label_down.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Left == OpenTK.Input.ButtonState.Pressed)
				{
					label_left.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Right == OpenTK.Input.ButtonState.Pressed)
				{
					label_right.ForeColor = Color.White;
				}
				if (InputTranslator.ControllerButtons.Pedal == OpenTK.Input.ButtonState.Pressed)
				{
					label_pedal.ForeColor = Color.White;
				}
			}

			switch (InputTranslator.ControllerModel)
			{
				case InputTranslator.ControllerModels.Classic:
					buttonCalibrate.Visible = true;
					label_d.Visible = false;
					label_up.Visible = false;
					label_down.Visible = false;
					label_left.Visible = false;
					label_right.Visible = false;
					label_pedal.Visible = false;
					break;
				case InputTranslator.ControllerModels.Unbalance:
					buttonCalibrate.Visible = false;
					label_d.Visible = true;
					label_pedal.Visible = false;
					if (ControllerUnbalance.hasDirectionButtons)
					{
						label_up.Visible = true;
						label_down.Visible = true;
						label_left.Visible = true;
						label_right.Visible = true;
					}
					else
					{
						label_up.Visible = false;
						label_down.Visible = false;
						label_left.Visible = false;
						label_right.Visible = false;
					}
					break;
				default:
					buttonCalibrate.Visible = false;
					label_d.Visible = true;
					label_up.Visible = true;
					label_down.Visible = true;
					label_left.Visible = true;
					label_right.Visible = true;
					label_pedal.Visible = true;
					break;
			}
		}

		/// <summary>
		/// Retranslates the configutarion interface.
		/// </summary>
		private void UpdateTranslation()
		{
			Text = Translations.GetInterfaceString("denshadego_config_title");
			deviceInputBox.Text = Translations.GetInterfaceString("denshadego_input_section");
			buttonCalibrate.Text = Translations.GetInterfaceString("denshadego_calibration_button");
			label_device.Text = Translations.GetInterfaceString("denshadego_device");
			buttonMappingBox.Text = Translations.GetInterfaceString("denshadego_button_section");
			handleMappingBox.Text = Translations.GetInterfaceString("denshadego_handle_section");
			convertnotchesCheck.Text = Translations.GetInterfaceString("denshadego_option_convert");
			minmaxCheck.Text = Translations.GetInterfaceString("denshadego_option_keep_minmax");
			holdbrakeCheck.Text = Translations.GetInterfaceString("denshadego_option_holdbrake");
			buttonSave.Text = Translations.GetInterfaceString("denshadego_save_button");
			buttonCancel.Text = Translations.GetInterfaceString("denshadego_cancel_button");

			buttonselectBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonstartBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonaBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonbBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttoncBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttondBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonupBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttondownBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonleftBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonrightBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");
			buttonpedalBox.Items[0] = Translations.GetInterfaceString("denshadego_command_none");

			label_brakeemg.Text = Translations.QuickReferences.HandleEmergency;
			label_brake8.Text = Translations.QuickReferences.HandleBrake + "8";
			label_brake7.Text = Translations.QuickReferences.HandleBrake + "7";
			label_brake6.Text = Translations.QuickReferences.HandleBrake + "6";
			label_brake5.Text = Translations.QuickReferences.HandleBrake + "5";
			label_brake4.Text = Translations.QuickReferences.HandleBrake + "4";
			label_brake3.Text = Translations.QuickReferences.HandleBrake + "3";
			label_brake2.Text = Translations.QuickReferences.HandleBrake + "2";
			label_brake1.Text = Translations.QuickReferences.HandleBrake + "1";
			label_braken.Text = Translations.QuickReferences.HandleBrakeNull;
			label_power5.Text = Translations.QuickReferences.HandlePower + "5";
			label_power4.Text = Translations.QuickReferences.HandlePower + "4";
			label_power3.Text = Translations.QuickReferences.HandlePower + "3";
			label_power2.Text = Translations.QuickReferences.HandlePower + "2";
			label_power1.Text = Translations.QuickReferences.HandlePower + "1";
			label_powern.Text = Translations.QuickReferences.HandlePowerNull;

			label_up.Text = Translations.GetInterfaceString("denshadego_label_up");
			label_down.Text = Translations.GetInterfaceString("denshadego_label_down");
			label_left.Text = Translations.GetInterfaceString("denshadego_label_left");
			label_right.Text = Translations.GetInterfaceString("denshadego_label_right");
			label_pedal.Text = Translations.GetInterfaceString("denshadego_label_pedal");
			label_buttonup.Text = Translations.GetInterfaceString("denshadego_label_up");
			label_buttondown.Text = Translations.GetInterfaceString("denshadego_label_down");
			label_buttonleft.Text = Translations.GetInterfaceString("denshadego_label_left");
			label_buttonright.Text = Translations.GetInterfaceString("denshadego_label_right");
			label_buttonpedal.Text = Translations.GetInterfaceString("denshadego_label_pedal");
		}

		private void Config_Shown(object sender, EventArgs e)
		{
			// Add connected devices to device list
			ListControllers();

			// Try to select the current device
			if (InputTranslator.activeControllerIndex < deviceBox.Items.Count)
			{
				deviceBox.SelectedIndex = InputTranslator.activeControllerIndex;
			}

			// Set command boxes
			buttonselectBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[0].Command;
			buttonstartBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[1].Command;
			buttonaBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[2].Command;
			buttonbBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[3].Command;
			buttoncBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[4].Command;
			buttondBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[5].Command;
			buttonupBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[6].Command;
			buttondownBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[7].Command;
			buttonleftBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[8].Command;
			buttonrightBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[9].Command;
			buttonpedalBox.SelectedIndex = DenshaDeGoInput.ButtonProperties[10].Command;

			// Set checkboxes
			convertnotchesCheck.Checked = DenshaDeGoInput.convertNotches;
			minmaxCheck.Checked = DenshaDeGoInput.keepMaxMin;
			holdbrakeCheck.Checked = DenshaDeGoInput.mapHoldBrake;
			minmaxCheck.Enabled = DenshaDeGoInput.convertNotches;

			// Start timer
			timer1.Enabled = true;

			// Translate the interface to the current language
			UpdateTranslation();
		}

		private void Config_FormClosed(Object sender, FormClosedEventArgs e)
		{
			// Reload the previous config and close the config dialog
			DenshaDeGoInput.LoadConfig();
		}

		private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			InputTranslator.activeControllerIndex = deviceList.IndexOf(deviceBox.Items[deviceBox.SelectedIndex].ToString());
		}

		private void buttonselectBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[0].Command = buttonselectBox.SelectedIndex;
		}

		private void buttonstartBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[1].Command = buttonstartBox.SelectedIndex;
		}

		private void buttonaBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[2].Command = buttonaBox.SelectedIndex;
		}

		private void buttonbBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[3].Command = buttonbBox.SelectedIndex;
		}

		private void buttoncBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[4].Command = buttoncBox.SelectedIndex;
		}

		private void buttondBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[5].Command = buttondBox.SelectedIndex;
		}

		private void buttonupBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[6].Command = buttonupBox.SelectedIndex;
		}

		private void buttondownBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[7].Command = buttondownBox.SelectedIndex;
		}

		private void buttonleftBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[8].Command = buttonleftBox.SelectedIndex;
		}

		private void buttonrightBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[9].Command = buttonrightBox.SelectedIndex;
		}

		private void buttonpedalBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonProperties[10].Command = buttonpedalBox.SelectedIndex;
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
			ControllerClassic.Calibrate();
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
