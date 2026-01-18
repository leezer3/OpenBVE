//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, S520, The OpenBVE Project
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

using Formats.OpenBve;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using TrainManager.Car;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	internal static class TrackFollowingObjectParser
	{
		private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

		private static TrainBase Train;

		/// <summary>Parses a track following object</summary>
		/// <param name="objectPath">Absolute path to the object folder of route data</param>
		/// <param name="fileName">The XML file to parse</param>
		internal static TrainBase ParseTrackFollowingObject(string objectPath, string fileName)
		{
			// The current XML file to load
			XDocument currentXML = XDocument.Load(fileName, LoadOptions.SetLineInfo);
			List<XElement> scriptedTrainElements = currentXML.XPathSelectElements("/openBVE/TrackFollowingObject").ToList();

			// Check this file actually contains OpenBVE other train definition elements
			if (!scriptedTrainElements.Any())
			{
				// We couldn't find any valid XML, so return false
				throw new InvalidDataException();
			}

			try
			{
				foreach (XElement element in scriptedTrainElements)
				{
					ParseScriptedTrainNode(objectPath, fileName, element);
				}
			}
			catch
			{
				return null;
			}
			return Train;
		}

		/// <summary>Parses a base track following object node</summary>
		/// <param name="objectPath">Absolute path to the object folder of route data</param>
		/// <param name="fileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		private static void ParseScriptedTrainNode(string objectPath, string fileName, XElement sectionElement)
		{
			string trainDirectory = string.Empty;
			bool consistReversed = false;
			List<TravelData> travelData = new List<TravelData>();
			XElement trainNode = null;

			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.Definition:
						Train = new ScriptedTrain(TrainState.Pending);
						ParseDefinitionNode(fileName, keyNode, Train as ScriptedTrain);
						break;
					case TrackFollowingObjectKey.RunInterval:
					case TrackFollowingObjectKey.PreTrain:
						Train = new TrainBase(TrainState.Pending, TrainType.PreTrain);
						NumberFormats.TryParseDoubleVb6(keyNode.Value, out Train.TimetableDelta);
						break;
					case TrackFollowingObjectKey.Train:
						trainNode = keyNode;
						break;
					case TrackFollowingObjectKey.Points:
					case TrackFollowingObjectKey.Stops:
						ParseTravelDataNodes(fileName, keyNode, travelData);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			if (trainNode != null)
			{
				ParseTrainNode(objectPath, fileName, trainNode, ref trainDirectory, ref consistReversed);
			}
			else
			{
				throw new InvalidDataException("No train node specified for scripted train");
			}

			if (Train is ScriptedTrain)
			{

				if (travelData.Count < 2)
				{
					Interface.AddMessage(MessageType.Error, false, $"There must be at least two points to go through in {fileName}");
					return;
				}

				if (!(travelData.First() is TravelStopData) || !(travelData.Last() is TravelStopData))
				{
					Interface.AddMessage(MessageType.Error, false, $"The first and the last point to go through must be the \"Stop\" node in {fileName}");
					return;
				}
			}

			if (string.IsNullOrEmpty(trainDirectory))
			{
				Interface.AddMessage(MessageType.Error, false, $"No train has been specified in {fileName}");
				return;
			}

			string trainData;
			if (!trainDirectory.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase) || !File.Exists(trainDirectory))
			{
				/*
				 * First check for a train.ai file- Functionally identical, but allows for differently configured AI
				 * trains not to show up as drivable
				 */
				trainData = Path.CombineFile(trainDirectory, "train.ai");
				if (!File.Exists(trainData))
				{
					// Check for the standard drivable train.dat
					trainData = Path.CombineFile(trainDirectory, "train.dat");
				}

				string exteriorFile = Path.CombineFile(trainDirectory, "extensions.cfg");
				if (!File.Exists(trainData) || !File.Exists(exteriorFile))
				{
					Interface.AddMessage(MessageType.Error, true,
						$"The supplied train folder in TrackFollowingObject {fileName} does not contain an exterior model.");
					return;
				}
			}
			else
			{
				// MSTS consist
				trainData = trainDirectory;
			}


			AbstractTrain currentTrain = Train;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(trainData))
				{
					Program.CurrentHost.Plugins[i].Train.LoadTrain(Encoding.UTF8, trainDirectory, ref currentTrain, ref Interface.CurrentControls);
				}
			}

			if (!Train.Cars.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"Failed to load the specified train in {fileName}");
				return;
			}

			if (consistReversed)
			{
				Train.Reverse();
			}

			if (Train is ScriptedTrain st)
			{
				Train.AI = new TrackFollowingObjectAI(st, travelData.ToArray());
				foreach (var car in Train.Cars)
				{
					car.FrontAxle.Follower.TrackIndex = travelData[0].RailIndex;
					car.RearAxle.Follower.TrackIndex = travelData[0].RailIndex;
					car.FrontBogie.FrontAxle.Follower.TrackIndex = travelData[0].RailIndex;
					car.FrontBogie.RearAxle.Follower.TrackIndex = travelData[0].RailIndex;
					car.RearBogie.FrontAxle.Follower.TrackIndex = travelData[0].RailIndex;
					car.RearBogie.RearAxle.Follower.TrackIndex = travelData[0].RailIndex;
					Train.PlaceCars(travelData[0].Position);
				}
			}
			else
			{
				Train.AI = new Game.SimpleHumanDriverAI(Train, Interface.CurrentOptions.PrecedingTrainSpeedLimit);
				Train.Specs.DoorOpenMode = DoorMode.Manual;
				Train.Specs.DoorCloseMode = DoorMode.Manual;
			}
		}

		/// <summary>
		/// Function to parse TFO definition
		/// </summary>
		/// <param name="fileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		/// <param name="scriptedTrain">The track following object to parse this node into</param>
		private static void ParseDefinitionNode(string fileName, XElement sectionElement, ScriptedTrain scriptedTrain)
		{
			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.AppearanceTime:
						if (value.Any() && !Interface.TryParseTime(value, out scriptedTrain.AppearanceTime))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.AppearanceStartPosition:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out scriptedTrain.AppearanceStartPosition))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.AppearanceEndPosition:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out scriptedTrain.AppearanceEndPosition))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.LeaveTime:
						if (value.Any() && !Interface.TryParseTime(value, out scriptedTrain.LeaveTime))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}
		}

		/// <summary>
		/// Function to parse train definition
		/// </summary>
		/// <param name="objectPath">Absolute path to the object folder of route data</param>
		/// <param name="FileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		/// <param name="trainDirectory">Absolute path to the train directory</param>
		/// <param name="consistReversed">Whether to reverse the train composition.</param>
		private static void ParseTrainNode(string objectPath, string FileName, XElement sectionElement, ref string trainDirectory, ref bool consistReversed)
		{
	
			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.Directory:
						string TmpPath;
						try
						{
							TmpPath = Path.CombineDirectory(Path.GetDirectoryName(FileName), value);

							if (!Directory.Exists(TmpPath) && value.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
							{
								// potential MSTS consist
								string consistDirectory = Path.CombineDirectory(Program.FileSystem.MSTSDirectory, "TRAINS\\Consists");
								string consistFile = Path.CombineFile(consistDirectory, value);
								if (File.Exists(consistFile))
								{
									trainDirectory = consistFile;
									break;
								}

							}

							if (!Directory.Exists(TmpPath))
							{
								TmpPath = Path.CombineFile(Program.FileSystem.InitialTrainFolder, value);
							}

							if (!Directory.Exists(TmpPath))
							{
								TmpPath = Path.CombineFile(Program.FileSystem.TrainInstallationDirectory, value);
							}

							if (!Directory.Exists(TmpPath))
							{
								TmpPath = Path.CombineFile(objectPath, value);
							}

							if (!Directory.Exists(TmpPath))
							{
								// very fuzzy match attempt- step backwards one level from the current train folder
								TmpPath = Path.CombineDirectory(Loading.CurrentTrainFolder, "..");
								TmpPath = Path.CombineFile(TmpPath, value);
							}
						}
						catch
						{
							Interface.AddMessage(MessageType.Error, false, $"Directory was invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {FileName}");
							break;
						}

						if (!Directory.Exists(TmpPath))
						{
							Interface.AddMessage(MessageType.Error, false, $"Directory was not found in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {FileName}");
						}
						else
						{
							trainDirectory = TmpPath;
						}
						break;
					case TrackFollowingObjectKey.Reversed:
						if (value.Any())
						{
							switch (value.ToLowerInvariant())
							{
								case "true":
									consistReversed = true;
									break;
								case "false":
									consistReversed = false;
									break;
								default:
									{
										if (!NumberFormats.TryParseIntVb6(value, out int n))
										{
											Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {FileName}");
										}
										else
										{
											consistReversed = Convert.ToBoolean(n);
										}
									}
									break;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {FileName}");
						break;
				}
			}
		}

		/// <summary>Parses a train travel data node</summary>
		/// <param name="fileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		/// <param name="travelData">The list of travel data to add this to</param>
		private static void ParseTravelDataNodes(string fileName, XElement sectionElement, ICollection<TravelData> travelData)
		{
			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.Stop:
						travelData.Add(ParseTravelStopNode(fileName, keyNode));
						break;
					case TrackFollowingObjectKey.Point:
						travelData.Add(ParseTravelPointNode(fileName, keyNode));
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}
		}

		/// <summary>
		/// Function to parse the contents of TravelData class
		/// </summary>
		/// <param name="fileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		/// <param name="travelData">Travel data to which the parse results apply</param>
		private static void ParseTravelDataNode(string fileName, XElement sectionElement, TravelData travelData)
		{
			double decelerate = 0.0;
			double accelerate = 0.0;
			double targetSpeed = 0.0;
			bool targetSpeedSet = false;

			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.Decelerate:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out decelerate) || decelerate < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.Position:
					case TrackFollowingObjectKey.StopPosition:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out travelData.Position))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.Accelerate:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out accelerate) || accelerate < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.TargetSpeed:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out targetSpeed) || targetSpeed < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						targetSpeedSet = true;
						break;
					case TrackFollowingObjectKey.Rail:
						if (value.Any() && !NumberFormats.TryParseIntVb6(value, out travelData.RailIndex) || travelData.RailIndex < 0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative integer number in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
							travelData.RailIndex = 0;
						}

						if (!Program.CurrentRoute.Tracks.ContainsKey(travelData.RailIndex) || Program.CurrentRoute.Tracks[travelData.RailIndex].Elements.Length == 0)
						{
							Interface.AddMessage(MessageType.Error, false, $"RailIndex is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
							travelData.RailIndex = 0;
						}
						break;
				}
			}

			if (!targetSpeedSet)
			{
				Interface.AddMessage(MessageType.Warning, false, $"A TargetSpeed was not set in {sectionElement.Name.LocalName}. This may cause unexpected results.");
			}
			travelData.Decelerate = -decelerate / 3.6;
			travelData.Accelerate = accelerate / 3.6;
			travelData.TargetSpeed = targetSpeed / 3.6;
		}

		/// <summary>
		/// Function to parse the contents of TravelStopData class
		/// </summary>
		/// <param name="fileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		/// <returns>An instance of the new TravelStopData class with the parse result applied</returns>
		private static TravelStopData ParseTravelStopNode(string fileName, XElement sectionElement)
		{
			TravelStopData travelStopData = new TravelStopData();

			ParseTravelDataNode(fileName, sectionElement, travelStopData);

			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.StopTime:
						if (value.Any() && !Interface.TryParseTime(value, out travelStopData.StopTime))
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.Doors:
						{
							int doorSide = 0;
							bool doorBoth = false;

							switch (value.ToLowerInvariant())
							{
								case "l":
								case "left":
									doorSide = -1;
									break;
								case "r":
								case "right":
									doorSide = 1;
									break;
								case "n":
								case "none":
								case "neither":
									doorSide = 0;
									break;
								case "b":
								case "both":
									doorBoth = true;
									break;
								default:
									if (value.Any() && !NumberFormats.TryParseIntVb6(value, out doorSide))
									{
										Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
									}
									break;
							}

							travelStopData.OpenLeftDoors = doorSide < 0.0 | doorBoth;
							travelStopData.OpenRightDoors = doorSide > 0.0 | doorBoth;
						}
						break;
					case TrackFollowingObjectKey.Direction:
						{
							int d = 0;
							switch (value.ToLowerInvariant())
							{
								case "f":
									d = 1;
									break;
								case "r":
									d = -1;
									break;
								default:
									if (value.Any() && (!NumberFormats.TryParseIntVb6(value, out d) || !Enum.IsDefined(typeof(TravelDirection), d)))
									{
										Interface.AddMessage(MessageType.Error, false, $"Value is invalid in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
										d = 1;
									}
									break;
							}

							travelStopData.Direction = (TravelDirection)d;
						}
						break;
					case TrackFollowingObjectKey.Decelerate:
					case TrackFollowingObjectKey.Position:
					case TrackFollowingObjectKey.StopPosition:
					case TrackFollowingObjectKey.Accelerate:
					case TrackFollowingObjectKey.TargetSpeed:
					case TrackFollowingObjectKey.Rail:
						// Already parsed
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return travelStopData;
		}

		/// <summary>
		/// Function to parse the contents of TravelPointData class
		/// </summary>
		/// <param name="fileName">The filename of the containing XML file</param>
		/// <param name="sectionElement">The XElement to parse</param>
		/// <returns>An instance of the new TravelPointData class with the parse result applied</returns>
		private static TravelPointData ParseTravelPointNode(string fileName, XElement sectionElement)
		{
			TravelPointData travelPointData = new TravelPointData();

			ParseTravelDataNode(fileName, sectionElement, travelPointData);

			double passingSpeed = 0.0;

			foreach (XElement keyNode in sectionElement.Elements())
			{
				Enum.TryParse(keyNode.Name.LocalName, true, out TrackFollowingObjectKey key);
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key)
				{
					case TrackFollowingObjectKey.PassingSpeed:
						if (value.Any() && !NumberFormats.TryParseDoubleVb6(value, out passingSpeed) || passingSpeed < 0.0)
						{
							Interface.AddMessage(MessageType.Error, false, $"Value is expected to be a non-negative floating-point number in {key} in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						break;
					case TrackFollowingObjectKey.Decelerate:
					case TrackFollowingObjectKey.Position:
					case TrackFollowingObjectKey.StopPosition:
					case TrackFollowingObjectKey.Accelerate:
					case TrackFollowingObjectKey.TargetSpeed:
					case TrackFollowingObjectKey.Rail:
						// Already parsed
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {sectionElement.Name.LocalName} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			travelPointData.PassingSpeed = passingSpeed / 3.6;

			return travelPointData;
		}
	}
}
