namespace OpenBveApi.Interface
{
	/// <summary>The joystick component used to activate a control</summary>
	public enum JoystickComponent
	{
		/// <summary>The joystick component is invalid</summary>
		Invalid, 
		/// <summary>A half-axis range is used</summary>
		Axis, 
		/// <summary>A full axis range is used</summary>
		FullAxis, 
		/// <summary>The joystick ball</summary>
		Ball, 
		/// <summary>A joystick hat direction</summary>
		Hat, 
		/// <summary>A joystick button</summary>
		Button
	}
}
