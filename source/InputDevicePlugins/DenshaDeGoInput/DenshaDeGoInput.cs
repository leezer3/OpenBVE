//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020-2021, Marc Riera, The OpenBVE Project
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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
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

		internal static HostInterface CurrentHost;

		/// <summary>Whether an issue has been encountered with LibUsb</summary>
		internal static bool LibUsbIssue;

		public InputControl[] Controls
		{
			get; private set;
		}

		internal static FileSystem FileSystem;

		private Config configForm;

		/// <summary>
		/// Whether the input plugin has just started running.
		/// </summary>
		private bool loading = true;

		/// <summary>
		/// Whether the input plugin is running in-game.
		/// </summary>
		internal static bool Ingame;

		/// <summary>Represents a speed limit at a specific track position.</summary>
		struct CompatibilityLimit
		{
			// --- members ---
			/// <summary>The speed limit.</summary>
			internal double Limit;
			/// <summary>The track position.</summary>
			internal double Location;
			// --- constructors ---
			/// <summary>Creates a new compatibility limit.</summary>
			/// <param name="limit">The speed limit.</param>
			/// <param name="location">The track position.</param>
			internal CompatibilityLimit(double limit, double location)
			{
				Limit = limit;
				Location = location;
			}
		}

		/// <summary>A list of track positions and speed limits in the current route.</summary>
		private static List<CompatibilityLimit> trackLimits = new List<CompatibilityLimit>();

		/// <summary>
		/// The specs of the driver's train.
		/// </summary>
		internal VehicleSpecs TrainSpecs = new VehicleSpecs(5, BrakeTypes.AutomaticAirBrake, 8, false, 1);

		/// <summary>
		/// The current train speed in kilometers per hour.
		/// </summary>
		internal static double CurrentTrainSpeed;

		/// <summary>
		/// The current speed limit in kilometers per hour.
		/// </summary>
		internal static double CurrentSpeedLimit;

		/// <summary>
		/// Whether the doors are closed or not.
		/// </summary>
		internal static bool TrainDoorsClosed;

		/// <summary>
		/// Whether the train is in an ATC section or not.
		/// </summary>
		internal static bool ATCSection;

		/// <summary>
		/// Whether the brake handle has been moved.
		/// </summary>
		private bool brakeHandleMoved;

		/// <summary>
		/// Whether the power handle has been moved.
		/// </summary>
		private bool powerHandleMoved;

		/// <summary>
		/// An array with the command indices configured for each brake notch.
		/// </summary>
		private static int[] brakeCommands = new int[10];

		/// <summary>
		/// An array with the command indices configured for each power notch.
		/// </summary>
		private static int[] powerCommands = new int[14];

		/// <summary>
		/// Class for the properties of the buttons.
		/// </summary>
		internal class ButtonProp
		{
			internal int Command;
			internal double Timer;
			internal bool Repeats;
		}

		/// <summary>
		/// An array with the properties for each button.
		/// </summary>
		internal static ButtonProp[] ButtonProperties = new ButtonProp[13];


		/// <summary>
		/// Whether to convert the handle notches to match the driver's train.
		/// </summary>
		internal static bool ConvertNotches;

		/// <summary>
		/// Whether to assign the maximum and minimum notches to P1/P5 and B1/B8.
		/// </summary>
		internal static bool KeepMaxMin;

		/// <summary>
		/// Whether to map the hold brake to B1.
		/// </summary>
		internal static bool MapHoldBrake;

		/// <summary>
		/// Initial delay when repeating a button press.
		/// </summary>
		private static int repeatDelay = 500;

		/// <summary>
		/// Internval for repeating a button press.
		/// </summary>
		private static int repeatInterval = 100;

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
			FileSystem = fileSystem;
			//HACK: In order to avoid meddling with a shipped interface (or making this field public and increasing the mess), let's grab it via reflection
			CurrentHost = (HostInterface)typeof(FileSystem).GetField("currentHost", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(fileSystem);

			if (loading)
			{
				InputTranslator.Load();
			}

			// Start thread for LibUsb-based controllers
			LibUsb.LibUsbThread = new Thread(LibUsb.LibUsbLoop);
			LibUsb.LibUsbThread.Start();

			// Initialize the array of button properties
			for (int i = 0; i < ButtonProperties.Length; i++)
			{
				ButtonProperties[i] = new ButtonProp();
			}

			// Load settings from the config file
			LoadConfig();

			// Create the config form
			configForm = new Config();

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
			LibUsb.LibUsbShouldLoop = false;
			configForm.Dispose();
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
			// Buttons
			for (int i = 0; i < ButtonProperties.Length; i++)
			{
				KeyUp(this, new InputEventArgs(Controls[100 + ButtonProperties[i].Command]));
			}

			InputTranslator.Update();
			if (loading)
			{
				// Configure the mappings on the first frame to fit the controller's features
				ConfigureMappings();
			}

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
				// Buttons
				for (int i = 0; i < ButtonProperties.Length; i++)
				{
					if (InputTranslator.ControllerButtons[i] == OpenTK.Input.ButtonState.Pressed && ButtonProperties[i].Timer <= 0)
					{
						KeyDown(this, new InputEventArgs(Controls[100 + ButtonProperties[i].Command]));
						if (ButtonProperties[i].Repeats)
						{
							ButtonProperties[i].Timer = repeatInterval;
						}
						else
						{
							ButtonProperties[i].Timer = repeatDelay;
							ButtonProperties[i].Repeats = true;
						}
					}
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
			Translations.CurrentLanguageCode = data.CurrentLanguageCode;

			// HACK: The number of stations cannot be zero in-game
			if (data.Stations.Count > 0)
			{
				Ingame = true;
			}

			// Set the current train speed
			CurrentTrainSpeed = data.Vehicle.Speed.KilometersPerHour;

			// Set the current speed limit
			CurrentSpeedLimit = GetCurrentSpeedLimit(data.Vehicle.Location);

			// Button timers
			for (int i = 0; i < ButtonProperties.Length; i++)
			{
				if (InputTranslator.ControllerButtons[i] == OpenTK.Input.ButtonState.Pressed)
				{
					ButtonProperties[i].Timer -= data.ElapsedTime.Milliseconds;
				}
				else
				{
					ButtonProperties[i].Timer = 0;
					ButtonProperties[i].Repeats = false;
				}
			}
		}

		public void SetMaxNotch(int powerNotch, int brakeNotch)
		{
		}

		/// <summary>
		/// A function notifying the plugin about the train's specifications.
		/// </summary>
		/// <param name="specs">The train's specifications.</param>
		public void SetVehicleSpecs(VehicleSpecs specs)
		{
			TrainSpecs = specs;
			ConfigureMappings();
		}

		/// <summary>Is called when the state of the doors changes.</summary>
		/// <param name="oldState">The old state of the doors.</param>
		/// <param name="newState">The new state of the doors.</param>
		public void DoorChange(DoorStates oldState, DoorStates newState)
		{
			TrainDoorsClosed = newState == DoorStates.None;
		}

		/// <summary>Is called when the aspect in the current or in any of the upcoming sections changes, or when passing section boundaries.</summary>
		/// <remarks>The signal array is guaranteed to have at least one element. When accessing elements other than index 0, you must check the bounds of the array first.</remarks>
		public void SetSignal(SignalData[] signal)
		{
		}

		/// <summary>Is called when the train passes a beacon.</summary>
		/// <param name="data">The beacon data.</param>
		public void SetBeacon(BeaconData data)
		{
			switch (data.Type)
			{
				case -16777214:
					// ATC speed limit (.Limit command)
					double limit = (data.Optional & 4095);
					double position = (data.Optional >> 12);
					var item = new CompatibilityLimit(limit, position);
					if (!trackLimits.Contains(item))
					{
						trackLimits.Add(item);
					}
					break;
				case -16777215:
					// ATC track compatibility
					ATCSection = (data.Optional >= 1 && data.Optional <= 3);
					break;
			}
		}

		/// <summary>
		/// Configures the correct mappings for the buttons and notches according to the user settings.
		/// </summary>
		internal void ConfigureMappings()
		{
			int controllerBrakeNotches = InputTranslator.GetControllerBrakeNotches();
			int controllerPowerNotches = InputTranslator.GetControllerPowerNotches();

			if (!ConvertNotches)
			{
				// The notches are not supposed to be converted
				// Brake notches
				if (MapHoldBrake && TrainSpecs.HasHoldBrake)
				{
					brakeCommands[0] = 0;
					brakeCommands[1] = 100 + (int)Translations.Command.HoldBrake;
					for (int i = 2; i < controllerBrakeNotches + 2; i++)
					{
						brakeCommands[i] = i - 1;
					}
				}
				else
				{
					for (int i = 0; i < controllerBrakeNotches + 2; i++)
					{
						brakeCommands[i] = i;
					}
				}
				// Emergency brake, only if the train has the same or less notches than the controller
				if (TrainSpecs.BrakeNotches <= controllerBrakeNotches)
				{
					brakeCommands[(int)InputTranslator.BrakeNotches.Emergency] = 100 + (int)Translations.Command.BrakeEmergency;
				}
				// Power notches
				for (int i = 0; i < controllerPowerNotches; i++)
				{
					powerCommands[i] = i;
				}
			}
			else
			{
				// The notches are supposed to be converted
				// Brake notches
				if (MapHoldBrake && TrainSpecs.HasHoldBrake)
				{
					double brakeStep = (TrainSpecs.BrakeNotches - 1) / (double)(controllerBrakeNotches - 1);
					brakeCommands[0] = 0;
					brakeCommands[1] = 100 + (int)Translations.Command.HoldBrake;
					for (int i = 2; i < controllerBrakeNotches + 1; i++)
					{
						brakeCommands[i] = (int)Math.Round(brakeStep * (i - 1), MidpointRounding.AwayFromZero);
						if (i > 0 && brakeCommands[i] == 0)
						{
							brakeCommands[i] = 1;
						}
						if (KeepMaxMin && i == 2)
						{
							brakeCommands[i] = 1;
						}
						if (KeepMaxMin && i == controllerBrakeNotches)
						{
							brakeCommands[i] = TrainSpecs.BrakeNotches - 1;
						}
					}
				}
				else
				{
					double brakeStep = TrainSpecs.BrakeNotches / (double)controllerBrakeNotches;
					for (int i = 0; i < controllerBrakeNotches + 1; i++)
					{
						brakeCommands[i] = (int)Math.Round(brakeStep * i, MidpointRounding.AwayFromZero);
						if (i > 0 && brakeCommands[i] == 0)
						{
							brakeCommands[i] = 1;
						}
						if (KeepMaxMin && i == 1)
						{
							brakeCommands[i] = 1;
						}
						if (KeepMaxMin && i == controllerBrakeNotches)
						{
							brakeCommands[i] = TrainSpecs.BrakeNotches;
						}
					}
				}
				// Emergency brake
				brakeCommands[(int)InputTranslator.BrakeNotches.Emergency] = 100 + (int)Translations.Command.BrakeEmergency;
				// Power notches
				double powerStep = TrainSpecs.PowerNotches / (double)controllerPowerNotches;
				for (int i = 0; i < controllerPowerNotches + 1; i++)
				{
					powerCommands[i] = (int)Math.Round(powerStep * i, MidpointRounding.AwayFromZero);
					if (i > 0 && powerCommands[i] == 0)
					{
						powerCommands[i] = 1;
					}
					if (KeepMaxMin && i == 1)
					{
						powerCommands[i] = 1;
					}
					if (KeepMaxMin && i == controllerPowerNotches)
					{
						powerCommands[i] = TrainSpecs.PowerNotches;
					}
				}
			}
		}

		/// <summary>
		/// Calculates the current speed limit.
		/// </summary>
		internal double GetCurrentSpeedLimit(double position)
		{
			int pointer = 0;

			if (trackLimits.Count > 0)
			{
				if (trackLimits.Count == 1)
				{
					// Only one limit has been found
					if (trackLimits[0].Location > position)
					{
						// Enforce the limit
						return trackLimits[0].Limit;
					}
					// No limit
					return -1;
				}
				while (pointer > 0 && trackLimits[pointer].Location > position)
				{
					// Detects speed limits when driving or jumping backwards
					pointer--;
				}
				while (pointer < trackLimits.Count - 1 && trackLimits[pointer + 1].Location <= position)
				{
					// Detects speed limits when driving or jumping forwards
					pointer++;
				}
				return trackLimits[pointer].Limit;
			}
			// No limit
			return -1;
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
			// Plugin options
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
										case "guid":
											{
												Guid a;
												if (Guid.TryParse(Value, out a))
												{
													InputTranslator.ActiveControllerGuid = a;
												}
											}
											break;
									}
									break;
								case "handles":
									switch (Key)
									{
										case "convert_notches":
											ConvertNotches = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "keep_max_min":
											KeepMaxMin = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "map_hold_brake":
											MapHoldBrake = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
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
													ButtonProperties[(int)InputTranslator.ControllerButton.Select].Command = a;
												}
											}
											break;
										case "start":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.Start].Command = a;
												}
											}
											break;
										case "a":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.A].Command = a;
												}
											}
											break;
										case "b":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.B].Command = a;
												}
											}
											break;
										case "c":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.C].Command = a;
												}
											}
											break;
										case "d":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.D].Command = a;
												}
											}
											break;
										case "up":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.Up].Command = a;
												}
											}
											break;
										case "down":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.Down].Command = a;
												}
											}
											break;
										case "left":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.Left].Command = a;
												}
											}
											break;
										case "right":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.Right].Command = a;
												}
											}
											break;
										case "pedal":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.Pedal].Command = a;
												}
											}
											break;
										case "ldoor":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.LDoor].Command = a;
												}
											}
											break;
										case "rdoor":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ButtonProperties[(int)InputTranslator.ControllerButton.RDoor].Command = a;
												}
											}
											break;
									}
									break;
								case "classic":
									switch (Key)
									{
										case "hat":
											ClassicController.UsesHat = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "hat_index":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.HatIndex = a;
												}
											}
											break;
										case "axis":
											ClassicController.UsesAxis = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "axis_index":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.AxisIndex = a;
												}
											}
											break;
										case "select":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Select = a;
												}
											}
											break;
										case "start":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Start = a;
												}
											}
											break;
										case "a":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.A = a;
												}
											}
											break;
										case "b":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.B = a;
												}
											}
											break;
										case "c":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.C = a;
												}
											}
											break;
										case "power1":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Power1 = a;
												}
											}
											break;
										case "power2":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Power2 = a;
												}
											}
											break;
										case "power3":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Power3 = a;
												}
											}
											break;
										case "brake1":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Brake1 = a;
												}
											}
											break;
										case "brake2":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Brake2 = a;
												}
											}
											break;
										case "brake3":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Brake3 = a;
												}
											}
											break;
										case "brake4":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													ClassicController.ButtonIndex.Brake4 = a;
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

			// General OpenBVE options
			string openbveConfigFile = OpenBveApi.Path.CombineFile(optionsFolder, "options.cfg");
			if (System.IO.File.Exists(openbveConfigFile))
			{
				// load options
				string[] Lines = System.IO.File.ReadAllLines(openbveConfigFile, new System.Text.UTF8Encoding());
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
								case "controls":
									switch (Key)
									{
										case "keyrepeatdelay":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													repeatDelay = a;
												}
											}
											break;
										case "keyrepeatinterval":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													repeatInterval = a;
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
				Builder.AppendLine("guid = " + InputTranslator.ActiveControllerGuid.ToString());
				Builder.AppendLine();
				Builder.AppendLine("[handles]");
				Builder.AppendLine("convert_notches = " + ConvertNotches.ToString(Culture).ToLower());
				Builder.AppendLine("keep_max_min = " + KeepMaxMin.ToString(Culture).ToLower());
				Builder.AppendLine("map_hold_brake = " + MapHoldBrake.ToString(Culture).ToLower());
				Builder.AppendLine();
				Builder.AppendLine("[buttons]");
				Builder.AppendLine("select = " + ButtonProperties[(int)InputTranslator.ControllerButton.Select].Command.ToString(Culture));
				Builder.AppendLine("start = " + ButtonProperties[(int)InputTranslator.ControllerButton.Start].Command.ToString(Culture));
				Builder.AppendLine("a = " + ButtonProperties[(int)InputTranslator.ControllerButton.A].Command.ToString(Culture));
				Builder.AppendLine("b = " + ButtonProperties[(int)InputTranslator.ControllerButton.B].Command.ToString(Culture));
				Builder.AppendLine("c = " + ButtonProperties[(int)InputTranslator.ControllerButton.C].Command.ToString(Culture));
				Builder.AppendLine("d = " + ButtonProperties[(int)InputTranslator.ControllerButton.D].Command.ToString(Culture));
				Builder.AppendLine("up = " + ButtonProperties[(int)InputTranslator.ControllerButton.Up].Command.ToString(Culture));
				Builder.AppendLine("down = " + ButtonProperties[(int)InputTranslator.ControllerButton.Down].Command.ToString(Culture));
				Builder.AppendLine("left = " + ButtonProperties[(int)InputTranslator.ControllerButton.Left].Command.ToString(Culture));
				Builder.AppendLine("right = " + ButtonProperties[(int)InputTranslator.ControllerButton.Right].Command.ToString(Culture));
				Builder.AppendLine("pedal = " + ButtonProperties[(int)InputTranslator.ControllerButton.Pedal].Command.ToString(Culture));
				Builder.AppendLine("ldoor = " + ButtonProperties[(int)InputTranslator.ControllerButton.LDoor].Command.ToString(Culture));
				Builder.AppendLine("rdoor = " + ButtonProperties[(int)InputTranslator.ControllerButton.RDoor].Command.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[classic]");
				Builder.AppendLine("hat = " + ClassicController.UsesHat.ToString(Culture).ToLower());
				Builder.AppendLine("hat_index = " + ClassicController.HatIndex.ToString(Culture));
				Builder.AppendLine("axis = " + ClassicController.UsesAxis.ToString(Culture).ToLower());
				Builder.AppendLine("axis_index = " + ClassicController.AxisIndex.ToString(Culture));
				Builder.AppendLine("select = " + ClassicController.ButtonIndex.Select.ToString(Culture));
				Builder.AppendLine("start = " + ClassicController.ButtonIndex.Start.ToString(Culture));
				Builder.AppendLine("a = " + ClassicController.ButtonIndex.A.ToString(Culture));
				Builder.AppendLine("b = " + ClassicController.ButtonIndex.B.ToString(Culture));
				Builder.AppendLine("c = " + ClassicController.ButtonIndex.C.ToString(Culture));
				Builder.AppendLine("power1 = " + ClassicController.ButtonIndex.Power1.ToString(Culture));
				Builder.AppendLine("power2 = " + ClassicController.ButtonIndex.Power2.ToString(Culture));
				Builder.AppendLine("power3 = " + ClassicController.ButtonIndex.Power3.ToString(Culture));
				Builder.AppendLine("brake1 = " + ClassicController.ButtonIndex.Brake1.ToString(Culture));
				Builder.AppendLine("brake2 = " + ClassicController.ButtonIndex.Brake2.ToString(Culture));
				Builder.AppendLine("brake3 = " + ClassicController.ButtonIndex.Brake3.ToString(Culture));
				Builder.AppendLine("brake4 = " + ClassicController.ButtonIndex.Brake4.ToString(Culture));

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
