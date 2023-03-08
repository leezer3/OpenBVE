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
		protected internal ControllerButtons Buttons
		{
			get;
			protected set;
		}

		/// <summary>The name of the controller</summary>
		protected internal string ControllerName;

		/// <summary>The amount of brake notches</summary>
		protected internal int BrakeNotches
		{
			get;
			protected set;
		}
		/// <summary>The amount of power notches</summary>
		protected internal int PowerNotches
		{
			get;
			protected set;
		}

		/// <summary>The Guid for the controller</summary>
		protected internal Guid Guid
		{
			get;
			protected set;
		}

		/// <summary>A string with the vendor and product ID for the controller</summary>
		protected internal ControllerID Id
		{
			get;
			protected set;
		}

		/// <summary>Whether the controller is connected</summary>
		protected internal bool IsConnected;

		/// <summary>Whether the controller requires calibration</summary>
		protected internal bool RequiresCalibration
		{
			get;
			protected set;
		}

		internal Controller()
		{
			Guid = Guid.Empty;
			Id = new ControllerID();
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

		/// <summary>A USB controller ID</summary>
		internal class ControllerID
		{
			/// <summary>The Vendor ID</summary>
			internal readonly string VID;
			/// <summary>The Product ID</summary>
			internal readonly string PID;

			/// <summary>Gets a controller's vendor and product ID.</summary>
			/// <param name="guid">The OpenTK GUID of the joystick.</param>
			internal ControllerID(Guid guid)
			{
				string id = guid.ToString("N");
				// OpenTK joysticks have a GUID which contains the vendor and product ID. It differs between platforms.
				switch (DenshaDeGoInput.CurrentHost.Platform)
				{
					case OpenBveApi.Hosts.HostPlatform.MicrosoftWindows:
						VID = id.Substring(4, 4);
						PID = id.Substring(0, 4);
						break;
					default:
						VID = id.Substring(10, 2) + id.Substring(8, 2);
						PID = id.Substring(18, 2) + id.Substring(16, 2);
						break;
				}
			}

			internal ControllerID()
			{
				VID = string.Empty;
				PID = string.Empty;
			}


			internal ControllerType Type
			{
				get
				{
					switch (VID)
					{
						case "0f0d":
							// Hori Inc
							if (PID == "00c1")
							{
								return ControllerType.Zuki;
							}
							// May actually be a USB fight stick, but if we get the PIDs for these we can block them
							DenshaDeGoInput.CurrentHost.AddMessage("Densha DeGo! Input: Unrecognised Hori Inc. PID " + PID + " - Please report this.");
							break;
						case "33dd":
							// Zuki Inc
							if (PID == "0001" || PID == "0002")
							{
								return ControllerType.Zuki;
							}
							// Zuki Inc 
							DenshaDeGoInput.CurrentHost.AddMessage("Densha DeGo! Input: Unrecognised Zuki Inc. PID " + PID + " - Please report this.");
							break;
						case "0ae4":
							// Taito Corp
							switch (PID)
							{
								case "0003":
									return ControllerType.DG255;
								case "0004":
									return ControllerType.PS2TypeII;
								case "0005":
									return ControllerType.PS2Shinkansen;
								case "0007":
									return ControllerType.PS2Ryojouhen;
								case "0008":
									return ControllerType.DRC184;
							}
							DenshaDeGoInput.CurrentHost.AddMessage("Densha DeGo! Input: Unrecognised Taito Corp. PID " + PID + " - Please report this.");
							break;

					}

					return ControllerType.GenericUSB;
				}
			}
		}

		/// <summary>The types of known controller</summary>
		internal enum ControllerType
		{
			/// <summary>Classic controllers, connected via pad adaptor</summary>
			GenericUSB,
			/// <summary>Zuki Controller for Switch</summary>
			Zuki,
			// ** PS2 USB Controllers **
			/// <summary>TCPP-20009 (Type II)</summary>
			PS2TypeII,
			/// <summary>TCPP-20011 (Shinkansen)</summary>
			PS2Shinkansen,
			/// <summary>TCPP-20014 (Ryojouhen)</summary>
			PS2Ryojouhen,
			// ** Unbalance Controllers **
			/// <summary>DGC-255 / DGOC-44U</summary>
			DG255,
			/// <summary>DRC-184 / DYC288</summary>
			DRC184,

		}
	}
}
