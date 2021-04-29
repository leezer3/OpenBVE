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

using System.Collections.Generic;
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class for Densha de GO! controllers for PC by Unbalance.
	/// </summary>
	internal static class ControllerUnbalance
	{
		/// <summary>
		/// The number of brake notches, excluding the emergency brake.
		/// </summary>
		internal static int ControllerBrakeNotches = 8;

		/// <summary>
		/// The number of power notches.
		/// </summary>
		internal static int ControllerPowerNotches = 5;

		/// <summary>
		/// Class for the indices of the buttons used by the controller.
		/// </summary>
		private class ButtonIndices
		{
			internal int Select = 4;
			internal int Start = 5;
			internal int A = 1;
			internal int B = 0;
			internal int C = 2;
			internal int D = 3;
		}

		/// <summary>
		/// Represents the possible bytes for brake notches.
		/// </summary>
		private enum BrakeByte
		{
			Released = 0x79,
			B1 = 0x8A,
			B2 = 0x94,
			B3 = 0x9A,
			B4 = 0xA2,
			B5 = 0xA8,
			B6 = 0xAF,
			B7 = 0xB2,
			B8 = 0xB5,
			Emergency = 0xB9,
			Transition = 0xFF
		}

		/// <summary>
		/// Represents the possible bytes for power notches.
		/// </summary>
		private enum PowerByte
		{
			N = 0x81,
			P1 = 0x6D,
			P2 = 0x54,
			P3 = 0x3F,
			P4 = 0x21,
			P5 = 0x00,
			Transition = 0xFF
		}

		/// <summary>
		/// Dictionary storing the mapping of each brake notch.
		/// </summary>
		private static readonly Dictionary<BrakeByte, InputTranslator.BrakeNotches> BrakeNotchMap = new Dictionary<BrakeByte, InputTranslator.BrakeNotches>
		{
			{ BrakeByte.Released, InputTranslator.BrakeNotches.Released },
			{ BrakeByte.B1, InputTranslator.BrakeNotches.B1 },
			{ BrakeByte.B2, InputTranslator.BrakeNotches.B2 },
			{ BrakeByte.B3, InputTranslator.BrakeNotches.B3 },
			{ BrakeByte.B4, InputTranslator.BrakeNotches.B4 },
			{ BrakeByte.B5, InputTranslator.BrakeNotches.B5 },
			{ BrakeByte.B6, InputTranslator.BrakeNotches.B6 },
			{ BrakeByte.B7, InputTranslator.BrakeNotches.B7 },
			{ BrakeByte.B8, InputTranslator.BrakeNotches.B8 },
			{ BrakeByte.Emergency, InputTranslator.BrakeNotches.Emergency }
		};

		/// <summary>
		/// Dictionary storing the mapping of each power notch.
		/// </summary>
		private static readonly Dictionary<PowerByte, InputTranslator.PowerNotches> PowerNotchMap = new Dictionary<PowerByte, InputTranslator.PowerNotches>
		{
			{ PowerByte.N, InputTranslator.PowerNotches.N },
			{ PowerByte.P1, InputTranslator.PowerNotches.P1 },
			{ PowerByte.P2, InputTranslator.PowerNotches.P2 },
			{ PowerByte.P3, InputTranslator.PowerNotches.P3 },
			{ PowerByte.P4, InputTranslator.PowerNotches.P4 },
			{ PowerByte.P5, InputTranslator.PowerNotches.P5 }
		};

		/// <summary>
		/// The button indices of the buttons used by the controller.
		/// </summary>
		private static ButtonIndices ButtonIndex = new ButtonIndices();

		/// <summary>
		/// Whether the controller has direction buttons.
		/// </summary>
		internal static bool HasDirectionButtons;

		/// <summary>
		/// Checks the controller model.
		/// </summary>
		/// <param name="id">A string representing the vendor and product ID.</param>
		/// <param name="capabilities">the capabilities of the joystick.</param>
		/// <returns>The controller model.</returns>
		internal static InputTranslator.ControllerModels GetControllerModel(string id, JoystickCapabilities capabilities)
		{
			// DGC-255/DGOC-44U
			if (id == "0ae4:0003")
			{
				// DGC-255 has direction buttons
				HasDirectionButtons = capabilities.HatCount > 0;
				ControllerBrakeNotches = 8;
				ControllerPowerNotches = 5;
				return InputTranslator.ControllerModels.UnbalanceStandard;
			}
			return InputTranslator.ControllerModels.Unsupported;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		/// <param name="joystick">The state of the joystick to read input from.</param>
		internal static void ReadInput(JoystickState joystick)
		{
			double brakeAxis = joystick.GetAxis(0);
			double powerAxis = joystick.GetAxis(1);
			foreach (var notch in BrakeNotchMap)
			{
				if (brakeAxis >= GetRangeMin((byte)notch.Key) && brakeAxis <= GetRangeMax((byte)notch.Key))
				{
					InputTranslator.BrakeNotch = notch.Value;
				}
			}
			foreach (var notch in PowerNotchMap)
			{
				if (powerAxis >= GetRangeMin((byte)notch.Key) && powerAxis <= GetRangeMax((byte)notch.Key))
				{
					InputTranslator.PowerNotch = notch.Value;
				}
			}

			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = joystick.GetButton(ButtonIndex.Select);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = joystick.GetButton(ButtonIndex.Start);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = joystick.GetButton(ButtonIndex.A);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = joystick.GetButton(ButtonIndex.B);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = joystick.GetButton(ButtonIndex.C);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = joystick.GetButton(ButtonIndex.D);

			if (HasDirectionButtons)
			{
				InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsUp ? 1 : 0);
				InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsDown ? 1 : 0);
				InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsLeft ? 1 : 0);
				InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsRight ? 1 : 0);
			}
		}

		/// <summary>
		/// Gets the minimum value for a notch byte.
		/// </summary>
		/// <param name="notch">A notch byte.</param>
		/// <returns>The minimum value for a notch byte.</returns>
		private static double GetRangeMin(byte notch)
		{
			double value = (notch - 1) * (2.0 / 255) - 1;
			return value;
		}

		/// <summary>
		/// Gets the maximum value for a notch byte.
		/// </summary>
		/// <param name="notch">A notch byte.</param>
		/// <returns>The maximum value for a notch byte.</returns>
		private static double GetRangeMax(byte notch)
		{
			double value = (notch + 1) * (2.0 / 255) - 1;
			return value;
		}
	}
}
