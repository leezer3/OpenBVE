using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenBve.Input;
using OpenBve.UserInterface;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenTK.Input;
using Key = OpenBveApi.Input.Key;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace OpenBve {
	internal partial class formMain
	{

		private bool blockComboBoxIndexEvent = false;
		
		// ========
		// controls
		// ========

		// controls
		private void listviewControls_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				{
					Tag = new object();
					// command
					if (Translations.CommandInfos.ContainsKey(Interface.CurrentControls[i].Command))
					{
						comboboxCommand.SelectedValue = Interface.CurrentControls[i].Command;
						updownCommandOption.Value = Interface.CurrentControls[i].Option;
						labelCommandOption.Enabled = Translations.CommandInfos[Interface.CurrentControls[i].Command].EnableOption;
						updownCommandOption.Enabled = Translations.CommandInfos[Interface.CurrentControls[i].Command].EnableOption;
					}
					else
					{
						comboboxCommand.SelectedIndex = -1;
						updownCommandOption.Value = 0;
						labelCommandOption.Enabled = false;
						updownCommandOption.Enabled = false;
					}
					// data
					switch (Interface.CurrentControls[i].Method)
					{
						case ControlMethod.Keyboard:
							radiobuttonKeyboard.Checked = true;
							break;
						case ControlMethod.Joystick:
						case ControlMethod.RailDriver:
							radiobuttonJoystick.Checked = true;
							break;
						default:
							radiobuttonKeyboard.Checked = false;
							radiobuttonJoystick.Checked = false;
							textboxJoystickGrab.Enabled = false;
							break;
					}
					panelKeyboard.Enabled = radiobuttonKeyboard.Checked;
					if (radiobuttonKeyboard.Checked)
					{
						if (Translations.TranslatedKeys.ContainsKey(Interface.CurrentControls[i].Key))
						{
							comboboxKeyboardKey.SelectedValue = Interface.CurrentControls[i].Key;
						}
						checkboxKeyboardShift.Checked = (Interface.CurrentControls[i].Modifier & KeyboardModifier.Shift) != 0;
						checkboxKeyboardCtrl.Checked = (Interface.CurrentControls[i].Modifier & KeyboardModifier.Ctrl) != 0;
						checkboxKeyboardAlt.Checked = (Interface.CurrentControls[i].Modifier & KeyboardModifier.Alt) != 0;
					} else if (radiobuttonJoystick.Checked) {
						labelJoystickAssignmentValue.Text = GetControlDetails(i);
					} else {
						comboboxKeyboardKey.SelectedIndex = -1;
						checkboxKeyboardShift.Checked = false;
						checkboxKeyboardCtrl.Checked = false;
						checkboxKeyboardAlt.Checked = false;
					}
					panelJoystick.Enabled = radiobuttonJoystick.Checked;
					// finalize
					Tag = null;
				}
				buttonControlRemove.Enabled = true;
				buttonControlUp.Enabled = i > 0;
				buttonControlDown.Enabled = i < Interface.CurrentControls.Length - 1;
				groupboxControl.Enabled = true;
			} else {
				Tag = new object();
				comboboxCommand.SelectedIndex = -1;
				radiobuttonKeyboard.Checked = false;
				radiobuttonJoystick.Checked = false;
				groupboxControl.Enabled = false;
				comboboxKeyboardKey.SelectedIndex = -1;
				checkboxKeyboardShift.Checked = false;
				checkboxKeyboardCtrl.Checked = false;
				checkboxKeyboardAlt.Checked = false;
				labelJoystickAssignmentValue.Text = "";
				Tag = null;
				buttonControlRemove.Enabled = false;
				buttonControlUp.Enabled = false;
				buttonControlDown.Enabled = false;
			}
		}
		private void UpdateControlListElement(ListViewItem Item, int Index, bool ResizeColumns)
		{
			Translations.CommandInfo Info = Translations.CommandInfos.TryGetInfo(Interface.CurrentControls[Index].Command);
			Item.SubItems[0].Text = Info.Name;
			switch (Info.Type) {
					case Translations.CommandType.Digital: Item.SubItems[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_type_digital"}); break;
					case Translations.CommandType.AnalogHalf: Item.SubItems[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_type_analoghalf"}); break;
					case Translations.CommandType.AnalogFull: Item.SubItems[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_type_analogfull"}); break;
					default: Item.SubItems[1].Text = Info.Type.ToString(); break;
			}
			Item.SubItems[2].Text = Info.Description;
			switch (Interface.CurrentControls[Index].Method)
			{
				case ControlMethod.Keyboard:
					Item.ImageKey = @"keyboard";
					break;
				case ControlMethod.Joystick:
					Item.ImageKey = Info.Type == Translations.CommandType.AnalogHalf || Info.Type == Translations.CommandType.AnalogFull ? @"joystick" : @"gamepad";
					break;
				default:
					Item.ImageKey = null;
					break;
			}
			Item.SubItems[3].Text = GetControlDetails(Index);
			Item.SubItems[4].Text = Interface.CurrentControls[Index].Option.ToString(System.Globalization.CultureInfo.InvariantCulture);
			if (ResizeColumns) {
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		// get control details
		private string GetControlDetails(int Index) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Separator = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_separator"});
			if (Interface.CurrentControls[Index].Method == ControlMethod.Keyboard) {
				string t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_keyboard"}) + Separator;
				if ((Interface.CurrentControls[Index].Modifier & KeyboardModifier.Shift) != 0) t += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_keyboard_shift"});
				if ((Interface.CurrentControls[Index].Modifier & KeyboardModifier.Ctrl) != 0) t += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_keyboard_ctrl"});
				if ((Interface.CurrentControls[Index].Modifier & KeyboardModifier.Alt) != 0) t += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_keyboard_alt"});

				if (Interface.CurrentControls[Index].Key != Key.Unknown)
				{
					if (Translations.TranslatedKeys.ContainsKey(Interface.CurrentControls[Index].Key))
					{
						t += Translations.TranslatedKeys[Interface.CurrentControls[Index].Key].Description;
						return t;
					}
					t += Interface.CurrentControls[Index].Key;
					return t;
				}
				t += "{" + Interface.CurrentControls[Index].Element.ToString(Culture) + "}";
				return t;
			} 
			
			if (Interface.CurrentControls[Index].Method == ControlMethod.Joystick) {

				string t = string.Empty;
				if (Program.Joysticks.AttachedJoysticks.ContainsKey(Interface.CurrentControls[Index].Device))
				{
					t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick"}).Replace("[index]", Program.Joysticks.AttachedJoysticks[Interface.CurrentControls[Index].Device].Handle + 1.ToString(Culture));
				}
					
				switch (Interface.CurrentControls[Index].Component) {
					case JoystickComponent.Axis:
						t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis"}).Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						switch (Interface.CurrentControls[Index].Direction)
						{
							case -1:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis_negative"});
								break;
							case 1:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis_positive"});
								break;
							default:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis_invalid"});
								break;
						} break;
					case JoystickComponent.Button:
						t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_button"}).Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						break;
					case JoystickComponent.Hat:
						t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat"}).Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						switch (Interface.CurrentControls[Index].Direction)
						{
							case (int)HatPosition.Left:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_left"});
								break;
							case (int)HatPosition.UpLeft:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_upleft"});
								break;
							case (int)HatPosition.Up:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_up"});
								break;
							case (int)HatPosition.UpRight:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_upright"});
								break;
							case (int)HatPosition.Right:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_right"});
								break;
							case (int)HatPosition.DownRight:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_downright"});
								break;
							case (int)HatPosition.Down:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_down"});
								break;
							case (int)HatPosition.DownLeft:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_downleft"});
								break;
							default:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_hat_invalid"});
								break;
						} break;
				}
				return t;
			} 
			if (Interface.CurrentControls[Index].Method == ControlMethod.RailDriver) {

				string t = "RailDriver";
				switch (Interface.CurrentControls[Index].Component) {
					case JoystickComponent.Axis:
						switch (Interface.CurrentControls[Index].Element)
						{
							case 0:
								t += Separator + "Reverser";
								break;
							case 1:
								t += Separator + "Combined Power / Brake";
								break;
							case 2:
								t += Separator + "Auto-Brake";
								break;
							case 3:
								t += Separator + "Independent Brake";
								break;
							case 4:
								t += Separator + "Bail-Off";
								break;
							case 5:
								t += Separator + "Wiper Switch";
								break;
							case 6:
								t += Separator + "Light Switch";
								break;
						}
						
						switch (Interface.CurrentControls[Index].Direction)
						{
							case -1:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis_negative"});
								break;
							case 1:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis_positive"});
								break;
							default:
								t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_axis_invalid"});
								break;
						} break;
					case JoystickComponent.Button:
						t += Separator + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_joystick_button"}).Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						break;
				}
				return t;
			} 
			return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","assignment_invalid"});
			
		}

		// control add
		private void buttonControlAdd_Click(object sender, EventArgs e) {
			for (int i = 0; i < Interface.CurrentControls.Length; i++) {
				listviewControls.Items[i].Selected = false;
			}
			int n = Interface.CurrentControls.Length;
			Array.Resize(ref Interface.CurrentControls, n + 1);
			Interface.CurrentControls[n].Command = Translations.Command.None;
			ListViewItem Item = new ListViewItem(new[] { "", "", "", "", "" });
			UpdateControlListElement(Item, n, true);
			listviewControls.Items.Add(Item);
			Item.Selected = true;
		}

		// control remove
		private void buttonControlRemove_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				for (int i = j; i < Interface.CurrentControls.Length - 1; i++) {
					Interface.CurrentControls[i] = Interface.CurrentControls[i + 1];
				}
				Array.Resize(ref Interface.CurrentControls, Interface.CurrentControls.Length - 1);
				listviewControls.Items[j].Remove();
			}
		}

		// control up
		private void buttonControlUp_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (j > 0) {
					(Interface.CurrentControls[j], Interface.CurrentControls[j - 1]) = (Interface.CurrentControls[j - 1], Interface.CurrentControls[j]);
					ListViewItem v = listviewControls.Items[j];
					listviewControls.Items.RemoveAt(j);
					listviewControls.Items.Insert(j - 1, v);
				}
			}
		}

		// control down
		private void buttonControlDown_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (j < Interface.CurrentControls.Length - 1) {
					(Interface.CurrentControls[j], Interface.CurrentControls[j + 1]) = (Interface.CurrentControls[j + 1], Interface.CurrentControls[j]);
					ListViewItem v = listviewControls.Items[j];
					listviewControls.Items.RemoveAt(j);
					listviewControls.Items.Insert(j + 1, v);
				}
			}
		}

		// command
		private void comboboxCommand_SelectedIndexChanged(object sender, EventArgs e) {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				int j = comboboxCommand.SelectedIndex;
				if (j >= 0)
				{
					Translations.Command selectedCommand = (Translations.Command)comboboxCommand.SelectedValue;
					Interface.CurrentControls[i].Command = selectedCommand;
					Translations.CommandInfo Info = Translations.CommandInfos.TryGetInfo(selectedCommand);
					Interface.CurrentControls[i].InheritedType = Info.Type;
					labelCommandOption.Enabled = Translations.CommandInfos[selectedCommand].EnableOption;
					updownCommandOption.Enabled = Translations.CommandInfos[selectedCommand].EnableOption;
					UpdateControlListElement(listviewControls.Items[i], i, true);
				}
			}
		}

		// command option
		private void updownCommandOption_ValueChanged(object sender, EventArgs e) {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				int j = comboboxCommand.SelectedIndex;
				if (j >= 0) {
					Interface.CurrentControls[i].Option = (int)updownCommandOption.Value;
					UpdateControlListElement(listviewControls.Items[i], i, true);
				}
			}
		}

		// ========
		// keyboard
		// ========

		// keyboard
		private void radiobuttonKeyboard_CheckedChanged(object sender, EventArgs e) {
			textboxJoystickGrab.Enabled = radiobuttonJoystick.Checked || radiobuttonKeyboard.Checked;
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Method = ControlMethod.Keyboard;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
			panelKeyboard.Enabled = radiobuttonKeyboard.Checked;
		}

		// key
		private void comboboxKeyboardKey_SelectedIndexChanged(object sender, EventArgs e) {
			if (blockComboBoxIndexEvent)
			{
				// we've set the key index programatically, don't trigger the event twice
				blockComboBoxIndexEvent = false;
				return;
			}
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Key selectedKey = (Key)comboboxKeyboardKey.SelectedValue;
				Interface.CurrentControls[i].Key = selectedKey;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}

		// modifiers
		private void checkboxKeyboardShift_CheckedChanged(object sender, EventArgs e) {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? KeyboardModifier.Shift : KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? KeyboardModifier.Ctrl : KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? KeyboardModifier.Alt : KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}
		private void checkboxKeyboardCtrl_CheckedChanged(object sender, EventArgs e) {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? KeyboardModifier.Shift : KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? KeyboardModifier.Ctrl : KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? KeyboardModifier.Alt : KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}
		private void checkboxKeyboardAlt_CheckedChanged(object sender, EventArgs e) {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? KeyboardModifier.Shift : KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? KeyboardModifier.Ctrl : KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? KeyboardModifier.Alt : KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}

		
		
		// ========
		// joystick
		// ========

		// joystick
		private void radiobuttonJoystick_CheckedChanged(object sender, EventArgs e) {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Method = ControlMethod.Joystick;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
			panelJoystick.Enabled = radiobuttonJoystick.Checked;
			if (radiobuttonJoystick.Checked || radiobuttonKeyboard.Checked)
			{
				textboxJoystickGrab.Enabled = true;
			}
			else
			{
				textboxJoystickGrab.Enabled = false;
			}

			textboxJoystickGrab.Text = Translations.GetInterfaceString(HostApplication.OpenBve, radiobuttonJoystick.Checked ? new[] {"controls","selection_joystick_assignment_grab"} : new[] {"controls","selection_keyboard_assignment_grab"});
		}

		// details
		private void UpdateJoystickDetails() {
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				labelJoystickAssignmentValue.Text = GetControlDetails(j);

			}
		}

		// import
		private void buttonControlsImport_Click(object sender, EventArgs e) {
			OpenFileDialog Dialog = new OpenFileDialog
			{
				CheckFileExists = true,
				Filter =
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"dialog","controlsfiles"}) + @"|*.controls|" +
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"dialog","allfiles"}) + @"|*"
			};
			//Dialog.InitialDirectory = Interface.GetControlsFolder();
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.LoadControls(Dialog.FileName, out Interface.CurrentControls);
					for (int i = 0; i < listviewControls.SelectedItems.Count; i++) {
						listviewControls.SelectedItems[i].Selected = false;
					}
					listviewControls.Items.Clear();
					ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						Items[i] = new ListViewItem(new[] { "", "", "", "", "" });
						UpdateControlListElement(Items[i], i, false);
					}
					listviewControls.Items.AddRange(Items);
					listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		private void buttonControlReset_Click(object sender, EventArgs e)
		{
			try
			{
				Interface.LoadControls(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default.controls"), out Interface.CurrentControls);
				for (int i = 0; i < listviewControls.SelectedItems.Count; i++)
				{
					listviewControls.SelectedItems[i].Selected = false;
				}
				listviewControls.Items.Clear();
				ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
				for (int i = 0; i < Interface.CurrentControls.Length; i++)
				{
					Items[i] = new ListViewItem(new[] { "", "", "", "", "" });
					UpdateControlListElement(Items[i], i, false);
				}
				listviewControls.Items.AddRange(Items);
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			
		}

		// export
		private void buttonControlsExport_Click(object sender, EventArgs e) {
			SaveFileDialog Dialog = new SaveFileDialog
			{
				OverwritePrompt = true,
				Filter =
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"dialog","controlsfiles"}) + @"|*.controls|" +
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"dialog","allfiles"}) + @"|*"
			};
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.SaveControls(Dialog.FileName, Interface.CurrentControls);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		private bool KeyGrab = false;

		// joystick grab
		private void textboxJoystickGrab_Enter(object sender, EventArgs e) {
			if (!radiobuttonJoystick.Checked)
			{
				textboxJoystickGrab.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_assignment_grabbing"});
				textboxJoystickGrab.BackColor = Color.LightSkyBlue;
				textboxJoystickGrab.ForeColor = Color.Black;
				KeyGrab = true;
				return;

			}
			bool FullAxis = false;
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (Interface.CurrentControls[j].InheritedType == Translations.CommandType.AnalogFull) {
					FullAxis = true;
				}
			}

			textboxJoystickGrab.Text = Translations.GetInterfaceString(HostApplication.OpenBve, FullAxis ? new[] {"controls","selection_joystick_assignment_grab_fullaxis"} : new[] {"controls","selection_joystick_assignment_grab_normal"});
			textboxJoystickGrab.BackColor = Color.LightSkyBlue;
			textboxJoystickGrab.ForeColor = Color.Black;
		}

		private void textboxJoystickGrab_KeyDown(object sender, KeyEventArgs e)
		{
			//Required to avoid race condition with openTK recieving the same event internally
			blockComboBoxIndexEvent = true;
			System.Threading.Thread.Sleep(1);
			var kbState = Keyboard.GetState();
			if (KeyGrab == false)
			{
				return;
			}
			for (int j = 0; j < Translations.TranslatedKeys.Count; j++)
			{
				Key k = Translations.TranslatedKeys.ElementAt(j).Key;
				if (kbState.IsKeyDown((OpenTK.Input.Key)k))
				{
					int i = listviewControls.SelectedIndices[0];
					Interface.CurrentControls[i].Key = k;
					UpdateControlListElement(listviewControls.Items[i], i, true);
					comboboxKeyboardKey.SelectedIndex = j;
				}
			}
			textboxJoystickGrab.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_assignment_grab"});
			textboxJoystickGrab.BackColor = Color.White;
			textboxJoystickGrab.ForeColor = Color.Black;
			comboboxKeyboardKey.Focus();
		}
		private void textboxJoystickGrab_Leave(object sender, EventArgs e) {
			textboxJoystickGrab.Text = Translations.GetInterfaceString(HostApplication.OpenBve, radiobuttonJoystick.Checked ? new[] {"controls","selection_joystick_assignment_grab"} : new[] {"controls","selection_keyboard_assignment_grab"});
			textboxJoystickGrab.BackColor = Color.White;
			textboxJoystickGrab.ForeColor = Color.Black;
			KeyGrab = false;
		}

		/// <summary>Array containing the configure link locations within the painted picturebox</summary>
		private Vector2[][] configureLinkLocations;

		// attached joysticks
		private void pictureboxJoysticks_Paint(object sender, PaintEventArgs e)
		{
			DoubleBuffered = true;
			int device = -1;
			JoystickComponent component = JoystickComponent.Invalid;
			int element = -1;
			int direction = -1;
			Translations.CommandType type = Translations.CommandType.Digital;
			if (Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (Interface.CurrentControls[j].Method == ControlMethod.Joystick && Program.Joysticks.AttachedJoysticks.ContainsKey(Interface.CurrentControls[j].Device)) {
					device = Program.Joysticks.AttachedJoysticks[Interface.CurrentControls[j].Device].Handle;
					component = Interface.CurrentControls[j].Component;
					element = Interface.CurrentControls[j].Element;
					direction = Interface.CurrentControls[j].Direction;
					type = Interface.CurrentControls[j].InheritedType;
				}
			}
			
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			Font f = new Font(Font.Name, 0.875f * Font.Size);
			float x = 2.0f, y = 2.0f;
			float threshold = (trackbarJoystickAxisThreshold.Value - (float)trackbarJoystickAxisThreshold.Minimum) / (trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			configureLinkLocations = new Vector2[Program.Joysticks.AttachedJoysticks.Count][];
			for (int i = 0; i < Program.Joysticks.AttachedJoysticks.Count; i++)
			{
				Guid guid = Program.Joysticks.AttachedJoysticks.ElementAt(i).Key;
				Program.Joysticks.AttachedJoysticks[guid].Poll();
				float w, h;
				Image image = JoystickImage;
				if (Program.Joysticks.AttachedJoysticks[guid] is AbstractRailDriver && RailDriverImage != null)
				{
					image = RailDriverImage;
				}
				else if (Program.Joysticks.AttachedJoysticks[guid].Name.IndexOf("gamepad", StringComparison.InvariantCultureIgnoreCase) != -1 && GamepadImage != null)
				{
					image = GamepadImage;
				}
				else if (Program.Joysticks.AttachedJoysticks[guid].Name.IndexOf("xinput", StringComparison.InvariantCultureIgnoreCase) != -1 && XboxImage != null)
				{
					image = XboxImage;
				}
				else if (Program.Joysticks.AttachedJoysticks[guid].Name.IndexOf("mascon", StringComparison.InvariantCultureIgnoreCase) != -1 && ZukiImage != null)
				{
					image = ZukiImage;
				}
				if (image != null) {
					e.Graphics.DrawImage(image, x, y);
					w = image.Width;
					h = image.Height;
					if (h < 64.0f) h = 64.0f;
				} else {
					w = 64.0f; h = 64.0f;
					e.Graphics.DrawRectangle(new Pen(labelControlsTitle.BackColor), x, y, w, h);
				}

				// joystick number
				e.Graphics.FillEllipse(Brushes.Gold, x + w - 16.0f, y, 16.0f, 16.0f);
				e.Graphics.DrawEllipse(Pens.Black, x + w - 16.0f, y, 16.0f, 16.0f);
				string joystickNumber = (i + 1).ToString(Culture);
				SizeF numberSize = e.Graphics.MeasureString(joystickNumber, f);
				e.Graphics.DrawString(joystickNumber, f, Brushes.Black, x + w - 8.0f - 0.5f * numberSize.Width, y + 8.0f - 0.5f * numberSize.Height);
				// joystick name
				e.Graphics.DrawString(Program.Joysticks.AttachedJoysticks[guid].Name, Font, Brushes.Black, x + w + 8.0f, y);
				// Configure Link
				if(Program.Joysticks.AttachedJoysticks[guid].ConfigurationLink != ConfigurationLink.None)
				{
					Font underlinedFont = new Font(Font, FontStyle.Underline);
					SizeF joystickSize = e.Graphics.MeasureString(Program.Joysticks.AttachedJoysticks[guid].Name, Font);
					SizeF configureSize = e.Graphics.MeasureString("Configure", underlinedFont);
					e.Graphics.DrawString("Configure", underlinedFont, Brushes.Blue, x + w + 8.0f + joystickSize.Width - configureSize.Width, y + numberSize.Height);
					configureLinkLocations[i] = new[] {new Vector2(x + w + 8.0f + joystickSize.Width - configureSize.Width, y + numberSize.Height), new Vector2(x + w + 8.0f + joystickSize.Width + configureSize.Width, y + numberSize.Height + configureSize.Height)};
				}
				
				if (OpenTK.Configuration.RunningOnSdl2)
				{
					//HACK: Control configuration doesn't work in-form on SDL2
					string error = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_ingame"});
					error = error.Replace("[platform]", "SDL2");
					e.Graphics.DrawString(error, Font, Brushes.Black, x + w + 8.0f, y + 30.0f);
					return;
				}
				float m;
				if (groupboxJoysticks.Enabled) {
					m = x;
					Pen p = new Pen(Color.DarkGoldenrod, 2.0f);
					Pen ps = new Pen(Color.Firebrick, 2.0f);
					// first row
					float u = x + w + 8.0f;
					float v = y + 24.0f;
					float g = h - 24.0f;
					// hats
					for (int j = 0; j < Program.Joysticks.AttachedJoysticks[guid].HatCount(); j++)
					{
						if (device == i & component == JoystickComponent.Hat & element == j)
						{
							e.Graphics.DrawEllipse(ps, u, v, g, g);
						}
						else
						{
							e.Graphics.DrawEllipse(p, u, v, g, g);
						}
						string t = "H" + (j + 1).ToString(Culture);
						SizeF s = e.Graphics.MeasureString(t, f);
						e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (g - s.Width), v + 0.5f * (g - s.Height));
						JoystickHatState aa = Program.Joysticks.AttachedJoysticks[guid].GetHat(j);
						if (aa.Position != HatPosition.Centered)
						{
							double rx = 0.0;
							double ry = 0.0;
							switch (aa.Position)
							{
								case HatPosition.Up:
									rx = 0.0;
									ry = -1.0;
									break;
								case HatPosition.Down:
									rx = 0.0;
									ry = 1.0;
									break;
								case HatPosition.Left:
									rx = -1.0;
									ry = 0.0;
									break;
								case HatPosition.Right:
									rx = 1.0;
									ry = 0.0;
									break;
								case HatPosition.UpLeft:
									rx = -1.0;
									ry = -1.0;
									break;
								case HatPosition.UpRight:
									rx = 1.0;
									ry = -1.0;
									break;
								case HatPosition.DownLeft:
									rx = -1.0;
									ry = 1.0;
									break;
								case HatPosition.DownRight:
									rx = 1.0;
									ry = 1.0;
									break;
							}

							double rt = rx * rx + ry * ry;
							rt = 1.0 / Math.Sqrt(rt);
							rx *= rt; ry *= rt;
							float dx = (float)(0.5 * rx * (g - 8.0));
							float dy = (float)(0.5 * ry * (g - 8.0));
							e.Graphics.FillEllipse(Brushes.White, u + 0.5f * g + dx - 4.0f, v + 0.5f * g + dy - 4.0f, 8.0f, 8.0f);
							e.Graphics.DrawEllipse(new Pen(Color.Firebrick, 2.0f), u + 0.5f * g + dx - 4.0f, v + 0.5f * g + dy - 4.0f, 8.0f, 8.0f);
						}
						if (device == i & component == JoystickComponent.Hat & element == j)
						{
							double rx = ((HatPosition)direction & HatPosition.Left) != 0 ? -1.0 : ((HatPosition)direction & HatPosition.Right) != 0 ? 1.0 : 0.0;
							double ry = ((HatPosition)direction & HatPosition.Up) != 0 ? -1.0 : ((HatPosition)direction & HatPosition.Down) != 0 ? 1.0 : 0.0;
							double rt = rx * rx + ry * ry;
							rt = 1.0 / Math.Sqrt(rt);
							rx *= rt; ry *= rt;
							float dx = (float)(0.5 * rx * (g - 8.0));
							float dy = (float)(0.5 * ry * (g - 8.0));
							e.Graphics.FillEllipse(Brushes.Firebrick, u + 0.5f * g + dx - 2.0f, v + 0.5f * g + dy - 2.0f, 4.0f, 4.0f);
						}
						u += g + 8.0f;
					}
					if (u > m) m = u;

					// second row
					u = x;
					v = y + h + 8.0f;
					// axes
					g = pictureboxJoysticks.ClientRectangle.Height - v - 2.0f;
					for (int j = 0; j < Program.Joysticks.AttachedJoysticks[guid].AxisCount(); j++)
					{
						float r = (float)Program.Joysticks.AttachedJoysticks[guid].GetAxis(j);
						float r0 = r < 0.0f ? r : 0.0f;
						float r1 = r > 0.0f ? r : 0.0f;
						e.Graphics.FillRectangle((float)Math.Abs((double)r) < threshold ? Brushes.RosyBrown : Brushes.Firebrick, u, v + 0.5f * g - 0.5f * r1 * g, 16.0f, 0.5f * g * (r1 - r0));
						if (device == i & component == JoystickComponent.Axis & element == j)
						{
							if (direction == -1 & type != Translations.CommandType.AnalogFull)
							{
								e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
								e.Graphics.DrawRectangle(ps, u, v + 0.5f * g, 16.0f, 0.5f * g);
							}
							else if (direction == 1 & type != Translations.CommandType.AnalogFull)
							{
								e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
								e.Graphics.DrawRectangle(ps, u, v, 16.0f, 0.5f * g);
							}
							else
							{
								e.Graphics.DrawRectangle(ps, u, v, 16.0f, g);
							}
						}
						else
						{
							e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
						}
						e.Graphics.DrawLine(p, u, v + (0.5f - 0.5f * threshold) * g, u + 16.0f, v + (0.5f - 0.5f * threshold) * g);
						e.Graphics.DrawLine(p, u, v + (0.5f + 0.5f * threshold) * g, u + 16.0f, v + (0.5f + 0.5f * threshold) * g);
						string t = "A" + (j + 1).ToString(Culture);
						SizeF s = e.Graphics.MeasureString(t, f);
						e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (16.0f - s.Width), v + g - s.Height - 2.0f);
						u += 24.0f;
					}

					// buttons
					g = 0.5f * (pictureboxJoysticks.ClientRectangle.Height - v - 10.0f);
					for (int j = 0; j < Program.Joysticks.AttachedJoysticks[guid].ButtonCount(); j++)
					{
						bool q = Program.Joysticks.AttachedJoysticks[guid].GetButton(j) != 0;
						float dv = (j & 1) * (g + 8.0f);
						if (q) e.Graphics.FillRectangle(Brushes.Firebrick, u, v + dv, g, g);
						if (device == i & component == JoystickComponent.Button & element == j)
						{
							e.Graphics.DrawRectangle(ps, u, v + dv, g, g);
						}
						else
						{
							e.Graphics.DrawRectangle(p, u, v + dv, g, g);
						}
						string t = "B" + (j + 1).ToString(Culture);
						SizeF s = e.Graphics.MeasureString(t, f);
						e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (g - s.Width), v + dv + 0.5f * (g - s.Height));
						if ((j & 1) != 0 | j == Program.Joysticks.AttachedJoysticks[guid].ButtonCount() - 1) u += g + 8.0f;
					}
					if (u > m) m = u;
				} else {
					m = x + w + 64.0f;
				}
				x = m + 8.0f;               
			}
		}

		private void pictureboxJoysticks_Click(object sender, EventArgs e)
		{
			MouseEventArgs mouseEvent = (MouseEventArgs)e;
			for (int i = 0; i < configureLinkLocations.Length; i++)
			{
				if (configureLinkLocations[i] != null)
				{
					if (mouseEvent.Location.X > configureLinkLocations[i][0].X && mouseEvent.Location.X < configureLinkLocations[i][1].X)
					{
						if (mouseEvent.Location.Y > configureLinkLocations[i][0].Y && mouseEvent.Location.Y < configureLinkLocations[i][1].Y)
						{
							AbstractJoystick j = Program.Joysticks.AttachedJoysticks.ElementAt(i).Value;
							switch (j.ConfigurationLink)
							{
								case ConfigurationLink.DenshaDeGo:
									for (int p = 0; p < InputDevicePlugin.AvailablePluginInfos.Count; p++)
									{
										if (string.Equals(InputDevicePlugin.AvailablePluginInfos[p].FileName, "DenshaDeGoInput.dll", StringComparison.InvariantCultureIgnoreCase))
										{
											if (InputDevicePlugin.AvailablePluginInfos[p].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable)
											{
												InputDevicePlugin.AvailablePlugins[p].Config(this);
											}
											else if (InputDevicePlugin.AvailablePluginInfos[p].Status == InputDevicePlugin.PluginInfo.PluginStatus.Disable)
											{
												InputDevicePlugin.CallPluginLoad(p, Program.CurrentHost);
												UpdateInputDeviceListViewItem(listviewInputDevice.Items[p], p, true);
												if (listviewInputDevice.SelectedIndices.Contains(p))
												{
													UpdateInputDeviceComponent(InputDevicePlugin.AvailablePluginInfos[p].Status);
												}
												InputDevicePlugin.AvailablePlugins[p].Config(this);
											}
										}
									}
									break;
								case ConfigurationLink.RailDriver:
									using (FormRaildriverCalibration f = new FormRaildriverCalibration())
									{
										f.ShowDialog();
									}
									break;
							}
							return;
						}
					}
				}
			}
			
		}
	}
}
