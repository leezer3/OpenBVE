using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenBveApi.Hosts;
using OpenTK.Input;
using OpenBveApi.Interface;
using Control = OpenBveApi.Interface.Control;

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
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			Builder.AppendLine("; Current control configuration");
			Builder.AppendLine("; =============================");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			Builder.AppendLine("; This file is INCOMPATIBLE with versions older than 1.4.4.");
			Builder.AppendLine();
			for (int i = 0; i < controlsToSave.Length; i++) {
				if (controlsToSave[i].Command == Translations.Command.None)
				{
					// don't save a control with no command set, just stores up problems for later
					continue;
				}
				Translations.CommandInfo Info = Translations.CommandInfos.TryGetInfo(controlsToSave[i].Command);
				Builder.Append(Info.Name + ", " + controlsToSave[i]);
				
				Builder.Append("\n");
			}
			string File = FileOrNull ?? OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}

		private static bool ControlsReset = false;

		private static string[] GetLines(Stream resourceStream)
		{
			List<string> lines = new List<string>();
			using (StreamReader reader = new StreamReader(resourceStream))
			{
				string currentLine;
				while ((currentLine = reader.ReadLine()) != null)
				{
					lines.Add(currentLine);
				}
			}
			return lines.ToArray();
		}

		/// <summary>Loads the current controls from the controls.cfg file</summary>
		/// <param name="FileOrNull">An absolute path reference to a saved controls.cfg file, or a null reference to check the default locations</param>
		/// <param name="Controls">The current controls array</param>
		internal static void LoadControls(string FileOrNull, out Control[] Controls)
		{
			string File;
			string[] Lines = {};
			try
			{
				//Don't crash horribly if the embedded default controls file is missing (makefile.....)
				Lines = GetLines(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenBve.Default.controls"));
			}
			catch
			{
				//ignored
			}
			
			if (FileOrNull == null)
			{
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
				if (!System.IO.File.Exists(File))
				{
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
				}

				if (!System.IO.File.Exists(File))
				{
					//Load the default key assignments if the user settings don't exist
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default.controls");
					if (!System.IO.File.Exists(File))
					{
						Program.ShowMessageBox(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","warning"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_missing"}),
							Application.ProductName);
					}
				}
			}
			else
			{
				File = FileOrNull;
			}

			Controls = new Control[256];
			int Length = 0;
			CultureInfo Culture = CultureInfo.InvariantCulture;
			if (System.IO.File.Exists(File))
			{
				Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
			}

			for (int i = 0; i < Lines.Length; i++)
			{
				Lines[i] = Lines[i].Trim();
				if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
				{
					string[] Terms = Lines[i].Split(',');
					for (int j = 0; j < Terms.Length; j++)
					{
						Terms[j] = Terms[j].Trim();
					}

					if (Terms.Length >= 2)
					{
						if (Length >= Controls.Length)
						{
							Array.Resize(ref Controls, Controls.Length << 1);
						}

						// note: to get rid of the underscore in the saved commmand file would require a format change, and this isn't a performance sensitive area hence don't bother....
						if (!Enum.TryParse(Terms[0].Replace("_", string.Empty), true, out Translations.Command parsedCommand))
						{
							Controls[Length].Command = Translations.Command.None;
							Controls[Length].InheritedType = Translations.CommandType.Digital;
							Controls[Length].Method = ControlMethod.Invalid;
							Controls[Length].Device = Guid.Empty;
							Controls[Length].Component = JoystickComponent.Invalid;
							Controls[Length].Element = -1;
							Controls[Length].Direction = 0;
							Controls[Length].Modifier = KeyboardModifier.None;
							Controls[Length].Option = 0;
						}
						else
						{
							Controls[Length].Command = parsedCommand;
							Controls[Length].InheritedType = Translations.CommandInfos[parsedCommand].Type;
							Enum.TryParse(Terms[1], true, out ControlMethod Method);
							bool Valid = false;
							if (Method == ControlMethod.Keyboard & Terms.Length >= 4)
							{
								if (int.TryParse(Terms[2], out _))
								{
									//We've discovered a SDL keybinding is present, so reset the loading process with the default keyconfig & show an appropriate error message
									if (ControlsReset == false)
									{
										Program.ShowMessageBox(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_oldversion"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_reset"}), Application.ProductName);
									}

									var DefaultControls = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
									if (System.IO.File.Exists(DefaultControls))
									{
										if (ControlsReset == false)
										{
											ControlsReset = true;
											LoadControls(DefaultControls, out CurrentControls);
										}
										else
										{
											Program.ShowMessageBox(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","warning"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_default_oldversion"}),
												Application.ProductName);
											i = 0;
											Lines = GetLines(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenBve.Default.controls"));
											continue;
										}

									}
									else
									{
										Program.ShowMessageBox(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","warning"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_default_missing"}),
											Application.ProductName);
										Controls = new Control[0];
									}

									return;
								}

								if (Enum.TryParse(Terms[2], true, out Key CurrentKey))
								{
									if (int.TryParse(Terms[3], NumberStyles.Integer, Culture, out int Modifiers))
									{
										Controls[Length].Method = Method;
										Controls[Length].Device = Guid.Empty;
										Controls[Length].Component = JoystickComponent.Invalid;
										Controls[Length].Key = (OpenBveApi.Input.Key)CurrentKey;
										Controls[Length].Direction = 0;
										Controls[Length].Modifier = (KeyboardModifier) Modifiers;
										if (Terms.Length >= 5 && int.TryParse(Terms[4], NumberStyles.Integer, Culture, out int Option))
										{
											Controls[Length].Option = Option;
										}

										Valid = true;
									}
								}
							}
							else if (Method == ControlMethod.Joystick & Terms.Length >= 4)
							{
								Guid Device = Guid.Empty;
								if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out int oldDevice))
								{
									Device = Joystick.GetGuid(oldDevice);
								}

								Enum.TryParse(Terms[3], true, out JoystickComponent Component);
								
								if (Device != Guid.Empty || Guid.TryParse(Terms[2], out Device))
								{
									
									if (Component == JoystickComponent.Axis & Terms.Length >= 6)
									{
										if (int.TryParse(Terms[4], out int CurrentAxis))
										{
											if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out int Direction))
											{

												Controls[Length].Method = Method;
												Controls[Length].Device = Device;
												Controls[Length].Component = JoystickComponent.Axis;
												Controls[Length].Element = CurrentAxis;
												Controls[Length].Direction = Direction;
												Controls[Length].Modifier = KeyboardModifier.None;
												if (Terms.Length >= 7 && int.TryParse(Terms[6], NumberStyles.Integer, Culture, out int Option))
												{
													Controls[Length].Option = Option;
												}

												Valid = true;
											}
										}
									}
									else if (Component == JoystickComponent.Hat & Terms.Length >= 6)
									{
										if (int.TryParse(Terms[4], out int CurrentHat))
										{
											if (int.TryParse(Terms[5], out int HatDirection))
											{
												Controls[Length].Method = Method;
												Controls[Length].Device = Device;
												Controls[Length].Component = JoystickComponent.Hat;
												Controls[Length].Element = CurrentHat;
												Controls[Length].Direction = HatDirection;
												Controls[Length].Modifier = KeyboardModifier.None;
												if (Terms.Length >= 7 && int.TryParse(Terms[6], NumberStyles.Integer, Culture, out int Option))
												{
													Controls[Length].Option = Option;
												}

												Valid = true;
											}

										}
									}
									else if (Component == JoystickComponent.Button & Terms.Length >= 5)
									{
										if (int.TryParse(Terms[4], out int CurrentButton))
										{
											Controls[Length].Method = Method;
											Controls[Length].Device = Device;
											Controls[Length].Component = JoystickComponent.Button;
											Controls[Length].Element = CurrentButton;
											Controls[Length].Direction = 0;
											Controls[Length].Modifier = KeyboardModifier.None;
											if (Terms.Length >= 6 && int.TryParse(Terms[5], NumberStyles.Integer, Culture, out int Option))
											{
												Controls[Length].Option = Option;
											}

											Valid = true;
										}
									}

								}
							}
							else if (Method == ControlMethod.RailDriver & Terms.Length >= 4)
							{
								Guid Device = Guid.Empty;
								if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out int oldDevice))
								{
									Device = Joystick.GetGuid(oldDevice);
								}
								
								if (Device != Guid.Empty || Guid.TryParse(Terms[2], out Device))
								{
									Enum.TryParse(Terms[3], true, out JoystickComponent Component);
									if (Component == JoystickComponent.Axis & Terms.Length >= 6)
									{
										if (int.TryParse(Terms[4], out int CurrentAxis))
										{
											if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out int Direction))
											{

												Controls[Length].Method = Method;
												Controls[Length].Device = Device;
												Controls[Length].Component = JoystickComponent.Axis;
												Controls[Length].Element = CurrentAxis;
												Controls[Length].Direction = Direction;
												Controls[Length].Modifier = KeyboardModifier.None;
												if (Terms.Length >= 7 && int.TryParse(Terms[6], NumberStyles.Integer, Culture, out int Option))
												{
													Controls[Length].Option = Option;
												}

												Valid = true;
											}
										}
									}
									else if (Component == JoystickComponent.Button & Terms.Length >= 5)
									{
										if (int.TryParse(Terms[4], out int CurrentButton))
										{
											Controls[Length].Method = ControlMethod.RailDriver;
											Controls[Length].Device = Device;
											Controls[Length].Component = JoystickComponent.Button;
											Controls[Length].Element = CurrentButton;
											Controls[Length].Direction = 0;
											Controls[Length].Modifier = KeyboardModifier.None;
											if (Terms.Length >= 6 && int.TryParse(Terms[5], NumberStyles.Integer, Culture, out int Option))
											{
												Controls[Length].Option = Option;
											}

											Valid = true;
										}
									}

								}
							}

							if (!Valid)
							{
								Controls[Length].Method = ControlMethod.Invalid;
								Controls[Length].Device = Guid.Empty;
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
			Array.Resize(ref Controls, Length);
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
					Array.Resize(ref Base, Base.Length + 1);
					Base[Base.Length - 1] = Add[i];
				}
			}
		}
	}
}
