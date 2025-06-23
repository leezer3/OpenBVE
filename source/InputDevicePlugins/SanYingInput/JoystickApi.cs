using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace SanYingInput
{
	/// <summary>
	/// The class that mediate between with the class of JoystickManager
	/// </summary>
	internal static class JoystickApi
	{
		/// <summary>
		/// Instance of the JoystickManager class
		/// </summary>
		private static JoystickManager Joystick = null;

		/// <summary>
		/// Index of joy-stick currently chosen
		/// </summary>
		internal static int CurrentDevice { get; private set; }

		/// <summary>
		/// State of the previous button
		/// </summary>
		internal static List<ButtonState> lastButtonState { get; private set; }

		/// <summary>
		/// Function to initialize
		/// </summary>
		internal static void Init()
		{
			Joystick = new JoystickManager();
			CurrentDevice = -1;
			lastButtonState = new List<ButtonState>();
		}

		/// <summary>
		/// Function to enumerate joy-stick
		/// </summary>
		internal static void EnumerateJoystick()
		{
			JoystickManager.AttachedJoysticks = new JoystickManager.Joystick[] { };
			Joystick.RefreshJoysticks();
		}

		/// <summary>
		/// Function to select joy-stick
		/// </summary>
		/// <param name="index">Index</param>
		internal static void SelectJoystick(int index)
		{
			if (index < 0 || index >= JoystickManager.AttachedJoysticks.Length)
			{
				CurrentDevice = -1;
				return;
			}
			CurrentDevice = index;
		}

		/// <summary>
		/// Function to update a state of chosen joy-stick
		/// </summary>
		internal static void Update()
		{
			lastButtonState = GetButtonsState();

			if (CurrentDevice < 0 || CurrentDevice >= JoystickManager.AttachedJoysticks.Length || !OpenTK.Input.Joystick.GetCapabilities(CurrentDevice).IsConnected)
			{
				CurrentDevice = -1;
				return;
			}

			JoystickManager.AttachedJoysticks[CurrentDevice].Poll();
		}

		/// <summary>
		/// Function to acquire a GUID of chosen joy-stick
		/// </summary>
		/// <returns>GUID of chosen joy-stick</returns>
		internal static Guid GetGuid()
		{

			if (CurrentDevice < 0 || CurrentDevice >= JoystickManager.AttachedJoysticks.Length)
			{
				CurrentDevice = -1;
				return Guid.Empty;
			}

			return JoystickManager.AttachedJoysticks[CurrentDevice].Guid;
		}

		/// <summary>
		/// Function to acquire the state of the button of chosen joy-stick
		/// </summary>
		/// <returns>State of the button of chosen joy-stick</returns>
		internal static List<ButtonState> GetButtonsState()
		{
			List<ButtonState> buttonsState = new List<ButtonState>();

			if (CurrentDevice < 0 || CurrentDevice >= JoystickManager.AttachedJoysticks.Length)
			{
				CurrentDevice = -1;
				return buttonsState;
			}

			for (int i = 0; i < JoystickManager.AttachedJoysticks[CurrentDevice].ButtonCount(); i++)
			{
				buttonsState.Add(JoystickManager.GetButton(CurrentDevice, i));
			}
			return buttonsState;
		}

		/// <summary>
		/// Function to acquire state of axial of chosen joy-stick
		/// </summary>
		/// <returns>State of axial of chosen joy-stick</returns>
		internal static List<double> GetAxisStates()
		{
			List<double> axisStates = new List<double>();

			if (CurrentDevice < 0 || CurrentDevice >= JoystickManager.AttachedJoysticks.Length)
			{
				CurrentDevice = -1;
				return axisStates;
			}

			for (int i = 0; i < JoystickManager.AttachedJoysticks[CurrentDevice].AxisCount(); i++)
			{
				axisStates.Add(JoystickManager.GetAxis(CurrentDevice, i));
			}
			return axisStates;
		}
	}
}
