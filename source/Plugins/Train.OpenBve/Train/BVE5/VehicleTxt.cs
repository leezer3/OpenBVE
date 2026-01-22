//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Formats.OpenBve;
using LibRender2.Trains;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using System;
using System.IO;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal partial class VehicleTxtParser
	{
		internal Plugin Plugin;

		internal VehicleTxtParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Checks whether the parser can load the specified file.</summary>
		/// <param name="fileName">The file path of the specified vehicle.txt</param>
		/// <returns>Whether the parser can load the specified file.</returns>
		internal bool CanLoad(string fileName)
		{
			try
			{
				using (StreamReader reader = new StreamReader(fileName))
				{
					var firstLine = reader.ReadLine() ?? "";
					string b = string.Empty;
					if (!firstLine.ToLowerInvariant().Trim().StartsWith("bvets vehicle"))
					{
						return false;
					}
					for (int i = 15; i < firstLine.Length; i++)
					{
						if (char.IsDigit(firstLine[i]) || firstLine[i] == '.')
						{
							b += firstLine[i];
						}
						else
						{
							break;
						}
					}
					if (b.Length > 0)
					{
						NumberFormats.TryParseDoubleVb6(b, out double version);
						if (version > 2.0)
						{
							throw new Exception(version + " is not a supported BVE5 vehicle version");
						}
					}
					else
					{
						return false;
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		private int MotorCars = 5;
		private int TrailerCars = 5;
		private bool FirstCarIsMotorCar = false;
		private bool OneLeverCab = false;
		private int PowerNotches = 8;
		private int BrakeNotches = 5;
		private int HoldBrakeNotch = 0;
		private int AtsCancelNotch = 1;
		private int MotorBrakeNotch = 1;
		private string[] PowerNotchDescriptions = { };
		private string[] BrakeNotchDescriptions = { };
		private string[] ReverserNotchDescriptions = { };
		private string[] HoldNotchDescriptions = { };
		private bool constantSpeedPower = false;
		private bool constantSpeedNeutral = false;
		private bool constantSpeedBrake = false;
		private bool LoadCompensatingDevice = true;
		private double RegenerationLimit = 5;
		private double RegenerationStartLimit;
		private double PowerLeverDelay = 0.5;
		private bool BrakePriority = false;
		private double SlipVelocityCoefficient = 0;
		private double SlipVelocity = 2;
		private double SlipAcceleration = 10;
		private double BalanceAcceleration = -2;
		private double HoldingTime = 2;
		private double ReferenceAcceleration = 4;
		private double ReferenceDeceleration = 4;
		private double CurrentDecrease = 0;
		private double CurrentIncrease = 0;
		private ParametersTxtSection BrakeType;
		private double MaximumPressure = 440000;
		private double[] BrakeNotchPressures;
		private double SapBcRatio = 0.94;
		private double SapBcOffset = 30000;
		private double BpInitialPressure = 490000;
		private double BrakeLeverDelay = 0.2;
		private double SapApplySpeed = 500;
		private double SapReleaseSpeed = 500;
		private double SapVolumeRatio = 20;
		private double ErApplySpeed = 500;
		private double ErReleaseSpeed = 500;
		private double ErVolumeRatio = 20;
		private double ErRapidReleaseSpeed = 1000;
		private double BpApplySpeed = 500;
		private double BpReleaseSpeed = 500;
		private double BpVolumeRatio = 20;
		private double BpRapidReleaseSpeed = 1000;
		private double CompressorLowerPressure = 700000;
		private double CompressorUpperPressure = 800000;
		private double CompressionSpeed = 5000;
		private double MotorCarWeight = 31500;
		private double TrailerCarWeight = 31500;
		private double MotorCarInertiaFactor = 0.1;
		private double TrailerCarInertiaFactor = 0.05;
		private double CarLength = 20;
		private double PassengerCapacity = 150; // default value at crush loading presumably
		private double PassengerWeight = 65;
		private double BoardingSpeed = 3.15; // time taken for 1 passenger to board in s
		private double AlightingSpeed = 6.3; // time taken for 1 passenger to alight in s
		private double DoorCloseSpeed = 5;
		private Vector3 Driver = new Vector3(-1, 2.5, -1);
		private Bve5PerformanceData PowerPerformanceData;
		private Bve5PerformanceData BrakePerformanceData;
		internal string GetDescription(string fileName)
		{
			ConfigFile<VehicleTxtSection, VehicleTxtKey> cfg = new ConfigFile<VehicleTxtSection, VehicleTxtKey>(fileName, Plugin.CurrentHost, "bvets vehicle", 0.03, 2.01, '#', true);
			// n.b. very early versions of this used a [Summary] block, but later don't use blocks at all
			Block<VehicleTxtSection, VehicleTxtKey> vehicleBlock = cfg.ReadNextBlock();
			vehicleBlock.GetValue(VehicleTxtKey.Comment, out string comment);
			return comment;
		}

		internal void Parse(string fileName, TrainBase train)
		{
			ConfigFile<VehicleTxtSection, VehicleTxtKey> cfg = new ConfigFile<VehicleTxtSection, VehicleTxtKey>(fileName, Plugin.CurrentHost, "bvets vehicle", 0.03, 2.01, '#', true);
			// n.b. very early versions of this used a [Summary] block, but later don't use blocks at all
			Block<VehicleTxtSection, VehicleTxtKey> vehicleBlock = cfg.ReadNextBlock();
			if (!vehicleBlock.GetPath(VehicleTxtKey.Parameters, Path.GetDirectoryName(fileName), out string parametersFile))
			{
				throw new Exception("BVE5: Unable to find the vehicle parameters file.");
			}
			ConfigFile<ParametersTxtSection, ParametersTxtKey> parameters = new ConfigFile<ParametersTxtSection, ParametersTxtKey>(parametersFile, Plugin.CurrentHost, "Bvets Vehicle Parameters", 2, 3, '#', true);
			// performance curves
			if (!vehicleBlock.GetPath(VehicleTxtKey.PerformanceCurve, Path.GetDirectoryName(fileName), out string performanceCurveTxtFile))
			{
				throw new Exception("BVE5: Unable to find the PerformanceCurve file.");
			}
			ConfigFile<PerformanceCurveTxtSection, PerformanceCurveTxtKey> performanceCurves = new ConfigFile<PerformanceCurveTxtSection, PerformanceCurveTxtKey>(performanceCurveTxtFile, Plugin.CurrentHost);
			parameters.ReadBlock(ParametersTxtSection.Default, out Block<ParametersTxtSection, ParametersTxtKey> defaultBlock); 
			// stuff not actually in a block...
			defaultBlock.GetValue(ParametersTxtKey.FirstCar, out string carType);
			FirstCarIsMotorCar = carType.Equals("M", StringComparison.InvariantCultureIgnoreCase);
			defaultBlock.TryGetValue(ParametersTxtKey.LoadCompensating, ref LoadCompensatingDevice);
			// cab
			if(parameters.ReadBlock(new[] { ParametersTxtSection.Cab, ParametersTxtSection.OneLeverCab}, out Block<ParametersTxtSection, ParametersTxtKey> cabBlock))
			{
				if (cabBlock.Key == ParametersTxtSection.OneLeverCab)
				{
					OneLeverCab = true;
				}

				cabBlock.TryGetValue(ParametersTxtKey.PowerNotchCount, ref PowerNotches);
				cabBlock.TryGetValue(ParametersTxtKey.BrakeNotchCount, ref BrakeNotches);
				cabBlock.TryGetValue(ParametersTxtKey.HoldingSpeedNotchCount, ref HoldBrakeNotch);
				cabBlock.TryGetValue(ParametersTxtKey.AtsCancelNotch, ref AtsCancelNotch);
				cabBlock.TryGetValue(ParametersTxtKey.MotorBrakeNotch, ref MotorBrakeNotch);
				cabBlock.TryGetStringArray(ParametersTxtKey.ReverserText, ',', ref ReverserNotchDescriptions);
				cabBlock.TryGetStringArray(ParametersTxtKey.PowerText, ',', ref PowerNotchDescriptions);
				cabBlock.TryGetStringArray(ParametersTxtKey.BrakeText, ',', ref BrakeNotchDescriptions);
				cabBlock.TryGetStringArray(ParametersTxtKey.HoldingSpeedText, ',', ref HoldNotchDescriptions);
			}
			else
			{
				throw new Exception("BVE5: The vehicle parameters must contain either a Cab or OneLeverCab section.");
			}
			// constant speed device
			if (parameters.ReadBlock(ParametersTxtSection.ConstantSpeedControl, out Block<ParametersTxtSection, ParametersTxtKey> constantSpeedBlock))
			{
				constantSpeedBlock.TryGetValue(ParametersTxtKey.Power, ref constantSpeedPower);
				constantSpeedBlock.TryGetValue(ParametersTxtKey.Neutral, ref constantSpeedNeutral);
				constantSpeedBlock.TryGetValue(ParametersTxtKey.Brake, ref constantSpeedBrake);
			}
			
			// main circuit
			if (parameters.ReadBlock(ParametersTxtSection.MainCircuit, out Block<ParametersTxtSection, ParametersTxtKey> mainCircuitBlock))
			{
				mainCircuitBlock.TryGetValue(ParametersTxtKey.RegenerationLimit, ref RegenerationLimit);
				if (!mainCircuitBlock.TryGetValue(ParametersTxtKey.RegenerationStartLimit, ref RegenerationStartLimit))
				{
					RegenerationStartLimit = RegenerationLimit + 5;
				}
				mainCircuitBlock.TryGetValue(ParametersTxtKey.LeverDelay, ref PowerLeverDelay);
				mainCircuitBlock.TryGetValue(ParametersTxtKey.SlipVelocityCoefficient, ref SlipVelocityCoefficient);
			}
			
			// power re-adhesion
			if (parameters.ReadBlock(ParametersTxtSection.PowerReAdhesion, out Block<ParametersTxtSection, ParametersTxtKey> powerReAdhesionBlock))
			{
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.SlipVelocity, ref SlipVelocity);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.SlipAcceleration, ref SlipAcceleration);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.BalanceAcceleration, ref BalanceAcceleration);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.HoldingTime, ref HoldingTime);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.ReferenceAcceleration, ref ReferenceAcceleration);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.ReferenceDeceleration, ref ReferenceDeceleration);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.CurrentDecrease, ref CurrentDecrease);
				powerReAdhesionBlock.TryGetValue(ParametersTxtKey.CurrentIncrease, ref CurrentIncrease);
			}
			
			// brakes
			if (parameters.ReadBlock(new[] { ParametersTxtSection.ECB, ParametersTxtSection.SMEE, ParametersTxtSection.CI }, out Block<ParametersTxtSection, ParametersTxtKey> brakeBlock))
			{
				BrakeType = brakeBlock.Key;
				brakeBlock.TryGetValue(ParametersTxtKey.MaximumPressure, ref MaximumPressure);
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (MaximumPressure == 1)
				{
					if ((BrakeType == ParametersTxtSection.ECB || BrakeType == ParametersTxtSection.SMEE) && !brakeBlock.TryGetDoubleArray(ParametersTxtKey.PressureRates, ',', ref BrakeNotchPressures) || BrakeNotchPressures.Length != BrakeNotches)
					{
						// CHECK: If no pressure is supplied for higher notches, does BVE5 actually just use the last one??
						BrakeNotchPressures = new double[] { };
						MaximumPressure = 440000;
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Invalid list of brake notch pressures supplied.");
					}
				}
				brakeBlock.TryGetValue(ParametersTxtKey.SapBcRatio, ref SapBcRatio);
				brakeBlock.TryGetValue(ParametersTxtKey.SapBcOffset, ref SapBcOffset);
				brakeBlock.TryGetValue(ParametersTxtKey.BpInitialPressure, ref BpInitialPressure);
				brakeBlock.TryGetValue(ParametersTxtKey.LeverDelay, ref BrakeLeverDelay);
			}
			else
			{
				throw new Exception("BVE5: The vehicle parameters must contain either a ECB, SMEE, or CI block.");
			}

			if (BrakeType == ParametersTxtSection.ECB)
			{
				// straight air pipe
				parameters.ReadBlock(ParametersTxtSection.SAP, out Block<ParametersTxtSection, ParametersTxtKey> sapBlock);
				sapBlock.TryGetValue(ParametersTxtKey.ApplySpeed, ref SapApplySpeed);
				sapBlock.TryGetValue(ParametersTxtKey.ReleaseSpeed, ref SapReleaseSpeed);
				sapBlock.TryGetValue(ParametersTxtKey.VolumeRatio, ref SapVolumeRatio);
			}

			if (BrakeType == ParametersTxtSection.CI)
			{
				// equalizing reservoir
				parameters.ReadBlock(ParametersTxtSection.ER, out Block<ParametersTxtSection, ParametersTxtKey> erBlock);
				erBlock.TryGetValue(ParametersTxtKey.ApplySpeed, ref ErApplySpeed);
				erBlock.TryGetValue(ParametersTxtKey.ReleaseSpeed, ref ErReleaseSpeed);
				erBlock.TryGetValue(ParametersTxtKey.VolumeRatio, ref ErVolumeRatio);
				erBlock.TryGetValue(ParametersTxtKey.RapidReleaseSpeed, ref ErRapidReleaseSpeed);
			}

			if (BrakeType == ParametersTxtSection.SMEE || BrakeType == ParametersTxtSection.CI)
			{
				// brake pipe
				parameters.ReadBlock(ParametersTxtSection.BP, out Block<ParametersTxtSection, ParametersTxtKey> bpBlock);
				bpBlock.TryGetValue(ParametersTxtKey.ApplySpeed, ref BpApplySpeed);
				bpBlock.TryGetValue(ParametersTxtKey.ReleaseSpeed, ref BpReleaseSpeed);
				bpBlock.TryGetValue(ParametersTxtKey.VolumeRatio, ref BpVolumeRatio);
				bpBlock.TryGetValue(ParametersTxtKey.RapidReleaseSpeed, ref BpRapidReleaseSpeed);
			}

			// compressor
			if (parameters.ReadBlock(ParametersTxtSection.Compressor, out Block<ParametersTxtSection, ParametersTxtKey> compressorBlock))
			{
				compressorBlock.TryGetValue(ParametersTxtKey.LowerPressure, ref CompressorLowerPressure);
				compressorBlock.TryGetValue(ParametersTxtKey.UpperPressure, ref CompressorUpperPressure);
				compressorBlock.TryGetValue(ParametersTxtKey.CompressionSpeed, ref CompressionSpeed);
			}
			
			// dynamics
			if (parameters.ReadBlock(ParametersTxtSection.Dynamics, out Block<ParametersTxtSection, ParametersTxtKey> dynamicsBlock))
			{
				dynamicsBlock.TryGetValue(ParametersTxtKey.MotorCarWeight, ref MotorCarWeight);
				dynamicsBlock.TryGetValue(ParametersTxtKey.TrailerWeight, ref TrailerCarWeight);
				dynamicsBlock.TryGetValue(ParametersTxtKey.MotorCarCount, ref MotorCars);
				dynamicsBlock.TryGetValue(ParametersTxtKey.TrailerCount, ref TrailerCars);
				dynamicsBlock.TryGetValue(ParametersTxtKey.MotorcarInertiaFactor, ref MotorCarInertiaFactor);
				dynamicsBlock.TryGetValue(ParametersTxtKey.TrailerInertiaFactor, ref TrailerCarInertiaFactor);
				dynamicsBlock.TryGetValue(ParametersTxtKey.CarLength, ref CarLength);
			}
			
			// passengers
			if (parameters.ReadBlock(ParametersTxtSection.Passengers, out Block<ParametersTxtSection, ParametersTxtKey> passengersBlock))
			{
				passengersBlock.TryGetValue(ParametersTxtKey.Capacity, ref PassengerCapacity);
				passengersBlock.TryGetValue(ParametersTxtKey.BodyWeight, ref PassengerWeight);
				passengersBlock.TryGetValue(ParametersTxtKey.BoardingSpeed, ref BoardingSpeed);
				passengersBlock.TryGetValue(ParametersTxtKey.AlightingSpeed, ref AlightingSpeed);
			}
			
			// door
			if (parameters.ReadBlock(ParametersTxtSection.Door, out Block<ParametersTxtSection, ParametersTxtKey> doorsBlock))
			{
				doorsBlock.TryGetValue(ParametersTxtKey.CloseTime, ref DoorCloseSpeed);
			}
			
			// viewpoint
			if (parameters.ReadBlock(ParametersTxtSection.ViewPoint, out Block<ParametersTxtSection, ParametersTxtKey> viewPointBlock))
			{
				viewPointBlock.TryGetValue(ParametersTxtKey.X, ref Driver.X);
				viewPointBlock.TryGetValue(ParametersTxtKey.Y, ref Driver.Y);
				viewPointBlock.TryGetValue(ParametersTxtKey.Z, ref Driver.Z);
			}
			
			if (MotorCars == 0)
			{
				throw new Exception("BVE5: A train must have at least one motor car.");
			}

			if (!performanceCurves.ReadBlock(PerformanceCurveTxtSection.Power, out Block<PerformanceCurveTxtSection, PerformanceCurveTxtKey> powerPerformanceBlock))
			{
				throw new Exception("BVE5: Power performance data section missing.");
			}
			PowerPerformanceData = ParsePerformanceData(powerPerformanceBlock, Path.GetDirectoryName(performanceCurveTxtFile));

			if (!performanceCurves.ReadBlock(PerformanceCurveTxtSection.Brake, out Block<PerformanceCurveTxtSection, PerformanceCurveTxtKey> brakePerformanceBlock))
			{
				throw new Exception("BVE5: Brake performance data section missing.");
			}
			BrakePerformanceData = ParsePerformanceData(brakePerformanceBlock, Path.GetDirectoryName(performanceCurveTxtFile));

			bool[] motorCars = TrainDatParser.AssignMotorCars(MotorCars, TrailerCars, FirstCarIsMotorCar);

			/*
			double OperatingPressure;
			if (BrakeType == ParametersTxtSection.CI)
			{
				OperatingPressure = 440000.0 + 0.75 * (690000.0 - 440000.0);
				if (OperatingPressure > 690000.0)
				{
					OperatingPressure = 690000.0;
				}
			}
			else
			{
				if (440000.0 < 480000.0 & 690000.0 > 500000.0)
				{
					OperatingPressure = 490000.0;
				}
				else
				{
					OperatingPressure = 440000.0 + 0.75 * (690000.0 - 440000.0);
				}
			}
			*/

			train.Cars = new CarBase[MotorCars + TrailerCars];
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i] = new CarBase(train, i);
				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i / 2], train.Cars.Length > 1 ? train.Cars[i / 2 + 1] : null);
				switch (BrakeType)
				{
					case ParametersTxtSection.ECB:
						// electric command brake
						train.Cars[i].CarBrake = new ElectricCommandBrake(EletropneumaticBrakeType.None, train.Cars[i], 0,0, 0, BrakePerformanceData);
						break;
					case ParametersTxtSection.SMEE:
						// electromagnetic straight air brake
						train.Cars[i].CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, train.Cars[i], BrakePerformanceData);
						break;
					case ParametersTxtSection.CI:
						// automatic air brake
						//train.Cars[i].CarBrake = new ElectricCommandBrake();
						break;
					default:
						throw new Exception("BVE5: Invalid brake type.");
				}

				train.Cars[i].CarBrake.MainReservoir = new MainReservoir(690000.0, 780000.0, 0.01, (BrakeType == ParametersTxtSection.CI ? 0.25 : 0.075) / train.Cars.Length);
				train.Cars[i].CarBrake.MainReservoir.Volume = 0.5;

				train.Cars[i].CarBrake.EqualizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0);
				train.Cars[i].CarBrake.EqualizingReservoir.NormalPressure = 1.005 * BpInitialPressure;
				train.Cars[i].CarBrake.EqualizingReservoir.Volume = 0.015; // very small reservoir for observation, so guess at 15L

				train.Cars[i].CarBrake.BrakePipe = new BrakePipe(BpInitialPressure, 10000000.0, 1500000.0, 5000000.0, BrakeType == ParametersTxtSection.ECB);
				train.Cars[i].CarBrake.BrakePipe.Volume = Math.Pow(0.0175 * Math.PI, 2) * (train.Cars.Length * 1.05); // Assuming Railway Group Standards 3.5cm diameter brake pipe, 5% extra length for bends etc.

				AirBrake airBrake = train.Cars[i].CarBrake as AirBrake;
				airBrake.StraightAirPipe = new StraightAirPipe(300000.0, 400000.0, 200000.0);
				double r = 200000.0 / 440000.0 - 1.0;
				if (r < 0.1) r = 0.1;
				if (r > 1.0) r = 1.0;
				train.Cars[i].CarBrake.AuxiliaryReservoir = new AuxiliaryReservoir(0.975 * BpInitialPressure, 200000.0, 0.5, r);
				train.Cars[i].CarBrake.AuxiliaryReservoir.Volume = 0.16; // guessed 1/3 of main reservoir volume
				if (motorCars[i])
				{
					train.Cars[i].TractionModel = new Bve5MotorCar(train.Cars[i], PowerPerformanceData, BrakePerformanceData);
					airBrake.BrakeType = TrainManager.BrakeSystems.BrakeType.Main;
					airBrake.Compressor = new Compressor(5000.0, train.Cars[i].CarBrake.MainReservoir, train.Cars[i]);
					train.Cars[i].EmptyMass = MotorCarWeight;
				}
				else
				{
					train.Cars[i].TractionModel = new Bve5TrailerCar(train.Cars[i]);
					airBrake.BrakeType = TrainManager.BrakeSystems.BrakeType.Auxiliary;
					train.Cars[i].EmptyMass = TrailerCarWeight;
				}

				train.Cars[i].CarBrake.BrakeCylinder = new BrakeCylinder(440000.0, 440000.0, BrakeType == ParametersTxtSection.CI ? 300000.0 : 0.3 * 300000.0, 300000.0, 300000.0);
				train.Cars[i].CarBrake.BrakeCylinder.Volume = 0.14; // 35cm diameter, 15cm stroke
				train.Cars[i].CarBrake.JerkUp = 10;
				train.Cars[i].CarBrake.JerkDown = 10;
				train.Cars[i].CarBrake.Initialize(Plugin.CurrentOptions.TrainStart);

				train.Cars[i].Doors[0] = new Door(-1, 1000, 0);
				train.Cars[i].Doors[1] = new Door(1, 1000, 0);
				train.Cars[i].ConstSpeed = new CarConstSpeed(train.Cars[i]);
				train.Cars[i].HoldBrake = new CarHoldBrake(train.Cars[i]);

				train.Cars[i].Specs.JerkPowerUp = 10;
				train.Cars[i].Specs.JerkPowerDown = 10;

				train.Cars[i].Width = 2.6;
				train.Cars[i].Height = 3.6;
				train.Cars[i].Length = CarLength;
				train.Cars[i].Specs.ExposedFrontalArea = 0.6 * train.Cars[i].Width * train.Cars[i].Height;
				train.Cars[i].Specs.UnexposedFrontalArea = 0.2 * train.Cars[i].Width * train.Cars[i].Height;
				train.Cars[i].Specs.CenterOfGravityHeight = 1.6;
				train.Cars[i].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * train.Cars[i].Specs.CenterOfGravityHeight / train.Cars[i].Width);
				train.Cars[i].FrontAxle.Follower.TriggerType = i == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
				train.Cars[i].RearAxle.Follower.TriggerType = i == train.Cars.Length - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
				train.Cars[i].BeaconReceiver.TriggerType = i == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
				train.Cars[i].BeaconReceiverPosition = 0.5 * CarLength;
				train.Cars[i].FrontAxle.Position = 0.4 * CarLength;
				train.Cars[i].RearAxle.Position = -0.4 * CarLength;
				train.Cars[i].ChangeCarSection(CarSectionType.NotVisible);
			}

			train.Handles.EmergencyBrake = new EmergencyHandle(train);
			train.Handles.Power = new PowerHandle(PowerNotches, train);
			train.Handles.Brake = new BrakeHandle(BrakeNotches, train.Handles.EmergencyBrake, train);
			train.Handles.HasLocoBrake = false;
			train.Handles.LocoBrake = new LocoBrakeHandle(0, train.Handles.EmergencyBrake, train);
			train.Handles.LocoBrakeType = LocoBrakeType.Independant;
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			train.Handles.HasHoldBrake = false;
			

			train.DriverCar = 0; // CHECK: BVE5 doesn't seem to allow setting of non-zero driver car

			train.SafetySystems.PassAlarm = new PassAlarm(PassAlarmType.Loop, train.Cars[0]); // CHECK: No pass alarm setting in the BVE5 documentation, but is in the K_SEI3500R sound.txt
			train.Cars[0].Breaker = new Breaker(train.Cars[0]);
			train.SafetySystems.PilotLamp = new PilotLamp(train.Cars[0]);
			train.SafetySystems.Headlights = new LightSource(train, 1);

			if (vehicleBlock.GetPath(VehicleTxtKey.Panel, Path.GetDirectoryName(fileName), out string panelFile))
			{
				train.Cars[0].CarSections.Add(CarSectionType.Interior, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true));
				train.Cars[0].Driver = Driver;
				Plugin.Panel2CfgParser.ParsePanel2Config(Path.GetFileName(panelFile), Path.GetDirectoryName(panelFile), train.Cars[0]);
				train.Cars[train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
				Plugin.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
			}

			if (vehicleBlock.GetPath(VehicleTxtKey.MotorNoise, Path.GetDirectoryName(fileName), out string motorNoiseFile))
			{
				string noisePath = Path.GetDirectoryName(motorNoiseFile);
				Block<MotorNoiseTxtSection, MotorNoiseTxtKey> motorNoiseBlock = new ConfigFile<MotorNoiseTxtSection, MotorNoiseTxtKey>(motorNoiseFile, Plugin.CurrentHost);
				if (motorNoiseBlock.ReadBlock(MotorNoiseTxtSection.Power, out Block<MotorNoiseTxtSection, MotorNoiseTxtKey> powerNoiseBlock) &&
				    motorNoiseBlock.ReadBlock(MotorNoiseTxtSection.Brake, out Block<MotorNoiseTxtSection, MotorNoiseTxtKey> brakeNoiseBlock))
				{
					powerNoiseBlock.GetPath(MotorNoiseTxtKey.Frequency, noisePath, out string powerFreq);
					powerNoiseBlock.GetPath(MotorNoiseTxtKey.Volume, noisePath, out string powerVol);
					brakeNoiseBlock.GetPath(MotorNoiseTxtKey.Frequency, noisePath, out string brakeFreq);
					brakeNoiseBlock.GetPath(MotorNoiseTxtKey.Volume, noisePath, out string brakeVol);
					
					for (int i = 0; i < train.Cars.Length; i++)
					{
						train.Cars[i].TractionModel.MotorSounds = Bve5MotorSoundTableParser.Parse(train.Cars[i], powerFreq, powerVol, brakeFreq, brakeVol);
					}
				}
			}

			if (vehicleBlock.GetPath(VehicleTxtKey.Sound, Path.GetDirectoryName(fileName), out string soundFile))
			{
				Plugin.BVE4SoundParser.Parse(soundFile, Path.GetDirectoryName(soundFile), train);
			}



			train.CameraCar = 0;
			train.Specs.AveragesPressureDistribution = true;
			train.PlaceCars(0.0);
			
		}

    }
}
