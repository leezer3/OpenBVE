using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using OpenBveApi.Math;
using OpenBveApi.Interface;
using OpenTK.Input;
using ButtonState = OpenTK.Input.ButtonState;

namespace OpenBve.Input
{
	/// <summary>The abstract base RailDriver class</summary>
	internal abstract class AbstractRailDriver : AbstractJoystick
	{

		internal static readonly Guid Guid = new Guid("4d7641ef-95ce-44a5-b405-b051fb6139b4"); //completely random value, just used as a unique identifier



		internal readonly AxisCalibration[] Calibration = new AxisCalibration[7];

		internal void LoadCalibration(string calibrationFile)
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
													Program.FileSystem.AppendToLogFile(@"Invalid index in RailDriver calibration file");
												}

												break;
											case "minimum":
												if (!NumberFormats.TryParseIntVb6(n.InnerText, out lMin))
												{
													Program.FileSystem.AppendToLogFile(@"Invalid minimum in RailDriver calibration file");
												}

												break;
											case "maximum":
												if (!NumberFormats.TryParseIntVb6(n.InnerText, out lMax))
												{
													Program.FileSystem.AppendToLogFile(@"Invalid minimum in RailDriver calibration file");
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

				MessageBox.Show(Translations.GetInterfaceString("raildriver_config_error"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
				lines.Add("<Index>" + i + "</Index>");
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
		}

		internal override bool IsConnected()
		{
			return true;
		}

		internal override Guid GetGuid()
		{
			return Guid;
		}

		private static int ScaleValue(int value, int value_min, int value_max)
		{
			long temp = (value - value_min) * 65535;
			return (int) (temp / (value_max - value_min) + Int16.MinValue);
		}

		internal byte[] wData;

		internal abstract void SetDisplay(int speed);

		internal static byte GetDigit(int num)
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

}
