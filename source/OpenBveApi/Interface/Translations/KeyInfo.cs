using OpenTK.Input;

namespace OpenBveApi.Interface
{
	public partial class Translations
	{
		/// <summary>Defines an available keyboard key</summary>
		public struct KeyInfo
		{
			/// <summary>The key (OpenTK.Input.Key enum member)</summary>
			public readonly Key Key;
			/// <summary>The internal key name</summary>
			internal readonly string Name;
			/// <summary>The translated key description</summary>
			public string Description;
			/// <summary>Creates a new translated key</summary>
			internal KeyInfo(Key Key, string Name)
			{
				this.Key = Key;
				this.Name = Name;
				this.Description = Name;
			}

			/// <returns>ToString for a key binding returns the binding description in textual format, NOT the phyisical key</returns>
			public override string ToString()
			{
				return Description;
			}
		
		
		}

		/// <summary>Holds the translations for all keys available for assignation</summary>
		public static KeyInfo[] TranslatedKeys = {
			new KeyInfo(Key.Number0, "0"),
			new KeyInfo(Key.Number1, "1"),
			new KeyInfo(Key.Number2, "2"),
			new KeyInfo(Key.Number3, "3"),
			new KeyInfo(Key.Number4, "4"),
			new KeyInfo(Key.Number5, "5"),
			new KeyInfo(Key.Number6, "6"),
			new KeyInfo(Key.Number7, "7"),
			new KeyInfo(Key.Number8, "8"),
			new KeyInfo(Key.Number9, "9"),
			new KeyInfo(Key.BackSlash, "BACKSLASH"),
			new KeyInfo(Key.BackSpace, "BACKSPACE"),
			new KeyInfo(Key.CapsLock, "CAPSLOCK"),
			new KeyInfo(Key.Clear, "CLEAR"),
			new KeyInfo(Key.Comma, "COMMA"),
			new KeyInfo(Key.Delete, "DELETE"),
			new KeyInfo(Key.Down, "DOWN"),
			new KeyInfo(Key.End, "END"),
			new KeyInfo(Key.Enter, "ENTER"),
			new KeyInfo(Key.Escape, "ESCAPE"),
			new KeyInfo(Key.F1, "F1"),
			new KeyInfo(Key.F2, "F2"),
			new KeyInfo(Key.F3, "F3"),
			new KeyInfo(Key.F4, "F4"),
			new KeyInfo(Key.F5, "F5"),
			new KeyInfo(Key.F6, "F6"),
			new KeyInfo(Key.F7, "F7"),
			new KeyInfo(Key.F8, "F8"),
			new KeyInfo(Key.F9, "F9"),
			new KeyInfo(Key.F10, "F10"),
			new KeyInfo(Key.F11, "F11"),
			new KeyInfo(Key.F12, "F12"),
			new KeyInfo(Key.F13, "F13"),
			new KeyInfo(Key.F14, "F14"),
			new KeyInfo(Key.F15, "F15"),
			new KeyInfo(Key.F16, "F16"),
			new KeyInfo(Key.F17, "F17"),
			new KeyInfo(Key.F18, "F18"),
			new KeyInfo(Key.F19, "F19"),
			new KeyInfo(Key.F20, "F20"),
			new KeyInfo(Key.Home, "HOME"),
			new KeyInfo(Key.Insert, "INSERT"),
			new KeyInfo(Key.Keypad0, "KP0"),
			new KeyInfo(Key.Keypad1, "KP1"),
			new KeyInfo(Key.Keypad2, "KP2"),
			new KeyInfo(Key.Keypad3, "KP3"),
			new KeyInfo(Key.Keypad4, "KP4"),
			new KeyInfo(Key.Keypad5, "KP5"),
			new KeyInfo(Key.Keypad6, "KP6"),
			new KeyInfo(Key.Keypad7, "KP7"),
			new KeyInfo(Key.Keypad8, "KP8"),
			new KeyInfo(Key.Keypad9, "KP9"),
			new KeyInfo(Key.KeypadDivide, "KP_DIVIDE"),
			new KeyInfo(Key.KeypadEnter, "KP_ENTER"),
			new KeyInfo(Key.KeypadMinus, "KP_MINUS"),
			new KeyInfo(Key.KeypadMultiply, "KP_MULTIPLY"),
			new KeyInfo(Key.KeypadDecimal, "KP_PERIOD"),
			new KeyInfo(Key.KeypadPlus, "KP_PLUS"),
			new KeyInfo(Key.LAlt, "LALT"),
			new KeyInfo(Key.LControl, "LCTRL"),
			new KeyInfo(Key.Left, "LEFT"),
			new KeyInfo(Key.BracketLeft, "LEFTBRACKET"),
			new KeyInfo(Key.ShiftLeft, "LSHIFT"),
			new KeyInfo(Key.Menu, "MENU"),
			new KeyInfo(Key.Minus, "MINUS"),
			new KeyInfo(Key.AltLeft, "MODE"),
			new KeyInfo(Key.NumLock, "NUMLOCK"),
			new KeyInfo(Key.PageDown, "PAGEDOWN"),
			new KeyInfo(Key.PageUp, "PAGEUP"),
			new KeyInfo(Key.Pause, "PAUSE"),
			new KeyInfo(Key.Period, "PERIOD"),
			new KeyInfo(Key.Plus, "PLUS"),
			new KeyInfo(Key.PrintScreen, "PRINT"),
			new KeyInfo(Key.Quote, "QUOTE"),
			new KeyInfo(Key.RAlt, "RALT"),
			new KeyInfo(Key.RControl, "RCTRL"),
			new KeyInfo(Key.Right, "RIGHT"),
			new KeyInfo(Key.BracketRight, "RIGHTBRACKET"),
			new KeyInfo(Key.RShift, "RSHIFT"),
			new KeyInfo(Key.ScrollLock, "SCROLLLOCK"),
			new KeyInfo(Key.Semicolon, "SEMICOLON"),
			new KeyInfo(Key.Slash, "SLASH"),
			new KeyInfo(Key.Space, "SPACE"),
			new KeyInfo(Key.Tab, "TAB"),
			new KeyInfo(Key.Up, "UP"),
			new KeyInfo(Key.A, "a"),
			new KeyInfo(Key.B, "b"),
			new KeyInfo(Key.C, "c"),
			new KeyInfo(Key.D, "d"),
			new KeyInfo(Key.E, "e"),
			new KeyInfo(Key.F, "f"),
			new KeyInfo(Key.G, "g"),
			new KeyInfo(Key.H, "h"),
			new KeyInfo(Key.I, "i"),
			new KeyInfo(Key.J, "j"),
			new KeyInfo(Key.K, "k"),
			new KeyInfo(Key.L, "l"),
			new KeyInfo(Key.M, "m"),
			new KeyInfo(Key.N, "n"),
			new KeyInfo(Key.O, "o"),
			new KeyInfo(Key.P, "p"),
			new KeyInfo(Key.Q, "q"),
			new KeyInfo(Key.R, "r"),
			new KeyInfo(Key.S, "s"),
			new KeyInfo(Key.T, "t"),
			new KeyInfo(Key.U, "u"),
			new KeyInfo(Key.V, "v"),
			new KeyInfo(Key.W, "w"),
			new KeyInfo(Key.X, "x"),
			new KeyInfo(Key.Y, "y"),
			new KeyInfo(Key.Z, "z")
		};
	}
}
