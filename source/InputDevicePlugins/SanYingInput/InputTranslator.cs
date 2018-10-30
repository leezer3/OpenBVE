//The MIT License (MIT)
//
//Copyright (c) 2017 Rock_On, 2018 The openBVE Project
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of
//this software and associated documentation files (the "Software"), to deal in
//the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
//the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using OpenTK.Input;

namespace SanYingInput
{
	/// <summary>
	/// The class which converts input of the joy-stick
	/// </summary>
	internal static class InputTranslator
	{
		/// <summary>
		/// Bit flag enumeration type to express each button state of BT7-10
		/// </summary>
		internal enum BT7_10_Pressed
		{
			/// <summary>
			/// The state that none of buttons are pressed
			/// </summary>
			None = 0x0,

			/// <summary>
			/// The state that BT7 is pressed
			/// </summary>
			BT7 = 0x8,

			/// <summary>
			/// The state that BT8 is pressed
			/// </summary>
			BT8 = 0x4,

			/// <summary>
			/// The state that BT9 is pressed
			/// </summary>
			BT9 = 0x2,

			/// <summary>
			/// The state that BT10 is pressed
			/// </summary>
			BT10 = 0x1
		};

		/// <summary>
		/// Dictionary that stores the relations with the position of the notch and bit flag.
		/// </summary>
		private static Dictionary<uint, int> _bitPatternMap = new Dictionary<uint, int>();

		/// <summary>
		/// Static constructer
		/// </summary>
		static InputTranslator()
		{
			_bitPatternMap.Add(0xF, 5);
			_bitPatternMap.Add(0xE, 4);
			_bitPatternMap.Add(0xD, 3);
			_bitPatternMap.Add(0xC, 2);
			_bitPatternMap.Add(0xB, 1);

			_bitPatternMap.Add(0xA, 0);

			_bitPatternMap.Add(0x9, -1);
			_bitPatternMap.Add(0x8, -2);
			_bitPatternMap.Add(0x7, -3);
			_bitPatternMap.Add(0x6, -4);
			_bitPatternMap.Add(0x5, -5);
			_bitPatternMap.Add(0x4, -6);
			_bitPatternMap.Add(0x3, -7);
			_bitPatternMap.Add(0x2, -8);
			_bitPatternMap.Add(0x1, -9);
		}

		/// <summary>
		/// Function that convert into the position of the notch from the state of the button of the joy-stick.
		/// </summary>
		/// <param name="buttonsState">State of the button of the joy-stick</param>
		/// <param name="notchPosition">Position of the notch</param>
		/// <returns>Middle position or not</returns>
		public static bool TranslateNotchPosition(List<ButtonState> buttonsState, out int notchPosition)
		{
			uint notchButtonsState;
			MakeBitFromNotchButtons(buttonsState, out notchButtonsState);

			if (_bitPatternMap.ContainsKey(notchButtonsState))
			{
				notchPosition = _bitPatternMap[notchButtonsState];
				return false;
			}
			else
			{
				notchPosition = 0;
				return true;
			}
		}

		/// <summary>
		/// Function that makes a bit flag expressing the state of the button about notch from the state of the button of the joy-stick.
		/// </summary>
		/// <param name="buttonsState">State of the button of the joy-stick</param>
		/// <param name="notchButtonsState">The bit flag which expresses the state of the button about notch</param>
		public static void MakeBitFromNotchButtons(List<ButtonState> buttonsState, out uint notchButtonsState)
		{
			notchButtonsState = (uint)BT7_10_Pressed.None;

			if (buttonsState.Count < 10)
			{
				return;
			}

			if (buttonsState[6] == ButtonState.Pressed)
			{
				notchButtonsState |= (uint)BT7_10_Pressed.BT7;
			}
			if (buttonsState[7] == ButtonState.Pressed)
			{
				notchButtonsState |= (uint)BT7_10_Pressed.BT8;
			}
			if (buttonsState[8] == ButtonState.Pressed)
			{
				notchButtonsState |= (uint)BT7_10_Pressed.BT9;
			}
			if (buttonsState[9] == ButtonState.Pressed)
			{
				notchButtonsState |= (uint)BT7_10_Pressed.BT10;
			}
		}

		/// <summary>
		/// Funtction that convert into the position of reverser from state of axial of the joy-stick.
		/// </summary>
		/// <param name="axises">State of axial of the joy-stick</param>
		/// <param name="reverserPosition">Position of reverser</param>
		public static void TranslateReverserPosition(List<double> axises, out int reverserPosition)
		{
			reverserPosition = 0;

			if (axises.Count < 2)
			{
				return;
			}

			if (axises[1] > 0.5)
			{
				reverserPosition = -1;
			}
			else if (axises[1] < -0.5)
			{
				reverserPosition = 1;
			}
		}
	}
}
