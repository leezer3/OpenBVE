//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
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
using System.Linq;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void LoadScriptedTrain(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly || Plugin.CurrentHost.Application != HostApplication.OpenBve)
			{
				return;
			}

			List<ScriptedTrain> OtherTrains = new List<ScriptedTrain>();

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Train)
				{
					continue;
				}

				switch (Statement.FunctionName)
				{
					case MapFunctionName.Add:
					case MapFunctionName.Load:
						string TrainKey;

						if (Statement.FunctionName == MapFunctionName.Add)
						{
							if (!Statement.HasArgument(ArgumentName.TrainKey))
							{
								continue;
							}
							TrainKey = Statement.GetArgumentValueAsString(ArgumentName.TrainKey);
						}
						else
						{
							TrainKey = Statement.Key;
						}
						
						if (!Statement.HasArgument(ArgumentName.FilePath))
						{
							continue;
						}

						string TrainFilePath = Statement.GetArgumentValueAsString(ArgumentName.FilePath);
						if (string.IsNullOrEmpty(TrainFilePath))
						{
							continue;
						}

						string TrackKey = !Statement.HasArgument(ArgumentName.TrackKey) ? "0" : Statement.GetArgumentValueAsString(ArgumentName.TrackKey);

						if (string.IsNullOrEmpty(TrackKey))
						{
							TrackKey = "0";
						}

						if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Attempted to place ScriptedTrain " + Statement.Key + " on the non-existent track " + TrackKey + " at track position " + Statement.Distance + "m");
							TrackKey = "0";
						}

						if (!Statement.HasArgument(ArgumentName.Direction) || !NumberFormats.TryParseIntVb6(Statement.GetArgumentValueAsString(ArgumentName.Direction), out int Direction))
						{
							Direction = 1;
						}
						TrainFilePath = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Convert.ToString(TrainFilePath));
						if (!File.Exists(Convert.ToString(TrainFilePath)))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Convert.ToString(TrainFilePath) + "is not found.");
							continue;
						}

						OtherTrains.Add(new ScriptedTrain
						{
							Key = TrainKey,
							FilePath = TrainFilePath,
							TrackKey = TrackKey,
							Direction = Direction
						});
						break;
					default:
						continue;
				}
			}

			foreach (var Statement in ParseData.Statements)
			{
				dynamic d = Statement;
				if (Statement.ElementName != MapElementName.Train || Statement.FunctionName != MapFunctionName.SetTrack)
				{
					continue;
				}

				string TrackKey = d.TrackKey;
				object TrainKey = Statement.Key;

				if (string.IsNullOrEmpty(TrackKey))
				{
					TrackKey = "0";
				}

				if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Attempted to place waypoint for ScriptedTrain " + Statement.Key + " on the non-existent track " + TrackKey + " at track position " + Statement.Distance + "m");
					TrackKey = "0";
				}

				if (!Statement.HasArgument(ArgumentName.Direction) || !NumberFormats.TryParseIntVb6(Statement.GetArgumentValueAsString(ArgumentName.Direction), out int Direction))
				{
					Direction = 1;
				}

				int TrainIndex = OtherTrains.FindIndex(Train => Train.Key.Equals(Convert.ToString(TrainKey), StringComparison.InvariantCultureIgnoreCase));
				if (TrainIndex == -1)
				{
					continue;
				}

				OtherTrains[TrainIndex].TrackKey = TrackKey;
				OtherTrains[TrainIndex].Direction = Direction;
			}

			foreach (var OtherTrain in OtherTrains)
			{
				if (!RouteData.TrackKeyList.Contains(OtherTrain.TrackKey, StringComparer.OrdinalIgnoreCase))
				{
					continue;
				}
				
				ParseScriptedTrain(OtherTrain);

				if (!OtherTrain.CarObjects.Any())
				{
					continue;
				}

				OtherTrain.CarObjects = OtherTrain.CarObjects.OrderByDescending(Object => Object.Distance).ToList();

				TrainManager.Trains.ScriptedTrain Train = new TrainManager.Trains.ScriptedTrain(TrainState.Pending);
				Train.Cars = new CarBase[OtherTrain.CarObjects.Count];
				Train.Handles.Reverser = new ReverserHandle(Train);

				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Train.Cars[i] = new CarBase(Train, i);
					Train.Cars[i].FrontAxle.Follower.TriggerType = i == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
					Train.Cars[i].RearAxle.Follower.TriggerType = i == OtherTrain.CarObjects.Count - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
					Train.Cars[i].BeaconReceiver.TriggerType = i == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
					Train.Cars[i].FrontAxle.Position = OtherTrain.CarObjects[i].Span - OtherTrain.CarObjects[i].Z;
					Train.Cars[i].RearAxle.Position = -OtherTrain.CarObjects[i].Z;
					Train.Cars[i].Doors[0] = new Door(-1, 10000, 10000);
					Train.Cars[i].Doors[1] = new Door(1, 10000, 10000);
					Train.Cars[i].Width = 2.6;
					Train.Cars[i].Height = 3.2;
					Train.Cars[i].Coupler = new Coupler(0.05, 0.1, Train.Cars[i], i < Train.Cars.Length - 1 ? Train.Cars[i + 1] : null);
					if (i + 1 < Train.Cars.Length)
					{
						Train.Cars[i].Length = OtherTrain.CarObjects[i].Distance - OtherTrain.CarObjects[i + 1].Distance;
					}
					else if (i > 0)
					{
						Train.Cars[i].Length = Train.Cars[i - 1].Length;
					}
					else
					{
						Train.Cars[i].Length = OtherTrain.CarObjects[i].Span;
					}
					Train.Cars[i].Specs.CenterOfGravityHeight = 1.5;
					Train.Cars[i].Specs.CriticalTopplingAngle = 0.5 * Math.PI - Math.Atan(2 * Train.Cars[i].Specs.CenterOfGravityHeight / Train.Cars[i].Width);

					RouteData.Objects.TryGetValue(OtherTrain.CarObjects[i].Key, out UnifiedObject CarObject);
					if (CarObject != null)
					{
						Train.Cars[i].CarSections.Add(CarSectionType.Exterior, new CarSection(Plugin.CurrentHost, ObjectType.Dynamic, false, Train.Cars[i]));
					}
				}

				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Train.Cars[i].TractionModel = new BVEMotorCar(Train.Cars[i], null);
					BVE5AITrainSounds carSounds = new BVE5AITrainSounds(Train.Cars[i], new BVE5AISoundEntry[OtherTrain.CarSounds.Count]);
					for (int j = 0; j < carSounds.SoundEntries.Length; j++)
					{
						RouteData.Sound3Ds.TryGetValue(OtherTrain.CarSounds[j].Key, out SoundHandle soundHandle);
						carSounds.SoundEntries[j] = new BVE5AISoundEntry(soundHandle as SoundBuffer, OtherTrain.CarSounds[j].Function);
					}

					Train.Cars[i].TractionModel.MotorSounds = carSounds;
				}

				List<TravelData> Data = new List<TravelData>();

				foreach (var Statement in ParseData.Statements)
				{
					if (Statement.ElementName != MapElementName.Train || Statement.Key != OtherTrain.Key)
					{
						continue;
					}

					switch (Statement.FunctionName)
					{
						case MapFunctionName.Enable:
							{
								TryParseBve5Time(Convert.ToString(Statement.GetArgumentValue(ArgumentName.Time)), out double Time);
								Train.AppearanceStartPosition = Statement.Distance;
								Train.AppearanceTime = Time;
							}
							break;
						case MapFunctionName.Stop:
							{
								object Decelerate = Statement.GetArgumentValue(ArgumentName.Decelerate);
								object StopTime = Statement.GetArgumentValue(ArgumentName.StopTime);
								object Accelerate = Statement.GetArgumentValue(ArgumentName.Accelerate);
								object Speed = Statement.GetArgumentValue(ArgumentName.Speed);	

								Data.Add(new TravelStopData
								{
									Decelerate = Convert.ToDouble(Decelerate) / 3.6,
									Position = Statement.Distance,
									StopTime = Convert.ToDouble(StopTime),
									Accelerate = Convert.ToDouble(Accelerate) / 3.6,
									TargetSpeed = Convert.ToDouble(Speed) / 3.6,
									Direction = (TravelDirection)OtherTrain.Direction,
									RailIndex = RouteData.TrackKeyList.IndexOf(OtherTrain.TrackKey, StringComparison.OrdinalIgnoreCase)
								});
							}
							break;
					}
				}

				if (OtherTrain.Direction == -1)
				{
					Data = Data.OrderByDescending(d => d.Position).ToList();
				}

				if (!Data.Any())
				{
					continue;
				}

				Train.AI = new TrackFollowingObjectAI(Train, Data.ToArray());

				// For debug
				Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"[{OtherTrain.Key}] 走行軌道: {OtherTrain.TrackKey}, 進行方向: {OtherTrain.Direction}, 有効開始位置: {Train.AppearanceStartPosition}m, 有効開始時刻: {Train.AppearanceTime}s");

				for (int i = 0; i < Data.Count; i++)
				{
					if (Data[i] is TravelStopData d)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"[{OtherTrain.Key}] 停車位置: {d.Position}m, 減速度: {d.Decelerate * 3.6}km/h/s, 停車時間: {d.StopTime}s, 加速度: {d.Accelerate * 3.6}km/h/s, 加速後の走行速度: {d.TargetSpeed * 3.6}km/h");
					}
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

				Train.PlaceCars(Data[0].Position);
				Plugin.TrainManager.TFOs.Add(Train);
			}
		}

		private static void ParseScriptedTrain(ScriptedTrain scriptedTrain)
		{
			scriptedTrain.CarObjects = new List<CarObject>();
			scriptedTrain.CarSounds = new List<CarSound>();

			System.Text.Encoding Encoding = Text.DetermineBVE5FileEncoding(scriptedTrain.FilePath);

			string[] Lines = File.ReadAllLines(scriptedTrain.FilePath, Encoding).Skip(1).ToArray();
			List<Dictionary<string, string>> Structures = new List<Dictionary<string, string>>();
			List<Dictionary<string, string>> Sound3ds = new List<Dictionary<string, string>>();
			string Section = string.Empty;

			for (int i = 0; i < Lines.Length; i++)
			{
				Lines[i] = Lines[i].Trim();

				if (!Lines[i].Any() || Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase) || Lines[i].StartsWith("#", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
				{
					Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();

					switch (Section)
					{
						case "structure":
							Structures.Add(new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));
							break;
						case "sound3d":
							Sound3ds.Add(new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));
							break;
					}
				}
				else
				{
					int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
					string Key, Value;
					if (j >= 0)
					{
						Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
						Value = Lines[i].Substring(j + 1).TrimStart().ToLowerInvariant();
					}
					else
					{
						Key = string.Empty;
						Value = Lines[i];
					}

					switch (Section)
					{
						case "structure":
							Structures.Last()[Key] = Value;
							break;
						case "sound3d":
							Sound3ds.Last()[Key] = Value;
							break;
					}
				}
			}

			foreach (var Structure in Structures)
			{
				Structure.TryGetValue("key", out string Key);
				Structure.TryGetValue("distance", out string TempDistance);
				Structure.TryGetValue("span", out string TempSpan);
				Structure.TryGetValue("z", out string TempZ);

				if (string.IsNullOrEmpty(TempDistance) || !NumberFormats.TryParseDoubleVb6(TempDistance, out double Distance))
				{
					Distance = 0.0;
				}
				if (string.IsNullOrEmpty(TempSpan) || !NumberFormats.TryParseDoubleVb6(TempSpan, out double Span))
				{
					Span = 0.0;
				}
				if (string.IsNullOrEmpty(TempZ) || !NumberFormats.TryParseDoubleVb6(TempZ, out double Z))
				{
					Z = 0.0;
				}

				scriptedTrain.CarObjects.Add(new CarObject(Key, Distance, Span, Z));
			}

			foreach (var Sound3d in Sound3ds)
			{
				Sound3d.TryGetValue("key", out string Key);
				Sound3d.TryGetValue("distance1", out string TempDistance1);
				Sound3d.TryGetValue("distance2", out string TempDistance2);
				Sound3d.TryGetValue("function", out string Function);

				Enum.TryParse(Function, true, out BVE5AISoundControl function);

				if (string.IsNullOrEmpty(TempDistance1) || !NumberFormats.TryParseDoubleVb6(TempDistance1, out double Distance1))
				{
					Distance1 = 0.0;
				}
				if (string.IsNullOrEmpty(TempDistance2) || !NumberFormats.TryParseDoubleVb6(TempDistance2, out double Distance2))
				{
					Distance2 = 0.0;
				}

				scriptedTrain.CarSounds.Add(new CarSound(Key, Distance1, Distance2, function));
			}
		}
	}
}
