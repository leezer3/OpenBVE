using System.Collections.Generic;
using System.Linq;
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

			// In order to make the Animated object function, temporary train data is set.
			TrainManager.OtherTrain Train = new TrainManager.OtherTrain(TrainManager.TrainState.Pending);
			string TrainData = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
			TrainDatParser.ParseTrainData(TrainData, System.Text.Encoding.UTF8, Train);

			foreach (XElement Element in DocumentElements)
			{
				ParseOtherTrainNode(Element, FileName, Path, Train);
			}

			return Train;
		}

		private static void ParseOtherTrainNode(XElement Element, string FileName, string Path, TrainManager.OtherTrain Train)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			int RailNumber = 0;
			int NumberOfCars = 0;
			string ExteriorFile = string.Empty;
			List<Game.TravelData> Data = new List<Game.TravelData>();

			foreach (XElement SectionElement in Element.Elements())
			{
				string Section = SectionElement.Name.LocalName;

				switch (SectionElement.Name.LocalName.ToLowerInvariant())
				{
					case "stops":
						ParseOtherTrainStopNode(SectionElement, FileName, Data);
						break;
					case "cars":
						foreach (XElement KeyNode in SectionElement.Elements())
						{
							string Key = KeyNode.Name.LocalName;
							string Value = KeyNode.Value;
							int LineNumber = ((IXmlLineInfo) KeyNode).LineNumber;

							switch (Key.ToLowerInvariant())
							{
								case "numberofcars":
									{
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out NumberOfCars))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
									}
									break;
								case "filename":
									{
										string File = OpenBveApi.Path.CombineFile(Path, Value);
										if (System.IO.File.Exists(File))
										{
											ExteriorFile = File;
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
								case "rail":
									{
										if (Value.Length != 0 && !NumberFormats.TryParseIntVb6(Value, out RailNumber))
										{
											Interface.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + Section + " at line " + LineNumber.ToString(Culture) + " in " + FileName);
										}
										break;
									}
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

			if (!Data.Any())
			{
				return;
			}

			// Initial setting
			Train.AI = new Game.OtherTrainAI(Train, Data);
			Train.Cars = new TrainManager.Car[NumberOfCars];
			for (int i = 0; i < NumberOfCars; i++)
			{
				Train.Cars[i] = new TrainManager.Car(Train, i);
			}
			Train.Couplers = new TrainManager.Coupler[NumberOfCars - 1];

			UnifiedObject[] CarObjects = new UnifiedObject[NumberOfCars];
			UnifiedObject[] BogieObjects = new UnifiedObject[NumberOfCars * 2];

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

			foreach (var Car in Train.Cars)
			{
				Car.ChangeCarSection(TrainManager.CarSectionType.NotVisible);
				Car.FrontAxle.Follower.TrackIndex = RailNumber;
				Car.RearAxle.Follower.TrackIndex = RailNumber;
				Car.FrontBogie.FrontAxle.Follower.TrackIndex = RailNumber;
				Car.FrontBogie.RearAxle.Follower.TrackIndex = RailNumber;
				Car.RearBogie.FrontAxle.Follower.TrackIndex = RailNumber;
				Car.RearBogie.RearAxle.Follower.TrackIndex = RailNumber;
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
