//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Marc Riera, The OpenBVE Project
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
using System.Reflection;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace KatoInput
{
	/// <summary>Input Device Plugin class for controllers by KATO</summary>
	public class KatoInput : ITrainInputDevice
	{
		public event EventHandler<InputEventArgs> KeyDown;
		public event EventHandler<InputEventArgs> KeyUp;

		internal static HostInterface CurrentHost;

		public InputControl[] Controls
		{
			get; private set;
		}

		internal static FileSystem FileSystem;

		/// <summary>The plugin's configuration.</summary>
		private Config config;

		/// <summary>Whether the input plugin has just started running.</summary>
		private bool loading = true;

		/// <summary>Whether the input plugin is running in-game.</summary>
		internal bool Ingame;

		/// <summary>The specs of the driver's train.</summary>
		internal VehicleSpecs TrainSpecs = new VehicleSpecs(5, BrakeTypes.ElectricCommandBrake, 8, false, 1);

		/// <summary>An array with the commands configured for each brake notch.</summary>
		private readonly InputControl[] brakeCommands = new InputControl[10];

		/// <summary>An array with the commands configured for each power notch.</summary>
		private readonly InputControl[] powerCommands = new InputControl[6];

		/// <summary>An array with the commands configured for each reverser notch.</summary>
		private readonly InputControl[] reverserCommands = new InputControl[3];


		/// <summary>The list of recognised controllers.</summary>
		private Dictionary<Guid, Controller> controllers;

		/// <summary>The GUID of the active controller.</summary>
		private Guid activeControllerGuid = Guid.Empty;


		/// <summary>A function call when the Config button is pressed.</summary>
		/// <param name="owner">The owner of the window</param>
		public void Config(IWin32Window owner)
		{
			config.ShowDialog(owner);
		}

		/// <summary>A function called when the plugin is loaded.</summary>
		/// <param name="fileSystem">The instance of FileSytem class</param>
		/// <returns>Whether the plugin has been loaded successfully.</returns>
		public bool Load(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
			//HACK: In order to avoid meddling with a shipped interface (or making this field public and increasing the mess), let's grab it via reflection
			CurrentHost = (HostInterface)typeof(FileSystem).GetField("currentHost", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(fileSystem);

			// Create the config form
			config = new Config();

			// Load the plugin configuration
			config.LoadConfig();

			// Initialize controls
			Controls = new InputControl[brakeCommands.Length + powerCommands.Length + reverserCommands.Length];

			// Initialize the list of controllers
			controllers = new Dictionary<Guid, Controller>();

			return true;
		}

		/// <summary>A function called when the plugin is unloaded.</summary>
		public void Unload()
		{
			config.Dispose();
		}

		/// <summary>A function called on each frame.</summary>
		public void OnUpdateFrame()
		{
			// Release controls 
			foreach (InputControl control in Controls)
			{
				KeyUp(this, new InputEventArgs(control));
			}

			if (!controllers.ContainsKey(activeControllerGuid) || !controllers[activeControllerGuid].State.IsConnected)
			{
				// Try to find an active controller
				FindActiveController();
			}
			else
			{
				// There is an active controller, update the state
				Controller controller = controllers[activeControllerGuid];
				controller.Update();

				// Apply brake handle
				if (controller.State.BrakeNotch != controller.PreviousState.BrakeNotch || loading)
				{
					KeyDown(this, new InputEventArgs(brakeCommands[(int)controller.State.BrakeNotch]));
				}
				// Apply power handle
				if (controller.State.PowerNotch != controller.PreviousState.PowerNotch || loading)
				{
					KeyDown(this, new InputEventArgs(powerCommands[(int)controller.State.PowerNotch]));
				}
				// Apply reverser
				if (controller.State.ReverserPosition != controller.PreviousState.ReverserPosition || loading)
				{
					KeyDown(this, new InputEventArgs(reverserCommands[(int)controller.State.ReverserPosition + 1]));
				}
			}

			loading = false;
		}

		/// <summary>A function notifying the plugin about the train's existing status.</summary>
		/// <param name="data">Data</param>
		public void SetElapseData(ElapseData data)
		{
			Translations.CurrentLanguageCode = data.CurrentLanguageCode;

			// HACK: The number of stations cannot be zero in-game
			if (data.Stations.Count > 0)
			{
				Ingame = true;
			}
		}

		public void SetMaxNotch(int powerNotch, int brakeNotch)
		{
		}

		/// <summary>A function notifying the plugin about the train's specifications.</summary>
		/// <param name="specs">The train's specifications.</param>
		public void SetVehicleSpecs(VehicleSpecs specs)
		{
			TrainSpecs = specs;
			FindActiveController();
			ConfigureMappings();
		}

		/// <summary>Is called when the state of the doors changes.</summary>
		/// <param name="oldState">The old state of the doors.</param>
		/// <param name="newState">The new state of the doors.</param>
		public void DoorChange(DoorStates oldState, DoorStates newState)
		{
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
		}

		/// <summary>Configures the correct mappings for the buttons and notches according to the user settings.</summary>
		internal void ConfigureMappings()
		{
			int controllerBrakeNotches = 0;
			int controllerPowerNotches = 0;
			if (controllers.ContainsKey(activeControllerGuid))
			{
				controllerBrakeNotches = controllers[activeControllerGuid].Capabilities.BrakeNotches;
				controllerPowerNotches = controllers[activeControllerGuid].Capabilities.PowerNotches;
			}

			Config.KatoInputConfiguration configuration = config.Configuration;

			if (!configuration.ConvertNotches)
			{
				// The notches are not supposed to be converted
				// Brake notches
				if (configuration.MapHoldBrake && TrainSpecs.HasHoldBrake)
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
					brakeCommands[(int)ControllerState.BrakeNotches.Emergency].Command = Translations.Command.BrakeEmergency;
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
				if (configuration.MapHoldBrake && TrainSpecs.HasHoldBrake)
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
						if (configuration.KeepMinMax && i == 2)
						{
							brakeCommands[i].Option = 1;
						}
						if (configuration.KeepMinMax && i == controllerBrakeNotches)
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
						if (configuration.KeepMinMax && i == 1)
						{
							brakeCommands[i].Option = 1;
						}
						if (configuration.KeepMinMax && i == controllerBrakeNotches)
						{
							brakeCommands[i].Option = TrainSpecs.BrakeNotches;
						}
					}
				}
				// Emergency brake
				brakeCommands[(int)ControllerState.BrakeNotches.Emergency].Command = Translations.Command.BrakeEmergency;
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
					if (configuration.KeepMinMax && i == 1)
					{
						powerCommands[i].Option = 1;
					}
					if (configuration.KeepMinMax && i == controllerPowerNotches)
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

			for (int i = 0; i < reverserCommands.Length; i++)
			{
				reverserCommands[i].Command = Translations.Command.ReverserAnyPosition;
				reverserCommands[i].Option = i - 1;
			}

			// Pass commands to the main program
			brakeCommands.CopyTo(Controls, 0);
			powerCommands.CopyTo(Controls, brakeCommands.Length);
			reverserCommands.CopyTo(Controls, brakeCommands.Length + powerCommands.Length);
		}

		/// <summary>Looks for the first connected controller that is supported by the plugin.</summary>
		internal void FindActiveController()
		{
			// When loading the plugin, get all the supported controllers
			// The plugin will not try to find new controllers after this
			if (loading)
			{
				EC1Controller.GetControllers(controllers);
			}

			// Set the first connected controller as the active controller
			foreach (KeyValuePair<Guid,Controller> controller in controllers)
			{
				controller.Value.Update();
				if (controller.Value.State.IsConnected)
				{
					activeControllerGuid = controller.Key;
					return;
				}
			}
		}
	}
}
