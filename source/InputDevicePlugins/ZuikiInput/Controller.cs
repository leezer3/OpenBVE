//Copyright (c) 2026, Marc Riera, The OpenBVE Project
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
using OpenBveApi.Runtime;
using OpenBveApi.Interface;

namespace ZuikiInput
{
	/// <summary>Class representing a generic controller.</summary>
	public class Controller
	{
		/// <summary>Enumeration representing controller buttons.</summary>
		[Flags]
		protected internal enum ControllerButtons
		{
			/// <summary>No buttons</summary>
			None = 0,
			/// <summary>A button</summary>
			A = 0x1,
			/// <summary>B button</summary>
			B = 0x2,
			/// <summary>X button</summary>
			X = 0x4,
			/// <summary>Y button</summary>
			Y = 0x8,
			/// <summary>D-pad Up button</summary>
			Up = 0x10,
			/// <summary>D-pad Down button</summary>
			Down = 0x20,
			/// <summary>D-pad Left button</summary>
			Left = 0x40,
			/// <summary>D-pad Right button</summary>
			Right = 0x80,
			/// <summary>+ button</summary>
			Plus = 0x100,
			/// <summary>- button</summary>
			Minus = 0x200,
			/// <summary>L button</summary>
			L = 0x400,
			/// <summary>R button</summary>
			R = 0x800,
			/// <summary>ZL button</summary>
			ZL = 0x1000,
			/// <summary>ZR button</summary>
			ZR = 0x2000,
			/// <summary>Home button</summary>
			Home = 0x4000,
			/// <summary>Screenshot button</summary>
			Screenshot = 0x8000,
			/// <summary>Horn button</summary>
			Horn = 0x10000,
			/// <summary>EB Reset button</summary>
			EbReset = 0x20000,
			/// <summary>ATS button</summary>
			Ats = 0x40000,
			/// <summary>Pantograph Down button</summary>
			PantographDown = 0x80000,
			/// <summary>Hill Start button</summary>
			HillStart = 0x100000,
			/// <summary>Square (unlabeled) button</summary>
			Square = 0x200000,
			/// <summary>Horn pedal light press</summary>
			PedalLight = 0x400000,
			/// <summary>Horn pedal strong press</summary>
			PedalStrong = 0x800000,
		}

		/// <summary>Class representing controller capabilities.</summary>
		protected internal class ControllerCapabilities
		{
			/// <summary>The amount of brake notches (excluding N and EMG).</summary>
			internal readonly int BrakeNotches;

			/// <summary>The amount of power notches (excluding N).</summary>
			internal readonly int PowerNotches;

			/// <summary>Whether the controller has a reverser.</summary>
			internal readonly bool HasReverser;

			/// <summary>The buttons supported by the controller.</summary>
			internal readonly ControllerButtons Buttons;

			/// <summary>Creates a set of controller capabilities.</summary>
			/// <param name="brakeNotches">The amount of brake notches (excluding N and EMG).</param>
			/// <param name="powerNotches">The amount of power notches (excluding N).</param>
			/// <param name="hasReverser">Whether the controller has a reverser.</param>
			/// <param name="buttons">The buttons supported by the controller.</param>
			internal ControllerCapabilities(int brakeNotches, int powerNotches, bool hasReverser, ControllerButtons buttons)
			{
				BrakeNotches = brakeNotches;
				PowerNotches = powerNotches;
				HasReverser = hasReverser;
				Buttons = buttons;
			}
		}


		/// <summary>The GUID of the controller.</summary>
		protected internal Guid Guid
		{
			get; protected set;
		}

		/// <summary>The name of the controller</summary>
		protected internal string Name
		{
			get; protected set;
		}

		/// <summary>The capabilities of the controller.</summary>
		protected internal ControllerCapabilities Capabilities
		{
			get; protected set;
		}

		/// <summary>The current state of the controller.</summary>
		internal ControllerState State = new ControllerState();

		/// <summary>The previous state of the controller.</summary>
		internal ControllerState PreviousState = new ControllerState();


		/// <summary>Updates the state of the controller.</summary>
		/// <param name="doorState">The current state of the doors.</param>
		internal virtual void Update(DoorStates doorState)
		{
		}

		/// <summary>Closes the connection to the controller.</summary>
		internal virtual void Close()
		{
		}

		/// <summary>Updates the previous state of the controller.</summary>
		protected void UpdatePreviousState()
		{
			PreviousState.IsConnected = State.IsConnected;
			PreviousState.BrakeNotch = State.BrakeNotch;
			PreviousState.PowerNotch = State.PowerNotch;
			PreviousState.ReverserPosition = State.ReverserPosition;
			PreviousState.PressedButtons = State.PressedButtons;
		}

		/// <summary>Gets a controller's ID string from an OpenTK GUID.</summary>
		/// <param name="guid">The OpenTK GUID of the joystick.</param>
		protected static string GetControllerId(Guid guid)
		{
			string id = guid.ToString("N");
			// OpenTK joysticks have a GUID which contains the vendor and product ID. It differs between platforms.
			switch (ZuikiInput.CurrentHost.Platform)
			{
				case OpenBveApi.Hosts.HostPlatform.MicrosoftWindows:
				case OpenBveApi.Hosts.HostPlatform.WINE:
					return String.Format("{0}:{1}", id.Substring(4, 4), id.Substring(0, 4));
				default:
					return String.Format("{0}:{1}", id.Substring(10, 2) + id.Substring(8, 2), id.Substring(18, 2) + id.Substring(16, 2));
			}
		}
	}
}
