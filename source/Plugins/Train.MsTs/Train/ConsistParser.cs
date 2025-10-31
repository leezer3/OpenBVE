using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibRender2.Trains;
using OpenBve.Formats.MsTs;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
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
			train.Handles.LocoBrake = new LocoBrakeHandle(0, train.Handles.EmergencyBrake, new double[] {}, new double[] {}, train);
			train.Handles.LocoBrakeType = LocoBrakeType.Independant;
			train.Handles.HasLocoBrake = false;
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			train.Specs.AveragesPressureDistribution = true;
			train.SafetySystems.Headlights = new LightSource(train, 2);
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
					string s = unicode ? Encoding.Unicode.GetString(newBytes) : Encoding.ASCII.GetString(newBytes);
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

			bool hasCabview = false;
			//create couplers & other necessary properties for the thing to load
			//TODO: Pull out MSTS properties
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i / 2], train.Cars.Length > 1 ? train.Cars[i / 2 + 1] : null);
				train.Cars[i].CurrentCarSection = -1;
				train.Cars[i].ChangeCarSection(CarSectionType.NotVisible);
				train.Cars[i].FrontBogie.ChangeSection(-1);
				train.Cars[i].RearBogie.ChangeSection(-1);
				train.Cars[i].Coupler.ChangeSection(-1);
				train.Cars[i].Specs.ExposedFrontalArea = 0.6 * train.Cars[i].Width * train.Cars[i].Height;
				train.Cars[i].Specs.UnexposedFrontalArea = 0.2 * train.Cars[i].Width * train.Cars[i].Height;
				train.Cars[i].Specs.CenterOfGravityHeight = 1.6;
				train.Cars[i].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * train.Cars[i].Specs.CenterOfGravityHeight / train.Cars[i].Width);
				if (train.Cars[i].HasInteriorView && hasCabview == false)
				{
					// For the minute at least, let's set our driver car to be the first car which has an interior view
					hasCabview = true;
					train.DriverCar = i;
				}

				train.Cars[train.Cars.Length - 1].RearAxle.Follower.TriggerType = i == train.Cars.Length - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
			}

			train.Cars[train.Cars.Length - 1].RearAxle.Follower.TriggerType = EventTriggerType.RearCarRearAxle;

			train.Cars[train.DriverCar].Windscreen = new Windscreen(256, 10.0, train.Cars[train.DriverCar]);
			train.Cars[train.DriverCar].Windscreen.Wipers = new WindscreenWiper(train.Cars[Train.DriverCar].Windscreen, WiperPosition.Left, WiperPosition.Left, 1.0, 0.0, true); // hack: zero hold time so they act as fast with two states
			train.PlaceCars(0.0);
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
				case KujuTokenID.Default:
					// presumably used internally by MSTS, not useful
					block.Skip((int)block.Length());
					break;
				case KujuTokenID.Name:
					string consistName = block.ReadString(); // displayed name for consist in-game
					break;
				case KujuTokenID.TrainCfg:
					string trainName = block.ReadString(); // we're unlikely to want this, as this is just the MSTS internal dictionary key
					while (block.Length() - block.Position() > 2)
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
					Array.Resize(ref Train.Cars, Train.Cars.Length + 1);
					currentCarIndex++;
					while (block.Length() - block.Position() > 2)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.EngineData, KujuTokenID.WagonData, KujuTokenID.UiD, KujuTokenID.Flip, KujuTokenID.EngineVariables });
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
					currentCar.Breaker = new Breaker(currentCar);
					currentCar.Sounds.Plugin = new Dictionary<int, CarSound>();
					Train.Cars[currentCarIndex] = currentCar;
					/*
					 * FIXME: Needs removing or sorting when the car is created
					 */
					Train.Cars[currentCarIndex].FrontAxle.Follower.TriggerType = currentCarIndex == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
					Train.Cars[currentCarIndex].BeaconReceiver.TriggerType = currentCarIndex == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
					Train.Cars[currentCarIndex].BeaconReceiverPosition = 0.5 * Train.Cars[currentCarIndex].Length;
					Train.Cars[currentCarIndex].FrontAxle.Position = 0.4 * Train.Cars[currentCarIndex].Length;
					Train.Cars[currentCarIndex].RearAxle.Position = -0.4 * Train.Cars[currentCarIndex].Length;
					break;
				// Engine / wagon block
				case KujuTokenID.UiD:
					// Unique ID of engine / wagon within consist
					// For the minute, let's just create a new car and advance our car number
					break;
				case KujuTokenID.WagonData:
				case KujuTokenID.EngineData:
					/*
					 * FIXME: All this needs to be pulled from the eng properties, or fixed so it doesn't matter
					 */
					currentCar = new CarBase(Train, currentCarIndex, 0.35, 0.0025, 1.1);
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
							Plugin.WagonParser.Parse(OpenBveApi.Path.CombineDirectory(currentFolder, "trainset"), wagonFiles[0], block.Token == KujuTokenID.EngineData, ref currentCar, ref Train);
							Plugin.currentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: No WagonFolder supplied, searching entire trainset folder.");
							break;
						case 2:
							Plugin.WagonParser.Parse(OpenBveApi.Path.CombineDirectory(currentFolder, "trainset\\" + wagonFiles[1]), wagonFiles[0], block.Token == KujuTokenID.EngineData, ref currentCar, ref Train);
							break;
						default:
							Plugin.WagonParser.Parse(OpenBveApi.Path.CombineDirectory(currentFolder, "trainset"), wagonFiles[1], block.Token == KujuTokenID.EngineData, ref currentCar, ref Train);
							Plugin.currentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: Two parameters were expected- Check for correct escaping of strings.");
							break;
					}
					break;
				case KujuTokenID.Flip:
					// Allows a car to be reversed within a consist
					reverseCurentCar = true;
					break;
				case KujuTokenID.EngineVariables:
					// Sets properties of the train when loaded, ignore for the minute
					break;
			}

		}

		internal string GetDescription(string fileName)
		{
			return string.Empty;
		}
	}
}
