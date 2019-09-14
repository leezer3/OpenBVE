using OpenTK.Input;

namespace OpenBve
{
	/// <summary>Represents a joystick.</summary>
	internal abstract class Joystick
	{
		// --- members ---
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

		internal virtual void SetDisplay(int speed)
		{ }
	}
}
