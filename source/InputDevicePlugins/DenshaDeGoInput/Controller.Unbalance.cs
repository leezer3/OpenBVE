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
	/// Class representing an Unbalance controller
	/// </summary>
	internal class UnbalanceController : Controller
	{
		/// <summary>A cached list of supported connected controllers.</summary>
		private static Dictionary<Guid, Controller> cachedControllers = new Dictionary<Guid, Controller>();

		/// <summary>The OpenTK joystick index for this controller.</summary>
		private int joystickIndex;

		/// <summary>Whether the controller supports a D-Pad using Select+A/B/C/D.</summary>
		private readonly bool comboDpad;

		/// <summary>The byte for each brake notch, from Released to Emergency.</summary>
		private readonly byte[] brakeBytes;

		/// <summary>The byte for each power notch, from Released to maximum.</summary>
		private readonly byte[] powerBytes;

		/// <summary>The index for each button. Follows order in InputTranslator.</summary>
		private readonly int[] buttonIndex;

		/// <summary>
		/// Initializes an Unbalance controller.
		/// </summary>
		internal UnbalanceController(ControllerButtons buttons, int[] buttonIndices, bool combo, byte[] brake, byte[] power)
		{
			ControllerName = string.Empty;
			IsConnected = false;
			RequiresCalibration = false;
			BrakeNotches = brake.Length - 2;
			PowerNotches = power.Length - 1;
			brakeBytes = brake;
			powerBytes = power;
			Buttons = buttons;
			buttonIndex = buttonIndices;
			comboDpad = combo;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal override void ReadInput()
		{
			JoystickState joystick = Joystick.GetState(joystickIndex);
			double brakeAxis = joystick.GetAxis(0);
			double powerAxis = joystick.GetAxis(1);
			for (int i = 0; i < brakeBytes.Length; i++)
			{
				if (brakeAxis >= GetRangeMin(brakeBytes[i]) && brakeAxis <= GetRangeMax(brakeBytes[i]))
				{
					if (brakeBytes.Length == i + 1)
					{
						// Last notch should be Emergency
						InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Emergency;
					}
					else
					{
						// Regular brake notch
						InputTranslator.BrakeNotch = (InputTranslator.BrakeNotches)i;
					}
					break;
				}
			}
			for (int i = 0; i < powerBytes.Length; i++)
			{
				if (powerAxis >= GetRangeMin(powerBytes[i]) && powerAxis <= GetRangeMax(powerBytes[i]))
				{
					InputTranslator.PowerNotch = (InputTranslator.PowerNotches)i;
					break;
				}
			}

			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Select]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Start]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.A]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.B]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.C]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.D]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.LDoor] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.LDoor]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.RDoor] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.RDoor]);

			if (Buttons.HasFlag(ControllerButtons.DPad))
			{
				if (comboDpad)
				{
					// On some controllers, check for Select+A/B/C/D combo
					bool dPadUp = Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Select])) && Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.D]));
					bool dPadDown = Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Select])) && Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.B]));
					bool dPadLeft = Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Select])) && Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.A]));
					bool dPadRight = Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Select])) && Convert.ToBoolean(joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.C]));
					bool dPadAny = dPadUp || dPadDown || dPadLeft || dPadRight;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (ButtonState)(dPadUp ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (ButtonState)(dPadDown ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (ButtonState)(dPadLeft ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (ButtonState)(dPadRight ? 1 : 0);
					// Disable original buttons if necessary
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] ^ (ButtonState)(dPadAny ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] ^ (ButtonState)(dPadLeft ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] ^ (ButtonState)(dPadDown ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] ^ (ButtonState)(dPadRight ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] ^ (ButtonState)(dPadUp ? 1 : 0);
				}
				else
				{
					// On other controllers, read the first hat
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsUp ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsDown ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsLeft ? 1 : 0);
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsRight ? 1 : 0);
				}
			}
		}

		/// <summary>
		/// Gets the list of connected controllers
		/// </summary>
		internal static Dictionary<Guid, Controller> GetControllers()
		{
			for (int i = 0; i < 10; i++)
			{
				Guid guid = Joystick.GetGuid(i);
				string id = GetControllerID(guid);
				string name = Joystick.GetName(i);
				bool comboDpad = name == "TAITO Densha de Go! Plug & Play";

				if (!cachedControllers.ContainsKey(guid))
				{
					// DGC-255/DGOC-44U/P&P
					if (id == "0ae4:0003")
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D;
						if (Joystick.GetCapabilities(i).HatCount > 0 || comboDpad)
						{
							// DGC-255 and P&P have a D-pad
							buttons = buttons | ControllerButtons.DPad;
						}
						int[] buttonIndices = { 4, 5, 1, 0, 2, 3, -1, -1 };
						byte[] brakeBytes = { 0x79, 0x8A, 0x94, 0x9A, 0xA2, 0xA8, 0xAF, 0xB2, 0xB5, 0xB9 };
						byte[] powerBytes = { 0x81, 0x6D, 0x54, 0x3F, 0x21, 0x00 };
						UnbalanceController newcontroller = new UnbalanceController(buttons, buttonIndices, comboDpad, brakeBytes, powerBytes)
						{
							Guid = guid,
							Id = id,
							joystickIndex = i,
							ControllerName = name,
							IsConnected = true
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// DRC-184/DYC-288
					if (id == "0ae4:0008")
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.LDoor | ControllerButtons.RDoor | ControllerButtons.DPad;
						int[] buttonIndices = { 5, 6, 2, 1, 0, -1, 4, 3 };
						byte[] brakeBytes = { };
						byte[] powerBytes = { 0x0, 0x3C, 0x78, 0xB4, 0xF0 };
						UnbalanceController newcontroller = new UnbalanceController(buttons, buttonIndices, comboDpad, brakeBytes, powerBytes)
						{
							Guid = guid,
							Id = id,
							joystickIndex = i,
							ControllerName = name,
							IsConnected = true
						};
						cachedControllers.Add(guid, newcontroller);
					}
				}
				else
				{
					// Cached controller, update it
					((UnbalanceController)cachedControllers[guid]).joystickIndex = i;
				}
			}

			return cachedControllers;
		}

		/// <summary>Gets the minimum value for a notch byte.</summary>
		/// <param name="notch">A notch byte.</param>
		/// <returns>The minimum value for a notch byte.</returns>
		private static double GetRangeMin(byte notch)
		{
			double value = (notch - 1) * (2.0 / 255) - 1;
			return value;
		}

		/// <summary>Gets the maximum value for a notch byte.</summary>
		/// <param name="notch">A notch byte.</param>
		/// <returns>The maximum value for a notch byte.</returns>
		private static double GetRangeMax(byte notch)
		{
			double value = (notch + 1) * (2.0 / 255) - 1;
			return value;
		}
	}
}
