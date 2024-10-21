//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020-2024, Marc Riera, The OpenBVE Project
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
	/// Class representing a PlayStation 2 Train Simulator controller
	/// </summary>
	internal class Ps2TSController : Controller
	{
		/// <summary>A cached list of supported connected controllers.</summary>
		private static readonly Dictionary<Guid, Controller> cachedControllers = new Dictionary<Guid, Controller>();

		private static readonly string[] controllerIds =
		{
			// SOTP-031201 (MTC P4/B7)
			"0ae4:0101:012c",
			// SOTP-031201 (MTC P4/B2-B7)
			"0ae4:0101:0190",
			// SOTP-031201 (MTC P5/B7)
			"0ae4:0101:0320",
			// SOTP-031201 (MTC P13/B7)
			"0ae4:0101:03e8",
			// COTM-02001 (Train Mascon)
			"1c06:77a7:0000"
		};

		/// <summary>The button mask for the buttons. Follows order in InputTranslator.</summary>
		private readonly ushort[] buttonMask;

		/// <summary>An array with raw input data from the controller.</summary>
		private byte[] inputBuffer;

		/// <summary>An array with raw output data for the controller.</summary>
		private byte[] outputBuffer;

		/// <summary>The setup packet needed to send data to the controller.</summary>
		private LibUsbDotNet.Main.UsbSetupPacket setupPacket = new LibUsbDotNet.Main.UsbSetupPacket(0x40, 0x50, 0x0, 0x0, 0x0);

		/// <summary>
		/// Initializes PS2 Train Simulator controller.
		/// </summary>
		internal Ps2TSController(ControllerButtons buttons, ushort[] buttonBytes, int brakeNotches, int powerNotches, bool reverser)
		{
			ControllerName = string.Empty;
			IsConnected = false;
			BrakeNotches = brakeNotches;
			PowerNotches = powerNotches;
			Buttons = buttons;
			buttonMask = buttonBytes;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal override void ReadInput()
		{
			// Set door lamp
			setupPacket.Value = (short)(DenshaDeGoInput.TrainDoorsClosed ? 0x10 : 0x0);

			// Sync input/output data
			LibUsb.SyncController(Guid, inputBuffer, outputBuffer, setupPacket);
			
			ushort buttonData = BitConverter.ToUInt16( new byte[2] { inputBuffer[2], inputBuffer[3] }, 0);
			byte handle = inputBuffer[1];
			byte reverser = (byte)(handle >> 4 & 0xF);

			// If the controller has a reverser, we need to remove low nibble
			if (HasReverser)
			{
				handle = (byte)(handle & 0xF);
			}

			// Handle must be a value between 1 and maximum notch (emergency + brake + neutral + power)
			if (handle > 0 && handle < BrakeNotches + PowerNotches + 3)
			{
				int emergencyNotch = 1;
				int neutralNotch = emergencyNotch + BrakeNotches + 1;

				if (handle == emergencyNotch)
				{
					InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Emergency;
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
				else if (handle < neutralNotch)
				{
					InputTranslator.BrakeNotch = (InputTranslator.BrakeNotches)(neutralNotch - handle);
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
				else if (handle > neutralNotch)
				{
					InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Released;
					InputTranslator.PowerNotch = (InputTranslator.PowerNotches)(handle - neutralNotch);
				}
				else
				{
					InputTranslator.BrakeNotch = InputTranslator.BrakeNotches.Released;
					InputTranslator.PowerNotch = InputTranslator.PowerNotches.N;
				}
			}

			if (HasReverser)
			{
				switch (reverser)
				{
					case 2:
					case 8:
						InputTranslator.ReverserPosition = InputTranslator.ReverserPositions.Forward;
						break;
					case 1:
					case 4:
						InputTranslator.ReverserPosition = InputTranslator.ReverserPositions.Backward;
						break;
					default:
						InputTranslator.ReverserPosition = InputTranslator.ReverserPositions.N;
						break;
				}
			}

			// Buttons
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Select] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Select]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Start] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Start]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.A]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.B] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.B]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.C] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.C]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.D] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.D]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.ATS] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.ATS]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.A2] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.A2]) != 0 ? ButtonState.Pressed : ButtonState.Released;

			// D-pad
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Up] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Up]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Down] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Down]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Left] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Left]) != 0 ? ButtonState.Pressed : ButtonState.Released;
			InputTranslator.ControllerButtons[(int)InputTranslator.ControllerButton.Right] = (buttonData & buttonMask[(int)InputTranslator.ControllerButton.Right]) != 0 ? ButtonState.Pressed : ButtonState.Released;
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

				if (!cachedControllers.ContainsKey(guid) && usbController.Value.IsConnected)
				{
					ControllerID id = new ControllerID(usbController.Value.VendorID, usbController.Value.ProductID, usbController.Value.Revision);
					string name = usbController.Value.ControllerName;

					// SOTP-031201 (MTC P4/B7)
					if (id.Type == ControllerType.PS2MTCP4B7)
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.DPad | ControllerButtons.ATS | ControllerButtons.A2;
						ushort[] buttonMask = { 0x200, 0x100, 0x4, 0x10, 0x20, 0x2, 0x0, 0x0, 0x400, 0x800, 0x1000, 0x2000, 0x0, 0x1, 0x8 };
						Ps2TSController newcontroller = new Ps2TSController(buttons, buttonMask, 7, 4, true)
						{
							// 4 bytes for input
							Guid = guid,
							Id = id,
							ControllerName = name,
							HasReverser = true,
							inputBuffer = new byte[] { 0x1, 0x0, 0x0, 0x0 },
							outputBuffer = new byte[0]
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// SOTP-031201 (MTC P4/B2-B7)
					if (id.Type == ControllerType.PS2MTCP4B2B7)
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.DPad | ControllerButtons.ATS | ControllerButtons.A2;
						ushort[] buttonMask = { 0x200, 0x100, 0x4, 0x10, 0x20, 0x2, 0x0, 0x0, 0x400, 0x800, 0x1000, 0x2000, 0x0, 0x1, 0x8 };
						Ps2TSController newcontroller = new Ps2TSController(buttons, buttonMask, 6, 4, true)
						{
							// 4 bytes for input
							Guid = guid,
							Id = id,
							ControllerName = name,
							HasReverser = true,
							inputBuffer = new byte[] { 0x1, 0x0, 0x0, 0x0 },
							outputBuffer = new byte[0]
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// SOTP-031201 (MTC P5/B7)
					if (id.Type == ControllerType.PS2MTCP5B7)
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.DPad | ControllerButtons.ATS | ControllerButtons.A2;
						ushort[] buttonMask = { 0x200, 0x100, 0x4, 0x10, 0x20, 0x2, 0x0, 0x0, 0x400, 0x800, 0x1000, 0x2000, 0x0, 0x1, 0x8 };
						Ps2TSController newcontroller = new Ps2TSController(buttons, buttonMask, 7, 5, true)
						{
							// 4 bytes for input
							Guid = guid,
							Id = id,
							ControllerName = name,
							inputBuffer = new byte[] { 0x1, 0x0, 0x0, 0x0 },
							outputBuffer = new byte[0]
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// SOTP-031201 (MTC P13/B7)
					if (id.Type == ControllerType.PS2MTCP13B7)
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.DPad | ControllerButtons.ATS | ControllerButtons.A2;
						ushort[] buttonMask = { 0x200, 0x100, 0x4, 0x10, 0x20, 0x2, 0x0, 0x0, 0x400, 0x800, 0x1000, 0x2000, 0x0, 0x1, 0x8 };
						Ps2TSController newcontroller = new Ps2TSController(buttons, buttonMask, 13, 4, true)
						{
							// 4 bytes for input
							Guid = guid,
							Id = id,
							ControllerName = name,
							inputBuffer = new byte[] { 0x1, 0x0, 0x0, 0x0 },
							outputBuffer = new byte[0]
						};
						cachedControllers.Add(guid, newcontroller);
					}
					// COTM-02001 (Train Mascon)
					if (id.Type == ControllerType.PS2TrainMascon)
					{
						ControllerButtons buttons = ControllerButtons.Select | ControllerButtons.Start | ControllerButtons.A | ControllerButtons.B | ControllerButtons.C | ControllerButtons.D | ControllerButtons.DPad | ControllerButtons.ATS | ControllerButtons.A2;
						ushort[] buttonMask = { 0x200, 0x100, 0x4, 0x10, 0x20, 0x2, 0x0, 0x0, 0x400, 0x800, 0x1000, 0x2000, 0x0, 0x1, 0x8 };
						Ps2TSController newcontroller = new Ps2TSController(buttons, buttonMask, 5, 5, true)
						{
							// 4 bytes for input
							Guid = guid,
							Id = id,
							ControllerName = name,
							HasReverser = true,
							inputBuffer = new byte[] { 0x1, 0x0, 0x0, 0x0 },
							outputBuffer = new byte[0]
						};
						cachedControllers.Add(guid, newcontroller);
					}
				}
				// Update connection status
				if (cachedControllers.ContainsKey(guid))
				{
					cachedControllers[guid].IsConnected = usbController.Value.IsConnected;
				}
			}
			return cachedControllers;
		}
	}
}
