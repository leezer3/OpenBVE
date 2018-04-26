using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Input;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
		// ========
		// controls
		// ========

		// controls
		private void listviewControls_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				{
					this.Tag = new object();
					{ // command
						int j; for (j = 0; j < Interface.CommandInfos.Length; j++) {
							if (Interface.CommandInfos[j].Command == Interface.CurrentControls[i].Command) {
								comboboxCommand.SelectedIndex = j;
								break;
							}
						} if (j == Interface.CommandInfos.Length) {
							comboboxCommand.SelectedIndex = -1;
						}
					}
					// data
					if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Keyboard) {
						radiobuttonKeyboard.Checked = true;
					} else if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick || Interface.CurrentControls[i].Method == Interface.ControlMethod.RailDriver) {
						radiobuttonJoystick.Checked = true;
					} else {
						radiobuttonKeyboard.Checked = false;
						radiobuttonJoystick.Checked = false;
					}
					panelKeyboard.Enabled = radiobuttonKeyboard.Checked;
					if (radiobuttonKeyboard.Checked)
					{
						for (int k = 0; k < Interface.TranslatedKeys.Length; k++)
						{
							if (Interface.CurrentControls[i].Key == Interface.TranslatedKeys[k].Key)
							{
								comboboxKeyboardKey.SelectedIndex = k;
								break;
							}
						}
						checkboxKeyboardShift.Checked = (Interface.CurrentControls[i].Modifier & Interface.KeyboardModifier.Shift) != 0;
						checkboxKeyboardCtrl.Checked = (Interface.CurrentControls[i].Modifier & Interface.KeyboardModifier.Ctrl) != 0;
						checkboxKeyboardAlt.Checked = (Interface.CurrentControls[i].Modifier & Interface.KeyboardModifier.Alt) != 0;
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
					this.Tag = null;
				}
				buttonControlRemove.Enabled = true;
				buttonControlUp.Enabled = i > 0;
				buttonControlDown.Enabled = i < Interface.CurrentControls.Length - 1;
				groupboxControl.Enabled = true;
			} else {
				this.Tag = new object();
				comboboxCommand.SelectedIndex = -1;
				radiobuttonKeyboard.Checked = false;
				radiobuttonJoystick.Checked = false;
				groupboxControl.Enabled = false;
				comboboxKeyboardKey.SelectedIndex = -1;
				checkboxKeyboardShift.Checked = false;
				checkboxKeyboardCtrl.Checked = false;
				checkboxKeyboardAlt.Checked = false;
				labelJoystickAssignmentValue.Text = "";
				this.Tag = null;
				buttonControlRemove.Enabled = false;
				buttonControlUp.Enabled = false;
				buttonControlDown.Enabled = false;
			}
		}
		private void UpdateControlListElement(ListViewItem Item, int Index, bool ResizeColumns) {
			Interface.CommandInfo Info;
			Interface.TryGetCommandInfo(Interface.CurrentControls[Index].Command, out Info);
			Item.SubItems[0].Text = Info.Name;
			switch (Info.Type) {
					case Interface.CommandType.Digital: Item.SubItems[1].Text = Interface.GetInterfaceString("controls_list_type_digital"); break;
					case Interface.CommandType.AnalogHalf: Item.SubItems[1].Text = Interface.GetInterfaceString("controls_list_type_analoghalf"); break;
					case Interface.CommandType.AnalogFull: Item.SubItems[1].Text = Interface.GetInterfaceString("controls_list_type_analogfull"); break;
					default: Item.SubItems[1].Text = Info.Type.ToString(); break;
			}
			Item.SubItems[2].Text = Info.Description;
			if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Keyboard) {
				Item.ImageKey = @"keyboard";
			} else if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Joystick) {
				if (Info.Type == Interface.CommandType.AnalogHalf | Info.Type == Interface.CommandType.AnalogFull) {
					Item.ImageKey = @"joystick";
				} else {
					Item.ImageKey = @"gamepad";
				}
			} else {
				Item.ImageKey = null;
			}
			Item.SubItems[3].Text = GetControlDetails(Index);
			if (ResizeColumns) {
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		// get control details
		private string GetControlDetails(int Index) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Separator = Interface.GetInterfaceString("controls_assignment_separator");
			if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Keyboard) {
				string t = Interface.GetInterfaceString("controls_assignment_keyboard") + Separator;
				if ((Interface.CurrentControls[Index].Modifier & Interface.KeyboardModifier.Shift) != 0) t += Interface.GetInterfaceString("controls_assignment_keyboard_shift");
				if ((Interface.CurrentControls[Index].Modifier & Interface.KeyboardModifier.Ctrl) != 0) t += Interface.GetInterfaceString("controls_assignment_keyboard_ctrl");
				if ((Interface.CurrentControls[Index].Modifier & Interface.KeyboardModifier.Alt) != 0) t += Interface.GetInterfaceString("controls_assignment_keyboard_alt");
				
				
				int j; 
				for (j = 0; j < 133; j++) {
					//OpenTK key description
					if (Interface.CurrentControls[Index].Key != Key.Unknown)
					{
						for (int k = 0; k < Interface.TranslatedKeys.Length; k++)
						{
							if (Interface.CurrentControls[Index].Key == Interface.TranslatedKeys[k].Key)
							{
								t += Interface.TranslatedKeys[k].Description;
								return t;
							}
						}
						t += Interface.CurrentControls[Index].Key;
						return t;
					}
				} if (j == 133) {
					t += "{" + Interface.CurrentControls[Index].Element.ToString(Culture) + "}";
				}
				return t;
			} 
			
			if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Joystick) {

				string t = Interface.GetInterfaceString("controls_assignment_joystick").Replace("[index]", (Interface.CurrentControls[Index].Device + 1).ToString(Culture));
				switch (Interface.CurrentControls[Index].Component) {
					case Interface.JoystickComponent.Axis:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						if (Interface.CurrentControls[Index].Direction == -1) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_negative");
						} else if (Interface.CurrentControls[Index].Direction == 1) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_positive");
						} else {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_invalid");
						} break;
					case Interface.JoystickComponent.Button:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_button").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						break;
					case Interface.JoystickComponent.Hat:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						if (Interface.CurrentControls[Index].Direction == (int)HatPosition.Left) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_left");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.UpLeft) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_upleft");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.Up) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_up");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.UpRight) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_upright");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.Right) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_right");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.DownRight) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_downright");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.Down) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_down");
						} else if (Interface.CurrentControls[Index].Direction == (int)HatPosition.DownLeft) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_downleft");
						} else {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_invalid");
						} break;
				}
				return t;
			} 
			if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.RailDriver) {

				string t = "RailDriver";
				switch (Interface.CurrentControls[Index].Component) {
					case Interface.JoystickComponent.Axis:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						if (Interface.CurrentControls[Index].Direction == -1) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_negative");
						} else if (Interface.CurrentControls[Index].Direction == 1) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_positive");
						} else {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_invalid");
						} break;
					case Interface.JoystickComponent.Button:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_button").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						break;
				}
				return t;
			} 
			
				return Interface.GetInterfaceString("controls_assignment_invalid");
			
		}

		// control add
		private void buttonControlAdd_Click(object sender, EventArgs e) {
			for (int i = 0; i < Interface.CurrentControls.Length; i++) {
				listviewControls.Items[i].Selected = false;
			}
			int n = Interface.CurrentControls.Length;
			Array.Resize<Interface.Control>(ref Interface.CurrentControls, n + 1);
			Interface.CurrentControls[n].Command = Interface.Command.None;
			ListViewItem Item = new ListViewItem(new string[] { "", "", "", "" });
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
				Array.Resize<Interface.Control>(ref Interface.CurrentControls, Interface.CurrentControls.Length - 1);
				listviewControls.Items[j].Remove();
			}
		}

		// control up
		private void buttonControlUp_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (j > 0) {
					Interface.Control c = Interface.CurrentControls[j];
					Interface.CurrentControls[j] = Interface.CurrentControls[j - 1];
					Interface.CurrentControls[j - 1] = c;
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
					Interface.Control c = Interface.CurrentControls[j];
					Interface.CurrentControls[j] = Interface.CurrentControls[j + 1];
					Interface.CurrentControls[j + 1] = c;
					ListViewItem v = listviewControls.Items[j];
					listviewControls.Items.RemoveAt(j);
					listviewControls.Items.Insert(j + 1, v);
				}
			}
		}

		// command
		private void comboboxCommand_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				int j = comboboxCommand.SelectedIndex;
				if (j >= 0) {
					Interface.CurrentControls[i].Command = Interface.CommandInfos[j].Command;
					Interface.CommandInfo Info;
					Interface.TryGetCommandInfo(Interface.CommandInfos[j].Command, out Info);
					Interface.CurrentControls[i].InheritedType = Info.Type;
					UpdateControlListElement(listviewControls.Items[i], i, true);
				}
			}
		}

		// ========
		// keyboard
		// ========

		// keyboard
		private void radiobuttonKeyboard_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Method = Interface.ControlMethod.Keyboard;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
			panelKeyboard.Enabled = radiobuttonKeyboard.Checked;
		}

		// key
		private void comboboxKeyboardKey_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				int j = comboboxKeyboardKey.SelectedIndex;

				Interface.KeyInfo k = comboboxKeyboardKey.Items[j] is Interface.KeyInfo ? (Interface.KeyInfo) comboboxKeyboardKey.Items[j] : new Interface.KeyInfo();
				Interface.CurrentControls[i].Key = k.Key;
				UpdateControlListElement(listviewControls.Items[i], i, true); 
			}
		}

		// modifiers
		private void checkboxKeyboardShift_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? Interface.KeyboardModifier.Shift : Interface.KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? Interface.KeyboardModifier.Ctrl : Interface.KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? Interface.KeyboardModifier.Alt : Interface.KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}
		private void checkboxKeyboardCtrl_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? Interface.KeyboardModifier.Shift : Interface.KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? Interface.KeyboardModifier.Ctrl : Interface.KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? Interface.KeyboardModifier.Alt : Interface.KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}
		private void checkboxKeyboardAlt_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? Interface.KeyboardModifier.Shift : Interface.KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? Interface.KeyboardModifier.Ctrl : Interface.KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? Interface.KeyboardModifier.Alt : Interface.KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}

		
		
		// ========
		// joystick
		// ========

		// joystick
		private void radiobuttonJoystick_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Method = Interface.ControlMethod.Joystick;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
			panelJoystick.Enabled = radiobuttonJoystick.Checked;
			if (radiobuttonJoystick.Checked)
			{
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab");
			}
			else
			{
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_keyboard_assignment_grab");		
			}

		}

		// details
		private void UpdateJoystickDetails() {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
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
					Interface.GetInterfaceString("dialog_controlsfiles") + @"|*.controls|" +
					Interface.GetInterfaceString("dialog_allfiles") + @"|*"
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
						Items[i] = new ListViewItem(new string[] { "", "", "", "" });
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
				Interface.LoadControls(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls"), out Interface.CurrentControls);
				for (int i = 0; i < listviewControls.SelectedItems.Count; i++)
				{
					listviewControls.SelectedItems[i].Selected = false;
				}
				listviewControls.Items.Clear();
				ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
				for (int i = 0; i < Interface.CurrentControls.Length; i++)
				{
					Items[i] = new ListViewItem(new string[] { "", "", "", "" });
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
					Interface.GetInterfaceString("dialog_controlsfiles") + @"|*.controls|" +
					Interface.GetInterfaceString("dialog_allfiles") + @"|*"
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
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_keyboard_assignment_grabbing");
				textboxJoystickGrab.BackColor = Color.Crimson;
				textboxJoystickGrab.ForeColor = Color.White;
				KeyGrab = true;
				return;

			}
			bool FullAxis = false;
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (Interface.CurrentControls[j].InheritedType == Interface.CommandType.AnalogFull) {
					FullAxis = true;
				}
			}
			if (FullAxis) {
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab_fullaxis");
			} else {
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab_normal");
			}
			textboxJoystickGrab.BackColor = Color.Crimson;
			textboxJoystickGrab.ForeColor = Color.White;
		}

		private void textboxJoystickGrab_KeyDown(object sender, KeyEventArgs e)
		{
			//Required to avoid race condition with openTK recieving the same event internally
			System.Threading.Thread.Sleep(1);
			var kbState = OpenTK.Input.Keyboard.GetState();
			if (KeyGrab == false)
			{
				return;
			}
			for (int j = 0; j < Interface.TranslatedKeys.Length; j++)
			{
				if (kbState.IsKeyDown(Interface.TranslatedKeys[j].Key))
				{
					int i = listviewControls.SelectedIndices[0];
					Interface.CurrentControls[i].Key = Interface.TranslatedKeys[j].Key;
					UpdateControlListElement(listviewControls.Items[i], i, true);
					comboboxKeyboardKey.SelectedIndex = j;
				}
			}
			textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_keyboard_assignment_grab");
			textboxJoystickGrab.BackColor = panelControls.BackColor;
			textboxJoystickGrab.ForeColor = Color.Black;
			comboboxKeyboardKey.Focus();
		}
		private void textboxJoystickGrab_Leave(object sender, EventArgs e) {
			if (radiobuttonJoystick.Checked)
			{
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab");
			}
			else
			{
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_keyboard_assignment_grab");
			}
			textboxJoystickGrab.BackColor = panelControls.BackColor;
			textboxJoystickGrab.ForeColor = Color.Black;
			KeyGrab = false;
		}

		
		// attached joysticks
		private void pictureboxJoysticks_Paint(object sender, PaintEventArgs e)
		{
			this.DoubleBuffered = true;
			int device = -1;
			Interface.JoystickComponent component = Interface.JoystickComponent.Invalid;
			int element = -1;
			int direction = -1;
			Interface.CommandType type = Interface.CommandType.Digital;
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (Interface.CurrentControls[j].Method == Interface.ControlMethod.Joystick) {
					device = Interface.CurrentControls[j].Device;
					component = Interface.CurrentControls[j].Component;
					element = Interface.CurrentControls[j].Element;
					direction = Interface.CurrentControls[j].Direction;
					type = Interface.CurrentControls[j].InheritedType;
				}
			}
			
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			Font f = new Font(this.Font.Name, 0.875f * this.Font.Size);
			float x = 2.0f, y = 2.0f;
			float threshold = ((float)trackbarJoystickAxisThreshold.Value - (float)trackbarJoystickAxisThreshold.Minimum) / (float)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			for (int i = 0; i < JoystickManager.AttachedJoysticks.Length; i++)
			{
				JoystickManager.AttachedJoysticks[i].Poll();
				float w, h;
				if (JoystickImage != null) {
					e.Graphics.DrawImage(JoystickImage, x, y);
					w = (float)JoystickImage.Width;
					h = (float)JoystickImage.Height;
					if (h < 64.0f) h = 64.0f;
				} else {
					w = 64.0f; h = 64.0f;
					e.Graphics.DrawRectangle(new Pen(labelControlsTitle.BackColor), x, y, w, h);
				}
				{ // joystick number
					e.Graphics.FillEllipse(Brushes.Gold, x + w - 16.0f, y, 16.0f, 16.0f);
					e.Graphics.DrawEllipse(Pens.Black, x + w - 16.0f, y, 16.0f, 16.0f);
					string t = (i + 1).ToString(Culture);
					SizeF s = e.Graphics.MeasureString(t, f);
					e.Graphics.DrawString(t, f, Brushes.Black, x + w - 8.0f - 0.5f * s.Width, y + 8.0f - 0.5f * s.Height);
				}
				{ // joystick name
					e.Graphics.DrawString(JoystickManager.AttachedJoysticks[i].Name, this.Font, Brushes.Black, x + w + 8.0f, y);
				}
				if (OpenTK.Configuration.RunningOnSdl2)
				{
					//HACK: Control configuration doesn't work in-form on SDL2
					string error = Interface.GetInterfaceString("errors_controls_ingame");
					if (OpenTK.Configuration.RunningOnSdl2)
					{
						error = error.Replace("[platform]", "SDL2");
					}
					e.Graphics.DrawString(error, this.Font, Brushes.Black, x + w + 8.0f, y + 30.0f);
					return;
				}
				float m;
				if (groupboxJoysticks.Enabled) {
					m = x;
					Pen p = new Pen(Color.DarkGoldenrod, 2.0f);
					Pen ps = new Pen(Color.Firebrick, 2.0f);
					{ // first row
						float u = x + w + 8.0f;
						float v = y + 24.0f;
						float g = h - 24.0f;
						{ // hats
							int n = JoystickManager.AttachedJoysticks[i].HatCount();
							for (int j = 0; j < n; j++) {
								if (device == i & component == Interface.JoystickComponent.Hat & element == j) {
									e.Graphics.DrawEllipse(ps, u, v, g, g);
								} else {
									e.Graphics.DrawEllipse(p, u, v, g, g);
								}
								string t = "H" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (g - s.Width), v + 0.5f * (g - s.Height));
								JoystickHatState aa = JoystickManager.AttachedJoysticks[i].GetHat(j);
								HatPosition a = aa.Position;
								if (a != HatPosition.Centered)
								{
									double rx = 0.0;
									double ry = 0.0;
									switch (a)
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
								if (device == i & component == Interface.JoystickComponent.Hat & element == j) {
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
						}
						if (u > m) m = u;
					}
					
					{ // second row
						float u = x;
						float v = y + h + 8.0f;
						{ // axes
							int n = JoystickManager.AttachedJoysticks[i].AxisCount();
							float g = (float)pictureboxJoysticks.ClientRectangle.Height - v - 2.0f;
							for (int j = 0; j < n; j++) {
								float r = (float)JoystickManager.AttachedJoysticks[i].GetAxis(j);
								float r0 = r < 0.0f ? r : 0.0f;
								float r1 = r > 0.0f ? r : 0.0f;
								if ((float)Math.Abs((double)r) < threshold) {
									e.Graphics.FillRectangle(Brushes.RosyBrown, u, v + 0.5f * g - 0.5f * r1 * g, 16.0f, 0.5f * g * (r1 - r0));
								} else {
									e.Graphics.FillRectangle(Brushes.Firebrick, u, v + 0.5f * g - 0.5f * r1 * g, 16.0f, 0.5f * g * (r1 - r0));
								}
								if (device == i & component == Interface.JoystickComponent.Axis & element == j) {
									if (direction == -1 & type != Interface.CommandType.AnalogFull) {
										e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
										e.Graphics.DrawRectangle(ps, u, v + 0.5f * g, 16.0f, 0.5f * g);
									} else if (direction == 1 & type != Interface.CommandType.AnalogFull) {
										e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
										e.Graphics.DrawRectangle(ps, u, v, 16.0f, 0.5f * g);
									} else {
										e.Graphics.DrawRectangle(ps, u, v, 16.0f, g);
									}
								} else {
									e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
								}
								e.Graphics.DrawLine(p, u, v + (0.5f - 0.5f * threshold) * g, u + 16.0f, v + (0.5f - 0.5f * threshold) * g);
								e.Graphics.DrawLine(p, u, v + (0.5f + 0.5f * threshold) * g, u + 16.0f, v + (0.5f + 0.5f * threshold) * g);
								string t = "A" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (16.0f - s.Width), v + g - s.Height - 2.0f);
								u += 24.0f;
							}
						}
						
						{ // buttons
							int n = JoystickManager.AttachedJoysticks[i].ButtonCount();
							float g = (float)0.5f * (pictureboxJoysticks.ClientRectangle.Height - v - 10.0f);
							for (int j = 0; j < n; j++) {
								bool q = JoystickManager.AttachedJoysticks[i].GetButton(j) != 0;
								float dv = (float)(j & 1) * (g + 8.0f);
								if (q) e.Graphics.FillRectangle(Brushes.Firebrick, u, v + dv, g, g);
								if (device == i & component == Interface.JoystickComponent.Button & element == j) {
									e.Graphics.DrawRectangle(ps, u, v + dv, g, g);
								} else {
									e.Graphics.DrawRectangle(p, u, v + dv, g, g);
								}
								string t = "B" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (g - s.Width), v + dv + 0.5f * (g - s.Height));
								if ((j & 1) != 0 | j == n - 1) u += g + 8.0f;
							}
						}
						 
						if (u > m) m = u;
					}
					 
				} else {
					m = x + w + 64.0f;
				}
				x = m + 8.0f;               
			}
		}
	}
}
