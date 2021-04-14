using System;
using System.Collections.Generic;
using OpenBveApi.Hosts;
using OpenTK.Input;
using PIEHid32Net;

namespace OpenBve 
{
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal partial class JoystickManager : PIEDataHandler, PIEErrorHandler
	{

		/// <summary>Represents a joystick.</summary>
		internal abstract class Joystick
		{
			// --- members ---
			/// <summary>The textual representation of the joystick.</summary>
			internal string Name;
			/// <summary>The handle to the joystick.</summary>
			internal int Handle;

			internal byte[] currentState = new byte[15];

			internal abstract ButtonState GetButton(int button);

			internal abstract double GetAxis(int axis);

			internal abstract JoystickHatState GetHat(int Hat);

			internal abstract int AxisCount();

			internal abstract int ButtonCount();

			internal abstract int HatCount();

			internal abstract void Poll();

			internal abstract bool IsConnected();

			internal bool Disconnected = false;

			internal abstract Guid GetGuid();
		}

		internal JoystickManager()
		{
			if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows)
			{
				return;
			}
			devices = PIEDevice.EnumeratePIE();
		}

		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Dictionary<Guid, Joystick> AttachedJoysticks = new Dictionary<Guid, Joystick>();

		/// <summary>Holds all raildrivers and other PI Engineering controllers attached to the computer</summary>
		internal static PIEDevice[] devices;

		private bool RailDriverInit = false;

		internal static int RailDriverIndex = -1;

		// --- functions ---

		/// <returns>Call this function to refresh the list of available joysticks and thier capabilities</returns>
		internal void RefreshJoysticks()
		{
			for (int i = 0; i < 100; i++)
			{
				//Load the list of attached openTK joysticks
				var state = OpenTK.Input.Joystick.GetState(i);
				var description = OpenTK.Input.Joystick.GetCapabilities(i);
				if (description.ToString() == "{Axes: 0; Buttons: 0; Hats: 0; IsConnected: True}")
				{
					break;
				}
				//A joystick with 56 buttons and zero axis is likely the RailDriver, which is bugged in openTK
				if (description.ToString() != "{Axes: 0; Buttons: 56; Hats: 0; IsConnected: True}")
				{
					if (Program.CurrentHost.MonoRuntime)
					{
						if (description.AxisCount == 0 && description.ButtonCount == 0 && description.HatCount == 0)
						{
							continue;
						}
					}
					else
					{
						if (!state.IsConnected)
						{
							continue;
						}
					}

					StandardJoystick newJoystick = new StandardJoystick(i);

					if (AttachedJoysticks.ContainsKey(newJoystick.GetGuid()))
					{
						AttachedJoysticks[newJoystick.GetGuid()].Handle = i;
						AttachedJoysticks[newJoystick.GetGuid()].Disconnected = false;
					}
					else
					{
						AttachedJoysticks.Add(newJoystick.GetGuid(), newJoystick);
					}
				}
			}
			if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows || devices == null || RailDriverInit)
			{
				return;
			}
			//Enumerate all PI Engineering devices
			RailDriverInit = true;
			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i].HidUsagePage == 0xc)
				{
					switch (devices[i].Pid)
					{
						case 210:
							//Raildriver controller
							Raildriver newJoystick = new Raildriver
							{
								Name = "RailDriver Desktop Cab Controller",
								Handle = i,
								wData = new byte[]
								{
									0,134,0,0,0,0,0,0,0
								}
							};
							if (!AttachedJoysticks.ContainsKey(new Guid()))
							{
								AttachedJoysticks.Add(new Guid(), newJoystick);
								devices[i].SetupInterface();
								devices[i].SetDataCallback(this);
								devices[i].SetErrorCallback(this);
							}
							break;
					}

				}
			}

		}

		internal static ButtonState GetButton(Guid Device, int Button)
		{
			if (AttachedJoysticks.ContainsKey(Device))
			{
				return AttachedJoysticks[Device].GetButton(Button);
			}
			return ButtonState.Released;
		}

		internal static double GetAxis(Guid Device, int Axis)
		{
			if (AttachedJoysticks.ContainsKey(Device))
			{
				return AttachedJoysticks[Device].GetAxis(Axis);
			}
			return 0.0;
		}

		internal static JoystickHatState GetHat(Guid Device, int Hat)
		{
			if (AttachedJoysticks.ContainsKey(Device))
			{
				return AttachedJoysticks[Device].GetHat(Hat);
			}
			return new JoystickHatState();
		}
	}
}
