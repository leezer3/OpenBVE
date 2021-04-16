using System;
using PIEHid32Net;

namespace OpenBve
{
	internal partial class JoystickManager
	{
		/// <summary>A 32-bit RailDriver</summary>
		internal class RailDriver32 : AbstractRailDriver, PIEDataHandler, PIEErrorHandler
		{
			internal readonly PIEDevice myDevice;
			internal RailDriver32(PIEDevice device)
			{
				myDevice = device;
				for (int i = 0; i < Calibration.Length; i++)
				{
					Calibration[i] = new AxisCalibration();
				}
				LoadCalibration(OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "RailDriver.xml"));
			}
			
			internal override void Poll()
			{
				myDevice.ReadData(ref currentState);
			}
			
			internal override void SetDisplay(int speed)
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
				while (result == 404) { result = myDevice.WriteData(wData); }
				if (result != 0)
				{
					throw new Exception();
				}
			}
			
			internal override Guid GetGuid()
			{
				return Guid;
			}
			
			/// <summary>Callback function from the PI Engineering DLL, raised each time the device pushes a data packet</summary>
			/// <param name="data">The callback data</param>
			/// <param name="sourceDevice">The source device</param>
			/// <param name="error">The last error generated (if any)</param>
			public void HandlePIEHidData(byte[] data, PIEHid32Net.PIEDevice sourceDevice, int error)
			{
				if (myDevice == sourceDevice)
				{
					for (int r = 0; r < sourceDevice.ReadLength; r++)
					{
						currentState[r] = data[r];
					}
				}
			}

			/// <summary>Callback function from the PI Engineering DLL, raised if an error is encountered</summary>
			/// <param name="sourceDevices">The source device</param>
			/// <param name="error">The error</param>
			public void HandlePIEHidError(PIEHid32Net.PIEDevice sourceDevices, int error)
			{
			}
		}
	}
}
