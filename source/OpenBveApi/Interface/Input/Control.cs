using System;
using OpenBveApi.Input;

namespace OpenBveApi.Interface
{
	/// <summary>Information on an in-game control</summary>
	public struct Control
	{
		/// <summary>The public command which this control performs</summary>
		public Translations.Command Command;
		/// <summary>The command type</summary>
		public Translations.CommandType InheritedType;
		/// <summary>The method of control</summary>
		public ControlMethod Method;
		/// <summary>Any keyboard modifiers used</summary>
		public KeyboardModifier Modifier;
		/// <summary>The GUID of the device which activates this control</summary>
		public Guid Device;
		/// <summary>The joystick component which activates this control (if joystick)</summary>
		public JoystickComponent Component;
		/// <summary>The joystick axis / hat number</summary>
		public int Element;
		/// <summary>The joystick movement direction</summary>
		public int Direction;
		/// <summary>The simulation state of the control</summary>
		public DigitalControlState DigitalState;
		/// <summary>The analog state of the control</summary>
		public double AnalogState;
		/// <summary>The keyboard key (if keyboard activated)</summary>
		public Key Key;
		/// <summary>The last state of the control</summary>
		public string LastState;
		/// <summary>The option applied to this control</summary>
		public int Option;

		/// <summary>Returns the control's representation in string format</summary>
		public override string ToString()
		{
			string s = Method + ", ";
			switch (Method)
			{
				case ControlMethod.Keyboard:
					s += Key + ", " + (int)Modifier + ", " + Option;
					break;
				case ControlMethod.Joystick:
					s += Device + ", " + Component + ", " + Element;
					if (Component != JoystickComponent.Button)
					{
						s += ", " + Direction;
					}
					s += ", " + Option;
					break;
				case ControlMethod.RailDriver:
					s += "0, " + Component + ", " + Element;
					if (Component != JoystickComponent.Button)
					{
						s += ", " + Direction;
					}
					s += ", " + Option;
					break;
			}
			return s;
		}
	}
}
