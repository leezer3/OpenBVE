//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Marc Riera, The OpenBVE Project
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using Timer = System.Timers.Timer;

namespace ZuikiInput
{
	public partial class Config : Form
	{
		/// <summary>The path to the file containing the configuration.</summary>
		private const string configFilename = "ZuikiInput.json";

		/// <summary>The list of recognised controllers.</summary>
		private Dictionary<Guid, Controller> controllers;

		/// <summary>The GUID of the selected controller.</summary>
		private Guid selectedControllerGuid = Guid.Empty;

		/// <summary>The state of the train doors.</summary>
		private DoorStates doorState = DoorStates.None;

		/// <summary>The list of controller profiles.</summary>
		public Dictionary<Guid, ControllerProfile> ControllerProfiles;

		/// <summary>Internal list of commands.</summary>
		private readonly Translations.Command[] commandList;

		/// <summary>Timer used to show controller input on the config form.</summary>
		private readonly Timer inputTimer;

		public Config()
		{
			InitializeComponent();

			// Populate the list of commands
			commandList = new Translations.Command[Translations.CommandInfos.Count + 1];
			commandList[0] = Translations.Command.None;
			for (int i = 0; i < Translations.CommandInfos.Count; i++)
			{
				commandList[i + 1] = Translations.CommandInfos.Keys.ElementAt(i);
			}

			// Load language files
			Translations.LoadLanguageFiles(OpenBveApi.Path.CombineDirectory(ZuikiInput.FileSystem.DataFolder, "Languages"));

			// Populate command boxes
			for (int i = 0; i < commandList.Length; i++)
			{
				string commandName = Translations.CommandInfos.TryGetInfo(commandList[i]).Name;
				comboBox_a.Items.Add(commandName);
				comboBox_b.Items.Add(commandName);
				comboBox_x.Items.Add(commandName);
				comboBox_y.Items.Add(commandName);
				comboBox_minus.Items.Add(commandName);
				comboBox_plus.Items.Add(commandName);
				comboBox_screenshot.Items.Add(commandName);
				comboBox_home.Items.Add(commandName);
				comboBox_up.Items.Add(commandName);
				comboBox_down.Items.Add(commandName);
				comboBox_left.Items.Add(commandName);
				comboBox_right.Items.Add(commandName);
				comboBox_l.Items.Add(commandName);
				comboBox_r.Items.Add(commandName);
				comboBox_zl.Items.Add(commandName);
				comboBox_zr.Items.Add(commandName);
				comboBox_square.Items.Add(commandName);
				comboBox_ats.Items.Add(commandName);
				comboBox_pantograph.Items.Add(commandName);
				comboBox_ebReset.Items.Add(commandName);
				comboBox_hillStart.Items.Add(commandName);
				comboBox_horn.Items.Add(commandName);
				comboBox_pedalLight.Items.Add(commandName);
				comboBox_pedalStrong.Items.Add(commandName);
			}

			// Initialize the list of controllers
			controllers = new Dictionary<Guid, Controller>();

			// Initialize the list of controller profiles
			ControllerProfiles = new Dictionary<Guid, ControllerProfile>();
			LoadConfig();

			// Initialize the timer
			inputTimer = new Timer { Interval = 100 };
			inputTimer.Elapsed += timer1_Tick;

			// Show the lamp link under Linux/WINE
			switch (ZuikiInput.CurrentHost.Platform)
			{
				case HostPlatform.GNULinux:
				case HostPlatform.WINE:
				case HostPlatform.FreeBSD:
					linkLabel_lamps.Visible = true;
					break;
			}
		}

		/// <summary>Loads the plugin settings from the config file.</summary>
		internal void LoadConfig()
		{
			string configFolder = OpenBveApi.Path.CombineDirectory(ZuikiInput.FileSystem.SettingsFolder, "1.5.0");
			string configFile = OpenBveApi.Path.CombineFile(configFolder, configFilename);
			if (File.Exists(configFile))
			{
				try
				{
					string json = File.ReadAllText(configFile);
					{
						JsonSerializer serializer = new JsonSerializer();
						serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
						using (StreamReader sr = new StreamReader(configFile))
						using (JsonReader reader = new JsonTextReader(sr))
						{
							Dictionary<Guid, ControllerProfile> profiles = serializer.Deserialize<Dictionary<Guid, ControllerProfile>>(reader);
							if (profiles != null)
							{
								// At least one profile was deserialized, load the config
								ControllerProfiles = profiles;
								foreach (ControllerProfile p in profiles.Values)
								{
									// Check if the list of button controls has the correct length, resize it otherwise
									if (p.ButtonControls.Length != ZuikiInput.ButtonControlsCount)
									{
										Array.Resize(ref p.ButtonControls, ZuikiInput.ButtonControlsCount);
									}
								}
							}
						}
					}
				}
				catch
				{
					MessageBox.Show("An error occured whilst loading the options for ZUIKI Input Plugin from disk." + Environment.NewLine +
									"The configuration file may be corrupt.");				}
			}
		}

		/// <summary>Saves the plugin settings to the config file.</summary>
		internal void SaveConfig()
		{
			string configFolder = OpenBveApi.Path.CombineDirectory(ZuikiInput.FileSystem.SettingsFolder, "1.5.0");
			if (!Directory.Exists(configFolder))
			{
				Directory.CreateDirectory(configFolder);
			}
			string configFile = OpenBveApi.Path.CombineFile(configFolder, configFilename);
			try
			{
				JsonSerializer serializer = new JsonSerializer();
				serializer.NullValueHandling = NullValueHandling.Ignore;
				serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
				using (StreamWriter sw = new StreamWriter(configFile))
				using (JsonWriter writer = new JsonTextWriter(sw))
				{
					writer.Formatting = Formatting.Indented;
					serializer.Serialize(writer, ControllerProfiles);
				}
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options for ZUIKI Input Plugin to disk." + Environment.NewLine +
								"Please check you have write permission.");
			}
		}

		/// <summary>Configures the correct mappings for a controller according to the controller profile.</summary>
		internal void ConfigureMappings(VehicleSpecs specs, Controller controller)
		{
			// Get train capabilities
			Controller.ControllerCapabilities capabilities = controller.Capabilities;

			// Get controller profile; if there is no profile, create one
			if (!ControllerProfiles.ContainsKey(controller.Guid))
			{
				ControllerProfiles.Add(controller.Guid, new ControllerProfile());
			}
			ControllerProfile profile = ControllerProfiles[controller.Guid];

			if (!profile.ConvertNotches)
			{
				// The notches are not supposed to be converted
				// Brake notches
				if (profile.MapHoldBrake && specs.HasHoldBrake)
				{
					profile.BrakeControls[0].Command = Translations.Command.BrakeAnyNotch;
					profile.BrakeControls[0].Option = 0;
					profile.BrakeControls[1].Command = Translations.Command.HoldBrake;
					for (int i = 2; i <= capabilities.BrakeNotches + 1; i++)
					{
						profile.BrakeControls[i].Command = Translations.Command.BrakeAnyNotch;
						profile.BrakeControls[i].Option = i - 1;
					}
				}
				else
				{
					for (int i = 0; i <= capabilities.BrakeNotches + 1; i++)
					{
						profile.BrakeControls[i].Command = Translations.Command.BrakeAnyNotch;
						profile.BrakeControls[i].Option = i;
					}
				}
				// Emergency brake, only if the train has the same or less notches than the controller
				if (specs.BrakeNotches <= capabilities.BrakeNotches)
				{
					profile.BrakeControls[(int)ControllerState.BrakeNotches.Emergency].Command = Translations.Command.BrakeEmergency;
				}
				// Power notches
				for (int i = 0; i <= capabilities.PowerNotches; i++)
				{
					profile.PowerControls[i].Command = Translations.Command.PowerAnyNotch;
					profile.PowerControls[i].Option = i;
				}
			}
			else
			{
				// The notches are supposed to be converted
				// Brake notches
				if (profile.MapHoldBrake && specs.HasHoldBrake)
				{
					double brakeStep = (specs.BrakeNotches - 1) / (double)(capabilities.BrakeNotches - 1);
					profile.BrakeControls[0].Command = Translations.Command.BrakeAnyNotch;
					profile.BrakeControls[0].Option = 0;
					profile.BrakeControls[1].Command = Translations.Command.HoldBrake;
					for (int i = 2; i < capabilities.BrakeNotches + 1; i++)
					{
						profile.BrakeControls[i].Command = Translations.Command.BrakeAnyNotch;
						profile.BrakeControls[i].Option = (int)Math.Round(brakeStep * (i - 1), MidpointRounding.AwayFromZero);
						if (i > 0 && profile.BrakeControls[i].Option == 0)
						{
							profile.BrakeControls[i].Option = 1;
						}
						if (profile.KeepMinMax && i == 2)
						{
							profile.BrakeControls[i].Option = 1;
						}
						if (profile.KeepMinMax && i == capabilities.BrakeNotches)
						{
							profile.BrakeControls[i].Option = specs.BrakeNotches - 1;
						}
					}
				}
				else
				{
					double brakeStep = specs.BrakeNotches / (double)capabilities.BrakeNotches;
					for (int i = 0; i < capabilities.BrakeNotches + 1; i++)
					{
						profile.BrakeControls[i].Command = Translations.Command.BrakeAnyNotch;
						profile.BrakeControls[i].Option = (int)Math.Round(brakeStep * i, MidpointRounding.AwayFromZero);
						if (i > 0 && profile.BrakeControls[i].Option == 0)
						{
							profile.BrakeControls[i].Option = 1;
						}
						if (profile.KeepMinMax && i == 1)
						{
							profile.BrakeControls[i].Option = 1;
						}
						if (profile.KeepMinMax && i == capabilities.BrakeNotches)
						{
							profile.BrakeControls[i].Option = specs.BrakeNotches;
						}
					}
				}
				// Emergency brake
				profile.BrakeControls[(int)ControllerState.BrakeNotches.Emergency].Command = Translations.Command.BrakeEmergency;
				// Power notches
				double powerStep = specs.PowerNotches / (double)capabilities.PowerNotches;
				for (int i = 0; i < capabilities.PowerNotches + 1; i++)
				{
					profile.PowerControls[i].Command = Translations.Command.PowerAnyNotch;
					profile.PowerControls[i].Option = (int)Math.Round(powerStep * i, MidpointRounding.AwayFromZero);
					if (i > 0 && profile.PowerControls[i].Option == 0)
					{
						profile.PowerControls[i].Option = 1;
					}
					if (profile.KeepMinMax && i == 1)
					{
						profile.PowerControls[i].Option = 1;
					}
					if (profile.KeepMinMax && i == capabilities.PowerNotches)
					{
						profile.PowerControls[i].Option = specs.PowerNotches;
					}
				}
			}

			if (specs.BrakeType == BrakeTypes.AutomaticAirBrake)
			{
				// Trains with an air brake are mapped differently
				double brakeStep = 3 / (double)(capabilities.BrakeNotches);
				for (int i = 1; i < capabilities.BrakeNotches + 1; i++)
				{
					profile.BrakeControls[i].Command = Translations.Command.BrakeAnyNotch;
					int notch = ((int)Math.Round(brakeStep * i, MidpointRounding.AwayFromZero) - 1);
					profile.BrakeControls[i].Option = notch >= 0 ? notch : 0;
				}
			}

			for (int i = 0; i < profile.ReverserControls.Length; i++)
			{
				profile.ReverserControls[i].Command = Translations.Command.ReverserAnyPosition;
				profile.ReverserControls[i].Option = i - 1;
			}
		}

		/// <summary>Adds the available controllers to the device dropdown list.</summary>
		private void ListControllers()
		{
			// Clear the internal and visible lists
			comboBox_device.Items.Clear();
			controllers.Clear();
			MasconController.GetControllers(controllers);

			if (controllers.Count > 0)
			{
				selectedControllerGuid = controllers.Keys.First();
			}

			foreach (KeyValuePair<Guid, Controller> controller in controllers)
			{
				comboBox_device.Items.Add(controller.Value.Name);
				if (!ControllerProfiles.ContainsKey(controller.Key))
				{
					// If there is no profile for the active controller, create one
					ControllerProfiles.Add(controller.Key, new ControllerProfile());

				}
			}

			// Adjust the width of the device dropdown to prevent truncation
			comboBox_device.DropDownWidth = comboBox_device.Width;
			foreach (var item in comboBox_device.Items)
			{
				int currentItemWidth = (int)comboBox_device.CreateGraphics().MeasureString(item.ToString(), comboBox_device.Font).Width;
				if (currentItemWidth > comboBox_device.DropDownWidth)
				{
					comboBox_device.DropDownWidth = currentItemWidth;
				}
			}
		}

		/// <summary>Updates the interface to reflect the input from the controller.</summary>
		private void UpdateInterface()
		{
			ControllerState.BrakeNotches brakeNotch = controllers[selectedControllerGuid].State.BrakeNotch;
			ControllerState.PowerNotches powerNotch = controllers[selectedControllerGuid].State.PowerNotch;
			ControllerState.ReverserPositions reverserPosition = controllers[selectedControllerGuid].State.ReverserPosition;
			Controller.ControllerButtons buttons = controllers[selectedControllerGuid].State.PressedButtons;

			switch (brakeNotch)
			{
				case ControllerState.BrakeNotches.Released:
					label_brakeNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_brake" }).Replace("[notch]", Translations.QuickReferences.HandleBrakeNull);
					break;
				case ControllerState.BrakeNotches.Emergency:
					label_brakeNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_brake" }).Replace("[notch]", Translations.QuickReferences.HandleEmergency);
					break;
				default:
					label_brakeNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_brake" }).Replace("[notch]", Translations.QuickReferences.HandleBrake + (int)brakeNotch);
					break;
			}

			switch (powerNotch)
			{
				case ControllerState.PowerNotches.N:
					label_powerNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_power" }).Replace("[notch]", Translations.QuickReferences.HandlePowerNull);
					break;
				default:
					label_powerNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_power" }).Replace("[notch]", Translations.QuickReferences.HandlePower + (int)powerNotch);
					break;
			}

			switch (reverserPosition)
			{
				case ControllerState.ReverserPositions.Forward:
					label_reverserNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleForward);
					break;
				case ControllerState.ReverserPositions.Backward:
					label_reverserNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleBackward);
					break;
				default:
					label_reverserNotch.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleNeutral);
					break;
			}

			label_a.BackColor = buttons.HasFlag(Controller.ControllerButtons.A) ? Color.Black : Color.White;
			label_a.ForeColor = buttons.HasFlag(Controller.ControllerButtons.A) ? Color.White : Color.Black;
			label_b.BackColor = buttons.HasFlag(Controller.ControllerButtons.B) ? Color.Black : Color.White;
			label_b.ForeColor = buttons.HasFlag(Controller.ControllerButtons.B) ? Color.White : Color.Black;
			label_x.BackColor = buttons.HasFlag(Controller.ControllerButtons.X) ? Color.Black : Color.White;
			label_x.ForeColor = buttons.HasFlag(Controller.ControllerButtons.X) ? Color.White : Color.Black;
			label_y.BackColor = buttons.HasFlag(Controller.ControllerButtons.Y) ? Color.Black : Color.White;
			label_y.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Y) ? Color.White : Color.Black;
			label_minus.BackColor = buttons.HasFlag(Controller.ControllerButtons.Minus) ? Color.Black : Color.White;
			label_minus.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Minus) ? Color.White : Color.Black;
			label_plus.BackColor = buttons.HasFlag(Controller.ControllerButtons.Plus) ? Color.Black : Color.White;
			label_plus.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Plus) ? Color.White : Color.Black;
			label_screenshot.BackColor = buttons.HasFlag(Controller.ControllerButtons.Screenshot) ? Color.Black : Color.White;
			label_screenshot.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Screenshot) ? Color.White : Color.Black;
			label_home.BackColor = buttons.HasFlag(Controller.ControllerButtons.Home) ? Color.Black : Color.White;
			label_home.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Home) ? Color.White : Color.Black;
			label_up.BackColor = buttons.HasFlag(Controller.ControllerButtons.Up) ? Color.Black : Color.White;
			label_up.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Up) ? Color.White : Color.Black;
			label_down.BackColor = buttons.HasFlag(Controller.ControllerButtons.Down) ? Color.Black : Color.White;
			label_down.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Down) ? Color.White : Color.Black;
			label_left.BackColor = buttons.HasFlag(Controller.ControllerButtons.Left) ? Color.Black : Color.White;
			label_left.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Left) ? Color.White : Color.Black;
			label_right.BackColor = buttons.HasFlag(Controller.ControllerButtons.Right) ? Color.Black : Color.White;
			label_right.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Right) ? Color.White : Color.Black;
			label_l.BackColor = buttons.HasFlag(Controller.ControllerButtons.L) ? Color.Black : Color.White;
			label_l.ForeColor = buttons.HasFlag(Controller.ControllerButtons.L) ? Color.White : Color.Black;
			label_r.BackColor = buttons.HasFlag(Controller.ControllerButtons.R) ? Color.Black : Color.White;
			label_r.ForeColor = buttons.HasFlag(Controller.ControllerButtons.R) ? Color.White : Color.Black;
			label_zl.BackColor = buttons.HasFlag(Controller.ControllerButtons.ZL) ? Color.Black : Color.White;
			label_zl.ForeColor = buttons.HasFlag(Controller.ControllerButtons.ZL) ? Color.White : Color.Black;
			label_zr.BackColor = buttons.HasFlag(Controller.ControllerButtons.ZR) ? Color.Black : Color.White;
			label_zr.ForeColor = buttons.HasFlag(Controller.ControllerButtons.ZR) ? Color.White : Color.Black;
			label_square.BackColor = buttons.HasFlag(Controller.ControllerButtons.Square) ? Color.Black : Color.White;
			label_square.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Square) ? Color.White : Color.Black;
			label_ats.BackColor = buttons.HasFlag(Controller.ControllerButtons.Ats) ? Color.Black : Color.White;
			label_ats.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Ats) ? Color.White : Color.Black;
			label_pantograph.BackColor = buttons.HasFlag(Controller.ControllerButtons.PantographDown) ? Color.Black : Color.White;
			label_pantograph.ForeColor = buttons.HasFlag(Controller.ControllerButtons.PantographDown) ? Color.White : Color.Black;
			label_ebReset.BackColor = buttons.HasFlag(Controller.ControllerButtons.EbReset) ? Color.Black : Color.White;
			label_ebReset.ForeColor = buttons.HasFlag(Controller.ControllerButtons.EbReset) ? Color.White : Color.Black;
			label_hillStart.BackColor = buttons.HasFlag(Controller.ControllerButtons.HillStart) ? Color.Black : Color.White;
			label_hillStart.ForeColor = buttons.HasFlag(Controller.ControllerButtons.HillStart) ? Color.White : Color.Black;
			label_horn.BackColor = buttons.HasFlag(Controller.ControllerButtons.Horn) ? Color.Black : Color.White;
			label_horn.ForeColor = buttons.HasFlag(Controller.ControllerButtons.Horn) ? Color.White : Color.Black;
			label_pedalLight.BackColor = buttons.HasFlag(Controller.ControllerButtons.PedalLight) ? Color.Black : Color.White;
			label_pedalLight.ForeColor = buttons.HasFlag(Controller.ControllerButtons.PedalLight) ? Color.White : Color.Black;
			label_pedalStrong.BackColor = buttons.HasFlag(Controller.ControllerButtons.PedalStrong) ? Color.Black : Color.White;
			label_pedalStrong.ForeColor = buttons.HasFlag(Controller.ControllerButtons.PedalStrong) ? Color.White : Color.Black;
		}

		/// <summary>Retranslates the configuration interface.</summary>
		private void UpdateTranslation()
		{
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","config_title"});
			groupBox_handleInput.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","input_section"});
			label_device.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","device"});
			groupBox_buttonMapping.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "button_section" });
			groupBox_handleMapping.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","handle_section"});
			checkBox_convertNotches.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","option_convert"});
			checkBox_minMax.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","option_keep_minmax"});
			checkBox_holdBrake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","option_holdbrake"});
			button_save.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","save_button"});
			button_cancel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"zuiki","cancel_button"});

			comboBox_a.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_b.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_x.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_y.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_minus.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_plus.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_screenshot.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_home.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_up.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_down.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_left.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_right.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_l.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_r.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_zl.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_zr.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_square.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_ats.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_pantograph.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_ebReset.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_hillStart.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_horn.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_pedalLight.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });
			comboBox_pedalStrong.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "command_none" });

			linkLabel_lamps.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "zuiki", "lamp_link" });
		}

		private void Config_Shown(object sender, EventArgs e)
		{
			// Add connected devices to device list
			ListControllers();

			// Try to select the selected device
			if (selectedControllerGuid != Guid.Empty)
			{
				comboBox_device.SelectedIndex = controllers.Keys.ToList().IndexOf(selectedControllerGuid);
			}

			// Start timer
			inputTimer.Enabled = true;

			// Translate the interface to the current language
			UpdateTranslation();
		}

		private void Config_FormClosed(Object sender, FormClosedEventArgs e)
		{
			// Reload the previous config
			LoadConfig();

			inputTimer.Enabled = false;

			// Close the controller
			if (selectedControllerGuid != Guid.Empty)
			{
				controllers[selectedControllerGuid].Close();
			}
		}

		private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			selectedControllerGuid = controllers.Keys.ToList()[comboBox_device.SelectedIndex];
			controllers[selectedControllerGuid].Update(doorState);

			ControllerProfile profile = ControllerProfiles[selectedControllerGuid];

			// Enable mapping boxes
			groupBox_handleMapping.Enabled = true;
			groupBox_buttonMapping.Enabled = true;

			// Set command boxes
			comboBox_a.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.A, 2)].Command);
			comboBox_b.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.B, 2)].Command);
			comboBox_x.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.X, 2)].Command);
			comboBox_y.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Y, 2)].Command);
			comboBox_minus.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Minus, 2)].Command);
			comboBox_plus.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Plus, 2)].Command);
			comboBox_screenshot.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Screenshot, 2)].Command);
			comboBox_home.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Home, 2)].Command);
			comboBox_up.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Up, 2)].Command);
			comboBox_down.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Down, 2)].Command);
			comboBox_left.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Left, 2)].Command);
			comboBox_right.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Right, 2)].Command);
			comboBox_l.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.L, 2)].Command);
			comboBox_r.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.R, 2)].Command);
			comboBox_zl.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.ZL, 2)].Command);
			comboBox_zr.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.ZR, 2)].Command);
			comboBox_square.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Square, 2)].Command);
			comboBox_ats.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Ats, 2)].Command);
			comboBox_pantograph.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.PantographDown, 2)].Command);
			comboBox_ebReset.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.EbReset, 2)].Command);
			comboBox_hillStart.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.HillStart, 2)].Command);
			comboBox_horn.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Horn, 2)].Command);
			comboBox_pedalLight.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.PedalLight, 2)].Command);
			comboBox_pedalStrong.SelectedIndex = Array.IndexOf(commandList, profile.ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.PedalStrong, 2)].Command);

			// Set checkboxes
			checkBox_convertNotches.Checked = profile.ConvertNotches;
			checkBox_minMax.Checked = profile.KeepMinMax;
			checkBox_holdBrake.Checked = profile.MapHoldBrake;
			checkBox_minMax.Enabled = profile.ConvertNotches;

			// Enable only the buttons of the current controller
			panel_a.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.A);
			panel_b.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.B);
			panel_x.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.X);
			panel_y.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Y);
			panel_minus.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Minus);
			panel_plus.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Plus);
			panel_screenshot.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Screenshot);
			panel_home.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Home);
			panel_up.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Up);
			panel_down.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Down);
			panel_left.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Left);
			panel_right.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Right);
			panel_l.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.L);
			panel_r.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.R);
			panel_zl.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.ZL);
			panel_zr.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.ZR);
			panel_square.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Square);
			panel_ats.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Ats);
			panel_pantograph.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.PantographDown);
			panel_ebReset.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.EbReset);
			panel_hillStart.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.HillStart);
			panel_horn.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.Horn);
			panel_pedalLight.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.PedalLight);
			panel_pedalStrong.Enabled = controllers[selectedControllerGuid].Capabilities.Buttons.HasFlag(Controller.ControllerButtons.PedalStrong);
		}

		private void checkBox_convertNotches_CheckedChanged(object sender, EventArgs e)
		{
			ControllerProfiles[selectedControllerGuid].ConvertNotches = checkBox_convertNotches.Checked;
			checkBox_minMax.Enabled = ControllerProfiles[selectedControllerGuid].ConvertNotches;
		}

		private void checkBox_minMax_CheckedChanged(object sender, EventArgs e)
		{
			ControllerProfiles[selectedControllerGuid].KeepMinMax = checkBox_minMax.Checked;
		}

		private void checkBox_holdBrake_CheckedChanged(object sender, EventArgs e)
		{
			ControllerProfiles[selectedControllerGuid].MapHoldBrake = checkBox_holdBrake.Checked;
		}

		private void button_save_Click(object sender, EventArgs e)
		{
			// Save the config and close the config dialog
			SaveConfig();
			Close();
		}

		private void button_cancel_Click(object sender, EventArgs e)
		{
			// Reload the previous config and close the config dialog
			LoadConfig();
			Close();
		}

		private void linkLabel_lamps_LinkClicked(object sender, EventArgs e)
		{
			Help helpForm = new Help();
			helpForm.ShowDialog(this);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			controllers[selectedControllerGuid].Update(doorState);

			//grab a random control, as we need something from the UI thread to check for invoke
			//WinForms is a pain
			if (button_cancel.InvokeRequired)
			{
				button_cancel.Invoke((MethodInvoker) UpdateInterface);
			}
			else
			{
				UpdateInterface();	
			}
			
		}

        private void comboBox_a_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.A, 2)].Command = commandList[comboBox_a.SelectedIndex];
        }

        private void comboBox_b_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.B, 2)].Command = commandList[comboBox_b.SelectedIndex];
		}

		private void comboBox_x_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.X, 2)].Command = commandList[comboBox_x.SelectedIndex];
		}

        private void comboBox_y_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Y, 2)].Command = commandList[comboBox_y.SelectedIndex];
		}

        private void comboBox_minus_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Minus, 2)].Command = commandList[comboBox_minus.SelectedIndex];
		}

        private void comboBox_plus_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Plus, 2)].Command = commandList[comboBox_plus.SelectedIndex];
		}

        private void comboBox_screenshot_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Screenshot, 2)].Command = commandList[comboBox_screenshot.SelectedIndex];
		}

        private void comboBox_home_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Home, 2)].Command = commandList[comboBox_home.SelectedIndex];
		}

        private void comboBox_up_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Up, 2)].Command = commandList[comboBox_up.SelectedIndex];
		}

        private void comboBox_down_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Down, 2)].Command = commandList[comboBox_down.SelectedIndex];
		}

        private void comboBox_left_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Left, 2)].Command = commandList[comboBox_left.SelectedIndex];
		}

        private void comboBox_right_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Right, 2)].Command = commandList[comboBox_right.SelectedIndex];
		}

        private void comboBox_l_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.L, 2)].Command = commandList[comboBox_l.SelectedIndex];
		}

        private void comboBox_r_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.R, 2)].Command = commandList[comboBox_r.SelectedIndex];
		}

        private void comboBox_zl_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.ZL, 2)].Command = commandList[comboBox_zl.SelectedIndex];
		}

        private void comboBox_zr_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.ZR, 2)].Command = commandList[comboBox_zr.SelectedIndex];
		}

        private void comboBox_square_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Square, 2)].Command = commandList[comboBox_square.SelectedIndex];
		}

        private void comboBox_ats_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Ats, 2)].Command = commandList[comboBox_ats.SelectedIndex];
		}

        private void comboBox_pantograph_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.PantographDown, 2)].Command = commandList[comboBox_pantograph.SelectedIndex];
		}

        private void comboBox_ebReset_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.EbReset, 2)].Command = commandList[comboBox_ebReset.SelectedIndex];
		}

        private void comboBox_hillStart_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.HillStart, 2)].Command = commandList[comboBox_hillStart.SelectedIndex];
		}

        private void comboBox_horn_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.Horn, 2)].Command = commandList[comboBox_horn.SelectedIndex];
		}

        private void comboBox_pedalLight_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.PedalLight, 2)].Command = commandList[comboBox_pedalLight.SelectedIndex];
		}

        private void comboBox_pedalStrong_SelectedIndexChanged(object sender, EventArgs e)
        {
			ControllerProfiles[selectedControllerGuid].ButtonControls[(int)Math.Log((int)Controller.ControllerButtons.PedalStrong, 2)].Command = commandList[comboBox_pedalStrong.SelectedIndex];
		}
    }
}
