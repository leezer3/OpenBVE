namespace OpenBveApi.Interface
{
	/// <summary>The method by which a control is activated</summary>
	public enum ControlMethod
	{
		/// <summary>This control is invalid</summary>
		Invalid = 0,
		/// <summary>This control is activated using a keyboard button</summary>
		Keyboard = 1,
		/// <summary>This control is activated using a joystick axis or button</summary>
		Joystick = 2,
		/// <summary>This control is activated using the RailDriver cab controller</summary>
		RailDriver = 3,
		/// <summary>This control is activated using the Input Device Plugin</summary>
		InputDevicePlugin = 4,
		/// <summary>This control is activated using a touch element</summary>
		Touch = 5
	}
}
