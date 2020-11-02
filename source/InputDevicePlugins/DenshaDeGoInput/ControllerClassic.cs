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
using OpenBveApi.Interface;
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class for Densha de GO! controllers for classic consoles.
	/// </summary>
	internal static class ControllerClassic
	{
		/// <summary>
		/// Whether the adapter uses a hat to map the direction buttons.
		/// </summary>
		internal static bool usesHat;

		/// <summary>
		/// The index of the hat used to map the direction buttons.
		/// </summary>
		internal static int hatIndex;

		/// <summary>
		/// Class for the indices of the buttons used by the controller.
		/// </summary>
		internal class ButtonIndices
		{
			internal int Select = -1;
			internal int Start = -1;
			internal int A = -1;
			internal int B = -1;
			internal int C = -1;
			internal int Power1 = -1;
			internal int Power2 = -1;
			internal int Power3 = -1;
			internal int Brake1 = -1;
			internal int Brake2 = -1;
			internal int Brake3 = -1;
			internal int Brake4 = -1;
		}

		/// <summary>
		/// The button indices of the buttons used by the controller.
		/// </summary>
		internal static ButtonIndices ButtonIndex = new ButtonIndices();

		/// <summary>
		/// Checks whether a joystick is a classic console controller.
		/// </summary>
		/// <param name="capabilities">the capabilities of the joystick.</param>
		/// <returns>Whether the controller is compatible.</returns>
		internal static bool IsCompatibleController(JoystickCapabilities capabilities)
		{
			// A valid controller needs at least 12 buttons or 10 buttons plus a hat. If there are more than 20 buttons, the joystick is unlikely a valid controller.
			if ((capabilities.ButtonCount >= 12 || (capabilities.ButtonCount >= 10 && capabilities.HatCount > 0)) && capabilities.ButtonCount <= 20)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		/// <param name="joystick">The state of the joystick to read input from.</param>
		/// <param name="powerNotch">The returned power notch</param>
		/// <param name="brakeNotch">The returned brake notch</param>
		internal static void ReadInput(JoystickState joystick, out InputTranslator.PowerNotches powerNotch, out InputTranslator.BrakeNotches brakeNotch)
		{
			powerNotch = InputTranslator.PowerNotches.None;
			brakeNotch = InputTranslator.BrakeNotches.None;
			powerNotch = joystick.IsButtonDown(ButtonIndex.Power1) ? powerNotch | InputTranslator.PowerNotches.Power1 : powerNotch & ~InputTranslator.PowerNotches.Power1;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake1) ? brakeNotch | InputTranslator.BrakeNotches.Brake1 : brakeNotch & ~InputTranslator.BrakeNotches.Brake1;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake2) ? brakeNotch | InputTranslator.BrakeNotches.Brake2 : brakeNotch & ~InputTranslator.BrakeNotches.Brake2;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake3) ? brakeNotch | InputTranslator.BrakeNotches.Brake3 : brakeNotch & ~InputTranslator.BrakeNotches.Brake3;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake4) ? brakeNotch | InputTranslator.BrakeNotches.Brake4 : brakeNotch & ~InputTranslator.BrakeNotches.Brake4;
			
			if (usesHat)
			{
				// The adapter uses the hat to map the direction buttons.
				// This is the case of some PlayStation adapters.
				powerNotch = joystick.GetHat((JoystickHat)hatIndex).IsLeft ? powerNotch | InputTranslator.PowerNotches.Power2 : powerNotch & ~InputTranslator.PowerNotches.Power2;
				powerNotch = joystick.GetHat((JoystickHat)hatIndex).IsRight ? powerNotch | InputTranslator.PowerNotches.Power3 : powerNotch & ~InputTranslator.PowerNotches.Power3;
			}
			else
			{
				// The adapter maps the direction buttons to independent buttons.
				powerNotch = joystick.IsButtonDown(ButtonIndex.Power2) ? powerNotch | InputTranslator.PowerNotches.Power2 : powerNotch & ~InputTranslator.PowerNotches.Power2;
				powerNotch = joystick.IsButtonDown(ButtonIndex.Power3) ? powerNotch | InputTranslator.PowerNotches.Power3 : powerNotch & ~InputTranslator.PowerNotches.Power3;
			}

			if (usesHat && powerNotch == InputTranslator.PowerNotches.P4)
			{
				if (InputTranslator.PreviousPowerNotch < InputTranslator.PowerNotches.P3)
				{
					// Hack for adapters which map the direction buttons to a hat and confuse N with P4
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
				else
				{
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.P4;
				}
			}
			else
			{
				InputTranslator.PowerNotch = powerNotch;
			}
			InputTranslator.ControllerButtons.Select = joystick.GetButton(ButtonIndex.Select);
			InputTranslator.ControllerButtons.Start = joystick.GetButton(ButtonIndex.Start);
			InputTranslator.ControllerButtons.A = joystick.GetButton(ButtonIndex.A);
			InputTranslator.ControllerButtons.B = joystick.GetButton(ButtonIndex.B);
			InputTranslator.ControllerButtons.C = joystick.GetButton(ButtonIndex.C);
		}


		/// <summary>
		/// Launches the calibration wizard to guess the button indices used by the adapter.
		/// </summary>
		internal static void Calibrate()
		{
			string[] input = 
			{
				"SELECT",
				"START",
				"A",
				"B",
				"C",
				Translations.QuickReferences.HandleEmergency,
				Translations.QuickReferences.HandleBrake + "6",
				Translations.QuickReferences.HandleBrake + "5",
				Translations.QuickReferences.HandleBrake + "4",
				Translations.QuickReferences.HandleBrake + "8",
				Translations.QuickReferences.HandlePower + "5",
				Translations.QuickReferences.HandlePowerNull,
				Translations.QuickReferences.HandlePower + "2",
				Translations.QuickReferences.HandlePower + "1",
				Translations.QuickReferences.HandlePower + "5",
			};

			List<OpenTK.Input.ButtonState> ButtonState = InputTranslator.GetButtonsState();
			List<OpenTK.Input.ButtonState> PreviousButtonState;
			List<HatPosition> HatPositions = InputTranslator.GetHatPositions();
			List<HatPosition> PreviousHatPositions;
			List<int> ignored = new List<int>();

			// Button calibration
			for (int i = 0; i < 5; i++)
			{
				MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_button").Replace("[button]", input[i]));
				PreviousButtonState = ButtonState;
				ButtonState = InputTranslator.GetButtonsState();
				int index = InputTranslator.GetDifferentPressedIndex(PreviousButtonState, ButtonState, ignored);
				ignored.Add(index);
				switch (i)
				{
					case 0:
						ButtonIndex.Select = index;
						break;
					case 1:
						ButtonIndex.Start = index;
						break;
					case 2:
						ButtonIndex.A = index;
						break;
					case 3:
						ButtonIndex.B = index;
						break;
					case 4:
						ButtonIndex.C = index;
						break;
				}
			}

			// The brake handle needs to be moved to EMG to initialise properly
			MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_brake").Replace("[notch]", input[5]));

			// Brake handle calibration
			for (int i = 6; i < 10; i++)
			{
				MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_brake").Replace("[notch]", input[i]));
				PreviousButtonState = ButtonState;
				ButtonState = InputTranslator.GetButtonsState();
				int index = InputTranslator.GetDifferentPressedIndex(PreviousButtonState, ButtonState, ignored);
				ignored.Add(index);
				switch (i)
				{
					case 6:
						ButtonIndex.Brake4 = index;
						break;
					case 7:
						ButtonIndex.Brake1 = index;
						break;
					case 8:
						ButtonIndex.Brake2 = index;
						break;
					case 9:
						ButtonIndex.Brake3 = index;
						break;
				}
			}

			// The power handle needs to be moved to P5 and N to initialise properly
			MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_power").Replace("[notch]", input[10]));
			MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_power").Replace("[notch]", input[11]));

			// Clear previous data before calibrating the power handle
			ignored.Clear();
			ButtonState = InputTranslator.GetButtonsState();
			HatPositions = InputTranslator.GetHatPositions();

			// Power handle calibration
			for (int i = 12; i < 15; i++)
			{
				MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_power").Replace("[notch]", input[i]));
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
					usesHat = true;
					hatIndex = hat;
				}
				else
				{
					usesHat = false;
				}

				switch (i)
				{
					case 12:
						ButtonIndex.Power2 = index;
						break;
					case 13:
						ButtonIndex.Power1 = index;
						break;
					case 14:
						ButtonIndex.Power3 = index;
						break;
				}
			}
		}
	}
}
