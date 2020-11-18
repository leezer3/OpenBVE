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
			/// <summary>Unbalance USB controller for PC</summary>
			Unbalance = 2,
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
		}

		/// <summary>
		/// An array with the state of the controller's buttons.
		/// </summary>
		internal static ButtonState[] ControllerButtons = new ButtonState[11];

		/// <summary>
		/// A dictionary containing GUID/index pairs for controllers.
		/// </summary>
		internal static Dictionary<Guid,int> ConnectedControllers = new Dictionary<Guid, int>();

		/// <summary>
		/// The GUID of the active controller.
		/// </summary>
		internal static Guid activeControllerGuid = new Guid();

		/// <summary>
		/// Whether a supported controller is connected or not.
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
		internal static ControllerModels GetControllerModel(Guid guid, JoystickCapabilities capabilities)
		{
			string id = GetControllerID(guid);

			if (ControllerUnbalance.IsCompatibleController(id, capabilities))
			{
				// The controller is a USB controller by Unbalance
				return ControllerModels.Unbalance;
			}
			if (ControllerClassic.IsCompatibleController(capabilities))
			{
				// The controller is a classic console controller
				return ControllerModels.Classic;
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
					}
				}
				else
				{
					// Update the controller index
					ConnectedControllers[guid] = i;
				}
			}
		}

		/// <summary>
		/// Updates the status of the controller.
		/// </summary>
		public static void Update()
		{
			RefreshControllers();

			if (ConnectedControllers.ContainsKey(activeControllerGuid))
			{
				if (ControllerModel == ControllerModels.Unknown)
				{
					ControllerModel = GetControllerModel(activeControllerGuid, Joystick.GetCapabilities(ConnectedControllers[activeControllerGuid]));
				}
				IsControllerConnected = Joystick.GetState(ConnectedControllers[activeControllerGuid]).IsConnected;
				if (IsControllerConnected)
				{
					// A valid controller is connected, get input
					GetInput();
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
					ControllerClassic.ReadInput(Joystick.GetState(ConnectedControllers[activeControllerGuid]));
					return;
				case ControllerModels.Unbalance:
					ControllerUnbalance.ReadInput(Joystick.GetState(ConnectedControllers[activeControllerGuid]));
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
				for (int i = 0; i < Joystick.GetCapabilities(ConnectedControllers[activeControllerGuid]).ButtonCount; i++)
				{
					buttonsState.Add(Joystick.GetState(ConnectedControllers[activeControllerGuid]).GetButton(i));
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
				for (int i = 0; i < Joystick.GetCapabilities(ConnectedControllers[activeControllerGuid]).ButtonCount; i++)
				{
					hatPositions.Add(Joystick.GetState(ConnectedControllers[activeControllerGuid]).GetHat((JoystickHat)i).Position);
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
