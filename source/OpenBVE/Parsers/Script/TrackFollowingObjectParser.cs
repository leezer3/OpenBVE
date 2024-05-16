using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	internal static class TrackFollowingObjectParser
	{
		private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

		/// <summary>Parses a track following object</summary>
		/// <param name="ObjectPath">Absolute path to the object folder of route data</param>
		/// <param name="FileName">The XML file to parse</param>
		internal static ScriptedTrain ParseTrackFollowingObject(string ObjectPath, string FileName)
		{
			// The current XML file to load
			XDocument CurrentXML = XDocument.Load(FileName, LoadOptions.SetLineInfo);
			List<XElement> ScriptedTrainElements = CurrentXML.XPathSelectElements("/openBVE/TrackFollowingObject").ToList();

			// Check this file actually contains OpenBVE other train definition elements
			if (!ScriptedTrainElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException();
			}

			ScriptedTrain Train = new ScriptedTrain(TrainState.Pending);

			foreach (XElement Element in ScriptedTrainElements)
			{
				ParseScriptedTrainNode(ObjectPath, FileName, Element, Train);
			}

			return Train;
		}

		/// <summary>Parses a base track following object node</summary>
		/// <param name="ObjectPath">Absolute path to the object folder of route data</param>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="SectionElement">The XElement to parse</param>
		/// <param name="Train">The track following object to parse this node into</param>
		private static void ParseScriptedTrainNode(string ObjectPath, string FileName, XElement SectionElement, ScriptedTrain Train)
		{
			string Section = SectionElement.Name.LocalName;

			string TrainDirectory = string.Empty;
			bool ConsistReversed = false;
			List<TravelData> Data = new List<TravelData>();

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

			if (!(Data.First() is TravelStopData) || !(Data.Last() is TravelStopData))
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
			string TrainData = Path.CombineFile(TrainDirectory, "train.ai");
			if (!File.Exists(TrainData))
			{
				// Check for the standard drivable train.dat
				TrainData = Path.CombineFile(TrainDirectory, "train.dat");
			}
			string ExteriorFile = Path.CombineFile(TrainDirectory, "extensions.cfg");
			if (!File.Exists(TrainData) || !File.Exists(ExteriorFile))
			{
				Interface.AddMessage(MessageType.Error, true, $"The supplied train folder in TrackFollowingObject {FileName} did not contain a complete set of data.");
				return;
			}
			AbstractTrain currentTrain = Train;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(TrainData))
				{
					Program.CurrentHost.Plugins[i].Train.LoadTrain(Encoding.UTF8, TrainDirectory, ref currentTrain, ref Interface.CurrentControls);
				}
			}

			if (!Train.Cars.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"Failed to load the specified train in {FileName}");
				return;
			}

			Train.AI = new TrackFollowingObjectAI(Train, Data.ToArray());
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
		private static void ParseDefinitionNode(string FileName, XElement SectionElement, ScriptedTrain Train)
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
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out Train.AppearanceStartPosition))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
						break;
					case "appearanceendposition":
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out Train.AppearanceEndPosition))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
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
							string TmpPath = Path.CombineDirectory(Path.GetDirectoryName(FileName), Value);
							if (!Directory.Exists(TmpPath))
							{
								TmpPath = Path.CombineFile(Program.FileSystem.InitialTrainFolder, Value);
							}
							if (!Directory.Exists(TmpPath))
							{
								TmpPath = Path.CombineFile(Program.FileSystem.TrainInstallationDirectory, Value);
							}
							if (!Directory.Exists(TmpPath))
							{
								TmpPath = Path.CombineFile(ObjectPath, Value);
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
										if (!NumberFormats.TryParseIntVb6(Value, out int n))
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
		private static void ParseTravelDataNodes(string FileName, XElement SectionElement, ICollection<TravelData> Data)
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
		private static void ParseTravelDataNode(string FileName, XElement SectionElement, TravelData Data)
		{
			string Section = SectionElement.Name.LocalName;

			double Decelerate = 0.0;
			double Accelerate = 0.0;
			double TargetSpeed = 0.0;
			bool targetSpeedSet = false;

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
						if (Value.Any() && !NumberFormats.TryParseDoubleVb6(Value, out Data.Position))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
						}
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
						targetSpeedSet = true;
						break;
					case "rail":
						if (Value.Any() && !NumberFormats.TryParseIntVb6(Value, out Data.RailIndex) || Data.RailIndex < 0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative integer number in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
							Data.RailIndex = 0;
						}

						if (!Program.CurrentRoute.Tracks.ContainsKey(Data.RailIndex) || Program.CurrentRoute.Tracks[Data.RailIndex].Elements.Length == 0)
						{
							Interface.AddMessage(MessageType.Error, false, $"RailIndex is invalid in {Key} in {Section} at line {LineNumber.ToString(culture)} in {FileName}");
							Data.RailIndex = 0;
						}
						break;
				}
			}

			if (!targetSpeedSet)
			{
				Interface.AddMessage(MessageType.Warning, false, $"A TargetSpeed was not set in {Section}. This may cause unexpected results.");
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
		private static TravelStopData ParseTravelStopNode(string FileName, XElement SectionElement)
		{
			string Section = SectionElement.Name.LocalName;

			TravelStopData Data = new TravelStopData();

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
		private static TravelPointData ParseTravelPointNode(string FileName, XElement SectionElement)
		{
			string Section = SectionElement.Name.LocalName;

			TravelPointData Data = new TravelPointData();

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
