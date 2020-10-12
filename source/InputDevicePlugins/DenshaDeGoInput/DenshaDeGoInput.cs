//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Marc Riera, The OpenBVE Project
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
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Input Device Plugin class for controllers of the Densha de GO! series
	/// </summary>
	public class DenshaDeGoInput : ITrainInputDevice
	{
		public event EventHandler<InputEventArgs> KeyDown;
		public event EventHandler<InputEventArgs> KeyUp;

		public InputControl[] Controls { get; private set; }

		private static FileSystem FileSystem;
		public static string currentLanguage;

		private Config configForm;

		/// <summary>
		/// Whether the input plugin has just started running.
		/// </summary>
		internal bool loading = true;

		/// <summary>
		/// The specs of the driver's train.
		/// </summary>
		internal VehicleSpecs vehicleSpecs = new VehicleSpecs(5, BrakeTypes.AutomaticAirBrake, 8, false, 1);

		/// <summary>
		/// Whether the brake handle has been moved.
		/// </summary>
		internal bool brakeHandleMoved;

		/// <summary>
		/// Whether the power handle has been moved.
		/// </summary>
		internal bool powerHandleMoved;

		/// <summary>
		/// An array with the command indices configured for each brake notch.
		/// </summary>
		internal static int[] brakeCommands = new int[10];

		/// <summary>
		/// An array with the command indices configured for each power notch.
		/// </summary>
		internal static int[] powerCommands = new int[6];

		/// <summary>
		/// An array with the command indices configured for each button.
		/// </summary>
		internal static int[] buttonCommands = new int[5];


		/// <summary>
		/// Whether to convert the handle notches to match the driver's train.
		/// </summary>
		internal static bool convertNotches;

		/// <summary>
		/// Whether to assign the maximum and minimum notches to P1/P5 and B1/B8.
		/// </summary>
		internal static bool keepMaxMin;

		/// <summary>
		/// Whether to map the hold brake to B1.
		/// </summary>
		internal static bool mapHoldBrake;

		/// <summary>
		/// A function call when the Config button is pressed.
		/// </summary>
		/// <param name="owner">The owner of the window</param>
		public void Config(IWin32Window owner)
		{
			configForm.ShowDialog(owner);
		}

		/// <summary>
		/// A function called when the plugin is loaded.
		/// </summary>
		/// <param name="fileSystem">The instance of FileSytem class</param>
		/// <returns>Whether the plugin has been loaded successfully.</returns>
		public bool Load(FileSystem fileSystem)
		{
			configForm = new Config();
			FileSystem = fileSystem;

			// Load settings from the config file
			LoadConfig();

			// Define the list of commands
			// We allocate 50 slots per handle plus one slot per command
			int commandCount = Translations.CommandInfos.Length;
			Controls = new InputControl[100 + commandCount];
			// Brake notches
			for (int i = 0; i < 50; i++)
			{
				Controls[i].Command = Translations.Command.BrakeAnyNotch;
				Controls[i].Option = i;
			}
			// Power notches
			for (int i = 50; i < 100; i++)
			{
				Controls[i].Command = Translations.Command.PowerAnyNotch;
				Controls[i].Option = i - 50;
			}
			// Other commands
			for (int i = 0; i < commandCount; i++)
			{
				Controls[i + 100].Command = (Translations.Command)i;
			}

			// Configure the mappings for the buttons and notches
			ConfigureMappings();

			return true;
		}

		/// <summary>
		/// A function called when the plugin is unloaded.
		/// </summary>
		public void Unload()
		{
		}

		/// <summary>
		/// A function called on each frame.
		/// </summary>
		public void OnUpdateFrame()
		{
			// Brake handle
			if (brakeHandleMoved)
			{
				KeyUp(this, new InputEventArgs(Controls[brakeCommands[(int)InputTranslator.BrakeNotch]]));
				brakeHandleMoved = false;
			}
			// Power handle
			if (powerHandleMoved)
			{
				KeyUp(this, new InputEventArgs(Controls[50 + powerCommands[(int)InputTranslator.PowerNotch]]));
				powerHandleMoved = false;
			}
			// Select button
			//if (InputTranslator.ControllerButtons.Select == OpenTK.Input.ButtonState.Released)
			//{
				KeyUp(this, new InputEventArgs(Controls[100 + buttonCommands[0]]));
			//}
			// Start button
			if (InputTranslator.ControllerButtons.Start == OpenTK.Input.ButtonState.Released)
			{
				KeyUp(this, new InputEventArgs(Controls[100 + buttonCommands[1]]));
			}
			// A button
			if (InputTranslator.ControllerButtons.A == OpenTK.Input.ButtonState.Released)
			{
				KeyUp(this, new InputEventArgs(Controls[100 + buttonCommands[2]]));
			}
			// B button
			if (InputTranslator.ControllerButtons.B == OpenTK.Input.ButtonState.Released)
			{
				KeyUp(this, new InputEventArgs(Controls[100 + buttonCommands[3]]));
			}
			// C button
			if (InputTranslator.ControllerButtons.C == OpenTK.Input.ButtonState.Released)
			{
				KeyUp(this, new InputEventArgs(Controls[100 + buttonCommands[4]]));
			}

			InputTranslator.Update();

			if (InputTranslator.IsControllerConnected)
			{
				// Brake handle
				if (InputTranslator.BrakeNotch != InputTranslator.PreviousBrakeNotch || loading)
				{
					KeyDown(this, new InputEventArgs(Controls[brakeCommands[(int)InputTranslator.BrakeNotch]]));
					brakeHandleMoved = true;
				}
				// Power handle
				if (InputTranslator.PowerNotch != InputTranslator.PreviousPowerNotch || loading)
				{
					KeyDown(this, new InputEventArgs(Controls[50 + powerCommands[(int)InputTranslator.PowerNotch]]));
					powerHandleMoved = true;
				}
				// Select button
				if (InputTranslator.ControllerButtons.Select == OpenTK.Input.ButtonState.Pressed)
				{
					KeyDown(this, new InputEventArgs(Controls[100 + buttonCommands[0]]));
				}
				// Start button
				if (InputTranslator.ControllerButtons.Start == OpenTK.Input.ButtonState.Pressed)
				{
					KeyDown(this, new InputEventArgs(Controls[100 + buttonCommands[1]]));
				}
				// A button
				if (InputTranslator.ControllerButtons.A == OpenTK.Input.ButtonState.Pressed)
				{
					KeyDown(this, new InputEventArgs(Controls[100 + buttonCommands[2]]));
				}
				// B button
				if (InputTranslator.ControllerButtons.B == OpenTK.Input.ButtonState.Pressed)
				{
					KeyDown(this, new InputEventArgs(Controls[100 + buttonCommands[3]]));
				}
				// C button
				if (InputTranslator.ControllerButtons.C == OpenTK.Input.ButtonState.Pressed)
				{
					KeyDown(this, new InputEventArgs(Controls[100 + buttonCommands[4]]));
				}
			}

			loading = false;
		}

		/// <summary>
		/// A function notifying the plugin about the train's existing status.
		/// </summary>
		/// <param name="data">Data</param>
		public void SetElapseData(ElapseData data)
		{
			currentLanguage = data.CurrentLanguageCode;
		}

		public void SetMaxNotch(int powerNotch, int brakeNotch) { }

		public void SetVehicleSpecs(VehicleSpecs specs)
		{
			vehicleSpecs = specs;
		}

		/// <summary>
		/// Configures the correct mappings for the buttons and notches according to the settings.
		/// </summary>
		internal void ConfigureMappings()
		{
			if (!convertNotches)
			{
				// The notches are not supposed to be converted
				// Brake notches
				if (mapHoldBrake && vehicleSpecs.HasHoldBrake)
				{
					brakeCommands[0] = 0;
					brakeCommands[1] = 100 + (int)Translations.Command.HoldBrake;
					for (int i = 2; i < 10; i++)
					{
						brakeCommands[i] = i - 1;
					}
				}
				else
				{
					for (int i = 0; i < 10; i++)
					{
						brakeCommands[i] = i;
					}
				}
				// Emergency brake, only if the train has 8 notches or less
				if (vehicleSpecs.BrakeNotches <= 8)
				{
					brakeCommands[9] = 100 + (int)Translations.Command.BrakeEmergency;
				}
				// Power notches
				for (int i = 0; i < 6; i++)
				{
					powerCommands[i] = i;
				}
			}
			else
			{
				// The notches are supposed to be converted
				// Brake notches
				if (mapHoldBrake && vehicleSpecs.HasHoldBrake)
				{
					double brakeStep = vehicleSpecs.BrakeNotches / 7.0;
					brakeCommands[0] = 0;
					brakeCommands[1] = 100 + (int)Translations.Command.HoldBrake;
					for (int i = 2; i < 9; i++)
					{
						brakeCommands[i] = (int)Math.Round(brakeStep * (i - 1), MidpointRounding.AwayFromZero);
						if (i > 0 && brakeCommands[i] == 0)
						{
							brakeCommands[i] = 1;
						}
						if (keepMaxMin && i == 2)
						{
							brakeCommands[i] = 1;
						}
						if (keepMaxMin && i == 8)
						{
							brakeCommands[i] = vehicleSpecs.BrakeNotches;
						}
					}
				}
				else
				{
					double brakeStep = vehicleSpecs.BrakeNotches / 8.0;
					for (int i = 0; i < 9; i++)
					{
						brakeCommands[i] = (int)Math.Round(brakeStep * i, MidpointRounding.AwayFromZero);
						if (i > 0 && brakeCommands[i] == 0)
						{
							brakeCommands[i] = 1;
						}
						if (keepMaxMin && i == 1)
						{
							brakeCommands[i] = 1;
						}
						if (keepMaxMin && i == 8)
						{
							brakeCommands[i] = vehicleSpecs.BrakeNotches;
						}
					}
				}
				// Emergency brake
				brakeCommands[9] = 100 + (int)Translations.Command.BrakeEmergency;
				// Power notches
				double powerStep = vehicleSpecs.PowerNotches / 5.0;
				for (int i = 0; i < 6; i++)
				{
					powerCommands[i] = (int)Math.Round(powerStep * i, MidpointRounding.AwayFromZero);
					if (i > 0 && powerCommands[i] == 0)
					{
						powerCommands[i] = 1;
					}
					if (keepMaxMin && i == 1)
					{
						powerCommands[i] = 1;
					}
					if (keepMaxMin && i == 5)
					{
						powerCommands[i] = vehicleSpecs.PowerNotches;
					}
				}
			}
		}

		/// <summary>
		/// Loads the plugin settings from the config file.
		/// </summary>
		internal static void LoadConfig()
		{
			string optionsFolder = OpenBveApi.Path.CombineDirectory(FileSystem.SettingsFolder, "1.5.0");
			if (!System.IO.Directory.Exists(optionsFolder))
			{
				System.IO.Directory.CreateDirectory(optionsFolder);
			}
			string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_denshadego.cfg");
			if (System.IO.File.Exists(configFile))
			{
				// load options
				string[] Lines = System.IO.File.ReadAllLines(configFile, new System.Text.UTF8Encoding());
				string Section = "";
				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i] = Lines[i].Trim(new char[] { });
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
					{
						if (Lines[i].StartsWith("[", StringComparison.Ordinal) &
							Lines[i].EndsWith("]", StringComparison.Ordinal))
						{
							Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim(new char[] { }).ToLowerInvariant();
						}
						else
						{
							int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
							string Key, Value;
							if (j >= 0)
							{
								Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
								Value = Lines[i].Substring(j + 1).TrimStart(new char[] { });
							}
							else
							{
								Key = "";
								Value = Lines[i];
							}
							switch (Section)
							{
								case "general":
									switch (Key)
									{
										case "controller":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													InputTranslator.activeControllerIndex = a;
												}
											}
											break;
									}
									break;
								case "handles":
									switch (Key)
									{
										case "convert_notches":
											convertNotches = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "keep_max_min":
											keepMaxMin = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "map_hold_brake":
											mapHoldBrake = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									}
									break;
								case "buttons":
									switch (Key)
									{
										case "select":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													buttonCommands[0] = a;
												}
											}
											break;
										case "start":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													buttonCommands[1] = a;
												}
											}
											break;
										case "a":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													buttonCommands[2] = a;
												}
											}
											break;
										case "b":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													buttonCommands[3] = a;
												}
											}
											break;
										case "c":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													buttonCommands[4] = a;
												}
											}
											break;
									}
									break;
								case "playstation":
									switch (Key)
									{
										case "hat":
											PSController.UsesHat = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "hat_index":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.hatIndex = a;
												}
											}
											break;
										case "cross":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Cross = a;
												}
											}
											break;
										case "square":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Square = a;
												}
											}
											break;
										case "triangle":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Triangle = a;
												}
											}
											break;
										case "circle":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Circle = a;
												}
											}
											break;
										case "l1":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.L1 = a;
												}
											}
											break;
										case "l2":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.L2 = a;
												}
											}
											break;
										case "r1":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.R1 = a;
												}
											}
											break;
										case "r2":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.R2 = a;
												}
											}
											break;
										case "select":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Select = a;
												}
											}
											break;
										case "start":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Start = a;
												}
											}
											break;
										case "left":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Left = a;
												}
											}
											break;
										case "right":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Right = a;
												}
											}
											break;
									}
									break;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Saves the plugin settings to the config file.
		/// </summary>
		internal static void SaveConfig()
		{
			try
			{
				CultureInfo Culture = CultureInfo.InvariantCulture;
				System.Text.StringBuilder Builder = new System.Text.StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine("; Specific options file for the Densha de GO! controller input plugin");
				Builder.AppendLine();
				Builder.AppendLine("[general]");
				Builder.AppendLine("controller = " + InputTranslator.activeControllerIndex.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[handles]");
				Builder.AppendLine("convert_notches = " + convertNotches.ToString(Culture).ToLower());
				Builder.AppendLine("keep_max_min = " + keepMaxMin.ToString(Culture).ToLower());
				Builder.AppendLine("map_hold_brake = " + mapHoldBrake.ToString(Culture).ToLower());
				Builder.AppendLine();
				Builder.AppendLine("[buttons]");
				Builder.AppendLine("select = " + buttonCommands[0].ToString(Culture));
				Builder.AppendLine("start = " + buttonCommands[1].ToString(Culture));
				Builder.AppendLine("a = " + buttonCommands[2].ToString(Culture));
				Builder.AppendLine("b = " + buttonCommands[3].ToString(Culture));
				Builder.AppendLine("c = " + buttonCommands[4].ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[playstation]");
				Builder.AppendLine("hat = " + PSController.UsesHat.ToString(Culture).ToLower());
				Builder.AppendLine("hat_index = " + PSController.hatIndex.ToString(Culture));
				Builder.AppendLine("cross = " + PSController.ButtonIndex.Cross.ToString(Culture));
				Builder.AppendLine("square = " + PSController.ButtonIndex.Square.ToString(Culture));
				Builder.AppendLine("triangle = " + PSController.ButtonIndex.Triangle.ToString(Culture));
				Builder.AppendLine("circle = " + PSController.ButtonIndex.Circle.ToString(Culture));
				Builder.AppendLine("l1 = " + PSController.ButtonIndex.L1.ToString(Culture));
				Builder.AppendLine("l2 = " + PSController.ButtonIndex.L2.ToString(Culture));
				Builder.AppendLine("r1 = " + PSController.ButtonIndex.R1.ToString(Culture));
				Builder.AppendLine("r2 = " + PSController.ButtonIndex.R2.ToString(Culture));
				Builder.AppendLine("select = " + PSController.ButtonIndex.Select.ToString(Culture));
				Builder.AppendLine("start = " + PSController.ButtonIndex.Start.ToString(Culture));
				Builder.AppendLine("left = " + PSController.ButtonIndex.Left.ToString(Culture));
				Builder.AppendLine("right = " + PSController.ButtonIndex.Right.ToString(Culture));
				string optionsFolder = OpenBveApi.Path.CombineDirectory(FileSystem.SettingsFolder, "1.5.0");
				string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_denshadego.cfg");
				System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + System.Environment.NewLine +
								"Please check you have write permission.");
			}
		}
	}
}
