//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020-2021, Marc Riera, The OpenBVE Project
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
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class representing a PlayStation 2 controller
	/// </summary>
	internal class Ps2Controller : Controller
	{
		/// <summary>A cached list of supported connected controllers.</summary>
		private static Dictionary<Guid, Controller> cachedControllers = new Dictionary<Guid, Controller>();

		private static string[] controllerIds =
		{
			// TCPP-20009 (Type II)
			"0ae4:0004",
			// TCPP-20011 (Shinkansen)
			"0ae4:0005",
			// TCPP-20014 (Ryojouhen)
			"0ae4:0007"
		};

		/// <summary>The min/max byte for each brake notch, from Released to Emergency. Each notch consists of two bytes.</summary>
		private readonly byte[] brakeBytes;

		/// <summary>The min/max byte for each power notch, from Released to maximum. Each notch consists of two bytes.</summary>
		private readonly byte[] powerBytes;

		/// <summary>The button mask for the buttons. Follows order in InputTranslator.</summary>
		private readonly byte[] buttonMask;

		/// <summary>An array with raw input data from the controller.</summary>
		private byte[] inputBuffer;

		/// <summary>An array with raw output data for the controller.</summary>
		private byte[] outputBuffer;

		/// <summary>
		/// Initializes an Unbalance controller.
		/// </summary>
		internal Ps2Controller(ControllerButtons buttons, byte[] buttonBytes, byte[] brake, byte[] power)
		{
			ControllerName = string.Empty;
			IsConnected = false;
			RequiresCalibration = false;
			BrakeNotches = brake.Length / 2 - 2;
			PowerNotches = power.Length / 2 - 1;
			brakeBytes = brake;
			powerBytes = power;
			Buttons = buttons;
			buttonMask = buttonBytes;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal override void ReadInput()
		{
			// Sync input/output data
			LibUsb.SyncController(Guid, inputBuffer, outputBuffer);

			byte brakeData;
			byte powerData;
			byte buttonData;
			byte dpadData;
			byte pedalData;
			switch (Id)
			{
				// TCPP-20009 (Type II)
				case "0ae4:0004":
					brakeData = inputBuffer[1];
					powerData = inputBuffer[2];
					buttonData = inputBuffer[5];
					dpadData = inputBuffer[4];
					pedalData = inputBuffer[3];
					break;
				// TCPP-20011 (Shinkansen)
				// TCPP-20014 (Ryojouhen)
				default:
					brakeData = inputBuffer[0];
					powerData = inputBuffer[1];
					buttonData = inputBuffer[4];
					dpadData = inputBuffer[3];
					pedalData = inputBuffer[2];
					break;
			}

			for (int i = 0; i < brakeBytes.Length; i+=2)
			{
				// Each notch uses two bytes, minimum value and maximum value
				if (brakeData >= brakeBytes[i] && brakeData <= brakeBytes[i + 1])
				{
					if (brakeBytes.Length == i + 2)
					{
						// Last notch should be Emergency
						InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Emergency;
					}
					else
					{
						// Regular brake notch
						InputTranslator.BrakeNotch = (InputTranslator.BrakeNotches)(i / 2);
					}
					break;
				}
			}
			for (int i = 0; i < powerBytes.Length; i+=2)
			{
				// Each notch uses two bytes, minimum value and maximum value
				if (powerData >= powerBytes[i] && powerData <= powerBytes[i + 1])
				{
					InputTranslator.PowerNotch = (InputTranslator.PowerNotches)(i / 2);
					break;
				}
			}

			// Standard buttons
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Select]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Start]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.A]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.B]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.C]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.D]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.LDoor] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.LDoor]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.RDoor] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.RDoor]) != 0 ? ButtonState.Pressed : ButtonState.Released;

			// D-pad
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (dpadData <= 1 || dpadData == 7) ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (dpadData >= 1 && dpadData <= 3) ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (dpadData >= 3 && dpadData <= 5) ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (dpadData >= 5 && dpadData <= 7) ? ButtonState.Pressed : ButtonState.Released;

			// Horn pedal
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Pedal] = pedalData == 0x0 ? ButtonState.Pressed : ButtonState.Released;

			switch (Id)
			{
				// TCPP-20009 (Type II)
				case "0ae4:0004":
					outputBuffer = new byte[] { 0x0, 0x3 };
					if (DenshaDeGoInput.Ingame)
					{
						// Door lamp
						outputBuffer[1] = (byte)(DenshaDeGoInput.TrainDoorsClosed ? 1 : 0);
					}
					break;
				// TCPP-20011 (Shinkansen)
				case "0ae4:0005":
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
					outputBuffer = new byte[] { 0x0, 0x0, 0x0, 0x0, 0xFF, 0xFF, 0xFF, 0xFF };
					if (DenshaDeGoInput.Ingame)
					{
						if (DenshaDeGoInput.CurrentSpeedLimit >= 0 && DenshaDeGoInput.ATCSection)
						{
							// Door lamp + limit approach
							outputBuffer[2] = (byte)((128 * (DenshaDeGoInput.TrainDoorsClosed ? 1 : 0)) + limit_approach);
							// Route limit
							outputBuffer[6] = (byte)(16 * limit2 + limit1);
							outputBuffer[7] = (byte)limit3;
						}
						else
						{
							// Door lamp
							outputBuffer[2] = (byte)(128 * (DenshaDeGoInput.TrainDoorsClosed ? 1 : 0));
						}

						// Speed gauge
						outputBuffer[3] = (byte)Math.Ceiling(Math.Round(DenshaDeGoInput.CurrentTrainSpeed) / 15);
						// Train speed
						outputBuffer[4] = (byte)(16 * speed2 + speed1);
						outputBuffer[5] = (byte)speed3;
					}
					break;
			}
		}

		/// <summary>
		/// Configures the supported controllers with LibUsb.
		/// </summary>
		internal static void ConfigureControllers()
		{
			LibUsb.AddSupportedControllers(controllerIds);
		}

		/// <summary>
		/// Gets the list of connected controllers
		/// </summary>
		/// <returns>The list of controllers handled by this class.</returns>
		internal static Dictionary<Guid, Controller> GetControllers()
		{
			foreach (KeyValuePair<Guid, LibUsb.UsbController> usbController in LibUsb.GetSupportedControllers())
			{
				Guid guid = usbController.Key;
				string id = GetControllerID(guid);
				string name = usbController.Value.ControllerName;

				if (!cachedControllers.ContainsKey(guid))
				{
					// TCPP-20009 (Type II)
					if (id == "0ae4:0004")
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.Pedal | ControllerButtons.DPad;
						byte[] buttonBytes = { 0x10, 0x20, 0x2, 0x1, 0x4, 0x8, 0x0, 0x0 };
						byte[] brakeBytes = { 0x79, 0x79, 0x8A, 0x8A, 0x94, 0x94, 0x9A, 0x9A, 0xA2, 0xA2, 0xA8, 0xA8, 0xAF, 0xAF, 0xB2, 0xB2, 0xB5, 0xB5, 0xB9, 0xB9 };
						byte[] powerBytes = { 0x81, 0x81, 0x6D, 0x6D, 0x54, 0x54, 0x3F, 0x3F, 0x21, 0x21, 0x00, 0x00 };
						Ps2Controller newcontroller = new Ps2Controller(buttons, buttonBytes, brakeBytes, powerBytes)
						{
							// 6 bytes for input, 2 for output
							Guid = guid,
							Id = id,
							ControllerName = name,
							inputBuffer = new byte[] { 0x1, 0x0, 0x0, 0xFF, 0x8, 0x0 },
							outputBuffer = new byte[] { 0x0, 0x3 }
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// TCPP-20011 (Shinkansen)
					if (id == "0ae4:0005")
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.Pedal | ControllerButtons.DPad;
						byte[] buttonBytes = { 0x10, 0x20, 0x8, 0x4, 0x2, 0x1, 0x0, 0x0 };
						byte[] brakeBytes = { 0x1C, 0x1C, 0x38, 0x38, 0x54, 0x54, 0x70, 0x70, 0x8B, 0x8B, 0xA7, 0xA7, 0xC3, 0xC3, 0xDF, 0xDF, 0xFB, 0xFB };
						byte[] powerBytes = { 0x12, 0x12, 0x24, 0x24, 0x36, 0x36, 0x48, 0x48, 0x5A, 0x5A, 0x6C, 0x6C, 0x7E, 0x7E, 0x90, 0x90, 0xA2, 0xA2, 0xB4, 0xB4, 0xC6, 0xC6, 0xD7, 0xD7, 0xE9, 0xE9, 0xFB, 0xFB };
						Ps2Controller newcontroller = new Ps2Controller(buttons, buttonBytes, brakeBytes, powerBytes)
						{
							// 6 bytes for input, 8 for output
							Guid = guid,
							Id = id,
							ControllerName = name,
							inputBuffer = new byte[] { 0x0, 0x0, 0xFF, 0x8, 0x0, 0x0 },
							outputBuffer = new byte[] { 0x0, 0x0, 0x0, 0x0, 0xFF, 0xFF, 0xFF, 0xFF }
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// TCPP-20014 (Ryojouhen)
					if (id == "0ae4:0007")
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.Pedal | ControllerButtons.LDoor | ControllerButtons.RDoor | ControllerButtons.DPad;
						byte[] buttonBytes = { 0x20, 0x40, 0x4, 0x2, 0x1, 0x0, 0x10, 0x8 };
						byte[] brakeBytes = { 0x23, 0x2C, 0x2D, 0x3E, 0x3F, 0x4E, 0x4F, 0x63, 0x64, 0x8A, 0x8B, 0xB0, 0xB1, 0xD4, 0xD5, 0xDF };
						byte[] powerBytes = { 0x0, 0x0, 0x3C, 0x3C, 0x78, 0x78, 0xB4, 0xB4, 0xF0, 0xF0 };
						Ps2Controller newcontroller = new Ps2Controller(buttons, buttonBytes, brakeBytes, powerBytes)
						{
							// 8 bytes for input, no output
							Guid = guid,
							Id = id,
							ControllerName = name,
							inputBuffer = new byte[] { 0x0, 0x0, 0xFF, 0x8, 0x0, 0x0, 0x0, 0x0 },
							outputBuffer = new byte[0]
						};
						cachedControllers.Add(guid, newcontroller);
					}
				}

				// Update connection status and name
				cachedControllers[guid].IsConnected = usbController.Value.IsConnected;
				cachedControllers[guid].ControllerName = name;
			}
			return cachedControllers;
		}
	}
}
