using System;
using System.Windows.Forms;
using LibRender2.Overlays;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			string Folder = Program.FileSystem.GetDataFolder("Flags");
			if (Translations.SelectedLanguage(Folder, ref Interface.CurrentOptions.LanguageCode, comboboxLanguages, pictureboxLanguage)) {
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

		
		
		private void ListInputDevicePlugins()
		{
			ListViewItem[] Items = new ListViewItem[Program.InputDevicePlugin.AvailableInfos.Count];

			for (int i = 0; i < Items.Length; i++)
			{
				InputDevicePlugin.Info info = Program.InputDevicePlugin.AvailableInfos[i];

				if (Array.Exists(Interface.CurrentOptions.EnableInputDevicePlugins, element => element.Equals(info.FileName)))
				{
					Program.InputDevicePlugin.CallPluginLoad(info);
				}

				Items[i] = new ListViewItem(new [] { "", "", "", "", "" });
				UpdateInputDeviceListViewItem(Items[i], i, false);
			}

			listviewInputDevice.Items.AddRange(Items);
			listviewInputDevice.AutoResizeColumns(Items.Length != 0 ? ColumnHeaderAutoResizeStyle.ColumnContent : ColumnHeaderAutoResizeStyle.None);
		}

		private void UpdateInputDeviceListViewItem(ListViewItem Item, int Index, bool ResizeColumns)
		{
			InputDevicePlugin.Info info = Program.InputDevicePlugin.AvailableInfos[Index];
			Item.SubItems[0].Text = info.Name.Title;

			switch (info.Status)
			{
				case InputDevicePlugin.Status.Failure:
					Item.SubItems[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status_failure");
					break;
				case InputDevicePlugin.Status.Disable:
					Item.SubItems[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status_disable");
					break;
				case InputDevicePlugin.Status.Enable:
					Item.SubItems[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status_enable");
					break;
			}

			Item.SubItems[2].Text = info.Version.Version;
			Item.SubItems[3].Text = info.Provider.Copyright;
			Item.SubItems[4].Text = info.FileName;

			if (ResizeColumns)
			{
				listviewInputDevice.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		private void UpdateInputDeviceComponent(InputDevicePlugin.Status Status)
		{
			switch (Status)
			{
				case InputDevicePlugin.Status.Failure:
					checkBoxInputDeviceEnable.Enabled = false;
					checkBoxInputDeviceEnable.Checked = false;
					buttonInputDeviceConfig.Enabled = false;
					break;
				case InputDevicePlugin.Status.Disable:
					checkBoxInputDeviceEnable.Enabled = true;
					checkBoxInputDeviceEnable.Checked = false;
					buttonInputDeviceConfig.Enabled = false;
					break;
				case InputDevicePlugin.Status.Enable:
					checkBoxInputDeviceEnable.Enabled = true;
					checkBoxInputDeviceEnable.Checked = true;
					buttonInputDeviceConfig.Enabled = true;
					break;
			}
		}

		private void listviewInputDevice_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listviewInputDevice.SelectedIndices.Count == 1)
			{
				int index = listviewInputDevice.SelectedIndices[0];
				this.Tag = new object();
				UpdateInputDeviceComponent(Program.InputDevicePlugin.AvailableInfos[index].Status);
				// finalize
				this.Tag = null;
			}
			else
			{
				this.Tag = new object();
				checkBoxInputDeviceEnable.Enabled = false;
				checkBoxInputDeviceEnable.Checked = false;
				buttonInputDeviceConfig.Enabled = false;
				this.Tag = null;
			}
		}

		private void checkBoxInputDeviceEnable_CheckedChanged(object sender, EventArgs e)
		{
			if (this.Tag ==  null && listviewInputDevice.SelectedIndices.Count == 1)
			{
				int index = listviewInputDevice.SelectedIndices[0];

				if (checkBoxInputDeviceEnable.Checked)
				{
					Program.InputDevicePlugin.CallPluginLoad(index);
				}
				else
				{
					Program.InputDevicePlugin.CallPluginUnload(index);
				}

				UpdateInputDeviceComponent(Program.InputDevicePlugin.AvailableInfos[index].Status);
				UpdateInputDeviceListViewItem(listviewInputDevice.Items[index], index, true);
			}
		}

		// Input Device Plugin Config
		private void buttonInputDeviceConfig_Click(object sender, EventArgs e)
		{
			if (listviewInputDevice.SelectedIndices.Count == 1)
			{
				Program.InputDevicePlugin.CallPluginConfig(this, listviewInputDevice.SelectedIndices[0]);
			}
		}

		private void comboboxCursor_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.Tag != null) return;
			Cursors.SelectedCursor(comboboxCursor, pictureboxCursor);
		}
	}
}
