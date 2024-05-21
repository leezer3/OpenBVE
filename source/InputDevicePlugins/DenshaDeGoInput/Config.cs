//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020-2024, Marc Riera, The OpenBVE Project
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
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using Timer = System.Timers.Timer;

namespace DenshaDeGoInput
{
	public partial class Config : Form
	{
		/// <summary>
		/// Internal list of connected controllers.
		/// </summary>
		private readonly List<Guid> controllerList = new List<Guid>();

		/// <summary>
		/// Internal list of commands.
		/// </summary>
		private readonly Translations.Command[] commandList;

		private readonly Timer Timer1;

		public Config()
		{
			InitializeComponent();

			// Populate the list of commands
			commandList = new Translations.Command[Translations.CommandInfos.Count + 1];
			commandList[0] = Translations.Command.None;
			for (int i = 0; i < Translations.CommandInfos.Count; i++)
			{
				commandList[i+1] = Translations.CommandInfos.Keys.ElementAt(i);
			}

			// Load language files
			Translations.LoadLanguageFiles(OpenBveApi.Path.CombineDirectory(DenshaDeGoInput.FileSystem.DataFolder, "Languages"));

			// Populate command boxes
			for (int i = 0; i < commandList.Length; i++)
			{
				string commandName = Translations.CommandInfos.TryGetInfo(commandList[i]).Name;
				buttonselectBox.Items.Add(commandName);
				buttonstartBox.Items.Add(commandName);
				buttonaBox.Items.Add(commandName);
				buttonbBox.Items.Add(commandName);
				buttoncBox.Items.Add(commandName);
				buttondBox.Items.Add(commandName);
				buttonupBox.Items.Add(commandName);
				buttondownBox.Items.Add(commandName);
				buttonleftBox.Items.Add(commandName);
				buttonrightBox.Items.Add(commandName);
				buttonldoorBox.Items.Add(commandName);
				buttonrdoorBox.Items.Add(commandName);
				buttonpedalBox.Items.Add(commandName);
				buttona2Box.Items.Add(commandName);
				buttonatsBox.Items.Add(commandName);
			}

			Timer1 = new Timer {Interval = 100};
			Timer1.Elapsed += timer1_Tick;
		}

		/// <summary>
		/// Adds the available controllers to the device dropdown list.
		/// </summary>
		private void ListControllers()
		{
			// Clear the internal and visible lists
			controllerList.Clear();
			deviceBox.Items.Clear();

			foreach (KeyValuePair<Guid, Controller> controller in InputTranslator.Controllers)
			{
				if (controller.Value.IsConnected)
				{
					controllerList.Add(controller.Key);
					deviceBox.Items.Add(controller.Value.ControllerName);
				}
			}

			// Adjust the width of the device dropdown to prevent truncation
			deviceBox.DropDownWidth = deviceBox.Width;
			foreach (var item in deviceBox.Items)
			{
				int currentItemWidth = (int)deviceBox.CreateGraphics().MeasureString(item.ToString(), deviceBox.Font).Width;
				if (currentItemWidth > deviceBox.DropDownWidth)
				{
					deviceBox.DropDownWidth = currentItemWidth;
				}
			}
		}

		/// <summary>
		/// Updates the interface to reflect the input from the controller.
		/// </summary>
		private void UpdateInterface()
		{
			switch (InputTranslator.BrakeNotch)
			{
				case InputTranslator.BrakeNotches.Released:
					label_brake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_brake"}).Replace("[notch]", Translations.QuickReferences.HandleBrakeNull);
					break;
				case InputTranslator.BrakeNotches.Emergency:
					label_brake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_brake"}).Replace("[notch]", Translations.QuickReferences.HandleEmergency);
					break;
				default:
					label_brake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_brake"}).Replace("[notch]", Translations.QuickReferences.HandleBrake + (int)InputTranslator.BrakeNotch);
					break;
			}

			switch (InputTranslator.PowerNotch)
			{
				case InputTranslator.PowerNotches.N:
					label_power.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_power"}).Replace("[notch]", Translations.QuickReferences.HandlePowerNull);
					break;
				default:
					label_power.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_power"}).Replace("[notch]", Translations.QuickReferences.HandlePower + (int)InputTranslator.PowerNotch);
					break;
			}

			switch (InputTranslator.ReverserPosition)
			{
				case InputTranslator.ReverserPositions.Forward:
					label_reverser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleForward);
					break;
				case InputTranslator.ReverserPositions.Backward:
					label_reverser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleBackward);
					break;
				default:
					label_reverser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleNeutral);
					break;
			}

			label_select.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_start.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_a.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_b.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_c.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_d.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_up.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_down.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_left.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_right.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_ldoor.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.LDoor] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_rdoor.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.RDoor] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_pedal.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Pedal] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_a2.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A2] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;
			label_ats.ForeColor = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.ATS] == OpenTK.Input.ButtonState.Pressed ? Color.White : Color.Black;

			if (!InputTranslator.Controllers.ContainsKey(InputTranslator.ActiveControllerGuid))
			{
				buttonCalibrate.Visible = false;
				label_select.Visible = false;
				label_start.Visible = false;
				label_a.Visible = false;
				label_b.Visible = false;
				label_c.Visible = false;
				label_d.Visible = false;
				label_up.Visible = false;
				label_down.Visible = false;
				label_left.Visible = false;
				label_right.Visible = false;
				label_ldoor.Visible = false;
				label_rdoor.Visible = false;
				label_pedal.Visible = false;
				label_a2.Visible = false;
				label_ats.Visible = false;
			}
			else
			{
				buttonCalibrate.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].RequiresCalibration;
				label_select.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.Select);
				label_start.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.Start);
				label_a.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.A);
				label_b.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.B);
				label_c.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.C);
				label_d.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.D);
				label_up.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.DPad);
				label_down.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.DPad);
				label_left.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.DPad);
				label_right.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.DPad);
				label_ldoor.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.LDoor);
				label_rdoor.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.RDoor);
				label_pedal.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.Pedal);
				label_a2.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.A2);
				label_ats.Visible = InputTranslator.Controllers[InputTranslator.ActiveControllerGuid].Buttons.HasFlag(Controller.ControllerButtons.ATS);
			}
			
		}

		/// <summary>
		/// Retranslates the configuration interface.
		/// </summary>
		private void UpdateTranslation()
		{
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","config_title"});
			deviceInputBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","input_section"});
			buttonCalibrate.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","calibration_button"});
			label_device.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","device"});
			buttonMappingBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","button_section"});
			handleMappingBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","handle_section"});
			convertnotchesCheck.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","option_convert"});
			minmaxCheck.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","option_keep_minmax"});
			holdbrakeCheck.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","option_holdbrake"});
			buttonSave.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","save_button"});
			buttonCancel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","cancel_button"});

			buttonselectBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonstartBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonaBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonbBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttoncBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttondBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonupBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttondownBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonleftBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonrightBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonldoorBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonrdoorBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonpedalBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttona2Box.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });
			buttonatsBox.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "command_none" });

			label_up.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_up"});
			label_down.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_down"});
			label_left.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_left"});
			label_right.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_right"});
			label_pedal.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_pedal"});
			label_ldoor.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_ldoor"});
			label_rdoor.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_rdoor"});
			label_buttonup.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_up"});
			label_buttondown.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_down"});
			label_buttonleft.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_left"});
			label_buttonright.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_right"});
			label_buttonldoor.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_ldoor"});
			label_buttonrdoor.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","label_rdoor"});
			label_buttonpedal.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "label_pedal" });
			label_buttona2.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "label_a2" });
			label_buttonats.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "denshadego", "label_ats" });

			linkLabel_driver.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"denshadego","linkLabel_driver"});
		}

		private void Config_Shown(object sender, EventArgs e)
		{
			// Add connected devices to device list
			InputTranslator.Update();
			ListControllers();

			// Try to select the current device
			if (controllerList.Contains(InputTranslator.ActiveControllerGuid))
			{
				deviceBox.SelectedIndex = controllerList.IndexOf(InputTranslator.ActiveControllerGuid);
			}

			// Set command boxes
			buttonselectBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Select].Command);
			buttonstartBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Start].Command);
			buttonaBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.A].Command);
			buttonbBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.B].Command);
			buttoncBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.C].Command);
			buttondBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.D].Command);
			buttonupBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Up].Command);
			buttondownBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Down].Command);
			buttonleftBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Left].Command);
			buttonrightBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Right].Command);
			buttonldoorBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.LDoor].Command);
			buttonrdoorBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.RDoor].Command);
			buttonpedalBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Pedal].Command);
			buttona2Box.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.A2].Command);
			buttonatsBox.SelectedIndex = Array.IndexOf(commandList, DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.ATS].Command);


			// Set checkboxes
			convertnotchesCheck.Checked = DenshaDeGoInput.ConvertNotches;
			minmaxCheck.Checked = DenshaDeGoInput.KeepMaxMin;
			holdbrakeCheck.Checked = DenshaDeGoInput.MapHoldBrake;
			minmaxCheck.Enabled = DenshaDeGoInput.ConvertNotches;
			// Start timer
			Timer1.Enabled = true;

			// Translate the interface to the current language
			UpdateTranslation();
		}

		private void Config_FormClosed(Object sender, FormClosedEventArgs e)
		{
			// Reload the previous config and close the config dialog
			DenshaDeGoInput.LoadConfig();
			Timer1.Enabled = false;
		}

		private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			InputTranslator.Update();
			InputTranslator.ActiveControllerGuid = controllerList[deviceBox.SelectedIndex];
		}

		private void buttonselectBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Select].Command = buttonselectBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonselectBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonstartBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Start].Command = buttonstartBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonstartBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonaBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.A].Command = buttonaBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonaBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonbBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.B].Command = buttonbBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonbBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttoncBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.C].Command = buttoncBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttoncBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttondBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.D].Command = buttondBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttondBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonupBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Up].Command = buttonupBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonupBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttondownBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Down].Command = buttondownBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttondownBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonleftBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Left].Command = buttonleftBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonleftBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonrightBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Right].Command = buttonrightBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonrightBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonldoorBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.LDoor].Command = buttonldoorBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonldoorBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonrdoorBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.RDoor].Command = buttonrdoorBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonrdoorBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonpedalBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.Pedal].Command = buttonpedalBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonpedalBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttona2Box_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.A2].Command = buttona2Box.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttona2Box.SelectedIndex - 1) : Translations.Command.None;
		}

		private void buttonatsBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ButtonCommands[(int)InputTranslator.ControllerButton.ATS].Command = buttonatsBox.SelectedIndex != 0 ? Translations.CommandInfos.Keys.ElementAt(buttonatsBox.SelectedIndex - 1) : Translations.Command.None;
		}

		private void convertnotchesCheck_CheckedChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.ConvertNotches = convertnotchesCheck.Checked;
			minmaxCheck.Enabled = DenshaDeGoInput.ConvertNotches;
		}

		private void minmaxCheck_CheckedChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.KeepMaxMin = minmaxCheck.Checked;
		}

		private void holdbrakeCheck_CheckedChanged(object sender, EventArgs e)
		{
			DenshaDeGoInput.MapHoldBrake = holdbrakeCheck.Checked;
		}

		private void buttonCalibrate_Click(object sender, EventArgs e)
		{
			if (controllerList.Count == 0)
			{
				return;
			}
			Timer1.Stop();
			ClassicController.Calibrate();
			Timer1.Start();
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

		private void linkLabel_LinkClicked(object sender, EventArgs e)
		{
			Help helpForm = new Help();
			helpForm.ShowDialog(this);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			InputTranslator.Update();
			//grab a random control, as we need something from the UI thread to check for invoke
			//WinForms is a pain
			if (buttonCalibrate.InvokeRequired)
			{
				buttonCalibrate.Invoke((MethodInvoker) UpdateInterface);
			}
			else
			{
				UpdateInterface();	
			}
			
		}

	}
}
