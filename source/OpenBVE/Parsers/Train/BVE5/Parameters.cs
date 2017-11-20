namespace OpenBve
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;
	using OpenBveApi.Math;

	partial class Bve5TrainParser
	{
		internal static void ParseTrainParameters(string FileName, ref TrainManager.Train Train)
		{
			if (string.IsNullOrEmpty(FileName))
			{
				throw new Exception("The BVE5 vehicle parameters file is missing.");
			}
			string fileFolder = System.IO.Path.GetDirectoryName(FileName);
			string fileFormat = File.ReadLines(FileName).First();
			string[] splitFormat = fileFormat.Split(':');
			if (!splitFormat[0].StartsWith("BveTs Vehicle Parameters", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Invalid BVE5 vehicle parameters header: " + splitFormat[0]);
			}
			int l = splitFormat[0].Length -1;
			while (l > 0)
			{
				if (!char.IsDigit(splitFormat[0][l]) && splitFormat[0][l] != '.')
				{
					string s = splitFormat[0].Substring(l + 1, splitFormat[0].Length - l -1);
					double Version;
					if (!double.TryParse(s, out Version))
					{
						throw new Exception("Invalid BVE5 vehicle parameters format version: " + s);
					}
					if (Version > 2.01)
					{
						throw new Exception("Unsupported BVE5 vehicle parameters format version: " + s);
					}
					break;
				}
				l--;

			}
			
			System.Text.Encoding e = Encoding.UTF8;
			if (splitFormat.Length >= 2)
			{
				/*
				 * Pull out the text encoding of our file
				 */
				e = TextEncoding.ParseEncoding(splitFormat[1]);
			}
			string[] Lines = File.ReadAllLines(FileName, e);

			string section = string.Empty;

			//DEFAULT PARAMETERS
			bool firstCarIsMotorCar = true;
			double MotorCarMass = 1.0, TrailerCarMass = 1.0;
			int MotorCars = 0, TrailerCars = 0, DriverCar = 0;
			double CarLength = 20.0;
			double CarWidth = 2.6;
			double CarHeight = 3.6;
			Vector3 Driver = new Vector3(-1.0, 2.5,-1.0);
			double MainReservoirMinimumPressure = 700000.0;
			double MainReservoirMaximumPressure = 800000.0;
			double StraightAirPipeApplySpeed = 200000.0;
			double StraightAirPipeReleaseSpeed = 300000.0;
			double BrakePipeApplySpeed = 100000.0;
			double BrakePipeReleaseSpeed = 1500000.0;
			double BrakePipePressure = 0.0;
			double CompressionSpeed = 5000.0;
			double BrakeCylinderUp = 300000.0;
			double BrakeCylinderDown = 200000.0;
			double BrakeCylinderServiceMaximumPressure = 440000.0;
			double BrakeControlSpeed = 5.0;

			
			double[] NotchPressureRatios = new double[0];
			TrainManager.CarBrakeType BrakeType = TrainManager.CarBrakeType.ElectromagneticStraightAirBrake;

			for (int i = 1; i < Lines.Length; i++)
			{
				string line = Lines[i];
				
				int hash = line.IndexOf('#');
				if (hash >= 0)
				{
					line = line.Substring(0, hash).Trim();
				}
				else
				{
					line = line.Trim();
				}
				if (line.Length > 0 && line[0] == '[' & line[line.Length - 1] == ']')
				{
					section = line.Substring(1, line.Length - 2).ToLowerInvariant();
				}
				else
				{
					int equals = line.IndexOf('=');
					if (equals >= 0)
					{
						string key = line.Substring(0, equals).Trim().ToLowerInvariant();
						string value = line.Substring(equals + 1).Trim().ToLowerInvariant();
						switch (section)
						{
							case "":
								switch (key)
								{
									case "firstcar":
										switch (value)
										{
											case "m":
												firstCarIsMotorCar = true;
												break;
											case "t":
												firstCarIsMotorCar = false;
												break;
											default:
												Interface.AddMessage(Interface.MessageType.Warning, false, "First Car Type has an invalid value: " + value);
												break;
										}
										break;
									case "loadcompensating":
										//Michelle appears never to have implemented the load compensating device, so ignore it for the moment
										break;
								}
								break;
							case "compressor":
								switch (key)
								{
									case "upperpressure":
										if (!Double.TryParse(value, out MainReservoirMaximumPressure))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Main reservoir maximum pressure has an invalid value: " + value);
										}
										break;
									case "lowerpressure":
										if (!Double.TryParse(value, out MainReservoirMinimumPressure))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Main reservoir minimum pressure has an invalid value: " + value);
										}
										break;
									case "compressionspeed":
										if (!Double.TryParse(value, out CompressionSpeed))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Compression speed has an invalid value: " + value);
										}
										break;
								}
								break;
							case "bc":
								switch (key)
								{
									case "applystart":
									case "applystop":
									case "releasestart":
									case "releasestop":
									case "pistonarea":
										//Not supported just at the minute
										break;
									case "applyspeed":
										if (!Double.TryParse(value, out BrakeCylinderUp))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Brake cylinder application flow rate has an invalid value: " + value);
											break;
										}
										BrakeCylinderUp *= 1000.0;
										break;
									case "releasespeed":
										if (!Double.TryParse(value, out BrakeCylinderDown))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Brake cylinder release flow rate has an invalid value: " + value);
											break;
										}
										BrakeCylinderDown *= 1000.0;
										break;
								}
								break;
							case "sap":
								//NOT EDITABLE IN BVE2 / BVE4 trains
								switch (key)
								{
									case "applyspeed":
										if (!Double.TryParse(value, out StraightAirPipeApplySpeed))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Straight air pipe application flow rate has an invalid value: " + value);
											break;
										}
										StraightAirPipeApplySpeed *= 1000.0;
										break;
									case "releasespeed":
										if (!Double.TryParse(value, out StraightAirPipeReleaseSpeed))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Straight air pipe release flow rate has an invalid value: " + value);
											break;
										}
										StraightAirPipeReleaseSpeed *= 1000.0;
										break;
								}
								break;
							case "bp":
								//NOT EDITABLE IN BVE2 / BVE4 trains
								switch (key)
								{
									case "applyspeed":
										if (!Double.TryParse(value, out BrakePipeApplySpeed))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Brake pipe application flow rate has an invalid value: " + value);
											break;
										}
										BrakePipeApplySpeed *= 1000.0;
										break;
									case "releasespeed":
										if (!Double.TryParse(value, out BrakePipeReleaseSpeed))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Brake pipe release flow rate has an invalid value: " + value);
											break;
										}
										BrakePipeReleaseSpeed *= 1000.0;
										break;
								}
								break;
							case "lockoutvalve":
								//Probably ignore these for the minute....
								break;
							case "ecb":
							case "smee":
							case "cl":
								switch (section)
								{
									//Double switch on the same case, but all the other parameters are identical
									case "ecb":
										BrakeType = TrainManager.CarBrakeType.ElectricCommandBrake;
										break;
									case "smee":
										BrakeType = TrainManager.CarBrakeType.ElectromagneticStraightAirBrake;
										break;
									case "cl":
										BrakeType = TrainManager.CarBrakeType.AutomaticAirBrake;
										break;
								}
								switch (key)
								{
									case "maximumpressure":
										if (!Double.TryParse(value, out BrakeCylinderServiceMaximumPressure))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Brake cylinder maximum service pressure has an invalid value: " + value);
										}
										break;
									case "pressurerates":
										if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "The PressureRates variable is not supported for a brake type of " + BrakeType);
											break;
										}
										/*
										 * MACKOY:
										 * Pressure command for each brake notch when MaximumPressure is set to 1.
										 * It can be set for electric command type brake and electromagnetic direct air brake.
										 * 
										 * -----------------------------------------------------------------------------------
										 * 
										 * Appears to be the ratio of air in the BC for each notch (From the figure given by maximum pressure), starts with REL
										 * and has EB as the final entry
										 */
										string[] splitRates = value.Split(',');
										NotchPressureRatios = new double[splitRates.Length];
										for (int j = 0; j < splitRates.Length; j++)
										{
											splitRates[j] = splitRates[j].Trim();
											if(!double.TryParse(splitRates[j], out NotchPressureRatios[j]))
											{
												Interface.AddMessage(Interface.MessageType.Warning, false, "The pressure ratio for Notch " + j + " is invalid.");
												if (j == 0)
												{
													NotchPressureRatios[j] = 0;
												}
												else
												{
													NotchPressureRatios[j] = NotchPressureRatios[j - 1];
												}
											}
										}
										break;
									case "sapbcratio":
										break;
									case "sapbcoffset":
										break;
									case "bpinitialpressure":
										if (BrakeType == TrainManager.CarBrakeType.ElectricCommandBrake)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "The BpInitialPressure variable is not supported for a brake type of " + BrakeType);
											break;
										}
										break;
									case "leverdelay":
										//Delay in seconds after the brake lever has been actuated
										break;
								}
								break;
							case "maincircuit":
								switch (key)
								{
									case "regenerationlimit":
										//Speed in km/h at which regenerative braking stops
										if (!Double.TryParse(value, out BrakeControlSpeed))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Regeneration limit has an invalid value: " + value);
											break;
										}
										break;
									case "regenerationstartlimit":
										//Speed at which regnerative braking starts
										break;
									case "leverdelay":
										//Delay in seconds after the power lever has been actuated
										break;
									case "brakepriority":
										//If set to TRUE, a brake application will block a power command
										break;
								}
								break;
							case "dynamics":
								double c;
								switch (key)
								{
									case "motorcarweight":
										if (!Double.TryParse(value, out MotorCarMass))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Motor car mass has an invalid value: " + value);
											break;
										}
										if (MotorCarMass <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Motor car mass is expected to be positive.");
											MotorCarMass = 1.0;
										}
										break;
									case "motorcarcount":
										if (!Double.TryParse(value, out c))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Number of motor cars has an invalid value: " + value);
											break;
										}
										if (c <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Number of motor cars is expected to be positive.");
											c = 1.0;
										}
										MotorCars = (int)Math.Round(c);
										break;
									case "trailercarweight":
										if (!Double.TryParse(value, out TrailerCarMass))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Trailer car mass has an invalid value: " + value);
											break;
										}
										if (TrailerCarMass <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Trailer car mass is expected to be positive.");
											TrailerCarMass = 1.0;
										}
										break;
									case "trailercarcount":
										if (!Double.TryParse(value, out c))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Number of motor cars has an invalid value: " + value);
											break;
										}
										if (c <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Number of motor cars is expected to be positive.");
											c = 1.0;
										}
										TrailerCars = (int)Math.Round(c);
										break;
									case "carlength":
										//NOTE: Width and height of a car appear to have been deprecated for BVE5
										//Check in the convertor what sort of assumtions are made (if any?)
										if (!Double.TryParse(value, out CarLength))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Car length has an invalid value: " + value);
											break;
										}
										if (CarLength <= 0.0)
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Car length is expected to be positive.");
											CarLength = 1.0;
										}
										break;
								}
								break;
							case "cab":
								switch (key)
								{
									case "brakenotchcount":
										if (!int.TryParse(value, out Train.Specs.MaximumBrakeNotch))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Brake notch count has an invalid value: " + value);
										}
										break;
									case "powernotchcount":
										if (!int.TryParse(value, out Train.Specs.MaximumPowerNotch))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Power notch count has an invalid value: " + value);
										}
										break;
									case "motorbrakenotch":
										break;
									case "atscancelnotch":
										break;
								}
								break;
							case "viewpoint":
								switch (key)
								{
									case "x":
										if (!double.TryParse(value, out Driver.X))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Driver viewpoint X has an invalid value: " + value);
										}
										break;
									case "y":
										if (!double.TryParse(value, out Driver.Y))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Driver viewpoint Y has an invalid value: " + value);
										}
										break;
									case "z":
										if (!double.TryParse(value, out Driver.Z))
										{
											Interface.AddMessage(Interface.MessageType.Warning, false, "Driver viewpoint Z has an invalid value: " + value);
										}
										break;
								}
								break;
						} 
					}
				}
			}

			int Cars = MotorCars + TrailerCars;
			Train.Cars = new TrainManager.Car[Cars];
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i] = new TrainManager.Car(Train, i);
				Train.Cars[i].FrontBogie = new TrainManager.Bogie(Train, Train.Cars[i]);
				Train.Cars[i].RearBogie = new TrainManager.Bogie(Train, Train.Cars[i]);
			}
			DriverCar = 0;
			Train.Cars[0].Specs.IsDriverCar = true;

			double OperatingPressure;
			double BrakeCylinderEmergencyMaximumPressure;
			//TODO: Not sure of this at the minute, but the EB max pressure is set by the last entry in PressureRatios
			//No idea what will happen if the EB pressure is less than service pressure, will need to test that more throughly
			if (NotchPressureRatios.Length == 0 || NotchPressureRatios.Length == 1)
			{
				BrakeCylinderEmergencyMaximumPressure = BrakeCylinderServiceMaximumPressure;
			}
			else
			{
				BrakeCylinderEmergencyMaximumPressure = BrakeCylinderServiceMaximumPressure * NotchPressureRatios[NotchPressureRatios.Length - 1];
			}
			if (BrakePipePressure <= 0.0)
			{
				if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
				{
					OperatingPressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					if (OperatingPressure > MainReservoirMinimumPressure)
					{
						OperatingPressure = MainReservoirMinimumPressure;
					}
				}
				else
				{
					if (BrakeCylinderEmergencyMaximumPressure < 480000.0 & MainReservoirMinimumPressure > 500000.0)
					{
						OperatingPressure = 490000.0;
					}
					else
					{
						OperatingPressure = BrakeCylinderEmergencyMaximumPressure + 0.75 * (MainReservoirMinimumPressure - BrakeCylinderEmergencyMaximumPressure);
					}
				}
			}
			else
			{
				OperatingPressure = BrakePipePressure;
			}
			// apply brake-specific attributes for all cars
			for (int i = 0; i < Cars; i++)
			{
				Train.Cars[i].Specs.BrakeType = BrakeType;
				Train.Cars[i].Specs.ElectropneumaticType = TrainManager.EletropneumaticBrakeType.None;
				Train.Cars[i].Specs.BrakeControlSpeed = BrakeControlSpeed;
				/*
				 * Train.Cars[i].Specs.BrakeDecelerationAtServiceMaximumPressure = BrakeDeceleration;
				 * Train.Cars[i].Specs.MotorDeceleration = MotorDeceleration;
				 *
				 * These are set in the performance data files, not the base train parameters
				 */
				Train.Cars[i].Specs.AirBrake.AirCompressorEnabled = false;
				Train.Cars[i].Specs.AirBrake.AirCompressorMinimumPressure = MainReservoirMinimumPressure;
				Train.Cars[i].Specs.AirBrake.AirCompressorMaximumPressure = MainReservoirMaximumPressure;
				Train.Cars[i].Specs.AirBrake.AirCompressorRate = CompressionSpeed;
				Train.Cars[i].Specs.AirBrake.MainReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AirCompressorMinimumPressure + (Train.Cars[i].Specs.AirBrake.AirCompressorMaximumPressure - Train.Cars[i].Specs.AirBrake.AirCompressorMinimumPressure) * Program.RandomNumberGenerator.NextDouble();
				Train.Cars[i].Specs.AirBrake.MainReservoirEqualizingReservoirCoefficient = 0.01;
				Train.Cars[i].Specs.AirBrake.MainReservoirBrakePipeCoefficient = (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake ? 0.25 : 0.075) / Cars;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure = 0.0;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirNormalPressure = 1.005 * OperatingPressure;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirServiceRate = 50000.0;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirEmergencyRate = 250000.0;
				Train.Cars[i].Specs.AirBrake.EqualizingReservoirChargeRate = 200000.0;
				Train.Cars[i].Specs.AirBrake.BrakePipeNormalPressure = OperatingPressure;
				Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = BrakeType == TrainManager.CarBrakeType.ElectricCommandBrake ? 0.0 : Train.Cars[i].Specs.AirBrake.BrakePipeNormalPressure;
				Train.Cars[i].Specs.AirBrake.BrakePipeChargeRate = BrakePipeReleaseSpeed;
				Train.Cars[i].Specs.AirBrake.BrakePipeServiceRate = BrakePipeApplySpeed;
				Train.Cars[i].Specs.AirBrake.BrakePipeEmergencyRate = BrakePipeApplySpeed;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure = 0.975 * OperatingPressure;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirChargeRate = 200000.0;
				Train.Cars[i].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient = 0.5;
				{
					double r = Train.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure / BrakeCylinderEmergencyMaximumPressure - 1.0;
					if (r < 0.1) r = 0.1;
					if (r > 1.0) r = 1.0;
					Train.Cars[i].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient = r;
				}
				Train.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure = BrakeCylinderEmergencyMaximumPressure;
				Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceMaximumPressure = BrakeCylinderServiceMaximumPressure;
				Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure = BrakeCylinderEmergencyMaximumPressure;
				if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
				{
					Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceChargeRate = BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyChargeRate = BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderReleaseRate = BrakeCylinderDown;
				}
				else
				{
					Train.Cars[i].Specs.AirBrake.BrakeCylinderServiceChargeRate = 0.3 * BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyChargeRate = BrakeCylinderUp;
					Train.Cars[i].Specs.AirBrake.BrakeCylinderReleaseRate = BrakeCylinderDown;
				}
				Train.Cars[i].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = BrakeCylinderEmergencyMaximumPressure;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure = 0.0;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeReleaseRate = StraightAirPipeReleaseSpeed;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeServiceRate = StraightAirPipeApplySpeed;
				Train.Cars[i].Specs.AirBrake.StraightAirPipeEmergencyRate = StraightAirPipeApplySpeed;
			}
			if (Train.Specs.HasHoldBrake & Train.Specs.MaximumBrakeNotch > 1)
			{
				Train.Specs.MaximumBrakeNotch--;
			}
			// apply train attributes
			Train.Specs.CurrentReverser.Driver = 0;
			Train.Specs.CurrentReverser.Actual = 0;
			Train.Specs.CurrentPowerNotch.Driver = 0;
			Train.Specs.CurrentPowerNotch.Safety = 0;
			Train.Specs.CurrentPowerNotch.Actual = 0;
			Train.Specs.DelayPowerUp = new double[] { 1.0 };
			Train.Specs.DelayPowerDown = new double[] { 1.0 };
			Train.Specs.CurrentPowerNotch.DelayedChanges = new TrainManager.HandleChange[] { };
			Train.Specs.CurrentBrakeNotch.Driver = 0;
			Train.Specs.CurrentBrakeNotch.Safety = 0;
			Train.Specs.CurrentBrakeNotch.Actual = 0;
			Train.Specs.DelayBrakeUp = new double[] { 1.0 };
			Train.Specs.DelayBrakeDown = new double[] { 1.0 };
			Train.Specs.CurrentBrakeNotch.DelayedChanges = new TrainManager.HandleChange[] { };
			Train.Specs.CurrentEmergencyBrake.ApplicationTime = double.MaxValue;
			if (BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
			{
				Train.Specs.SingleHandle = false;
				Train.Specs.HasHoldBrake = false;
			}
			// apply other attributes for all cars
			double AxleDistance = 0.4 * CarLength;
			for (int i = 0; i < Cars; i++)
			{
				Train.Cars[i].CurrentCarSection = -1;
				Train.Cars[i].ChangeCarSection(TrainManager.CarSectionType.NotVisible);
				Train.Cars[i].FrontBogie.ChangeSection(-1);
				Train.Cars[i].RearBogie.ChangeSection(-1);
				Train.Cars[i].FrontAxle.Follower.TriggerType = i == 0 ? TrackManager.EventTriggerType.FrontCarFrontAxle : TrackManager.EventTriggerType.OtherCarFrontAxle;
				Train.Cars[i].RearAxle.Follower.TriggerType = i == Cars - 1 ? TrackManager.EventTriggerType.RearCarRearAxle : TrackManager.EventTriggerType.OtherCarRearAxle;
				Train.Cars[i].BeaconReceiver.TriggerType = i == 0 ? TrackManager.EventTriggerType.TrainFront : TrackManager.EventTriggerType.None;
				Train.Cars[i].BeaconReceiverPosition = 0.5 * CarLength;
				Train.Cars[i].FrontAxle.Follower.CarIndex = i;
				Train.Cars[i].RearAxle.Follower.CarIndex = i;
				Train.Cars[i].FrontAxle.Position = AxleDistance;
				Train.Cars[i].RearAxle.Position = -AxleDistance;
				Train.Cars[i].Specs.IsMotorCar = false;
				Train.Cars[i].Specs.CoefficientOfStaticFriction = 0.35;
				Train.Cars[i].Specs.CoefficientOfRollingResistance = 0.0025;
				Train.Cars[i].Specs.AerodynamicDragCoefficient = 1.1;
				Train.Cars[i].Specs.JerkPowerUp = new double[] { 1.0 };
				Train.Cars[i].Specs.JerkPowerDown = new double[] { 1.0 };
				Train.Cars[i].Specs.JerkBrakeUp = new double[] { 1.0 };
				Train.Cars[i].Specs.JerkBrakeDown = new double[] { 1.0 };
				/*
				 * 	
				 * 	
				 *	Train.Cars[i].Specs.ExposedFrontalArea = CarExposedFrontalArea;		
				 *	Train.Cars[i].Specs.UnexposedFrontalArea = CarUnexposedFrontalArea;
				 *	
				 *	Set in the notch parameters
				 */
				Train.Cars[i].Doors = new TrainManager.Door[2];
				Train.Cars[i].Doors[0].Direction = -1;
				Train.Cars[i].Doors[0].State = 0.0;
				Train.Cars[i].Doors[1].Direction = 1;
				Train.Cars[i].Doors[1].State = 0.0;
				Train.Cars[i].Specs.DoorOpenFrequency = 0.0;
				Train.Cars[i].Specs.DoorCloseFrequency = 0.0;
				//Train.Cars[i].Specs.CenterOfGravityHeight = CenterOfGravityHeight;
				//TODO: Issue here is that we appear to no longer be able to specify width/ height
				//Need to investigate more as to where this has been moved to, or any assumptions made?

				Train.Cars[i].Width = CarWidth;
				Train.Cars[i].Height = CarHeight;
				Train.Cars[i].Length = CarLength;
				Train.Cars[i].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * Train.Cars[i].Specs.CenterOfGravityHeight / Train.Cars[i].Width);
			}
			// assign motor cars
			if (MotorCars == 1)
			{
				if (firstCarIsMotorCar | TrailerCars == 0)
				{
					Train.Cars[0].Specs.IsMotorCar = true;
				}
				else
				{
					Train.Cars[Cars - 1].Specs.IsMotorCar = true;
				}
			}
			else if (MotorCars == 2)
			{
				if (firstCarIsMotorCar | TrailerCars == 0)
				{
					Train.Cars[0].Specs.IsMotorCar = true;
					Train.Cars[Cars - 1].Specs.IsMotorCar = true;
				}
				else if (TrailerCars == 1)
				{
					Train.Cars[1].Specs.IsMotorCar = true;
					Train.Cars[2].Specs.IsMotorCar = true;
				}
				else
				{
					int i = (int)Math.Ceiling(0.25 * (double)(Cars - 1));
					int j = (int)Math.Floor(0.75 * (double)(Cars - 1));
					Train.Cars[i].Specs.IsMotorCar = true;
					Train.Cars[j].Specs.IsMotorCar = true;
				}
			}
			else if (MotorCars > 0)
			{
				if (firstCarIsMotorCar)
				{
					Train.Cars[0].Specs.IsMotorCar = true;
					double t = 1.0 + (double)TrailerCars / (double)(MotorCars - 1);
					double r = 0.0;
					double x = 0.0;
					while (true)
					{
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= Cars) break;
						Train.Cars[i].Specs.IsMotorCar = true;
					}
				}
				else
				{
					Train.Cars[1].Specs.IsMotorCar = true;
					double t = 1.0 + (double)(TrailerCars - 1) / (double)(MotorCars - 1);
					double r = 0.0;
					double x = 1.0;
					while (true)
					{
						double y = x + t - r;
						x = Math.Ceiling(y);
						r = x - y;
						int i = (int)x;
						if (i >= Cars) break;
						Train.Cars[i].Specs.IsMotorCar = true;
					}
				}
			}
			// assign motor/trailer-specific settings
			for (int i = 0; i < Cars; i++)
			{
				Train.Cars[i].Specs.ReAdhesionDevice = new TrainManager.CarReAdhesionDevice(Train.Cars[i]);
				Train.Cars[i].Specs.ConstSpeed = new TrainManager.CarConstSpeed(Train.Cars[i]);
				Train.Cars[i].Specs.HoldBrake = new TrainManager.CarHoldBrake(Train.Cars[i]);
				Train.Cars[i].Specs.AccelerationCurves = new TrainManager.AccelerationCurve[] { };
				Train.Cars[i].Specs.DecelerationCurves = new TrainManager.AccelerationCurve[] { };
				if (Train.Cars[i].Specs.IsMotorCar)
				{
					// motor car
					Train.Cars[i].Specs.AirBrake.Type = TrainManager.AirBrakeType.Main;
					Train.Cars[i].Specs.MassEmpty = MotorCarMass;
					Train.Cars[i].Specs.MassCurrent = MotorCarMass;
					
					Train.Cars[i].Specs.AccelerationCurveMaximum = 0.0;
					Train.Cars[i].Sounds.Motor.Tables = new TrainManager.MotorSoundTable[0];

					Train.Cars[i].Specs.ReAdhesionDevice.UpdateInterval = 1.0;
					Train.Cars[i].Specs.ReAdhesionDevice.ApplicationFactor = 1.0;
					Train.Cars[i].Specs.ReAdhesionDevice.ReleaseInterval = 1.0;
					Train.Cars[i].Specs.ReAdhesionDevice.ReleaseFactor = 99.0;
				}
				else
				{
					// trailer car
					Train.Cars[i].Specs.AirBrake.Type = Train == TrainManager.PlayerTrain & i == Train.DriverCar | BrakeType == TrainManager.CarBrakeType.ElectricCommandBrake ? TrainManager.AirBrakeType.Main : TrainManager.AirBrakeType.Auxillary;
					Train.Cars[i].Specs.MassEmpty = TrailerCarMass;
					Train.Cars[i].Specs.MassCurrent = TrailerCarMass;
					Train.Cars[i].Specs.AccelerationCurveMaximum = 0.0;
				}
			}
			Train.Couplers = new TrainManager.Coupler[Cars - 1];
			for (int i = 0; i < Train.Couplers.Length; i++)
			{
				Train.Couplers[i].MinimumDistanceBetweenCars = 0.9 * 0.3;
				Train.Couplers[i].MaximumDistanceBetweenCars = 1.1 * 0.3;
			}

			Train.Cars[Train.DriverCar].Driver.X = Driver.X;
			Train.Cars[Train.DriverCar].Driver.Y = Driver.Y;
			Train.Cars[Train.DriverCar].Driver.Z = 0.5 * CarLength + Driver.Z;
		}
	}
}
