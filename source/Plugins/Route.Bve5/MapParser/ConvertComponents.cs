using System;
using System.Collections.Generic;
using System.Linq;
using Bve5Parser.MapGrammar;
using OpenBveApi.Colors;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using RouteManager2.Stations;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void ConvertCurve(MapData ParseData, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "curve" && !(Statement.MapElement[0] == "legacy" && (Statement.Function == "curve" || Statement.Function == "turn")))
				{
					continue;
				}

				switch (Statement.Function)
				{
					case "setgauge":
					case "gauge":
						{
							object Gauge;
							Statement.Arguments.TryGetValue("value", out Gauge);
							for (int tt = 0; tt < Plugin.CurrentRoute.Tracks.Count; tt++)
							{
								int t = Plugin.CurrentRoute.Tracks.ElementAt(tt).Key;
								Plugin.CurrentRoute.Tracks[t].RailGauge = Convert.ToDouble(Gauge);
							}
						}
						break;
					case "begintransition":
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].Rails[0].CurveInterpolateStart = true;
							Blocks[Index].Rails[0].CurveTransitionStart = true;
						}
						break;
					case "begin":
					case "begincircular":
					case "change":
					case "curve":
						{
							object Radius, Cant;
							Statement.Arguments.TryGetValue("radius", out Radius);
							Statement.Arguments.TryGetValue("cant", out Cant);
							if (Statement.Function == "curve")
							{
								Cant = Convert.ToDouble(Cant) / 1000.0;
							}

							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].CurrentTrackState.CurveRadius = Convert.ToDouble(Radius);
							Blocks[Index].CurrentTrackState.CurveCant = Math.Abs(Convert.ToDouble(Cant)) * Math.Sign(Convert.ToDouble(Radius));
							Blocks[Index].Rails[0].CurveInterpolateStart = true;
							Blocks[Index].Rails[0].CurveTransitionEnd = true;
						}
						break;
					case "end":
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].Rails[0].CurveInterpolateStart = true;
							Blocks[Index].Rails[0].CurveTransitionEnd = true;
						}
						break;
					case "interpolate":
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							int LastInterpolateIndex = -1;
							if (Index > 0)
							{
								LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[0].CurveInterpolateEnd);
							}

							object Radius, Cant;
							if (!Statement.Arguments.TryGetValue("radius", out Radius) || Radius == null)
							{
								Radius = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].CurrentTrackState.CurveRadius : 0.0;
							}
							if (!Statement.Arguments.TryGetValue("cant", out Cant) || Cant == null)
							{
								Cant = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].CurrentTrackState.CurveCant : 0.0;
							}

							Blocks[Index].CurrentTrackState.CurveRadius = Convert.ToDouble(Radius);
							Blocks[Index].CurrentTrackState.CurveCant = Math.Abs(Convert.ToDouble(Cant)) * Math.Sign(Convert.ToDouble(Radius));
							Blocks[Index].Rails[0].CurveInterpolateStart = true;
							Blocks[Index].Rails[0].CurveInterpolateEnd = true;
							Blocks[Index].Rails[0].CurveTransitionEnd = true;
						}
						break;
					case "turn":
						{
							object Slope;
							Statement.Arguments.TryGetValue("slope", out Slope);

							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].Turn = Convert.ToDouble(Slope);
						}
						break;
				}
			}

			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[0].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails[0].CurveTransitionEnd)
						{
							break;
						}

						if (TempBlocks[k].Rails[0].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = TempBlocks[i].StartingDistance;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}

			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[0].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails[0].CurveInterpolateEnd && !TempBlocks[k].Rails[0].CurveInterpolateStart)
						{
							break;
						}

						if (TempBlocks[k].Rails[0].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartRadius = TempBlocks[StartBlock].CurrentTrackState.CurveRadius;
						double StartCant = TempBlocks[StartBlock].CurrentTrackState.CurveCant;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndRadius = TempBlocks[i].CurrentTrackState.CurveRadius;
						double EndCant = TempBlocks[i].CurrentTrackState.CurveCant;

						if (StartRadius == EndRadius && StartCant == EndCant)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}
		}

		private static void ConvertGradient(MapData ParseData, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "gradient" && !(Statement.MapElement[0] == "legacy" && Statement.Function == "pitch"))
				{
					continue;
				}

				switch (Statement.Function)
				{
					case "begintransition":
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientTransitionStart = true;
						}
						break;
					case "begin":
					case "beginconst":
					case "pitch":
						{
							object Gradient;
							if (Statement.Function == "pitch")
							{
								Statement.Arguments.TryGetValue("rate", out Gradient);
							}
							else
							{
								Statement.Arguments.TryGetValue("gradient", out Gradient);
							}

							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].Pitch = Convert.ToDouble(Gradient) / 1000.0;
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientTransitionEnd = true;
						}
						break;
					case "end":
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientTransitionEnd = true;
						}
						break;
					case "interpolate":
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							int LastInterpolateIndex = -1;
							if (Index > 0)
							{
								LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.GradientInterpolateEnd);
							}

							object Gradient;
							if (!Statement.Arguments.TryGetValue("gradient", out Gradient) || Gradient == null)
							{
								Gradient = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Pitch * 1000.0 : 0.0;
							}

							Blocks[Index].Pitch = Convert.ToDouble(Gradient) / 1000.0;
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientInterpolateEnd = true;
							Blocks[Index].GradientTransitionEnd = true;
						}
						break;
				}
			}

			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].GradientTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].GradientTransitionEnd)
						{
							break;
						}

						if (TempBlocks[k].GradientTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = TempBlocks[i].StartingDistance;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}

			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].GradientInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].GradientInterpolateEnd && !TempBlocks[k].GradientInterpolateStart)
						{
							break;
						}

						if (TempBlocks[k].GradientInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartPitch = TempBlocks[StartBlock].Pitch;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndPitch = TempBlocks[i].Pitch;

						if (StartPitch == EndPitch)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}
		}

		private static void ConvertTrack(MapData ParseData, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;

			// Own track is excluded.
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				foreach (var Statement in ParseData.Statements)
				{
					if (Statement.MapElement[0] != "track" || !Statement.Key.Equals(RouteData.TrackKeyList[j], StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}

					if (Statement.Function == "position" || (Statement.MapElement.Length == 2 && Statement.MapElement[1] == "x"))
					{
						int Index = RouteData.FindOrAddBlock(Statement.Distance);
						int LastInterpolateIndex = -1;
						if (Index > 0)
						{
							LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[j].InterpolateX);
						}

						object X;
						if (!Statement.Arguments.TryGetValue("x", out X) || X == null)
						{
							if (Statement.Function == "position")
							{
								X = 0.0;
							}
							else
							{
								X = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RailX : 0.0;
							}
						}

						object RadiusH;
						if (Statement.Function == "position")
						{
							if (!Statement.Arguments.TryGetValue("radiush", out RadiusH) || RadiusH == null)
							{
								RadiusH = 0.0;
							}
						}
						else
						{
							if (!Statement.Arguments.TryGetValue("radius", out RadiusH) || RadiusH == null)
							{
								RadiusH = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RadiusH : 0.0;
							}
						}

						Blocks[Index].Rails[j].RailX = Convert.ToDouble(X);
						Blocks[Index].Rails[j].RadiusH = Convert.ToDouble(RadiusH);
						Blocks[Index].Rails[j].InterpolateX = true;
					}

					if (Statement.Function == "position" || (Statement.MapElement.Length == 2 && Statement.MapElement[1] == "y"))
					{
						int Index = RouteData.FindOrAddBlock(Statement.Distance);
						int LastInterpolateIndex = -1;
						if (Index > 0)
						{
							LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[j].InterpolateY);
						}

						object Y;
						if (!Statement.Arguments.TryGetValue("y", out Y) || Y == null)
						{
							if (Statement.Function == "position")
							{
								Y = 0.0;
							}
							else
							{
								Y = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RailY : 0.0;
							}
						}

						object RadiusV;
						if (Statement.Function == "position")
						{
							if (!Statement.Arguments.TryGetValue("radiusv", out RadiusV) || RadiusV == null)
							{
								RadiusV = 0.0;
							}
						}
						else
						{
							if (!Statement.Arguments.TryGetValue("radius", out RadiusV) || RadiusV == null)
							{
								RadiusV = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RadiusV : 0.0;
							}
						}

						Blocks[Index].Rails[j].RailY = Convert.ToDouble(Y);
						Blocks[Index].Rails[j].RadiusV = Convert.ToDouble(RadiusV);
						Blocks[Index].Rails[j].InterpolateY = true;
					}

					if (Statement.MapElement.Length == 2 && Statement.MapElement[1] == "cant")
					{
						switch (Statement.Function)
						{
							case "begintransition":
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									Blocks[Index].Rails[j].CurveInterpolateStart = true;
									Blocks[Index].Rails[j].CurveTransitionStart = true;
								}
								break;
							case "begin":
								{
									object Cant;
									Statement.Arguments.TryGetValue("cant", out Cant);

									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									Blocks[Index].Rails[j].CurveCant = Convert.ToDouble(Cant);
									Blocks[Index].Rails[j].CurveInterpolateStart = true;
									Blocks[Index].Rails[j].CurveTransitionEnd = true;
								}
								break;
							case "end":
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									Blocks[Index].Rails[j].CurveInterpolateStart = true;
									Blocks[Index].Rails[j].CurveTransitionEnd = true;
								}
								break;
							case "interpolate":
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									int LastInterpolateIndex = -1;
									if (Index > 0)
									{
										LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[j].CurveInterpolateEnd);
									}

									object Cant;
									if (!Statement.Arguments.TryGetValue("cant", out Cant) || Cant == null)
									{
										Cant = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].CurveCant : 0.0;
									}

									Blocks[Index].Rails[j].CurveCant = Convert.ToDouble(Cant);
									Blocks[Index].Rails[j].CurveInterpolateStart = true;
									Blocks[Index].Rails[j].CurveInterpolateEnd = true;
									Blocks[Index].Rails[j].CurveTransitionEnd = true;
								}
								break;
						}
					}
				}

				if (!Blocks.First().Rails[j].InterpolateX)
				{
					int FirstInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						FirstInterpolateIndex = Blocks.FindIndex(1, Block => Block.Rails[j].InterpolateX);
					}
					Blocks.First().Rails[j].RailX = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RailX : 0.0;
					//Blocks.First().Rails[j].RadiusH = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusH : 0.0;
					Blocks.First().Rails[j].InterpolateX = true;
				}

				if (!Blocks.First().Rails[j].InterpolateY)
				{
					int FirstInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						FirstInterpolateIndex = Blocks.FindIndex(1, Block => Block.Rails[j].InterpolateY);
					}
					Blocks.First().Rails[j].RailY = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RailY : 0.0;
					//Blocks.First().Rails[j].RadiusV = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusV : 0.0;
					Blocks.First().Rails[j].InterpolateY = true;
				}

				if (!Blocks.Last().Rails[j].InterpolateX)
				{
					int LastInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						LastInterpolateIndex = Blocks.FindLastIndex(Blocks.Count - 2, Blocks.Count - 1, Block => Block.Rails[j].InterpolateX);
					}
					Blocks.Last().Rails[j].RailX = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RailX : 0.0;
					Blocks.Last().Rails[j].RadiusH = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RadiusH : 0.0;
					Blocks.Last().Rails[j].InterpolateX = true;
				}

				if (!Blocks.Last().Rails[j].InterpolateY)
				{
					int LastInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						LastInterpolateIndex = Blocks.FindLastIndex(Blocks.Count - 2, Blocks.Count - 1, Block => Block.Rails[j].InterpolateY);
					}
					Blocks.Last().Rails[j].RailY = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RailY : 0.0;
					Blocks.Last().Rails[j].RadiusV = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[j].RadiusV : 0.0;
					Blocks.Last().Rails[j].InterpolateY = true;
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 1; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[j].InterpolateX)
					{
						continue;
					}

					int StartBlock = TempBlocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].InterpolateX);

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartX = TempBlocks[StartBlock].Rails[j].RailX;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndX = TempBlocks[i].Rails[j].RailX;

						if (StartX == EndX)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 1; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[j].InterpolateY)
					{
						continue;
					}

					int StartBlock = TempBlocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].InterpolateY);

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartY = TempBlocks[StartBlock].Rails[j].RailY;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndY = TempBlocks[i].Rails[j].RailY;

						if (StartY == EndY)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[j].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails[j].CurveTransitionEnd)
						{
							break;
						}

						if (TempBlocks[k].Rails[j].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = TempBlocks[i].StartingDistance;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				List<Block> TempBlocks = new List<Block>(Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[j].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails[j].CurveInterpolateEnd && !TempBlocks[k].Rails[j].CurveInterpolateStart)
						{
							break;
						}

						if (TempBlocks[k].Rails[j].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartCant = TempBlocks[StartBlock].Rails[j].CurveCant;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndCant = TempBlocks[i].Rails[j].CurveCant;

						if (StartCant == EndCant)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
				}
			}
		}

		private static void ConvertStation(MapData ParseData, RouteData RouteData)
		{
			int CurrentStation = 0;

			Plugin.CurrentRoute.Stations = new RouteStation[CurrentStation];

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "station")
				{
					continue;
				}

				int Index = RouteData.StationList.FindIndex(Station => Station.Key.Equals(Statement.Key, StringComparison.InvariantCultureIgnoreCase));
				if (Index == -1)
				{
					continue;
				}

				object Doors;
				object BackwardTolerance;
				object ForwardTolerance;
				Statement.Arguments.TryGetValue("door", out Doors);
				Statement.Arguments.TryGetValue("margin1", out BackwardTolerance);
				Statement.Arguments.TryGetValue("margin2", out ForwardTolerance);

				double DefaultTrackPosition = Statement.Distance + StationNoticeDistance;
				if (DefaultTrackPosition < 0.0)
				{
					DefaultTrackPosition = 0.0;
				}

				SoundHandle ArrivalSound;
				SoundHandle DepartureSound;
				RouteData.Sounds.TryGetValue(RouteData.StationList[Index].ArrivalSoundKey, out ArrivalSound);
				RouteData.Sounds.TryGetValue(RouteData.StationList[Index].DepartureSoundKey, out DepartureSound);

				RouteStation NewStation = new RouteStation();
				NewStation.Name = RouteData.StationList[Index].Name;
				NewStation.ArrivalTime = RouteData.StationList[Index].ArrivalTime;
				NewStation.ArrivalSoundBuffer = ArrivalSound;
				NewStation.StopMode = RouteData.StationList[Index].StopMode;
				NewStation.DepartureTime = RouteData.StationList[Index].DepartureTime;
				NewStation.DepartureSoundBuffer = DepartureSound;
				NewStation.Type = RouteData.StationList[Index].StationType;
				NewStation.StopTime = RouteData.StationList[Index].StopTime;
				NewStation.ForceStopSignal = RouteData.StationList[Index].ForceStopSignal;
				NewStation.OpenLeftDoors = Convert.ToDouble(Doors) < 0.0;
				NewStation.OpenRightDoors = Convert.ToDouble(Doors) > 0.0;
				NewStation.Stops = new StationStop[1];
				NewStation.Stops[0].TrackPosition = Statement.Distance;
				NewStation.Stops[0].BackwardTolerance = Math.Abs(Convert.ToDouble(BackwardTolerance));
				NewStation.Stops[0].ForwardTolerance = Convert.ToDouble(ForwardTolerance);
				NewStation.PassengerRatio = RouteData.StationList[Index].PassengerRatio;
				NewStation.DefaultTrackPosition = DefaultTrackPosition;
				NewStation.ReopenDoor = RouteData.StationList[Index].ReopenDoor;
				NewStation.ReopenStationLimit = 5;
				NewStation.InterferenceInDoor = RouteData.StationList[Index].InterferenceInDoor;
				NewStation.MaxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);

				int StationBlockIndex = RouteData.FindOrAddBlock(DefaultTrackPosition);
				RouteData.Blocks[StationBlockIndex].Station = CurrentStation;

				int StopBlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
				RouteData.Blocks[StopBlockIndex].Stop = CurrentStation;

				Array.Resize(ref Plugin.CurrentRoute.Stations, CurrentStation + 1);
				Plugin.CurrentRoute.Stations[CurrentStation] = NewStation;
				CurrentStation++;
			}
		}

		private static void ConvertBackground(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Backgrounds = new List<Background>();

			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "background")
				{
					continue;
				}

				object BackgroundKey;
				Statement.Arguments.TryGetValue("structurekey", out BackgroundKey);

				int BackgroundIndex = RouteData.Backgrounds.FindIndex(Background => Background.Key.Equals(Convert.ToString(BackgroundKey), StringComparison.InvariantCultureIgnoreCase));

				if (BackgroundIndex == -1)
				{
					BackgroundIndex = RouteData.Backgrounds.Count;

					UnifiedObject Object;
					RouteData.Objects.TryGetValue(Convert.ToString(BackgroundKey), out Object);

					if (Object != null)
					{
						RouteData.Backgrounds.Add(new Background(Convert.ToString(BackgroundKey), new BackgroundObject((StaticObject)Object)));
					}
					else
					{
						RouteData.Backgrounds.Add(new Background(Convert.ToString(BackgroundKey), new StaticBackground(null, 6, false)));
					}
				}

				if (BackgroundIndex != -1)
				{
					int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
					RouteData.Blocks[BlockIndex].Background = BackgroundIndex;
				}
				else
				{
					//Failed to load the background
				}
			}
		}

		private static void ConvertFog(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "fog" && !(Statement.MapElement[0] == "legacy" && Statement.Function == "fog"))
				{
					continue;
				}

				switch (Statement.Function)
				{
					case "fog":
						{
							object Start, End, TempRed, TempGreen, TempBlue;
							Statement.Arguments.TryGetValue("start", out Start);
							Statement.Arguments.TryGetValue("end", out End);
							if (!Statement.Arguments.TryGetValue("red", out TempRed))
							{
								TempRed = 128;
							}
							if (!Statement.Arguments.TryGetValue("green", out TempGreen))
							{
								TempGreen = 128;
							}
							if (!Statement.Arguments.TryGetValue("blue", out TempBlue))
							{
								TempBlue = 128;
							}

							int Red = Convert.ToInt32(TempRed);
							int Green = Convert.ToInt32(TempGreen);
							int Blue = Convert.ToInt32(TempBlue);
							if (Red < 0 || Red > 255)
							{
								Red = Red < 0 ? 0 : 255;
							}
							if (Green < 0 || Green > 255)
							{
								Green = Green < 0 ? 0 : 255;
							}
							if (Blue < 0 || Blue > 255)
							{
								Blue = Blue < 0 ? 0 : 255;
							}

							int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);

							if (Convert.ToSingle(Start) < Convert.ToSingle(End))
							{
								Blocks[BlockIndex].Fog.Start = Convert.ToSingle(Start);
								Blocks[BlockIndex].Fog.End = Convert.ToSingle(End);
							}
							else
							{
								Blocks[BlockIndex].Fog.Start = Plugin.CurrentRoute.NoFogStart;
								Blocks[BlockIndex].Fog.End = Plugin.CurrentRoute.NoFogEnd;
							}
							Blocks[BlockIndex].Fog.Color = new Color24((byte)Red, (byte)Green, (byte)Blue);
							Blocks[BlockIndex].FogDefined = true;
						}
						break;
					case "interpolate":
					case "set":
						{
							object Density, TempRed, TempGreen, TempBlue;
							if (!Statement.Arguments.TryGetValue("density", out Density) || Convert.ToDouble(Density) == 0.0)
							{
								Density = 0.001;
							}
							if (!Statement.Arguments.TryGetValue("red", out TempRed))
							{
								TempRed = 1.0;
							}
							if (!Statement.Arguments.TryGetValue("green", out TempGreen))
							{
								TempGreen = 1.0;
							}
							if (!Statement.Arguments.TryGetValue("blue", out TempBlue))
							{
								TempBlue = 1.0;
							}

							double Red = Convert.ToDouble(TempRed);
							double Green = Convert.ToDouble(TempGreen);
							double Blue = Convert.ToDouble(TempBlue);
							if (Red < 0.0 || Red > 1.0)
							{
								Red = Red < 0.0 ? 0.0 : 1.0;
							}
							if (Green < 0.0 || Green > 1.0)
							{
								Green = Green < 0.0 ? 0.0 : 1.0;
							}
							if (Blue < 0.0 || Blue > 1.0)
							{
								Blue = Blue < 0.0 ? 0.0 : 1.0;
							}

							int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);

							Blocks[BlockIndex].Fog.Start = 0.0f;
							Blocks[BlockIndex].Fog.End = 1.0f / Convert.ToSingle(Density);
							Blocks[BlockIndex].Fog.Color = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
							Blocks[BlockIndex].FogDefined = true;
						}
						break;
				}
			}
		}

		private static void ConvertIrregularity(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "irregularity")
				{
					continue;
				}

				object X;
				if (!Statement.Arguments.TryGetValue("x", out X))
				{
					X = 0.0;
				}

				double Squaring = 1.0 - 8.0 * (8.0 - Convert.ToDouble(X) * 10000.0);
				if (Squaring < 0.0)
				{
					Squaring = 0.0;
				}
				double Accuracy = (-1.0 + Math.Sqrt(Squaring)) / 4.0;
				if (Accuracy < 0.0 || Accuracy > 4.0)
				{
					Accuracy = Accuracy < 0.0 ? 0.0 : 4.0;
				}

				int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
				Blocks[BlockIndex].Accuracy = Accuracy;
				Blocks[BlockIndex].AccuracyDefined = true;
			}

			if (!Blocks.Last().AccuracyDefined)
			{
				int LastDefinedIndex = -1;
				if (Blocks.Count > 1)
				{
					LastDefinedIndex = Blocks.FindLastIndex(Blocks.Count - 2, Blocks.Count - 1, Block => Block.AccuracyDefined);
				}
				Blocks.Last().Accuracy = LastDefinedIndex != -1 ? Blocks[LastDefinedIndex].Accuracy : 2.0;
				Blocks.Last().AccuracyDefined = true;
			}
		}

		private static void ConvertAdhesion(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "adhesion")
				{
					continue;
				}

				switch (Statement.Arguments.Count)
				{
					case 1:
						{
							object C;
							if (!Statement.Arguments.TryGetValue("a", out C))
							{
								//Invalid number
								continue;
							}

							int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
							//Presumably this is just the adhesion coefficent at 0km/h
							Blocks[BlockIndex].AdhesionMultiplier = (int)(Convert.ToDouble(C) * 100 / 0.26) / 100.0;
							Blocks[BlockIndex].AdhesionMultiplierDefined = true;
						}
						break;
					case 3:
						{
							object TempC0, TempC1, TempC2;
							Statement.Arguments.TryGetValue("a", out TempC0);
							Statement.Arguments.TryGetValue("b", out TempC1);
							Statement.Arguments.TryGetValue("c", out TempC2);

							double C0 = Convert.ToDouble(TempC0);
							double C1 = Convert.ToDouble(TempC1);
							double C2 = Convert.ToDouble(TempC2);
							if (C0 != 0.0 && C1 == 0.0 && C2 != 0.0)
							{
								int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
								Blocks[BlockIndex].AdhesionMultiplierDefined = true;

								if (C0 == 0.35 && C2 == 0.01)
								{
									//Default adhesion value of 100
									Blocks[BlockIndex].AdhesionMultiplier = 1;
									continue;
								}

								int CA = (int)(C0 * 100 / 0.26);
								double CB = 1.0 / (300 * (CA / 100.0 * 0.259999990463257));

								if (Math.Round(CB, 8) == C2)
								{
									//BVE2 / 4 converted value, so let's use that
									Blocks[BlockIndex].AdhesionMultiplier = CA / 100.0;
									continue;
								}

								//They don't match.....
								//Use an average of the two to get something reasonable
								//TODO: Implement adhesion based upon speed formula
								Blocks[BlockIndex].AdhesionMultiplier = (CA + CB) / 2 / 100;
							}
						}
						break;
				}
			}

			if (!Blocks.Last().AdhesionMultiplierDefined)
			{
				int LastDefinedIndex = -1;
				if (Blocks.Count > 1)
				{
					LastDefinedIndex = Blocks.FindLastIndex(Blocks.Count - 2, Blocks.Count - 1, Block => Block.AdhesionMultiplierDefined);
				}
				Blocks.Last().AdhesionMultiplier = LastDefinedIndex != -1 ? Blocks[LastDefinedIndex].AdhesionMultiplier : 1.0;
				Blocks.Last().AdhesionMultiplierDefined = true;
			}
		}

		private static void ConvertJointNoise(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "jointnoise")
				{
					continue;
				}

				int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
				RouteData.Blocks[BlockIndex].JointSound = true;
			}
		}
	}
}
