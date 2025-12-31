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
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal class ConsistParser
	{
		internal readonly Plugin Plugin;
		internal string TrainsetDirectory;

		internal ConsistParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		internal void ReadConsist(string fileName, ref AbstractTrain parsedTrain)
		{
			currentCarIndex = -1;
			TrainBase train = parsedTrain as TrainBase;
			if (train == null)
			{
				throw new Exception();
			}
			train.Handles.Reverser = new ReverserHandle(train);
			train.Handles.EmergencyBrake = new EmergencyHandle(train);
			train.Handles.Power = new PowerHandle(8, train);
			train.Handles.Brake = new BrakeHandle(8, train.Handles.EmergencyBrake, train);
			train.Handles.LocoBrake = new LocoBrakeHandle(0, train.Handles.EmergencyBrake, train);
			train.Handles.LocoBrakeType = LocoBrakeType.Independant;
			train.Handles.HasLocoBrake = false;
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			train.Specs.AveragesPressureDistribution = true;
			train.SafetySystems.Headlights = new LightSource(train, 2);
			if(Directory.Exists(Plugin.FileSystem.MSTSDirectory))
			{
				TrainsetDirectory = OpenBveApi.Path.CombineDirectory(Plugin.FileSystem.MSTSDirectory, "TRAINS\\trainset");
			}
			else
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The MSTS directory has not been set. Attempting to find the trainset directory.");
				string currentFolder = Path.GetDirectoryName(fileName);
				if (currentFolder == null)
				{
					throw new Exception("MSTS Consist Parser: Unable to determine the consist working directory.");
				}
				DirectoryInfo d = Directory.GetParent(currentFolder);
				if (d == null || !d.Name.Equals("TRAINS", StringComparison.InvariantCultureIgnoreCase))
				{
					//FIXME: Better finding of the trainset folder (set in options?)
					throw new Exception("MSTS Consist Parser: Unable to find the MSTS TRAINS folder.");
				}
				TrainsetDirectory = OpenBveApi.Path.CombineDirectory(currentFolder, "trainset");
			}

			if (!Directory.Exists(TrainsetDirectory))
			{
				throw new Exception("MSTS Consist Parser: Unable to find the MSTS trainset folder.");
			}

			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

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

			if (train.Cars.Length == 0)
			{
				throw new InvalidDataException("Consist " + fileName + " appears to be invalid or malformed");
			}

			bool hasCabview = false;
			//create couplers & other necessary properties for the thing to load
			//TODO: Pull out MSTS properties
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i / 2], train.Cars.Length > 1 ? train.Cars[i / 2 + 1] : null);
				train.Cars[i].CurrentCarSection = CarSectionType.NotVisible;
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
					train.CameraCar = i;
				}

				train.Cars[train.Cars.Length - 1].RearAxle.Follower.TriggerType = i == train.Cars.Length - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;

				if (train.Cars[i].TractionModel is TenderEngine)
				{
					bool hasTender = i > 0 && train.Cars[i - 1].TractionModel is Tender || i < train.Cars.Length - 2 && train.Cars[i + 1].TractionModel is Tender;

					if (hasTender == false)
					{
						// this is actually harmless at the minute
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS Consist Parser: Steam Engine in car " + i + " requires a tender, but none is present.");
					}
				}
			}

			train.Cars[train.Cars.Length - 1].RearAxle.Follower.TriggerType = EventTriggerType.RearCarRearAxle;
			train.Cars[train.DriverCar].Windscreen = new Windscreen(256, 10.0, train.Cars[train.DriverCar]);
			train.Cars[train.DriverCar].Windscreen.Wipers = new WindscreenWiper(train.Cars[parsedTrain.DriverCar].Windscreen, WiperPosition.Left, WiperPosition.Left, 1.0, 0.0, true); // hack: zero hold time so they act as fast with two states
			train.Specs.AveragesPressureDistribution = false;
			train.PlaceCars(0.0);
		}

		private int currentCarIndex = -1;
		private CarBase currentCar;
		private bool reverseCurrentCar;
		private void ParseBlock(Block block, ref TrainBase currentTrain)
		{
			Block newBlock;
			switch (block.Token)
			{
				default:
					while (block.Length() - block.Position() > 2)
					{
						// YUCK: If valid wagon blocks are outside the TrainCfg block (incorrect terminator)
						// MSTS actually adds them to the end of the train, e.g. see default oenoloco.con
						newBlock = block.ReadSubBlock(true);
						ParseBlock(newBlock, ref currentTrain);
					}
					break;
				case KujuTokenID.Comment:
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
							newBlock = block.ReadSubBlock(true);
							ParseBlock(newBlock, ref currentTrain);
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
					Array.Resize(ref currentTrain.Cars, currentTrain.Cars.Length + 1);
					currentCarIndex++;
					while (block.Length() - block.Position() > 2)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.EngineData, KujuTokenID.WagonData, KujuTokenID.UiD, KujuTokenID.Flip, KujuTokenID.EngineVariables });
						ParseBlock(newBlock, ref currentTrain);
					}
					currentCar.Doors = new[]
					{
						new Door(-1, 1000.0, 0),
						new Door(1, 1000.0, 0)
					};
					if (reverseCurrentCar)
					{
						currentCar.Reverse();
						reverseCurrentCar = false;
					}
					currentCar.Breaker = new Breaker(currentCar);
					currentCar.Sounds.Plugin = new Dictionary<int, CarSound>();
					currentTrain.Cars[currentCarIndex] = currentCar;
					/*
					 * FIXME: Needs removing or sorting when the car is created
					 */
					currentTrain.Cars[currentCarIndex].FrontAxle.Follower.TriggerType = currentCarIndex == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
					currentTrain.Cars[currentCarIndex].BeaconReceiver.TriggerType = currentCarIndex == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
					currentTrain.Cars[currentCarIndex].BeaconReceiverPosition = 0.5 * currentTrain.Cars[currentCarIndex].Length;
					currentTrain.Cars[currentCarIndex].FrontAxle.Position = 0.4 * currentTrain.Cars[currentCarIndex].Length;
					currentTrain.Cars[currentCarIndex].RearAxle.Position = -0.4 * currentTrain.Cars[currentCarIndex].Length;
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
					currentCar = new CarBase(currentTrain, currentCarIndex);
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
					 * [0] - WagonFileName => Must add appropriate eng / wag extension
					 * [1] - Search path relative to the TRAINS\trainset directory
					 *
					 * Going to match MSTS for the minute, but possibly needs an OpenRails detection mechanism(?)
					 * Note that in all / most cases, both should be the same anyways.
					 */

					string[] wagonFiles = block.ReadStringArray();
					switch (wagonFiles.Length)
					{
						case 0:
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: Unable to determine WagonFile to load.");
							break;
						case 1:
							//Just a WagonName- This is likely invalid, but let's ignore
							Plugin.WagonParser.Parse(TrainsetDirectory, wagonFiles[0], block.Token == KujuTokenID.EngineData, ref currentCar, ref currentTrain);
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: No WagonFolder supplied, searching entire trainset folder.");
							break;
						case 2:
							string wagonDirectory = OpenBveApi.Path.CombineDirectory(TrainsetDirectory, wagonFiles[1]);
							if (!Directory.Exists(wagonDirectory))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: WagonFolder " + wagonDirectory + " was not found.");
								currentCar.Width = 2.6;
								currentCar.Height = 3.6;
								currentCar.Length = 25;
								currentCar.EmptyMass = 1000;
								currentCar.TractionModel = new BVETrailerCar(currentCar);
								currentCar.CarBrake = new ThroughPiped(currentCar); // dummy
								break;
							}
							Plugin.WagonParser.Parse(wagonDirectory, wagonFiles[0], block.Token == KujuTokenID.EngineData, ref currentCar, ref currentTrain);
							break;
						default:
							Plugin.WagonParser.Parse(TrainsetDirectory, wagonFiles[1], block.Token == KujuTokenID.EngineData, ref currentCar, ref currentTrain);
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "MSTS Consist Parser: Two parameters were expected- Check for correct escaping of strings.");
							break;
					}
					break;
				case KujuTokenID.Flip:
					// Allows a car to be reversed within a consist
					reverseCurrentCar = true;
					break;
				case KujuTokenID.EngineVariables:
					// Sets properties of the train when loaded, ignore for the minute
					break;
			}

		}
	}
}
