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
		internal static int currentDevice { get; private set; }

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
			currentDevice = -1;
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
				currentDevice = -1;
				return;
			}
			currentDevice = index;
		}

		/// <summary>
		/// Function to update a state of chosen joy-stick
		/// </summary>
		internal static void Update()
		{
			lastButtonState = GetButtonsState();

			if (currentDevice < 0 || currentDevice >= JoystickManager.AttachedJoysticks.Length || !OpenTK.Input.Joystick.GetCapabilities(currentDevice).IsConnected)
			{
				currentDevice = -1;
				return;
			}

			JoystickManager.AttachedJoysticks[currentDevice].Poll();
		}

		/// <summary>
		/// Function to acquire a GUID of chosen joy-stick
		/// </summary>
		/// <returns>GUID of chosen joy-stick</returns>
		internal static Guid GetGuid()
		{

			if (currentDevice < 0 || currentDevice >= JoystickManager.AttachedJoysticks.Length)
			{
				currentDevice = -1;
				return new Guid();
			}

			return JoystickManager.AttachedJoysticks[currentDevice].Guid;
		}

		/// <summary>
		/// Function to acquire the state of the button of chosen joy-stick
		/// </summary>
		/// <returns>State of the button of chosen joy-stick</returns>
		internal static List<ButtonState> GetButtonsState()
		{
			List<ButtonState> buttonsState = new List<ButtonState>();

			if (currentDevice < 0 || currentDevice >= JoystickManager.AttachedJoysticks.Length)
			{
				currentDevice = -1;
				return buttonsState;
			}

			for (int i = 0; i < JoystickManager.AttachedJoysticks[currentDevice].ButtonCount(); i++)
			{
				buttonsState.Add(JoystickManager.GetButton(currentDevice, i));
			}
			return buttonsState;
		}

		/// <summary>
		/// Function to acquire state of axial of chosen joy-stick
		/// </summary>
		/// <returns>State of axial of chosen joy-stick</returns>
		internal static List<double> GetAxises()
		{
			List<double> axises = new List<double>();

			if (currentDevice < 0 || currentDevice >= JoystickManager.AttachedJoysticks.Length)
			{
				currentDevice = -1;
				return axises;
			}

			for (int i = 0; i < JoystickManager.AttachedJoysticks[currentDevice].ButtonCount(); i++)
			{
				axises.Add(JoystickManager.GetAxis(currentDevice, i));
			}
			return axises;
		}
	}
}
