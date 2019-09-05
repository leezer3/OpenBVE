using System;

namespace TrainEditor2.Models.Others
{
	internal class InputEventModel
	{
		internal enum ButtonState
		{
			Released,
			Pressed
		}

		[Flags]
		internal enum ModifierKeys
		{
			None = 0,
			Alt = 1,
			Control = 2,
			Shift = 4,
			Windows = 8
		}

		internal enum CursorType
		{
			None,
			No,
			Arrow,
			AppStarting,
			Cross,
			Help,
			IBeam,
			SizeAll,
			SizeNESW,
			SizeNS,
			SizeNWSE,
			SizeWE,
			UpArrow,
			Wait,
			Hand,
			Pen,
			ScrollNS,
			ScrollWE,
			ScrollAll,
			ScrollN,
			ScrollS,
			ScrollW,
			ScrollE,
			ScrollNW,
			ScrollNE,
			ScrollSW,
			ScrollSE,
			ArrowCD
		}

		internal class EventArgs
		{
			internal ButtonState LeftButton;
			internal ButtonState MiddleButton;
			internal ButtonState RightButton;
			internal ButtonState XButton1;
			internal ButtonState XButton2;
			internal double X;
			internal double Y;
		}
	}
}
