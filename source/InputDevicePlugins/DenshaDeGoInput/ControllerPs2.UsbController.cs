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
		/// <summary>The USB controller.</summary>
		internal UsbDevice ControllerDevice;
		/// <summary>The USB endpoint reader</summary>
		internal UsbEndpointReader ControllerReader;
		/// <summary>Whether the controller is connected</summary>
		internal bool IsConnected;

		internal UsbController(int vid, int pid)
		{
			VendorID = vid;
			ProductID = pid;
			controllerName = string.Empty;
			ControllerDevice = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(vid, pid));
			ControllerReader = ControllerDevice.OpenEndpointReader(ReadEndpointID.Ep01);
			IsConnected = false;
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
					controllerName = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(VendorID, ProductID)).Info.ProductString;
				}
				catch
				{
					//Something failed in the LibUsb call
					controllerName = @"Unknown";
				}

				return controllerName;
			}
		}

		/// <summary>
		/// Unloads the controller
		/// </summary>
		internal void Unload()
		{
			// Specially crafted array that blanks the display and turns off the door lamp
			byte[] writeBuffer;
			if (InputTranslator.ControllerModel == InputTranslator.ControllerModels.Ps2Shinkansen)
			{
				writeBuffer = new byte[] { 0x0, 0x0, 0x0, 0x0, 0xFF, 0xFF, 0xFF, 0xFF };
			}
			else
			{
				writeBuffer = new byte[] { 0x0, 0x3 };
			}

			try
			{
				int bytesWritten;
				ControllerDevice.ControlTransfer(ref ControllerPs2.setupPacket, writeBuffer, 8, out bytesWritten);

				IUsbDevice wholeUsbDevice = ControllerDevice as IUsbDevice;
				if (!ReferenceEquals(wholeUsbDevice, null))
				{
					// Release interface
					wholeUsbDevice.ReleaseInterface(1);
				}
			}
			catch
			{
				//Only trying to unload
			}

			ControllerDevice.Close();
			UsbDevice.Exit();
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
				int readCount;
				ErrorCode readError = ControllerReader.Read(ControllerPs2.readBuffer, 0, 6, 0, out readCount);
				if (readError != ErrorCode.Ok)
				{
					DenshaDeGoInput.LibUsbIssue = true;
					return;
				}

				int bytesWritten;
				ControllerDevice.ControlTransfer(ref ControllerPs2.setupPacket, ControllerPs2.writeBuffer, ControllerPs2.writeBuffer.Length, out bytesWritten);
			}
			catch
			{
				DenshaDeGoInput.LibUsbIssue = true;
				DenshaDeGoInput.CurrentHost.AddMessage(MessageType.Error, false, "The DenshaDeGo! Input Plugin encountered a critical error whilst attempting to poll controller " + InputTranslator.ActiveControllerGuid);
			}
			
		}
	}
}
