using System;
using OpenTK.Input;

namespace OpenBve
{
	internal partial class JoystickManager
	{
		/// <summary>Represents a standard joystick, handled via openTK</summary>
		internal class StandardJoystick : Joystick
		{
			private JoystickState state;
			private readonly Guid Guid;

			internal StandardJoystick(int joystickHandle)
			{
				Handle = joystickHandle;
				Name = OpenTK.Input.Joystick.GetName(Handle);
				Guid = OpenTK.Input.Joystick.GetGuid(Handle);
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
				return OpenTK.Input.Joystick.GetCapabilities(Handle).AxisCount;
			}

			internal override int ButtonCount()
			{
				return OpenTK.Input.Joystick.GetCapabilities(Handle).ButtonCount;
			}

			internal override int HatCount()
			{
				return OpenTK.Input.Joystick.GetCapabilities(Handle).HatCount;
			}

			internal override void Poll()
			{
				state = OpenTK.Input.Joystick.GetState(Handle);
			}

			internal override bool IsConnected()
			{
				return OpenTK.Input.Joystick.GetCapabilities(Handle).IsConnected;
			}

			internal override Guid GetGuid()
			{
				return Guid;
			}
		}
	}
}
