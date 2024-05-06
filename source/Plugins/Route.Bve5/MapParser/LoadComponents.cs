using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using CsvHelper;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void LoadStationList(string FileName, MapData ParseData, RouteData RouteData)
		{
			RouteData.StationList = new List<Station>();
			// Everything breaks if no station list
			if (string.IsNullOrEmpty(ParseData.StationListPath))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, true, "No BVE5 Station List file was specified.");
				return;
			}

			string stationList = ParseData.StationListPath;
			
			if (!File.Exists(stationList))
			{
				stationList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), stationList);

				if (!File.Exists(stationList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Station List file "+ stationList + " was not found.");
					return;
				}
			}
			
			System.Text.Encoding Encoding = DetermineFileEncoding(stationList);
			string[] Lines = File.ReadAllLines(stationList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			using (CsvReader Csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, Lines))))
			{
				Csv.Configuration.AllowComments = true;
				while (Csv.Read())
				{
					Station newStation = new Station();

					for (int i = 0; i < Csv.CurrentRecord.Length; i++)
					{
						string Value = string.Empty;
						if (Csv.CurrentRecord[i] != null)
						{
							Value = Csv.CurrentRecord[i].Split('#')[0].Trim();
						}

						switch (i)
						{
							case 0:
								newStation.Key = Value;
								break;
							case 1:
								newStation.Name = Value;
								break;
							case 2:
								if (Value.Equals("p", StringComparison.InvariantCultureIgnoreCase))
								{
									newStation.StopMode = StationStopMode.AllPass;
								}

								if (!TryParseBve5Time(Value, out newStation.ArrivalTime))
								{
									newStation.ArrivalTime = -1.0;
								}
								break;
							case 3:
								if (Value.Equals("t", StringComparison.InvariantCultureIgnoreCase))
								{
									newStation.StationType = StationType.Terminal;
								}

								if (!TryParseBve5Time(Value, out newStation.DepartureTime))
								{
									newStation.DepartureTime = -1.0;
								}
								break;
							case 4:
								{
									if (!NumberFormats.TryParseDoubleVb6(Value, out double stopTime))
									{
										stopTime = 15.0;
									}
									else if (stopTime < 5.0)
									{
										stopTime = 5.0;
									}

									newStation.StopTime = stopTime;
								}
								break;
							case 5:
								if (!TryParseBve5Time(Value, out newStation.DefaultTime))
								{
									newStation.DefaultTime = -1.0;
								}
								break;
							case 6:
								{
									if (!NumberFormats.TryParseIntVb6(Value, out int signalFlag))
									{
										signalFlag = 0;
									}

									newStation.ForceStopSignal = signalFlag == 1;
								}
								break;
							case 7:
								{
									if (!NumberFormats.TryParseDoubleVb6(Value, out double alightingTime))
									{
										alightingTime = 0.0;
									}

									newStation.AlightingTime = alightingTime;
								}
								break;
							case 8:
								{
									if (!NumberFormats.TryParseDoubleVb6(Value, out double passengerRatio))
									{
										passengerRatio = 100.0;
									}

									newStation.PassengerRatio = passengerRatio / 100.0;
								}
								break;
							case 9:
								newStation.ArrivalSoundKey = Value;
								break;
							case 10:
								newStation.DepartureSoundKey = Value;
								break;
							case 11:
								{
									if (!NumberFormats.TryParseDoubleVb6(Value, out double reopenDoor) || reopenDoor < 0.0)
									{
										reopenDoor = 0.0;
									}

									newStation.ReopenDoor = reopenDoor / 100.0;
								}
								break;
							case 12:
								{
									if (!NumberFormats.TryParseDoubleVb6(Value, out double interferenceInDoor) || interferenceInDoor < 0.0)
									{
										interferenceInDoor = 0.0;
									}

									newStation.InterferenceInDoor = interferenceInDoor;
								}
								break;
						}
					}

					RouteData.StationList.Add(newStation);
				}
			}
		}

		private static void LoadStructureList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Objects = new ObjectDictionary();
			
			if (PreviewOnly || string.IsNullOrEmpty(ParseData.StructureListPath))
			{
				return;
			}

			string structureList = ParseData.StructureListPath;

			if (!File.Exists(structureList))
			{
				structureList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), structureList);

				if (!File.Exists(structureList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Structure List file " + structureList + " was not found.");
					return;
				}
			}

			string BaseDirectory = System.IO.Path.GetDirectoryName(structureList);

			System.Text.Encoding Encoding = DetermineFileEncoding(structureList);
			string[] Lines = File.ReadAllLines(structureList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			using (CsvReader Csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, Lines))))
			{
				Csv.Configuration.AllowComments = true;
				while (Csv.Read())
				{
					string Key = string.Empty;
					string ObjectFileName = string.Empty;

					for (int i = 0; i < Csv.CurrentRecord.Length; i++)
					{
						string Value = string.Empty;
						if (Csv.CurrentRecord[i] != null)
						{
							Value = Csv.CurrentRecord[i].Split('#')[0].Trim();
						}

						switch (i)
						{
							case 0:
								Key = Value;
								break;
							case 1:
								ObjectFileName = Value;
								break;
						}
					}

					try
					{
						ObjectFileName = Path.CombineFile(BaseDirectory, ObjectFileName);
					}
					catch
					{
						// ignored
					}

					if (!File.Exists(ObjectFileName))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false,  "The Object File " + ObjectFileName + " with key "+ Key + " was not found.");
						continue;
					}

					System.Text.Encoding ObjectEncoding = TextEncoding.GetSystemEncodingFromFile(ObjectFileName);
					UnifiedObject obj;
					Plugin.CurrentHost.LoadObject(ObjectFileName, ObjectEncoding, out obj);
					RouteData.Objects.Add(Key, obj);
				}
			}
		}

		private static void LoadSignalList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.SignalObjects = new List<SignalData>();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.SignalListPath))
			{
				return;
			}

			string signalList = ParseData.SignalListPath;

			if (!File.Exists(signalList))
			{
				signalList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), signalList);

				if (!File.Exists(signalList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Signal List file " + signalList + " was not found.");
					return;
				}
			}
			
			System.Text.Encoding Encoding = DetermineFileEncoding(signalList);
			string[] Lines = File.ReadAllLines(signalList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			using (CsvReader Csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, Lines))))
			{
				Csv.Configuration.AllowComments = true;
				while (Csv.Read())
				{
					string Key = string.Empty;
					List<int> Numbers = new List<int>();
					List<string> ObjectKeys = new List<string>();

					for (int i = 0; i < Csv.CurrentRecord.Length; i++)
					{
						string Value = string.Empty;
						if (Csv.CurrentRecord[i] != null)
						{
							Value = Csv.CurrentRecord[i].Split('#')[0].Trim();
						}

						switch (i)
						{
							case 0:
								Key = Value;
								break;
							default:
								if (!string.IsNullOrEmpty(Value))
								{
									Numbers.Add(i - 1);
									ObjectKeys.Add(Value);
								}
								break;
						}
					}

					List<StaticObject> Objects = new List<StaticObject>();
					foreach (var ObjectKey in ObjectKeys)
					{
						UnifiedObject Object;
						RouteData.Objects.TryGetValue(ObjectKey, out Object);
						if (Object != null)
						{
							Objects.Add((StaticObject)Object);
						}
						else
						{
							Objects.Add(new StaticObject(Plugin.CurrentHost));
						}
					}

					if (!string.IsNullOrEmpty(Key))
					{
						RouteData.SignalObjects.Add(new SignalData
						{
							Key = Key,
							Numbers = Numbers.ToArray(),
							BaseObjects = Objects.ToArray()
						});
					}
					else
					{
						if (RouteData.SignalObjects.Any())
						{
							RouteData.SignalObjects.Last().GlowObjects = Objects.ToArray();
						}
					}
				}
			}
		}

		private static void LoadSoundList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Sounds = new SoundDictionary();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.SoundListPath))
			{
				return;
			}

			string soundList = ParseData.SoundListPath;
			if (!File.Exists(soundList))
			{
				soundList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), soundList);

				if (!File.Exists(soundList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Sound List file " + soundList + " was not found.");
					return;
				}
			}
			
			string BaseDirectory = System.IO.Path.GetDirectoryName(soundList);

			System.Text.Encoding Encoding = DetermineFileEncoding(soundList);
			string[] Lines = File.ReadAllLines(soundList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			using (CsvReader Csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, Lines))))
			{
				Csv.Configuration.AllowComments = true;
				while (Csv.Read())
				{
					string Key = string.Empty;
					string SoundFileName = string.Empty;

					for (int i = 0; i < Csv.CurrentRecord.Length; i++)
					{
						string Value = string.Empty;
						if (Csv.CurrentRecord[i] != null)
						{
							Value = Csv.CurrentRecord[i].Split('#')[0].Trim();
						}

						switch (i)
						{
							case 0:
								Key = Value;
								break;
							case 1:
								SoundFileName = Value;
								break;
						}
					}

					try
					{
						SoundFileName = Path.CombineFile(BaseDirectory, SoundFileName);
					}
					catch
					{
						// ignored
					}

					if (!File.Exists(SoundFileName))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The Sound File " + SoundFileName + " with key " + Key + " was not found.");
						continue;
					}

					Plugin.CurrentHost.RegisterSound(SoundFileName, 15.0, out OpenBveApi.Sounds.SoundHandle handle);
					RouteData.Sounds.Add(Key, handle);
				}
			}
		}

		private static void LoadSound3DList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Sound3Ds = new SoundDictionary();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.Sound3DListPath))
			{
				return;
			}

			string sound3DList = ParseData.Sound3DListPath;

			if (!File.Exists(sound3DList))
			{
				sound3DList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), sound3DList);

				if (!File.Exists(sound3DList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Sound3D List file " + sound3DList + " was not found.");
					return;
				}
			}
			
			string BaseDirectory = System.IO.Path.GetDirectoryName(sound3DList);

			System.Text.Encoding Encoding = DetermineFileEncoding(sound3DList);
			string[] Lines = File.ReadAllLines(sound3DList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			using (CsvReader Csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, Lines))))
			{
				Csv.Configuration.AllowComments = true;
				while (Csv.Read())
				{
					string Key = string.Empty;
					string SoundFileName = string.Empty;

					for (int i = 0; i < Csv.CurrentRecord.Length; i++)
					{
						string Value = string.Empty;
						if (Csv.CurrentRecord[i] != null)
						{
							Value = Csv.CurrentRecord[i].Split('#')[0].Trim();
						}

						switch (i)
						{
							case 0:
								Key = Value;
								break;
							case 1:
								SoundFileName = Value;
								break;
						}
					}

					try
					{
						SoundFileName = Path.CombineFile(BaseDirectory, SoundFileName);
					}
					catch
					{
						// ignored
					}

					if (!File.Exists(SoundFileName))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The Sound3D File " + SoundFileName + " with key " + Key + " was not found.");
						continue;
					}

					Plugin.CurrentHost.RegisterSound(SoundFileName, 15.0, out OpenBveApi.Sounds.SoundHandle handle);
					RouteData.Sound3Ds.Add(Key, handle);
				}
			}
		}

		private static void LoadOtherTrain(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			List<OtherTrain> OtherTrains = new List<OtherTrain>();

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
						string TrainKey, TrainFilePath, TrackKey = Statement.Key;
						int Direction;

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
						
						if (!Statement.HasArgument(ArgumentName.TrainFilePath))
						{
							continue;
						}

						TrainFilePath = Statement.GetArgumentValueAsString(ArgumentName.TrainFilePath);
						if (string.IsNullOrEmpty(TrainFilePath))
						{
							continue;
						}

						if (!Statement.HasKey || string.IsNullOrEmpty(Statement.Key))
						{
							TrackKey = "0";
						}

						if (!Statement.HasArgument(ArgumentName.Direction) || !int.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Direction), out Direction))
						{
							Direction = 1;
						}
						TrainFilePath = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Convert.ToString(TrainFilePath));
						if (!File.Exists(Convert.ToString(TrainFilePath)))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, Convert.ToString(TrainFilePath) + "is not found.");
							continue;
						}

						OtherTrains.Add(new OtherTrain
						{
							Key = Convert.ToString(TrainKey),
							FilePath = Convert.ToString(TrainFilePath),
							TrackKey = Convert.ToString(TrackKey),
							Direction = Convert.ToInt32(Direction)
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
				int Direction;
				object TrainKey = Statement.Key;

				if (string.IsNullOrEmpty(TrackKey))
				{
					TrackKey = "0";
				}
				if (!Statement.HasArgument(ArgumentName.Direction) || !int.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Direction), out Direction))
				{
					Direction = 1;
				}

				int TrainIndex = OtherTrains.FindIndex(Train => Train.Key.Equals(Convert.ToString(TrainKey), StringComparison.InvariantCultureIgnoreCase));
				if (TrainIndex == -1)
				{
					continue;
				}

				OtherTrains[TrainIndex].TrackKey = Convert.ToString(TrackKey);
				OtherTrains[TrainIndex].Direction = Convert.ToInt32(Direction);
			}

			foreach (var OtherTrain in OtherTrains)
			{
				int RailIndex = RouteData.TrackKeyList.IndexOf(OtherTrain.TrackKey);
				if (RailIndex == -1)
				{
					continue;
				}

				ParseOtherTrain(OtherTrain);

				if (!OtherTrain.CarObjects.Any())
				{
					continue;
				}

				OtherTrain.CarObjects = OtherTrain.CarObjects.OrderByDescending(Object => Object.Distance).ToList();

				ScriptedTrain Train = new ScriptedTrain(TrainState.Pending);
				Train.Cars = new CarBase[OtherTrain.CarObjects.Count];
				Train.Handles.Reverser = new ReverserHandle(Train);

				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Train.Cars[i] = new CarBase(Train, i);
					Train.Cars[i].CurrentCarSection = -1;
					Train.Cars[i].ChangeCarSection(CarSectionType.NotVisible);
					Train.Cars[i].FrontBogie.ChangeSection(-1);
					Train.Cars[i].RearBogie.ChangeSection(-1);
					Train.Cars[i].FrontAxle.Follower.TriggerType = i == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
					Train.Cars[i].RearAxle.Follower.TriggerType = i == OtherTrain.CarObjects.Count - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
					Train.Cars[i].BeaconReceiver.TriggerType = i == 0 ? EventTriggerType.TrainFront : EventTriggerType.None;
					Train.Cars[i].FrontAxle.Position = OtherTrain.CarObjects[i].Span - OtherTrain.CarObjects[i].Z;
					Train.Cars[i].RearAxle.Position = -OtherTrain.CarObjects[i].Z;
					Train.Cars[i].Doors[0] = new Door(-1, 10000, 10000);
					Train.Cars[i].Doors[1] = new Door(1, 10000, 10000);
					Train.Cars[i].Width = 2.6;
					Train.Cars[i].Height = 3.2;
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

					UnifiedObject CarObject;
					RouteData.Objects.TryGetValue(OtherTrain.CarObjects[i].Key, out CarObject);
					if (CarObject != null)
					{
						Train.Cars[i].LoadCarSections(CarObject, false);
					}
				}

				List<TravelData> Data = new List<TravelData>();

				foreach (var Statement in ParseData.Statements)
				{
					double d = Statement.Distance;
					if (Statement.ElementName != MapElementName.Train || Statement.Key != OtherTrain.Key)
					{
						continue;
					}

					switch (Statement.FunctionName)
					{
						case MapFunctionName.Enable:
							{
								object TempTime;
								double Time;
								TempTime = Statement.GetArgumentValue(ArgumentName.Time);

								TryParseBve5Time(Convert.ToString(TempTime), out Time);

								Train.AppearanceStartPosition = Statement.Distance;
								Train.AppearanceTime = Time;
							}
							break;
						case MapFunctionName.Stop:
							{
								object Decelerate, StopTime, Accelerate, Speed;
								Decelerate = Statement.GetArgumentValue(ArgumentName.Decelerate);
								StopTime = Statement.GetArgumentValue(ArgumentName.StopTime);
								Accelerate = Statement.GetArgumentValue(ArgumentName.Accelerate);
								Speed = Statement.GetArgumentValue(ArgumentName.Speed);	

								Data.Add(new TravelStopData
								{
									Decelerate = Convert.ToDouble(Decelerate) / 3.6,
									Position = Statement.Distance,
									StopTime = Convert.ToDouble(StopTime),
									Accelerate = Convert.ToDouble(Accelerate) / 3.6,
									TargetSpeed = Convert.ToDouble(Speed) / 3.6,
									Direction = (TravelDirection)OtherTrain.Direction,
									RailIndex = RailIndex
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
				Plugin.CurrentHost.AddMessage(MessageType.Information, false, string.Format("[{0}] 走行軌道: {1}, 進行方向: {2}, 有効開始位置: {3}m, 有効開始時刻: {4}s", OtherTrain.Key, OtherTrain.TrackKey, OtherTrain.Direction, Train.AppearanceStartPosition, Train.AppearanceTime));

				for (int i = 0; i < Data.Count; i++)
				{
					if (Data[i] is TravelStopData d)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Information, false, string.Format("[{0}] 停車位置: {1}m, 減速度: {2}km/h/s, 停車時間: {3}s, 加速度: {4}km/h/s, 加速後の走行速度: {5}km/h", OtherTrain.Key, d.Position, d.Decelerate * 3.6, d.StopTime, d.Accelerate * 3.6, d.TargetSpeed * 3.6));
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

				int n = Plugin.TrainManager.TFOs.Length;
				Array.Resize(ref Plugin.TrainManager.TFOs, n + 1);
				Plugin.TrainManager.TFOs[n] = Train;
			}
		}

		private static void ParseOtherTrain(OtherTrain OtherTrain)
		{
			OtherTrain.CarObjects = new List<CarObject>();
			OtherTrain.CarSounds = new List<CarSound>();

			System.Text.Encoding Encoding = DetermineFileEncoding(OtherTrain.FilePath);

			string[] Lines = File.ReadAllLines(OtherTrain.FilePath, Encoding).Skip(1).ToArray();
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
				string Key, TempDistance, TempSpan, TempZ;

				Structure.TryGetValue("key", out Key);
				Structure.TryGetValue("distance", out TempDistance);
				Structure.TryGetValue("span", out TempSpan);
				Structure.TryGetValue("z", out TempZ);

				double Distance, Span, Z;
				if (string.IsNullOrEmpty(TempDistance) || !NumberFormats.TryParseDoubleVb6(TempDistance, out Distance))
				{
					Distance = 0.0;
				}
				if (string.IsNullOrEmpty(TempSpan) || !NumberFormats.TryParseDoubleVb6(TempSpan, out Span))
				{
					Span = 0.0;
				}
				if (string.IsNullOrEmpty(TempZ) || !NumberFormats.TryParseDoubleVb6(TempZ, out Z))
				{
					Z = 0.0;
				}

				OtherTrain.CarObjects.Add(new CarObject
				{
					Key = Key,
					Distance = Distance,
					Span = Span,
					Z = Z
				});
			}

			foreach (var Sound3d in Sound3ds)
			{
				string Key, TempDistance1, TempDistance2, Function;

				Sound3d.TryGetValue("key", out Key);
				Sound3d.TryGetValue("distance1", out TempDistance1);
				Sound3d.TryGetValue("distance2", out TempDistance2);
				Sound3d.TryGetValue("function", out Function);

				double Distance1;
				double Distance2;
				if (string.IsNullOrEmpty(TempDistance1) || !NumberFormats.TryParseDoubleVb6(TempDistance1, out Distance1))
				{
					Distance1 = 0.0;
				}
				if (string.IsNullOrEmpty(TempDistance2) || !NumberFormats.TryParseDoubleVb6(TempDistance2, out Distance2))
				{
					Distance2 = 0.0;
				}

				OtherTrain.CarSounds.Add(new CarSound
				{
					Key = Key,
					Distance1 = Distance1,
					Distance2 = Distance2,
					Function = Function
				});
			}
		}
	}
}
