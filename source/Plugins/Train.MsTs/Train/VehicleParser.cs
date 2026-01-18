//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
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

using LibRender2.Smoke;
using LibRender2.Trains;
using OpenBve.Formats.MsTs;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal partial class WagonParser
	{
		private readonly Dictionary<string, string> wagonCache;
		private readonly Dictionary<string, string> engineCache;
		private readonly List<string> soundFiles;
		private string[] wagonFiles;
		private double wheelRadius;
		private bool exteriorLoaded = false;
		private bool RequiresTender = false;
		private bool EngineAlreadyParsed = false;

		internal WagonParser()
		{
			wagonCache = new Dictionary<string, string>();
			engineCache = new Dictionary<string, string>();
			soundFiles = new List<string>();
		}

		internal void Parse(string trainSetDirectory, string wagonName, bool isEngine, ref CarBase currentCar, ref TrainBase train)
		{
			/*
			 * Reset to 'sensible' defaults for required parameters
			 * => International Union of Railways figure for 'standard' wheel size
			 * => Default BVE brake system parameters (we know these work correctly)
			 * => Reset gearbox parameters
			 * => Set max force figures to zero
			 */
			wheelRadius = 0.92;
			mainReservoirMinimumPressure = 690000;
			mainReservoirMaximumPressure = 780000;
			brakeCylinderMaximumPressure = 440000;
			compressionRate = 3500;
			maxForce = 0;
			maxBrakeForce = 0;
			Gears = null;
			exteriorLoaded = false;
			wagonFiles = Directory.GetFiles(trainSetDirectory, isEngine ? "*.eng" : "*.wag", SearchOption.AllDirectories);
			currentEngineType = EngineType.NoEngine;
			ParticleSources = new List<ParticleSource>();
			//ExhaustMaxMagnitude = 0;
			//ExhaustInitialRate = 0;
			//ExhaustMaxRate = 0;
			/*
			 * MSTS maintains an internal database, as opposed to using full paths
			 * Unfortunately, this means we've got to do an approximation of the same thing!
			 * (TrainStore is / was an early MSTS attempt to deal with the same problem by moving
			 * excess eng, wag and con files out from the MSTS directory)
			 *
			 * Unclear at the minute as to whether an eng can refer to a *separate* wag file, but
			 * unless documentation specifically states otherwise, we'll assume it can
			 *
			 * So, the *first* thing we need to do is to read the engine (as this may
			 * refer to a different sub-wagon):
			 */
			if (isEngine)
			{
				if (engineCache.TryGetValue(wagonName, out string engineFile))
				{
					ReadWagonData(engineFile, ref wagonName, true, ref currentCar, ref train);
				}
				else
				{
					for (int i = 0; i < wagonFiles.Length; i++)
					{
						if (ReadWagonData(wagonFiles[i], ref wagonName, true, ref currentCar, ref train))
						{
							break;
						}
					}
				}
			}
			else
			{
				currentCar.TractionModel = new BVETrailerCar(currentCar);
			}
			/*
			 * We've now found the engine properties-
			 * Now, we need to read the wagon properties to find the visual wagon to display
			 * (The Engine only holds the physics data)
			 */
			if (wagonCache.TryGetValue(wagonName, out string wagonFile))
			{
				ReadWagonData(wagonFile, ref wagonName, false, ref currentCar, ref train);
			}
			else
			{
				for (int i = 0; i < wagonFiles.Length; i++)
				{
					if (ReadWagonData(wagonFiles[i], ref wagonName, false, ref currentCar, ref train))
					{
						break;
					}
				}
			}

			// as properties may not be in order, set this stuff last
			if (isEngine)
			{
				EngineAlreadyParsed = true;
				// FIXME: Default BVE values
				currentCar.Specs.JerkPowerUp = 10.0;
				currentCar.Specs.JerkPowerDown = 10.0;
				if (currentCar.DrivingWheels.Count == 0)
				{
					// An engine must implicitly have at least one set of driving wheels
					currentCar.DrivingWheels.Add(new Wheels(1, wheelRadius));
				}
				// NOTE: Amps figure appears to need to be divided by the total wheels (to give a per-axle figure)
				// see NWC_40138.eng for example- This is an issue where higher amps figures are in play
				maxEngineAmps /= currentCar.DrivingWheels[0].TotalNumber;
				maxBrakeAmps /= currentCar.DrivingWheels[0].TotalNumber;
				
				switch (currentEngineType)
				{
					case EngineType.Diesel:
						currentCar.TractionModel = new DieselEngine(currentCar, new AccelerationCurve[] { new MSTSAccelerationCurve(currentCar, maxForce, maxContinuousForce, maxVelocity) }, dieselIdleRPM, dieselIdleRPM, dieselMaxRPM, dieselRPMChangeRate, dieselRPMChangeRate, dieselIdleUse, dieselMaxUse);
						currentCar.TractionModel.FuelTank = new FuelTank(GetMaxDieselCapacity(currentCar.Index));
						currentCar.TractionModel.IsRunning = true;

						if (maxBrakeAmps > 0 && maxEngineAmps > 0)
						{
							currentCar.TractionModel.Components.Add(EngineComponent.RegenerativeTractionMotor, new RegenerativeTractionMotor(currentCar.TractionModel, maxEngineAmps, maxBrakeAmps));
						}
						else if (maxEngineAmps > 0)
						{
							currentCar.TractionModel.Components.Add(EngineComponent.TractionMotor, new TractionMotor(currentCar.TractionModel, maxEngineAmps));
						}
						break;
					case EngineType.DieselHydraulic:
						AccelerationCurve[] accelerationCurves;
						if (Gears == null)
						{
							accelerationCurves = new AccelerationCurve[] { new MSTSAccelerationCurve(currentCar, maxForce, maxContinuousForce, maxVelocity) };
						}
						else
						{
							accelerationCurves = new AccelerationCurve[Gears.Length];
							for (int i = 0; i < Gears.Length; i++)
							{
								accelerationCurves[i] = new MSTSAccelerationCurve(currentCar, Gears[i].MaxTractiveForce, maxContinuousForce, Gears[i].MaximumSpeed);
							}
						}

						currentCar.TractionModel = new DieselEngine(currentCar, accelerationCurves, dieselIdleRPM, dieselIdleRPM, dieselMaxRPM, dieselRPMChangeRate, dieselRPMChangeRate, dieselIdleUse, dieselMaxUse);
						currentCar.TractionModel.FuelTank = new FuelTank(GetMaxDieselCapacity(currentCar.Index));
						currentCar.TractionModel.IsRunning = true;
						currentCar.TractionModel.Components.Add(EngineComponent.Gearbox, new Gearbox(currentCar.TractionModel, Gears, gearboxOperationMode));
						break;
					case EngineType.Electric:
						currentCar.TractionModel = new ElectricEngine(currentCar, new AccelerationCurve[] { new MSTSAccelerationCurve(currentCar, maxForce, maxContinuousForce, maxVelocity) });
						currentCar.TractionModel.Components.Add(EngineComponent.Pantograph, new Pantograph(currentCar.TractionModel));

						if (maxBrakeAmps > 0 && maxEngineAmps > 0)
						{
							currentCar.TractionModel.Components.Add(EngineComponent.RegenerativeTractionMotor, new RegenerativeTractionMotor(currentCar.TractionModel, maxEngineAmps, maxBrakeAmps));
						}
						else if (maxEngineAmps > 0)
						{
							currentCar.TractionModel.Components.Add(EngineComponent.TractionMotor, new TractionMotor(currentCar.TractionModel, maxEngineAmps));
						}
						break;
					case EngineType.Steam:
						// NOT YET IMPLEMENTED FULLY
						if (RequiresTender)
						{
							currentCar.TractionModel = new TenderEngine(currentCar, new AccelerationCurve[] { new MSTSAccelerationCurve(currentCar, maxForce, maxContinuousForce, maxVelocity) });
							if (currentCar.Index > 0)
							{
								CarBase previousCar = currentCar.baseTrain.Cars[currentCar.Index - 1];
								if (previousCar.TractionModel is Tender tender && tender.MaxWaterLevel == -1)
								{
									// recreate, as values are stored in the ENG
									previousCar.TractionModel = new Tender(previousCar, MaxFuelLevel, MaxWaterLevel);
								}
							}
						}
						else
						{
							currentCar.TractionModel = new TankEngine(currentCar, new AccelerationCurve[] { new MSTSAccelerationCurve(currentCar, maxForce, maxContinuousForce, maxVelocity) }, MaxFuelLevel, MaxWaterLevel);
						}

						currentCar.TractionModel.Components.Add(EngineComponent.CylinderCocks, new CylinderCocks(currentCar.TractionModel));
						currentCar.TractionModel.Components.Add(EngineComponent.Blowers, new Blowers(currentCar.TractionModel));
						break;
					case EngineType.NoEngine:
						currentCar.TractionModel = new BVETrailerCar(currentCar);
						break;
				}
				
				if (currentCar.ReAdhesionDevice == null)
				{
					currentCar.ReAdhesionDevice = new BveReAdhesionDevice(currentCar, hasAntiSlipDevice ? ReadhesionDeviceType.TypeB : ReadhesionDeviceType.NotFitted);
				}
				
				currentCar.Windscreen = new Windscreen(0, 0, currentCar);
				currentCar.Windscreen.Wipers = new WindscreenWiper(currentCar.Windscreen, WiperPosition.Left, WiperPosition.Left, 1.0, 1.0);

				for (int i = 0; i < ParticleSources.Count; i++)
				{
					if (ExhaustMaxMagnitude == 0)
					{
						if (currentEngineType == EngineType.Steam)
						{
							// Steam locomotives don't seem to have a max particle size setting
							// this is a fudge to something that looks reasonable
							ExhaustMaxMagnitude = 40 * ParticleSources[i].Size;
						}
						else
						{
							// also handle any diesels which don't set it
							// again, fudge to something that looks OK
							ExhaustMaxMagnitude = 10 * ParticleSources[i].Size;
						}
					}

					// NOTES: particle life / sizes is just a fudge at the minute
					// * Diesel exhaust actually specifies max size, if missing assumed to spread a bit, but not too much, much shorter life
					// * Funnel steam assumed to spread lots, long lasting to give a nice trail
					// * WhistleFX short life, smaller spread

					LibRender2.Smoke.ParticleSource particleSource = null;
					switch (ParticleSources[i].Token)
					{
						case KujuTokenID.Exhaust1:
						case KujuTokenID.Exhaust2:
						case KujuTokenID.Exhaust3:
						case KujuTokenID.Exhaust4:
						case KujuTokenID.StackFX:
							particleSource = new LibRender2.Smoke.ParticleSource(Plugin.Renderer, currentCar, ParticleSources[i].Offset, ParticleSources[i].Size, ExhaustMaxMagnitude, ParticleSources[i].Direction, currentEngineType == EngineType.Diesel ? 15.0 : 40.0, currentEngineType == EngineType.Diesel ? ParticleType.Smoke : ParticleType.Steam);
							particleSource.Controller = new FunctionScript(Plugin.CurrentHost, currentCar.Index + " enginepowerindex", false);
							break;
						case KujuTokenID.WhistleFX:
							particleSource = new LibRender2.Smoke.ParticleSource(Plugin.Renderer, currentCar, ParticleSources[i].Offset, ParticleSources[i].Size, ParticleSources[i].Size * 15, ParticleSources[i].Direction, 5.0, ParticleType.Steam);
							particleSource.Controller = new FunctionScript(Plugin.CurrentHost, "primaryklaxon", false);
							particleSource.EmitsAtIdle = false;
							break;
						case KujuTokenID.CylindersFX:
							particleSource = new LibRender2.Smoke.ParticleSource(Plugin.Renderer, currentCar, ParticleSources[i].Offset, ParticleSources[i].Size, ParticleSources[i].Size * 25, ParticleSources[i].Direction, 10.0, ParticleType.Steam);
							particleSource.Controller = new FunctionScript(Plugin.CurrentHost, currentCar.Index + " enginepowerindex " + currentCar.Index + " cylindercocksstateindex *", false);
							particleSource.EmitsAtIdle = false;
							break;

					}

					if (particleSource != null)
					{
						currentCar.ParticleSources.Add(particleSource);
					}
				}
			}
			else
			{
				if (currentWagonType == WagonType.Tender)
				{
					currentCar.TractionModel = new Tender(currentCar, MaxFuelLevel, MaxWaterLevel);
				}

				if (currentCar.TrailingWheels.Count == 0)
				{
					// non-engine must implicitly have at least one set of trailing wheels
					currentCar.TrailingWheels.Add(new Wheels(1, wheelRadius));
				}
			}

			if (brakeSystemTypes != null)
			{

				// Add brakes last, as we need the acceleration values
				if (brakeSystemTypes.Contains(BrakeSystemType.Vacuum_Piped) || brakeSystemTypes.Contains(BrakeSystemType.Air_Piped) || (brakeSystemTypes.Length == 1 && brakeSystemTypes[0] == BrakeSystemType.Handbrake))
				{
					/*
					 * FIXME: Need to implement vac braked / air piped and vice-versa, but for the minute, we'll assume that if one or the other is present
					 * then the vehicle has no brakes
					 */
					currentCar.CarBrake = new ThroughPiped(currentCar);
				}
				else
				{
					if (brakeSystemTypes.Contains(BrakeSystemType.Air_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Air_twin_pipe) || brakeSystemTypes.Contains(BrakeSystemType.EP) || brakeSystemTypes.Contains(BrakeSystemType.ECP))
					{
						AirBrake airBrake;
						// FIXME: Relationship of brake system values needs to be (close) to original proportions else the physics bug out
						double bcVal = brakeCylinderMaximumPressure / 440000;
						double emergencyVal = emergencyRate / 300000.0;
						double releaseVal = releaseRate / 200000.0;
						mainReservoirMinimumPressure = 690000.0 * bcVal;
						mainReservoirMaximumPressure = 780000.0 * bcVal;
						double operatingPressure = brakeCylinderMaximumPressure + 0.75 * (mainReservoirMinimumPressure - brakeCylinderMaximumPressure);

						if (brakeSystemTypes.Contains(BrakeSystemType.EP) || brakeSystemTypes.Contains(BrakeSystemType.ECP))
						{
							// Combined air brakes and control signals
							// Assume equivalent to ElectromagneticStraightAirBrake
							airBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, currentCar, 0, 0, 0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxBrakeForce == 0 ? maxForce : maxBrakeForce) });
							airBrake.BrakePipe = new BrakePipe(operatingPressure, 10000000.0, 1500000.0, 5000000.0, true);
						}
						else
						{
							// Assume equivalent to ElectromagneticStraightAirBrake
							airBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, currentCar, 0, 0, 0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxBrakeForce == 0 ? maxForce : maxBrakeForce) });
							airBrake.BrakePipe = new BrakePipe(operatingPressure, 10000000.0, 1500000.0, 5000000.0, false);
						}

						airBrake.MainReservoir = new MainReservoir(mainReservoirMinimumPressure, mainReservoirMaximumPressure, 0.01, 0.075 / train.Cars.Length);
						airBrake.Compressor = new Compressor(5000.0, airBrake.MainReservoir, currentCar);
						airBrake.BrakeCylinder = new BrakeCylinder(brakeCylinderMaximumPressure, brakeCylinderMaximumPressure * 1.1, 90000.0, emergencyRate, releaseRate);
						double r = 200000.0 / airBrake.BrakeCylinder.EmergencyMaximumPressure - 1.0;
						if (r < 0.1) r = 0.1;
						if (r > 1.0) r = 1.0;
						airBrake.AuxiliaryReservoir = new AuxiliaryReservoir(0.975 * operatingPressure, 200000.0, 0.5, r);
						airBrake.EqualizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0);
						airBrake.EqualizingReservoir.NormalPressure = 1.005 * operatingPressure;
						airBrake.StraightAirPipe = new StraightAirPipe(300000.0, emergencyRate * emergencyVal, releaseRate * releaseVal);

						currentCar.CarBrake = airBrake;
					}

					if (brakeSystemTypes.Contains(BrakeSystemType.Vaccum_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_twin_pipe))
					{
						VacuumBrake vacuumBrake = new VacuumBrake(currentCar, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxBrakeForce == 0 ? maxForce : maxBrakeForce) });
						vacuumBrake.MainReservoir = new MainReservoir(71110, 84660, 0.01, 0.075 / train.Cars.Length); // ~21in/hg - ~25in/hg
						vacuumBrake.BrakeCylinder = new BrakeCylinder(brakeCylinderMaximumPressure, brakeCylinderMaximumPressure * 1.1, 90000.0, 300000.0, 200000.0);
						vacuumBrake.AuxiliaryReservoir = new AuxiliaryReservoir(0.975 * brakeCylinderMaximumPressure, 200000.0, 0.5, 1.0);
						vacuumBrake.EqualizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0);
						vacuumBrake.EqualizingReservoir.NormalPressure = 1.005 * (vacuumBrake.BrakeCylinder.EmergencyMaximumPressure + 0.75 * (vacuumBrake.MainReservoir.MinimumPressure - vacuumBrake.BrakeCylinder.EmergencyMaximumPressure));
						vacuumBrake.BrakePipe = new BrakePipe(brakeCylinderMaximumPressure, 10000000.0, 1500000.0, 5000000.0, false);
						currentCar.CarBrake = vacuumBrake;
					}

					currentCar.CarBrake.BrakeType = currentCar.Index == train.DriverCar || isEngine ? BrakeType.Main : BrakeType.Auxiliary;
					currentCar.CarBrake.JerkUp = 10;
					currentCar.CarBrake.JerkDown = 10;
				}

				currentCar.CarBrake.Initialize(TrainStartMode.ServiceBrakesAts);
			}
			else
			{
				currentCar.CarBrake = new ThroughPiped(currentCar);
			}

			currentCar.FrontAxle = new MSTSAxle(Plugin.CurrentHost, train, currentCar, friction ?? new Friction(), adhesion ?? new Adhesion(currentCar, currentEngineType == EngineType.Steam));
			currentCar.RearAxle = new MSTSAxle(Plugin.CurrentHost, train, currentCar, friction ?? new Friction(), adhesion ?? new Adhesion(currentCar, currentEngineType == EngineType.Steam));

			if (soundFiles.Count > 0)
			{
				for (int i = 0; i < soundFiles.Count; i++)
				{
					SoundModelSystemParser.ParseSoundFile(soundFiles[i], ref currentCar);
				}
				soundFiles.Clear();
			}
		}

		internal bool ReadWagonData(string fileName, ref string wagonName, bool isEngine, ref CarBase car, ref TrainBase train)
		{
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			vigilanceDevices = new List<VigilanceDevice>();
			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = buffer[0] == 0xFF && buffer[1] == 0xFE;

			string headerString;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				headerString = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 2, 14);
				headerString = Encoding.ASCII.GetString(buffer, 0, 8);
			}

			// SIMISA@F  means compressed
			// SIMISA@@  means uncompressed
			if (headerString.StartsWith("SIMISA@F"))
			{
				fb = new ZlibStream(fb, CompressionMode.Decompress);
			}
			else if (headerString.StartsWith("\r\nSIMISA"))
			{
				// ie us1rd2l1000r10d.s, we are going to allow this but warn
				Console.Error.WriteLine("Improper header in " + fileName);
				fb.Read(buffer, 0, 4);
			}
			else if (!headerString.StartsWith("SIMISA@@"))
			{
				throw new Exception("MSTS Vehicle Parser: Unrecognized vehicle file header " + headerString + " in " + fileName);
			}

			string subHeader;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				subHeader = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 0, 16);
				subHeader = Encoding.ASCII.GetString(buffer, 0, 8);
			}
			if (subHeader[7] == 't')
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					byte[] newBytes = reader.ReadBytes((int)(fb.Length - fb.Position));
					string s = unicode ? Encoding.Unicode.GetString(newBytes) : Encoding.ASCII.GetString(newBytes);

					/*
					 * Engine files contain two blocks, not in an enclosing block
					 * Assume that these can be of arbitrary order, so read using a dictionary
					 */
					List<Block> blocks = TextualBlock.ReadBlocks(s);

					List<Block> wagonBlocks = blocks.Where(b => b.Token == KujuTokenID.Wagon).ToList();
					List<Block> engineBlocks = blocks.Where(b => b.Token == KujuTokenID.Engine).ToList();

					if (wagonBlocks.Count == 0)
					{
						//Not found any wagon data in this file
						return false;
					}
					if (isEngine && engineBlocks.Count > 0)
					{
						if (engineBlocks.Count > 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Multiple engine blocks encountered in MSTS ENG file "+ fileName);
						}
						return ParseBlock(engineBlocks[0], fileName, ref wagonName, true, ref car, ref train);
					}
					if (!isEngine && wagonBlocks.Count > 0)
					{
						if (wagonBlocks.Count > 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Multiple wagon blocks encountered in MSTS WAG file " + fileName);
						}
						return ParseBlock(wagonBlocks[0], fileName, ref wagonName, false, ref car, ref train);
					}
					return false;
				}
					
			}
			if (subHeader[7] != 'b')
			{
				throw new Exception("Unrecognized subHeader \"" + subHeader + "\" in " + fileName);
			}

			using (BinaryReader reader = new BinaryReader(fb))
			{
				KujuTokenID currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.Wagon)
				{
					throw new Exception(); //Shape definition
				}
				reader.ReadUInt16(); 
				uint remainingBytes = reader.ReadUInt32();
				byte[] newBytes = reader.ReadBytes((int) remainingBytes);
				BinaryBlock block = new BinaryBlock(newBytes, KujuTokenID.Wagon);
				try
				{
					ParseBlock(block, fileName, ref wagonName, isEngine, ref car, ref train);
				}
				catch (InvalidDataException)
				{
					return false;
				}
					
			}
			return true;
		}

		private double maxForce = 0;
		private double maxContinuousForce = 0;
		private double maxBrakeForce = 0;
		private BrakeSystemType[] brakeSystemTypes;
		private EngineType currentEngineType;
		private WagonType currentWagonType;
		private double dieselIdleRPM;
		private double dieselMaxRPM;
		private double dieselRPMChangeRate;
		private double dieselIdleUse;
		private double dieselMaxUse;
		private double dieselCapacity;
		private double dieselMaxTractiveEffortSpeed;
		private double maxEngineAmps;
		private double maxBrakeAmps;
		private double mainReservoirMinimumPressure = 690000.0;
		private double mainReservoirMaximumPressure = 780000.0;
		private double brakeCylinderMaximumPressure = 440000.0;
		private double emergencyRate;
		private double releaseRate;
		private double compressionRate = 3500;
		private double maxVelocity;
		private bool hasAntiSlipDevice;
		private List<VigilanceDevice> vigilanceDevices;
		private List<ParticleSource> ParticleSources;
		private Gear[] Gears;
		private GearboxOperation gearboxOperationMode = GearboxOperation.Manual;
		private double maxSandingSpeed;
		private CouplingType couplingType;
		private Friction friction;
		private Adhesion adhesion;
		private UnitOfPressure brakeSystemDefaultUnits = UnitOfPressure.PoundsPerSquareInch;
		private double MaxWaterLevel = -1;
		private double MaxFuelLevel = -1;
		/// <summary>The maximum expanded size of a particle</summary>
		internal double ExhaustMaxMagnitude;
		/// <summary>The rate of particle emissions at idle</summary>
		internal double ExhaustInitialRate;
		/// <summary>The rate of particle emissions at maximum power</summary>SmokeMaxMagnitude`
		internal double ExhaustMaxRate;

		private double GetMaxDieselCapacity(int carIndex)
		{
			if (dieselCapacity <= 0 && MaxFuelLevel > 0)
			{
				return MaxFuelLevel; // if zero capacity, try the figure from EngineVariables
			}

			if (dieselCapacity == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Diesel locomotive for car " + carIndex + " appears to have zero fuel capacity.");
			}
			return dieselCapacity;
		}

		private bool ParseBlock(Block block, string fileName, ref string wagonName, bool isEngine, ref CarBase car, ref TrainBase train)
		{
			Block newBlock;
			switch (block.Token)
			{
				case KujuTokenID.Wagon:
					string name = block.ReadString().Trim();
					if (isEngine)
					{
						// Within an Engine block, the Wagon block defines the visual wagon to display
						wagonName = name;
					}
					else
					{
						if (!name.Equals(wagonName, StringComparison.InvariantCultureIgnoreCase))
						{
							if (!wagonCache.ContainsKey(name))
							{
								// CHECK: How do MSTS / OR mediate between files with the same key
								wagonCache.Add(name, fileName);
							}
							return false;
						}
						while (block.Length() - block.Position() > 2)
						{
							try
							{
								newBlock = block.ReadSubBlock(true);
								ParseBlock(newBlock, fileName, ref wagonName, false, ref car, ref train);
							}
							catch
							{
								//ignore
							}
						}
					}
					break;
				case KujuTokenID.Engine:
					name = block.ReadString().Trim();
					if (!name.Equals(wagonName, StringComparison.InvariantCultureIgnoreCase))
					{
						if (!engineCache.ContainsKey(name))
						{
							// CHECK: How do MSTS / OR mediate between files with the same key
							engineCache.Add(name, fileName);
						}
						return false;
					}
					while (block.Length() - block.Position() > 2)
					{
						try
						{
							newBlock = block.ReadSubBlock(true);
							ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
						}
						catch
						{
							//ignore
						}
					}
					break;
				case KujuTokenID.Type:
					switch (block.ParentBlock.Token)
					{
						case KujuTokenID.Engine:
						case KujuTokenID.Wagon:
							if (isEngine)
							{
								currentEngineType = block.ReadEnumValue(default(EngineType));
							}
							else
							{
								try
								{
									currentWagonType = block.ReadEnumValue(default(WagonType));
								}
								catch
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Invalid vehicle type specified.");
								}
							}
							break;
						case KujuTokenID.Coupling:
							couplingType = block.ReadEnumValue(default(CouplingType));
							break;
					}
					break;
				case KujuTokenID.DieselEngineType:
					if (currentEngineType == EngineType.Diesel)
					{
						string type = block.ReadString();
						switch (type.ToLowerInvariant())
						{
							case "hydraulic":
								currentEngineType = EngineType.DieselHydraulic;
								break;
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Invalid vehicle type specified.");
					}
					break;
				case KujuTokenID.WagonShape:
					if(Plugin.PreviewOnly)
					{
						break;
					}
					// Loads exterior object
					string objectFile = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), block.ReadString());
					if (!File.Exists(objectFile))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle object file " + objectFile + " was not found");
						return true;
					}

					for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
					{
						if (Plugin.CurrentHost.Plugins[i].Object != null && Plugin.CurrentHost.Plugins[i].Object.CanLoadObject(objectFile))
						{
							Plugin.CurrentHost.Plugins[i].Object.LoadObject(objectFile, Path.GetDirectoryName(fileName), Encoding.Default, out UnifiedObject carObject);
							if (exteriorLoaded)
							{
								CarSection exteriorCarSection = car.CarSections[CarSectionType.Exterior];
								exteriorCarSection.AppendObject(Plugin.CurrentHost, Vector3.Zero, car, carObject);
								car.CarSections[CarSectionType.Exterior] = exteriorCarSection;
							}
							else
							{
								car.CarSections.Add(CarSectionType.Exterior, new CarSection(Plugin.CurrentHost, ObjectType.Dynamic, false, car, carObject));
							}
							break;
						}
					}

					exteriorLoaded = true;
					break;
				case KujuTokenID.Size:
					// Physical size of the car
					car.Width = block.ReadSingle(UnitOfLength.Meter);
					if (car.Width <= 0.1) // see for example LU1938TS - typo makes the car 2.26mm high
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle width is invalid.");
						car.Width = 2;
					}
					car.Height = block.ReadSingle(UnitOfLength.Meter);
					if (car.Height <= 0.1)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle height is invalid.");
						car.Height = 2;
					}
					car.Length = block.ReadSingle(UnitOfLength.Meter);
					if (car.Length <= 0.5)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle length is invalid.");
						car.Length = 25;
					}
					break;
				case KujuTokenID.Mass:
					// Sets the empty mass of the car
					car.EmptyMass = block.ReadSingle(UnitOfWeight.Kilograms);
					break;
				case KujuTokenID.BrakeEquipmentType:
					// Determines the brake equipment types available
					BrakeEquipmentType[] brakeEquipmentTypes = block.ReadEnumArray(default(BrakeEquipmentType));
					break;
				case KujuTokenID.BrakeSystemType:
					// Determines the brake system types available
					brakeSystemTypes = block.ReadEnumArray(default(BrakeSystemType));
					// WARNING: If vehicle only has vac brakes, default parameters for brake system are in/hg
					//          otherwise, default parameters are psi
					bool hasAirBrakes = false;
					for (int i = 0; i < brakeSystemTypes.Length; i++)
					{
						switch (brakeSystemTypes[i])
						{
							case BrakeSystemType.Air_single_pipe:
							case BrakeSystemType.Air_twin_pipe:
							case BrakeSystemType.EP:
							case BrakeSystemType.ECP:
								hasAirBrakes = true;
								break;
						}
					}

					if (!hasAirBrakes)
					{
						brakeSystemDefaultUnits = UnitOfPressure.InchesOfMercury;
					}
					break;
				case KujuTokenID.CabView:
					// Loads cab view file
					if (Plugin.PreviewOnly)
					{
						break;
					}
					string cabViewFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Path.GetDirectoryName(fileName), "CABVIEW"), block.ReadString());
					if (!File.Exists(cabViewFile))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Cab view file " + cabViewFile + " was not found");
						return true;
					}
					
					CabviewFileParser.ParseCabViewFile(cabViewFile, ref car);
					car.HasInteriorView = true;
					break;
				case KujuTokenID.Description:
					/*
					 * Only I believe valid in ENG files
					 * NOTE: For some reason, the array appears to be as lines, however it also contains the newline character
					 * Binary format??
					 */
					string[] strings = block.ReadStringArray();
					car.Description = string.Join("", strings).Replace(@"\n", Environment.NewLine);
					break;
				case KujuTokenID.Comment:
					if(car.Description == string.Empty)
					{
						// WAG files often have a comment block with a basic description
						strings = block.ReadStringArray();
						car.Description = string.Join("", strings);
					}
					break;
				case KujuTokenID.MaxPower:
					// maximum continuous power at the rails provided to the wheels
					break;
				case KujuTokenID.MaxForce:
					// maximum force applied when starting
					if (!isEngine)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: MaxForce is not expected to be present in a wagon block.");
						break;
					}
					maxForce = block.ReadSingle(UnitOfForce.Newton);
					break;
				case KujuTokenID.MaxVelocity:
					maxVelocity = block.ReadSingle(UnitOfVelocity.MetersPerSecond);
					break;
				case KujuTokenID.MaxBrakeForce:
					maxBrakeForce = block.ReadSingle(UnitOfForce.Newton);
					break;
				case KujuTokenID.MaxContinuousForce:
					// Maximum continuous force
					if (!isEngine)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: MaxContinuousForce is not expected to be present in a wagon block.");
						break;
					}
					maxContinuousForce = block.ReadSingle(UnitOfForce.Newton);
					break;
				case KujuTokenID.RunUpTimeToMaxForce:
					// 
					break;
				case KujuTokenID.WheelRadius:
					wheelRadius = block.ReadSingle(UnitOfLength.Meter);
					break;
				case KujuTokenID.NumWheels:
					int numWheels = block.ReadInt32();
					if (numWheels < 2)
					{
						// NumWheels *should* be divisible by two (to get axles), but some content uses a single wheel, e.g. stock Class 50
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Invalid number of wheels.");
						numWheels = 1;
					}
					else
					{
						numWheels /= 2;
					}

					if (block.ParentBlock.Token == KujuTokenID.Engine)
					{
						car.DrivingWheels.Add(new Wheels(numWheels == 1 ? 2 : numWheels, wheelRadius));
					}
					else
					{
						car.TrailingWheels.Add(new Wheels(numWheels == 1 ? 2 : numWheels, wheelRadius));
					}
					break;
				case KujuTokenID.Sound:
					// parse the sounds *after* we've loaded the traction model though
					string sF = block.ReadString();
					string soundFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Path.GetDirectoryName(fileName), "SOUND"), sF);
					if (!File.Exists(soundFile))
					{
						if (Directory.Exists(Plugin.FileSystem.MSTSDirectory))
						{
							// If sound file is not relative to the ENG / WAG, try in the MSTS common sound directory (most generic wagons + coaches)
							soundFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Plugin.FileSystem.MSTSDirectory, "SOUND"), sF);
						}
						if (!File.Exists(soundFile))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: SMS file " + soundFile + " was not found.");
						}
						break;
					}
					soundFiles.Add(soundFile);
					break;
				case KujuTokenID.EngineControllers:
					if (!isEngine)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: An EngineControllers block is not valid in a wagon block.");
						break;
					}

					while (block.Position() < block.Length() - 2)
					{
						// large number of potential controls when including diesel + steam, so allow *any* block here
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, fileName, ref wagonName, true, ref car, ref train);
					}
					break;
				case KujuTokenID.Throttle:
					if (EngineAlreadyParsed)
					{
						// NOTE: Per-car handles not yet supported, so take the handles from the first engine in the train
						// otherwise, if we've got a VariableHandle (0-100) followed by notched, we'll be stuck at basically zero power
						break;
					}
					if (currentEngineType == EngineType.Steam)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: A throttle is not valid for a Steam Locomotive.");
						break;
					}
					train.Handles.Power = ParseHandle(block, train, true);
					break;
				case KujuTokenID.Brake_Train:
					if (EngineAlreadyParsed)
					{
						break;
					}
					train.Handles.Brake = ParseHandle(block, train, false);
					break;
				case KujuTokenID.Brake_Engine:
					if (EngineAlreadyParsed)
					{
						break;
					}
					train.Handles.HasLocoBrake = true;
					train.Handles.LocoBrake = ParseHandle(block, train, false);
					break;
				case KujuTokenID.Combined_Control:
					if (EngineAlreadyParsed)
					{
						break;
					}
					block.ReadSingle();
					block.ReadSingle();
					block.ReadSingle();
					block.ReadSingle();

					CombinedControlType firstCombinedControl = block.ReadEnumValue(default(CombinedControlType));
					CombinedControlType secondCombinedControl = block.ReadEnumValue(default(CombinedControlType));

					if (firstCombinedControl == CombinedControlType.Throttle && secondCombinedControl == CombinedControlType.Dynamic)
					{
						train.Handles.HandleType = HandleType.SingleHandle;
					}
					break;
				case KujuTokenID.Sanding:
					switch (block.ParentBlock.Token)
					{
						case KujuTokenID.Engine:
							maxSandingSpeed = block.ReadSingle(UnitOfVelocity.MetersPerSecond, UnitOfVelocity.MilesPerHour);
							break;
						case KujuTokenID.EngineControllers:
							int p1 = block.ReadInt16();
							int p2 = block.ReadInt16();
							int p3 = block.ReadInt16();
							if (p1 == 0 && p2 == 1 && p3 == 0)
							{
								car.ReAdhesionDevice = new Sanders(car, SandersType.PressAndHold, maxSandingSpeed);
							}
							break;
					}
					break;
				case KujuTokenID.DieselEngineSpeedOfMaxTractiveEffort:
					dieselMaxTractiveEffortSpeed = block.ReadSingle(UnitOfVelocity.MetersPerSecond);
					break;
				case KujuTokenID.DieselEngineIdleRPM:
					dieselIdleRPM = block.ReadSingle();
					break;
				case KujuTokenID.DieselEngineMaxRPM:
					dieselMaxRPM = block.ReadSingle();
					break;
				case KujuTokenID.DieselEngineMaxRPMChangeRate:
					dieselRPMChangeRate = block.ReadSingle();
					break;
				case KujuTokenID.DieselUsedPerHourAtIdle:
					dieselIdleUse = block.ReadSingle(UnitOfVolume.Litres);
					dieselIdleUse /= 3600;
					break;
				case KujuTokenID.DieselUsedPerHourAtMaxPower:
					dieselMaxUse = block.ReadSingle(UnitOfVolume.Litres);
					dieselMaxUse /= 3600;
					break;
				case KujuTokenID.MaxDieselLevel:
					dieselCapacity = block.ReadSingle(UnitOfVolume.Litres);
					break;
				case KujuTokenID.MaxCurrent:
					maxEngineAmps = block.ReadSingle(UnitOfCurrent.Amps);
					break;
				case KujuTokenID.DynamicBrakesResistorCurrentLimit:
					maxBrakeAmps = block.ReadSingle(UnitOfCurrent.Amps);
					break;
				case KujuTokenID.AirBrakesMainMinResAirPressure:
					mainReservoirMinimumPressure = block.ReadSingle(UnitOfPressure.Pascal, UnitOfPressure.PoundsPerSquareInch);
					break;
				case KujuTokenID.AirBrakesMainMaxAirPressure:
					mainReservoirMaximumPressure = block.ReadSingle(UnitOfPressure.Pascal, UnitOfPressure.PoundsPerSquareInch);
					break;
				case KujuTokenID.BrakeCylinderPressureForMaxBrakeBrakeForce:
					brakeCylinderMaximumPressure = block.ReadSingle(UnitOfPressure.Pascal, brakeSystemDefaultUnits);
					break;
				case KujuTokenID.TrainBrakesControllerEmergencyApplicationRate:
					emergencyRate = block.ReadSingle(UnitOfPressure.Pascal, brakeSystemDefaultUnits);
					break;
				case KujuTokenID.AirBrakesAirCompressorPowerRating:
					compressionRate = block.ReadSingle(UnitOfPressure.Pascal, UnitOfPressure.PoundsPerSquareInch);
					if (compressionRate < 3500 || compressionRate > 34475) // assume valid range to be 0.5psi/s to 5psi/s
					{
						compressionRate = 3500;
					}
					break;
				case KujuTokenID.TrainBrakesControllerMaxReleaseRate:
					releaseRate = block.ReadSingle(UnitOfPressure.Pascal, brakeSystemDefaultUnits);
					break;
				case KujuTokenID.AntiSlip:
					// if any value in this block, car has wheelslip detection control
					hasAntiSlipDevice = true;
					break;
				case KujuTokenID.AWSMonitor:
				case KujuTokenID.EmergencyStopMonitor:
				case KujuTokenID.OverspeedMonitor:
				case KujuTokenID.VigilanceMonitor:
					// MSTS safety systems
					VigilanceDevice device = VigilanceDevice.CreateVigilanceDevice(block.Token);
					while (block.Position() < block.Length() - 2)
					{
						newBlock = block.ReadSubBlock(true);
						device.ParseBlock(newBlock);
					}
					device.Create(car);
					vigilanceDevices.Add(device);
					break;
				case KujuTokenID.FreightAnim:
					if (Plugin.PreviewOnly)
					{
						break;
					}
					objectFile = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), block.ReadString());
					
					if (!File.Exists(objectFile))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: FreightAnim object file " + objectFile + " was not found");
						break;
					}

					/*
					 *
					 * https://tsforum.forumotion.net/t655-freight-animation
					 * FreightAnim are a total mess...
					 * Whilst it appears they were intended to work with all cars, the positioning logic only works correctly
					 * with tenders.
					 *
					 * TENDERS:
					 * --------
					 * First value: Starting Y position (full)
					 * Second value: Ending Y position (empty)
					 * Third value: Must be positive or omitted for animation to work. If negative, animation stays at full.
					 *
					 * OTHER CARS:
					 * ----------
					 * First value: Ignored
					 * Second value: Must be any positive number.
					 *
					 * However, this is actually done by directly replacing the Y translation component of Matrix[0] within the shape
					 * This means that if our shape actually has a value here, things can get really messy.
					 *
					 * The UKTS RCH wagon loads contain a Y value of approx 2.53, which when loaded in this way actually gets discarded
					 * 
					 */

					double loadPosition = 0;
					double emptyPosition = 0;
					try
					{
						loadPosition = block.ReadSingle();
						emptyPosition = block.ReadSingle();
						// may also be one more number, but this appears unused
					}
					catch
					{
						// ignore
					}

					if (isEngine == false && currentWagonType != WagonType.Tender)
					{
						if (emptyPosition == 0)
						{
							break;
						}
						loadPosition = 0;
					}
					
					for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
					{
						if (Plugin.CurrentHost.Plugins[i].Object == null || !Plugin.CurrentHost.Plugins[i].Object.CanLoadObject(objectFile))
						{
							continue;
						}
						Plugin.CurrentHost.Plugins[i].Object.LoadObject(objectFile, Path.GetDirectoryName(fileName), Encoding.Default, out UnifiedObject freightObject);
						if (exteriorLoaded)
						{
								
							CarSection exteriorCarSection = car.CarSections[CarSectionType.Exterior];
							exteriorCarSection.AppendObject(Plugin.CurrentHost, new Vector3(0, loadPosition, 0), car, freightObject);
							car.CarSections[CarSectionType.Exterior] = exteriorCarSection;
						}
						else
						{
							car.CarSections.Add(CarSectionType.Exterior, new CarSection(Plugin.CurrentHost, ObjectType.Dynamic, false, car, freightObject));
						}
						break;
					}
					break;
				case KujuTokenID.Effects:
					while (block.Position() < block.Length() - 2)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
					}
					break;
				case KujuTokenID.SteamSpecialEffects:
				case KujuTokenID.DieselSpecialEffects:
					while (block.Position() < block.Length() - 2)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
					}
					break;
				case KujuTokenID.StackFX: // steam loco
				case KujuTokenID.Exhaust1: // diesel loco
				case KujuTokenID.Exhaust2:
				case KujuTokenID.Exhaust3:
				case KujuTokenID.WhistleFX:
				case KujuTokenID.CylindersFX:
					ParticleSource particleSource = new ParticleSource(block.Token);
					particleSource.Offset = new Vector3(block.ReadSingle(), block.ReadSingle(), block.ReadSingle());
					particleSource.Direction = new Vector3(block.ReadSingle(), block.ReadSingle(), block.ReadSingle());
					particleSource.Size = block.ReadSingle();
					ParticleSources.Add(particleSource);
					break;
				case KujuTokenID.DieselSmokeEffectMaxMagnitude:
					ExhaustMaxMagnitude = block.ReadSingle();
					break;
				case KujuTokenID.DieselSmokeEffectInitialSmokeRate:
					ExhaustInitialRate = block.ReadSingle();
					break;
				case KujuTokenID.DieselSmokeEffectMaxSmokeRate:
					ExhaustMaxRate = block.ReadSingle();
					break;
				case KujuTokenID.GearBoxNumberOfGears:
					int numGears = block.ReadInt16();
					Gears = new Gear[numGears];
					break;
				case KujuTokenID.GearBoxMaxSpeedForGears:
					if (Gears == null)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS Vehicle Parser: Gears must be specified when using GearBoxMaxSpeedForGears.");
						break;
					}

					for (int i = 0; i < Gears.Length; i++)
					{
						Gears[i].MaximumSpeed = block.ReadSingle(UnitOfVelocity.MetersPerSecond, UnitOfVelocity.MilesPerHour);
					}
					break;
				case KujuTokenID.GearBoxMaxTractiveForceForGears:
					if (Gears == null)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS Vehicle Parser: Gears must be specified when using GearBoxMaxTractiveForceForGears.");
						break;
					}

					for (int i = 0; i < Gears.Length; i++)
					{
						Gears[i].MaxTractiveForce = block.ReadSingle(UnitOfForce.Newton);
					}
					break;
				case KujuTokenID.GearBoxOverspeedPercentageForFailure:
					if (Gears == null)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS Vehicle Parser: Gears must be specified when using GearBoxOverspeedPercentageForFailure.");
						break;
					}

					double perc = block.ReadSingle() / 100;
					for (int i = 0; i < Gears.Length; i++)
					{
						Gears[i].OverspeedFailure = Gears[i].MaximumSpeed * perc;
					}
					break;
				case KujuTokenID.GearBoxOperation:
					gearboxOperationMode = block.ReadEnumValue(default(GearboxOperation));
					break;
				case KujuTokenID.Friction:
					friction = new Friction(block);
					break;
				case KujuTokenID.Adheasion:
					adhesion = new Adhesion(block, car, currentEngineType == EngineType.Steam);
					break;
				case KujuTokenID.Coupling:
					while (block.Position() < block.Length() - 2)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
					}
					break;
				case KujuTokenID.Spring:
					while (block.Position() < block.Length() - 2)
					{
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
					}
					break;
				case KujuTokenID.r0:
					try
					{
						car.Coupler.MinimumDistanceBetweenCars = block.ReadSingle(UnitOfLength.Meter);
						car.Coupler.MaximumDistanceBetweenCars = couplingType != CouplingType.Bar ? block.ReadSingle(UnitOfLength.Meter) : car.Coupler.MinimumDistanceBetweenCars;
					}
					catch
					{
						// ignored
					}

					if (car.Coupler.MaximumDistanceBetweenCars > 2)
					{
						// some automatic / bar couplers seem to have absurd maximum distances
						// so let's assume they're no good
						car.Coupler.MaximumDistanceBetweenCars = car.Coupler.MinimumDistanceBetweenCars;
					}
					break;
				case KujuTokenID.IsTenderRequired:
					RequiresTender = block.ReadBool();
					break;
				case KujuTokenID.EngineVariables:
					switch (currentEngineType)
					{
						case EngineType.DieselHydraulic:
						case EngineType.Diesel:
							// Fuel capacity (L)
							// NOTE: Max fuel is set by MaxDieselLevel
							MaxFuelLevel = block.ReadSingle(UnitOfVolume.Litres);
							break;
						case EngineType.Steam:
							// https://tsforum.forumotion.net/t120-msts-helpful-facts-and-links-part-14-enginevariables-for-steam-locomotives-by-slipperman12
							// Fire temp (deg c)
							// Fire mass (lbs)
							// Water mass in boiler (lbs)
							// Boiler pressure (psi)
							// Tender water mass (If switched to in ACT mode, so ignore)
							// Tender coal mass (If switched to in ACT mode, so ignore)
							// Smoke quantity multiplier
							// Fire condition
							// Coal quality
							// NOTE: Max fuel / water levels are set by MaxTenderCoalMass and MaxTenderWaterMass
							break;
						case EngineType.Electric:
							// Not present
							break;
					}
					break;
				case KujuTokenID.MaxTenderCoalMass:
					MaxFuelLevel = block.ReadSingle(UnitOfWeight.Kilograms, UnitOfWeight.Pounds);
					break;
				case KujuTokenID.MaxTenderWaterMass:
					// at atmospheric pressure, 1kg of water = 1L
					MaxWaterLevel = block.ReadSingle(UnitOfWeight.Kilograms, UnitOfWeight.Pounds);
					break;
			}
			return true;
		}
	}
}
