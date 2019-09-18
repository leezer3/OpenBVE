using System;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Interface;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.IO.Trains.TrainDat
{
	internal static partial class TrainDat
	{
		private const int currentVersion = 15311;

		internal static void Parse(string fileName, out Train train)
		{
			train = new Train();

			CultureInfo culture = CultureInfo.InvariantCulture;
			string[] lines = File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName));

			for (int i = 0; i < lines.Length; i++)
			{
				int j = lines[i].IndexOf(';');

				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim();
				}
				else
				{
					lines[i] = lines[i].Trim();
				}
			}

			bool ver1220000 = false;

			foreach (string line in lines)
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

								if (int.TryParse(tt, NumberStyles.Float, culture, out v))
								{
									if (v > currentVersion)
									{
										Interface.AddMessage(MessageType.Warning, false, $"The train.dat {fileName} was created with a newer version of openBVE. Please check for an update.");
									}
								}
								else
								{
									Interface.AddMessage(MessageType.Error, false, $"The train.dat version {lines[0].ToLowerInvariant()} is invalid in {fileName}");
								}
							}

							break;
					}

					break;
				}
			}

			Acceleration acceleration = new Acceleration();
			Performance performance = new Performance();
			Delay delay = new Delay();
			Move move = new Move();
			Brake brake = new Brake();
			Pressure pressure = new Pressure();
			Motor motor = new Motor();

			double motorCarMass = 40.0;
			int numberOfMotorCars = 1;
			double trailerCarMass = 40.0;
			int numberOfTrailerCars = 1;
			double lengthOfACar = 20.0;
			bool frontCarIsAMotorCar = false;
			double widthOfACar = 2.6;
			double heightOfACar = 3.2;
			double centerOfGravityHeight = 1.5;
			double exposedFrontalArea = 5.0;
			double unexposedFrontalArea = 1.6;

			for (int i = 0; i < lines.Length; i++)
			{
				int n = 0;

				switch (lines[i].ToLowerInvariant())
				{
					case "#acceleration":
						i++;

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							if (n == acceleration.Entries.Count)
							{
								acceleration.Entries.Add(new Acceleration.Entry());
							}

							string u = lines[i] + ",";
							int m = 0;

							while (true)
							{
								int j = u.IndexOf(',');

								if (j == -1)
								{
									break;
								}

								string s = u.Substring(0, j).Trim();
								u = u.Substring(j + 1);
								double a;

								if (double.TryParse(s, NumberStyles.Float, culture, out a))
								{
									switch (m)
									{
										case 0:
											acceleration.Entries[n].A0 = Math.Max(a, 0.0);
											break;
										case 1:
											acceleration.Entries[n].A1 = Math.Max(a, 0.0);
											break;
										case 2:
											acceleration.Entries[n].V1 = Math.Max(a, 0.0);
											break;
										case 3:
											acceleration.Entries[n].V2 = Math.Max(a, 0.0);

											if (acceleration.Entries[n].V2 < acceleration.Entries[n].V1)
											{
												double x = acceleration.Entries[n].V1;
												acceleration.Entries[n].V1 = acceleration.Entries[n].V2;
												acceleration.Entries[n].V2 = x;
											}

											break;
										case 4:
											if (ver1220000)
											{
												if (a <= 0.0)
												{
													acceleration.Entries[n].E = 1.0;
												}
												else
												{
													const double c = 1.23315173118822;
													acceleration.Entries[n].E = 1.0 - Math.Log(a) * acceleration.Entries[n].V2 * c;

													if (acceleration.Entries[n].E > 4.0)
													{
														acceleration.Entries[n].E = 4.0;
													}
												}
											}
											else
											{
												acceleration.Entries[n].E = a;
											}

											break;
									}
								}

								m++;
							}

							i++;
							n++;
						}

						i--;
						break;
					case "#performance":
					case "#deceleration":
						i++;

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a >= 0.0)
										{
											performance.Deceleration = a;
										}

										break;
									case 1:
										if (a >= 0.0)
										{
											performance.CoefficientOfStaticFriction = a;
										}

										break;
									case 3:
										if (a >= 0.0)
										{
											performance.CoefficientOfRollingResistance = a;
										}

										break;
									case 4:
										if (a >= 0.0)
										{
											performance.AerodynamicDragCoefficient = a;
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

						double[] delayPowerUp = delay.DelayPower.Select(x => x.Up).ToArray();
						double[] delayPowerDown = delay.DelayPower.Select(x => x.Down).ToArray();
						double[] delayBrakeUp = delay.DelayBrake.Select(x => x.Up).ToArray();
						double[] delayBrakeDown = delay.DelayBrake.Select(x => x.Down).ToArray();
						double[] delayLocoBrakeUp = delay.DelayLocoBrake.Select(x => x.Up).ToArray();
						double[] delayLocoBrakeDown = delay.DelayLocoBrake.Select(x => x.Down).ToArray();

						delay.DelayPower.Clear();
						delay.DelayBrake.Clear();
						delay.DelayLocoBrake.Clear();

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a >= 0.0)
										{
											delayPowerUp = new[] { a };
										}

										break;
									case 1:
										if (a >= 0.0)
										{
											delayPowerDown = new[] { a };
										}

										break;
									case 2:
										if (a >= 0.0)
										{
											delayBrakeUp = new[] { a };
										}

										break;
									case 3:
										if (a >= 0.0)
										{
											delayBrakeDown = new[] { a };
										}

										break;
								}
							}
							else if (lines[i].IndexOf(',') != -1)
							{
								switch (n)
								{
									case 0:
										delayPowerUp = lines[i].Split(',').Select(x => double.Parse(x, culture)).ToArray();
										break;
									case 1:
										delayPowerDown = lines[i].Split(',').Select(x => double.Parse(x, culture)).ToArray();
										break;
									case 2:
										delayBrakeUp = lines[i].Split(',').Select(x => double.Parse(x, culture)).ToArray();
										break;
									case 3:
										delayBrakeDown = lines[i].Split(',').Select(x => double.Parse(x, culture)).ToArray();
										break;
									case 4:
										delayLocoBrakeUp = lines[i].Split(',').Select(x => double.Parse(x, culture)).ToArray();
										break;
									case 5:
										delayLocoBrakeDown = lines[i].Split(',').Select(x => double.Parse(x, culture)).ToArray();
										break;
								}
							}

							i++;
							n++;
						}

						for (int j = 0; j < Math.Max(delayPowerUp.Length, delayPowerDown.Length); j++)
						{
							Delay.Entry entry = new Delay.Entry();

							if (j < delayPowerUp.Length)
							{
								entry.Up = delayPowerUp[j];
							}

							if (j < delayPowerDown.Length)
							{
								entry.Down = delayPowerDown[j];
							}

							delay.DelayPower.Add(entry);
						}

						for (int j = 0; j < Math.Max(delayBrakeUp.Length, delayBrakeDown.Length); j++)
						{
							Delay.Entry entry = new Delay.Entry();

							if (j < delayBrakeUp.Length)
							{
								entry.Up = delayBrakeUp[j];
							}

							if (j < delayBrakeDown.Length)
							{
								entry.Down = delayBrakeDown[j];
							}

							delay.DelayBrake.Add(entry);
						}

						for (int j = 0; j < Math.Max(delayLocoBrakeUp.Length, delayLocoBrakeDown.Length); j++)
						{
							Delay.Entry entry = new Delay.Entry();

							if (j < delayLocoBrakeUp.Length)
							{
								entry.Up = delayLocoBrakeUp[j];
							}

							if (j < delayLocoBrakeDown.Length)
							{
								entry.Down = delayLocoBrakeDown[j];
							}

							delay.DelayLocoBrake.Add(entry);
						}

						i--;
						break;
					case "#move":
						i++;

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a >= 0.0)
										{
											move.JerkPowerUp = a;
										}

										break;
									case 1:
										if (a >= 0.0)
										{
											move.JerkPowerDown = a;
										}

										break;
									case 2:
										if (a >= 0.0)
										{
											move.JerkBrakeUp = a;
										}

										break;
									case 3:
										if (a >= 0.0)
										{
											move.JerkBrakeDown = a;
										}

										break;
									case 4:
										if (a >= 0.0)
										{
											move.BrakeCylinderUp = a;
										}

										break;
									case 5:
										if (a >= 0.0)
										{
											move.BrakeCylinderDown = a;
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

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (b >= 0 & b <= 2)
										{
											brake.BrakeType = (Brake.BrakeTypes)b;
										}

										break;
									case 1:
										if (b >= 0 & b <= 2)
										{
											brake.BrakeControlSystem = (Brake.BrakeControlSystems)b;
										}

										break;
									case 2:
										if (a >= 0.0)
										{
											brake.BrakeControlSpeed = a;
										}

										break;
									case 3:
										if (a <= 0 && a > 3)
										{
											brake.LocoBrakeType = (Brake.LocoBrakeTypes)b;
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

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								switch (n)
								{
									case 0:
										if (a > 0.0)
										{
											pressure.BrakeCylinderServiceMaximumPressure = a;
										}

										break;
									case 1:
										if (a > 0.0)
										{
											pressure.BrakeCylinderEmergencyMaximumPressure = a;
										}

										break;
									case 2:
										if (a > 0.0)
										{
											pressure.MainReservoirMinimumPressure = a;
										}

										break;
									case 3:
										if (a > 0.0)
										{
											pressure.MainReservoirMaximumPressure = a;
										}

										break;
									case 4:
										if (a > 0.0)
										{
											pressure.BrakePipeNormalPressure = a;
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

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (b == 0 | b == 1)
										{
											train.Handle.HandleType = (Handle.HandleTypes)b;
										}

										break;
									case 1:
										if (b > 0)
										{
											train.Handle.PowerNotches = b;
										}

										break;
									case 2:
										if (b > 0)
										{
											train.Handle.BrakeNotches = b;
										}

										break;
									case 3:
										if (b >= 0)
										{
											train.Handle.PowerNotchReduceSteps = b;
										}

										break;
									case 4:
										if (a >= 0 && a < 4)
										{
											train.Handle.HandleBehaviour = (Handle.EbHandleBehaviour)b;
										}

										break;
									case 5:
										if (b > 0)
										{
											train.Handle.LocoBrakeNotches = b;
										}

										break;
									case 6:
										if (a <= 0 && a > 3)
										{
											train.Handle.LocoBrake = (Handle.LocoBrakeType)b;
										}

										break;
									case 7:
										if (b > 0)
										{
											train.Handle.DriverPowerNotches = b;
										}

										break;
									case 8:
										if (b > 0)
										{
											train.Handle.DriverBrakeNotches = b;
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

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								switch (n)
								{
									case 0:
										train.Cab.PositionX = a;
										break;
									case 1:
										train.Cab.PositionY = a;
										break;
									case 2:
										train.Cab.PositionZ = a;
										break;
									case 3:
										train.Cab.DriverCar = (int)Math.Round(a);
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

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (a > 0.0)
										{
											motorCarMass = a;
										}

										break;
									case 1:
										if (b >= 1)
										{
											numberOfMotorCars = b;
										}

										break;
									case 2:
										if (a > 0.0)
										{
											trailerCarMass = a;
										}

										break;
									case 3:
										if (b >= 0)
										{
											numberOfTrailerCars = b;
										}

										break;
									case 4:
										if (b > 0.0)
										{
											lengthOfACar = a;
										}

										break;
									case 5:
										frontCarIsAMotorCar = a == 1.0;
										break;
									case 6:
										if (a > 0.0)
										{
											widthOfACar = a;
										}

										break;
									case 7:
										if (a > 0.0)
										{
											heightOfACar = a;
										}

										break;
									case 8:
										centerOfGravityHeight = a;
										break;
									case 9:
										if (a > 0.0)
										{
											exposedFrontalArea = a;
										}

										break;
									case 10:
										if (a > 0.0)
										{
											unexposedFrontalArea = a;
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

						while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							double a;

							if (double.TryParse(lines[i], NumberStyles.Float, culture, out a))
							{
								int b = (int)Math.Round(a);

								switch (n)
								{
									case 0:
										if (b >= -1 & b <= 1)
										{
											train.Device.Ats = (Device.AtsModes)b;
										}

										break;
									case 1:
										if (b >= 0 & b <= 2)
										{
											train.Device.Atc = (Device.AtcModes)b;
										}

										break;
									case 2:
										train.Device.Eb = a == 1.0;
										break;
									case 3:
										train.Device.ConstSpeed = a == 1.0;
										break;
									case 4:
										train.Device.HoldBrake = a == 1.0;
										break;
									case 5:
										if (b >= -1 & b <= 3)
										{
											train.Device.ReAdhesionDevice = (Device.ReAdhesionDevices)b;
										}

										break;
									case 6:
										train.Device.LoadCompensatingDevice = a;
										break;
									case 7:
										if (b >= 0 & b <= 2)
										{
											train.Device.PassAlarm = (Device.PassAlarmModes)b;
										}

										break;
									case 8:
										if (b >= 0 & b <= 2)
										{
											train.Device.DoorOpenMode = (Device.DoorModes)b;
										}

										break;
									case 9:
										if (b >= 0 & b <= 2)
										{
											train.Device.DoorCloseMode = (Device.DoorModes)b;
										}

										break;
									case 10:
										if (a >= 0.0)
										{
											train.Device.DoorWidth = a;
										}

										break;
									case 11:
										if (a >= 0.0)
										{
											train.Device.DoorMaxTolerance = a;
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
							string section = lines[i].ToLowerInvariant();
							i++;
							Motor.Entry[] entries = new Motor.Entry[800];

							while (i < lines.Length && !lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
							{
								if (n == entries.Length)
								{
									Array.Resize(ref entries, entries.Length << 1);
								}

								string u = lines[i] + ",";
								int k = 0;

								while (true)
								{
									int j = u.IndexOf(',');

									if (j == -1)
									{
										break;
									}

									string s = u.Substring(0, j).Trim();
									u = u.Substring(j + 1);
									double a;

									if (double.TryParse(s, NumberStyles.Float, culture, out a))
									{
										int b = (int)Math.Round(a);

										switch (k)
										{
											case 0:
												entries[n].SoundIndex = b >= 0 ? b : -1;
												break;
											case 1:
												entries[n].Pitch = Math.Max(a, 0.0);
												break;
											case 2:
												entries[n].Volume = Math.Max(a, 0.0);
												break;
										}
									}

									k++;
								}

								i++;
								n++;
							}

							Array.Resize(ref entries, n);
							i--;

							switch (section)
							{
								case "#motor_p1":
									motor.Tracks[(int)Motor.TrackInfo.Power1] = Motor.Track.EntriesToTrack(entries);
									break;
								case "#motor_p2":
									motor.Tracks[(int)Motor.TrackInfo.Power2] = Motor.Track.EntriesToTrack(entries);
									break;
								case "#motor_b1":
									motor.Tracks[(int)Motor.TrackInfo.Brake1] = Motor.Track.EntriesToTrack(entries);
									break;
								case "#motor_b2":
									motor.Tracks[(int)Motor.TrackInfo.Brake2] = Motor.Track.EntriesToTrack(entries);
									break;
							}
						}
						break;
				}
			}

			int numberOfCars = numberOfMotorCars + numberOfTrailerCars;
			bool[] isMotorCars = new bool[numberOfCars];
			if (numberOfMotorCars == 1)
			{
				if (frontCarIsAMotorCar | numberOfTrailerCars == 0)
				{
					isMotorCars[0] = true;
				}
				else
				{
					isMotorCars[numberOfCars - 1] = true;
				}
			}
			else if (numberOfMotorCars == 2)
			{
				if (frontCarIsAMotorCar | numberOfTrailerCars == 0)
				{
					isMotorCars[0] = true;
					isMotorCars[numberOfCars - 1] = true;
				}
				else if (numberOfTrailerCars == 1)
				{
					isMotorCars[1] = true;
					isMotorCars[2] = true;
				}
				else
				{
					int i = (int)Math.Ceiling(0.25 * (numberOfCars - 1));
					int j = (int)Math.Floor(0.75 * (numberOfCars - 1));
					isMotorCars[i] = true;
					isMotorCars[j] = true;
				}
			}
			else if (numberOfMotorCars > 0)
			{
				if (frontCarIsAMotorCar)
				{
					isMotorCars[0] = true;
					double t = 1.0 + numberOfTrailerCars / (double)(numberOfMotorCars - 1);
					double r = 0.0;
					double x = 0.0;

					while (true)
					{
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;

						if (i >= numberOfCars)
						{
							break;
						}

						isMotorCars[i] = true;
					}
				}
				else
				{
					isMotorCars[1] = true;
					double t = 1.0 + (numberOfTrailerCars - 1) / (double)(numberOfMotorCars - 1);
					double r = 0.0;
					double x = 1.0;

					while (true)
					{
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;

						if (i >= numberOfCars)
						{
							break;
						}

						isMotorCars[i] = true;
					}
				}
			}

			foreach (bool isMotorCar in isMotorCars)
			{
				Car car = isMotorCar ? (Car)new MotorCar() : new TrailerCar();

				car.Mass = isMotorCar ? motorCarMass : trailerCarMass;
				car.Length = lengthOfACar;
				car.Width = widthOfACar;
				car.Height = heightOfACar;
				car.CenterOfGravityHeight = centerOfGravityHeight;
				car.ExposedFrontalArea = exposedFrontalArea;
				car.UnexposedFrontalArea = unexposedFrontalArea;
				car.Performance = (Performance)performance.Clone();
				car.Delay = (Delay)delay.Clone();
				car.Move = (Move)move.Clone();
				car.Brake = (Brake)brake.Clone();
				car.Pressure = (Pressure)pressure.Clone();

				if (isMotorCar)
				{
					((MotorCar)car).Acceleration = (Acceleration)acceleration.Clone();
					((MotorCar)car).Motor = (Motor)motor.Clone();
				}

				train.Cars.Add(car);
			}

			for (int i = 0; i < numberOfCars - 1; i++)
			{
				train.Couplers.Add(new Coupler());
			}
		}
	}
}
