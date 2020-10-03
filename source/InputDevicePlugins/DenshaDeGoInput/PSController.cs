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

using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK.Input;
namespace DenshaDeGoInput
{
	/// <summary>
	/// Class for Densha de GO! controllers for the Sony PlayStation
	/// </summary>
	internal class PSController
	{
		internal static bool UsesHat = false;

		internal static int hatIndex;

		internal class ButtonIndices
		{
			internal int Cross = -1;
			internal int Circle = -1;
			internal int Square = -1;
			internal int Triangle = -1;
			internal int L1 = -1;
			internal int R1 = -1;
			internal int L2 = -1;
			internal int R2 = -1;
			internal int Select = -1;
			internal int Start = -1;
			internal int Left = -1;
			internal int Right = -1;
		}

		internal class PressedButtons
		{
			internal bool Cross;
			internal bool Circle;
			internal bool Square;
			internal bool Triangle;
			internal bool L1;
			internal bool R1;
			internal bool L2;
			internal bool R2;
			internal bool Select;
			internal bool Start;
			internal bool Left;
			internal bool Right;
		}

		internal static ButtonIndices ButtonIndex = new ButtonIndices();
		internal static PressedButtons ButtonPressed = new PressedButtons();

		/// <summary>
		/// Checks if a joystick is a Sony PlayStation controller
		/// </summary>
		public static bool IsPSController(JoystickCapabilities joystick)
		{
			if ((joystick.ButtonCount >= 12 || (joystick.ButtonCount >= 10 && joystick.HatCount > 0)) && joystick.ButtonCount <= 20)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Reads the buttons from the controller
		/// </summary>
		public static void ReadButtons(JoystickState joystick)
		{
			ButtonPressed.Cross = joystick.IsButtonDown(ButtonIndex.Cross);
			ButtonPressed.Circle = joystick.IsButtonDown(ButtonIndex.Circle);
			ButtonPressed.Square = joystick.IsButtonDown(ButtonIndex.Square);
			ButtonPressed.Triangle = joystick.IsButtonDown(ButtonIndex.Triangle);
			ButtonPressed.L1 = joystick.IsButtonDown(ButtonIndex.L1);
			ButtonPressed.L2 = joystick.IsButtonDown(ButtonIndex.L2);
			ButtonPressed.R1 = joystick.IsButtonDown(ButtonIndex.R1);
			ButtonPressed.R2 = joystick.IsButtonDown(ButtonIndex.R2);
			ButtonPressed.Select = joystick.IsButtonDown(ButtonIndex.Select);
			ButtonPressed.Start = joystick.IsButtonDown(ButtonIndex.Start);

			if (UsesHat)
			{
				ButtonPressed.Left = joystick.GetHat((JoystickHat)hatIndex).IsLeft;
				ButtonPressed.Right = joystick.GetHat((JoystickHat)hatIndex).IsRight;
			}
			else
			{
				ButtonPressed.Left = joystick.IsButtonDown(ButtonIndex.Left);
				ButtonPressed.Right = joystick.IsButtonDown(ButtonIndex.Right);
			}

			if (!ButtonPressed.L2 && !ButtonPressed.R2 && !ButtonPressed.L1 && !ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Emergency;
			}
			if (ButtonPressed.L2 && !ButtonPressed.R2 && !ButtonPressed.L1 && ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B8;
			}
			if (ButtonPressed.L2 && !ButtonPressed.R2 && ButtonPressed.L1 && ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B7;
			}
			if (!ButtonPressed.L2 && ButtonPressed.R2 && !ButtonPressed.L1 && !ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B6;
			}
			if (!ButtonPressed.L2 && ButtonPressed.R2 && ButtonPressed.L1 && !ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B5;
			}
			if (ButtonPressed.L2 && ButtonPressed.R2 && !ButtonPressed.L1 && !ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B4;
			}
			if (ButtonPressed.L2 && ButtonPressed.R2 && ButtonPressed.L1 && !ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B3;
			}
			if (!ButtonPressed.L2 && ButtonPressed.R2 && !ButtonPressed.L1 && ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B2;
			}
			if (!ButtonPressed.L2 && ButtonPressed.R2 && ButtonPressed.L1 && ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.B1;
			}
			if (ButtonPressed.L2 && ButtonPressed.R2 && !ButtonPressed.L1 && ButtonPressed.R1)
			{
				InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Released;
			}

			if (ButtonPressed.Right && ButtonPressed.Left && !ButtonPressed.Triangle)
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
			}
			if (ButtonPressed.Right && !ButtonPressed.Left && ButtonPressed.Triangle)
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P1;
			}
			if (ButtonPressed.Right && !ButtonPressed.Left && !ButtonPressed.Triangle)
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P2;
			}
			if (!ButtonPressed.Right && ButtonPressed.Left && ButtonPressed.Triangle)
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P3;
			}
			if (!ButtonPressed.Right && ButtonPressed.Left && !ButtonPressed.Triangle)
			{
				if (UsesHat && InputTranslator.PreviousPowerNotch < InputTranslator.PowerNotches.P3)
				{
					// Hack for adapters which map the direction buttons to a hat and confuse N with P4
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
				else
				{
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.P4;
				}
			}
			if (!ButtonPressed.Right && !ButtonPressed.Left && ButtonPressed.Triangle)
			{
				InputTranslator.PowerNotch = InputTranslator.PowerNotches.P5;
			}

			InputTranslator.ControllerButtons.A = (OpenTK.Input.ButtonState)(ButtonPressed.Square ? 1 : 0);
			InputTranslator.ControllerButtons.B = (OpenTK.Input.ButtonState)(ButtonPressed.Cross ? 1 : 0);
			InputTranslator.ControllerButtons.C = (OpenTK.Input.ButtonState)(ButtonPressed.Circle ? 1 : 0);
			InputTranslator.ControllerButtons.Select = (OpenTK.Input.ButtonState)(ButtonPressed.Select ? 1 : 0);
			InputTranslator.ControllerButtons.Start = (OpenTK.Input.ButtonState)(ButtonPressed.Start ? 1 : 0);
		}

		public static void Calibrate()
		{
			string[] input = { "A", "B", "C", "SELECT", "START", "EMG", "B6", "B5", "B4", "B8", "P5", "N", "P2", "P1", "P5" };
			List<OpenTK.Input.ButtonState> ButtonState = InputTranslator.GetButtonsState();
			List<OpenTK.Input.ButtonState> PreviousButtonState;
			List<HatPosition> HatPositions = InputTranslator.GetHatPositions();
			List<HatPosition> PreviousHatPositions;
			List<int> ignored = new List<int>();

			// Button calibration
			for (int i = 0; i < 5; i++)
			{
				MessageBox.Show($"Hold the {input[i]} button in the controller and press OK.");
				PreviousButtonState = ButtonState;
				ButtonState = InputTranslator.GetButtonsState();
				int index = InputTranslator.GetDifferentPressedIndex(PreviousButtonState, ButtonState, ignored);
				ignored.Add(index);
				switch (i)
				{
					case 0:
						ButtonIndex.Square = index;
						break;
					case 1:
						ButtonIndex.Cross = index;
						break;
					case 2:
						ButtonIndex.Circle = index;
						break;
					case 3:
						ButtonIndex.Select = index;
						break;
					case 4:
						ButtonIndex.Start = index;
						break;
				}
			}

			// The brake handle needs to be moved to EMG to initialise properly
			MessageBox.Show($"Move the brake handle to {input[5]} and press OK.");

			// Brake handle calibration
			for (int i = 6; i < 10; i++)
			{
				MessageBox.Show($"Move the brake handle to {input[i]} and press OK.");
				PreviousButtonState = ButtonState;
				ButtonState = InputTranslator.GetButtonsState();
				int index = InputTranslator.GetDifferentPressedIndex(PreviousButtonState, ButtonState, ignored);
				ignored.Add(index);
				switch (i)
				{
					case 6:
						ButtonIndex.R2 = index;
						break;
					case 7:
						ButtonIndex.L1 = index;
						break;
					case 8:
						ButtonIndex.L2 = index;
						break;
					case 9:
						ButtonIndex.R1 = index;
						break;
				}
			}

			// The power handle needs to be moved to P5 and N to initialise properly
			MessageBox.Show($"Move the power handle to {input[10]} and press OK.");
			MessageBox.Show($"Move the power handle to {input[11]} and press OK.");

			// Clear previous data before calibrating the power handle
			ignored.Clear();
			ButtonState = InputTranslator.GetButtonsState();
			HatPositions = InputTranslator.GetHatPositions();

			// Power handle calibration
			for (int i = 12; i < 15; i++)
			{
				MessageBox.Show($"Move the power handle to {input[i]} and press OK.");
				PreviousButtonState = ButtonState;
				PreviousHatPositions = HatPositions;
				ButtonState = InputTranslator.GetButtonsState();
				HatPositions = InputTranslator.GetHatPositions();
				int index = InputTranslator.GetDifferentPressedIndex(PreviousButtonState, ButtonState, ignored);
				ignored.Add(index);

				int hat = InputTranslator.GetChangedHat(PreviousHatPositions, HatPositions);
				if (hat != -1 && i != 13)
				{
					// If a hat has changed, it means the converter is mapping the direction buttons 
					UsesHat = true;
					hatIndex = hat;
				}
				else
				{
					UsesHat = false;
				}

				switch (i)
				{
					case 12:
						ButtonIndex.Left = index;
						break;
					case 13:
						ButtonIndex.Triangle = index;
						break;
					case 14:
						ButtonIndex.Right = index;
						break;
				}
			}
		}

	}
}
