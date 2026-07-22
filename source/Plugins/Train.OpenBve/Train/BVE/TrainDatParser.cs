using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Formats.OpenBve;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class TrainDatParser
	{
		internal readonly Plugin Plugin;

		internal TrainDatParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>
		/// Read the file of the specified train.dat
		/// </summary>
		/// <param name="fileName">The file path of the specified train.dat</param>
		/// <param name="encoding">The text encoding to use</param>
		/// <returns>The array read from the specified train.dat</returns>
		private static string[] ReadFile(string fileName, Encoding encoding)
		{
			//Create the array using the default compatibility train.dat
			string[] lines = { "BVE2000000", "#CAR", "1", "1", "1", "0", "1", "1" };

			// load file
			try
			{
				lines = System.IO.File.ReadAllLines(fileName, encoding);
			}
			catch
			{
				//ignore and load default
			}
			if (lines.Length == 1 && encoding.Equals(Encoding.Unicode))
			{
				/*
				 * Probably not unicode after all
				 * Stuff edited with BVE2 / BVE4 tools should either be ASCII or SHIFT_JIS
				 * both of which should read OK with ASCII for our purposes
				 */
				encoding = Encoding.ASCII;
				lines = System.IO.File.ReadAllLines(fileName, encoding);
			}
			else if (lines.Length == 0)
			{
				//Catch zero-length train.dat files
				throw new Exception("The train.dat file " + fileName + " is of zero length.");
			}

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
				if (lines[i].EndsWith(","))
				{
					//File edited with MSExcel may have additional commas at the end of a line
					lines[i] = lines[i].TrimEnd(',');
				}
			}

			return lines;
		}

		/// <summary>
		/// Checks whether the parser can load the specified file.
		/// </summary>
		/// <param name="fileName">The file path of the specified train.dat</param>
		/// <returns>Whether the parser can load the specified file.</returns>
		internal bool CanLoad(string fileName)
		{
			Encoding encoding = TextEncoding.GetSystemEncodingFromFile(fileName);
			
			string[] lines;
			try
			{
				lines = ReadFile(fileName, encoding);
			}
			catch
			{
				return false;
			}

			TrainDatFormats format = TrainDatFile<TrainDatSection, TrainDatKey>.ParseFormat(lines[0], out _);
			if (format == TrainDatFormats.MissingHeader)
			{
				// Some NYCTA stuff seems to be missing the version header from their train.dat files
				// absolute mess....
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The train.dat file " + fileName + " has a missing version header.");
			}
			return format != TrainDatFormats.Unsupported;
		}

		/// <summary>Parses a BVE2 / BVE4 / openBVE train.dat file</summary>
		/// <param name="FileName">The train.dat file to parse</param>
		/// <param name="Encoding">The text encoding to use</param>
		/// <param name="Train">The train</param>
		internal void Parse(string FileName, Encoding Encoding, TrainBase Train) {
			// initialize
			double BrakeCylinderServiceMaximumPressure = 440000.0;
			double BrakeCylinderEmergencyMaximumPressure = 440000.0;
			double MainReservoirMinimumPressure = 690000.0;
			double MainReservoirMaximumPressure = 780000.0;
			double BrakePipePressure = 0.0;
			BrakeSystemType trainBrakeType = BrakeSystemType.ElectromagneticStraightAirBrake;
			BrakeSystemType locomotiveBrakeType = BrakeSystemType.ElectromagneticStraightAirBrake;
			EletropneumaticBrakeType ElectropneumaticType = EletropneumaticBrakeType.None;
			double BrakeControlSpeed = 0.0;
			double BrakeDeceleration = 0.277777777777778;
			double JerkPowerUp = 10.0;
			double JerkPowerDown = 10.0;
			double JerkBrakeUp = 10.0;
			double JerkBrakeDown = 10.0;
			double BrakeCylinderUp = 300000.0;
			double BrakeCylinderDown = 200000.0;
			double CoefficientOfStaticFriction = 0.35;
			double CoefficientOfRollingResistance = 0.0025;
			double AerodynamicDragCoefficient = 1.1;
			BveAccelerationCurve[] AccelerationCurves = { };
			Vector3 Driver = new Vector3();
			int DriverCar = 0;
			double MotorCarMass = 1.0, TrailerCarMass = 1.0;
			int MotorCars = 0, TrailerCars = 0;
			double CarLength = 20.0;
			double CarWidth = 2.6;
			double CarHeight = 3.6;
			double CenterOfGravityHeight = 1.6;
			double CarExposedFrontalArea = 0.6 * CarWidth * CarHeight;
			double CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;
			bool FrontCarIsMotorCar = true;
			double DoorWidth = 1000.0;
			double DoorTolerance = 0.0;
			ReadhesionDeviceType ReAdhesionDevice = ReadhesionDeviceType.TypeA;
			PassAlarmType passAlarm = PassAlarmType.None;
			Train.Handles.HasLocoBrake = false;
			Train.Specs.AveragesPressureDistribution = true;
			double[] powerDelayUp = { }, powerDelayDown = { }, brakeDelayUp = { }, brakeDelayDown = { }, locoBrakeDelayUp = { }, locoBrakeDelayDown = { };
			double electricBrakeDelayUp = 0, electricBrakeDelayDown = 0;
			
			int powerNotches = 0, brakeNotches = 0, locoBrakeNotches = 0, powerReduceSteps = -1, locoBrakeType = 0, driverPowerNotches = 0, driverBrakeNotches = 0;
			BVEMotorSoundTable[] Tables = new BVEMotorSoundTable[4];
			for (int i = 0; i < 4; i++) {
				Tables[i].Entries = new BVEMotorSoundTableEntry[16];
				for (int j = 0; j < 16; j++) {
					Tables[i].Entries[j].SoundIndex = -1;
					Tables[i].Entries[j].Pitch = 1.0f;
					Tables[i].Entries[j].Gain = 1.0f;
				}
			}
			// parse configuration
			TrainDatFile<TrainDatSection, TrainDatKey> datFile = new TrainDatFile<TrainDatSection, TrainDatKey>(FileName, Plugin.CurrentHost);
			double invfac = datFile.RemainingSubBlocks == 0 ? 0.1 : 0.1 / datFile.RemainingSubBlocks;
			int totalBlocks = datFile.RemainingSubBlocks;
			while (datFile.RemainingSubBlocks > 0)
			{
				Block<TrainDatSection, TrainDatKey> subBlock = datFile.ReadNextBlock();
				switch (subBlock.Key)
				{
					case TrainDatSection.Acceleration:
						while (subBlock.RemainingDataValues > 0)
						{
							if (subBlock.GetNextDoubleArray(',', out double[] curveValues) && curveValues.Length >= 4)
							{
								Array.Resize(ref AccelerationCurves, AccelerationCurves.Length + 1);
								BveAccelerationCurve curve = new BveAccelerationCurve();
								if (curveValues[0] > 0)
								{
									curve.StageZeroAcceleration = curveValues[0] * 0.277777777777778;
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "a0 in section #ACCELERATION is expected to be greater than zero for Curve " + AccelerationCurves.Length + " in file " + FileName);
								}
								if (curveValues[1] > 0)
								{
									curve.StageOneAcceleration = curveValues[1] * 0.277777777777778;
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "a1 in section #ACCELERATION is expected to be greater than zero for Curve " + AccelerationCurves.Length + " in file " + FileName);
								}
								if (curveValues[2] > 0)
								{
									curve.StageOneSpeed = curveValues[2] * 0.277777777777778;
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "v1 in section #ACCELERATION is expected to be greater than zero for Curve " + AccelerationCurves.Length + " in file " + FileName);
								}
								if (curveValues[3] > 0)
								{
									curve.StageTwoSpeed = curveValues[3] * 0.277777777777778;
									if (curve.StageTwoSpeed < curve.StageOneSpeed)
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "v2 in section #ACCELERATION is expected to be greater than or equal to v1 for Curve " + AccelerationCurves.Length + " in file " + FileName);
										curve.StageTwoSpeed = curve.StageOneSpeed;
									}
								}
								else
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "v2 in section #ACCELERATION is expected to be greater than zero for Curve " + AccelerationCurves.Length + " in file " + FileName);
								}
								if (curveValues[4] > 0)
								{
									if (datFile.Format > TrainDatFormats.BVE2000000)
									{
										if (curveValues[4] <= 0.0)
										{
											curve.StageTwoExponent = 1.0;
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "e in section #ACCELERATION is expected to be positive for Curve " + AccelerationCurves.Length + " in file " + FileName);
										}
										else
										{
											const double c = 4.439346232277577;
											curve.StageTwoExponent = 1.0 - Math.Log(curveValues[4]) * curve.StageTwoSpeed * c;
											if (curve.StageTwoExponent <= 0.0)
											{
												curve.StageTwoExponent = 1.0;
											}
											else if (curve.StageTwoExponent > 4.0)
											{
												curve.StageTwoExponent = 4.0;
											}
										}
									}
									else
									{
										curve.StageTwoExponent = curveValues[4];
										if (curve.StageTwoExponent <= 0.0)
										{
											curve.StageTwoExponent = 1.0;
										}
									}
								}
								AccelerationCurves[AccelerationCurves.Length - 1] = curve;
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid acceleration curve entry for Curve " + AccelerationCurves.Length + " in file " + FileName);
							}
						}
						break;
					case TrainDatSection.Performance:
						if (subBlock.GetNextDouble(TrainDatKey.BrakeDeceleration, NumberRange.Positive, out double brakeDeceleration))
						{
							BrakeDeceleration = brakeDeceleration * 0.277777777777778;
						}
						if (subBlock.GetNextDouble(TrainDatKey.CoefficientOfStaticFriction, NumberRange.Positive, out double staticFriction))
						{
							if (Plugin.CurrentOptions.EnableBveTsHacks && (staticFriction > 0.1 || staticFriction < 1.0))
							{
								break;
							}
							CoefficientOfStaticFriction = staticFriction;
						}
						if (subBlock.GetNextDouble(TrainDatKey.CoefficientOfRollingResistance, NumberRange.Positive, out double rollingResistance))
						{
							CoefficientOfRollingResistance = rollingResistance;
						}

						if (subBlock.GetNextDouble(TrainDatKey.CoefficientOfAerodynamicDrag, NumberRange.Positive, out double aerodynamicDrag))
						{
							AerodynamicDragCoefficient = aerodynamicDrag;
						}
						break;
					case TrainDatSection.Delay:
						if (subBlock.GetNextDoubleArray(',', out powerDelayUp))
						{
							if (datFile.Format != TrainDatFormats.openBVE || datFile.Version < 1534)
							{
								Array.Resize(ref powerDelayUp, 1);
								if (Plugin.CurrentOptions.EnableBveTsHacks && powerDelayUp[0] > 60)
								{
									powerDelayUp[0] = 0;
								}
							}
						}
						if (subBlock.GetNextDoubleArray(',', out powerDelayDown))
						{
							if (datFile.Format != TrainDatFormats.openBVE || datFile.Version < 1534)
							{
								Array.Resize(ref powerDelayDown, 1);
								if (Plugin.CurrentOptions.EnableBveTsHacks && powerDelayDown[0] > 60)
								{
									powerDelayDown[0] = 0;
								}
							}
						}
						if (subBlock.GetNextDoubleArray(',', out brakeDelayUp))
						{
							if (datFile.Format != TrainDatFormats.openBVE || datFile.Version < 1534)
							{
								Array.Resize(ref brakeDelayUp, 1);
								if (Plugin.CurrentOptions.EnableBveTsHacks && brakeDelayUp[0] > 60)
								{
									brakeDelayUp[0] = 0;
								}
							}
						}
						if (subBlock.GetNextDoubleArray(',', out brakeDelayDown))
						{
							if (datFile.Format != TrainDatFormats.openBVE || datFile.Version < 1534)
							{
								Array.Resize(ref brakeDelayDown, 1);
								if (Plugin.CurrentOptions.EnableBveTsHacks && brakeDelayDown[0] > 60)
								{
									brakeDelayDown[0] = 0;
								}
							}
						}
						/*
						 * https://github.com/leezer3/OpenBVE/issues/737
						 * OpenBVE appears to have overlooked these originally
						 * We can (hopefully) assume that any trains using the LocoBrake feature
						 * will have set the OPENBVE header, hence it's reasonable to assume that
						 * others will actually be genuine users
						 *
						 * Don't split per-notch, as this wll just cause more confusion
						 */
						if (datFile.Format != TrainDatFormats.openBVE || datFile.Version >= 1830)
						{
							subBlock.GetNextDouble(TrainDatKey.ElectricBrakeDelayUp, NumberRange.Positive, out electricBrakeDelayUp);
							subBlock.GetNextDouble(TrainDatKey.ElectricBrakeDelayDown, NumberRange.Positive, out electricBrakeDelayDown);
						}
						subBlock.GetNextDoubleArray(',', out locoBrakeDelayUp);
						subBlock.GetNextDoubleArray(',', out locoBrakeDelayDown);
						break;
					case TrainDatSection.Move:
						if (subBlock.GetNextDouble(TrainDatKey.JerkPowerUp, NumberRange.NonZero, out double jerkUp))
						{
							JerkPowerUp = 0.01 * jerkUp;
						}
						if (subBlock.GetNextDouble(TrainDatKey.JerkPowerUp, NumberRange.NonZero, out double jerkDown))
						{
							JerkPowerDown = 0.01 * jerkDown;
						}
						if (subBlock.GetNextDouble(TrainDatKey.JerkBrakeUp, NumberRange.NonZero, out jerkUp))
						{
							JerkBrakeUp = 0.01 * jerkUp;
						}
						if (subBlock.GetNextDouble(TrainDatKey.JerkBrakeUp, NumberRange.NonZero, out jerkDown))
						{
							JerkBrakeDown = 0.01 * jerkDown;
						}
						if (subBlock.GetNextDouble(TrainDatKey.BrakeCylinderUp, NumberRange.Positive, out double bcUp))
						{
							BrakeCylinderUp = 1000.0 * bcUp;
						}
						if (subBlock.GetNextDouble(TrainDatKey.BrakeCylinderDown, NumberRange.Positive, out double bcDown))
						{
							BrakeCylinderDown = 1000.0 * bcDown;
						}
						break;
					case TrainDatSection.Brake:
						subBlock.GetNextEnumValue(TrainDatKey.BrakeSystemType, out trainBrakeType);
						subBlock.GetNextEnumValue(TrainDatKey.ElectropneumaticType, out ElectropneumaticType);
						if (subBlock.GetNextDouble(TrainDatKey.BrakeControlSpeed, NumberRange.Positive, out double controlSpeed))
						{
							if (controlSpeed > 0 && trainBrakeType == BrakeSystemType.AutomaticAirBrake)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BrakeControlSpeed will be ignored due to the current brake setup in " + FileName);
							}
							else
							{
								BrakeControlSpeed = controlSpeed * 0.277777777777778; //Convert to m/s
							}
						}
						subBlock.GetNextInt(TrainDatKey.LocoBrakeType, NumberRange.NonNegative, out locoBrakeType);
						switch (locoBrakeType)
						{
							case 0:
								//Not fitted
								break;
							case 1:
								//Notched air brake
								Train.Handles.HasLocoBrake = true;
								locomotiveBrakeType = BrakeSystemType.ElectromagneticStraightAirBrake;
								break;
							case 2:
								//Automatic air brake
								Train.Handles.HasLocoBrake = true;
								locomotiveBrakeType = BrakeSystemType.AutomaticAirBrake;
								break;
						}
						break;
					case TrainDatSection.Pressure:
						if (subBlock.GetNextDouble(TrainDatKey.BrakeCylinderServiceMaximumPressure, NumberRange.Positive, out double pressure))
						{
							BrakeCylinderServiceMaximumPressure = pressure * 1000.0;
						}
						if (subBlock.GetNextDouble(TrainDatKey.BrakeCylinderEmergencyMaximumPressure, NumberRange.Positive, out pressure))
						{
							BrakeCylinderEmergencyMaximumPressure = pressure * 1000.0;
						}
						if (subBlock.GetNextDouble(TrainDatKey.MainReservoirMinimumPressure, NumberRange.Positive, out pressure))
						{
							MainReservoirMinimumPressure = pressure * 1000.0;
						}
						if (subBlock.GetNextDouble(TrainDatKey.MainReservoirMaximumPressure, NumberRange.Positive, out pressure))
						{
							MainReservoirMaximumPressure = pressure * 1000.0;
						}
						if (subBlock.GetNextDouble(TrainDatKey.BrakePipePressure, NumberRange.Positive, out pressure))
						{
							BrakePipePressure = pressure * 1000.0;
						}
						break;
					case TrainDatSection.Handle:
						subBlock.GetNextEnumValue(TrainDatKey.HandleType, out Train.Handles.HandleType);
						if (subBlock.GetNextInt(TrainDatKey.NumberOfPowerNotches, NumberRange.Positive, out int power))
						{
							powerNotches = power;
						}
						if (subBlock.GetNextInt(TrainDatKey.NumberOfBrakeNotches, NumberRange.NonNegative, out int brake))
						{
							if (brake == 0)
							{
								brake = 8;
								if (trainBrakeType != BrakeSystemType.AutomaticAirBrake)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumberOfBrakeNotches is expected to be positive and non-zero in " + FileName);
								}
							}

							brakeNotches = brake;
						}
						if (subBlock.GetNextInt(TrainDatKey.PowerReduceSteps, NumberRange.NonNegative, out int steps))
						{
							powerReduceSteps = steps;
						}
						subBlock.GetNextEnumValue(TrainDatKey.EbHandleBehaviour, out Train.Handles.EmergencyBrake.OtherHandlesBehaviour);
						if (subBlock.GetNextInt(TrainDatKey.LocoBrakeNotches, NumberRange.NonNegative, out int locoBrake))
						{
							locoBrakeNotches = locoBrake;
						}
						subBlock.GetNextEnumValue(TrainDatKey.LocoBrakeType, out Train.Handles.LocoBrakeType);
						if (datFile.Format == TrainDatFormats.openBVE && datFile.Version >= 15311)
						{
							if (subBlock.GetNextInt(TrainDatKey.DriverPowerNotches, NumberRange.Positive, out power))
							{
								driverPowerNotches = power;
							}
							if (subBlock.GetNextInt(TrainDatKey.DriverBrakeNotches, NumberRange.Positive, out brake))
							{
								driverBrakeNotches = brake;
							}
						}
						break;
					case TrainDatSection.Cab:
						subBlock.GetNextDouble(TrainDatKey.DriverX, NumberRange.Any, out Driver.X);
						subBlock.GetNextDouble(TrainDatKey.DriverY, NumberRange.Any, out Driver.Y);
						subBlock.GetNextDouble(TrainDatKey.DriverZ, NumberRange.Any, out Driver.Z);
						Driver *= 0.001;
						break;
					case TrainDatSection.Car:
						if (subBlock.GetNextDouble(TrainDatKey.MotorCarMass, NumberRange.Positive, out double mass))
						{
							MotorCarMass = mass * 1000.0;
						}
						if (subBlock.GetNextInt(TrainDatKey.NumberOfMotorCars, NumberRange.Positive, out int cars))
						{
							MotorCars = cars;
						}
						if (subBlock.GetNextDouble(TrainDatKey.TrailerCarMass, NumberRange.Positive, out mass))
						{
							TrailerCarMass = mass * 1000.0;
						}
						if (subBlock.GetNextInt(TrainDatKey.NumberOfTrailerCars, NumberRange.NonNegative, out cars))
						{
							TrailerCars = cars;
						}
						if (subBlock.GetNextDouble(TrainDatKey.CarLength, NumberRange.Positive, out double length))
						{
							CarLength = length;
						}
						if (subBlock.GetNextBool(TrainDatKey.FrontCarIsMotorCar, out bool frontMotor))
						{
							FrontCarIsMotorCar = frontMotor;
						}
						if (subBlock.GetNextDouble(TrainDatKey.WidthOfCar, NumberRange.Positive, out double width))
						{
							CarWidth = width;
						}
						if (subBlock.GetNextDouble(TrainDatKey.HeightOfCar, NumberRange.Positive, out double height))
						{
							CarHeight = height;
						}
						CarExposedFrontalArea = 0.65 * CarWidth * CarHeight;
						CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;

						if (subBlock.GetNextDouble(TrainDatKey.ExposedFrontalArea, NumberRange.Positive, out double area))
						{
							CarExposedFrontalArea = area;
							CarUnexposedFrontalArea = 0.2 * CarWidth * CarHeight;
						}
						if (subBlock.GetNextDouble(TrainDatKey.UnexposedFrontalArea, NumberRange.Positive, out area))
						{
							CarUnexposedFrontalArea = area;
						}
						break;
					case TrainDatSection.Device:
						subBlock.GetNextInt(TrainDatKey.DefaultSafetySystems, NumberRange.NonNegative, out int atsType);
						switch (atsType)
						{
							case 0:
								Train.Specs.DefaultSafetySystems |= DefaultSafetySystems.AtsSn;
								break;
							case 1:
								Train.Specs.DefaultSafetySystems |= DefaultSafetySystems.AtsSn;
								Train.Specs.DefaultSafetySystems |= DefaultSafetySystems.AtsP;
								break;
						}

						subBlock.GetNextInt(TrainDatKey.DefaultSafetySystems, NumberRange.NonNegative, out int atcType);
						switch (atcType)
						{
							case 1:
							case 2:
								Train.Specs.DefaultSafetySystems |= DefaultSafetySystems.Atc;
								break;
						}

						subBlock.GetNextBool(TrainDatKey.DefaultSafetySystems, out bool hasEb);
						if (hasEb)
						{
							Train.Specs.DefaultSafetySystems |= DefaultSafetySystems.Eb;
						}

						subBlock.GetNextBool(TrainDatKey.DefaultSafetySystems, out Train.Specs.HasConstSpeed);
						subBlock.GetNextBool(TrainDatKey.DefaultSafetySystems, out Train.Handles.HasHoldBrake);
						subBlock.GetNextEnumValue(TrainDatKey.PassAlarm, out passAlarm);
						subBlock.GetNextEnumValue(TrainDatKey.DoorOpenMode, out Train.Specs.DoorOpenMode);
						subBlock.GetNextEnumValue(TrainDatKey.DoorCloseMode, out Train.Specs.DoorCloseMode);
						if (subBlock.GetNextDouble(TrainDatKey.DoorWidth, NumberRange.Positive, out width))
						{
							DoorWidth = width;
						}
						if (subBlock.GetNextDouble(TrainDatKey.DoorMaxTolerance, NumberRange.Positive, out double tolerance))
						{
							DoorTolerance = tolerance;
						}
						break;
					case TrainDatSection.Motor_P1:
					case TrainDatSection.Motor_P2:
					case TrainDatSection.Motor_B1:
					case TrainDatSection.Motor_B2:
						int msi = 0;
						switch (subBlock.Key)
						{
							case TrainDatSection.Motor_P1: msi = BVEMotorSound.MotorP1; break;
							case TrainDatSection.Motor_P2: msi = BVEMotorSound.MotorP2; break;
							case TrainDatSection.Motor_B1: msi = BVEMotorSound.MotorB1; break;
							case TrainDatSection.Motor_B2: msi = BVEMotorSound.MotorB2; break;
						}
						Array.Resize(ref Tables[msi].Entries, subBlock.RemainingDataValues);
						for (int i = 0; i < Tables[msi].Entries.Length; i++)
						{
							subBlock.GetNextDoubleArray(',', out double[] entry);
							Tables[msi].Entries[i].SoundIndex = (int)Math.Round(entry[0]);
							
							if (entry[1] < 0.0) entry[1] = 0.0;
							Tables[msi].Entries[i].Pitch = (float)(0.01 * entry[1]);
							if (entry[2] < 0.0) entry[2] = 0.0;
							Tables[msi].Entries[i].Gain = (float)Math.Pow((0.0078125 * entry[2]), 0.25);
						}
						break;
				}
				Plugin.CurrentProgress = Plugin.LastProgress + invfac * (totalBlocks - datFile.RemainingSubBlocks);
			}


			if (TrailerCars > 0 && TrailerCarMass == 0.0) {
				if (datFile.Format < TrainDatFormats.openBVE && Plugin.CurrentOptions.EnableBveTsHacks && TrailerCars == 1 && TrailerCarMass == 0)
				{
					/*
					 * Early BVE train editor versions appear to have been unable to create a train with no trailer cars,
					 * hence the use of a single zero-mass variety as a workaround e.g. EvA6
					 */
					TrailerCars = 0;
				}
				else
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "TrailerCarMass is expected to be non-zero in " + FileName);
					TrailerCarMass = 1.0;	
				}
				
			}
			
			if (powerNotches == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumberOfPowerNotches was not set in " + FileName);
				powerNotches = 8;
			}
			if (brakeNotches == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumberOfBrakeNotches was not set in " + FileName);
				brakeNotches = 8;
			}
			if (driverPowerNotches == 0)
			{
				if (datFile.Format == TrainDatFormats.openBVE && datFile.Version >= 15311)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumberOfDriverPowerNotches was not set in " + FileName);
				}
				driverPowerNotches = powerNotches;
			}
			if (driverBrakeNotches == 0)
			{
				if (datFile.Format == TrainDatFormats.openBVE && datFile.Version >= 15311)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumberOfDriverBrakeNotches was not set in " + FileName);
				}
				driverBrakeNotches = brakeNotches;
			}
			Train.Handles.Power = new PowerHandle(powerNotches, driverPowerNotches, powerDelayUp, powerDelayDown, Train);
			if (powerReduceSteps != -1)
			{
				PowerHandle powerHandle = Train.Handles.Power as PowerHandle;
				powerHandle.ReduceSteps = powerReduceSteps;
			}

			if (trainBrakeType == BrakeSystemType.AutomaticAirBrake)
			{
				Train.Handles.Brake = new AirBrakeHandle(Train);
			}
			else
			{
				Train.Handles.Brake = new BrakeHandle(brakeNotches, driverBrakeNotches, Train.Handles.EmergencyBrake, brakeDelayUp, brakeDelayDown, Train);
				
			}

			if (locomotiveBrakeType == BrakeSystemType.AutomaticAirBrake)
			{
				Train.Handles.LocoBrake = new LocoAirBrakeHandle(Train);
			}
			else
			{
				Train.Handles.LocoBrake = new LocoBrakeHandle(locoBrakeNotches, Train.Handles.EmergencyBrake, locoBrakeDelayUp, locoBrakeDelayDown, Train);
			}
			Train.Handles.LocoBrakeType = (LocoBrakeType)locoBrakeType;
			Train.Handles.HoldBrake = new HoldBrakeHandle(Train);
			// apply data
			MotorCars = Math.Max(MotorCars, 1);
			int Cars = MotorCars + TrailerCars;
			Train.Cars = new CarBase[Cars];
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i] = new CarBase(Train, i, CoefficientOfStaticFriction, CoefficientOfRollingResistance, AerodynamicDragCoefficient);
			}
			double DistanceBetweenTheCars = 0.3;
			
			if (DriverCar >= Cars) {
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DriverCar must point to an existing car in " + FileName);
				DriverCar = 0;

			}
			Train.DriverCar = DriverCar;
			// brake system
			double OperatingPressure;
			if (BrakePipePressure <= 0.0) {
				if (trainBrakeType == BrakeSystemType.AutomaticAirBrake) {
					OperatingPressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					if (OperatingPressure > MainReservoirMinimumPressure) {
						OperatingPressure = MainReservoirMinimumPressure;
					}
				} else {
					if (BrakeCylinderEmergencyMaximumPressure < 480000.0 & MainReservoirMinimumPressure > 500000.0) {
						OperatingPressure = 490000.0;
					} else {
						OperatingPressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					}
				}
			} else {
				OperatingPressure = BrakePipePressure;
			}
			// acceleration curves
			double MaximumAcceleration = 0.0;
			if (AccelerationCurves.Length != Train.Handles.Power.MaximumNotch && !FileName.ToLowerInvariant().EndsWith("compatibility\\pretrain\\train.dat"))
			{
				//NOTE: The compatibility train.dat is only used to load some properties, hence this warning does not apply
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The #ACCELERATION section defines " + AccelerationCurves.Length + " curves, but the #HANDLE section defines " + Train.Handles.Power.MaximumNotch + " power notches in " + FileName);
			}
			
			for (int i = 0; i < Math.Min(AccelerationCurves.Length, Train.Handles.Power.MaximumNotch); i++) {
				bool errors = false;
				if (AccelerationCurves[i].StageZeroAcceleration <= 0.0) {
					AccelerationCurves[i].StageZeroAcceleration = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageOneAcceleration <= 0.0) {
					AccelerationCurves[i].StageOneAcceleration = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageOneSpeed <= 0.0) {
					AccelerationCurves[i].StageOneSpeed = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageTwoSpeed <= 0.0) {
					AccelerationCurves[i].StageTwoSpeed = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageTwoExponent <= 0.0) {
					AccelerationCurves[i].StageTwoExponent = 1.0;
					errors = true;
				}
				if (AccelerationCurves[i].StageOneSpeed > AccelerationCurves[i].StageTwoSpeed) {
					double x = 0.5 * (AccelerationCurves[i].StageOneSpeed + AccelerationCurves[i].StageTwoSpeed);
					AccelerationCurves[i].StageOneSpeed = x;
					AccelerationCurves[i].StageTwoSpeed = x;
					errors = true;
				}
				if (errors) {
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Entry " + (i + 1) + " in the #ACCELERATION section is missing or invalid in " + FileName);
				}
				if (AccelerationCurves[i].MaximumAcceleration > MaximumAcceleration) {
					MaximumAcceleration = AccelerationCurves[i].MaximumAcceleration;
				}
			}

			bool[] motorCars = new bool[Train.Cars.Length];

			// assign motor cars
			if (MotorCars == 1) {
				if (FrontCarIsMotorCar | TrailerCars == 0) {
					motorCars[0] = true;
				} else {
					motorCars[Cars - 1] = true;
				}
			} else if (MotorCars == 2) {
				if (FrontCarIsMotorCar | TrailerCars == 0) {
					motorCars[0] = true;
					motorCars[Cars - 1] = true;
				} else if (TrailerCars == 1) {
					motorCars[1] = true;
					motorCars[2] = true;
				} else {
					int i = (int)Math.Ceiling(0.25 * (Cars - 1));
					int j = (int)Math.Floor(0.75 * (Cars - 1));
					motorCars[i] = true;
					motorCars[j] = true;
				}
			} else if (MotorCars > 0) {
				if (FrontCarIsMotorCar) {
					motorCars[0] = true;
					double t = 1.0 + TrailerCars / (double)(MotorCars - 1);
					double r = 0.0;
					double x = 0.0;
					while (true) {
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= Cars) break;
						motorCars[i] = true;
					}
				} else {
					motorCars[1] = true;
					double t = 1.0 + (TrailerCars - 1) / (double)(MotorCars - 1);
					double r = 0.0;
					double x = 1.0;
					while (true) {
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= Cars) break;
						motorCars[i] = true;
					}
				}
			}
			double MotorDeceleration = Math.Sqrt(MaximumAcceleration * BrakeDeceleration);
			// apply brake-specific attributes for all cars
			for (int i = 0; i < Cars; i++) {
				AccelerationCurve[] DecelerationCurves = 
				{
					new BveDecelerationCurve(BrakeDeceleration), 
				};
				if (i == Train.DriverCar && Train.Handles.HasLocoBrake)
				{
					switch (locomotiveBrakeType)
					{
						case BrakeSystemType.AutomaticAirBrake:
							Train.Cars[i].CarBrake = new AutomaticAirBrake(ElectropneumaticType, Train.Cars[i], BrakeControlSpeed, MotorDeceleration, DecelerationCurves);
							break;
						case BrakeSystemType.ElectricCommandBrake:
							Train.Cars[i].CarBrake = new ElectricCommandBrake(ElectropneumaticType, Train.Cars[i], BrakeControlSpeed, MotorDeceleration, electricBrakeDelayUp, electricBrakeDelayDown, DecelerationCurves);
							break;
						case BrakeSystemType.ElectromagneticStraightAirBrake:
							Train.Cars[i].CarBrake = new ElectromagneticStraightAirBrake(ElectropneumaticType, Train.Cars[i], BrakeControlSpeed, MotorDeceleration, electricBrakeDelayUp, electricBrakeDelayDown, DecelerationCurves);
							break;
					}
				}
				else
				{
					switch (trainBrakeType)
					{
						case BrakeSystemType.AutomaticAirBrake:
							Train.Cars[i].CarBrake = new AutomaticAirBrake(ElectropneumaticType, Train.Cars[i], BrakeControlSpeed, MotorDeceleration, DecelerationCurves);
							break;
						case BrakeSystemType.ElectricCommandBrake:
							Train.Cars[i].CarBrake = new ElectricCommandBrake(ElectropneumaticType, Train.Cars[i], BrakeControlSpeed, MotorDeceleration, electricBrakeDelayUp, electricBrakeDelayDown, DecelerationCurves);
							break;
						case BrakeSystemType.ElectromagneticStraightAirBrake:
							Train.Cars[i].CarBrake = new ElectromagneticStraightAirBrake(ElectropneumaticType, Train.Cars[i], BrakeControlSpeed, MotorDeceleration, electricBrakeDelayUp, electricBrakeDelayDown, DecelerationCurves);
							break;
					}
				}

				if (motorCars[i])
				{
					Train.Cars[i].TractionModel = new BVEMotorCar(Train.Cars[i], AccelerationCurves);
				}
				else
				{
					Train.Cars[i].TractionModel = new BVETrailerCar(Train.Cars[i]);
				}

				Train.Cars[i].CarBrake.MainReservoir = new MainReservoir(MainReservoirMinimumPressure, MainReservoirMaximumPressure, 0.01, (trainBrakeType == BrakeSystemType.AutomaticAirBrake ? 0.25 : 0.075) / Cars);
				Train.Cars[i].CarBrake.MainReservoir.Volume = 0.5; // Organization for Co-Operation between Railways specifies 340L to 680L main reservoir capacity for EMU, so let's pick something in the middle (in m³)

				Train.Cars[i].CarBrake.EqualizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0, 1.005 * OperatingPressure);
				Train.Cars[i].CarBrake.EqualizingReservoir.Volume = 0.015; // very small reservoir for observation, so guess at 15L

				Train.Cars[i].CarBrake.BrakePipe = new BrakePipe(OperatingPressure, 10000000.0, 1500000.0, 5000000.0, trainBrakeType == BrakeSystemType.ElectricCommandBrake);
				Train.Cars[i].CarBrake.BrakePipe.Volume = Math.Pow(0.0175 * Math.PI, 2) * (Train.Cars.Length * 1.05); // Assuming Railway Group Standards 3.5cm diameter brake pipe, 5% extra length for bends etc.

				AirBrake airBrake = Train.Cars[i].CarBrake as AirBrake;
				if (Train.Cars[i].TractionModel.ProvidesPower || Train.IsPlayerTrain && i == Train.DriverCar || trainBrakeType == BrakeSystemType.ElectricCommandBrake)
				{
					Train.Cars[i].CarBrake.BrakeType = BrakeType.Main;
					airBrake.Compressor = new Compressor(5000.0, Train.Cars[i].CarBrake.MainReservoir, Train.Cars[i]);
				}
				else
				{
					Train.Cars[i].CarBrake.BrakeType = BrakeType.Auxiliary;
				}
				airBrake.StraightAirPipe = new StraightAirPipe(300000.0, 400000.0, 200000.0);
				{
					double r = 200000.0 / BrakeCylinderEmergencyMaximumPressure - 1.0;
					if (r < 0.1) r = 0.1;
					if (r > 1.0) r = 1.0;
					Train.Cars[i].CarBrake.AuxiliaryReservoir = new AuxiliaryReservoir(0.975 * OperatingPressure, 200000.0, 0.5, r);
					Train.Cars[i].CarBrake.AuxiliaryReservoir.Volume = 0.16; // guessed 1/3 of main reservoir volume
				}
				Train.Cars[i].CarBrake.BrakeCylinder = new BrakeCylinder(BrakeCylinderServiceMaximumPressure, BrakeCylinderEmergencyMaximumPressure, trainBrakeType == BrakeSystemType.AutomaticAirBrake ? BrakeCylinderUp : 0.3 * BrakeCylinderUp, BrakeCylinderUp, BrakeCylinderDown);
				Train.Cars[i].CarBrake.BrakeCylinder.Volume = 0.14; // 35cm diameter, 15cm stroke
				Train.Cars[i].CarBrake.JerkUp = JerkBrakeUp;
				Train.Cars[i].CarBrake.JerkDown = JerkBrakeDown;
				Train.Cars[i].CarBrake.Initialize(Plugin.CurrentOptions.TrainStart);
			}
			if (Train.Handles.HasHoldBrake & Train.Handles.Brake.MaximumNotch > 1) {
				Train.Handles.Brake.MaximumNotch--;
			}
			// apply train attributes
			if (trainBrakeType == BrakeSystemType.AutomaticAirBrake) {
				Train.Handles.HandleType = HandleType.TwinHandle;
				Train.Handles.HasHoldBrake = false;
			}
			Train.Cars[Train.DriverCar].SafetySystems.Add(SafetySystem.OverspeedMessage, new OverspeedMessage(Train.Cars[Train.DriverCar]));
			Train.SafetySystems.PassAlarm = new PassAlarm(passAlarm, Train.Cars[DriverCar]);
			Train.SafetySystems.PilotLamp = new PilotLamp(Train.Cars[DriverCar]);
			Train.SafetySystems.StationAdjust = new StationAdjustAlarm(Train);
			Train.SafetySystems.Headlights = new LightSource(Train, 1);
			Train.Handles.Setup(Plugin.CurrentOptions.TrainStart);
			// apply other attributes for all cars
			double AxleDistance = 0.4 * CarLength;
			for (int i = 0; i < Cars; i++) {
				if (Train.Cars.Length > 1)
				{
					Train.Cars[i].Coupler = new Coupler(0.9 * DistanceBetweenTheCars, 1.1 * DistanceBetweenTheCars, Train.Cars[i], i < Cars - 1 ? Train.Cars[i + 1] : null);
				}
				else
				{
					Train.Cars[i].Coupler = new Coupler(0.9 * DistanceBetweenTheCars, 1.1 * DistanceBetweenTheCars, Train.Cars[i], null);
				}
				if (i == DriverCar)
				{
					Train.Cars[i].Breaker = new Breaker(Train.Cars[i]);
				}
				Train.Cars[i].ChangeCarSection(CarSectionType.NotVisible);
				Train.Cars[i].FrontAxle.Follower.TriggerType = i == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
				Train.Cars[i].RearAxle.Follower.TriggerType = i == Cars - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
				Train.Cars[i].BeaconReceiver.TriggerType = i == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
				Train.Cars[i].BeaconReceiverPosition = 0.5 * CarLength;
				Train.Cars[i].FrontAxle.Position = AxleDistance;
				Train.Cars[i].RearAxle.Position = -AxleDistance;
				Train.Cars[i].Specs.JerkPowerUp = JerkPowerUp;
				Train.Cars[i].Specs.JerkPowerDown = JerkPowerDown;
				Train.Cars[i].Specs.ExposedFrontalArea = CarExposedFrontalArea;
				Train.Cars[i].Specs.UnexposedFrontalArea = CarUnexposedFrontalArea;
				Train.Cars[i].Doors[0] = new Door(-1, DoorWidth, DoorTolerance);
				Train.Cars[i].Doors[1] = new Door(1, DoorWidth, DoorTolerance);
				Train.Cars[i].Specs.DoorOpenFrequency = 0.0;
				Train.Cars[i].Specs.DoorCloseFrequency = 0.0;
				Train.Cars[i].Specs.CenterOfGravityHeight = CenterOfGravityHeight;
				Train.Cars[i].Width = CarWidth;
				Train.Cars[i].Height = CarHeight;
				Train.Cars[i].Length = CarLength;
				Train.Cars[i].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * Train.Cars[i].Specs.CenterOfGravityHeight / Train.Cars[i].Width);
			}

			Train.Cars[Train.Cars.Length - 1].BeaconReceiver.TriggerType = Cars == 1 ? EventTriggerType.SingleCarTrain : EventTriggerType.TrainRear;
			

			Plugin.MotorSoundTables = Tables;
			Plugin.AccelerationCurves = AccelerationCurves;
			Plugin.MaximumAcceleration = MaximumAcceleration;

			// assign motor/trailer-specific settings
			for (int i = 0; i < Cars; i++) {
				Train.Cars[i].ConstSpeed = new CarConstSpeed(Train.Cars[i]);
				Train.Cars[i].HoldBrake = new CarHoldBrake(Train.Cars[i]);
				Train.Cars[i].ReAdhesionDevice = new BveReAdhesionDevice(Train.Cars[i], ReAdhesionDevice);
				if (Train.Cars[i].TractionModel.ProvidesPower) {
					// motor car
					Train.Cars[i].EmptyMass = MotorCarMass;
					Train.Cars[i].CargoMass = 0;
					Array.Resize(ref Train.Cars[i].TractionModel.AccelerationCurves, AccelerationCurves.Length);
					for (int j = 0; j < AccelerationCurves.Length; j++)
					{
						Train.Cars[i].TractionModel.AccelerationCurves[j] = AccelerationCurves[j].Clone(1.0 + TrailerCars * TrailerCarMass / (MotorCars * MotorCarMass));
					}
					Train.Cars[i].TractionModel.MaximumPossibleAcceleration = MaximumAcceleration;
				} else {
					// trailer car
					Train.Cars[i].EmptyMass = TrailerCarMass;
					Train.Cars[i].CargoMass = 0;
				}
			}
			// driver
			
			Train.Cars[Train.DriverCar].Driver.X = Driver.X;
			Train.Cars[Train.DriverCar].Driver.Y = Driver.Y;
			Train.Cars[Train.DriverCar].Driver.Z = 0.5 * CarLength + Driver.Z;
			if (Train.IsPlayerTrain)
			{
				Train.Cars[DriverCar].HasInteriorView = true;
			}

			
			/*
			 * Determine the maximum width of the handle strings for drawing purposes
			 * (As translators may use much longer strings than we expect)
			 */
			if (Plugin.Renderer?.Fonts?.NormalFont != null && Translations.QuickReferences != null)
			{
				Vector2 rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandleForward);
				if (rs.X > Train.Handles.Reverser.MaxWidth)
				{
					Train.Handles.Reverser.MaxWidth = rs.X;
				}
				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandleNeutral);
				if (rs.X > Train.Handles.Reverser.MaxWidth)
				{
					Train.Handles.Reverser.MaxWidth = rs.X;
				}
				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandleBackward);
				if (rs.X > Train.Handles.Reverser.MaxWidth)
				{
					Train.Handles.Reverser.MaxWidth = rs.X;
				}

				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandlePower);
				if (rs.X > Train.Handles.Power.MaxWidth)
				{
					Train.Handles.Power.MaxWidth = rs.X;
				}
				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandlePowerNull);
				if (rs.X > Train.Handles.Power.MaxWidth)
				{
					Train.Handles.Power.MaxWidth = rs.X;
				}

				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandleBrake);
				if (rs.X > Train.Handles.Brake.MaxWidth)
				{
					Train.Handles.Brake.MaxWidth = rs.X;
				}
				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandleBrakeNull);
				if (rs.X > Train.Handles.Brake.MaxWidth)
				{
					Train.Handles.Brake.MaxWidth = rs.X;
				}
				rs = Plugin.Renderer.Fonts.NormalFont.MeasureString(Translations.QuickReferences.HandleEmergency);
				if (rs.X > Train.Handles.Brake.MaxWidth)
				{
					Train.Handles.Brake.MaxWidth = rs.X;
				}
			}
		}

	}
}
