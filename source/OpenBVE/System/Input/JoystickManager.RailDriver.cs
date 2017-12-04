using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using OpenBveApi.Math;
using OpenTK.Input;
using PIEHid32Net;
using ButtonState = OpenTK.Input.ButtonState;

namespace OpenBve
{
	internal partial class JoystickManager
	{
		internal class Raildriver : Joystick
		{
			internal Raildriver()
			{
				for (int i = 0; i < Calibration.Length; i++)
				{
					Calibration[i] = new AxisCalibration();
				}
				LoadCalibration(OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "RailDriver.xml"));
			}

			internal AxisCalibration[] Calibration = new AxisCalibration[7];

			private void LoadCalibration(string calibrationFile)
			{
				if (!File.Exists(calibrationFile))
				{
					return;
				}
				try
				{
					for (int i = 0; i < Calibration.Length; i++)
					{
						Calibration[i] = new AxisCalibration();
					}
					XmlDocument currentXML = new XmlDocument();
					currentXML.Load(calibrationFile);
					XmlNodeList documentNodes = currentXML.SelectNodes("openBVE/RailDriverCalibration");
					if (documentNodes != null && documentNodes.Count != 0)
					{
						for (int i = 0; i < documentNodes.Count; i++)
						{
							int idx = -1;
							int lMin = 0;
							int lMax = 255;
							foreach (XmlNode node in documentNodes[i].ChildNodes)
							{
								switch (node.Name.ToLowerInvariant())
								{
									case "axis":
										foreach (XmlNode n in node.ChildNodes)
										{
											switch (n.Name.ToLowerInvariant())
											{
												case "index":
													if (!NumberFormats.TryParseIntVb6(n.InnerText, out idx))
													{
														Program.AppendToLogFile(@"Invalid index in RailDriver calibration file");
													}
													break;
												case "minimum":
													if (!NumberFormats.TryParseIntVb6(n.InnerText, out lMin))
													{
														Program.AppendToLogFile(@"Invalid minimum in RailDriver calibration file");
													}
													break;
												case "maximum":
													if (!NumberFormats.TryParseIntVb6(n.InnerText, out lMax))
													{
														Program.AppendToLogFile(@"Invalid minimum in RailDriver calibration file");
													}
													break;
											}
										}
										lMin = Math.Abs(lMin);
										lMax = Math.Abs(lMax);
										if (lMin > 255)
										{
											lMin = 255;
										}
										else if (lMin < 0)
										{
											lMin = 0;
										}
										if (lMax >= 255)
										{
											lMax = 255;
										}
										else if (lMax < 0)
										{
											lMax = 0;
										}
										if (lMin >= lMax)
										{
											throw new InvalidDataException(@"Maximum must be non-zero and greater than minimum.");
										}
										if (idx == -1)
										{
											throw new InvalidDataException(@"Invalid axis specified.");
										}
										Calibration[idx].Minimum = lMin;
										Calibration[idx].Maximum = lMax;
										break;
								}
							}
							
						}
					}
				}
				catch
				{
					for (int i = 0; i < Calibration.Length; i++)
					{
						Calibration[i] = new AxisCalibration();
					}
					MessageBox.Show(Interface.GetInterfaceString("raildriver_config_error"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					//Clear the calibration file
					File.Delete(calibrationFile);
				}
				
			}

			internal void SaveCalibration(string calibrationFile)
			{
				List<string> lines = new List<string>();
				lines.Add("<openBVE>");
				lines.Add("<RailDriverCalibration>");
				for (int i = 0; i < Calibration.Length; i++)
				{
					if (Calibration[i].Maximum < Calibration[i].Minimum)
					{
						//If calibration min and max are reversed flip them
						int t = Calibration[i].Maximum;
						Calibration[i].Maximum = Calibration[i].Minimum;
						Calibration[i].Minimum = t;
					}
					if (Calibration[i].Maximum == Calibration[i].Minimum)
					{
						//If calibration values are identical, reset to defaults
						Calibration[i].Minimum = 0;
						Calibration[i].Maximum = 255;
					}
					//Bounds check values
					if (Calibration[i].Minimum < 0)
					{
						Calibration[i].Minimum = 0;
					}
					if (Calibration[i].Maximum > 255)
					{
						Calibration[i].Maximum = 255;
					}
					lines.Add("<Axis>");
					lines.Add("<Index>"+ i +"</Index>");
					lines.Add("<Minimum>" + Calibration[i].Minimum + "</Minimum>");
					lines.Add("<Maximum>" + Calibration[i].Maximum + "</Maximum>");
					lines.Add("</Axis>");
				}
				lines.Add("</RailDriverCalibration>");
				lines.Add("</openBVE>");
				try
				{
					File.WriteAllLines(calibrationFile, lines);
				}
				catch
				{
				}
				
			}

			internal override ButtonState GetButton(int button)
			{
				return 1 == ((currentState[8 + (button / 8)] >> button % 8) & 1) ? ButtonState.Pressed : ButtonState.Released;
			}

			internal override double GetAxis(int axis)
			{
				return ScaleValue(currentState[axis + 1], Calibration[axis].Minimum, Calibration[axis].Maximum) * 1.0f / (short.MaxValue + 0.5f);
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

			internal class AxisCalibration
			{
				internal int Minimum = 0;
				internal int Maximum = 255;
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
