using OpenTK.Input;
using System;

namespace OpenBve.Input
{
	/// <summary>Represents a joystick.</summary>
	internal abstract class AbstractJoystick
	{
		/// <summary>The textual representation of the joystick.</summary>
		internal string Name;
		/// <summary>The handle to the joystick.</summary>
		internal int Handle;
		
		internal byte[] currentState = new byte[15];
		
		internal abstract ButtonState GetButton(int button);
		
		internal abstract double GetAxis(int axis);
		
		internal abstract JoystickHatState GetHat(int Hat);

		internal abstract int AxisCount();

		internal abstract int ButtonCount();

		internal abstract int HatCount();

		internal abstract void Poll();

		internal abstract bool IsConnected();

		internal bool Disconnected = false;

		internal abstract Guid GetGuid();
	}
}
