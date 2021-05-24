//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2021, Marc Riera, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class for USB Densha de GO! controllers for the Sony Playstation 2.
	/// </summary>
	public partial class ControllerPs2
	{
		/// <summary>
		/// The number of brake notches, excluding the emergency brake.
		/// </summary>
		internal static int ControllerBrakeNotches;

		/// <summary>
		/// The number of power notches.
		/// </summary>
		internal static int ControllerPowerNotches;
		
		/// <summary>
		/// The raw data read from the USB controller.
		/// </summary>
		internal static byte[] readBuffer = new byte[6];

		/// <summary>
		/// The raw data to be written to the USB controller.
		/// </summary>
		internal static byte[] writeBuffer;

		/// <summary>
		/// Whether the controller display or door lamp is enabled.
		/// </summary>
		internal static bool ControllerDisplayEnabled;

		/// <summary>
		/// Represents the bytes for each button.
		/// </summary>
		[Flags]
		private enum ButtonByte
		{
			None = 0x0,
			Select = 0x10,
			Start = 0x20,
			A = 0x2,
			B = 0x1,
			C = 0x4,
			D = 0x8
		}

		/// <summary>
		/// Represents the bytes for each button (Shinkansen controller).
		/// </summary>
		[Flags]
		private enum ButtonByteShinkansen
		{
			None = 0x0,
			Select = 0x10,
			Start = 0x20,
			A = 0x8,
			B = 0x4,
			C = 0x2,
			D = 0x1
		}

		/// <summary>
		/// Represents the possible bytes for the pedal.
		/// </summary>
		private enum PedalBytes
		{
			Released = 0xFF,
			Pressed = 0x0
		}

		/// <summary>
		/// Represents the possible bytes for brake notches.
		/// </summary>
		private enum BrakeByte
		{
			Released = 0x79,
			B1 = 0x8A,
			B2 = 0x94,
			B3 = 0x9A,
			B4 = 0xA2,
			B5 = 0xA8,
			B6 = 0xAF,
			B7 = 0xB2,
			B8 = 0xB5,
			Emergency = 0xB9,
			Transition = 0xFF
		}

		/// <summary>
		/// Represents the possible bytes for brake notches (Shinkansen controller).
		/// </summary>
		private enum BrakeByteShinkansen
		{
			Released = 0x1C,
			B1 = 0x38,
			B2 = 0x54,
			B3 = 0x70,
			B4 = 0x8B,
			B5 = 0xA7,
			B6 = 0xC3,
			B7 = 0xDF,
			Emergency = 0xFB,
			Transition = 0xFF
		}

		/// <summary>
		/// Represents the possible bytes for power notches.
		/// </summary>
		private enum PowerByte
		{
			N = 0x81,
			P1 = 0x6D,
			P2 = 0x54,
			P3 = 0x3F,
			P4 = 0x21,
			P5 = 0x00,
			Transition = 0xFF
		}

		/// <summary>
		/// Represents the possible bytes for power notches (Shinkansen controller).
		/// </summary>
		private enum PowerByteShinkansen
		{
			N = 0x12,
			P1 = 0x24,
			P2 = 0x36,
			P3 = 0x48,
			P4 = 0x5A,
			P5 = 0x6C,
			P6 = 0x7E,
			P7 = 0x90,
			P8 = 0xA2,
			P9 = 0xB4,
			P10 = 0xC6,
			P11 = 0xD7,
			P12 = 0xE9,
			P13 = 0xFB,
			Transition = 0xFF
		}

		/// <summary>
		/// Dictionary storing the mapping of each brake notch.
		/// </summary>
		private static readonly Dictionary<BrakeByte, InputTranslator.BrakeNotches> BrakeNotchMap = new Dictionary<BrakeByte, InputTranslator.BrakeNotches>
		{
			{ BrakeByte.Released, InputTranslator.BrakeNotches.Released },
			{ BrakeByte.B1, InputTranslator.BrakeNotches.B1 },
			{ BrakeByte.B2, InputTranslator.BrakeNotches.B2 },
			{ BrakeByte.B3, InputTranslator.BrakeNotches.B3 },
			{ BrakeByte.B4, InputTranslator.BrakeNotches.B4 },
			{ BrakeByte.B5, InputTranslator.BrakeNotches.B5 },
			{ BrakeByte.B6, InputTranslator.BrakeNotches.B6 },
			{ BrakeByte.B7, InputTranslator.BrakeNotches.B7 },
			{ BrakeByte.B8, InputTranslator.BrakeNotches.B8 },
			{ BrakeByte.Emergency, InputTranslator.BrakeNotches.Emergency }
		};

		/// <summary>
		/// Dictionary storing the mapping of each brake notch (Shinkansen controller.
		/// </summary>
		private static readonly Dictionary<BrakeByteShinkansen, InputTranslator.BrakeNotches> BrakeNotchMapShinkansen = new Dictionary<BrakeByteShinkansen, InputTranslator.BrakeNotches>
		{
			{ BrakeByteShinkansen.Released, InputTranslator.BrakeNotches.Released },
			{ BrakeByteShinkansen.B1, InputTranslator.BrakeNotches.B1 },
			{ BrakeByteShinkansen.B2, InputTranslator.BrakeNotches.B2 },
			{ BrakeByteShinkansen.B3, InputTranslator.BrakeNotches.B3 },
			{ BrakeByteShinkansen.B4, InputTranslator.BrakeNotches.B4 },
			{ BrakeByteShinkansen.B5, InputTranslator.BrakeNotches.B5 },
			{ BrakeByteShinkansen.B6, InputTranslator.BrakeNotches.B6 },
			{ BrakeByteShinkansen.B7, InputTranslator.BrakeNotches.B7 },
			{ BrakeByteShinkansen.Emergency, InputTranslator.BrakeNotches.Emergency }
		};

		/// <summary>
		/// Dictionary storing the mapping of each power notch.
		/// </summary>
		private static readonly Dictionary<PowerByte, InputTranslator.PowerNotches> PowerNotchMap = new Dictionary<PowerByte, InputTranslator.PowerNotches>
		{
			{ PowerByte.N, InputTranslator.PowerNotches.N },
			{ PowerByte.P1, InputTranslator.PowerNotches.P1 },
			{ PowerByte.P2, InputTranslator.PowerNotches.P2 },
			{ PowerByte.P3, InputTranslator.PowerNotches.P3 },
			{ PowerByte.P4, InputTranslator.PowerNotches.P4 },
			{ PowerByte.P5, InputTranslator.PowerNotches.P5 }
		};

		/// <summary>
		/// Dictionary storing the mapping of each power notch.
		/// </summary>
		private static readonly Dictionary<PowerByteShinkansen, InputTranslator.PowerNotches> PowerNotchMapShinkansen = new Dictionary<PowerByteShinkansen, InputTranslator.PowerNotches>
		{
			{ PowerByteShinkansen.N, InputTranslator.PowerNotches.N },
			{ PowerByteShinkansen.P1, InputTranslator.PowerNotches.P1 },
			{ PowerByteShinkansen.P2, InputTranslator.PowerNotches.P2 },
			{ PowerByteShinkansen.P3, InputTranslator.PowerNotches.P3 },
			{ PowerByteShinkansen.P4, InputTranslator.PowerNotches.P4 },
			{ PowerByteShinkansen.P5, InputTranslator.PowerNotches.P5 },
			{ PowerByteShinkansen.P6, InputTranslator.PowerNotches.P6 },
			{ PowerByteShinkansen.P7, InputTranslator.PowerNotches.P7 },
			{ PowerByteShinkansen.P8, InputTranslator.PowerNotches.P8 },
			{ PowerByteShinkansen.P9, InputTranslator.PowerNotches.P9 },
			{ PowerByteShinkansen.P10, InputTranslator.PowerNotches.P10 },
			{ PowerByteShinkansen.P11, InputTranslator.PowerNotches.P11 },
			{ PowerByteShinkansen.P12, InputTranslator.PowerNotches.P12 },
			{ PowerByteShinkansen.P13, InputTranslator.PowerNotches.P13 }
		};

		/// <summary>
		/// Checks the controller model.
		/// </summary>
		/// <param name="id">A string representing the vendor and product ID.</param>
		/// <returns>The controller model.</returns>
		internal static InputTranslator.ControllerModels GetControllerModel(string id)
		{
			// Type 2
			if (id == "0ae4:0004")
			{
				ControllerBrakeNotches = 8;
				ControllerPowerNotches = 5;
				return InputTranslator.ControllerModels.Ps2Type2;
			}
			// Shinkansen
			if (id == "0ae4:0005")
			{
				ControllerBrakeNotches = 7;
				ControllerPowerNotches = 13;
				return InputTranslator.ControllerModels.Ps2Shinkansen;
			}
			return InputTranslator.ControllerModels.Unsupported;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal static void ReadInput()
		{
			// If running in-game, always enable the display
			if (DenshaDeGoInput.Ingame)
			{
				ControllerDisplayEnabled = true;
			}
			
			byte brakeByte;
			byte powerByte;
			switch (InputTranslator.ControllerModel)
			{
				case InputTranslator.ControllerModels.Ps2Type2:
					brakeByte = readBuffer[1];
					powerByte = readBuffer[2];
					if (BrakeNotchMap.ContainsKey((BrakeByte)brakeByte))
					{
						// Change the brake notch if the brake byte is valid
						InputTranslator.BrakeNotch = BrakeNotchMap[(BrakeByte)brakeByte];
					}
					if (PowerNotchMap.ContainsKey((PowerByte)powerByte))
					{
						// Change the power notch if the power byte is valid
						InputTranslator.PowerNotch = PowerNotchMap[(PowerByte)powerByte];
					}

					// Standard buttons
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = (readBuffer[5] & (byte)ButtonByteShinkansen.Select) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = (readBuffer[5] & (byte)ButtonByteShinkansen.Start) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = (readBuffer[5] & (byte)ButtonByteShinkansen.A) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = (readBuffer[5] & (byte)ButtonByteShinkansen.B) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = (readBuffer[5] & (byte)ButtonByteShinkansen.C) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = (readBuffer[5] & (byte)ButtonByteShinkansen.D) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;

					// Direction buttons
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (readBuffer[4] <= 1 || readBuffer[4] == 7) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (readBuffer[4] >= 1 && readBuffer[4] <= 3) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (readBuffer[4] >= 3 && readBuffer[4] <= 5) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (readBuffer[4] >= 5 && readBuffer[4] <= 7) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;

					// Horn pedal
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Pedal] = readBuffer[3] == (byte)PedalBytes.Pressed ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;

					// Specially crafted array that turns off the door lamp
					writeBuffer = new byte[]{ 0x0, 0x3 };
					if (ControllerDisplayEnabled)
					{
						// Door lamp
						writeBuffer[1] = (byte)(DenshaDeGoInput.TrainDoorsClosed ? 1 : 0);
					}
					break;
				case InputTranslator.ControllerModels.Ps2Shinkansen:
					brakeByte = readBuffer[0];
					powerByte = readBuffer[1];
					if (BrakeNotchMapShinkansen.ContainsKey((BrakeByteShinkansen)brakeByte))
					{
						// Change the brake notch if the brake byte is valid
						InputTranslator.BrakeNotch = BrakeNotchMapShinkansen[(BrakeByteShinkansen)brakeByte];
					}
					if (PowerNotchMapShinkansen.ContainsKey((PowerByteShinkansen)powerByte))
					{
						// Change the power notch if the power byte is valid
						InputTranslator.PowerNotch = PowerNotchMapShinkansen[(PowerByteShinkansen)powerByte];
					}

					// Standard buttons
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = (readBuffer[4] & (byte)ButtonByteShinkansen.Select) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = (readBuffer[4] & (byte)ButtonByteShinkansen.Start) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = (readBuffer[4] & (byte)ButtonByteShinkansen.A) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = (readBuffer[4] & (byte)ButtonByteShinkansen.B) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = (readBuffer[4] & (byte)ButtonByteShinkansen.C) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = (readBuffer[4] & (byte)ButtonByteShinkansen.D) != (byte)ButtonByteShinkansen.None ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;

					// Direction buttons
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (readBuffer[3] <= 1 || readBuffer[3] == 7) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (readBuffer[3] >= 1 && readBuffer[3] <= 3) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (readBuffer[3] >= 3 && readBuffer[3] <= 5) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (readBuffer[3] >= 5 && readBuffer[3] <= 7) ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;

					// Horn pedal
					InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Pedal] = readBuffer[2] == (byte)PedalBytes.Pressed ? OpenTK.Input.ButtonState.Pressed : OpenTK.Input.ButtonState.Released;

					double speed = Math.Round(DenshaDeGoInput.CurrentTrainSpeed, 0);
					double limit = Math.Round(DenshaDeGoInput.CurrentSpeedLimit, 0);
					int speed1 = (int)(speed % 10);
					int speed2 = (int)(speed % 100 / 10);
					int speed3 = (int)(speed % 1000 / 100);
					int limit1 = (int)(limit % 10);
					int limit2 = (int)(limit % 100 / 10);
					int limit3 = (int)(limit % 1000 / 100);
					int limit_approach = 0;
					if (speed >= limit)
					{
						limit_approach = 10;
					}
					else if (speed > limit - 10)
					{
						limit_approach = -(int)(limit - speed - 10);
					}
					// Specially crafted array that blanks the display
					writeBuffer = new byte[]{ 0x0, 0x0, 0x0, 0x0, 0xFF, 0xFF, 0xFF, 0xFF };
					if (ControllerDisplayEnabled)
					{
						if (DenshaDeGoInput.CurrentSpeedLimit >= 0 && DenshaDeGoInput.ATCSection)
						{
							// Door lamp + limit approach
							writeBuffer[2] = (byte)((128 * (DenshaDeGoInput.TrainDoorsClosed ? 1 : 0)) + limit_approach);
							// Route limit
							writeBuffer[6] = (byte)(16 * limit2 + limit1);
							writeBuffer[7] = (byte)limit3;
						}
						else
						{
							// Door lamp
							writeBuffer[2] = (byte)(128 * (DenshaDeGoInput.TrainDoorsClosed ? 1 : 0));
						}

						// Speed gauge
						writeBuffer[3] = (byte)Math.Ceiling(Math.Round(DenshaDeGoInput.CurrentTrainSpeed) / 15);
						// Train speed
						writeBuffer[4] = (byte)(16 * speed2 + speed1);
						writeBuffer[5] = (byte)speed3;
					}
					break;
			}

		}
	}
}
