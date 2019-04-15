using System;
using System.Globalization;
using System.Windows.Forms;
using OpenTK.Input;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>The list of current in-game controls</summary>
		internal static Control[] CurrentControls = { };

		/// <summary>Saves a control configuration to disk</summary>
		/// <param name="FileOrNull">An absolute file path if we are exporting the controls, or a null reference to save to the default configuration location</param>
		/// <param name="controlsToSave">The list of controls to save</param>
		internal static void SaveControls(string FileOrNull, Control[] controlsToSave) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			Builder.AppendLine("; Current control configuration");
			Builder.AppendLine("; =============================");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			Builder.AppendLine("; This file is INCOMPATIBLE with versions older than 1.4.4.");
			Builder.AppendLine();
			for (int i = 0; i < controlsToSave.Length; i++) {
				Translations.CommandInfo Info = Translations.CommandInfos.TryGetInfo(controlsToSave[i].Command);
				Builder.Append(Info.Name + ", ");
				switch (controlsToSave[i].Method) {
					case ControlMethod.Keyboard:
						Builder.Append("keyboard, " + controlsToSave[i].Key + ", " + ((int)controlsToSave[i].Modifier).ToString(Culture) + ", " + controlsToSave[i].Option.ToString(Culture));
						break;
					case ControlMethod.Joystick:
						Builder.Append("joystick, " + controlsToSave[i].Device.ToString(Culture) + ", ");
						switch (controlsToSave[i].Component) {
							case JoystickComponent.Axis:
								Builder.Append("axis, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Ball:
								Builder.Append("ball, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Hat:
								Builder.Append("hat, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Button:
								Builder.Append("button, " + controlsToSave[i].Element.ToString(Culture));
								break;
							default:
								Builder.Append("invalid");
								break;
						}
						Builder.Append(", " + controlsToSave[i].Option.ToString(Culture));
						break;
					case ControlMethod.RailDriver:
						Builder.Append("raildriver, 0, ");
						switch (controlsToSave[i].Component) {
							case JoystickComponent.Axis:
								Builder.Append("axis, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Button:
								Builder.Append("button, " + controlsToSave[i].Element.ToString(Culture));
								break;
							default:
								Builder.Append("invalid");
								break;
						}
						Builder.Append(", " + controlsToSave[i].Option.ToString(Culture));
						break;
				}
				Builder.Append("\n");
			}
			string File;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
			} else {
				File = FileOrNull;
			}
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}

		/// <summary>Loads the current controls from the controls.cfg file</summary>
		/// <param name="FileOrNull">An absolute path reference to a saved controls.cfg file, or a null reference to check the default locations</param>
		/// <param name="Controls">The current controls array</param>
		internal static void LoadControls(string FileOrNull, out Control[] Controls)
		{
			string File;
			bool ControlsReset = false;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
				if (!System.IO.File.Exists(File))
				{
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
				}
				if (!System.IO.File.Exists(File)) {
					//Load the default key assignments if the user settings don't exist
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
					if (!System.IO.File.Exists(File))
					{
						MessageBox.Show(Translations.GetInterfaceString("errors_warning") + Environment.NewLine + Translations.GetInterfaceString("errors_controls_missing"),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
						Controls = new Control[0];
						return;
					}
				}
			} else {
				File = FileOrNull;
			}
			Controls = new Control[256];
			int Length = 0;
			CultureInfo Culture = CultureInfo.InvariantCulture;
			if (System.IO.File.Exists(File)) {
				string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
				for (int i = 0; i < Lines.Length; i++) {
					Lines[i] = Lines[i].Trim();
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
						string[] Terms = Lines[i].Split(',');
						for (int j = 0; j < Terms.Length; j++) {
							Terms[j] = Terms[j].Trim();
						}
						if (Terms.Length >= 2) {
							if (Length >= Controls.Length) {
								Array.Resize<Control>(ref Controls, Controls.Length << 1);
							}
							int j;
							for (j = 0; j < Translations.CommandInfos.Length; j++) {
								if (string.Compare(Translations.CommandInfos[j].Name, Terms[0], StringComparison.OrdinalIgnoreCase) == 0) break;
							}
							if (j == Translations.CommandInfos.Length) {
								Controls[Length].Command = Translations.Command.None;
								Controls[Length].InheritedType = Translations.CommandType.Digital;
								Controls[Length].Method = ControlMethod.Invalid;
								Controls[Length].Device = -1;
								Controls[Length].Component = JoystickComponent.Invalid;
								Controls[Length].Element = -1;
								Controls[Length].Direction = 0;
								Controls[Length].Modifier = KeyboardModifier.None;
								Controls[Length].Option = 0;
							} else {
								Controls[Length].Command = Translations.CommandInfos[j].Command;
								Controls[Length].InheritedType = Translations.CommandInfos[j].Type;
								string Method = Terms[1].ToLowerInvariant();
								bool Valid = false;
								if (Method == "keyboard" & Terms.Length >= 4)
								{
									Key CurrentKey;
									// ReSharper disable once NotAccessedVariable
									int SDLTest;
									if (int.TryParse(Terms[2], out SDLTest))
									{
										//We've discovered a SDL keybinding is present, so reset the loading process with the default keyconfig & show an appropriate error message
										if (ControlsReset == false)
										{
											MessageBox.Show(Translations.GetInterfaceString("errors_controls_oldversion") + Environment.NewLine + Translations.GetInterfaceString("errors_controls_reset"), Application.ProductName,
												MessageBoxButtons.OK, MessageBoxIcon.Hand);
										}
										var DefaultControls = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"),"Default keyboard assignment.controls");
										if (System.IO.File.Exists(DefaultControls))
										{
											if (ControlsReset == false)
											{
												LoadControls(DefaultControls, out CurrentControls);
												ControlsReset = true;
											}
											else
											{
												MessageBox.Show(Translations.GetInterfaceString("errors_warning") + Environment.NewLine + Translations.GetInterfaceString("errors_controls_default_oldversion"),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
												Controls = new Control[0];
											}
											
										}
										else
										{
											MessageBox.Show(Translations.GetInterfaceString("errors_warning") + Environment.NewLine + Translations.GetInterfaceString("errors_controls_default_missing"),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
												Controls = new Control[0];
										}
										continue;
									}
									if (Enum.TryParse(Terms[2], true, out CurrentKey))
									{
										int Modifiers;
										if (int.TryParse(Terms[3], NumberStyles.Integer, Culture, out Modifiers))
										{
											Controls[Length].Method = ControlMethod.Keyboard;
											Controls[Length].Device = -1;
											Controls[Length].Component = JoystickComponent.Invalid;
											Controls[Length].Key = CurrentKey;
											Controls[Length].Direction = 0;
											Controls[Length].Modifier = (KeyboardModifier) Modifiers;
											int Option;
											if (Terms.Length >= 5 && int.TryParse(Terms[4], NumberStyles.Integer, Culture, out Option))
											{
												Controls[Length].Option = Option;
											}
											Valid = true;
										}
									}
								}
								

								 else if (Method == "joystick" & Terms.Length >= 4) {
									int Device;
									if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out Device)) {
										string Component = Terms[3].ToLowerInvariant();
										if (Component == "axis" & Terms.Length >= 6)
										{
											int CurrentAxis;
											if (Int32.TryParse(Terms[4], out CurrentAxis))
											{
												int Direction;
												if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Direction))
												{

													Controls[Length].Method = ControlMethod.Joystick;
													Controls[Length].Device = Device;
													Controls[Length].Component = JoystickComponent.Axis;
													Controls[Length].Element = CurrentAxis;
													Controls[Length].Direction = Direction;
													Controls[Length].Modifier = KeyboardModifier.None;
													int Option;
													if (Terms.Length >= 7 && int.TryParse(Terms[6], NumberStyles.Integer, Culture, out Option)) {
														Controls[Length].Option = Option;
													}
													Valid = true;
												}
											}
										}
										else if (Component == "hat" & Terms.Length >= 6)
										{
											int CurrentHat;
											if (Int32.TryParse(Terms[4], out CurrentHat))
											{
												int HatDirection;
												if (Int32.TryParse(Terms[5], out HatDirection))
												{
													Controls[Length].Method = ControlMethod.Joystick;
													Controls[Length].Device = Device;
													Controls[Length].Component = JoystickComponent.Hat;
													Controls[Length].Element = CurrentHat;
													Controls[Length].Direction = HatDirection;
													Controls[Length].Modifier = KeyboardModifier.None;
													int Option;
													if (Terms.Length >= 7 && int.TryParse(Terms[6], NumberStyles.Integer, Culture, out Option)) {
														Controls[Length].Option = Option;
													}
													Valid = true;
												}

											}
										}
										else if (Component == "button" & Terms.Length >= 5)
										{
											int CurrentButton;
											if (Int32.TryParse(Terms[4], out CurrentButton))
											{
												Controls[Length].Method = ControlMethod.Joystick;
												Controls[Length].Device = Device;
												Controls[Length].Component = JoystickComponent.Button;
												Controls[Length].Element = CurrentButton;
												Controls[Length].Direction = 0;
												Controls[Length].Modifier = KeyboardModifier.None;
												int Option;
												if (Terms.Length >= 6 && int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Option)) {
													Controls[Length].Option = Option;
												}
												Valid = true;
											}
										}

									}	  
								}
								else if (Method == "raildriver" & Terms.Length >= 4) {
									int Device;
									if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out Device)) {
										string Component = Terms[3].ToLowerInvariant();
										if (Component == "axis" & Terms.Length >= 6)
										{
											int CurrentAxis;
											if (Int32.TryParse(Terms[4], out CurrentAxis))
											{
												int Direction;
												if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Direction))
												{

													Controls[Length].Method = ControlMethod.RailDriver;
													Controls[Length].Device = Device;
													Controls[Length].Component = JoystickComponent.Axis;
													Controls[Length].Element = CurrentAxis;
													Controls[Length].Direction = Direction;
													Controls[Length].Modifier = KeyboardModifier.None;
													int Option;
													if (Terms.Length >= 7 && int.TryParse(Terms[6], NumberStyles.Integer, Culture, out Option)) {
														Controls[Length].Option = Option;
													}
													Valid = true;
												}
											}
										}
										else if (Component == "button" & Terms.Length >= 5)
										{
											int CurrentButton;
											if (Int32.TryParse(Terms[4], out CurrentButton))
											{
												Controls[Length].Method = ControlMethod.RailDriver;
												Controls[Length].Device = Device;
												Controls[Length].Component = JoystickComponent.Button;
												Controls[Length].Element = CurrentButton;
												Controls[Length].Direction = 0;
												Controls[Length].Modifier = KeyboardModifier.None;
												int Option;
												if (Terms.Length >= 6 && int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Option)) {
													Controls[Length].Option = Option;
												}
												Valid = true;
											}
										}

									}	  
								}

								if (!Valid) {
									Controls[Length].Method = ControlMethod.Invalid;
									Controls[Length].Device = -1;
									Controls[Length].Component = JoystickComponent.Invalid;
									Controls[Length].Element = -1;
									Controls[Length].Direction = 0;
									Controls[Length].Modifier = KeyboardModifier.None;
									Controls[Length].Option = 0;
								}
							}
							Length++;
						}
					}
				}
			}
			Array.Resize<Control>(ref Controls, Length);
		}

		/// <summary>Adds an array of controls to an existing control array</summary>
		/// <param name="Base">The base control array</param>
		/// <param name="Add">The new controls to add</param>
		internal static void AddControls(ref Control[] Base, Control[] Add) {
			for (int i = 0; i < Add.Length; i++) {
				int j;
				for (j = 0; j < Base.Length; j++) {
					if (Add[i].Command == Base[j].Command) break;
				}
				if (j == Base.Length) {
					Array.Resize<Control>(ref Base, Base.Length + 1);
					Base[Base.Length - 1] = Add[i];
				}
			}
		}
	}
}
