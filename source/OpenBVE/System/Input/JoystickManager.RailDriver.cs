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

			internal byte[] wData;

			internal void SetDisplay(int speed)
			{
				int d1 = (int)((double)speed / 100 % 10); //1 digit
				int d2 = (int)((double)speed / 10 % 10); //10 digit
				int d3 = (int)((double)speed % 10); //100 digit
				for (int i = 2; i < 5; i++)
				{
					switch (i)
					{
						//100 digit display
						case 2:
							wData[i] = GetDigit(d3);
							break;
						//10 digit display
						case 3:
							wData[i] = GetDigit(d2); 
							break;
						//1 digit display
						case 4:
							wData[i] = GetDigit(d1);
							break;
					}
				}
				int result = 404;
				while (result == 404) { result = devices[Handle].WriteData(wData); }
				if (result != 0)
				{
					throw new Exception();
				}
			}
			private byte GetDigit(int num)
			{
				num = Math.Abs(num);
				if (num > 9 || num < 0)
				{
					//Invalid data
					return 0;
				}
				switch (num)
				{
					case 0:
						return 63;
					case 1:
						return 6;
					case 2:
						return 91;
					case 3:
						return 79;
					case 4:
						return 102;
					case 5:
						return 109;
					case 6:
						return 125;
					case 7:
						return 7;
					case 8:
						return 127;
					case 9:
						return 103;
					default:
						return 0;
				}
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
