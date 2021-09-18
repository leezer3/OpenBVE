using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibRender2.Trains;
using OpenBve.Formats.MsTs;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.World;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using TrainManager.BrakeSystems;
using TrainManager.Car;
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

		internal WagonParser(Plugin Plugin)
		{
			plugin = Plugin;
			wagonCache = new Dictionary<string, string>();
			engineCache = new Dictionary<string, string>();
		}

		internal void Parse(string trainSetDirectory, string wagonName, bool isEngine, ref CarBase Car, ref TrainBase train)
		{
			wagonFiles = Directory.GetFiles(trainSetDirectory, isEngine ? "*.eng" : "*.wag", SearchOption.AllDirectories);
			Car.Specs.IsMotorCar = false;
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
				Car.Specs.IsMotorCar = true;
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
					string s;
					if (unicode)
					{
						s = Encoding.Unicode.GetString(newBytes);
					}
					else
					{
						s = Encoding.ASCII.GetString(newBytes);
					}

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
					break;
				case KujuTokenID.Type:
					string wagonType = block.ReadString().ToLowerInvariant();
					if (isEngine)
					{
						//Will load engine type
					}
					else
					{
						if (wagonType != "wagon" && wagonType != "carriage" && wagonType != "engine" && wagonType != "freight")
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vechicle Parser: Expected a Carriage or Wagon, however " + wagonType + " was found.");
						}
					}
					break;
				case KujuTokenID.WagonShape:
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
							UnifiedObject carObject;
							Plugin.currentHost.Plugins[i].Object.LoadObject(objectFile, Encoding.Default, out carObject);
							car.LoadCarSections(carObject, false);
							break;
						}
					}
					break;
				case KujuTokenID.Size:
					// Physical size of the car
					car.Width = block.ReadSingle(UnitOfLength.Meter);
					car.Height = block.ReadSingle(UnitOfLength.Meter);
					car.Length = block.ReadSingle(UnitOfLength.Meter);
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
					BrakeSystemType[] brakeSystemTypes = block.ReadEnumArray(default(BrakeSystemType));
					if (brakeSystemTypes.Contains(BrakeSystemType.Vacuum_piped) || brakeSystemTypes.Contains(BrakeSystemType.Air_piped))
					{
						/*
						 * FIXME: Need to implement vac braked / air piped and vice-versa, but for the minute, we'll assume that if one or the other is present
						 * then the vehicle has no brakes
						 */ 
						car.CarBrake = new ThroughPiped();
					}
					else
					{
						if (brakeSystemTypes.Contains(BrakeSystemType.EP))
						{
							// Combined air brakes and control signals
							// Assume equivilant to ElectromagneticStraightAirBrake
							car.CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.DelayFillingControl, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0, 0, new AccelerationCurve[] { });
						}
						else if (brakeSystemTypes.Contains(BrakeSystemType.ECP))
						{
							// Complex computer control
							// Assume equivialant to ElectricCommandBrake at the minute
							car.CarBrake = new ElectricCommandBrake(EletropneumaticBrakeType.DelayFillingControl, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0, 0, new AccelerationCurve[] { });
						}
						else if (brakeSystemTypes.Contains(BrakeSystemType.Air_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Air_twin_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_single_pipe) || brakeSystemTypes.Contains(BrakeSystemType.Vacuum_twin_pipe))
						{
							// The car contains no control gear, but is air / vac braked
							// Assume equivilant to AutomaticAirBrake
							// NOTE: This must be last in the else-if chain to enure that a vehicle with EP / ECP and these declared is setup correctly
							car.CarBrake = new AutomaticAirBrake(EletropneumaticBrakeType.DelayFillingControl, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0, 0, new AccelerationCurve[] { });
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
					break;
				case KujuTokenID.CabView:
					// Loads cab view file
					string cabViewFile = OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(Path.GetDirectoryName(fileName), "CABVIEW"), block.ReadString());
					if (!File.Exists(cabViewFile))
					{
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: Cab view file " + cabViewFile + " was not found");
						return true;
					}

					if (car.CarSections.Length > 0)
					{
						// Cab View must always be at CarSection zero, but the order is not guaranteed within an eng / wag
						CarSection[] move = new CarSection[car.CarSections.Length + 1];
						for (int i = 0; i < car.CarSections.Length; i++)
						{
							move[i + 1] = car.CarSections[i];
						}
						car.CarSections = move;
					}
					CabviewFileParser.ParseCabViewFile(cabViewFile, Encoding.ASCII, ref car);
					car.HasInteriorView = true;
					break;
			}
			return true;
		}
	}
}
