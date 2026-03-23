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
using OpenTK.Input;

namespace KatoInput
{
	/// <summary>Class representing a KATO EC-1 controller.</summary>
	public class EC1Controller : Controller
	{
		/// <summary>Enumeration representing controller models.</summary>
		private enum ControllerModels
		{
			/// <summary>Unsupported controller</summary>
			Unsupported,
			/// <summary>KATO EC-1</summary>
			EC1,
		};

		/// <summary>Enumeration representing handle notches.</summary>
		[Flags]
		private enum HandleNotchesEnum
		{
			// The controller uses 4 physical buttons to represent handle notches.
			// These do *not* map directly to the simulation

			/// <summary>No buttons are pressed on the controller</summary>
			None = 0,
			/// <summary>The Handle1 button is pressed</summary>
			Handle1 = 1,
			/// <summary>The Handle2 button is pressed</summary>
			Handle2 = 2,
			/// <summary>The Handle3 button is pressed</summary>
			Handle3 = 4,
			/// <summary>The Handle4 button is pressed</summary>
			Handle4 = 8,

			/// <summary>Emergency</summary>
			Emergency = Handle1,
			/// <summary>Brake notch B8</summary>
			B8 = Handle2,
			/// <summary>Brake notch B7</summary>
			B7 = Handle1 | Handle2,
			/// <summary>Brake notch B6</summary>
			B6 = Handle3,
			/// <summary>Brake notch B5</summary>
			B5 = Handle1 | Handle3,
			/// <summary>Brake notch B4</summary>
			B4 = Handle2 | Handle3,
			/// <summary>Brake notch B3</summary>
			B3 = Handle1 | Handle2 | Handle3,
			/// <summary>Brake notch B2</summary>
			B2 = Handle4,
			/// <summary>Brake notch B1</summary>
			B1 = Handle1 | Handle4,
			/// <summary>Neutral</summary>
			N = Handle2 | Handle4,
			/// <summary>Power notch P1</summary>
			P1 = Handle1 | Handle2 | Handle4,
			/// <summary>Power notch P2</summary>
			P2 = Handle3 | Handle4,
			/// <summary>Power notch P3</summary>
			P3 = Handle1 | Handle3 | Handle4,
			/// <summary>Power notch P4</summary>
			P4 = Handle2 | Handle3 | Handle4,
			/// <summary>Power notch P5</summary>
			P5 = Handle1 | Handle2 | Handle3 | Handle4,
		};

		/// <summary>Dictionary storing the mapping of each brake notch.</summary>
		private readonly Dictionary<HandleNotchesEnum, ControllerState.BrakeNotches> brakeNotchMap = new Dictionary<HandleNotchesEnum, ControllerState.BrakeNotches>
		{
			{ HandleNotchesEnum.N, ControllerState.BrakeNotches.Released },
			{ HandleNotchesEnum.B1, ControllerState.BrakeNotches.B1 },
			{ HandleNotchesEnum.B2, ControllerState.BrakeNotches.B2 },
			{ HandleNotchesEnum.B3, ControllerState.BrakeNotches.B3 },
			{ HandleNotchesEnum.B4, ControllerState.BrakeNotches.B4 },
			{ HandleNotchesEnum.B5, ControllerState.BrakeNotches.B5 },
			{ HandleNotchesEnum.B6, ControllerState.BrakeNotches.B6 },
			{ HandleNotchesEnum.B7, ControllerState.BrakeNotches.B7 },
			{ HandleNotchesEnum.B8, ControllerState.BrakeNotches.B8 },
			{ HandleNotchesEnum.Emergency, ControllerState.BrakeNotches.Emergency }
		};

		/// <summary>Dictionary storing the mapping of each power notch.</summary>
		private readonly Dictionary<HandleNotchesEnum, ControllerState.PowerNotches> powerNotchMap = new Dictionary<HandleNotchesEnum, ControllerState.PowerNotches>
		{
			{ HandleNotchesEnum.N, ControllerState.PowerNotches.N },
			{ HandleNotchesEnum.P1, ControllerState.PowerNotches.P1 },
			{ HandleNotchesEnum.P2, ControllerState.PowerNotches.P2 },
			{ HandleNotchesEnum.P3, ControllerState.PowerNotches.P3 },
			{ HandleNotchesEnum.P4, ControllerState.PowerNotches.P4 },
			{ HandleNotchesEnum.P5, ControllerState.PowerNotches.P5 }
		};

		/// <summary>The controller model.</summary>
		private ControllerModels controllerModel;

		/// <summary>The controller index in OpenTK.</summary>
		private int controllerIndex;

		private EC1Controller(Guid guid, string name, int index, ControllerModels model, int brakeNotches, int powerNotches, bool hasReverser)
		{
			Guid = guid;
			Name = name;
			controllerIndex = index;
			controllerModel = model;
			Capabilities = new ControllerCapabilities(brakeNotches, powerNotches, hasReverser);
		}

		/// <summary>Lists the controllers supported by this class.</summary>
		internal static void GetControllers(Dictionary<Guid, Controller> controllerList)
		{
			Dictionary<Guid, Controller> controllers = new Dictionary<Guid, Controller>();

			// Check the first 10 joysticks, should be enough
			for (int i = 0; i < 10; i++)
			{
				Guid guid = Joystick.GetGuid(i);
				string name = Joystick.GetName(i);
				string controllerId = GetControllerId(guid);
				switch (controllerId)
				{
					// KATO EC-1
					case "2e8a:106d":
						// The controller uses buttons 7-10 and axis 2, we need those at minimum
						if (Joystick.GetCapabilities(i).ButtonCount >= 10 && Joystick.GetCapabilities(i).AxisCount >= 2)
						{
							Controller controller = new EC1Controller(guid, name, i, ControllerModels.EC1, 8, 5, true);
							controller.State.IsConnected = Joystick.GetState(i).IsConnected;
							if (!controllerList.ContainsKey(guid))
							{
								controllerList.Add(guid, controller);
							}
						}
						break;
					default:
						continue;
				}
			}
		}

		/// <summary>Updates the state of the controller.</summary>
		internal override void Update()
		{
			JoystickState state = Joystick.GetState(controllerIndex);
			UpdatePreviousState();
			State.IsConnected = state.IsConnected;

			if (State.IsConnected)
			{
				HandleNotchesEnum handleButtons = HandleNotchesEnum.None;
				handleButtons = state.IsButtonDown(9) ? handleButtons | HandleNotchesEnum.Handle1 : handleButtons & ~HandleNotchesEnum.Handle1;
				handleButtons = state.IsButtonDown(8) ? handleButtons | HandleNotchesEnum.Handle2 : handleButtons & ~HandleNotchesEnum.Handle2;
				handleButtons = state.IsButtonDown(7) ? handleButtons | HandleNotchesEnum.Handle3 : handleButtons & ~HandleNotchesEnum.Handle3;
				handleButtons = state.IsButtonDown(6) ? handleButtons | HandleNotchesEnum.Handle4 : handleButtons & ~HandleNotchesEnum.Handle4;

				State.BrakeNotch = brakeNotchMap.ContainsKey(handleButtons) ? brakeNotchMap[handleButtons] : ControllerState.BrakeNotches.Released;
				State.PowerNotch = powerNotchMap.ContainsKey(handleButtons) ? powerNotchMap[handleButtons] : ControllerState.PowerNotches.N;

				float reverser = state.GetAxis(1);
				if (reverser > -0.15 && reverser < 0.15)
				{
					State.ReverserPosition = ControllerState.ReverserPositions.Forward;
				}
				else if (reverser > 0.85)
				{
					State.ReverserPosition = ControllerState.ReverserPositions.Neutral;
				}
				else
				{
					State.ReverserPosition = ControllerState.ReverserPositions.Backward;
				}
			}
		}

	}
}
