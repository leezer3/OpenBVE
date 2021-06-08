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
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class which holds generic controller input.
	/// </summary>
	internal static class InputTranslator
	{
		/// <summary>
		/// Dictionary containing all the available supported controllers.
		/// </summary>
		internal static Dictionary<Guid, Controller> Controllers = new Dictionary<Guid, Controller>();

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
			/// <summary>Left door button</summary>
			LDoor = 6,
			/// <summary>Right door button</summary>
			RDoor = 7,
			/// <summary>Up button</summary>
			Up = 8,
			/// <summary>Down button</summary>
			Down = 9,
			/// <summary>Left button</summary>
			Left = 10,
			/// <summary>Right button</summary>
			Right = 11,
			/// <summary>Pedal button</summary>
			Pedal = 12,
		}

		/// <summary>
		/// The current state of the controller's buttons.
		/// </summary>
		internal static ButtonState[] ControllerButtons = new ButtonState[13];

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
		/// The GUID of the active controller.
		/// </summary>
		internal static Guid ActiveControllerGuid = new Guid();

		/// <summary>
		/// Whether the active controller is connected or not.
		/// </summary>
		internal static bool IsControllerConnected;

		/// <summary>
		/// Configures controller-specific settings on load.
		/// </summary>
		internal static void Load()
		{
			Ps2Controller.ConfigureControllers();
		}

		/// <summary>
		/// Gets the number of brake notches, excluding the emergency brake.
		/// </summary>
		/// <returns>The number of brake notches, excluding the emergency brake.</returns>
		internal static int GetControllerBrakeNotches()
		{
			if (Controllers.ContainsKey(ActiveControllerGuid))
			{
				return Controllers[ActiveControllerGuid].BrakeNotches;
			}
			return 0;
		}

		/// <summary>
		/// Gets the number of power notches.
		/// </summary>
		/// <returns>The number of power notches.</returns>
		internal static int GetControllerPowerNotches()
		{
			if (Controllers.ContainsKey(ActiveControllerGuid))
			{
				return Controllers[ActiveControllerGuid].PowerNotches;
			}
			return 0;
		}

		/// <summary>
		/// Refreshes the connected controllers.
		/// </summary>
		public static void RefreshControllers()
		{
			// PlayStation 2 controllers
			foreach (KeyValuePair<Guid, Controller> controller in Ps2Controller.GetControllers())
			{
				if (!Controllers.ContainsKey(controller.Key))
				{
					Controllers.Add(controller.Key, controller.Value);
				}
				else
				{
					Controllers[controller.Key] = controller.Value;
				}
			}
			// Unbalance controllers
			foreach (KeyValuePair<Guid, Controller> controller in UnbalanceController.GetControllers())
			{
				if (!Controllers.ContainsKey(controller.Key))
				{
					Controllers.Add(controller.Key, controller.Value);
				}
				else
				{
					Controllers[controller.Key] = controller.Value;
				}
			}
			// Classic controllers, they need to be added last because we do not use VID/PID
			foreach (KeyValuePair<Guid, Controller> controller in ClassicController.GetControllers())
			{
				if (!Controllers.ContainsKey(controller.Key))
				{
					Controllers.Add(controller.Key, controller.Value);
				}
				else if (controller.GetType() == typeof(ClassicController))
				{
					// Replace controller only if it is a classic controller
					Controllers[controller.Key] = controller.Value;
				}
			}
		}

		/// <summary>
		/// Updates the status of the controller.
		/// </summary>
		public static void Update()
		{
			RefreshControllers();

			//Console.WriteLine(IsControllerConnected);
			if (Controllers.ContainsKey(ActiveControllerGuid) && Controllers[ActiveControllerGuid].IsConnected)
			{
				// The active controller is connected, get input
				IsControllerConnected = true;
				GetInput();
			}
			else
			{
				IsControllerConnected = false;
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

			Controllers[ActiveControllerGuid].ReadInput();
		}
	}
}
