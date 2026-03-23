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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using Timer = System.Timers.Timer;

namespace KatoInput
{
	public partial class Config : Form
	{
		/// <summary>The path to the file containing the configuration.</summary>
		private const string configFilename = "KatoInput.xml";

		/// <summary>Struct containing all the settings, for serialization.</summary>
		public struct KatoInputConfiguration
		{
			/// <summary>Whether to convert the handle notches to match the driver's train.</summary>
			public bool ConvertNotches;

			/// <summary>Whether to assign the minimum and maximum notches to the first and last notches, respectively.</summary>
			public bool KeepMinMax;

			/// <summary>Whether to map the hold brake to B1.</summary>
			public bool MapHoldBrake;
		}

		/// <summary>The configuration for the plugin.</summary>
		internal KatoInputConfiguration Configuration;

		/// <summary>The list of recognised controllers.</summary>
		private Dictionary<Guid, Controller> controllers;

		/// <summary>The GUID of the selected controller.</summary>
		private Guid selectedControllerGuid = Guid.Empty;

		/// <summary>Timer used to show controller input on the config form.</summary>
		private readonly Timer inputTimer;

		public Config()
		{
			InitializeComponent();

			// Load language files
			Translations.LoadLanguageFiles(OpenBveApi.Path.CombineDirectory(KatoInput.FileSystem.DataFolder, "Languages"));

			// Initialize the list of controllers
			controllers = new Dictionary<Guid, Controller>();

			// Initialize the timer
			inputTimer = new Timer { Interval = 100 };
			inputTimer.Elapsed += timer1_Tick;
		}

		/// <summary>Loads the plugin settings from the config file.</summary>
		internal void LoadConfig()
		{
			string configFolder = OpenBveApi.Path.CombineDirectory(KatoInput.FileSystem.SettingsFolder, "1.5.0");
			string configFile = OpenBveApi.Path.CombineFile(configFolder, configFilename);
			if (File.Exists(configFile))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(KatoInputConfiguration));
				FileStream fs = new FileStream(configFile, FileMode.Open);
				Configuration = (KatoInputConfiguration)serializer.Deserialize(fs);
				fs.Close();
			}
		}

		/// <summary>Saves the plugin settings to the config file.</summary>
		internal void SaveConfig()
		{
			string configFolder = OpenBveApi.Path.CombineDirectory(KatoInput.FileSystem.SettingsFolder, "1.5.0");
			if (!Directory.Exists(configFolder))
			{
				Directory.CreateDirectory(configFolder);
			}
			string configFile = OpenBveApi.Path.CombineFile(configFolder, configFilename);
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(KatoInputConfiguration));
				FileStream fs = new FileStream(configFile, FileMode.Create);
				serializer.Serialize(fs, Configuration);
				fs.Close();
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + Environment.NewLine +
								"Please check you have write permission.");
			}
		}

		/// <summary>Adds the available controllers to the device dropdown list.</summary>
		private void ListControllers()
		{
			// Clear the internal and visible lists
			deviceBox.Items.Clear();
			controllers.Clear();
			EC1Controller.GetControllers(controllers);

			if (controllers.Count > 0)
			{
				selectedControllerGuid = controllers.Keys.First();
			}

			foreach (KeyValuePair<Guid, Controller> controller in controllers)
			{
				deviceBox.Items.Add(controller.Value.Name);
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

		/// <summary>Updates the interface to reflect the input from the controller.</summary>
		private void UpdateInterface()
		{
			ControllerState.BrakeNotches brakeNotch = controllers[selectedControllerGuid].State.BrakeNotch;
			ControllerState.PowerNotches powerNotch = controllers[selectedControllerGuid].State.PowerNotch;
			ControllerState.ReverserPositions reverserPosition = controllers[selectedControllerGuid].State.ReverserPosition;

			switch (brakeNotch)
			{
				case ControllerState.BrakeNotches.Released:
					label_brake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_brake" }).Replace("[notch]", Translations.QuickReferences.HandleBrakeNull);
					break;
				case ControllerState.BrakeNotches.Emergency:
					label_brake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_brake" }).Replace("[notch]", Translations.QuickReferences.HandleEmergency);
					break;
				default:
					label_brake.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_brake" }).Replace("[notch]", Translations.QuickReferences.HandleBrake + (int)brakeNotch);
					break;
			}

			switch (powerNotch)
			{
				case ControllerState.PowerNotches.N:
					label_power.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_power" }).Replace("[notch]", Translations.QuickReferences.HandlePowerNull);
					break;
				default:
					label_power.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_power" }).Replace("[notch]", Translations.QuickReferences.HandlePower + (int)powerNotch);
					break;
			}

			switch (reverserPosition)
			{
				case ControllerState.ReverserPositions.Forward:
					label_reverser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleForward);
					break;
				case ControllerState.ReverserPositions.Backward:
					label_reverser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleBackward);
					break;
				default:
					label_reverser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "kato", "label_reverser" }).Replace("[notch]", Translations.QuickReferences.HandleNeutral);
					break;
			}
		}

		/// <summary>Retranslates the configuration interface.</summary>
		private void UpdateTranslation()
		{
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","config_title"});
			deviceInputBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","input_section"});
			label_device.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","device"});
			handleMappingBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","handle_section"});
			convertnotchesCheck.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","option_convert"});
			minmaxCheck.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","option_keep_minmax"});
			holdbrakeCheck.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","option_holdbrake"});
			buttonSave.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","save_button"});
			buttonCancel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"kato","cancel_button"});
		}

		private void Config_Shown(object sender, EventArgs e)
		{
			// Add connected devices to device list
			ListControllers();

			// Try to select the selected device
			if (selectedControllerGuid != Guid.Empty)
			{
				deviceBox.SelectedIndex = controllers.Keys.ToList().IndexOf(selectedControllerGuid);
			}

			// Set checkboxes
			convertnotchesCheck.Checked = Configuration.ConvertNotches;
			minmaxCheck.Checked = Configuration.KeepMinMax;
			holdbrakeCheck.Checked = Configuration.MapHoldBrake;
			minmaxCheck.Enabled = Configuration.ConvertNotches;

			// Start timer
			inputTimer.Enabled = true;

			// Translate the interface to the current language
			UpdateTranslation();
		}

		private void Config_FormClosed(Object sender, FormClosedEventArgs e)
		{
			// Reload the previous config and close the config dialog
			LoadConfig();
			inputTimer.Enabled = false;
		}

		private void deviceBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			selectedControllerGuid = controllers.Keys.ToList()[deviceBox.SelectedIndex];
			controllers[selectedControllerGuid].Update();
		}

		private void convertnotchesCheck_CheckedChanged(object sender, EventArgs e)
		{
			Configuration.ConvertNotches = convertnotchesCheck.Checked;
			minmaxCheck.Enabled = Configuration.ConvertNotches;
		}

		private void minmaxCheck_CheckedChanged(object sender, EventArgs e)
		{
			Configuration.KeepMinMax = minmaxCheck.Checked;
		}

		private void holdbrakeCheck_CheckedChanged(object sender, EventArgs e)
		{
			Configuration.MapHoldBrake = holdbrakeCheck.Checked;
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			// Save the config and close the config dialog
			SaveConfig();
			Close();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			// Reload the previous config and close the config dialog
			LoadConfig();
			Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			controllers[selectedControllerGuid].Update();

			//grab a random control, as we need something from the UI thread to check for invoke
			//WinForms is a pain
			if (buttonCancel.InvokeRequired)
			{
				buttonCancel.Invoke((MethodInvoker) UpdateInterface);
			}
			else
			{
				UpdateInterface();	
			}
			
		}

	}
}
