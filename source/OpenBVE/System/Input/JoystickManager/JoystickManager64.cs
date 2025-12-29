using System;
using OpenBveApi.Hosts;
using PIEHid64Net;

namespace OpenBve.Input
{
	internal class JoystickManager64 : JoystickManager
	{
		private readonly PIEDevice[] devices;

		internal JoystickManager64()
		{
			if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				devices = PIEDevice.EnumeratePIE();
			}
		}
		
		internal override int RailDriverCount => devices?.Length ?? 0;

		internal override void RefreshJoysticks()
		{
			for (int i = 0; i < 100; i++)
			{
				try
				{
					//Load the list of attached openTK joysticks
					var state = OpenTK.Input.Joystick.GetState(i);
					Guid foundGuid = OpenTK.Input.Joystick.GetGuid(i);
					var description = OpenTK.Input.Joystick.GetCapabilities(i);
					if (description.ButtonCount == 0 && description.HatCount == 0 && description.AxisCount == 0 && description.IsConnected)
					{
						// Broken joystick drivers- pointless attempting to go any further
						break;
					}
					// A joystick with 56 buttons and zero axis is likely the RailDriver, which is bugged in openTK
					// OpenTK also returns an empty GUID if we call GetState on an invalid joystick
					if (description.AxisCount == 0 && description.ButtonCount == 56 && description.IsConnected || foundGuid == Guid.Empty) continue;
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
				catch
				{
					// ignored
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
							RailDriver64 newJoystick = new RailDriver64(devices[i])
							{
								Name = "RailDriver Desktop Cab Controller (64-bit)",
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
