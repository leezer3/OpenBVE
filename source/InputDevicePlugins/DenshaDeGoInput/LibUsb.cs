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
using OpenBveApi;
using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class LibUsb-related functions.
	/// </summary>
	internal partial class LibUsb
	{
		/// <summary>
		/// Dictionary containing the supported USB controllers
		/// </summary>
		private static Dictionary<Guid, UsbController> supportedUsbControllers = new Dictionary<Guid, UsbController>();

		/// <summary>
		/// GUID of the active controller
		/// </summary>
		private static Guid activeControllerGuid = new Guid();

		/// <summary>
		/// The thread which spins to poll for LibUsb input
		/// </summary>
		internal static Thread LibUsbThread;

		/// <summary>
		/// The control variable for the LibUsb input thread
		/// </summary>
		internal static bool LibUsbShouldLoop = true;

		/// <summary>
		/// The setup packet needed to send data to the controller.
		/// </summary>
		private static UsbSetupPacket setupPacket = new UsbSetupPacket(0x40, 0x09, 0x0301, 0x0000, 0x0008);

		/// <summary>
		/// Adds the supported controller models to the LibUsb list.
		/// </summary>
		/// <param name="ids">A list of VID+PID identifiers to search</param>
		internal static void AddSupportedControllers(string[] ids)
		{
			foreach (string id in ids)
			{
				int vid = int.Parse(id.Substring(0, 4), NumberStyles.HexNumber);
				int pid = int.Parse(id.Substring(5, 4), NumberStyles.HexNumber);
				Guid guid;
				switch (DenshaDeGoInput.CurrentHost.Platform)
				{
					case OpenBveApi.Hosts.HostPlatform.MicrosoftWindows:
						guid = new Guid(id.Substring(5, 4) + id.Substring(0, 4) + "-ffff-ffff-ffff-ffffffffffff");
						break;
					default:
						string vendor = id.Substring(2, 2) + id.Substring(0, 2);
						string product = id.Substring(7, 2) + id.Substring(5, 2);
						guid = new Guid("ffffffff-" + vendor + "-ffff-" + product + "-ffffffffffff");
						break;
				}
				UsbController controller = new UsbController(vid, pid);
				if (!supportedUsbControllers.ContainsKey(guid))
				{
					// Add new controller
					supportedUsbControllers.Add(guid, controller);
				}
				else
				{
					// Replace existing controller
					supportedUsbControllers[guid] = controller;
				}
			}
		}

		/// <summary>
		/// Loop to be executed on a separate thread to handle LibUsb work.
		/// </summary>
		internal static void LibUsbLoop()
		{
			while (LibUsbShouldLoop && !DenshaDeGoInput.LibUsbIssue)
			{
				// First, let's check which USB devices are connected
				CheckConnectedControllers();

				if (activeControllerGuid != InputTranslator.ActiveControllerGuid)
				{
					if (supportedUsbControllers.ContainsKey(activeControllerGuid))
					{
						// If the selected controller has changed, unload the previous one
						supportedUsbControllers[activeControllerGuid].Unload();
					}
					activeControllerGuid = InputTranslator.ActiveControllerGuid;
				}

				// If the current controller is a supported controller and is connected, poll it for input
				if (supportedUsbControllers.ContainsKey(activeControllerGuid) && supportedUsbControllers[activeControllerGuid].IsConnected)
				{
					supportedUsbControllers[activeControllerGuid].Poll();
				}
			}

			foreach (var controller in supportedUsbControllers.Values)
			{
				controller.Unload();
			}
		}

		/// <summary>
		/// Checks the connection status of supported LibUsb controllers.
		/// </summary>
		private static void CheckConnectedControllers()
		{
			if (DenshaDeGoInput.LibUsbIssue)
			{
				return;
			}
			try
			{
				foreach (UsbController controller in supportedUsbControllers.Values)
				{
					if (controller.ControllerDevice == null || !controller.IsConnected)
					{
						// The device is not configured, try to find it
						controller.ControllerDevice = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(controller.VendorID, controller.ProductID));
					}
					if (controller.ControllerDevice == null)
					{
						// The controller is not connected
						controller.IsConnected = false;
					}
					else
					{
						if (!controller.IsConnected)
						{
							// Open endpoint reader, if necessary
							controller.ControllerReader = controller.ControllerDevice.OpenEndpointReader(ReadEndpointID.Ep01);
						}
						// The controller is connected
						controller.IsConnected = true;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
				if (DenshaDeGoInput.CurrentHost.SimulationState == SimulationState.Running)
				{
					DenshaDeGoInput.CurrentHost.AddMessage(MessageType.Error, false, "The DenshaDeGo! Input Plugin encountered a critical error whilst attempting to update the connected controller list.");
				}
				//LibUsb isn't working right
				DenshaDeGoInput.LibUsbIssue = true;
			}
		}

		/// <summary>
		/// Gets the list of supported controllers.
		/// </summary>
		/// <returns>The list of supported controllers.</returns>
		internal static Dictionary<Guid, UsbController> GetSupportedControllers()
		{
			return supportedUsbControllers;
		}

		/// <summary>
		/// Syncs the read and input buffers for the controller with the specified GUID.
		/// </summary>
		/// <param name="guid">The GUID of the controller</param>
		/// <param name="read">An array containing the previous read buffer</param>
		/// <param name="write">The bytes to be sent to the controller</param>
		/// <returns>The bytes read from the controller.</returns>
		internal static byte[] SyncController(Guid guid, byte[] read, byte[] write)
		{
			// If the read buffer's length is 0, copy the initial input bytes to get the required read length
			if (supportedUsbControllers[guid].ReadBuffer.Length == 0)
			{
				supportedUsbControllers[guid].ReadBuffer = read;
			}
			// Copy the output bytes to the write buffer
			supportedUsbControllers[guid].WriteBuffer = write;
			// If the length of the byte array to be sent to the controller when unloaded is 0, use the write buffer
			if (supportedUsbControllers[guid].UnloadBuffer.Length == 0)
			{
				supportedUsbControllers[guid].UnloadBuffer = write;
			}
			// Return the bytes from the read buffer
			return supportedUsbControllers[guid].ReadBuffer;
		}

	}
}
