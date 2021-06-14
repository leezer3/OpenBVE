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

using LibUsbDotNet;
using LibUsbDotNet.Main;
using OpenBveApi.Interface;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class LibUsb-related functions.
	/// </summary>
	internal partial class LibUsb
	{
		/// <summary>
		/// Class representing a USB controller accessed via LibUsb
		/// </summary>
		internal class UsbController
		{
			/// <summary>The USB Vendor ID</summary>
			internal readonly int VendorID;
			/// <summary>The USB product ID</summary>
			internal readonly int ProductID;
			/// <summary>Backing property containing the cached controller name</summary>
			private string controllerName;
			/// <summary>An array to be sent to the controller upon unload</summary>
			internal byte[] UnloadBuffer;
			/// <summary>The USB controller.</summary>
			internal UsbDevice ControllerDevice;
			/// <summary>The USB endpoint reader</summary>
			internal UsbEndpointReader ControllerReader;
			/// <summary>Whether the controller is connected</summary>
			internal bool IsConnected;
			/// <summary>Byte array containing the data received from the controller</summary>
			internal byte[] ReadBuffer;
			/// <summary>Byte array containing the data to be sent to the controller</summary>
			internal byte[] WriteBuffer;

			/// <summary>
			/// Initializes a controller
			/// <param name="vid">A string representing the vendor ID.</param>
			/// <param name="pid">A string representing the product ID.</param>
			/// </summary>
			internal UsbController(int vid, int pid)
			{
				VendorID = vid;
				ProductID = pid;
				controllerName = string.Empty;
				UnloadBuffer = new byte[0];
				IsConnected = false;
				ReadBuffer = new byte[0];
				WriteBuffer = new byte[0];
			}

			/// <summary>Gets the name of the controller</summary>
			internal string ControllerName
			{
				get
				{
					if (!string.IsNullOrEmpty(controllerName))
					{
						return controllerName;
					}

					try
					{
						if (ControllerDevice != null)
						{
							controllerName = ControllerDevice.Info.ProductString;
						}
					}
					catch
					{
					}
					return controllerName;
				}
			}

			/// <summary>
			/// Unloads the controller
			/// </summary>
			internal void Unload()
			{
				try
				{
					if (ControllerDevice != null)
					{
						if (UnloadBuffer.Length > 0)
						{
							// Send unload buffer to turn off controller
							int bytesWritten;
							ControllerDevice.ControlTransfer(ref setupPacket, UnloadBuffer, UnloadBuffer.Length, out bytesWritten);
						}
						IUsbDevice wholeUsbDevice = ControllerDevice as IUsbDevice;
						if (!ReferenceEquals(wholeUsbDevice, null))
						{
							// Release interface
							wholeUsbDevice.ReleaseInterface(1);
						}
						ControllerDevice.Close();
						UsbDevice.Exit();
					}
				}
				catch
				{
					//Only trying to unload
				}

				if (ControllerDevice != null)
				{

				}
			}

			/// <summary>
			/// Polls the controller for input
			/// </summary>
			internal void Poll()
			{
				if (DenshaDeGoInput.LibUsbIssue)
				{
					return;
				}
				try
				{
					// Ask for input
					if (ReadBuffer.Length > 0)
					{
						int readCount;
						ErrorCode readError = ControllerReader.Read(ReadBuffer, 0, ReadBuffer.Length, 100, out readCount);

						if (readError == ErrorCode.DeviceNotFound || readError == ErrorCode.Win32Error || readError == ErrorCode.MonoApiError)
						{
							// If the device is not found during read, mark it as disconnected
							IsConnected = false;
						}
					}

					// Send output buffer
					if (WriteBuffer.Length > 0)
					{
						int bytesWritten;
						ControllerDevice.ControlTransfer(ref LibUsb.setupPacket, WriteBuffer, WriteBuffer.Length, out bytesWritten);
					}
				}
				catch
				{
					DenshaDeGoInput.LibUsbIssue = true;
					DenshaDeGoInput.CurrentHost.AddMessage(MessageType.Error, false, "The DenshaDeGo! Input Plugin encountered a critical error whilst attempting to poll controller " + activeControllerGuid);
				}

			}
		}
	}
}
