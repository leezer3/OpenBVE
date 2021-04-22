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
using System.Collections.Generic;
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class which holds generic controller input.
	/// </summary>
	internal static class InputTranslator
	{
		/// <summary>
		/// Enumeration representing controller models.
		/// </summary>
		internal enum ControllerModels
		{
			/// <summary>Unsupported controller</summary>
			Unsupported = -1,
			/// <summary>Unknown controller</summary>
			Unknown = 0,
			/// <summary>Classic non-USB console controller</summary>
			Classic = 1,
			/// <summary>Unbalance standard USB controller for PC</summary>
			UnbalanceStandard = 2,
			/// <summary>Unbalance Ryojōhen USB controller for PC</summary>
			UnbalanceRyojouhen = 3,
			/// <summary>Type II USB controller for PlayStation 2</summary>
			Ps2Type2 = 4,
			/// <summary>Shinkansen USB controller for PlayStation 2</summary>
			Ps2Shinkansen = 5,
			/// <summary>Ryojōhen USB controller for PlayStation 2</summary>
			Ps2Ryojouhen = 6,
		};

		/// <summary>
		/// Enumeration representing brake notches.
		/// </summary>
		internal enum BrakeNotches
		{
			/// <summary>Brakes are released</summary>
			Released = 0,
			/// <summary>Brake Notch B1</summary>
			B1 = 1,
			/// <summary>Brake Notch B2</summary>
			B2 = 2,
			/// <summary>Brake Notch B3</summary>
			B3 = 3,
			/// <summary>Brake Notch B4</summary>
			B4 = 4,
			/// <summary>Brake Notch B5</summary>
			B5 = 5,
			/// <summary>Brake Notch B6</summary>
			B6 = 6,
			/// <summary>Brake Notch B7</summary>
			B7 = 7,
			/// <summary>Brake Notch B8</summary>
			B8 = 8,
			/// <summary>Emergency</summary>
			Emergency = 9,
		};

		/// <summary>
		/// Enumeration representing power notches.
		/// </summary>
		internal enum PowerNotches
		{
			/// <summary>Power is in N</summary>
			N = 0,
			/// <summary>Power notch P1</summary>
			P1 = 1,
			/// <summary>Power notch P2</summary>
			P2 = 2,
			/// <summary>Power notch P3</summary>
			P3 = 3,
			/// <summary>Power notch P4</summary>
			P4 = 4,
			/// <summary>Power notch P5</summary>
			P5 = 5,
			/// <summary>Power notch P6</summary>
			P6 = 6,
			/// <summary>Power notch P7</summary>
			P7 = 7,
			/// <summary>Power notch P8</summary>
			P8 = 8,
			/// <summary>Power notch P9</summary>
			P9 = 9,
			/// <summary>Power notch P10</summary>
			P10 = 10,
			/// <summary>Power notch P11</summary>
			P11 = 11,
			/// <summary>Power notch P12</summary>
			P12 = 12,
			/// <summary>Power notch P13</summary>
			P13 = 13,
		};

		/// <summary>
		/// Enumeration representing controller buttons.
		/// </summary>
		internal enum ControllerButton
		{
			/// <summary>Select button</summary>
			Select = 0,
			/// <summary>Start button</summary>
			Start = 1,
			/// <summary>A button</summary>
			A = 2,
			/// <summary>B button</summary>
			B = 3,
			/// <summary>C button</summary>
			C = 4,
			/// <summary>D button</summary>
			D = 5,
			/// <summary>Up button</summary>
			Up = 6,
			/// <summary>Down button</summary>
			Down = 7,
			/// <summary>Left button</summary>
			Left = 8,
			/// <summary>Right button</summary>
			Right = 9,
			/// <summary>Pedal button</summary>
			Pedal = 10,
			/// <summary>Left door button</summary>
			LDoor = 11,
			/// <summary>Right door button</summary>
			RDoor = 12,
		}

		/// <summary>
		/// An array with the state of the controller's buttons.
		/// </summary>
		internal static ButtonState[] ControllerButtons = new ButtonState[13];

		/// <summary>
		/// A dictionary containing GUID/index pairs for controllers.
		/// </summary>
		internal static Dictionary<Guid,int> ConnectedControllers = new Dictionary<Guid, int>();

		/// <summary>
		/// A dictionary containing GUID/model pairs for controllers.
		/// </summary>
		internal static Dictionary<Guid, ControllerModels> ConnectedModels = new Dictionary<Guid, ControllerModels>();

		/// <summary>
		/// The GUID of the active controller.
		/// </summary>
		internal static Guid ActiveControllerGuid = new Guid();

		/// <summary>
		/// Whether the active controller is connected or not.
		/// </summary>
		internal static bool IsControllerConnected;

		/// <summary>
		/// The controller model.
		/// </summary>
		internal static ControllerModels ControllerModel;

		/// <summary>
		/// The current brake notch reported by the controller.
		/// </summary>
		internal static BrakeNotches BrakeNotch;

		/// <summary>
		/// The current power notch reported by the controller.
		/// </summary>
		internal static PowerNotches PowerNotch;

		/// <summary>
		/// The previous brake notch reported by the controller.
		/// </summary>
		internal static BrakeNotches PreviousBrakeNotch;

		/// <summary>
		/// The previous power notch reported by the controller.
		/// </summary>
		internal static PowerNotches PreviousPowerNotch;

		/// <summary>
		/// Gets the controller model.
		/// </summary>
		/// <param name="guid">The GUID of the joystick.</param>
		/// <param name="capabilities">The capabilities of the joystick.</param>
		/// <returns>The controller model.</returns>
		internal static ControllerModels GetControllerModel(Guid guid, JoystickCapabilities capabilities)
		{
			string id = GetControllerID(guid);
			ControllerModels model;

			model = ControllerPs2.GetControllerModel(id);
			if (model != ControllerModels.Unsupported)
			{
				// The controller is a PlayStation 2 USB controller
				return model;
			}
			model = ControllerUnbalance.GetControllerModel(id, capabilities);
			if (model != ControllerModels.Unsupported)
			{
				// The controller is a USB controller by Unbalance
				return model;
			}
			model = ControllerClassic.GetControllerModel(capabilities);
			if (model != ControllerModels.Unsupported)
			{
				// The controller is a classic console controller
				return model;
			}
			// Unsupported controller
			return ControllerModels.Unsupported;
		}

		/// <summary>
		/// Gets a string representing a controller's vendor and product ID.
		/// </summary>
		/// <param name="guid">The GUID of the joystick.</param>
		/// <returns>String representing the controller's vendor and product ID.</returns>
		internal static string GetControllerID(Guid guid)
		{
			string id = guid.ToString("N");
			// OpenTK joysticks have a GUID which contains the vendor and product ID.
			id = id.Substring(10,2)+id.Substring(8,2)+":"+id.Substring(18,2)+id.Substring(16,2);
			return id;
		}

		/// <summary>
		/// Gets the number of brake notches, excluding the emergency brake.
		/// </summary>
		/// <returns>The number of brake notches, excluding the emergency brake.</returns>
		internal static int GetControllerBrakeNotches()
		{
			switch (ControllerModel)
			{
				case ControllerModels.Classic:
					return ControllerClassic.ControllerBrakeNotches;
				case ControllerModels.UnbalanceStandard:
				case ControllerModels.UnbalanceRyojouhen:
					return ControllerUnbalance.ControllerBrakeNotches;
				case ControllerModels.Ps2Type2:
				case ControllerModels.Ps2Shinkansen:
				case ControllerModels.Ps2Ryojouhen:
					return ControllerPs2.ControllerBrakeNotches;
			}
			return 0;
		}

		/// <summary>
		/// Gets the number of power notches.
		/// </summary>
		/// <returns>The number of power notches.</returns>
		internal static int GetControllerPowerNotches()
		{
			switch (ControllerModel)
			{
				case ControllerModels.Classic:
					return ControllerClassic.ControllerPowerNotches;
				case ControllerModels.UnbalanceStandard:
				case ControllerModels.UnbalanceRyojouhen:
					return ControllerUnbalance.ControllerPowerNotches;
				case ControllerModels.Ps2Type2:
				case ControllerModels.Ps2Shinkansen:
				case ControllerModels.Ps2Ryojouhen:
					return ControllerPs2.ControllerPowerNotches;
			}
			return 0;
		}

		/// <summary>
		/// Refreshes the connected controllers.
		/// </summary>
		public static void RefreshControllers()
		{
			for (int i = 0; i < 10; i++)
			{
				Guid guid = Joystick.GetGuid(i);
				if (!ConnectedControllers.ContainsKey(guid))
				{
					// New controller
					JoystickCapabilities capabilities = Joystick.GetCapabilities(i);
					ControllerModels model = GetControllerModel(guid, capabilities);
					if (Joystick.GetState(i).IsConnected && model != ControllerModels.Unsupported)
					{
						ConnectedControllers.Add(guid, i);
						ConnectedModels.Add(guid, model);
					}
				}
				else
				{
					// Update the controller index
					ConnectedControllers[guid] = i;
				}
			}
			
			foreach (KeyValuePair<Guid, int> controller in ControllerPs2.FindControllers())
			{
				if (!ConnectedControllers.ContainsKey(controller.Key))
				{
					ControllerModels model = GetControllerModel(controller.Key, new JoystickCapabilities());
					ConnectedControllers.Add(controller.Key, controller.Value);
					ConnectedModels.Add(controller.Key, model);
				}
			}
		}

		/// <summary>
		/// Gets the name of a controller by GUID.
		/// </summary>
		/// <returns>The name of the controller.</returns>
		public static string GetControllerName(Guid guid)
		{
			if (guid.ToString().Substring(0,8) == "ffffffff")
			{
				// PS2 controller, handled via LibUsbDotNet instead of OpenTK
				return ControllerPs2.GetControllerName(GetControllerID(guid));
			}
			else
			{
				int index = ConnectedControllers[guid];
				return Joystick.GetName(index);
			}
		}

		/// <summary>
		/// Does unloading tasks for certain controllers.
		/// </summary>
		public static void Unload()
		{
			if (ControllerModel == ControllerModels.Ps2Type2 || ControllerModel == ControllerModels.Ps2Shinkansen || ControllerModel == ControllerModels.Ps2Ryojouhen)
			{
				ControllerPs2.Unload();
			}
		}

		/// <summary>
		/// Updates the status of the controller.
		/// </summary>
		public static void Update()
		{
			RefreshControllers();

			if (ConnectedControllers.ContainsKey(ActiveControllerGuid))
			{
				ControllerModel = ConnectedModels[ActiveControllerGuid];
				if (ControllerModel == ControllerModels.Ps2Type2 || ControllerModel == ControllerModels.Ps2Shinkansen || ControllerModel == ControllerModels.Ps2Ryojouhen)
				{
					string id = GetControllerID(ActiveControllerGuid);
					IsControllerConnected = ControllerPs2.IsControlledConnected(id);
				}
				else
				{
					IsControllerConnected = Joystick.GetState(ConnectedControllers[ActiveControllerGuid]).IsConnected;
				}
				if (IsControllerConnected)
				{
					// The active controller is connected, get input
					GetInput();
				}
				else
				{
					// The active controller is no longer connected, remove it
					ConnectedControllers.Remove(ActiveControllerGuid);
					ConnectedModels.Remove(ActiveControllerGuid);
				}
			}
		}

		/// <summary>
		/// Gets the input from the controller.
		/// </summary>
		internal static void GetInput()
		{
			// Store the current notches so we can later tell if they have been moved
			PreviousBrakeNotch = BrakeNotch;
			PreviousPowerNotch = PowerNotch;

			// Read the input from the controller according to the type
			switch (ControllerModel)
			{
				case ControllerModels.Classic:
					ControllerClassic.ReadInput(Joystick.GetState(ConnectedControllers[ActiveControllerGuid]));
					return;
				case ControllerModels.UnbalanceStandard:
				case ControllerModels.UnbalanceRyojouhen:
					ControllerUnbalance.ReadInput(Joystick.GetState(ConnectedControllers[ActiveControllerGuid]));
					return;
				case ControllerModels.Ps2Type2:
				case ControllerModels.Ps2Shinkansen:
				case ControllerModels.Ps2Ryojouhen:
					ControllerPs2.ReadInput();
					return;
			}
		}

		/// <summary>
		/// Gets the state of the buttons of the current controller
		/// </summary>
		/// <returns>State of the buttons of the current controller</returns>
		internal static List<ButtonState> GetButtonsState()
		{
			List<ButtonState> buttonsState = new List<ButtonState>();

			if (IsControllerConnected)
			{
				for (int i = 0; i < Joystick.GetCapabilities(ConnectedControllers[ActiveControllerGuid]).ButtonCount; i++)
				{
					buttonsState.Add(Joystick.GetState(ConnectedControllers[ActiveControllerGuid]).GetButton(i));
				}
			}
			return buttonsState;
		}

		/// <summary>
		/// Gets the position of the hats of the current controller
		/// </summary>
		/// <returns>Position of the hats of the current controller</returns>
		internal static List<HatPosition> GetHatPositions()
		{
			List<HatPosition> hatPositions = new List<HatPosition>();

			if (IsControllerConnected)
			{
				for (int i = 0; i < Joystick.GetCapabilities(ConnectedControllers[ActiveControllerGuid]).ButtonCount; i++)
				{
					hatPositions.Add(Joystick.GetState(ConnectedControllers[ActiveControllerGuid]).GetHat((JoystickHat)i).Position);
				}
			}
			return hatPositions;
		}

		/// <summary>
		/// Compares two button states to find the index of the button that has been pressed.
		/// </summary>
		/// <param name="previousState">The previous state of the buttons.</param>
		/// <param name="newState">The new state of the buttons.</param>
		/// <param name="ignored">The list of ignored buttons.</param>
		/// <returns>Index of the button that has been pressed.</returns>
		internal static int GetDifferentPressedIndex(List<ButtonState> previousState, List<ButtonState> newState, List<int> ignored)
		{
			for (int i = 0; i < newState.Count; i++)
			{
				if (!ignored.Contains(i) && newState[i] != previousState[i])
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Compares two hat states to find the index of the hat that has changed.
		/// </summary>
		/// <param name="previousPosition">The previous position of the hat.</param>
		/// <param name="newPosition">The new position of the hat.</param>
		/// <returns>Index of the hat that has changed.</returns>
		internal static int GetChangedHat(List<HatPosition> previousPosition, List<HatPosition> newPosition)
		{
			for (int i = 0; i < newPosition.Count; i++)
			{
				if (newPosition[i] != previousPosition[i])
					return i;
			}
			return -1;
		}

	}
}
