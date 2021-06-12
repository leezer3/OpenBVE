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

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class representing a generic controller
	/// </summary>
	internal class Controller
	{
		[Flags]
		internal enum ControllerButtons
		{
			/// <summary>No button</summary>
			None = 0,
			/// <summary>The Select button</summary>
			Select = 1,
			/// <summary>The Start button</summary>
			Start = 2,
			/// <summary>The A button</summary>
			A = 4,
			/// <summary>The B button</summary>
			B = 8,
			/// <summary>The C button</summary>
			C = 16,
			/// <summary>The D button</summary>
			D = 32,
			/// <summary>The D-pad</summary>
			DPad = 64,
			/// <summary>The pedal button</summary>
			Pedal = 128,
			/// <summary>The left doors button</summary>
			LDoor = 256,
			/// <summary>The right doors button</summary>
			RDoor = 512,
		}

		/// <summary>The buttons the controller has</summary>
		internal protected ControllerButtons Buttons
		{
			get;
			protected set;
		}

		/// <summary>The name of the controller</summary>
		internal protected string ControllerName;

		/// <summary>The amount of brake notches</summary>
		internal protected int BrakeNotches
		{
			get;
			protected set;
		}
		/// <summary>The amount of power notches</summary>
		internal protected int PowerNotches
		{
			get;
			protected set;
		}

		/// <summary>The Guid for the controller</summary>
		internal protected Guid Guid
		{
			get;
			protected set;
		}

		/// <summary>A string with the vendor and product ID for the controller</summary>
		internal protected string Id
		{
			get;
			protected set;
		}

		/// <summary>Whether the controller is connected</summary>
		internal protected bool IsConnected;

		/// <summary>Whether the controller requires calibration</summary>
		internal protected bool RequiresCalibration
		{
			get;
			protected set;
		}

		internal Controller()
		{
			Guid = Guid.Empty;
			Id = string.Empty;
			ControllerName = string.Empty;
			IsConnected = false;
			RequiresCalibration = false;
			Buttons = ControllerButtons.None;
		}

		/// <summary>
		/// Reads the input from the controller.
		/// </summary>
		internal virtual void ReadInput()
		{
		}

		/// <summary>
		/// Gets a string representing a controller's vendor and product ID.
		/// </summary>
		/// <param name="guid">The GUID of the joystick.</param>
		/// <returns>String representing the controller's vendor and product ID.</returns>
		internal static string GetControllerID(Guid guid)
		{
			string id = guid.ToString("N");
			// OpenTK joysticks have a GUID which contains the vendor and product ID. It differs between platforms.
			switch (DenshaDeGoInput.CurrentHost.Platform)
			{
				case OpenBveApi.Hosts.HostPlatform.MicrosoftWindows:
					id = id.Substring(4, 4) + ":" + id.Substring(0, 4);
					break;
				default:
					id = id.Substring(10, 2) + id.Substring(8, 2) + ":" + id.Substring(18, 2) + id.Substring(16, 2);
					break;
			}
			return id;
		}

	}
}
