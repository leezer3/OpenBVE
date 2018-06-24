using System;
using OpenTK.Input;

namespace OpenBve
{
    internal static partial class Interface
    {
        /// <summary>The method by which a control is activated</summary>
        internal enum ControlMethod
        {
            /// <summary>This control is invalid</summary>
            Invalid = 0,
            /// <summary>This control is activated using a keyboard button</summary>
            Keyboard = 1,
            /// <summary>This control is activated using a joystick axis or button</summary>
            Joystick = 2,
	        /// <summary>This control is activated using the RailDriver cab controller</summary>
			RailDriver = 3
        }

        /// <summary>Keyboard modifiers</summary>
        [Flags]
        internal enum KeyboardModifier
        {
            None = 0,
            Shift = 1,
            Ctrl = 2,
            Alt = 4
        }

        /// <summary>The joystick component which will activate this control</summary>
        internal enum JoystickComponent { Invalid, Axis, FullAxis, Ball, Hat, Button }

        /// <summary>The possible states of a digital control (button)</summary>
        internal enum DigitalControlState
        {
            ReleasedAcknowledged = 0,
            Released = 1,
            Pressed = 2,
            PressedAcknowledged = 3
        }

        /// <summary>Information on an in-game control</summary>
        internal struct Control
        {
            /// <summary>The internal command which this control performs</summary>
            internal Command Command;
            internal CommandType InheritedType;
            internal ControlMethod Method;
            internal KeyboardModifier Modifier;
            internal int Device;
            internal JoystickComponent Component;
            internal int Element;
            internal int Direction;
            internal DigitalControlState DigitalState;
            internal double AnalogState;
            internal Key Key;
            internal string LastState;
        }
    }
}
