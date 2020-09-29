using System.Collections.Generic;
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// The class which converts the input from the controller
	/// </summary>
	internal class InputTranslator
	{
		/// <summary>
		/// Enumeration representing controller models.
		/// </summary>
		internal enum ControllerModels
		{
			None = 0,
			PlayStation = 1,
		};

		/// <summary>
		/// Enumeration representing brake notches.
		/// </summary>
		internal enum BrakeNotches
		{
			Released = 0,
			B1 = 1,
			B2 = 2,
			B3 = 3,
			B4 = 4,
			B5 = 5,
			B6 = 6,
			B7 = 7,
			B8 = 8,
			Emergency = 9,
		};

		/// <summary>
		/// Enumeration representing power notches.
		/// </summary>
		internal enum PowerNotches
		{
			N = 0,
			P1 = 1,
			P2 = 2,
			P3 = 3,
			P4 = 4,
			P5 = 5,
		};

		/// <summary>
		/// Class with the state of the buttons.
		/// </summary>
		internal class ButtonState
		{
			internal OpenTK.Input.ButtonState A;
			internal OpenTK.Input.ButtonState B;
			internal OpenTK.Input.ButtonState C;
			internal OpenTK.Input.ButtonState Start;
			internal OpenTK.Input.ButtonState Select;
		}

		/// <summary>
		/// The index of the active controller, or -1 if not set.
		/// </summary>
		public static int activeControllerIndex = -1;

		/// <summary>
		/// Whether a supported controller is connected or not.
		/// </summary>
		internal static bool IsControllerConnected;

		/// <summary>
		/// The controller model.
		/// </summary>
		internal static ControllerModels ControllerModel;

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

		internal static ButtonState ControllerButtons = new ButtonState();

		/// <summary>
		/// Gets the controller model.
		/// </summary>
		internal static ControllerModels GetControllerModel(JoystickState state)
		{
			if (PSController.IsPSController(state))
			{
				return ControllerModels.PlayStation;
			}
			return ControllerModels.None;
		}

		/// <summary>
		/// Updates the status of the controller.
		/// </summary>
		public static void Update()
		{
			if (!IsControllerConnected)
			{
				JoystickState state = Joystick.GetState(activeControllerIndex);
				JoystickCapabilities capabilities = Joystick.GetCapabilities(activeControllerIndex);
				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				if (capabilities.ButtonCount > 0 && PSController.IsPSController(state))
				{
					IsControllerConnected = true;
					ControllerModel = ControllerModels.PlayStation;
					return;
				}
				ControllerModel = ControllerModels.None;
				activeControllerIndex = -1;
			}
			else
			{
				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				if (Joystick.GetCapabilities(activeControllerIndex).ButtonCount == 0)
				{
					IsControllerConnected = false;
					return;
				}
				GetInput();
			}
		}

		/// <summary>
		/// Gets the input from the controller.
		/// </summary>
		internal static void GetInput()
		{
			PreviousBrakeNotch = BrakeNotch;
			PreviousPowerNotch = PowerNotch;

			switch (ControllerModel)
			{
				case ControllerModels.PlayStation:
					PSController.ReadButtons(Joystick.GetState(activeControllerIndex));
					return;
			}
		}

		/// <summary>
		/// Gets the state of the buttons of the current controller
		/// </summary>
		/// <returns>State of the buttons of the current controller</returns>
		internal static List<OpenTK.Input.ButtonState> GetButtonsState()
		{
			List<OpenTK.Input.ButtonState> buttonsState = new List<OpenTK.Input.ButtonState>();

			if (IsControllerConnected)
			{
				for (int i = 0; i < Joystick.GetCapabilities(activeControllerIndex).ButtonCount; i++)
				{
					buttonsState.Add(Joystick.GetState(activeControllerIndex).GetButton(i));
				}
			}
			return buttonsState;
		}

		/// <summary>
		/// Gets the position of the hats of the current controller
		/// </summary>
		/// <returns>Position of the hats of the current controller</returns>
		internal static List<HatPosition> GetHatPositions()
		{
			List<HatPosition> hatPositions = new List<HatPosition>();

			if (IsControllerConnected)
			{
				for (int i = 0; i < Joystick.GetCapabilities(activeControllerIndex).ButtonCount; i++)
				{
					hatPositions.Add(Joystick.GetState(activeControllerIndex).GetHat((JoystickHat)i).Position);
				}
			}
			return hatPositions;
		}

		/// <summary>
		/// Compares two button states to find the index of the button that has been pressed.
		/// </summary>
		/// <returns>Index of the button that has been pressed.</returns>
		internal static int GetDifferentPressedIndex(List<OpenTK.Input.ButtonState> previousState, List<OpenTK.Input.ButtonState> newState, List<int> ignored)
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
		/// <returns>Index of the hat that has changed.</returns>
		internal static int GetChangedHat(List<HatPosition> previousPosition, List<HatPosition> newPosition)
		{
			for (int i = 0; i < newPosition.Count; i++)
			{
				if (newPosition[i] != previousPosition[i])
					return i;
			}
			return -1;
		}

	}
}
