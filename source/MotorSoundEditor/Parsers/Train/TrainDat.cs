using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MotorSoundEditor.Parsers.Train
{
	internal static partial class TrainDat
	{
		private const int currentVersion = 15311;

		/// <summary>
		/// Loads a file into an instance of the Train class.
		/// </summary>
		/// <param name="FileName">The train.dat file to load.</param>
		/// <returns>An instance of the Train class.</returns>
		internal static Train Load(string FileName)
		{
			Train t = new Train();
			t.Pressure.BrakePipeNormalPressure = 0.0;
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string[] Lines = File.ReadAllLines(FileName, new UTF8Encoding());

			for (int i = 0; i < Lines.Length; i++)
			{
				int j = Lines[i].IndexOf(';');

				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim(new char[] {' '});
				}
				else
				{
					Lines[i] = Lines[i].Trim(new char[] {' '});
				}
			}

			bool ver1220000 = false;

			foreach (string line in Lines)
			{
				if (line.Length != 0)
				{
					string s = line.ToLowerInvariant();

					switch (s)
					{
						case "bve1200000":
						case "bve1210000":
						case "bve1220000":
							ver1220000 = true;
							break;
						case "bve2000000":
						case "openbve":
							//No action
							break;
						default:
							if (s.ToLowerInvariant().StartsWith("openbve"))
							{
								string tt = s.Substring(7, s.Length - 7);
								int v;

								if (int.TryParse(tt, NumberStyles.Float, Culture, out v))
								{
									if (v > currentVersion)
									{
										MessageBox.Show(string.Format("The train.dat {0} was created with a newer version of openBVE. Please check for an update.", FileName));
									}
								}
								else
								{
									MessageBox.Show(string.Format("The train.dat version {0} is invalid in {1}", Lines[0].ToLowerInvariant(), FileName));
								}
							}
							break;
					}
					break;
				}
			}

			for (int i = 0; i < Lines.Length; i++)
			{
				int n = 0;

				switch (Lines[i].ToLowerInvariant())
				{
					case "#acceleration":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							if (n == t.Acceleration.Entries.Length)
							{
								Array.Resize(ref t.Acceleration.Entries, t.Acceleration.Entries.Length << 1);
							}

							string u = Lines[i] + ",";
							int m = 0;

							while (true)
							{
								int j = u.IndexOf(',');

								if (j == -1)
								{
									break;
								}

								string s = u.Substring(0, j).Trim(new char[] {' '});
								u = u.Substring(j + 1);
								double a;

								if (double.TryParse(s, NumberStyles.Float, Culture, out a))
								{
									switch (m)
									{
										case 0:
											t.Acceleration.Entries[n].a0 = Math.Max(a, 0.0);
											break;
										case 1:
											t.Acceleration.Entries[n].a1 = Math.Max(a, 0.0);
											break;
										case 2:
											t.Acceleration.Entries[n].v1 = Math.Max(a, 0.0);
											break;
										case 3:
											t.Acceleration.Entries[n].v2 = Math.Max(a, 0.0);

											if (t.Acceleration.Entries[n].v2 < t.Acceleration.Entries[n].v1)
											{
												double x = t.Acceleration.Entries[n].v1;
												t.Acceleration.Entries[n].v1 = t.Acceleration.Entries[n].v2;
												t.Acceleration.Entries[n].v2 = x;
											}
											break;
										case 4:
											if (ver1220000)
											{
												if (a <= 0.0)
												{
													t.Acceleration.Entries[n].e = 1.0;
												}
												else
												{
													const double c = 1.23315173118822;
													t.Acceleration.Entries[n].e = 1.0 - Math.Log(a) * t.Acceleration.Entries[n].v2 * c;

													if (t.Acceleration.Entries[n].e > 4.0)
													{
														t.Acceleration.Entries[n].e = 4.0;
													}
												}
											}
											else
											{
												t.Acceleration.Entries[n].e = a;
											}
											break;
									}
								}

								m++;
							}

							i++;
							n++;
						}

						Array.Resize(ref t.Acceleration.Entries, n);
						i--;
						break;
					case "#performance":
					case "#deceleration":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a >= 0.0)
										{
											t.Performance.Deceleration = a;
										}
										break;
									case 1:
										if (a >= 0.0)
										{
											t.Performance.CoefficientOfStaticFriction = a;
										}
										break;
									case 3:
										if (a >= 0.0)
										{
											t.Performance.CoefficientOfRollingResistance = a;
										}
										break;
									case 4:
										if (a >= 0.0)
										{
											t.Performance.AerodynamicDragCoefficient = a;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#delay":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a >= 0.0)
										{
											t.Delay.DelayPowerUp = new[] { a };
										}
										break;
									case 1:
										if (a >= 0.0)
										{
											t.Delay.DelayPowerDown = new[] { a };
										}
										break;
									case 2:
										if (a >= 0.0)
										{
											t.Delay.DelayBrakeUp = new[] { a };
										}
										break;
									case 3:
										if (a >= 0.0)
										{
											t.Delay.DelayBrakeDown = new[] { a };
										}
										break;
								}
							}
							else if (Lines[i].IndexOf(',') != -1)
							{
								switch (n)
								{
									case 0:
										t.Delay.DelayPowerUp = Lines[i].Split(new char[] {','}).Select(x => double.Parse(x, Culture)).ToArray();
										break;
									case 1:
										t.Delay.DelayPowerDown = Lines[i].Split(new char[] {','}).Select(x => double.Parse(x, Culture)).ToArray();
										break;
									case 2:
										t.Delay.DelayBrakeUp = Lines[i].Split(new char[] {','}).Select(x => double.Parse(x, Culture)).ToArray();
										break;
									case 3:
										t.Delay.DelayBrakeDown = Lines[i].Split(new char[] {','}).Select(x => double.Parse(x, Culture)).ToArray();
										break;
									case 4:
										t.Delay.DelayLocoBrakeUp = Lines[i].Split(new char[] {','}).Select(x => double.Parse(x, Culture)).ToArray();
										break;
									case 5:
										t.Delay.DelayLocoBrakeDown = Lines[i].Split(new char[] {','}).Select(x => double.Parse(x, Culture)).ToArray();
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#move":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a >= 0.0)
										{
											t.Move.JerkPowerUp = a;
										}
										break;
									case 1:
										if (a >= 0.0)
										{
											t.Move.JerkPowerDown = a;
										}
										break;
									case 2:
										if (a >= 0.0)
										{
											t.Move.JerkBrakeUp = a;
										}
										break;
									case 3:
										if (a >= 0.0)
										{
											t.Move.JerkBrakeDown = a;
										}
										break;
									case 4:
										if (a >= 0.0)
										{
											t.Move.BrakeCylinderUp = a;
										}
										break;
									case 5:
										if (a >= 0.0)
										{
											t.Move.BrakeCylinderDown = a;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#brake":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (b >= 0 & b <= 2)
										{
											t.Brake.BrakeType = (Brake.BrakeTypes)b;
										}
										break;
									case 1:
										if (b >= 0 & b <= 2)
										{
											t.Brake.BrakeControlSystem = (Brake.BrakeControlSystems)b;
										}
										break;
									case 2:
										if (a >= 0.0)
										{
											t.Brake.BrakeControlSpeed = a;
										}
										break;
									case 3:
										if (a <= 0 && a > 3)
										{
											t.Brake.LocoBrakeType = (Brake.LocoBrakeTypes)b;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#pressure":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a > 0.0)
										{
											t.Pressure.BrakeCylinderServiceMaximumPressure = a;
										}
										break;
									case 1:
										if (a > 0.0)
										{
											t.Pressure.BrakeCylinderEmergencyMaximumPressure = a;
										}
										break;
									case 2:
										if (a > 0.0)
										{
											t.Pressure.MainReservoirMinimumPressure = a;
										}
										break;
									case 3:
										if (a > 0.0)
										{
											t.Pressure.MainReservoirMaximumPressure = a;
										}
										break;
									case 4:
										if (a > 0.0)
										{
											t.Pressure.BrakePipeNormalPressure = a;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#handle":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (b == 0 | b == 1)
										{
											t.Handle.HandleType = (Handle.HandleTypes)b;
										}
										break;
									case 1:
										if (b > 0)
										{
											t.Handle.PowerNotches = b;
										}
										break;
									case 2:
										if (b > 0)
										{
											t.Handle.BrakeNotches = b;
										}
										break;
									case 3:
										if (b >= 0)
										{
											t.Handle.PowerNotchReduceSteps = b;
										}
										break;
									case 4:
										if (a >= 0 && a < 4)
										{
											t.Handle.HandleBehaviour = (Handle.EbHandleBehaviour)b;
										}
										break;
									case 5:
										if (b > 0)
										{
											t.Handle.LocoBrakeNotches = b;
										}
										break;
									case 6:
										if (a <= 0 && a > 3)
										{
											t.Handle.LocoBrake = (Handle.LocoBrakeType)b;
										}
										break;
									case 7:
										if (b > 0)
										{
											t.Handle.DriverPowerNotches = b;
										}
										break;
									case 8:
										if (b > 0)
										{
											t.Handle.DriverBrakeNotches = b;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#cockpit":
					case "#cab":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								switch (n)
								{
									case 0:
										t.Cab.X = a;
										break;
									case 1:
										t.Cab.Y = a;
										break;
									case 2:
										t.Cab.Z = a;
										break;
									case 3:
										t.Cab.DriverCar = (int)Math.Round(a);
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#car":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (a > 0.0)
										{
											t.Car.MotorCarMass = a;
										}
										break;
									case 1:
										if (b >= 1)
										{
											t.Car.NumberOfMotorCars = b;
										}
										break;
									case 2:
										if (a > 0.0)
										{
											t.Car.TrailerCarMass = a;
										}
										break;
									case 3:
										if (b >= 0)
										{
											t.Car.NumberOfTrailerCars = b;
										}
										break;
									case 4:
										if (b > 0.0)
										{
											t.Car.LengthOfACar = a;
										}
										break;
									case 5:
										t.Car.FrontCarIsAMotorCar = a == 1.0;
										break;
									case 6:
										if (a > 0.0)
										{
											t.Car.WidthOfACar = a;
										}
										break;
									case 7:
										if (a > 0.0)
										{
											t.Car.HeightOfACar = a;
										}
										break;
									case 8:
										t.Car.CenterOfGravityHeight = a;
										break;
									case 9:
										if (a > 0.0)
										{
											t.Car.ExposedFrontalArea = a;
										}
										break;
									case 10:
										if (a > 0.0)
										{
											t.Car.UnexposedFrontalArea = a;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#device":
						i++;

						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(Lines[i], NumberStyles.Float, Culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (b >= -1 & b <= 1)
										{
											t.Device.Ats = (Device.AtsModes)b;
										}
										break;
									case 1:
										if (b >= 0 & b <= 2)
										{
											t.Device.Atc = (Device.AtcModes)b;
										}
										break;
									case 2:
										t.Device.Eb = a == 1.0;
										break;
									case 3:
										t.Device.ConstSpeed = a == 1.0;
										break;
									case 4:
										t.Device.HoldBrake = a == 1.0;
										break;
									case 5:
										if (b >= -1 & b <= 3)
										{
											t.Device.ReAdhesionDevice = (Device.ReAdhesionDevices)b;
										}
										break;
									case 6:
										t.Device.LoadCompensatingDevice = a;
										break;
									case 7:
										if (b >= 0 & b <= 2)
										{
											t.Device.PassAlarm = (Device.PassAlarmModes)b;
										}
										break;
									case 8:
										if (b >= 0 & b <= 2)
										{
											t.Device.DoorOpenMode = (Device.DoorModes)b;
										}
										break;
									case 9:
										if (b >= 0 & b <= 2)
										{
											t.Device.DoorCloseMode = (Device.DoorModes)b;
										}
										break;
									case 10:
										if (a >= 0.0)
										{
											t.Device.DoorWidth = a;
										}
										break;
									case 11:
										if (a >= 0.0)
										{
											t.Device.DoorMaxTolerance = a;
										}
										break;
								}
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#motor_p1":
					case "#motor_p2":
					case "#motor_b1":
					case "#motor_b2":
						{
							string section = Lines[i].ToLowerInvariant();
							i++;
							Motor m = new Motor();

							while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
							{
								if (n == m.Entries.Length)
								{
									Array.Resize(ref m.Entries, m.Entries.Length << 1);
								}

								string u = Lines[i] + ",";
								int k = 0;

								while (true)
								{
									int j = u.IndexOf(',');

									if (j == -1)
									{
										break;
									}

									string s = u.Substring(0, j).Trim(new char[] {' '});
									u = u.Substring(j + 1);
									double a;

									if (double.TryParse(s, NumberStyles.Float, Culture, out a))
									{
										int b = (int)Math.Round(a);

										switch (k)
										{
											case 0:
												m.Entries[n].SoundIndex = b >= 0 ? b : -1;
												break;
											case 1:
												m.Entries[n].Pitch = Math.Max(a, 0.0);
												break;
											case 2:
												m.Entries[n].Volume = Math.Max(a, 0.0);
												break;
										}
									}
									k++;
								}

								i++;
								n++;
							}

							Array.Resize(ref m.Entries, n);
							i--;

							switch (section)
							{
								case "#motor_p1":
									t.MotorP1 = m;
									break;
								case "#motor_p2":
									t.MotorP2 = m;
									break;
								case "#motor_b1":
									t.MotorB1 = m;
									break;
								case "#motor_b2":
									t.MotorB2 = m;
									break;
							}
						}
						break;
				}
			}

			if (t.Delay.DelayPowerUp.Length < t.Handle.PowerNotches)
			{
				int l = t.Delay.DelayPowerUp.Length;
				Array.Resize(ref t.Delay.DelayPowerUp, t.Handle.PowerNotches);

				for (int i = l + 1; i < t.Delay.DelayPowerUp.Length; i++)
				{
					t.Delay.DelayPowerUp[i] = 0;
				}
			}

			if (t.Delay.DelayPowerDown.Length < t.Handle.PowerNotches)
			{
				int l = t.Delay.DelayPowerDown.Length;
				Array.Resize(ref t.Delay.DelayPowerDown, t.Handle.PowerNotches);

				for (int i = l + 1; i < t.Delay.DelayPowerDown.Length; i++)
				{
					t.Delay.DelayPowerDown[i] = 0;
				}
			}

			if (t.Pressure.BrakePipeNormalPressure <= 0.0)
			{
				if (t.Brake.BrakeType == Brake.BrakeTypes.AutomaticAirBrake)
				{
					t.Pressure.BrakePipeNormalPressure = t.Pressure.BrakeCylinderEmergencyMaximumPressure + 0.75 * (t.Pressure.MainReservoirMinimumPressure - t.Pressure.BrakeCylinderEmergencyMaximumPressure);

					if (t.Pressure.BrakePipeNormalPressure > t.Pressure.MainReservoirMinimumPressure)
					{
						t.Pressure.BrakePipeNormalPressure = t.Pressure.MainReservoirMinimumPressure;
					}
				}
				else
				{
					if (t.Pressure.BrakeCylinderEmergencyMaximumPressure < 480000.0 & t.Pressure.MainReservoirMinimumPressure > 500000.0)
					{
						t.Pressure.BrakePipeNormalPressure = 490000.0;
					}
					else
					{
						t.Pressure.BrakePipeNormalPressure = t.Pressure.BrakeCylinderEmergencyMaximumPressure + 0.75 * (t.Pressure.MainReservoirMinimumPressure - t.Pressure.BrakeCylinderEmergencyMaximumPressure);
					}
				}
			}

			if (t.Brake.BrakeType == Brake.BrakeTypes.AutomaticAirBrake)
			{
				t.Device.HoldBrake = false;
			}

			if (t.Device.HoldBrake & t.Handle.BrakeNotches <= 0)
			{
				t.Handle.BrakeNotches = 1;
			}

			if (t.Cab.DriverCar < 0 | t.Cab.DriverCar >= t.Car.NumberOfMotorCars + t.Car.NumberOfTrailerCars)
			{
				t.Cab.DriverCar = 0;
			}

			return t;
		}

		/// <summary>
		/// Saves an instance of the Train class into a specified file.
		/// </summary>
		/// <param name="FileName">The train.dat file to save.</param>
		/// <param name="t">An instance of the Train class to save.</param>
		internal static void Save(string FileName, Train t)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			StringBuilder b = new StringBuilder();
			b.AppendLine("OPENBVE" + currentVersion);
			b.AppendLine("#ACCELERATION");

			if (t.Acceleration.Entries.Length > t.Handle.PowerNotches)
			{
				Array.Resize(ref t.Acceleration.Entries, t.Handle.PowerNotches);
			}

			for (int i = 0; i < t.Acceleration.Entries.Length; i++)
			{
				b.Append(t.Acceleration.Entries[i].a0.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].a1.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].v1.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].v2.ToString(Culture) + ",");
				b.AppendLine(t.Acceleration.Entries[i].e.ToString(Culture));
			}

			int n = 15;
			b.AppendLine("#PERFORMANCE");
			b.AppendLine(t.Performance.Deceleration.ToString(Culture).PadRight(n, ' ') + "; Deceleration");
			b.AppendLine(t.Performance.CoefficientOfStaticFriction.ToString(Culture).PadRight(n, ' ') + "; CoefficientOfStaticFriction");
			b.AppendLine("0".PadRight(n, ' ') + "; Reserved (not used)");
			b.AppendLine(t.Performance.CoefficientOfRollingResistance.ToString(Culture).PadRight(n, ' ') + "; CoefficientOfRollingResistance");
			b.AppendLine(t.Performance.AerodynamicDragCoefficient.ToString(Culture).PadRight(n, ' ') + "; AerodynamicDragCoefficient");
			b.AppendLine("#DELAY");
			b.AppendLine(string.Join(",", t.Delay.DelayPowerUp.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayPowerUp");
			b.AppendLine(string.Join(",", t.Delay.DelayPowerDown.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayPowerDown");
			b.AppendLine(string.Join(",", t.Delay.DelayBrakeUp.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayBrakeUp");
			b.AppendLine(string.Join(",", t.Delay.DelayBrakeDown.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayBrakeDown");
			b.AppendLine(string.Join(",", t.Delay.DelayLocoBrakeUp.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayLocoBrakeUp (1.5.3.4+)");
			b.AppendLine(string.Join(",", t.Delay.DelayLocoBrakeDown.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayLocoBrakeDown (1.5.3.4+)");
			b.AppendLine("#MOVE");
			b.AppendLine(t.Move.JerkPowerUp.ToString(Culture).PadRight(n, ' ') + "; JerkPowerUp");
			b.AppendLine(t.Move.JerkPowerDown.ToString(Culture).PadRight(n, ' ') + "; JerkPowerDown");
			b.AppendLine(t.Move.JerkBrakeUp.ToString(Culture).PadRight(n, ' ') + "; JerkBrakeUp");
			b.AppendLine(t.Move.JerkBrakeDown.ToString(Culture).PadRight(n, ' ') + "; JerkBrakeDown");
			b.AppendLine(t.Move.BrakeCylinderUp.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderUp");
			b.AppendLine(t.Move.BrakeCylinderDown.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderDown");
			b.AppendLine("#BRAKE");
			b.AppendLine(((int)t.Brake.BrakeType).ToString(Culture).PadRight(n, ' ') + "; BrakeType");
			b.AppendLine(((int)t.Brake.BrakeControlSystem).ToString(Culture).PadRight(n, ' ') + "; BrakeControlSystem");
			b.AppendLine(t.Brake.BrakeControlSpeed.ToString(Culture).PadRight(n, ' ') + "; BrakeControlSpeed");
			b.AppendLine(((int)t.Brake.LocoBrakeType).ToString(Culture).PadRight(n, ' ') + "; LocoBrakeType (1.5.3.4+)");
			b.AppendLine("#PRESSURE");
			b.AppendLine(t.Pressure.BrakeCylinderServiceMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderServiceMaximumPressure");
			b.AppendLine(t.Pressure.BrakeCylinderEmergencyMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderEmergencyMaximumPressure");
			b.AppendLine(t.Pressure.MainReservoirMinimumPressure.ToString(Culture).PadRight(n, ' ') + "; MainReservoirMinimumPressure");
			b.AppendLine(t.Pressure.MainReservoirMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; MainReservoirMaximumPressure");
			b.AppendLine(t.Pressure.BrakePipeNormalPressure.ToString(Culture).PadRight(n, ' ') + "; BrakePipeNormalPressure");
			b.AppendLine("#HANDLE");
			b.AppendLine(((int)t.Handle.HandleType).ToString(Culture).PadRight(n, ' ') + "; HandleType");
			b.AppendLine(t.Handle.PowerNotches.ToString(Culture).PadRight(n, ' ') + "; PowerNotches");
			b.AppendLine(t.Handle.BrakeNotches.ToString(Culture).PadRight(n, ' ') + "; BrakeNotches");
			b.AppendLine(t.Handle.PowerNotchReduceSteps.ToString(Culture).PadRight(n, ' ') + "; PowerNotchReduceSteps");
			b.AppendLine(((int)t.Handle.HandleBehaviour).ToString(Culture).PadRight(n, ' ') + "; EbHandleBehaviour (1.5.3.3+)");
			b.AppendLine(t.Handle.LocoBrakeNotches.ToString(Culture).PadRight(n, ' ') + "; LocoBrakeNotches (1.5.3.4+)");
			b.AppendLine(((int)t.Handle.LocoBrake).ToString(Culture).PadRight(n, ' ') + "; LocoBrakeType (1.5.3.4+)");
			b.AppendLine(t.Handle.DriverPowerNotches.ToString(Culture).PadRight(n, ' ') + "; DriverPowerNotches");
			b.AppendLine(t.Handle.DriverBrakeNotches.ToString(Culture).PadRight(n, ' ') + "; DriverBrakeNotches");
			b.AppendLine("#CAB");
			b.AppendLine(t.Cab.X.ToString(Culture).PadRight(n, ' ') + "; X");
			b.AppendLine(t.Cab.Y.ToString(Culture).PadRight(n, ' ') + "; Y");
			b.AppendLine(t.Cab.Z.ToString(Culture).PadRight(n, ' ') + "; Z");
			b.AppendLine(t.Cab.DriverCar.ToString(Culture).PadRight(n, ' ') + "; DriverCar");
			b.AppendLine("#CAR");
			b.AppendLine(t.Car.MotorCarMass.ToString(Culture).PadRight(n, ' ') + "; MotorCarMass");
			b.AppendLine(t.Car.NumberOfMotorCars.ToString(Culture).PadRight(n, ' ') + "; NumberOfMotorCars");
			b.AppendLine(t.Car.TrailerCarMass.ToString(Culture).PadRight(n, ' ') + "; TrailerCarMass");
			b.AppendLine(t.Car.NumberOfTrailerCars.ToString(Culture).PadRight(n, ' ') + "; NumberOfTrailerCars");
			b.AppendLine(t.Car.LengthOfACar.ToString(Culture).PadRight(n, ' ') + "; LengthOfACar");
			b.AppendLine((t.Car.FrontCarIsAMotorCar ? "1" : "0").PadRight(n, ' ') + "; FrontCarIsAMotorCar");
			b.AppendLine(t.Car.WidthOfACar.ToString(Culture).PadRight(n, ' ') + "; WidthOfACar");
			b.AppendLine(t.Car.HeightOfACar.ToString(Culture).PadRight(n, ' ') + "; HeightOfACar");
			b.AppendLine(t.Car.CenterOfGravityHeight.ToString(Culture).PadRight(n, ' ') + "; CenterOfGravityHeight");
			b.AppendLine(t.Car.ExposedFrontalArea.ToString(Culture).PadRight(n, ' ') + "; ExposedFrontalArea");
			b.AppendLine(t.Car.UnexposedFrontalArea.ToString(Culture).PadRight(n, ' ') + "; UnexposedFrontalArea");
			b.AppendLine("#DEVICE");
			b.AppendLine(((int)t.Device.Ats).ToString(Culture).PadRight(n, ' ') + "; Ats");
			b.AppendLine(((int)t.Device.Atc).ToString(Culture).PadRight(n, ' ') + "; Atc");
			b.AppendLine((t.Device.Eb ? "1" : "0").PadRight(n, ' ') + "; Eb");
			b.AppendLine((t.Device.ConstSpeed ? "1" : "0").PadRight(n, ' ') + "; ConstSpeed");
			b.AppendLine((t.Device.HoldBrake ? "1" : "0").PadRight(n, ' ') + "; HoldBrake");
			b.AppendLine(((int)t.Device.ReAdhesionDevice).ToString(Culture).PadRight(n, ' ') + "; ReAdhesionDevice");
			b.AppendLine(t.Device.LoadCompensatingDevice.ToString(Culture).PadRight(n, ' ') + "; Reserved (not used)");
			b.AppendLine(((int)t.Device.PassAlarm).ToString(Culture).PadRight(n, ' ') + "; PassAlarm");
			b.AppendLine(((int)t.Device.DoorOpenMode).ToString(Culture).PadRight(n, ' ') + "; DoorOpenMode");
			b.AppendLine(((int)t.Device.DoorCloseMode).ToString(Culture).PadRight(n, ' ') + "; DoorCloseMode");
			b.AppendLine(t.Device.DoorWidth.ToString(Culture).PadRight(n, ' ') + "; DoorWidth");
			b.AppendLine(t.Device.DoorMaxTolerance.ToString(Culture).PadRight(n, ' ') + "; DoorMaxTolerance");

			for (int i = 0; i < 4; i++)
			{
				Motor m = null;

				switch (i)
				{
					case 0:
						b.AppendLine("#MOTOR_P1");
						m = t.MotorP1;
						break;
					case 1:
						b.AppendLine("#MOTOR_P2");
						m = t.MotorP2;
						break;
					case 2:
						b.AppendLine("#MOTOR_B1");
						m = t.MotorB1;
						break;
					case 3:
						b.AppendLine("#MOTOR_B2");
						m = t.MotorB2;
						break;
				}

				int k;

				for (k = m.Entries.Length - 1; k >= 0; k--)
				{
					if (m.Entries[k].SoundIndex >= 0)
					{
						break;
					}
				}

				k = Math.Min(k + 2, m.Entries.Length);
				Array.Resize(ref m.Entries, k);

				for (int j = 0; j < m.Entries.Length; j++)
				{
					b.Append(m.Entries[j].SoundIndex.ToString(Culture) + ",");
					b.Append(m.Entries[j].Pitch.ToString(Culture) + ",");
					b.AppendLine(m.Entries[j].Volume.ToString(Culture));
				}
			}

			File.WriteAllText(FileName, b.ToString(), new UTF8Encoding(true));
		}
	}
}
