using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	class OtherTrainXMLParser
	{
		internal static TrainManager.OtherTrain ParseOtherTrainXML(string FileName)
		{
			// The current XML file to load
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);
			string Path = System.IO.Path.GetDirectoryName(FileName);

			// Check for null
			if (CurrentXML.Root == null)
			{
				// We couldn't find any valid XML, so return false
				throw new System.IO.InvalidDataException();
			}

			IEnumerable<XElement> DocumentElements = CurrentXML.Root.Elements("OtherTrain");

			// Check this file actually contains OpenBVE other train definition elements
			if (DocumentElements == null || !DocumentElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new System.IO.InvalidDataException();
			}

			TrainManager.OtherTrain Train = new TrainManager.OtherTrain(TrainManager.TrainState.Pending);

			foreach (XElement Element in DocumentElements)
			{
				ParseOtherTrainNode(Element, FileName, Path, Train);
			}

			return Train;
		}

		private static void ParseOtherTrainNode(XElement Element, string FileName, string Path, TrainManager.OtherTrain Train)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string TrainDirectory = string.Empty;
			List<Game.TravelData> Data = new List<Game.TravelData>();

			foreach (XElement SectionElement in Element.Elements())
			{
				string Section = SectionElement.Name.LocalName;

				switch (SectionElement.Name.LocalName.ToLowerInvariant())
				{
					case "stops":
						ParseOtherTrainStopNode(SectionElement, FileName, Data);
						break;
					case "train":
						foreach (XElement KeyNode in SectionElement.Elements())
						{
							string Key = KeyNode.Name.LocalName;
							string Value = KeyNode.Value;
							int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

							switch (Key.ToLowerInvariant())
							{
								case "directory":
									{
										string Directory = OpenBveApi.Path.CombineDirectory(Path, Value);
										if (System.IO.Directory.Exists(Directory))
										{
											TrainDirectory = Directory;
										}
										else
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
									}
									break;
							}
						}
						break;
					case "definition":
						foreach (XElement KeyNode in SectionElement.Elements())
						{
							string Key = KeyNode.Name.LocalName;
							string Value = KeyNode.Value;
							int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

							switch (Key.ToLowerInvariant())
							{
								case "appearancetime":
									if (Value.Length != 0 && !Interface.TryParseTime(Value, out Train.AppearanceTime))
									{
										Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "appearancestartposition":
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Train.AppearanceStartPosition))
									{
										Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "appearanceendposition":
									if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Train.AppearanceEndPosition))
									{
										Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
								case "leavetime":
									if (Value.Length != 0 && !Interface.TryParseTime(Value, out Train.LeaveTime))
									{
										Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
									}
									break;
							}
						}
						break;
				}
			}

			if (!Data.Any() || string.IsNullOrEmpty(TrainDirectory))
			{
				return;
			}

			// Initial setting
			string TrainData = OpenBveApi.Path.CombineFile(TrainDirectory, "train.dat");
			string ExteriorFile = OpenBveApi.Path.CombineFile(TrainDirectory, "extensions.cfg");
			TrainDatParser.ParseTrainData(TrainData, TextEncoding.GetSystemEncodingFromFile(TrainData), Train);
			SoundCfgParser.ParseSoundConfig(TrainDirectory, Encoding.UTF8, Train);
			Train.AI = new Game.OtherTrainAI(Train, Data);

			UnifiedObject[] CarObjects = new UnifiedObject[Train.Cars.Length];
			UnifiedObject[] BogieObjects = new UnifiedObject[Train.Cars.Length * 2];

			ExtensionsCfgParser.ParseExtensionsConfig(System.IO.Path.GetDirectoryName(ExteriorFile), TextEncoding.GetSystemEncodingFromFile(ExteriorFile), ref CarObjects, ref BogieObjects, Train, true);

			int currentBogieObject = 0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (CarObjects[i] == null)
				{
					// load default exterior object
					string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
					ObjectManager.StaticObject so = ObjectManager.LoadStaticObject(file, System.Text.Encoding.UTF8, false, false, false);
					if (so == null)
					{
						CarObjects[i] = null;
					}
					else
					{
						double sx = Train.Cars[i].Width;
						double sy = Train.Cars[i].Height;
						double sz = Train.Cars[i].Length;
						so.ApplyScale(sx, sy, sz);
						CarObjects[i] = so;
					}
				}
				if (CarObjects[i] != null)
				{
					// add object
					Train.Cars[i].LoadCarSections(CarObjects[i]);
				}

				//Load bogie objects
				if (BogieObjects[currentBogieObject] != null)
				{
					Train.Cars[i].FrontBogie.LoadCarSections(BogieObjects[currentBogieObject]);
				}
				currentBogieObject++;
				if (BogieObjects[currentBogieObject] != null)
				{
					Train.Cars[i].RearBogie.LoadCarSections(BogieObjects[currentBogieObject]);
				}
				currentBogieObject++;
			}

			// door open/close speed
			foreach (var Car in Train.Cars)
			{
				if (Car.Specs.DoorOpenFrequency <= 0.0)
				{
					if (Car.Doors[0].OpenSound.Buffer != null & Car.Doors[1].OpenSound.Buffer != null)
					{
						Sounds.LoadBuffer(Car.Doors[0].OpenSound.Buffer);
						Sounds.LoadBuffer(Car.Doors[1].OpenSound.Buffer);
						double a = Car.Doors[0].OpenSound.Buffer.Duration;
						double b = Car.Doors[1].OpenSound.Buffer.Duration;
						Car.Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
					}
					else if (Car.Doors[0].OpenSound.Buffer != null)
					{
						Sounds.LoadBuffer(Car.Doors[0].OpenSound.Buffer);
						double a = Car.Doors[0].OpenSound.Buffer.Duration;
						Car.Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
					}
					else if (Car.Doors[1].OpenSound.Buffer != null)
					{
						Sounds.LoadBuffer(Car.Doors[0].OpenSound.Buffer);
						double b = Car.Doors[1].OpenSound.Buffer.Duration;
						Car.Specs.DoorOpenFrequency = b > 0.0 ? 1.0 / b : 0.8;
					}
					else
					{
						Car.Specs.DoorOpenFrequency = 0.8;
					}
				}
				if (Car.Specs.DoorCloseFrequency <= 0.0)
				{
					if (Car.Doors[0].CloseSound.Buffer != null & Car.Doors[1].CloseSound.Buffer != null)
					{
						Sounds.LoadBuffer(Car.Doors[0].CloseSound.Buffer);
						Sounds.LoadBuffer(Car.Doors[1].CloseSound.Buffer);
						double a = Car.Doors[0].CloseSound.Buffer.Duration;
						double b = Car.Doors[1].CloseSound.Buffer.Duration;
						Car.Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
					}
					else if (Car.Doors[0].CloseSound.Buffer != null)
					{
						Sounds.LoadBuffer(Car.Doors[0].CloseSound.Buffer);
						double a = Car.Doors[0].CloseSound.Buffer.Duration;
						Car.Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.8;
					}
					else if (Car.Doors[1].CloseSound.Buffer != null)
					{
						Sounds.LoadBuffer(Car.Doors[0].CloseSound.Buffer);
						double b = Car.Doors[1].CloseSound.Buffer.Duration;
						Car.Specs.DoorCloseFrequency = b > 0.0 ? 1.0 / b : 0.8;
					}
					else
					{
						Car.Specs.DoorCloseFrequency = 0.8;
					}
				}
				const double f = 0.015;
				const double g = 2.75;
				Car.Specs.DoorOpenPitch = Math.Exp(f * Math.Tan(g * (Program.RandomNumberGenerator.NextDouble() - 0.5)));
				Car.Specs.DoorClosePitch = Math.Exp(f * Math.Tan(g * (Program.RandomNumberGenerator.NextDouble() - 0.5)));
				Car.Specs.DoorOpenFrequency /= Car.Specs.DoorOpenPitch;
				Car.Specs.DoorCloseFrequency /= Car.Specs.DoorClosePitch;
				/*
				 * Remove the following two lines, then the pitch at which doors play
				 * takes their randomized opening and closing times into account.
				 * */
				Car.Specs.DoorOpenPitch = 1.0;
				Car.Specs.DoorClosePitch = 1.0;
			}

			foreach (var Car in Train.Cars)
			{
				Car.FrontAxle.Follower.TrackIndex = Data[0].RailIndex;
				Car.RearAxle.Follower.TrackIndex = Data[0].RailIndex;
				Car.FrontBogie.FrontAxle.Follower.TrackIndex = Data[0].RailIndex;
				Car.FrontBogie.RearAxle.Follower.TrackIndex = Data[0].RailIndex;
				Car.RearBogie.FrontAxle.Follower.TrackIndex = Data[0].RailIndex;
				Car.RearBogie.RearAxle.Follower.TrackIndex = Data[0].RailIndex;
			}

			Train.PlaceCars(Data[0].StopPosition);
		}

		private static void ParseOtherTrainStopNode(XElement Element, string FileName, List<Game.TravelData> Data)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			foreach (XElement SectionElement in Element.Elements())
			{
				string Section = SectionElement.Name.LocalName;

				switch (SectionElement.Name.LocalName.ToLowerInvariant())
				{
					case "stop":
						{
							Game.TravelData NewData = new Game.TravelData();
							double Decelerate = 0.0;
							double Accelerate = 0.0;
							double TargetSpeed = 0.0;

							foreach (XElement KeyNode in SectionElement.Elements())
							{
								string Key = KeyNode.Name.LocalName;
								string Value = KeyNode.Value;
								int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

								switch (Key.ToLowerInvariant())
								{
									case "decelerate":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Decelerate))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "stopposition":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out NewData.StopPosition))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "stoptime":
										if (Value.Length != 0 && !Interface.TryParseTime(Value, out NewData.StopTime))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "doors":
										{
											int door = 0;
											bool doorboth = false;

											switch (Value.ToLowerInvariant())
											{
												case "l":
												case "left":
													door = -1;
													break;
												case "r":
												case "right":
													door = 1;
													break;
												case "n":
												case "none":
												case "neither":
													door = 0;
													break;
												case "b":
												case "both":
													doorboth = true;
													break;
												default:
													if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out door))
													{
														Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
														door = 0;
													}
													break;
											}

											NewData.OpenLeftDoors = door < 0.0 | doorboth;
											NewData.OpenRightDoors = door > 0.0 | doorboth;
										}
										break;
									case "accelerate":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out Accelerate))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "targetspeed":
										if (Value.Length != 0 && !NumberFormats.TryParseDoubleVb6(Value, out TargetSpeed))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									case "direction":
										{
											int d = 0;
											if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out d))
											{
												Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
											}
											else if (d == 1 || d == -1)
											{
												NewData.Direction = (Game.TravelDirection) d;
											}
										}
										break;
									case "rail":
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out NewData.RailIndex))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
								}
							}

							NewData.Decelerate = Decelerate / 3.6;
							NewData.Accelerate = Accelerate / 3.6;
							NewData.TargetSpeed = TargetSpeed / 3.6;
							Data.Add(NewData);
						}
						break;
				}
			}
		}
	}
}
