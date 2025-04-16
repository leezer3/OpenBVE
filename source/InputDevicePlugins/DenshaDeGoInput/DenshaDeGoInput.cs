//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020-2024, Marc Riera, The OpenBVE Project
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
			internal readonly double Limit;
			/// <summary>The track position.</summary>
			internal readonly double Location;
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
		private static readonly List<CompatibilityLimit> trackLimits = new List<CompatibilityLimit>();

		/// <summary>
		/// The specs of the driver's train.
		/// </summary>
		internal VehicleSpecs TrainSpecs = new VehicleSpecs(5, BrakeTypes.ElectricCommandBrake, 8, false, 1);

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
		/// Whether the reverser has been moved.
		/// </summary>
		private bool reverserMoved;

		/// <summary>
		/// An array with the commands configured for each brake notch.
		/// </summary>
		private static readonly InputControl[] brakeCommands = new InputControl[10];

		/// <summary>
		/// An array with the commands configured for each power notch.
		/// </summary>
		private static readonly InputControl[] powerCommands = new InputControl[14];

		/// <summary>
		/// An array with the commands configured for each reverser notch.
		/// </summary>
		private static readonly InputControl[] reverserCommands = new InputControl[3];

		/// <summary>
		/// An array with the commands configured for each button.
		/// </summary>
		internal static InputControl[] ButtonCommands = new InputControl[15];


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

			// Load settings from the config file
			LoadConfig();

			// Create the config form
			configForm = new Config();

			// Initialize controls
			Controls = new InputControl[brakeCommands.Length + powerCommands.Length + reverserCommands.Length + ButtonCommands.Length];

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
			// Store previous state of the buttons
			OpenTK.Input.ButtonState[] previousButtonState = (OpenTK.Input.ButtonState[])InputTranslator.ControllerButtons.Clone();

			// Brake handle (release)
			if (brakeHandleMoved)
			{
				KeyUp(this, new InputEventArgs(brakeCommands[(int)InputTranslator.BrakeNotch]));
				brakeHandleMoved = false;
			}
			// Power handle (release)
			if (powerHandleMoved)
			{
				KeyUp(this, new InputEventArgs(powerCommands[(int)InputTranslator.PowerNotch]));
				powerHandleMoved = false;
			}
			// Reverser (release)
			if (reverserMoved)
			{
				KeyUp(this, new InputEventArgs(reverserCommands[(int)InputTranslator.ReverserPosition + 1]));
				reverserMoved = false;
			}

			// Update input from controller
			InputTranslator.Update();

			// Buttons (release)
			for (int i = 0; i < ButtonCommands.Length; i++)
			{
				if (InputTranslator.ControllerButtons[i] == OpenTK.Input.ButtonState.Released && previousButtonState[i] == OpenTK.Input.ButtonState.Pressed)
				{
					KeyUp(this, new InputEventArgs(ButtonCommands[i]));
				}
			}

			if (InputTranslator.IsControllerConnected)
			{
				// Brake handle (apply)
				if (InputTranslator.BrakeNotch != InputTranslator.PreviousBrakeNotch || loading)
				{
					KeyDown(this, new InputEventArgs(brakeCommands[(int)InputTranslator.BrakeNotch]));
					brakeHandleMoved = true;
				}
				// Power handle (apply)
				if (InputTranslator.PowerNotch != InputTranslator.PreviousPowerNotch || loading)
				{
					KeyDown(this, new InputEventArgs(powerCommands[(int)InputTranslator.PowerNotch]));
					powerHandleMoved = true;
				}
				// Reverser (apply)
				if (InputTranslator.ReverserPosition != InputTranslator.PreviousReverserPosition || loading)
				{
					KeyDown(this, new InputEventArgs(reverserCommands[(int)InputTranslator.ReverserPosition + 1]));
					reverserMoved = true;
				}
				// Buttons (apply)
				for (int i = 0; i < ButtonCommands.Length; i++)
				{
					if (InputTranslator.ControllerButtons[i] == OpenTK.Input.ButtonState.Pressed && previousButtonState[i] == OpenTK.Input.ButtonState.Released)
					{
						KeyDown(this, new InputEventArgs(ButtonCommands[i]));
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
			InputTranslator.Update();
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
					brakeCommands[0].Command = Translations.Command.BrakeAnyNotch;
					brakeCommands[0].Option = 0;
					brakeCommands[1].Command = Translations.Command.HoldBrake;
					for (int i = 2; i <= controllerBrakeNotches + 1; i++)
					{
						brakeCommands[i].Command = Translations.Command.BrakeAnyNotch;
						brakeCommands[i].Option = i - 1;
					}
				}
				else
				{
					for (int i = 0; i <= controllerBrakeNotches + 1; i++)
					{
						brakeCommands[i].Command = Translations.Command.BrakeAnyNotch;
						brakeCommands[i].Option = i;
					}
				}
				// Emergency brake, only if the train has the same or less notches than the controller
				if (TrainSpecs.BrakeNotches <= controllerBrakeNotches)
				{
					brakeCommands[(int)InputTranslator.BrakeNotches.Emergency].Command = Translations.Command.BrakeEmergency;
				}
				// Power notches
				for (int i = 0; i <= controllerPowerNotches; i++)
				{
					powerCommands[i].Command = Translations.Command.PowerAnyNotch;
					powerCommands[i].Option = i;
				}
			}
			else
			{
				// The notches are supposed to be converted
				// Brake notches
				if (MapHoldBrake && TrainSpecs.HasHoldBrake)
				{
					double brakeStep = (TrainSpecs.BrakeNotches - 1) / (double)(controllerBrakeNotches - 1);
					brakeCommands[0].Command = Translations.Command.BrakeAnyNotch;
					brakeCommands[0].Option = 0;
					brakeCommands[1].Command = Translations.Command.HoldBrake;
					for (int i = 2; i < controllerBrakeNotches + 1; i++)
					{
						brakeCommands[i].Command = Translations.Command.BrakeAnyNotch;
						brakeCommands[i].Option = (int)Math.Round(brakeStep * (i - 1), MidpointRounding.AwayFromZero);
						if (i > 0 && brakeCommands[i].Option == 0)
						{
							brakeCommands[i].Option = 1;
						}
						if (KeepMaxMin && i == 2)
						{
							brakeCommands[i].Option = 1;
						}
						if (KeepMaxMin && i == controllerBrakeNotches)
						{
							brakeCommands[i].Option = TrainSpecs.BrakeNotches - 1;
						}
					}
				}
				else
				{
					double brakeStep = TrainSpecs.BrakeNotches / (double)controllerBrakeNotches;
					for (int i = 0; i < controllerBrakeNotches + 1; i++)
					{
						brakeCommands[i].Command = Translations.Command.BrakeAnyNotch;
						brakeCommands[i].Option = (int)Math.Round(brakeStep * i, MidpointRounding.AwayFromZero);
						if (i > 0 && brakeCommands[i].Option == 0)
						{
							brakeCommands[i].Option = 1;
						}
						if (KeepMaxMin && i == 1)
						{
							brakeCommands[i].Option = 1;
						}
						if (KeepMaxMin && i == controllerBrakeNotches)
						{
							brakeCommands[i].Option = TrainSpecs.BrakeNotches;
						}
					}
				}
				// Emergency brake
				brakeCommands[(int)InputTranslator.BrakeNotches.Emergency].Command = Translations.Command.BrakeEmergency;
				// Power notches
				double powerStep = TrainSpecs.PowerNotches / (double)controllerPowerNotches;
				for (int i = 0; i < controllerPowerNotches + 1; i++)
				{
					powerCommands[i].Command = Translations.Command.PowerAnyNotch;
					powerCommands[i].Option = (int)Math.Round(powerStep * i, MidpointRounding.AwayFromZero);
					if (i > 0 && powerCommands[i].Option == 0)
					{
						powerCommands[i].Option = 1;
					}
					if (KeepMaxMin && i == 1)
					{
						powerCommands[i].Option = 1;
					}
					if (KeepMaxMin && i == controllerPowerNotches)
					{
						powerCommands[i].Option = TrainSpecs.PowerNotches;
					}
				}
			}

			if (TrainSpecs.BrakeType == BrakeTypes.AutomaticAirBrake)
			{
				// Trains with an air brake are mapped differently
				double brakeStep = 3 / (double)(controllerBrakeNotches);
				for (int i = 1; i < controllerBrakeNotches + 1; i++)
				{
					brakeCommands[i].Command = Translations.Command.BrakeAnyNotch;
					int notch = ((int)Math.Round(brakeStep * i, MidpointRounding.AwayFromZero) - 1);
					brakeCommands[i].Option = notch >= 0 ? notch : 0;
				}
			}

			for (int i = 0; i < 3; i++)
			{
				reverserCommands[i].Command = Translations.Command.ReverserAnyPosition;
				reverserCommands[i].Option = i - 1;
			}

			// Pass commands to the main program
			brakeCommands.CopyTo(Controls, 0);
			powerCommands.CopyTo(Controls, brakeCommands.Length);
			reverserCommands.CopyTo(Controls, brakeCommands.Length + powerCommands.Length);
			ButtonCommands.CopyTo(Controls, brakeCommands.Length + powerCommands.Length + reverserCommands.Length);
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
								Value = Lines[i].Substring(j + 1).TrimStart();
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
												if (Guid.TryParse(Value, out Guid a))
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
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Select].Command = parsedCommand;
												}
											}
											break;
										case "start":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Start].Command = parsedCommand;
												}
											}
											break;
										case "a":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.A].Command = parsedCommand;
												}
											}
											break;
										case "b":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.B].Command = parsedCommand;
												}
											}
											break;
										case "c":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.C].Command = parsedCommand;
												}
											}
											break;
										case "d":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.D].Command = parsedCommand;
												}
											}
											break;
										case "up":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Up].Command = parsedCommand;
												}
											}
											break;
										case "down":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Down].Command = parsedCommand;
												}
											}
											break;
										case "left":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Left].Command = parsedCommand;
												}
											}
											break;
										case "right":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Right].Command = parsedCommand;
												}
											}
											break;
										case "pedal":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.Pedal].Command = parsedCommand;
												}
											}
											break;
										case "ldoor":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.LDoor].Command = parsedCommand;
												}
											}
											break;
										case "rdoor":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.RDoor].Command = parsedCommand;
												}
											}
											break;
										case "ats":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.ATS].Command = parsedCommand;
												}
											}
											break;
										case "a2":
											{
												if (Enum.TryParse(Value.Replace("_", string.Empty), true, out Translations.Command parsedCommand))
												{
													ButtonCommands[(int)InputTranslator.ControllerButton.A2].Command = parsedCommand;
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
												if (int.TryParse(Value, out int a))
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
												if (int.TryParse(Value, out int a))
												{
													ClassicController.AxisIndex = a;
												}
											}
											break;
										case "select":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Select = a;
												}
											}
											break;
										case "start":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Start = a;
												}
											}
											break;
										case "a":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.A = a;
												}
											}
											break;
										case "b":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.B = a;
												}
											}
											break;
										case "c":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.C = a;
												}
											}
											break;
										case "power1":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Power1 = a;
												}
											}
											break;
										case "power2":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Power2 = a;
												}
											}
											break;
										case "power3":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Power3 = a;
												}
											}
											break;
										case "brake1":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Brake1 = a;
												}
											}
											break;
										case "brake2":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Brake2 = a;
												}
											}
											break;
										case "brake3":
											{
												if (int.TryParse(Value, out int a))
												{
													ClassicController.ButtonIndex.Brake3 = a;
												}
											}
											break;
										case "brake4":
											{
												if (int.TryParse(Value, out int a))
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
				for (int i = 0; i < ButtonCommands.Length; i++)
				{
					if (ButtonCommands[i].Command != Translations.Command.None)
					{
						Builder.AppendLine(((InputTranslator.ControllerButton)i).ToString().ToLower() + " = " + Translations.CommandInfos.TryGetInfo(ButtonCommands[i].Command).Name);
					}
				}
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
