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
		public static bool IsCompatibleController(JoystickCapabilities joystick)
		{
			// The controller's ID is 0AE4:0003, but we cannot use it here (yet)
			if (joystick.ButtonCount == 6 && joystick.AxisCount == 2)
			{
				// The one-handle controller has direction buttons
				hasDirectionButtons = joystick.HatCount > 0;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		public static void ReadInput(JoystickState joystick)
		{
			InputTranslator.ControllerButtons.Select = (ButtonState)(joystick.IsButtonDown(ButtonIndex.Select) ? 1 : 0);
			InputTranslator.ControllerButtons.Start = (ButtonState)(joystick.IsButtonDown(ButtonIndex.Start) ? 1 : 0);
			InputTranslator.ControllerButtons.A = (ButtonState)(joystick.IsButtonDown(ButtonIndex.A) ? 1 : 0);
			InputTranslator.ControllerButtons.B = (ButtonState)(joystick.IsButtonDown(ButtonIndex.B) ? 1 : 0);
			InputTranslator.ControllerButtons.C = (ButtonState)(joystick.IsButtonDown(ButtonIndex.C) ? 1 : 0);
			InputTranslator.ControllerButtons.D = (ButtonState)(joystick.IsButtonDown(ButtonIndex.D) ? 1 : 0);

			if (hasDirectionButtons)
			{
				InputTranslator.ControllerButtons.Up = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsUp ? 1 : 0);
				InputTranslator.ControllerButtons.Down = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsDown ? 1 : 0);
				InputTranslator.ControllerButtons.Left = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsLeft ? 1 : 0);
				InputTranslator.ControllerButtons.Right = (ButtonState)(joystick.GetHat(JoystickHat.Hat0).IsRight ? 1 : 0);
			}
		}
	}
}
