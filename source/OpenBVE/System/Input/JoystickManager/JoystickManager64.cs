using System;
using OpenBveApi.Hosts;
using PIEHid64Net;

namespace OpenBve.Input
{
	class JoystickManager64 : JoystickManager
	{
		internal PIEDevice[] devices;

		internal JoystickManager64()
		{
			if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				devices = PIEDevice.EnumeratePIE();
			}
		}
		
		internal override int RailDriverCount
		{
			get
			{
				if (devices == null)
				{
					return 0;
				}
				return devices.Length;
			} 
		}

		internal override void RefreshJoysticks()
		{
			for (int i = 0; i < 100; i++)
			{
				//Load the list of attached openTK joysticks
				var state = OpenTK.Input.Joystick.GetState(i);
				Guid foundGuid = OpenTK.Input.Joystick.GetGuid(i);
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
			if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows || devices == null || RailDriverInit == true)
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
							RailDriver64 newJoystick = new RailDriver64(devices[i])
							{
								Name = "RailDriver Desktop Cab Controller (32-bit)",
								Handle = i,
								wData = new byte[]
								{
									0,134,0,0,0,0,0,0,0
								}
							};
							if (!AttachedJoysticks.ContainsKey(AbstractRailDriver.Guid))
							{
								AttachedJoysticks.Add(AbstractRailDriver.Guid, newJoystick);
								devices[i].SetupInterface();
								devices[i].SetDataCallback(newJoystick);
								devices[i].SetErrorCallback(newJoystick);
							}
							break;
					}

				}
			}
			

		}
	}
}
