using System;
using OpenTK.Input;
using PIEHid32Net;

namespace OpenBve {
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal partial class JoystickManager 
	{
		internal JoystickManager()
		{
			if (!Program.CurrentlyRunningOnWindows)
			{
				return;
			}

			try
			{
				if (IntPtr.Size == 4)
				{
					Win32PIEDevices = PIEHid32Net.PIEDevice.EnumeratePIE();
				}
				else
				{
					Win64PIEDevices = PIEHid64Net.PIEDevice.EnumeratePIE();
				}
				
			}
			catch
			{
			}
			
		}

		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Joystick[] AttachedJoysticks = new Joystick[] { };

		/// <summary>Holds all raildrivers and other PI Engineering controllers attached to the computer</summary>
		/// <remarks>32-bit</remarks>
		internal static PIEHid32Net.PIEDevice[] Win32PIEDevices;

		/// <summary>Holds all raildrivers and other PI Engineering controllers attached to the computer</summary>
		/// <remarks>32-bit</remarks>
		internal static PIEHid64Net.PIEDevice[] Win64PIEDevices;

		private bool RailDriverInit = false;

		internal static int RailDriverIndex = -1;

		// --- functions ---

		/// <returns>Call this function to refresh the list of available joysticks and thier capabilities</returns>
		internal void RefreshJoysticks()
		{
			for (int i = 0; i < 10; i++)
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
					if (Program.CurrentlyRunningOnMono)
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
					StandardJoystick newJoystick = new StandardJoystick
					{
						Name = "Joystick" + i,
						Handle = i,

					};

					bool alreadyFound = false;
					for (int j = 0; j < AttachedJoysticks.Length; j++)
					{
						if (AttachedJoysticks[j] is StandardJoystick && AttachedJoysticks[j].Handle == newJoystick.Handle)
						{
							alreadyFound = true;
						}
					}
					if (!alreadyFound)
					{
						int l = AttachedJoysticks.Length;
						Array.Resize(ref AttachedJoysticks, AttachedJoysticks.Length + 1);
						AttachedJoysticks[l] = newJoystick;
					}
				}
			}
			if (!Program.CurrentlyRunningOnWindows || RailDriverInit == true)
			{
				return;
			}

			if ((IntPtr.Size == 4 && Win32PIEDevices == null) || (IntPtr.Size != 4 && Win64PIEDevices == null))
			{
				return;
			}

			//Enumerate all PI Engineering devices
			RailDriverInit = true;
			if (IntPtr.Size == 4)
			{
				for (int i = 0; i < Win32PIEDevices.Length; i++)
				{
					if (Win32PIEDevices[i].HidUsagePage == 0xc)
					{
						switch (Win32PIEDevices[i].Pid)
						{
							case 210:
								//Raildriver controller
								RailDriver32Bit newJoystick = new RailDriver32Bit(i);
								bool alreadyFound = false;
								for (int j = 0; j < AttachedJoysticks.Length; j++)
								{
									if (AttachedJoysticks[j] is RailDriver32Bit && AttachedJoysticks[j].Handle == newJoystick.Handle)
									{
										alreadyFound = true;
										break;
									}
								}

								if (!alreadyFound)
								{
									int l = AttachedJoysticks.Length;
									Array.Resize(ref AttachedJoysticks, AttachedJoysticks.Length + 1);
									AttachedJoysticks[l] = newJoystick;
									Win32PIEDevices[i].SetupInterface();
									Win32PIEDevices[i].SetDataCallback(newJoystick);
									Win32PIEDevices[i].SetErrorCallback(newJoystick);
									RailDriverIndex = l;
								}

								break;
						}

					}
				}
			}
			else
			{
				for (int i = 0; i < Win64PIEDevices.Length; i++)
				{
					if (Win64PIEDevices[i].HidUsagePage == 0xc)
					{
						switch (Win64PIEDevices[i].Pid)
						{
							case 210:
								//Raildriver controller
								RailDriver64Bit newJoystick = new RailDriver64Bit(i);
								bool alreadyFound = false;
								for (int j = 0; j < AttachedJoysticks.Length; j++)
								{
									if (AttachedJoysticks[j] is RailDriver64Bit && AttachedJoysticks[j].Handle == newJoystick.Handle)
									{
										alreadyFound = true;
										break;
									}
								}

								if (!alreadyFound)
								{
									int l = AttachedJoysticks.Length;
									Array.Resize(ref AttachedJoysticks, AttachedJoysticks.Length + 1);
									AttachedJoysticks[l] = newJoystick;
									Win64PIEDevices[i].SetupInterface();
									Win64PIEDevices[i].SetDataCallback(newJoystick);
									Win64PIEDevices[i].SetErrorCallback(newJoystick);
									RailDriverIndex = l;
								}

								break;
						}

					}
				}
			}

		}

		internal static ButtonState GetButton(int Device, int Button)
		{
			if (Device < AttachedJoysticks.Length)
			{
				return AttachedJoysticks[Device].GetButton(Button);
			}
			return ButtonState.Released;
		}

		internal static double GetAxis(int Device, int Axis)
		{
			if (Device < AttachedJoysticks.Length)
			{
				return AttachedJoysticks[Device].GetAxis(Axis);
			}
			return 0.0;
		}

		internal static JoystickHatState GetHat(int Device, int Hat)
		{
			if (Device < AttachedJoysticks.Length)
			{
				return AttachedJoysticks[Device].GetHat(Hat);
			}
			return new JoystickHatState();
		}



		public void HandlePIEHidError(PIEDevice sourceDevices, long error)
		{
			//Don't care
		}
	}
}
