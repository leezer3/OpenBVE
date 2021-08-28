﻿//Simplified BSD License (BSD-2-Clause)
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
	/// Class representing a Zuiki controller
	/// </summary>
	internal class ZuikiController : Controller
	{
		/// <summary>A cached list of supported connected controllers.</summary>
		private static Dictionary<Guid, Controller> cachedControllers = new Dictionary<Guid, Controller>();

		/// <summary>The OpenTK joystick index for this controller.</summary>
		private int joystickIndex;

		/// <summary>The min/max byte for each brake notch, from Released to Emergency. Each notch consists of two bytes.</summary>
		private readonly byte[] brakeBytes;

		/// <summary>The min/max byte for each power notch, from Released to maximum. Each notch consists of two bytes.</summary>
		private readonly byte[] powerBytes;

		/// <summary>The index for each button. Follows order in InputTranslator.</summary>
		private readonly int[] buttonIndex;

		/// <summary>
		/// Initializes a Zuiki controller.
		/// </summary>
		internal ZuikiController(ControllerButtons buttons, int[] buttonIndices, byte[] brake, byte[] power)
		{
			ControllerName = string.Empty;
			IsConnected = false;
			RequiresCalibration = false;
			BrakeNotches = brake.Length / 2 - 2;
			PowerNotches = power.Length / 2 - 1;
			brakeBytes = brake;
			powerBytes = power;
			Buttons = buttons;
			buttonIndex = buttonIndices;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal override void ReadInput()
		{
			JoystickState joystick = Joystick.GetState(joystickIndex);
			double handleAxis = Math.Round(joystick.GetAxis(1),4);
			for (int i = 0; i < brakeBytes.Length; i+=2)
			{
				// Each notch uses two bytes, minimum value and maximum value
				if (handleAxis >= GetAxisValue(brakeBytes[i]) && handleAxis <= GetAxisValue(brakeBytes[i + 1]))
				{
					if (brakeBytes.Length == i + 2)
					{
						// Last notch should be Emergency
						InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Emergency;
					}
					else
					{
						// Regular brake notch
						InputTranslator.BrakeNotch = (InputTranslator.BrakeNotches)(i / 2);
					}
					break;
				}
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Released;
			}
			for (int i = 0; i < powerBytes.Length; i+=2)
			{
				// Each notch uses two bytes, minimum value and maximum value
				if (handleAxis >= GetAxisValue(powerBytes[i]) && handleAxis <= GetAxisValue(powerBytes[i + 1]))
				{
					InputTranslator.PowerNotch = (InputTranslator.PowerNotches)(i / 2);
					break;
				}
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
			}

			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Select]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.Start]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.A]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.B]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.C]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.D]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.LDoor] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.LDoor]);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.RDoor] = joystick.GetButton(buttonIndex[(int)InputTranslator.ControllerButton.RDoor]);

			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsUp ? 1 : 0);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsDown ? 1 : 0);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsLeft ? 1 : 0);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsRight ? 1 : 0);
		}

		/// <summary>
		/// Gets the list of connected controllers
		/// </summary>
		/// <returns>The list of controllers handled by this class.</returns>
		internal static Dictionary<Guid, Controller> GetControllers()
		{
			for (int i = 0; i < 10; i++)
			{
				Guid guid = Joystick.GetGuid(i);
				string id = GetControllerID(guid);
				string name = Joystick.GetName(i);

				if (!cachedControllers.ContainsKey(guid))
				{
					// ZKNS-001
					if (id == "0f0d:00c1")
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.LDoor | ControllerButtons.RDoor | ControllerButtons.DPad;
						int[] buttonIndices = { 8, 9, 0, 1, 2, 3, 4, 5 };
						byte[] brakeBytes = { 0x7F, 0x81, 0x64, 0x66, 0x56, 0x58, 0x48, 0x4A, 0x3B, 0x3D, 0x2D, 0x2F, 0x1F, 0x21, 0x12, 0x14, 0x04, 0x06, 0x0, 0x1 };
						byte[] powerBytes = { 0x7F, 0x81, 0x9E, 0xA0, 0xB6, 0xB8, 0xCD, 0xCF, 0xE5, 0xE7, 0xFE, 0xFF };
						ZuikiController newcontroller = new ZuikiController(buttons, buttonIndices, brakeBytes, powerBytes)
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
					((ZuikiController)cachedControllers[guid]).joystickIndex = i;
					// HACK: IsConnected is broken, we check the capabilities instead to know if the controller is connected or not
					cachedControllers[guid].IsConnected = Joystick.GetCapabilities(i).ButtonCount > 0;
				}
			}

			return cachedControllers;
		}

		/// <summary>Gets the equivalent axis value for a notch byte.</summary>
		/// <param name="notch">A notch byte.</param>
		/// <returns>The axis value for a notch byte.</returns>
		private static double GetAxisValue(byte notch)
		{
			double value = Math.Round(notch * (2.0 / 255) - 1, 4);
			return value;
		}
	}
}
