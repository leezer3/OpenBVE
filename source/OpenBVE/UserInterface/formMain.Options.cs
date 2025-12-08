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


		private void updownTimeAccelerationFactor_ValueChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.TimeAccelerationFactor = (int)updownTimeAccelerationFactor.Value;
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
	}
}
