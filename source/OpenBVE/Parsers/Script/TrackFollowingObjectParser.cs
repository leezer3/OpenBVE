using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace OpenBve
{
	internal static class TrackFollowingObjectParser
	{
		private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

		/// <summary>Parses a track following object</summary>
		/// <param name="ObjectPath">Absolute path to the object folder of route data</param>
		/// <param name="FileName">The XML file to parse</param>
		internal static TrainManager.TrackFollowingObject ParseTrackFollowingObject(string ObjectPath, string FileName)
		{
			// The current XML file to load
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);
			List<XElement> TfoElements = CurrentXML.XPathSelectElements("/openBVE/TrackFollowingObject").ToList();

			// Check this file actually contains OpenBVE other train definition elements
			if (!TfoElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException();
			}

			TrainManager.TrackFollowingObject Train = new TrainManager.TrackFollowingObject(TrainState.Pending);

			foreach (XElement Element in TfoElements)
			{
				ParseTrackFollowingObjectNode(ObjectPath, FileName, Element, Train);
			}

			return Train;
		}

		/// <summary>Parses a base track following object node</summary>
		/// <param name="ObjectPath">Absolute path to the object folder of route data</param>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <param name="Train">The track following object to parse this node into</param>
		private static void ParseTrackFollowingObjectNode(string ObjectPath, string FileName, XElement SectionElement, TrainManager.TrackFollowingObject Train)
		{
			string Section = SectionElement.Name.LocalName;

			string TrainDirectory = string.Empty;
			bool ConsistReversed = false;
			List<Game.TravelData> Data = new List<Game.TravelData>();

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "definition":
						ParseDefinitionNode(FileName, KeyNode, Train);
						break;
					case "train":
						ParseTrainNode(ObjectPath, FileName, KeyNode, ref TrainDirectory, ref ConsistReversed);
						break;
					case "points":
					case "stops":
						ParseTravelDataNodes(FileName, KeyNode, Data);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {Key} encountered in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}

			if (Data.Count < 2)
			{
				Interface.AddMessage(MessageType.Error, false, $"There must be at least two points to go through in {FileName}");
				return;
			}

			if (!(Data.First() is Game.TravelStopData) || !(Data.Last() is Game.TravelStopData))
			{
				Interface.AddMessage(MessageType.Error, false, $"The first and the last point to go through must be the \"Stop\" node in {FileName}");
				return;
			}

			if (string.IsNullOrEmpty(TrainDirectory))
			{
				Interface.AddMessage(MessageType.Error, false, $"No train has been specified in {FileName}");
				return;
			}

			/*
			 * First check for a train.ai file- Functionally identical, but allows for differently configured AI
			 * trains not to show up as drivable
			 */
			string TrainData = OpenBveApi.Path.CombineFile(TrainDirectory, "train.ai");
			if (!File.Exists(TrainData))
			{
				// Check for the standard drivable train.dat
				TrainData = OpenBveApi.Path.CombineFile(TrainDirectory, "train.dat");
			}
			string ExteriorFile = OpenBveApi.Path.CombineFile(TrainDirectory, "extensions.cfg");
			if (!File.Exists(TrainData) || !File.Exists(ExteriorFile))
			{
				Interface.AddMessage(MessageType.Error, true, $"The supplied train folder in TrackFollowingObject {FileName} did not contain a complete set of data.");
				return;
			}
			TrainDatParser.ParseTrainData(TrainData, TextEncoding.GetSystemEncodingFromFile(TrainData), Train);
			SoundCfgParser.ParseSoundConfig(TrainDirectory, Train);
			Train.AI = new Game.TrackFollowingObjectAI(Train, Data.ToArray());

			UnifiedObject[] CarObjects = new UnifiedObject[Train.Cars.Length];
			UnifiedObject[] BogieObjects = new UnifiedObject[Train.Cars.Length * 2];
			UnifiedObject[] CouplerObjects = new UnifiedObject[Train.Cars.Length - 1];
			bool[] VisibleFromInterior = new bool[Train.Cars.Length];
			ExtensionsCfgParser.ParseExtensionsConfig(System.IO.Path.GetDirectoryName(ExteriorFile), TextEncoding.GetSystemEncodingFromFile(ExteriorFile), ref CarObjects, ref BogieObjects, ref CouplerObjects, ref VisibleFromInterior, Train, true);

			int currentBogieObject = 0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (CarObjects[i] == null)
				{
					// load default exterior object
					string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
					StaticObject so;
					Program.CurrentHost.LoadStaticObject(file, Encoding.UTF8, false, out so);
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
					Train.Cars[i].LoadCarSections(CarObjects[i], false);
				}

				//Load bogie objects
				if (BogieObjects[currentBogieObject] != null)
				{
					Train.Cars[i].FrontBogie.LoadCarSections(BogieObjects[currentBogieObject], false);
				}
				currentBogieObject++;
				if (BogieObjects[currentBogieObject] != null)
				{
					Train.Cars[i].RearBogie.LoadCarSections(BogieObjects[currentBogieObject], false);
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
						Program.Sounds.LoadBuffer(Car.Doors[0].OpenSound.Buffer);
						Program.Sounds.LoadBuffer(Car.Doors[1].OpenSound.Buffer);
						double a = Car.Doors[0].OpenSound.Buffer.Duration;
						double b = Car.Doors[1].OpenSound.Buffer.Duration;
						Car.Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
					}
					else if (Car.Doors[0].OpenSound.Buffer != null)
					{
						Program.Sounds.LoadBuffer(Car.Doors[0].OpenSound.Buffer);
						double a = Car.Doors[0].OpenSound.Buffer.Duration;
						Car.Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
					}
					else if (Car.Doors[1].OpenSound.Buffer != null)
					{
						Program.Sounds.LoadBuffer(Car.Doors[0].OpenSound.Buffer);
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
						Program.Sounds.LoadBuffer(Car.Doors[0].CloseSound.Buffer);
						Program.Sounds.LoadBuffer(Car.Doors[1].CloseSound.Buffer);
						double a = Car.Doors[0].CloseSound.Buffer.Duration;
						double b = Car.Doors[1].CloseSound.Buffer.Duration;
						Car.Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
					}
					else if (Car.Doors[0].CloseSound.Buffer != null)
					{
						Program.Sounds.LoadBuffer(Car.Doors[0].CloseSound.Buffer);
						double a = Car.Doors[0].CloseSound.Buffer.Duration;
						Car.Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.8;
					}
					else if (Car.Doors[1].CloseSound.Buffer != null)
					{
						Program.Sounds.LoadBuffer(Car.Doors[0].CloseSound.Buffer);
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

			if (ConsistReversed)
			{
				Train.Reverse();
			}
			Train.PlaceCars(Data[0].Position);
		}

		/// <summary>
		/// Function to parse TFO definition
		/// </summary>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <param name="Train">The track following object to parse this node into</param>
		private static void ParseDefinitionNode(string FileName, XElement SectionElement, TrainManager.TrackFollowingObject Train)
		{
			string Section = SectionElement.Name.LocalName;

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				string Value = KeyNode.Value;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "appearancetime":
						if (Value.Any() && !Interface.TryParseTime(Value, out Train.AppearanceTime))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "appearancestartposition":
						Train.AppearanceStartPosition = NumberFormats.ParseDouble(Value, Key, Section, LineNumber, FileName);
						break;
					case "appearanceendposition":
						Train.AppearanceEndPosition = NumberFormats.ParseDouble(Value, Key, Section, LineNumber, FileName);
						break;
					case "leavetime":
						if (Value.Any() && !Interface.TryParseTime(Value, out Train.LeaveTime))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {Key} encountered in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}
		}

		/// <summary>
		/// Function to parse train definition
		/// </summary>
		/// <param name="ObjectPath">Absolute path to the object folder of route data</param>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <param name="TrainDirectory">Absolute path to the train directory</param>
		/// <param name="ConsistReversed">Whether to reverse the train composition.</param>
		private static void ParseTrainNode(string ObjectPath, string FileName, XElement SectionElement, ref string TrainDirectory, ref bool ConsistReversed)
		{
			string Section = SectionElement.Name.LocalName;

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				string Value = KeyNode.Value;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "directory":
						{
							string TmpPath = OpenBveApi.Path.CombineDirectory(System.IO.Path.GetDirectoryName(FileName), Value);
							if (!Directory.Exists(TmpPath))
							{
								TmpPath = OpenBveApi.Path.CombineFile(Program.FileSystem.InitialTrainFolder, Value);
							}
							if (!Directory.Exists(TmpPath))
							{
								TmpPath = OpenBveApi.Path.CombineFile(Program.FileSystem.TrainInstallationDirectory, Value);
							}
							if (!Directory.Exists(TmpPath))
							{
								TmpPath = OpenBveApi.Path.CombineFile(ObjectPath, Value);
							}

							if (!Directory.Exists(TmpPath))
							{
								Interface.AddMessage(MessageType.Error, false, $"Directory was not found in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
							}
							else
							{
								TrainDirectory = TmpPath;
							}
						}
						break;
					case "reversed":
						if (Value.Any())
						{
							switch (Value.ToLowerInvariant())
							{
								case "true":
									ConsistReversed = true;
									break;
								case "false":
									ConsistReversed = false;
									break;
								default:
									{
										int n;

										if (!NumberFormats.TryParseIntVb6(Value, out n))
										{
											Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
										}
										else
										{
											ConsistReversed = Convert.ToBoolean(n);
										}
									}
									break;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {Key} encountered in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}
		}

		/// <summary>Parses a train travel data node</summary>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <param name="Data">The list of travel data to add this to</param>
		private static void ParseTravelDataNodes(string FileName, XElement SectionElement, ICollection<Game.TravelData> Data)
		{
			string Section = SectionElement.Name.LocalName;

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "stop":
						Data.Add(ParseTravelStopNode(FileName, KeyNode));
						break;
					case "point":
						Data.Add(ParseTravelPointNode(FileName, KeyNode));
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {Key} encountered in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}
		}

		/// <summary>
		/// Function to parse the contents of TravelData class
		/// </summary>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <param name="Data">Travel data to which the parse results apply</param>
		private static void ParseTravelDataNode(string FileName, XElement SectionElement, Game.TravelData Data)
		{
			string Section = SectionElement.Name.LocalName;

			double Decelerate = 0.0;
			double Accelerate = 0.0;
			double TargetSpeed = 0.0;

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				string Value = KeyNode.Value;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "decelerate":
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out Decelerate) || Decelerate < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "position":
					case "stopposition":
						Data.Position = NumberFormats.ParseDouble(Value, Key, Section, LineNumber, FileName);
						break;
					case "accelerate":
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out Accelerate) || Accelerate < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "targetspeed":
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out TargetSpeed) || TargetSpeed < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "rail":
						if (Value.Any() && !NumberFormats.TryParseIntVb6(Value, out Data.RailIndex) || Data.RailIndex < 0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative integer number in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
							Data.RailIndex = 0;
						}
						break;
				}
			}

			Data.Decelerate = -Decelerate / 3.6;
			Data.Accelerate = Accelerate / 3.6;
			Data.TargetSpeed = TargetSpeed / 3.6;
		}

		/// <summary>
		/// Function to parse the contents of TravelStopData class
		/// </summary>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <returns>An instance of the new TravelStopData class with the parse result applied</returns>
		private static Game.TravelStopData ParseTravelStopNode(string FileName, XElement SectionElement)
		{
			string Section = SectionElement.Name.LocalName;

			Game.TravelStopData Data = new Game.TravelStopData();

			ParseTravelDataNode(FileName, SectionElement, Data);

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				string Value = KeyNode.Value;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "stoptime":
						if (Value.Any() && !Interface.TryParseTime(Value, out Data.StopTime))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "doors":
						{
							int Door = 0;
							bool DoorBoth = false;

							switch (Value.ToLowerInvariant())
							{
								case "l":
								case "left":
									Door = -1;
									break;
								case "r":
								case "right":
									Door = 1;
									break;
								case "n":
								case "none":
								case "neither":
									Door = 0;
									break;
								case "b":
								case "both":
									DoorBoth = true;
									break;
								default:
									if (Value.Any() && !NumberFormats.TryParseIntVb6(Value, out Door))
									{
										Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
									}
									break;
							}

							Data.OpenLeftDoors = Door < 0.0 | DoorBoth;
							Data.OpenRightDoors = Door > 0.0 | DoorBoth;
						}
						break;
					case "direction":
						{
							int d = 0;
							switch (Value.ToLowerInvariant())
							{
								case "f":
									d = 1;
									break;
								case "r":
									d = -1;
									break;
								default:
									if (Value.Any() && (!NumberFormats.TryParseIntVb6(Value, out d) || !Enum.IsDefined(typeof(TravelDirection), d)))
									{
										Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
										d = 1;
									}
									break;
							}

							Data.Direction = (TravelDirection)d;
						}
						break;
					case "decelerate":
					case "position":
					case "stopposition":
					case "accelerate":
					case "targetspeed":
					case "rail":
						// Already parsed
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {Key} encountered in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}

			return Data;
		}

		/// <summary>
		/// Function to parse the contents of TravelPointData class
		/// </summary>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <returns>An instance of the new TravelPointData class with the parse result applied</returns>
		private static Game.TravelPointData ParseTravelPointNode(string FileName, XElement SectionElement)
		{
			string Section = SectionElement.Name.LocalName;

			Game.TravelPointData Data = new Game.TravelPointData();

			ParseTravelDataNode(FileName, SectionElement, Data);

			double PassingSpeed = 0.0;

			foreach (XElement KeyNode in SectionElement.Elements())
			{
				string Key = KeyNode.Name.LocalName;
				string Value = KeyNode.Value;
				int LineNumber = ((IXmlLineInfo)KeyNode).LineNumber;

				switch (Key.ToLowerInvariant())
				{
					case "passingspeed":
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out PassingSpeed) || PassingSpeed < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "decelerate":
					case "position":
					case "stopposition":
					case "accelerate":
					case "targetspeed":
					case "rail":
						// Already parsed
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {Key} encountered in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}

			Data.PassingSpeed = PassingSpeed / 3.6;

			return Data;
		}
	}
}
