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
			/// <summary>The ATS button</summary>
			ATS = 1024,
			/// <summary>The A2 button</summary>
			A2 = 2048,
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

		/// <summary>Whether the controller has a reverser</summary>
		protected internal bool HasReverser
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
			HasReverser = false;
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
			internal readonly int VID;
			/// <summary>The Product ID</summary>
			internal readonly int PID;
			/// <summary>The Revision</summary>
			internal readonly int Rev;

			/// <summary>Sets a controller's vendor and product ID from an OpenTK GUID.</summary>
			/// <param name="guid">The OpenTK GUID of the joystick.</param>
			internal ControllerID(Guid guid)
			{
				string id = guid.ToString("N");
				// OpenTK joysticks have a GUID which contains the vendor and product ID. It differs between platforms.
				switch (DenshaDeGoInput.CurrentHost.Platform)
				{
					case OpenBveApi.Hosts.HostPlatform.MicrosoftWindows:
					case OpenBveApi.Hosts.HostPlatform.WINE:
						VID = Convert.ToInt32(id.Substring(4, 4), 16);
						PID = Convert.ToInt32(id.Substring(0, 4), 16);
						break;
					default:
						VID = Convert.ToInt32(id.Substring(10, 2) + id.Substring(8, 2), 16);
						PID = Convert.ToInt32(id.Substring(18, 2) + id.Substring(16, 2), 16);
						break;
				}
			}

			internal ControllerID(int vid, int pid, int rev)
			{
				VID = vid;
				PID = pid;
				Rev = rev;
			}

			internal ControllerID()
			{
			}


			internal ControllerType Type
			{
				get
				{
					switch (VID)
					{
						case 0x0f0d:
							// Hori Inc
							if (PID == 0x00c1)
							{
								return ControllerType.Zuiki;
							}
							// May actually be a USB fight stick, but if we get the PIDs for these we can block them
							DenshaDeGoInput.CurrentHost.AddMessage("Densha de GO! Input: Unrecognised Hori Inc. PID " + PID + " - Please report this.");
							break;
						case 0x33dd:
							// Zuiki Inc
							if (PID >= 0x0001 && PID <= 0x0004)
							{
								return ControllerType.Zuiki;
							}
							// Something from Zuiki Inc, probably a future mascon revision
							DenshaDeGoInput.CurrentHost.AddMessage("Densha de GO! Input: Unrecognised Zuiki Inc. PID " + PID + " - Please report this.");
							return ControllerType.Zuiki;
						case 0x0ae4:
							// Taito Corp
							switch (PID)
							{
								case 0x0003:
									return ControllerType.PCTwoHandle;
								case 0x0004:
									return ControllerType.PS2TypeII;
								case 0x0005:
									return ControllerType.PS2Shinkansen;
								case 0x0007:
									return ControllerType.PS2Ryojouhen;
								case 0x0008:
									return ControllerType.PCRyojouhen;
								case 0x0101:
									switch (Rev)
									{
										case 300:
											return ControllerType.PS2MTCP4B7;
										case 400:
											return ControllerType.PS2MTCP4B2B7;
										case 800:
											return ControllerType.PS2MTCP5B7;
										case 1000:
											return ControllerType.PS2MTCP13B7;
									}
									break;
							}
							DenshaDeGoInput.CurrentHost.AddMessage("Densha de GO! Input: Unrecognised Taito Corp. PID " + PID + " - Please report this.");
							break;
						case 0x1c06:
							// Pony Canyon
							if (PID == 0x77a7)
							{
								return ControllerType.PS2TrainMascon;
							}
							// Pony Canyon 
							DenshaDeGoInput.CurrentHost.AddMessage("Densha de GO! Input: Unrecognised Pony Canyon PID " + PID + " - Please report this.");
							break;

					}

					return ControllerType.GenericUSB;
				}
			}
		}

		/// <summary>The types of known controllers</summary>
		internal enum ControllerType
		{
			/// <summary>Classic controllers, connected via USB adapter</summary>
			GenericUSB,
			/// <summary>Zuiki One Handle Controller for Switch</summary>
			Zuiki,
			// ** PS2 USB Controllers **
			/// <summary>TCPP-20009 (Type II)</summary>
			PS2TypeII,
			/// <summary>TCPP-20011 (Shinkansen)</summary>
			PS2Shinkansen,
			/// <summary>TCPP-20014 (Ryojouhen)</summary>
			PS2Ryojouhen,
			/// <summary>COTM-02001 (Train Mascon</summary>
			PS2TrainMascon,
			/// <summary>SOTP-031201 (MTC with P4/B7 cartridge)</summary>
			PS2MTCP4B7,
			/// <summary>SOTP-031201 (MTC with P4/B2-B7 cartridge)</summary>
			PS2MTCP4B2B7,
			/// <summary>SOTP-031201 (MTC with P5/B7 cartridge)</summary>
			PS2MTCP5B7,
			/// <summary>SOTP-031201 (MTC with P13/B7 cartridge)</summary>
			PS2MTCP13B7,
			// ** Unbalance Controllers **
			/// <summary>DGC-255 / DGOC-44U (Normally PC Two-Handle)</summary>
			PCTwoHandle,
			/// <summary>DRC-184 / DYC288 (Ryojouhen controller for PC)</summary>
			PCRyojouhen,

		}
	}
}
