using System;
using System.Windows.Forms;
using LibRender2.Overlays;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;

namespace OpenBve {
	internal partial class formMain {
		
		
		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			string Folder = Program.FileSystem.GetDataFolder("Flags");
			string newImage;
			if (Translations.SelectedLanguage(Folder, ref Interface.CurrentOptions.LanguageCode, comboboxLanguages, out newImage))
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
			ListViewItem[] Items = new ListViewItem[InputDevicePlugin.AvailablePluginInfos.Count];
			for (int i = 0; i < Items.Length; i++) {
				InputDevicePlugin.PluginInfo Info = InputDevicePlugin.AvailablePluginInfos[i];
				if (Array.Exists(Interface.CurrentOptions.EnableInputDevicePlugins, element => element.Equals(Info.FileName))) {
					InputDevicePlugin.CallPluginLoad(i, Program.CurrentHost);
				}
				Items[i] = new ListViewItem(new[] { "", "", "", "", "" });
				UpdateInputDeviceListViewItem(Items[i], i, false);
			}
			listviewInputDevice.Items.AddRange(Items);
			if (Items.Length != 0)
			{
				listviewInputDevice.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
			else
			{
				listviewInputDevice.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
			}
		}

		private void UpdateInputDeviceListViewItem(ListViewItem Item, int Index, bool ResizeColumns) {
			Item.SubItems[0].Text = InputDevicePlugin.AvailablePluginInfos[Index].Name.Title;
			switch (InputDevicePlugin.AvailablePluginInfos[Index].Status)
			{
				case InputDevicePlugin.PluginInfo.PluginStatus.Failure:
					Item.SubItems[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status_failure");
					break;
				case InputDevicePlugin.PluginInfo.PluginStatus.Disable:
					Item.SubItems[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status_disable");
					break;
				case InputDevicePlugin.PluginInfo.PluginStatus.Enable:
					Item.SubItems[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status_enable");
					break;
			}
			Item.SubItems[2].Text = InputDevicePlugin.AvailablePluginInfos[Index].Version.Version;
			Item.SubItems[3].Text = InputDevicePlugin.AvailablePluginInfos[Index].Provider.Copyright;
			Item.SubItems[4].Text = InputDevicePlugin.AvailablePluginInfos[Index].FileName;
			if (ResizeColumns) {
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
			if (listviewInputDevice.SelectedIndices.Count == 1) {
				int index = listviewInputDevice.SelectedIndices[0];
				this.Tag = new object();
				UpdateInputDeviceComponent(InputDevicePlugin.AvailablePluginInfos[index].Status);
				// finalize
				this.Tag = null;
			} else {
				this.Tag = new object();
				checkBoxInputDeviceEnable.Enabled = false;
				checkBoxInputDeviceEnable.Checked = false;
				buttonInputDeviceConfig.Enabled = false;
				this.Tag = null;
			}
		}

		private void checkBoxInputDeviceEnable_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag ==  null && listviewInputDevice.SelectedIndices.Count == 1) {
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
		
		private void listviewInputDevice_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
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
			if (this.Tag != null) return;
			Cursors.SelectedCursor(comboboxCursor, pictureboxCursor);
		}
	}
}
