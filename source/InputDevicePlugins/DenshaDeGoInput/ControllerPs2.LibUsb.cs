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
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using OpenBveApi;
using OpenBveApi.Interface;

namespace DenshaDeGoInput
{
	public partial class ControllerPs2
	{
		/// <summary>
		/// Dictionary containing the connected USB controllers
		/// </summary>
		internal static Dictionary<Guid, UsbController> connectedUsbControllers = new Dictionary<Guid, UsbController>();

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
		internal static UsbSetupPacket setupPacket = new UsbSetupPacket(0x40, 0x09, 0x0301, 0x0000, 0x0008);

		internal static void LibUsbLoop()
		{
			while (LibUsbShouldLoop && !DenshaDeGoInput.LibUsbIssue)
			{
				//First, let's update our conected USB device list
				FindControllers();
				if (connectedUsbControllers.ContainsKey(InputTranslator.ActiveControllerGuid))
				{
					//The active controller is connected
					UsbController activeController = connectedUsbControllers[InputTranslator.ActiveControllerGuid];
					activeController.Poll();
				}
				Thread.Sleep(100);
			}

			foreach (var controller in connectedUsbControllers.Values)
			{
				controller.Unload();
			}
		}

		/// <summary>
		/// Finds compatible non-standard controllers.
		/// </summary>
		internal static void FindControllers()
		{
			if (DenshaDeGoInput.LibUsbIssue)
			{
				return;
			}
			try
			{
				UsbDeviceFinder[] FinderPS2 = { new UsbDeviceFinder(0x0ae4, 0x0004), new UsbDeviceFinder(0x0ae4, 0x0005) };
				foreach (UsbDeviceFinder device in FinderPS2)
				{
					UsbDevice controller = UsbDevice.OpenUsbDevice(device);
					if (controller != null)
					{
						string vendor = device.Vid.ToString("X4").ToLower().Substring(2, 2) + device.Vid.ToString("X4").ToLower().Substring(0, 2);
						string product = device.Pid.ToString("X4").ToLower().Substring(2, 2) + device.Pid.ToString("X4").ToLower().Substring(0, 2);
						Guid foundGuid = new Guid("ffffffff-" + vendor + "-ffff-" + product + "-ffffffffffff");
						if (!connectedUsbControllers.ContainsKey(foundGuid))
						{
							connectedUsbControllers.Add(foundGuid, new UsbController(device.Vid, device.Pid));
						}
						else
						{
							connectedUsbControllers[foundGuid].IsConnected = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(device.Vid, device.Pid)) != null;
						}
					}
				}
			}
			catch
			{
				if (DenshaDeGoInput.CurrentHost.SimulationState == SimulationState.Running)
				{
					DenshaDeGoInput.CurrentHost.AddMessage(MessageType.Error, false, "The DenshaDeGo! Input Plugin encountered a critical error whilst attempting to update the connected controller list.");
				}
				//LibUsb isn't working right
				DenshaDeGoInput.LibUsbIssue = true;
			}
		}
	}
}
