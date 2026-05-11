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
using System.Collections.Generic;
using OpenTK.Input;
using OpenBveApi.Runtime;
using HidSharp;

namespace ZuikiInput
{
	/// <summary>Class representing a ZUIKI Mascon controller.</summary>
	public class MasconController : Controller
	{
		/// <summary>Enumeration representing controller models.</summary>
		private enum ControllerModels
		{
			/// <summary>Unsupported controller</summary>
			Unsupported,
			/// <summary>ZUIKI Mascon</summary>
			Mascon,
			/// <summary>ZUIKI Mascon Pro</summary>
			MasconPro,
		};

		/// <summary>The controller model.</summary>
		private readonly ControllerModels controllerModel;

		/// <summary>The min/max value for each brake notch, from Released to Emergency. Each notch consists of two values.</summary>
		private readonly double[] brakeValues;

		/// <summary>The min/max value for each power notch, from N to maximum. Each notch consists of two values.</summary>
		private readonly double[] powerValues;

		/// <summary>The controller index in OpenTK.</summary>
		private int controllerIndex;

		/// <summary>The controller HID stream.</summary>
		private HidStream hidStream;

		/// <summary>The bytes sent to the controller via HID.</summary>
		private byte[] outputBytes = new byte[9];

		private MasconController(Guid guid, string name, int index, ControllerModels model, double[] brakeNotchValues, double[] powerNotchValues, bool hasReverser, ControllerButtons buttons)
		{
			Guid = guid;
			Name = name;
			controllerIndex = index;
			controllerModel = model;
			brakeValues = brakeNotchValues;
			powerValues = powerNotchValues;
			Capabilities = new ControllerCapabilities(brakeNotchValues.Length / 2 - 2, powerNotchValues.Length / 2 - 1, hasReverser, buttons);
		}

		/// <summary>Lists the controllers supported by this class.</summary>
		internal static void GetControllers(Dictionary<Guid, Controller> controllerList)
		{
			Dictionary<Guid, Controller> controllers = new Dictionary<Guid, Controller>();

			// Check the first 10 joysticks, should be enough
			for (int i = 0; i < 10; i++)
			{
				Guid guid = Joystick.GetGuid(i);
				string name = Joystick.GetName(i);
				string controllerId = GetControllerId(guid);
				switch (controllerId)
				{
					// ZUIKI Mascon / Densha de GO! One Handle controller for Nintendo Switch
					case "0f0d:00c1":
					case "33dd:0001":
					case "33dd:0002":
					case "33dd:0003":
					case "33dd:0004":
					case "33dd:0005":
						// The controller uses buttons 1-13, hat 1 and axis 2, we need those at minimum
						if (Joystick.GetCapabilities(i).ButtonCount >= 13 && Joystick.GetCapabilities(i).HatCount >= 1 && Joystick.GetCapabilities(i).AxisCount >= 2)
						{
							double[] brake = { -0.15, 0.15, -0.25, -0.15, -0.35, -0.25, -0.45, -0.35, -0.57, -0.45, -0.7, -0.57, -0.8, -0.7, -0.9, -0.8, -0.97, -0.9, -1, -0.97 };
							double[] power = { -0.15, 0.15, 0.15, 0.32, 0.32, 0.52, 0.52, 0.7, 0.7, 0.87, 0.87, 1 };
							MasconController controller = new MasconController(guid, name, i, ControllerModels.Mascon, brake, power, false, (ControllerButtons)0xFFFF);
							controller.State.IsConnected = Joystick.GetState(i).IsConnected;
							if (!controllerList.ContainsKey(guid))
							{
								controllerList.Add(guid, controller);
							}
						}
						break;
					// ZUIKI Mascon Pro
					case "33dd:0006":
						// The controller uses buttons 1-13, hat 1 and axis 2, we need those at minimum
						if (Joystick.GetCapabilities(i).ButtonCount >= 17 && Joystick.GetCapabilities(i).HatCount >= 1 && Joystick.GetCapabilities(i).AxisCount >= 4)
						{
							double[] brake = { -0.15, 0.15, -0.25, -0.15, -0.35, -0.25, -0.45, -0.35, -0.57, -0.45, -0.7, -0.57, -0.8, -0.7, -0.9, -0.8, -0.97, -0.9, -1, -0.97 };
							double[] power = { -0.15, 0.15, 0.15, 0.32, 0.32, 0.52, 0.52, 0.7, 0.7, 0.87, 0.87, 1 };
							MasconController controller = new MasconController(guid, name, i, ControllerModels.MasconPro, brake, power, true, (ControllerButtons)0xFFFFFF);
							controller.State.IsConnected = Joystick.GetState(i).IsConnected;
							try
							{
								DeviceList list = DeviceList.Local;
								HidDevice device = list.GetHidDeviceOrNull(0x33dd, 0x0006);
								controller.hidStream = device.Open();
							}
							catch
							{
								Console.WriteLine("ZUIKI input: Could not open HID device. If using Linux, make sure the udev rule has been installed.");
							}
							if (!controllerList.ContainsKey(guid))
							{
								controllerList.Add(guid, controller);
							}
						}
						break;
					default:
						continue;
				}
			}
		}

		/// <summary>Updates the state of the controller.</summary>
		/// <param name="doorState">The current state of the doors.</param>
		internal override void Update(DoorStates doorState)
		{
			JoystickState state = Joystick.GetState(controllerIndex);
			UpdatePreviousState();
			State.IsConnected = state.IsConnected;

			State.BrakeNotch = ControllerState.BrakeNotches.Released;
			State.PowerNotch = ControllerState.PowerNotches.N;

			if (State.IsConnected)
			{
				double handleAxis = Math.Round(state.GetAxis(1), 4);
				for (int i = 0; i < brakeValues.Length; i += 2)
				{
					// Each notch uses two values, minimum and maximum
					if (handleAxis >= brakeValues[i] && handleAxis <= brakeValues[i + 1])
					{
						if (i > 0)
						{
							State.BrakeNotch = (ControllerState.BrakeNotches)(i / 2);
						}
						break;
					}
				}
				for (int i = 0; i < powerValues.Length; i += 2)
				{
					// Each notch uses two values, minimum and maximum
					if (handleAxis >= powerValues[i] && handleAxis <= powerValues[i + 1])
					{
						if (i > 0)
						{
							State.PowerNotch = (ControllerState.PowerNotches)(i / 2);
						}
						break;
					}
				}

				// Buttons
				State.PressedButtons = ControllerButtons.None;
				if (state.IsButtonDown(0)) State.PressedButtons |= ControllerButtons.Y;
				if (state.IsButtonDown(1)) State.PressedButtons |= ControllerButtons.B;
				if (state.IsButtonDown(2)) State.PressedButtons |= ControllerButtons.A;
				if (state.IsButtonDown(3)) State.PressedButtons |= ControllerButtons.X;
				if (state.IsButtonDown(4)) State.PressedButtons |= ControllerButtons.L;
				if (state.IsButtonDown(5)) State.PressedButtons |= ControllerButtons.R;
				if (state.IsButtonDown(6)) State.PressedButtons |= ControllerButtons.ZL;
				if (state.IsButtonDown(7)) State.PressedButtons |= ControllerButtons.ZR;
				if (state.IsButtonDown(8)) State.PressedButtons |= ControllerButtons.Minus;
				if (state.IsButtonDown(9)) State.PressedButtons |= ControllerButtons.Plus;
				if (state.IsButtonDown(12)) State.PressedButtons |= ControllerButtons.Home;
				if (state.IsButtonDown(13)) State.PressedButtons |= ControllerButtons.Screenshot;
				if (state.GetHat(0).IsUp) State.PressedButtons |= ControllerButtons.Up;
				if (state.GetHat(0).IsDown) State.PressedButtons |= ControllerButtons.Down;
				if (state.GetHat(0).IsLeft) State.PressedButtons |= ControllerButtons.Left;
				if (state.GetHat(0).IsRight) State.PressedButtons |= ControllerButtons.Right;

				// ZUIKI Mascon Pro
				if (controllerModel == ControllerModels.MasconPro)
				{
					float reverser = state.GetAxis(3);
					if (reverser >= 0.5)
					{
						State.ReverserPosition = ControllerState.ReverserPositions.Backward;
					}
					else if (reverser <= -0.5)
					{
						State.ReverserPosition = ControllerState.ReverserPositions.Neutral;
					}
					else
					{
						State.ReverserPosition = ControllerState.ReverserPositions.Forward;
					}

					if (state.IsButtonDown(10)) State.PressedButtons |= ControllerButtons.EbReset;
					if (state.IsButtonDown(11)) State.PressedButtons |= ControllerButtons.Ats;
					if (state.IsButtonDown(16)) State.PressedButtons |= ControllerButtons.Square;
					if (state.GetAxis(0) <= -0.25) State.PressedButtons |= ControllerButtons.PedalLight;
					if (state.GetAxis(0) <= -0.75) State.PressedButtons |= ControllerButtons.PedalStrong;
					if (state.GetAxis(0) >= 0.75) State.PressedButtons |= ControllerButtons.Horn;
					if (state.GetAxis(2) <= -0.75) State.PressedButtons |= ControllerButtons.PantographDown;
					if (state.GetAxis(2) >= 0.75) State.PressedButtons |= ControllerButtons.HillStart;

					// Turn on the door lamp
					try
					{
						outputBytes[4] = doorState == DoorStates.None ? (byte)1 : (byte)0;
						hidStream.Write(outputBytes);
					}
					catch
					{
					}
				}
			}
		}

		/// <summary>Closes the connection to the controller.</summary>
		internal override void Close()
		{
			// ZUIKI Mascon Pro
			if (controllerModel == ControllerModels.MasconPro)
			{
				// Turn off the door lamp
				try
				{
					outputBytes[4] = 0;
					hidStream.Write(outputBytes);
					hidStream.Close();
				}
				catch
				{
				}
			}
		}

	}
}
