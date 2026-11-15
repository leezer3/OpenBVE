using System;
using System.Windows.Forms;
using LibRender2.Overlays;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve {
	internal partial class formMain {
		
		
		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (Tag != null) return;
			string flagFolder = Program.FileSystem.GetDataFolder("Flags");
			if (Translations.SelectedLanguage(flagFolder, ref Interface.CurrentOptions.LanguageCode, comboboxLanguages, out string newImage))
			{
				pictureboxLanguage.Image = ImageExtensions.FromFile(newImage);
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
			Interface.CurrentOptions.TimeTableStyle = (TimeTableMode)comboBoxTimeTableDisplayMode.SelectedIndex;
		}

		private void UpdateFPSLimitEnabled()
		{
			bool vsyncEnabled = comboboxVSync.SelectedIndex == 1;
			comboBoxFPSLimit.Enabled = !vsyncEnabled;
			labelFPSLimit.ForeColor = vsyncEnabled ? System.Drawing.SystemColors.GrayText : System.Drawing.SystemColors.ControlText;
		}

		private void comboBoxVSync_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateFPSLimitEnabled();
		}


		private void updownTimeAccelerationFactor_ValueChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.TimeAccelerationFactor = (int)updownTimeAccelerationFactor.Value;
		}

		private void comboboxShadowResolution_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Enable/disable sub-controls when shadows are turned off/on
			bool shadowEnabled = comboboxShadowResolution.SelectedIndex != 0;
			comboboxShadowDistance.Enabled = shadowEnabled;
			comboboxShadowCascades.Enabled = shadowEnabled;
			trackbarShadowStrength.Enabled = shadowEnabled;
			labelShadowStrengthValue.Enabled = shadowEnabled;
			updownShadowBias.Enabled = shadowEnabled;
			updownShadowNormalBias.Enabled = shadowEnabled;
			checkboxShadowFilterCascades.Enabled = shadowEnabled;
			checkboxShadowSmooth.Enabled = shadowEnabled;
			bool softnessEnabled = shadowEnabled && checkboxShadowSmooth.Checked;
			labelShadowSoftness.Enabled = softnessEnabled;
			comboboxShadowFilterRadius.Enabled = softnessEnabled;

			if (!shadowEnabled)
			{
				// Visual hint: grey out the strength label
				labelShadowStrengthValue.Text = @"—";
			}
			else
			{
				labelShadowStrengthValue.Text = trackbarShadowStrength.Value + @"%";
			}
		}

		private void trackbarShadowStrength_Scroll(object sender, EventArgs e)
		{
			labelShadowStrengthValue.Text = trackbarShadowStrength.Value + @"%";
		}

		private void updownShadowBias_ValueChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.ShadowBias = (double)updownShadowBias.Value;
		}

		private void updownShadowNormalBias_ValueChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.ShadowNormalBias = (double)updownShadowNormalBias.Value;
		}

		private void comboboxShadowFilterRadius_SelectedIndexChanged(object sender, EventArgs e)
		{
			double[] presets = { 1.0, 1.5, 2.5 };
			int idx = comboboxShadowFilterRadius.SelectedIndex;
			if (idx >= 0 && idx < presets.Length) Interface.CurrentOptions.ShadowFilterRadius = presets[idx];
		}

		private void checkboxShadowSmooth_CheckedChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.ShadowSmooth = checkboxShadowSmooth.Checked;
			bool shadowEnabled = comboboxShadowResolution.SelectedIndex != 0;
			bool softnessEnabled = shadowEnabled && checkboxShadowSmooth.Checked;
			labelShadowSoftness.Enabled = softnessEnabled;
			comboboxShadowFilterRadius.Enabled = softnessEnabled;
		}

		
		// =======
		// options
		// =======

		// joysticks enabled
		private void checkboxJoysticksUsed_CheckedChanged(object sender, EventArgs e) {
			groupboxJoysticks.Enabled = checkboxJoysticksUsed.Checked;
		}

		
		
		private void ListInputDevicePlugins() {
			ListViewItem[] listItems = new ListViewItem[InputDevicePlugin.AvailablePluginInfos.Count];
			for (int i = 0; i < listItems.Length; i++) {
				if (Array.Exists(Interface.CurrentOptions.EnabledInputDevicePlugins, element => element.Equals(InputDevicePlugin.AvailablePluginInfos[i].FileName))) {
					InputDevicePlugin.CallPluginLoad(i, Program.CurrentHost);
				}
				listItems[i] = new ListViewItem(new[] { "", "", "", "", "" });
				UpdateInputDeviceListViewItem(listItems[i], i, false);
			}
			listviewInputDevice.Items.AddRange(listItems);
			listviewInputDevice.AutoResizeColumns(listItems.Length != 0 ? ColumnHeaderAutoResizeStyle.ColumnContent : ColumnHeaderAutoResizeStyle.None);
		}

		private void UpdateInputDeviceListViewItem(ListViewItem listItem, int pluginIndex, bool resizeColumns) {
			listItem.SubItems[0].Text = InputDevicePlugin.AvailablePluginInfos[pluginIndex].Name.Title;
			switch (InputDevicePlugin.AvailablePluginInfos[pluginIndex].Status)
			{
				case InputDevicePlugin.PluginInfo.PluginStatus.Failure:
					listItem.SubItems[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_status_failure"});
					break;
				case InputDevicePlugin.PluginInfo.PluginStatus.Disable:
					listItem.SubItems[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_status_disable"});
					break;
				case InputDevicePlugin.PluginInfo.PluginStatus.Enable:
					listItem.SubItems[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_status_enable"});
					break;
			}
			listItem.SubItems[2].Text = InputDevicePlugin.AvailablePluginInfos[pluginIndex].Version.Version;
			listItem.SubItems[3].Text = InputDevicePlugin.AvailablePluginInfos[pluginIndex].Provider.Copyright;
			listItem.SubItems[4].Text = InputDevicePlugin.AvailablePluginInfos[pluginIndex].FileName;
			if (resizeColumns) {
				listviewInputDevice.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		private void UpdateInputDeviceComponent(InputDevicePlugin.PluginInfo.PluginStatus Status) {
			switch (Status)
			{
				case InputDevicePlugin.PluginInfo.PluginStatus.Failure:
					checkBoxInputDeviceEnable.Enabled = false;
					checkBoxInputDeviceEnable.Checked = false;
					buttonInputDeviceConfig.Enabled = false;
					break;
				case InputDevicePlugin.PluginInfo.PluginStatus.Disable:
					checkBoxInputDeviceEnable.Enabled = true;
					checkBoxInputDeviceEnable.Checked = false;
					buttonInputDeviceConfig.Enabled = false;
					break;
				case InputDevicePlugin.PluginInfo.PluginStatus.Enable:
					checkBoxInputDeviceEnable.Enabled = true;
					checkBoxInputDeviceEnable.Checked = true;
					buttonInputDeviceConfig.Enabled = true;
					break;
			}
		}

		private void listviewInputDevice_SelectedIndexChanged(object sender, EventArgs e) {
			Tag = new object();
			if (listviewInputDevice.SelectedIndices.Count == 1) {
				int index = listviewInputDevice.SelectedIndices[0];
				UpdateInputDeviceComponent(InputDevicePlugin.AvailablePluginInfos[index].Status);
			} else {
				checkBoxInputDeviceEnable.Enabled = false;
				checkBoxInputDeviceEnable.Checked = false;
				buttonInputDeviceConfig.Enabled = false;
			}
			// finalize
			Tag = null;
		}

		private void checkBoxInputDeviceEnable_CheckedChanged(object sender, EventArgs e) {
			if (Tag == null && listviewInputDevice.SelectedIndices.Count == 1) {
				int index = listviewInputDevice.SelectedIndices[0];
				if (checkBoxInputDeviceEnable.Checked) {
					InputDevicePlugin.CallPluginLoad(index, Program.CurrentHost);
				} else {
					InputDevicePlugin.CallPluginUnload(index);
				}
				UpdateInputDeviceComponent(InputDevicePlugin.AvailablePluginInfos[index].Status);
				UpdateInputDeviceListViewItem(listviewInputDevice.Items[index], index, true);
			}
		}
		
		private void listviewInputDevice_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (listviewInputDevice.SelectedIndices.Count == 1)
			{
				checkBoxInputDeviceEnable.Checked = !checkBoxInputDeviceEnable.Checked;
			}
		}

		// Input Device Plugin Config
		private void buttonInputDeviceConfig_Click(object sender, EventArgs e) {
			if (listviewInputDevice.SelectedIndices.Count == 1) {
				InputDevicePlugin.CallPluginConfig(this, listviewInputDevice.SelectedIndices[0]);
			}
		}

		private void comboboxCursor_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Tag != null) return;
			Cursors.SelectedCursor(comboboxCursor, pictureboxCursor);
		}

		// =========
		// upscaler
		// =========

                /// <summary>The quality presets of the currently selected upscaler mode</summary>
                private UpscalerQuality[] upscalerQualityList = new UpscalerQuality[] { };

                /// <summary>The localized label of an upscaler quality preset (falls back to English)</summary>
                private string GetUpscalerQualityLabel(UpscalerQuality quality) {
                        string key = "upscaler_quality_" + quality.ToString().ToLowerInvariant();
                        string label = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", key });
                        if (label == key) {
                                label = UpscalerPresets.GetDefaultLabel(quality);
                        }
                        return label;
                }

                /// <summary>The stored sharpness value of the given upscaler mode</summary>
                private double GetUpscalerSharpness(UpscalerMode mode) {
                        switch (mode) {
                                case UpscalerMode.Nis:
                                        return Interface.CurrentOptions.UpscalerSharpnessNis;
                                case UpscalerMode.Sgsr1:
                                        return Interface.CurrentOptions.UpscalerSharpnessSgsr;
                                default:
                                        return Interface.CurrentOptions.UpscalerSharpnessFsr;
                        }
                }

		private void comboboxUpscaler_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxUpscaler.SelectedIndex < 0) return;
			Interface.CurrentOptions.UpscalerMode = (UpscalerMode)comboboxUpscaler.SelectedIndex;
			UpscalerMode mode = Interface.CurrentOptions.UpscalerMode;
			bool upscalerEnabled = mode != UpscalerMode.Off;
			comboboxUpscalerQuality.Enabled = upscalerEnabled;
			trackbarUpscalerSharpness.Enabled = upscalerEnabled;
			labelUpscalerSharpnessValue.Enabled = upscalerEnabled;
			// the RCAS denoiser is an FSR 1.0 specific option
			checkboxUpscalerDenoise.Enabled = mode == UpscalerMode.Fsr1;
			// snap the stored preset to the vendor-recommended list of the new mode
			Interface.CurrentOptions.UpscalerQuality = UpscalerPresets.SnapToMode(mode, Interface.CurrentOptions.UpscalerQuality);
			// rebuild the quality list with the vendor-recommended presets
			upscalerQualityList = UpscalerPresets.GetQualities(mode);
			comboboxUpscalerQuality.Items.Clear();
			foreach (UpscalerQuality quality in upscalerQualityList) {
				comboboxUpscalerQuality.Items.Add(GetUpscalerQualityLabel(quality));
			}
			int qualityIndex = Array.IndexOf(upscalerQualityList, Interface.CurrentOptions.UpscalerQuality);
			comboboxUpscalerQuality.SelectedIndex = qualityIndex;
			// switch the sharpness slider to the value stored for the selected upscaler
			double sharpness = GetUpscalerSharpness(mode);
			trackbarUpscalerSharpness.Value = Math.Max(trackbarUpscalerSharpness.Minimum, Math.Min(trackbarUpscalerSharpness.Maximum, (int)Math.Round(sharpness * 100.0)));
			labelUpscalerSharpnessValue.Text = trackbarUpscalerSharpness.Value + @"%";
		}

		private void comboboxUpscalerQuality_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxUpscalerQuality.SelectedIndex < 0) return;
			if (comboboxUpscalerQuality.SelectedIndex < upscalerQualityList.Length) {
				Interface.CurrentOptions.UpscalerQuality = upscalerQualityList[comboboxUpscalerQuality.SelectedIndex];
			}
		}

		private void trackbarUpscalerSharpness_Scroll(object sender, EventArgs e) {
			labelUpscalerSharpnessValue.Text = trackbarUpscalerSharpness.Value + @"%";
			double sharpness = trackbarUpscalerSharpness.Value / 100.0;
			if (Interface.CurrentOptions.UpscalerMode == UpscalerMode.Nis) {
				Interface.CurrentOptions.UpscalerSharpnessNis = sharpness;
			} else if (Interface.CurrentOptions.UpscalerMode == UpscalerMode.Sgsr1) {
				Interface.CurrentOptions.UpscalerSharpnessSgsr = sharpness;
			} else {
				Interface.CurrentOptions.UpscalerSharpnessFsr = sharpness;
			}
		}

		private void checkboxUpscalerDenoise_CheckedChanged(object sender, EventArgs e) {
			Interface.CurrentOptions.UpscalerDenoise = checkboxUpscalerDenoise.Checked;
		}
	}
}
