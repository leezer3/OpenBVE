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
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class for Densha de GO! controllers for PC by Unbalance.
	/// </summary>
	internal static class ControllerUnbalance
	{
		/// <summary>
		/// Class for the indices of the buttons used by the controller.
		/// </summary>
		internal class ButtonIndices
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
		internal enum BrakeByte
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
		internal enum PowerByte
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
		/// The button indices of the buttons used by the controller.
		/// </summary>
		internal static ButtonIndices ButtonIndex = new ButtonIndices();

		/// <summary>
		/// Whether the controller has direction buttons.
		/// </summary>
		internal static bool hasDirectionButtons;

		/// <summary>
		/// Checks whether a joystick is an Unbalance controller.
		/// </summary>
		/// <param name="id">A string representing the vendor and product ID.</param>
		/// <param name="capabilities">the capabilities of the joystick.</param>
		/// <returns>Whether the controller is compatible.</returns>
		internal static bool IsCompatibleController(string id, JoystickCapabilities capabilities)
		{
			// DGC-255/DGOC-44U
			if (id == "0ae4:0003")
			{
				// DGC-255 has direction buttons
				hasDirectionButtons = capabilities.HatCount > 0;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		/// <param name="joystick">The state of the joystick to read input from.</param>
		internal static void ReadInput(JoystickState joystick)
		{
			double brakeAxis = joystick.GetAxis(0);
			double powerAxis = joystick.GetAxis(1);
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.Emergency) && brakeAxis <= GetRangeMax((byte)BrakeByte.Emergency))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Emergency;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B8) && brakeAxis <= GetRangeMax((byte)BrakeByte.B8))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B8;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B7) && brakeAxis <= GetRangeMax((byte)BrakeByte.B7))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B7;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B6) && brakeAxis <= GetRangeMax((byte)BrakeByte.B6))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B6;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B5) && brakeAxis <= GetRangeMax((byte)BrakeByte.B5))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B5;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B4) && brakeAxis <= GetRangeMax((byte)BrakeByte.B4))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B4;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B3) && brakeAxis <= GetRangeMax((byte)BrakeByte.B3))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B3;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B2) && brakeAxis <= GetRangeMax((byte)BrakeByte.B2))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B2;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.B1) && brakeAxis <= GetRangeMax((byte)BrakeByte.B1))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B1;
			}
			if (brakeAxis >= GetRangeMin((byte)BrakeByte.Released) && brakeAxis <= GetRangeMax((byte)BrakeByte.Released))
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Released;
			}

			if (powerAxis >= GetRangeMin((byte)PowerByte.N) && powerAxis <= GetRangeMax((byte)PowerByte.N))
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
			}
			if (powerAxis >= GetRangeMin((byte)PowerByte.P1) && powerAxis <= GetRangeMax((byte)PowerByte.P1))
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P1;
			}
			if (powerAxis >= GetRangeMin((byte)PowerByte.P2) && powerAxis <= GetRangeMax((byte)PowerByte.P2))
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P2;
			}
			if (powerAxis >= GetRangeMin((byte)PowerByte.P3) && powerAxis <= GetRangeMax((byte)PowerByte.P3))
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P3;
			}
			if (powerAxis >= GetRangeMin((byte)PowerByte.P4) && powerAxis <= GetRangeMax((byte)PowerByte.P4))
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P4;
			}
			if (powerAxis >= GetRangeMin((byte)PowerByte.P5) && powerAxis <= GetRangeMax((byte)PowerByte.P5))
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P5;
			}

			InputTranslator.ControllerButtons.Select = joystick.GetButton(ButtonIndex.Select);
			InputTranslator.ControllerButtons.Start = joystick.GetButton(ButtonIndex.Start);
			InputTranslator.ControllerButtons.A = joystick.GetButton(ButtonIndex.A);
			InputTranslator.ControllerButtons.B = joystick.GetButton(ButtonIndex.B);
			InputTranslator.ControllerButtons.C = joystick.GetButton(ButtonIndex.C);
			InputTranslator.ControllerButtons.D = joystick.GetButton(ButtonIndex.D);

			if (hasDirectionButtons)
			{
				InputTranslator.ControllerButtons.Up = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsUp ? 1 : 0);
				InputTranslator.ControllerButtons.Down = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsDown ? 1 : 0);
				InputTranslator.ControllerButtons.Left = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsLeft ? 1 : 0);
				InputTranslator.ControllerButtons.Right = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsRight ? 1 : 0);
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
