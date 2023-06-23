using OpenTK.Input;
using System;

namespace OpenBve.Input
{
	/// <summary>Represents a standard joystick, handled via openTK</summary>
		internal class StandardJoystick : AbstractJoystick
		{
			private JoystickState state;
			private readonly Guid Guid;

		internal StandardJoystick(int joystickHandle)
		{
			Handle = joystickHandle;
			Name = Joystick.GetName(Handle);
			if (Name.IndexOf("mascon", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				ConfigurationLink = ConfigurationLink.DenshaDeGo;
			}
			Guid = Joystick.GetGuid(Handle);
		}

		internal override ButtonState GetButton(int button)
		{
			return state.GetButton(button);
		}

		internal override double GetAxis(int axis)
		{
			return state.GetAxis(axis);
		}

		internal override JoystickHatState GetHat(int Hat)
		{
			return state.GetHat((JoystickHat)Hat);
		}

		internal override int AxisCount()
		{
			return Joystick.GetCapabilities(Handle).AxisCount;
		}

		internal override int ButtonCount()
		{
			return Joystick.GetCapabilities(Handle).ButtonCount;
		}

		internal override int HatCount()
		{
			return Joystick.GetCapabilities(Handle).HatCount;
		}

		internal override void Poll()
		{
			state = Joystick.GetState(Handle);
		}

		internal override bool IsConnected()
		{
			return Joystick.GetCapabilities(Handle).IsConnected;
		}
		
		internal override Guid GetGuid()
		{
			return Guid;
		}

		public override string ToString()
		{
			return "GUID: " + Guid + ", Name: " + Name + ", Buttons: " + ButtonCount() + ", Axis: " + AxisCount() + ", Hats: " + HatCount();
		}
	}
}
