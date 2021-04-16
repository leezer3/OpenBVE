using System;
using System.Collections.Generic;
using OpenBveApi.Hosts;
using OpenTK.Input;
using PIEHid32Net;

namespace OpenBve 
{
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal partial class JoystickManager
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

			if (IntPtr.Size == 4)
			{
				devices32 = PIEHid32Net.PIEDevice.EnumeratePIE();
			}
			else
			{
				devices64 = PIEHid64Net.PIEDevice.EnumeratePIE();
			}
		}

		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Dictionary<Guid, Joystick> AttachedJoysticks = new Dictionary<Guid, Joystick>();

		/// <summary>Holds all raildrivers and other PI Engineering controllers attached to the computer, when in 32-bit mode</summary>
		internal static PIEHid32Net.PIEDevice[] devices32 = { };

		/// <summary>Holds all raildrivers and other PI Engineering controllers attached to the computer, when in 64-bit mode</summary>
		internal static PIEHid64Net.PIEDevice[] devices64 = { };
		
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
			if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows || devices32 == null || RailDriverInit == true)
			{
				return;
			}
			//Enumerate all PI Engineering devices
			RailDriverInit = true;
			if (IntPtr.Size == 4)
			{
				for (int i = 0; i < devices32.Length; i++)
				{
					if (devices32[i].HidUsagePage == 0xc)
					{
						switch (devices32[i].Pid)
						{
							case 210:
								//Raildriver controller
								RailDriver32 newJoystick = new RailDriver32(devices32[i])
								{
									Name = "RailDriver Desktop Cab Controller (32-bit)",
									Handle = i,
									wData = new byte[]
									{
										0,134,0,0,0,0,0,0,0
									}
								};
								if (!AttachedJoysticks.ContainsKey(new Guid()))
								{
									AttachedJoysticks.Add(new Guid(), newJoystick);
									devices32[i].SetupInterface();
									devices32[i].SetDataCallback(newJoystick);
									devices32[i].SetErrorCallback(newJoystick);
								}
								break;
						}

					}
				}
			}
			else
			{
				for (int i = 0; i < devices64.Length; i++)
				{
					if (devices64[i].HidUsagePage == 0xc)
					{
						switch (devices64[i].Pid)
						{
							case 210:
								//Raildriver controller
								RailDriver64 newJoystick = new RailDriver64(devices64[i])
								{
									Name = "RailDriver Desktop Cab Controller (64-bit)",
									Handle = i,
									wData = new byte[]
									{
										0,134,0,0,0,0,0,0,0
									}
								};
								if (!AttachedJoysticks.ContainsKey(new Guid()))
								{
									AttachedJoysticks.Add(new Guid(), newJoystick);
									devices64[i].SetupInterface();
									devices64[i].SetDataCallback(newJoystick);
									devices64[i].SetErrorCallback(newJoystick);
								}
								break;
						}

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
