using System;
using System.IO;
using System.Text;
using OpenBve.Formats.MsTs;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal class ConsistParser
	{
		internal readonly Plugin plugin;

		internal string currentFolder;

		internal ConsistParser(Plugin Plugin)
		{
			plugin = Plugin;
		}

		internal void ReadConsist(string fileName, ref AbstractTrain Train)
		{
			currentCarIndex = -1;
			TrainBase train = Train as TrainBase;
			train.Handles.Reverser = new ReverserHandle(train);
			train.Handles.EmergencyBrake = new EmergencyHandle(train);
			train.Handles.Power = new PowerHandle(8, 8, new double[] { }, new double[] { }, train);
			train.Handles.Brake = new BrakeHandle(8, 8, train.Handles.EmergencyBrake, new double[] { }, new double[] { }, train);
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			currentFolder = Path.GetDirectoryName(fileName);
			DirectoryInfo d = Directory.GetParent(currentFolder);
			if(!d.Name.Equals("TRAINS", StringComparison.InvariantCultureIgnoreCase))
			{
				//FIXME: Better finding of the trainset folder (set in options?)
				throw new Exception("Unable to find the TRAINS folder");
			}

			currentFolder = d.FullName;

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
				throw new Exception("Unrecognized shape file header " + headerString + " in " + fileName);
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
					TextualBlock block = new TextualBlock(s, KujuTokenID.Train);
					ParseBlock(block, ref train);
				}
					
			}
			else if (subHeader[7] != 'b')
			{
				throw new Exception("Unrecognized subHeader \"" + subHeader + "\" in " + fileName);
			}
			else
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					KujuTokenID currentToken = (KujuTokenID) reader.ReadUInt16();
					if (currentToken != KujuTokenID.Train)
					{
						throw new Exception(); //Shape definition
					}
					reader.ReadUInt16(); 
					uint remainingBytes = reader.ReadUInt32();
					byte[] newBytes = reader.ReadBytes((int) remainingBytes);
					BinaryBlock block = new BinaryBlock(newBytes, KujuTokenID.Train);
					ParseBlock(block, ref train);
				}
			}

			//create couplers & other necessary properties for the thing to load
			//TODO: Pull out MSTS properties
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i / 2], train.Cars.Length > 1 ? train.Cars[i / 2 + 1] : null, train);
			}

			
		}

		private int currentCarIndex = -1;
		private CarBase currentCar;
		private bool reverseCurentCar;
		private void ParseBlock(Block block, ref TrainBase Train)
		{
			Block newBlock;
			switch (block.Token)
			{
				default:
					newBlock = block.ReadSubBlock();
					ParseBlock(newBlock, ref Train);
					break;
				case KujuTokenID.TrainCfg:
					string trainName = block.ReadString(); // we're unlikely to want this, as this is just the MSTS internal dictionary key
					while (block.Length() - block.Position() > 1)
					{
						try
						{
							newBlock = block.ReadSubBlock();
							ParseBlock(newBlock, ref Train);
						}
						catch
						{
							// ignore
						}
					}
					break;
				case KujuTokenID.Serial:
					/*
					 * This identifies the revision number of the consist (e.g. how many times it's been edited using the built-in MSTS tools)
					 * OpenRails ignores it.
					 * Not really helpful for our use-case at the minute.
					 */
					break;
				case KujuTokenID.MaxVelocity:
					// Presumably max speed of the consist (derails over this or something?)
					break;
				case KujuTokenID.NextWagonUID:
					/*
					 * This seems to identify the *next* wagon UID to be used when a train is coupled
					 * Again, OpenRails ignores it.
					 */
					break;
				case KujuTokenID.Durability:
					// Dunno- Presumably used for derailment / similar physics somewhere
					break;
				case KujuTokenID.Engine:
				case KujuTokenID.Wagon:
					newBlock = block.ReadSubBlock(new[] {KujuTokenID.EngineData, KujuTokenID.WagonData, KujuTokenID.UiD});
					Block secondBlock = block.ReadSubBlock(new[] {KujuTokenID.EngineData, KujuTokenID.WagonData, KujuTokenID.UiD});
					//Must have 2x blocks, car UiD and car name. Order doesn't matter however, so we've gotta DIY as we need the car number
					if (newBlock.Token == KujuTokenID.UiD)
					{
						ParseBlock(newBlock, ref Train);
						ParseBlock(secondBlock, ref Train);
					}
					else
					{
						ParseBlock(secondBlock, ref Train);
						ParseBlock(newBlock, ref Train);
					}

					currentCar.Doors = new[]
					{
						new Door(-1, 1000.0, 0),
						new Door(1, 1000.0, 0)
					};
					if (reverseCurentCar)
					{
						currentCar.Reverse();
						reverseCurentCar = false;
					}
					Train.Cars[currentCarIndex] = currentCar;
					/*
					 * FIXME: Needs removing or sorting when the car is created
					 */
					Train.Cars[currentCarIndex].FrontAxle.Follower.TriggerType = currentCarIndex == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
					Train.Cars[currentCarIndex].RearAxle.Follower.TriggerType = currentCarIndex == Train.Cars.Length - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
					Train.Cars[currentCarIndex].BeaconReceiver.TriggerType = currentCarIndex == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
					Train.Cars[currentCarIndex].BeaconReceiverPosition = 0.5 * Train.Cars[currentCarIndex].Length;
					Train.Cars[currentCarIndex].FrontAxle.Follower.Car = Train.Cars[currentCarIndex];
					Train.Cars[currentCarIndex].RearAxle.Follower.Car = Train.Cars[currentCarIndex];
					Train.Cars[currentCarIndex].FrontAxle.Position = 0.4 * Train.Cars[currentCarIndex].Length;
					Train.Cars[currentCarIndex].RearAxle.Position = -0.4 * Train.Cars[currentCarIndex].Length;
					break;
				// Engine / wagon block
				case KujuTokenID.UiD:
					// Unique ID of engine / wagon within consist
					// For the minute, let's just create a new car and advance our car number
					Array.Resize(ref Train.Cars, Train.Cars.Length + 1);
					currentCarIndex++;
					break;
				case KujuTokenID.WagonData:
				case KujuTokenID.EngineData:
					/*
					 * FIXME: All this needs to be pulled from the eng properties, or fixed so it doesn't matter
					 */
					currentCar = new CarBase(Train, currentCarIndex, 0.35, 0.0025, 1.1);
					currentCar.Specs = new CarPhysics();
					currentCar.CarBrake = new ElectricCommandBrake(EletropneumaticBrakeType.None, Train.Handles.EmergencyBrake, Train.Handles.Reverser, true, 0, 0, new AccelerationCurve[] { });
					currentCar.CarBrake.mainReservoir = new MainReservoir(690000.0, 780000.0, 0.01, 0.075 / Train.Cars.Length);
					currentCar.CarBrake.airCompressor = new Compressor(5000.0, currentCar.CarBrake.mainReservoir, currentCar);
					currentCar.CarBrake.equalizingReservoir = new EqualizingReservoir(50000.0, 250000.0, 200000.0);
					currentCar.CarBrake.equalizingReservoir.NormalPressure = 1.005 * 490000.0;
					currentCar.CarBrake.brakePipe = new BrakePipe(490000.0, 10000000.0, 1500000.0, 5000000.0, true);
					double r = 200000.0 / 440000.0 - 1.0;
					if (r < 0.1) r = 0.1;
					if (r > 1.0) r = 1.0;
					currentCar.CarBrake.auxiliaryReservoir = new AuxiliaryReservoir(0.975 * 490000.0, 200000.0, 0.5, r);
					currentCar.CarBrake.brakeCylinder = new BrakeCylinder(440000.0, 440000.0, 0.3 * 300000.0, 300000.0, 200000.0);
					currentCar.CarBrake.straightAirPipe = new StraightAirPipe(300000.0, 400000.0, 200000.0);
					currentCar.CarBrake.JerkUp = 10;
					currentCar.CarBrake.JerkDown = 10;
					currentCar.HoldBrake = new CarHoldBrake(currentCar);
					//FIXME END

					/*
					 * Pull out the wagon path bits from the block next
					 * From the available documentation and experience this *appears* to be as follows:
					 * [0] - Name of the wagon to search for
					 * [1] - Search path relative to the TRAINS\trainset directory, if not found in DB
					 *
					 * If WagonName that is already in the database, but with a different folder is supplied
					 * then the original will be returned
					 * https://digital-rails.com/wordpress/2018/11/18/duplicate-wagons/
					 * 
					 * HOWEVER:
					 * http://www.elvastower.com/forums/index.php?/topic/34187-or-consist-format/
					 * OpenRails seems to treat these as:
					 * [0] - WagonFileName => Must add approprite eng / wag extension
					 * [1] - Search path relative to the TRAINS\trainset directory
					 *
					 * Going to match MSTS for the minute, but possibly needs an OpenRails detection mechanism(?)
					 * Note that in all / most cases, both should be the same anyways.
					 */

					string[] wagonFiles = block.ReadStringArray();
					switch (wagonFiles.Length)
					{
						case 0:
							Plugin.currentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: Unable to determine WagonFile to load.");
							break;
						case 1:
							//Just a WagonName- This is likely invalid, but let's ignore
							Plugin.WagonParser.Parse(OpenBveApi.Path.CombineDirectory(currentFolder, "trainset"), wagonFiles[0], block.Token == KujuTokenID.EngineData, ref currentCar);
							Plugin.currentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: No WagonFolder supplied, searching entire trainset folder.");
							break;
						case 2:
							Plugin.WagonParser.Parse(OpenBveApi.Path.CombineDirectory(currentFolder, "trainset\\" + wagonFiles[1]), wagonFiles[0], block.Token == KujuTokenID.EngineData, ref currentCar);
							break;
						default:
							Plugin.WagonParser.Parse(OpenBveApi.Path.CombineDirectory(currentFolder, "trainset"), wagonFiles[1], block.Token == KujuTokenID.EngineData, ref currentCar);
							Plugin.currentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: Two parameters were expected- Check for correct escaping of strings.");
							break;
					}
					break;
				case KujuTokenID.Flip:
					// Allows a car to be reversed within a consist
					reverseCurentCar = true;
					break;
			}

		}

		internal string GetDescription(string fileName)
		{
			return string.Empty;
		}
	}
}
