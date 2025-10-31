using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibRender2.Trains;
using OpenBve.Formats.Msts;
using OpenBve.Formats.MsTs;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal class WagonParser
	{
		private readonly Plugin plugin;

		private readonly Dictionary<string, string> wagonCache;
		private readonly Dictionary<string, string> engineCache;
		private string[] wagonFiles;
		private int wheelRadiusNum;
		private double wheelRadius;

		internal WagonParser(Plugin Plugin)
		{
			plugin = Plugin;
			wagonCache = new Dictionary<string, string>();
			engineCache = new Dictionary<string, string>();
		}

		internal void Parse(string trainSetDirectory, string wagonName, bool isEngine, ref CarBase Car, ref TrainBase train)
		{
			wheelRadiusNum = 1;
			wagonFiles = Directory.GetFiles(trainSetDirectory, isEngine ? "*.eng" : "*.wag", SearchOption.AllDirectories);
			currentEngineType = EngineType.NoEngine;
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
				if (engineCache.ContainsKey(wagonName))
				{
					ReadWagonData(engineCache[wagonName], ref wagonName, true, ref Car, ref train);
				}
				else
				{
					for (int i = 0; i < wagonFiles.Length; i++)
					{
						if (ReadWagonData(wagonFiles[i], ref wagonName, true, ref Car, ref train))
						{
							break;
						}
					}
				}
			}
			/*
			 * We've now found the engine properties-
			 * Now, we need to read the wagon properties to find the visual wagon to display
			 * (The Engine only holds the physics data)
			 */
			if (wagonCache.ContainsKey(wagonName))
			{
				ReadWagonData(wagonCache[wagonName], ref wagonName, false, ref Car, ref train);
			}
			else
			{
				for (int i = 0; i < wagonFiles.Length; i++)
				{
					if (ReadWagonData(wagonFiles[i], ref wagonName, false, ref Car, ref train))
					{
						break;
					}
				}	
			}

			Car.Specs.AccelerationCurveMaximum = maxForce / Car.CurrentMass;
			// as properties may not be in order, set this stuff last
			if (isEngine)
			{
				Car.Specs.AccelerationCurves = new AccelerationCurve[]
				{
					new MSTSAccelerationCurve(Car, maxForce, maxContinuousForce, maxVelocity)
				};
				// FIXME: Default BVE values
				Car.Specs.JerkPowerUp = 10.0;
				Car.Specs.JerkPowerDown = 10.0;
				Car.ReAdhesionDevice = new BveReAdhesionDevice(Car, hasAntiSlipDevice ? ReadhesionDeviceType.TypeB : ReadhesionDeviceType.NotFitted);
				switch (currentEngineType)
				{
					case EngineType.Diesel:
						Car.Engine = new DieselEngine(Car, dieselIdleRPM, dieselIdleRPM, dieselMaxRPM, dieselRPMChangeRate, dieselRPMChangeRate, dieselIdleUse, dieselMaxUse);
						Car.Engine.FuelTank = new FuelTank(dieselCapacity, 0, dieselCapacity);
						Car.Engine.IsRunning = true;

						if (maxBrakeAmps > 0 && maxEngineAmps > 0)
						{
							Car.Engine.Components.Add(EngineComponent.RegenerativeTractionMotor, new RegenerativeTractionMotor(Car.Engine, maxEngineAmps, maxBrakeAmps));
						}
						else if (maxEngineAmps > 0)
						{
							Car.Engine.Components.Add(EngineComponent.TractionMotor, new TractionMotor(Car.Engine, maxEngineAmps));
						}
						break;
					case EngineType.Electric:
						Car.Engine = new ElectricEngine(Car);
						Car.Engine.Components.Add(EngineComponent.Pantograph, new Pantograph(Car.Engine));
						break;
				}
			}
		}

		internal bool ReadWagonData(string fileName, ref string wagonName, bool isEngine, ref CarBase car, ref TrainBase train)
		{
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = (buffer[0] == 0xFF && buffer[1] == 0xFE);

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
				throw new Exception("Unrecognized vehicle file header " + headerString + " in " + fileName);
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
					 * Assume that these can be of arbritrary order, so read using a dictionary
					 */
					Dictionary<KujuTokenID, Block> blocks = TextualBlock.ReadBlocks(s);
					if (!blocks.ContainsKey(KujuTokenID.Wagon))
					{
						//Not found any wagon data in this file
						return false;
					}
					if (isEngine && blocks.ContainsKey(KujuTokenID.Engine))
					{
						return ParseBlock(blocks[KujuTokenID.Engine], fileName, ref wagonName, true, ref car, ref train);
					}
					if (!isEngine && blocks.ContainsKey(KujuTokenID.Wagon))
					{
						return ParseBlock(blocks[KujuTokenID.Wagon], fileName, ref wagonName, false, ref car, ref train);
					}
					return false;
				}
					
			}
			if (subHeader[7] != 'b')
			{
				throw new Exception("Unrecognized subHeader \"" + subHeader + "\" in " + fileName);
			}
			else
			{
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
			}
			return true;
		}

		private double maxForce = 0;
		private double maxContinuousForce = 0;
		private double maxBrakeForce = 0;
		private BrakeSystemType[] brakeSystemTypes;
		private EngineType currentEngineType;
		private double dieselIdleRPM;
		private double dieselMaxRPM;
		private double dieselRPMChangeRate;
		private double dieselIdleUse;
		private double dieselMaxUse;
		private double dieselCapacity;
		private double maxEngineAmps;
		private double maxBrakeAmps;
		private double mainReservoirMinimumPressure;
		private double mainReservoirMaximumPressure;
		private double brakeCylinderMaximumPressure;
		private double emergencyRate;
		private double releaseRate;
		private double maxVelocity;
		private bool hasAntiSlipDevice;


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
						while (block.Length() - block.Position() > 1)
						{
							try
							{
								newBlock = block.ReadSubBlock();
								ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
							}
							catch
							{
								//ignore
							}
						}
						if (brakeSystemTypes == null)
						{
							break;
						}
						// Add brakes last, as we need the acceleration values
						if (brakeSystemTypes.Contains(BrakeSystemType.Vacuum_Piped) || brakeSystemTypes.Contains(BrakeSystemType.Air_Piped))
						{
							/*
							 * FIXME: Need to implement vac braked / air piped and vice-versa, but for the minute, we'll assume that if one or the other is present
							 * then the vehicle has no brakes
							 */
							car.CarBrake = new ThroughPiped(car);
						}
						else
						{
							if (brakeSystemTypes.Contains(BrakeSystemType.EP))
							{
								// Combined air brakes and control signals
								// Assume equivilant to ElectromagneticStraightAirBrake
								car.CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.DelayFillingControl, car);
							}
							else if (brakeSystemTypes.Contains(BrakeSystemType.ECP))
							{
								// Complex computer control
								// Assume equivialant to ElectricCommandBrake at the minute
								car.CarBrake = new ElectricCommandBrake(EletropneumaticBrakeType.DelayFillingControl, car, 0, 0, 0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxForce) });
							}
							else if (brakeSystemTypes.Contains(BrakeSystemType.Air_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Air_twin_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_twin_pipe))
							{
								// The car contains no control gear, but is air / vac braked
								// Assume equivilant to AutomaticAirBrake
								// NOTE: This must be last in the else-if chain to enure that a vehicle with EP / ECP and these declared is setup correctly
								car.CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.DelayFillingControl, car, 0,0,0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxForce) });
							}

							car.CarBrake.mainReservoir = new MainReservoir(690000.0, 780000.0, 0.01, 0.075 / train.Cars.Length);
							car.CarBrake.airCompressor = new Compressor(5000.0, car.CarBrake.mainReservoir, car);
							car.CarBrake.equalizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0);
							car.CarBrake.equalizingReservoir.NormalPressure = 1.005 * 490000.0;
							double r = 200000.0 / 440000.0 - 1.0;
							if (r < 0.1) r = 0.1;
							if (r > 1.0) r = 1.0;
							car.CarBrake.auxiliaryReservoir = new AuxiliaryReservoir(0.975 * 490000.0, 200000.0, 0.5, r);
							car.CarBrake.brakeCylinder = new BrakeCylinder(440000.0, 440000.0, 0.3 * 300000.0, 300000.0, 200000.0);
							car.CarBrake.straightAirPipe = new StraightAirPipe(300000.0, 400000.0, 200000.0);

						}

						car.CarBrake.brakePipe = new BrakePipe(490000.0, 10000000.0, 1500000.0, 5000000.0, true);
						car.CarBrake.JerkUp = 10;
						car.CarBrake.JerkDown = 10;
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
					while (block.Length() - block.Position() > 1)
					{
						try
						{
							newBlock = block.ReadSubBlock();
							ParseBlock(newBlock, fileName, ref wagonName, isEngine, ref car, ref train);
						}
						catch
						{
							//ignore
						}
					}

					if (brakeSystemTypes == null)
					{
						break;
					}
					// Add brakes last, as we need the acceleration values
					if (brakeSystemTypes.Contains(BrakeSystemType.Vacuum_Piped) || brakeSystemTypes.Contains(BrakeSystemType.Air_Piped))
					{
						/*
						 * FIXME: Need to implement vac braked / air piped and vice-versa, but for the minute, we'll assume that if one or the other is present
						 * then the vehicle has no brakes
						 */
						car.CarBrake = new ThroughPiped(car);
					}
					else
					{
						if (brakeSystemTypes.Contains(BrakeSystemType.EP))
						{
							// Combined air brakes and control signals
							// Assume equivilant to ElectromagneticStraightAirBrake
							car.CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.DelayFillingControl, car, 0, 0, 0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxBrakeForce == 0 ? maxForce : maxBrakeForce) });
						}
						else if (brakeSystemTypes.Contains(BrakeSystemType.ECP))
						{
							// Complex computer control
							// Assume equivialant to ElectricCommandBrake at the minute
							car.CarBrake = new ElectricCommandBrake(EletropneumaticBrakeType.DelayFillingControl, car, 0, 0, 0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxBrakeForce == 0 ? maxForce : maxBrakeForce) });
						}
						else if (brakeSystemTypes.Contains(BrakeSystemType.Air_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Air_twin_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_twin_pipe))
						{
							// The car contains no control gear, but is air / vac braked
							// Assume equivilant to AutomaticAirBrake
							// NOTE: This must be last in the else-if chain to enure that a vehicle with EP / ECP and these declared is setup correctly
							car.CarBrake = new AutomaticAirBrake(EletropneumaticBrakeType.DelayFillingControl, car, 0, 0, new AccelerationCurve[] { new MSTSDecelerationCurve(train, maxBrakeForce == 0 ? maxForce : maxBrakeForce) });
						}

						car.CarBrake.mainReservoir = new MainReservoir(mainReservoirMinimumPressure, mainReservoirMaximumPressure, 0.01, 0.075 / train.Cars.Length);
						car.CarBrake.airCompressor = new Compressor(5000.0, car.CarBrake.mainReservoir, car);
						car.CarBrake.equalizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0);
						car.CarBrake.equalizingReservoir.NormalPressure = 1.005 * brakeCylinderMaximumPressure;
						double r = 200000.0 / 440000.0 - 1.0;
						if (r < 0.1) r = 0.1;
						if (r > 1.0) r = 1.0;
						car.CarBrake.auxiliaryReservoir = new AuxiliaryReservoir(0.975 * brakeCylinderMaximumPressure, 200000.0, 0.5, r);
						car.CarBrake.brakeCylinder = new BrakeCylinder(brakeCylinderMaximumPressure, brakeCylinderMaximumPressure, 0.3 * 300000.0, 300000.0, 200000.0);
						car.CarBrake.straightAirPipe = new StraightAirPipe(300000.0, 400000.0, 200000.0);

					}

					car.CarBrake.brakePipe = new BrakePipe(brakeCylinderMaximumPressure, 10000000.0, 1500000.0, 5000000.0, true);
					car.CarBrake.JerkUp = 10;
					car.CarBrake.JerkDown = 10;
					break;
				case KujuTokenID.Type:
					if (isEngine)
					{
						currentEngineType = block.ReadEnumValue(default(EngineType));
					}
					else
					{
						try
						{
							WagonType type = block.ReadEnumValue(default(WagonType));
						}
						catch
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vechicle Parser: Invalid vehicle type specified.");
						}
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
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle object file " + objectFile + " was not found");
						return true;
					}

					for (int i = 0; i < Plugin.currentHost.Plugins.Length; i++)
					{
						
						if (Plugin.currentHost.Plugins[i].Object != null && Plugin.currentHost.Plugins[i].Object.CanLoadObject(objectFile))
						{
							Plugin.currentHost.Plugins[i].Object.LoadObject(objectFile, Encoding.Default, out UnifiedObject carObject);
							car.LoadCarSections(carObject, false);
							break;
						}
					}
					break;
				case KujuTokenID.Size:
					// Physical size of the car
					car.Width = block.ReadSingle(UnitOfLength.Meter);
					if (car.Width == 0)
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle width is invalid.");
						car.Width = 2;
					}
					car.Height = block.ReadSingle(UnitOfLength.Meter);
					if (car.Height == 0)
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle height is invalid.");
						car.Height = 2;
					}
					car.Length = block.ReadSingle(UnitOfLength.Meter);
					if (car.Length == 0)
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Vehicle length is invalid.");
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
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Cab view file " + cabViewFile + " was not found");
						return true;
					}

					if (car.CarSections.Length == 0)
					{
						car.CarSections = new CarSection[1];
					}
					else if (car.CarSections.Length > 0)
					{
						// Cab View must always be at CarSection zero, but the order is not guaranteed within an eng / wag
						CarSection[] move = new CarSection[car.CarSections.Length + 1];
						for (int i = 0; i < car.CarSections.Length; i++)
						{
							move[i + 1] = car.CarSections[i];
						}
						car.CarSections = move;
					}
					car.CarSections[0] = new CarSection(Plugin.currentHost, ObjectType.Overlay, true, car);
					car.CameraRestrictionMode = CameraRestrictionMode.On;
					Plugin.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
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
					// maximum continous power at the rails provided to the wheels
					break;
				case KujuTokenID.MaxForce:
					// maximum force applied when starting
					if (!isEngine)
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Engine force is not expected to be present in a wagon block.");
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
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Engine force is not expected to be present in a wagon block.");
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
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Invalid number of wheels.");
						numWheels = 1;
					}
					else
					{
						numWheels /= 2;
					}

					if (numWheels == 1)
					{
						car.Wheels.Add("WHEELS" + wheelRadiusNum, new Wheels(2, "WHEELS" + wheelRadiusNum, wheelRadius));
					}
					else
					{
						car.Wheels.Add("WHEELS" + wheelRadiusNum, new Wheels(2, "WHEELS" + wheelRadiusNum, wheelRadius));
						for (int i = 1; i < numWheels + 1; i++)
						{
							car.Wheels.Add("WHEELS" + wheelRadiusNum + i, new Wheels(2, "WHEELS1" + wheelRadiusNum + i, wheelRadius));
						}
					}
					wheelRadiusNum++;
					break;
				case KujuTokenID.Sound:
					string soundFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Path.GetDirectoryName(fileName), "SOUND"), block.ReadString());
					if (File.Exists(soundFile))
					{
						SoundModelSystemParser.ParseSoundFile(soundFile, ref car);
					}
					break;
				case KujuTokenID.EngineControllers:
					if (!isEngine)
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Engine controllers are not expected to be present in a wagon block.");
						break;
					}

					while (block.Position() < block.Length() - 2)
					{
						// large number of potential controls when including diesel + steam, so allow *any* block here
						newBlock = block.ReadSubBlock();
						ParseBlock(newBlock, fileName, ref wagonName, true, ref car, ref train);
					}
					break;
				case KujuTokenID.Throttle:
				case KujuTokenID.Brake_Train:
					// NOTE: Throttle is valid for DIESEL + ELECTRIC only
					block.ReadSingle(); // minimum
					block.ReadSingle(); // maxiumum
					block.ReadSingle(); // power step per notch
					block.ReadSingle(); // default value (at start of simulation presumably)
					newBlock = block.ReadSubBlock(KujuTokenID.NumNotches);
					ParseBlock(newBlock, fileName, ref wagonName, true, ref car, ref train);
					break;
				case KujuTokenID.NumNotches:
					// n.b. totalNotches value includes zero in MSTS
					int totalNotches = block.ReadInt16();
					for (int i = 0; i < totalNotches; i++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.Notch);
						ParseBlock(newBlock, fileName, ref wagonName, true, ref car, ref train);
					}
					switch (block.ParentBlock.Token)
					{
						case KujuTokenID.Throttle:
							train.Handles.Power = new PowerHandle(totalNotches - 1, train);
							break;
						case KujuTokenID.Brake_Train:
							train.Handles.Brake = new BrakeHandle(totalNotches - 1, totalNotches - 1, train.Handles.EmergencyBrake, new double[] { }, new double[] { }, train);
							break;
					}
					break;
				case KujuTokenID.Notch:
					double powerValue = block.ReadSingle();
					double graduationValue = block.ReadSingle();
					string notchToken = block.ReadString();
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
					brakeCylinderMaximumPressure = block.ReadSingle(UnitOfPressure.Pascal, UnitOfPressure.PoundsPerSquareInch);
					break;
				case KujuTokenID.TrainBrakesControllerEmergencyApplicationRate:
					emergencyRate = block.ReadSingle(UnitOfPressure.Pascal, UnitOfPressure.PoundsPerSquareInch);
					break;
				case KujuTokenID.TrainBrakesControllerMaxReleaseRate:
					releaseRate = block.ReadSingle(UnitOfPressure.Pascal, UnitOfPressure.PoundsPerSquareInch);
					break;
				case KujuTokenID.AntiSlip:
					// if any value in this block, car has wheelslip detection control
					hasAntiSlipDevice = true;
					break;
			}
			return true;
		}
	}
}
