using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace OpenBve.Input
{
	internal abstract class JoystickManager
	{
		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal Dictionary<Guid, AbstractJoystick> AttachedJoysticks = new Dictionary<Guid, AbstractJoystick>();

		/// <returns>Call this function to refresh the list of available joysticks and thier capabilities</returns>
		internal abstract void RefreshJoysticks();

		internal bool RailDriverInit;

		/// <summary>Gets the number of RailDriver controllers connected</summary>
		internal virtual int RailDriverCount
		{
			get
			{
				return 0;
			}
		}

		internal ButtonState GetButton(Guid Device, int Button)
		{
			if (AttachedJoysticks.ContainsKey(Device))
			{
				return AttachedJoysticks[Device].GetButton(Button);
			}
			return ButtonState.Released;
		}

		internal double GetAxis(Guid Device, int Axis)
		{
			if (AttachedJoysticks.ContainsKey(Device))
			{
				return AttachedJoysticks[Device].GetAxis(Axis);
			}
			return 0.0;
		}

		internal JoystickHatState GetHat(Guid Device, int Hat)
		{
			if (AttachedJoysticks.ContainsKey(Device))
			{
				return AttachedJoysticks[Device].GetHat(Hat);
			}
			return new JoystickHatState();
		}
	}
}
