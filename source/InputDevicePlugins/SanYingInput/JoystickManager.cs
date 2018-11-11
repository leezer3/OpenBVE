using System;
using OpenTK.Input;

namespace SanYingInput {
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal partial class JoystickManager
	{

		/// <summary>Represents a joystick.</summary>
		internal abstract class Joystick
		{
			// --- members ---
			/// <summary>The textual representation of the joystick.</summary>
			internal string Name;
			/// <summary>The handle to the joystick.</summary>
			internal int Handle;

            internal Guid Guid;

			internal byte[] currentState = new byte[15];

			internal abstract ButtonState GetButton(int button);

			internal abstract double GetAxis(int axis);

			internal abstract JoystickHatState GetHat(int Hat);

			internal abstract int AxisCount();

			internal abstract int ButtonCount();

			internal abstract int HatCount();

			internal abstract void Poll();
		}

		internal JoystickManager()
		{
		}

		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Joystick[] AttachedJoysticks = new Joystick[] { };

		// --- functions ---

		/// <returns>Call this function to refresh the list of available joysticks and thier capabilities</returns>
		internal void RefreshJoysticks()
		{
			for (int i = 0; i < 10; i++)
			{
				//Load the list of attached openTK joysticks
				var state = OpenTK.Input.Joystick.GetState(i);
				var description = OpenTK.Input.Joystick.GetCapabilities(i).ToString();
				if (description == "{Axes: 0; Buttons: 0; Hats: 0; IsConnected: True}")
				{
					break;
				}
				//A joystick with 56 buttons and zero axis is likely the RailDriver, which is bugged in openTK
				if (state.IsConnected && description != "{Axes: 0; Buttons: 56; Hats: 0; IsConnected: True}")
				{
					StandardJoystick newJoystick = new StandardJoystick
					{
						Name = "Joystick" + i,
						Handle = i,
                        Guid = OpenTK.Input.Joystick.GetGuid(i)
					};

					bool alreadyFound = false;
					for (int j = 0; j < AttachedJoysticks.Length; j++)
					{
						if (AttachedJoysticks[j] is StandardJoystick && AttachedJoysticks[j].Handle == newJoystick.Handle)
						{
							alreadyFound = true;
						}
					}
					if (!alreadyFound)
					{
						int l = AttachedJoysticks.Length;
						Array.Resize(ref AttachedJoysticks, AttachedJoysticks.Length + 1);
						AttachedJoysticks[l] = newJoystick;
					}
				}
			}
		}

		internal static ButtonState GetButton(int Device, int Button)
		{
			if (Device < AttachedJoysticks.Length)
			{
				return AttachedJoysticks[Device].GetButton(Button);
			}
			return ButtonState.Released;
		}

		internal static double GetAxis(int Device, int Axis)
		{
			if (Device < AttachedJoysticks.Length)
			{
				return AttachedJoysticks[Device].GetAxis(Axis);
			}
			return 0.0;
		}

		internal static JoystickHatState GetHat(int Device, int Hat)
		{
			if (Device < AttachedJoysticks.Length)
			{
				return AttachedJoysticks[Device].GetHat(Hat);
			}
			return new JoystickHatState();
		}
	}
}
