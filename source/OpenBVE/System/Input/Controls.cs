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
		/// <param name="fileOrNull">An absolute file path if we are exporting the controls, or a null reference to save to the default configuration location</param>
		/// <param name="controlsToSave">The list of controls to save</param>
		internal static void SaveControls(string fileOrNull, Control[] controlsToSave) {
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			builder.AppendLine("; Current control configuration");
			builder.AppendLine("; =============================");
			builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			builder.AppendLine("; This file is INCOMPATIBLE with versions older than 1.4.4.");
			builder.AppendLine();
			for (int i = 0; i < controlsToSave.Length; i++) {
				if (controlsToSave[i].Command == Translations.Command.None)
				{
					// don't save a control with no command set, just stores up problems for later
					continue;
				}
				Translations.CommandInfo info = Translations.CommandInfos.TryGetInfo(controlsToSave[i].Command);
				builder.Append(info.Name + ", " + controlsToSave[i]);
				
				builder.Append("\n");
			}
			string file = fileOrNull ?? OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
			File.WriteAllText(file, builder.ToString(), new System.Text.UTF8Encoding(true));
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
		/// <param name="fileOrNull">An absolute path reference to a saved controls.cfg file, or a null reference to check the default locations</param>
		/// <param name="controls">The current controls array</param>
		internal static void LoadControls(string fileOrNull, out Control[] controls)
		{
			string file;
			string[] lines = {};
			try
			{
				//Don't crash horribly if the embedded default controls file is missing (makefile.....)
				lines = GetLines(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenBve.Default.controls"));
			}
			catch
			{
				//ignored
			}
			
			if (fileOrNull == null)
			{
				file = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
				if (!File.Exists(file))
				{
					file = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
				}

				if (!File.Exists(file))
				{
					//Load the default key assignments if the user settings don't exist
					file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default.controls");
					if (!File.Exists(file))
					{
						MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","warning"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_missing"}),
							Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				}
			}
			else
			{
				file = fileOrNull;
			}

			controls = new Control[256];
			int length = 0;
			CultureInfo culture = CultureInfo.InvariantCulture;
			if (File.Exists(file))
			{
				lines = File.ReadAllLines(file, new System.Text.UTF8Encoding());
			}

			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = lines[i].Trim();
				if (lines[i].Length != 0 && !lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
				{
					string[] terms = lines[i].Split(',');
					for (int j = 0; j < terms.Length; j++)
					{
						terms[j] = terms[j].Trim();
					}

					if (terms.Length >= 2)
					{
						if (length >= controls.Length)
						{
							Array.Resize(ref controls, controls.Length << 1);
						}

						// note: to get rid of the underscore in the saved commmand file would require a format change, and this isn't a performance sensitive area hence don't bother....
						if (!Enum.TryParse(terms[0].Replace("_", string.Empty), true, out Translations.Command parsedCommand))
						{
							controls[length].Command = Translations.Command.None;
							controls[length].InheritedType = Translations.CommandType.Digital;
							controls[length].Method = ControlMethod.Invalid;
							controls[length].Device = Guid.Empty;
							controls[length].Component = JoystickComponent.Invalid;
							controls[length].Element = -1;
							controls[length].Direction = 0;
							controls[length].Modifier = KeyboardModifier.None;
							controls[length].Option = 0;
						}
						else
						{
							controls[length].Command = parsedCommand;
							controls[length].InheritedType = Translations.CommandInfos[parsedCommand].Type;
							Enum.TryParse(terms[1], true, out ControlMethod method);
							bool valid = false;
							if (method == ControlMethod.Keyboard & terms.Length >= 4)
							{
								if (int.TryParse(terms[2], out _))
								{
									//We've discovered a SDL keybinding is present, so reset the loading process with the default keyconfig & show an appropriate error message
									if (ControlsReset == false)
									{
										MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_oldversion"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_reset"}), Application.ProductName,
											MessageBoxButtons.OK, MessageBoxIcon.Hand);
									}

									var defaultControls = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
									if (File.Exists(defaultControls))
									{
										if (ControlsReset == false)
										{
											ControlsReset = true;
											LoadControls(defaultControls, out CurrentControls);
										}
										else
										{
											MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","warning"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_default_oldversion"}),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
											i = 0;
											lines = GetLines(Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenBve.Default.controls"));
											continue;
										}

									}
									else
									{
										MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","warning"}) + Environment.NewLine + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","controls_default_missing"}),
											Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
										controls = new Control[0];
									}

									return;
								}

								if (Enum.TryParse(terms[2], true, out Key currentKey))
								{
									if (int.TryParse(terms[3], NumberStyles.Integer, culture, out int modifiers))
									{
										controls[length].Method = method;
										controls[length].Device = Guid.Empty;
										controls[length].Component = JoystickComponent.Invalid;
										controls[length].Key = (OpenBveApi.Input.Key)currentKey;
										controls[length].Direction = 0;
										controls[length].Modifier = (KeyboardModifier) modifiers;
										if (terms.Length >= 5 && int.TryParse(terms[4], NumberStyles.Integer, culture, out int option))
										{
											controls[length].Option = option;
										}

										valid = true;
									}
								}
							}
							else if (method == ControlMethod.Joystick & terms.Length >= 4)
							{
								Guid device = Guid.Empty;
								if (int.TryParse(terms[2], NumberStyles.Integer, culture, out int oldDevice))
								{
									device = Joystick.GetGuid(oldDevice);
								}

								Enum.TryParse(terms[3], true, out JoystickComponent component);
								
								if (device != Guid.Empty || Guid.TryParse(terms[2], out device))
								{
									
									if (component == JoystickComponent.Axis & terms.Length >= 6)
									{
										if (int.TryParse(terms[4], out int currentAxis))
										{
											if (int.TryParse(terms[5], NumberStyles.Integer, culture, out int direction))
											{

												controls[length].Method = method;
												controls[length].Device = device;
												controls[length].Component = JoystickComponent.Axis;
												controls[length].Element = currentAxis;
												controls[length].Direction = direction;
												controls[length].Modifier = KeyboardModifier.None;
												if (terms.Length >= 7 && int.TryParse(terms[6], NumberStyles.Integer, culture, out int option))
												{
													controls[length].Option = option;
												}

												valid = true;
											}
										}
									}
									else if (component == JoystickComponent.Hat & terms.Length >= 6)
									{
										if (int.TryParse(terms[4], out int currentHat))
										{
											if (int.TryParse(terms[5], out int hatDirection))
											{
												controls[length].Method = method;
												controls[length].Device = device;
												controls[length].Component = JoystickComponent.Hat;
												controls[length].Element = currentHat;
												controls[length].Direction = hatDirection;
												controls[length].Modifier = KeyboardModifier.None;
												if (terms.Length >= 7 && int.TryParse(terms[6], NumberStyles.Integer, culture, out int option))
												{
													controls[length].Option = option;
												}

												valid = true;
											}

										}
									}
									else if (component == JoystickComponent.Button & terms.Length >= 5)
									{
										if (int.TryParse(terms[4], out int currentButton))
										{
											controls[length].Method = method;
											controls[length].Device = device;
											controls[length].Component = JoystickComponent.Button;
											controls[length].Element = currentButton;
											controls[length].Direction = 0;
											controls[length].Modifier = KeyboardModifier.None;
											if (terms.Length >= 6 && int.TryParse(terms[5], NumberStyles.Integer, culture, out int option))
											{
												controls[length].Option = option;
											}

											valid = true;
										}
									}

								}
							}
							else if (method == ControlMethod.RailDriver & terms.Length >= 4)
							{
								Guid device = Guid.Empty;
								if (int.TryParse(terms[2], NumberStyles.Integer, culture, out int oldDevice))
								{
									device = Joystick.GetGuid(oldDevice);
								}
								
								if (device != Guid.Empty || Guid.TryParse(terms[2], out device))
								{
									Enum.TryParse(terms[3], true, out JoystickComponent component);
									if (component == JoystickComponent.Axis & terms.Length >= 6)
									{
										if (int.TryParse(terms[4], out int currentAxis))
										{
											if (int.TryParse(terms[5], NumberStyles.Integer, culture, out int direction))
											{

												controls[length].Method = method;
												controls[length].Device = device;
												controls[length].Component = JoystickComponent.Axis;
												controls[length].Element = currentAxis;
												controls[length].Direction = direction;
												controls[length].Modifier = KeyboardModifier.None;
												if (terms.Length >= 7 && int.TryParse(terms[6], NumberStyles.Integer, culture, out int option))
												{
													controls[length].Option = option;
												}

												valid = true;
											}
										}
									}
									else if (component == JoystickComponent.Button & terms.Length >= 5)
									{
										if (int.TryParse(terms[4], out int currentButton))
										{
											controls[length].Method = ControlMethod.RailDriver;
											controls[length].Device = device;
											controls[length].Component = JoystickComponent.Button;
											controls[length].Element = currentButton;
											controls[length].Direction = 0;
											controls[length].Modifier = KeyboardModifier.None;
											if (terms.Length >= 6 && int.TryParse(terms[5], NumberStyles.Integer, culture, out int option))
											{
												controls[length].Option = option;
											}

											valid = true;
										}
									}

								}
							}

							if (!valid)
							{
								controls[length].Method = ControlMethod.Invalid;
								controls[length].Device = Guid.Empty;
								controls[length].Component = JoystickComponent.Invalid;
								controls[length].Element = -1;
								controls[length].Direction = 0;
								controls[length].Modifier = KeyboardModifier.None;
								controls[length].Option = 0;
							}
						}

						length++;
					}
				}
			}
			Array.Resize(ref controls, length);
		}


		/// <summary>Adds an array of controls to an existing control array</summary>
		/// <param name="baseControls">The base control array</param>
		/// <param name="additionalControls">The new controls to add</param>
		internal static void AddControls(ref Control[] baseControls, Control[] additionalControls) {
			for (int i = 0; i < additionalControls.Length; i++) {
				int j;
				for (j = 0; j < baseControls.Length; j++) {
					if (additionalControls[i].Command == baseControls[j].Command) break;
				}
				if (j == baseControls.Length) {
					Array.Resize(ref baseControls, baseControls.Length + 1);
					baseControls[baseControls.Length - 1] = additionalControls[i];
				}
			}
		}
	}
}
