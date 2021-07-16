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
using System.Windows.Forms;
using OpenBveApi.Interface;
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class representing a classic console controller
	/// </summary>
	internal class ClassicController : Controller
	{
		/// <summary>A cached list of supported connected controllers.</summary>
		private static Dictionary<Guid, Controller> cachedControllers = new Dictionary<Guid, Controller>();

		/// <summary>Whether the adapter uses a hat to map the direction buttons.</summary>
		internal static bool UsesHat;

		/// <summary>Whether the adapter uses an axis to map the direction buttons.</summary>
		internal static bool UsesAxis;

		/// <summary>The index of the hat used to map the direction buttons.</summary>
		internal static int HatIndex;

		/// <summary>The index of the axis used to map the direction buttons.</summary>
		internal static int AxisIndex;

		/// <summary>The OpenTK joystick index for this controller.</summary>
		private int joystickIndex;

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
		/// Enumeration representing brake notches.
		/// </summary>
		[Flags]
		private enum BrakeNotchesEnum
		{
			// The controller has 4 physical buttons.
			// These do *not* map directly to the simulation

			/// <summary>No buttons are pressed on the controller</summary>
			None = 0,
			/// <summary>The Brake1 button is pressed</summary>
			Brake1 = 1,
			/// <summary>The Brake2 button is pressed</summary>
			Brake2 = 2,
			/// <summary>The Brake3 button is pressed</summary>
			Brake3 = 4,
			/// <summary>The Brake4 button is pressed</summary>
			Brake4 = 8,

			// Our returned notches to the simulation are a bitflag button combination

			/// <summary>Brakes are released</summary>
			Released = Brake2 | Brake3 | Brake4,
			/// <summary>Brake Notch B1</summary>
			B1 = Brake1 | Brake3 | Brake4,
			/// <summary>Brake Notch B2</summary>
			B2 = Brake3 | Brake4,
			/// <summary>Brake Notch B3</summary>
			B3 = Brake1 | Brake2 | Brake4,
			/// <summary>Brake Notch B4</summary>
			B4 = Brake2 | Brake4,
			/// <summary>Brake Notch B5</summary>
			B5 = Brake1 | Brake4,
			/// <summary>Brake Notch B6</summary>
			B6 = Brake4,
			/// <summary>Brake Notch B7</summary>
			B7 = Brake1 | Brake2 | Brake3,
			/// <summary>Brake Notch B8</summary>
			B8 = Brake2 | Brake3,
			/// <summary>Emergency</summary>
			Emergency = None,
			/// <summary>Transition between notches</summary>
			Transition = Brake1 | Brake2 | Brake3 | Brake4,
		};

		/// <summary>
		/// Dictionary storing the mapping of each brake notch.
		/// </summary>
		private static readonly Dictionary<BrakeNotchesEnum, InputTranslator.BrakeNotches> BrakeNotchMap = new Dictionary<BrakeNotchesEnum, InputTranslator.BrakeNotches>
		{
			{ BrakeNotchesEnum.Released, InputTranslator.BrakeNotches.Released },
			{ BrakeNotchesEnum.B1, InputTranslator.BrakeNotches.B1 },
			{ BrakeNotchesEnum.B2, InputTranslator.BrakeNotches.B2 },
			{ BrakeNotchesEnum.B3, InputTranslator.BrakeNotches.B3 },
			{ BrakeNotchesEnum.B4, InputTranslator.BrakeNotches.B4 },
			{ BrakeNotchesEnum.B5, InputTranslator.BrakeNotches.B5 },
			{ BrakeNotchesEnum.B6, InputTranslator.BrakeNotches.B6 },
			{ BrakeNotchesEnum.B7, InputTranslator.BrakeNotches.B7 },
			{ BrakeNotchesEnum.B8, InputTranslator.BrakeNotches.B8 },
			{ BrakeNotchesEnum.Emergency, InputTranslator.BrakeNotches.Emergency }
		};

		/// <summary>
		/// Enumeration representing power notches.
		/// </summary>
		[Flags]
		private enum PowerNotchesEnum
		{
			// The controller has 3 physical buttons.
			// These do *not* map directly to the simulation

			/// <summary>No buttons are pressed on the controller</summary>
			None = 0,
			/// <summary>The Power1 button is pressed</summary>
			Power1 = 1,
			/// <summary>The Power2 button is pressed</summary>
			Power2 = 2,
			/// <summary>The Power3 button is pressed</summary>
			Power3 = 4,

			// Our returned notches to the simulation are a bitflag button combination

			/// <summary>Power is in N</summary>
			N = Power2 | Power3,
			/// <summary>Power notch P1</summary>
			P1 = Power1 | Power3,
			/// <summary>Power notch P2</summary>
			P2 = Power3,
			/// <summary>Power notch P3</summary>
			P3 = Power1 | Power2,
			/// <summary>Power notch P4</summary>
			P4 = Power2,
			/// <summary>Power notch P5</summary>
			P5 = Power1,
			/// <summary>Transition between notches</summary>
			Transition = None,
		};

		/// <summary>
		/// Dictionary storing the mapping of each power notch.
		/// </summary>
		private static readonly Dictionary<PowerNotchesEnum, InputTranslator.PowerNotches> PowerNotchMap = new Dictionary<PowerNotchesEnum, InputTranslator.PowerNotches>
		{
			{ PowerNotchesEnum.N, InputTranslator.PowerNotches.N },
			{ PowerNotchesEnum.P1, InputTranslator.PowerNotches.P1 },
			{ PowerNotchesEnum.P2, InputTranslator.PowerNotches.P2 },
			{ PowerNotchesEnum.P3, InputTranslator.PowerNotches.P3 },
			{ PowerNotchesEnum.P4, InputTranslator.PowerNotches.P4 },
			{ PowerNotchesEnum.P5, InputTranslator.PowerNotches.P5 }
		};


		/// <summary>
		/// Initializes a classic controller.
		/// </summary>
		internal ClassicController()
		{
			joystickIndex = -1;
			RequiresCalibration = true;
			BrakeNotches = 8;
			PowerNotches = 5;
			Buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal override void ReadInput()
		{
			JoystickState joystick = Joystick.GetState(joystickIndex);
			PowerNotchesEnum powerNotch = PowerNotchesEnum.None;
			BrakeNotchesEnum brakeNotch = BrakeNotchesEnum.None;
			powerNotch = joystick.IsButtonDown(ButtonIndex.Power1) ? powerNotch | PowerNotchesEnum.Power1 : powerNotch & ~PowerNotchesEnum.Power1;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake1) ? brakeNotch | BrakeNotchesEnum.Brake1 : brakeNotch & ~BrakeNotchesEnum.Brake1;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake2) ? brakeNotch | BrakeNotchesEnum.Brake2 : brakeNotch & ~BrakeNotchesEnum.Brake2;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake3) ? brakeNotch | BrakeNotchesEnum.Brake3 : brakeNotch & ~BrakeNotchesEnum.Brake3;
			brakeNotch = joystick.IsButtonDown(ButtonIndex.Brake4) ? brakeNotch | BrakeNotchesEnum.Brake4 : brakeNotch & ~BrakeNotchesEnum.Brake4;

			if (UsesAxis)
			{
				// The adapter uses an axis to map the direction buttons.
				// This is the case of some PlayStation adapters.
				powerNotch = joystick.GetAxis(AxisIndex) < -0.5 ? powerNotch | PowerNotchesEnum.Power2 : powerNotch & ~PowerNotchesEnum.Power2;
				powerNotch = joystick.GetAxis(AxisIndex) > 0.5 ? powerNotch | PowerNotchesEnum.Power3 : powerNotch & ~PowerNotchesEnum.Power3;
			}
			else if (UsesHat)
			{
				// The adapter uses the hat to map the direction buttons.
				// This is the case of some PlayStation adapters.
				powerNotch = joystick.GetHat((JoystickHat)HatIndex).IsLeft ? powerNotch | PowerNotchesEnum.Power2 : powerNotch & ~PowerNotchesEnum.Power2;
				powerNotch = joystick.GetHat((JoystickHat)HatIndex).IsRight ? powerNotch | PowerNotchesEnum.Power3 : powerNotch & ~PowerNotchesEnum.Power3;
			}
			else
			{
				// The adapter maps the direction buttons to independent buttons.
				powerNotch = joystick.IsButtonDown(ButtonIndex.Power2) ? powerNotch | PowerNotchesEnum.Power2 : powerNotch & ~PowerNotchesEnum.Power2;
				powerNotch = joystick.IsButtonDown(ButtonIndex.Power3) ? powerNotch | PowerNotchesEnum.Power3 : powerNotch & ~PowerNotchesEnum.Power3;
			}

			if ((UsesHat || UsesAxis) && powerNotch == PowerNotchesEnum.P4)
			{
				// Hack for adapters using a hat/axis where pressing left and right simultaneously reports only left being pressed
				if (InputTranslator.PreviousPowerNotch < InputTranslator.PowerNotches.P3)
				{
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
				else
				{
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.P4;
				}
			}
			else if ((UsesHat || UsesAxis ) && powerNotch == PowerNotchesEnum.Transition)
			{
				// Hack for adapters using a hat/axis where pressing left and right simultaneously reports nothing being pressed, the same as the transition state
				// Has the side effect of the power notch jumping P1>N>P2, but it is barely noticeable unless moving the handle very slowly
				if (InputTranslator.PreviousPowerNotch < InputTranslator.PowerNotches.P2)
				{
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
			}
			else if (powerNotch != PowerNotchesEnum.Transition)
			{
				// Set notch only if it is not a transition
				InputTranslator.PowerNotch = PowerNotchMap[powerNotch];
			}
			if (brakeNotch != BrakeNotchesEnum.Transition && (brakeNotch == BrakeNotchesEnum.Emergency || brakeNotch >= BrakeNotchesEnum.B8))
			{
				// Set notch only if it is not a transition nor an unmarked notch
				InputTranslator.BrakeNotch = BrakeNotchMap[brakeNotch];
			}
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = joystick.GetButton(ButtonIndex.Select);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = joystick.GetButton(ButtonIndex.Start);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = joystick.GetButton(ButtonIndex.A);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = joystick.GetButton(ButtonIndex.B);
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = joystick.GetButton(ButtonIndex.C);
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
					// A valid controller needs at least 12 buttons or 10 buttons plus a hat. If there are more than 20 buttons, the joystick is unlikely a valid controller.
					JoystickCapabilities capabilities = Joystick.GetCapabilities(i);
					if ((capabilities.ButtonCount >= 12 || (capabilities.ButtonCount >= 10 && capabilities.HatCount > 0)) && capabilities.ButtonCount <= 20)
					{
						ClassicController newcontroller = new ClassicController()
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
					// Cached controller, update index
					((ClassicController)cachedControllers[guid]).joystickIndex = i;
					// HACK: IsConnected is broken, we check the capabilities instead to know if the controller is connected or not
					cachedControllers[guid].IsConnected = Joystick.GetCapabilities(i).ButtonCount > 0;
				}
			}

			return cachedControllers;
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

			UsesHat = false;
			HatIndex = -1;
			UsesAxis = false;
			AxisIndex = -1;

			List<OpenTK.Input.ButtonState> buttonState = GetButtonsState();
			List<OpenTK.Input.ButtonState> PreviousButtonState;
			List<HatPosition> hatPositions = GetHatPositions();
			List<HatPosition> previousHatPositions;
			List<float> axisValues = GetAxisValues();
			List<float> previousAxisValues;
			List<int> ignored = new List<int>();

			// Button calibration
			for (int i = 0; i < 5; i++)
			{
				MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_button").Replace("[button]", input[i]));
				PreviousButtonState = buttonState;
				buttonState = GetButtonsState();
				int index = GetDifferentPressedIndex(PreviousButtonState, buttonState, ignored);
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
				PreviousButtonState = buttonState;
				buttonState = GetButtonsState();
				int index = GetDifferentPressedIndex(PreviousButtonState, buttonState, ignored);
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
			buttonState = GetButtonsState();
			hatPositions = GetHatPositions();
			axisValues = GetAxisValues();

			// Power handle calibration
			for (int i = 12; i < 15; i++)
			{
				MessageBox.Show(Translations.GetInterfaceString("denshadego_calibrate_power").Replace("[notch]", input[i]));
				PreviousButtonState = buttonState;
				previousHatPositions = hatPositions;
				previousAxisValues = axisValues;
				buttonState = GetButtonsState();
				hatPositions = GetHatPositions();
				axisValues = GetAxisValues();
				int index = GetDifferentPressedIndex(PreviousButtonState, buttonState, ignored);
				ignored.Add(index);

				int axis = GetChangedAxis(previousAxisValues, axisValues);
				if (axis != -1 && i == 12)
				{
					// If an axis has changed, it means the converter is mapping the direction buttons 
					UsesAxis = true;
					AxisIndex = axis;
				}

				int hat = GetChangedHat(previousHatPositions, hatPositions);
				if (!UsesAxis && hat != -1 && i == 12)
				{
					// If a hat has changed, it means the converter is mapping the direction buttons 
					UsesHat = true;
					HatIndex = hat;
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

		/// <summary>
		/// Gets the state of the buttons of the current controller
		/// </summary>
		/// <returns>State of the buttons of the current controller</returns>
		private static List<OpenTK.Input.ButtonState> GetButtonsState()
		{
			List<OpenTK.Input.ButtonState> buttonsState = new List<OpenTK.Input.ButtonState>();

			if (InputTranslator.IsControllerConnected)
			{
				int index = (((ClassicController)InputTranslator.Controllers[InputTranslator.ActiveControllerGuid]).joystickIndex);
				for (int i = 0; i < Joystick.GetCapabilities(index).ButtonCount; i++)
				{
					buttonsState.Add(Joystick.GetState(index).GetButton(i));
				}
			}
			return buttonsState;
		}

		/// <summary>
		/// Gets the position of the hats of the current controller
		/// </summary>
		/// <returns>Position of the hats of the current controller</returns>
		private static List<HatPosition> GetHatPositions()
		{
			List<HatPosition> hatPositions = new List<HatPosition>();

			if (InputTranslator.IsControllerConnected)
			{
				int index = (((ClassicController)InputTranslator.Controllers[InputTranslator.ActiveControllerGuid]).joystickIndex);
				for (int i = 0; i < Joystick.GetCapabilities(index).HatCount; i++)
				{
					hatPositions.Add(Joystick.GetState(index).GetHat((JoystickHat)i).Position);
				}
			}
			return hatPositions;
		}

		/// <summary>
		/// Gets the value of the axes of the current controller
		/// </summary>
		/// <returns>Value of the axes of the current controller</returns>
		private static List<float> GetAxisValues()
		{
			List<float> axisValues = new List<float>();

			if (InputTranslator.IsControllerConnected)
			{
				int index = (((ClassicController)InputTranslator.Controllers[InputTranslator.ActiveControllerGuid]).joystickIndex);
				for (int i = 0; i < Joystick.GetCapabilities(index).AxisCount; i++)
				{
					axisValues.Add(Joystick.GetState(index).GetAxis(i));
				}
			}
			return axisValues;
		}

		/// <summary>
		/// Compares two button states to find the index of the button that has been pressed.
		/// </summary>
		/// <param name="previousState">The previous state of the buttons.</param>
		/// <param name="newState">The new state of the buttons.</param>
		/// <param name="ignored">The list of ignored buttons.</param>
		/// <returns>Index of the button that has been pressed.</returns>
		private static int GetDifferentPressedIndex(List<OpenTK.Input.ButtonState> previousState, List<OpenTK.Input.ButtonState> newState, List<int> ignored)
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
		private static int GetChangedHat(List<HatPosition> previousPosition, List<HatPosition> newPosition)
		{
			for (int i = 0; i < newPosition.Count; i++)
			{
				if (newPosition[i] != previousPosition[i])
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Compares two axis values to find the index of the axis that has changed.
		/// </summary>
		/// <param name="previousValue">The previous value of the axis.</param>
		/// <param name="newValue">The new value of the axis.</param>
		/// <returns>Index of the hat that has changed.</returns>
		private static int GetChangedAxis(List<float> previousValue, List<float> newValue)
		{
			for (int i = 0; i < newValue.Count; i++)
			{
				if (newValue[i] != previousValue[i])
					return i;
			}
			return -1;
		}

	}
}
