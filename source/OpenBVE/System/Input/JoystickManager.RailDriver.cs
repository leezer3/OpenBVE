using System;
using OpenTK.Input;
using PIEHid32Net;

namespace OpenBve
{
	internal partial class JoystickManager
	{
		internal class Raildriver : Joystick
		{
			internal override ButtonState GetButton(int button)
			{
				return 1 == ((currentState[8 + (button / 8)] >> button % 8) & 1) ? ButtonState.Pressed : ButtonState.Released;
			}

			internal override double GetAxis(int axis)
			{
				return ScaleValue(currentState[axis + 1], 0, 255) * 1.0f / (short.MaxValue + 0.5f);
			}

			internal override JoystickHatState GetHat(int Hat)
			{
				throw new NotImplementedException();
			}

			internal override int AxisCount()
			{
				return 7;
			}

			internal override int ButtonCount()
			{
				return 44;
			}

			internal override int HatCount()
			{
				return 0;
			}

			internal override void Poll()
			{
				devices[0].ReadData(ref currentState);
			}

			private int ScaleValue(int value, int value_min, int value_max)
			{
				long temp = (value - value_min) * 65535;
				return (int)(temp / (value_max - value_min) + Int16.MinValue);
			}
		}
		/// <summary>Callback function from the PI Engineering DLL, raised each time the device pushes a data packet</summary>
		/// <param name="data">The callback data</param>
		/// <param name="sourceDevice">The source device</param>
		/// <param name="error">The last error generated (if any)</param>
		public void HandlePIEHidData(Byte[] data, PIEDevice sourceDevice, int error)
		{
			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i] == sourceDevice)
				{
					//Source device found, so map it
					for (int j = 0; j < AttachedJoysticks.Length; j++)
					{
						if (AttachedJoysticks[j] is Raildriver && AttachedJoysticks[j].Handle == i)
						{
							for (int r = 0; r < sourceDevice.ReadLength; r++)
							{
								AttachedJoysticks[j].currentState[r] = data[r];
							}
						}
					}
				}
			}
		}

		/// <summary>Callback function from the PI Engineering DLL, raised if an error is encountered</summary>
		/// <param name="sourceDevices">The source device</param>
		/// <param name="error">The error</param>
		public void HandlePIEHidError(PIEDevice sourceDevices, int error)
		{
		}
	}
}
