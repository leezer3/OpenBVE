using System;
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

			// Add commands to buttons
			for (int i = 0; i < Translations.CommandInfos.Length; i++)
			{
				buttonaBox.Items.Add(Translations.CommandInfos[i].Name);
				buttonbBox.Items.Add(Translations.CommandInfos[i].Name);
				buttoncBox.Items.Add(Translations.CommandInfos[i].Name);
				buttonstartBox.Items.Add(Translations.CommandInfos[i].Name);
				buttonselectBox.Items.Add(Translations.CommandInfos[i].Name);
			}
		}

		private void ListControllers()
		{
			for (int i = 0; i < 10; i++)
			{
				JoystickState state = Joystick.GetState(i);
				JoystickCapabilities capabilities = Joystick.GetCapabilities(i);
				InputTranslator.ControllerModels model = InputTranslator.GetControllerModel(state);
				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				if (capabilities.ButtonCount > 0 && model != InputTranslator.ControllerModels.None)
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
		}

		private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			InputTranslator.activeControllerIndex = deviceBox.SelectedIndex;
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
