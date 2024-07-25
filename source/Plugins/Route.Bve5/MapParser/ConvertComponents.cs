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
using System.Linq;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using RouteManager2.Stations;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void ConvertCurve(Statement Statement, RouteData RouteData)
		{
			{
				dynamic d = Statement;

				switch (Statement.FunctionName)
				{
					case MapFunctionName.SetGauge:
					case MapFunctionName.Gauge:
						{
							double Gauge = Statement.GetArgumentValueAsDouble(ArgumentName.Value);
							if (Gauge <= 0)
							{
								// Gauge must be positive and non-zero
								// Note that zero may also be returned if the gauge value is non-numeric
								break;
							}
							for (int tt = 0; tt < Plugin.CurrentRoute.Tracks.Count; tt++)
							{
								int t = Plugin.CurrentRoute.Tracks.ElementAt(tt).Key;
								Plugin.CurrentRoute.Tracks[t].RailGauge = Gauge;
							}
						}
						break;
					case MapFunctionName.BeginTransition:
						{
							int blockIndex = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[blockIndex].Rails["0"].CurveInterpolateStart = true;
							RouteData.Blocks[blockIndex].Rails["0"].CurveTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
					case MapFunctionName.BeginCircular:
					case MapFunctionName.Change:
					case MapFunctionName.Curve:
						{
							double Radius = d.Radius;
							double Cant = Statement.GetArgumentValueAsDouble(ArgumentName.Cant) / 1000.0;
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[Index].CurrentTrackState.CurveRadius = Radius;
							RouteData.Blocks[Index].CurrentTrackState.CurveCant = Math.Abs(Cant) * Math.Sign(Radius);
							RouteData.Blocks[Index].Rails["0"].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails["0"].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.End:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[Index].Rails["0"].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails["0"].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.Interpolate:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							int LastInterpolateIndex = -1;
							if (Index > 0)
							{
								LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails["0"].CurveInterpolateEnd);
							}

							double Radius, Cant;
							if (d.Radius == null)
							{
								Radius = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].CurrentTrackState.CurveRadius : 0.0;
							}
							else
							{
								Radius = d.Radius;
							}
							if (d.Cant == null)
							{
								Cant = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].CurrentTrackState.CurveCant : 0.0;
							}
							else
							{
								Cant = d.Cant / 1000.0;
							}

							RouteData.Blocks[Index].CurrentTrackState.CurveRadius = Convert.ToDouble(Radius);
							RouteData.Blocks[Index].CurrentTrackState.CurveCant = Math.Abs(Convert.ToDouble(Cant)) * Math.Sign(Convert.ToDouble(Radius));
							RouteData.Blocks[Index].Rails["0"].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails["0"].CurveInterpolateEnd = true;
							RouteData.Blocks[Index].Rails["0"].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.Turn:
						{
							object Slope = d.Slope;
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[Index].Turn = Convert.ToDouble(Slope);
						}
						break;
				}
			}

			{
				List<Block> TempBlocks = new List<Block>(RouteData.Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails["0"].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails["0"].CurveTransitionEnd)
						{
							break;
						}

						if (TempBlocks[k].Rails["0"].CurveTransitionStart)
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
				List<Block> TempBlocks = new List<Block>(RouteData.Blocks);

				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails["0"].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails["0"].CurveInterpolateEnd && !TempBlocks[k].Rails["0"].CurveInterpolateStart)
						{
							break;
						}

						if (TempBlocks[k].Rails["0"].CurveInterpolateStart)
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

		private static void ConvertGradient(Statement Statement, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;

			{
				dynamic d = Statement;
				switch (Statement.FunctionName)
				{
					case MapFunctionName.BeginTransition:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
					case MapFunctionName.BeginConst:
					case MapFunctionName.Pitch:
						{
							object Gradient = Statement.FunctionName == MapFunctionName.Pitch ? d.Rate : d.Gradient;
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].Pitch = Convert.ToDouble(Gradient) / 1000.0;
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientTransitionEnd = true;
						}
						break;
					case MapFunctionName.End:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							Blocks[Index].GradientInterpolateStart = true;
							Blocks[Index].GradientTransitionEnd = true;
						}
						break;
					case MapFunctionName.Interpolate:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							int LastInterpolateIndex = -1;
							if (Index > 0)
							{
								LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.GradientInterpolateEnd);
							}

							double Gradient;
							if (d.Gradient == null)
							{
								Gradient = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Pitch * 1000.0 : 0.0;
							}
							else
							{
								Gradient = d.Gradient;
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
			for (int railIndex = 1; railIndex < RouteData.TrackKeyList.Count; railIndex++)
			{
				string railKey = RouteData.TrackKeyList[railIndex];
				foreach (var Statement in ParseData.Statements)
				{
					if (Statement.ElementName != MapElementName.Track || !Statement.Key.Equals(RouteData.TrackKeyList[railIndex], StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}

					dynamic d = Statement;

					if (Statement.FunctionName == MapFunctionName.Position || (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.X))
					{
						int Index = RouteData.FindOrAddBlock(Statement.Distance);
						int LastInterpolateIndex = -1;
						if (Index > 0)
						{
							LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railKey].InterpolateX);
						}

						double X;

						if (d.X == null)
						{
							if (Statement.FunctionName == MapFunctionName.Position)
							{
								X = 0.0;
							}
							else
							{
								X = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].Position.X : 0.0;
							}
						}
						else
						{
							X = d.X;
						}

						double RadiusH;
						if (Statement.FunctionName == MapFunctionName.Position)
						{
							RadiusH = d.RadiusH == null ? 0.0 : (double)d.RadiusH;
						}
						else
						{
							if (d.Radius == null)
							{
								RadiusH = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].RadiusH : 0.0;
							}
							else
							{
								RadiusH = d.Radius;
							}
						}

						Blocks[Index].Rails[railKey].Position.X = Convert.ToDouble(X);
						Blocks[Index].Rails[railKey].RadiusH = Convert.ToDouble(RadiusH);
						Blocks[Index].Rails[railKey].InterpolateX = true;
					}

					if (Statement.FunctionName == MapFunctionName.Position || (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.Y))
					{
						int Index = RouteData.FindOrAddBlock(Statement.Distance);
						int LastInterpolateIndex = -1;
						if (Index > 0)
						{
							LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railKey].InterpolateY);
						}

						double Y;
						if (d.Y == null)
						{
							if (Statement.FunctionName == MapFunctionName.Position)
							{
								Y = 0.0;
							}
							else
							{
								Y = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].Position.Y : 0.0;
							}
						}
						else
						{
							Y = d.Y;
						}

						double RadiusV;
						if (Statement.FunctionName == MapFunctionName.Position)
						{
							RadiusV = d.RadiusV == null ? 0.0 : (double)d.RadiusV;
						}
						else
						{
							if (d.Radius == null)
							{
								RadiusV = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].RadiusV : 0.0;
							}
							else
							{
								RadiusV = d.Radius;
							}
						}

						Blocks[Index].Rails[railKey].Position.Y = Convert.ToDouble(Y);
						Blocks[Index].Rails[railKey].RadiusV = Convert.ToDouble(RadiusV);
						Blocks[Index].Rails[railKey].InterpolateY = true;
					}

					if (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.Cant)
					{
						switch (Statement.FunctionName)
						{
							case MapFunctionName.BeginTransition:
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									Blocks[Index].Rails[railKey].CurveInterpolateStart = true;
									Blocks[Index].Rails[railKey].CurveTransitionStart = true;
								}
								break;
							case MapFunctionName.Begin:
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									Blocks[Index].Rails[railKey].CurveCant = Statement.GetArgumentValueAsDouble(ArgumentName.Cant) / 1000.0;
									Blocks[Index].Rails[railKey].CurveInterpolateStart = true;
									Blocks[Index].Rails[railKey].CurveTransitionEnd = true;
								}
								break;
							case MapFunctionName.End:
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									Blocks[Index].Rails[railKey].CurveInterpolateStart = true;
									Blocks[Index].Rails[railKey].CurveTransitionEnd = true;
								}
								break;
							case MapFunctionName.Interpolate:
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									int LastInterpolateIndex = -1;
									if (Index > 0)
									{
										LastInterpolateIndex = Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railKey].CurveInterpolateEnd);
									}

									double Cant;
									if (d.Cant == null)
									{
										Cant = LastInterpolateIndex != -1
											? Blocks[LastInterpolateIndex].Rails[railKey].CurveCant
											: 0.0;
									}
									else
									{
										Cant = d.Cant / 1000.0;
									}

									Blocks[Index].Rails[railKey].CurveCant = Cant;
									Blocks[Index].Rails[railKey].CurveInterpolateStart = true;
									Blocks[Index].Rails[railKey].CurveInterpolateEnd = true;
									Blocks[Index].Rails[railKey].CurveTransitionEnd = true;
								}
								break;
						}
					}
				}

				if (!Blocks.First().Rails[railKey].InterpolateX)
				{
					int FirstInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						FirstInterpolateIndex = Blocks.FindIndex(1, Block => Block.Rails[railKey].InterpolateX);
					}
					Blocks.First().Rails[railKey].Position.X = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[railKey].Position.X : 0.0;
					//Blocks.First().Rails[j].RadiusH = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusH : 0.0;
					Blocks.First().Rails[railKey].InterpolateX = true;
				}

				if (!Blocks.First().Rails[railKey].InterpolateY)
				{
					int FirstInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						FirstInterpolateIndex = Blocks.FindIndex(1, Block => Block.Rails[railKey].InterpolateY);
					}
					Blocks.First().Rails[railKey].Position.Y = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[railKey].Position.Y : 0.0;
					//Blocks.First().Rails[j].RadiusV = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusV : 0.0;
					Blocks.First().Rails[railKey].InterpolateY = true;
				}

				if (!Blocks.Last().Rails[railKey].InterpolateX)
				{
					int LastInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						LastInterpolateIndex = Blocks.FindLastIndex(Blocks.Count - 2, Blocks.Count - 1, Block => Block.Rails[railKey].InterpolateX);
					}
					Blocks.Last().Rails[railKey].Position.X = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].Position.X : 0.0;
					Blocks.Last().Rails[railKey].RadiusH = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].RadiusH : 0.0;
					Blocks.Last().Rails[railKey].InterpolateX = true;
				}

				if (!Blocks.Last().Rails[railKey].InterpolateY)
				{
					int LastInterpolateIndex = -1;
					if (Blocks.Count > 1)
					{
						LastInterpolateIndex = Blocks.FindLastIndex(Blocks.Count - 2, Blocks.Count - 1, Block => Block.Rails[railKey].InterpolateY);
					}
					Blocks.Last().Rails[railKey].Position.Y = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].Position.Y : 0.0;
					Blocks.Last().Rails[railKey].RadiusV = LastInterpolateIndex != -1 ? Blocks[LastInterpolateIndex].Rails[railKey].RadiusV : 0.0;
					Blocks.Last().Rails[railKey].InterpolateY = true;
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				List<Block> TempBlocks = new List<Block>(Blocks);
				string railKey = RouteData.TrackKeyList[j];
				for (int i = 1; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[railKey].InterpolateX)
					{
						continue;
					}

					int StartBlock = TempBlocks.FindLastIndex(i - 1, i, Block => Block.Rails[railKey].InterpolateX);

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartX = TempBlocks[StartBlock].Rails[railKey].Position.X;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndX = TempBlocks[i].Rails[railKey].Position.X;

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
				string railKey = RouteData.TrackKeyList[j];
				for (int i = 1; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[railKey].InterpolateY)
					{
						continue;
					}

					int StartBlock = TempBlocks.FindLastIndex(i - 1, i, Block => Block.Rails[railKey].InterpolateY);

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartY = TempBlocks[StartBlock].Rails[railKey].Position.Y;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndY = TempBlocks[i].Rails[railKey].Position.Y;

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
				string railKey = RouteData.TrackKeyList[j];
				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[railKey].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails[railKey].CurveTransitionEnd)
						{
							break;
						}

						if (TempBlocks[k].Rails[railKey].CurveTransitionStart)
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
				string railKey = RouteData.TrackKeyList[j];
				for (int i = 0; i < TempBlocks.Count; i++)
				{
					if (!TempBlocks[i].Rails[railKey].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (TempBlocks[k].Rails[railKey].CurveInterpolateEnd && !TempBlocks[k].Rails[railKey].CurveInterpolateStart)
						{
							break;
						}

						if (TempBlocks[k].Rails[railKey].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(TempBlocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartCant = TempBlocks[StartBlock].Rails[railKey].CurveCant;
						double EndDistance = TempBlocks[i].StartingDistance;
						double EndCant = TempBlocks[i].Rails[railKey].CurveCant;

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

		private static int CurrentStation = 0;

		private static void ConvertStation(Statement Statement, RouteData RouteData)
		{

			{
				string stationKey = Statement.Key.ToLowerInvariant();
				if (!RouteData.StationList.ContainsKey(stationKey))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unable to find a station with key " + Statement.Key + " in the BVE5 station list.");
					return;
				}

				dynamic d = Statement;

				object Doors = d.Door;
				object BackwardTolerance = d.Margin1;
				object ForwardTolerance = d.Margin2;

				double DefaultTrackPosition = Statement.Distance + StationNoticeDistance;
				if (DefaultTrackPosition < 0.0)
				{
					DefaultTrackPosition = 0.0;
				}
				
				RouteStation NewStation = new RouteStation();
				NewStation.Name = RouteData.StationList[stationKey].Name;
				NewStation.ArrivalTime = RouteData.StationList[stationKey].ArrivalTime;
				RouteData.Sounds.TryGetValue(RouteData.StationList[stationKey].ArrivalSoundKey, out NewStation.ArrivalSoundBuffer);
				NewStation.StopMode = RouteData.StationList[stationKey].StopMode;
				NewStation.DepartureTime = RouteData.StationList[stationKey].DepartureTime;
				RouteData.Sounds.TryGetValue(RouteData.StationList[stationKey].DepartureSoundKey, out NewStation.DepartureSoundBuffer);
				NewStation.Type = RouteData.StationList[stationKey].StationType;
				NewStation.StopTime = RouteData.StationList[stationKey].StopTime;
				NewStation.ForceStopSignal = RouteData.StationList[stationKey].ForceStopSignal;
				NewStation.OpenLeftDoors = Convert.ToDouble(Doors) < 0.0;
				NewStation.OpenRightDoors = Convert.ToDouble(Doors) > 0.0;
				NewStation.Stops = new StationStop[1];
				NewStation.Stops[0].TrackPosition = Statement.Distance;
				NewStation.Stops[0].BackwardTolerance = Math.Abs(Convert.ToDouble(BackwardTolerance));
				NewStation.Stops[0].ForwardTolerance = Convert.ToDouble(ForwardTolerance);
				NewStation.PassengerRatio = RouteData.StationList[stationKey].PassengerRatio;
				NewStation.DefaultTrackPosition = DefaultTrackPosition;
				NewStation.ReopenDoor = RouteData.StationList[stationKey].ReopenDoor;
				NewStation.ReopenStationLimit = 5;
				NewStation.InterferenceInDoor = RouteData.StationList[stationKey].InterferenceInDoor;
				NewStation.MaxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
				NewStation.Key = stationKey;

				int StationBlockIndex = RouteData.FindOrAddBlock(DefaultTrackPosition);
				RouteData.Blocks[StationBlockIndex].StationIndex = CurrentStation;

				int StopBlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
				RouteData.Blocks[StopBlockIndex].Stop = CurrentStation;

				Array.Resize(ref Plugin.CurrentRoute.Stations, CurrentStation + 1);
				Plugin.CurrentRoute.Stations[CurrentStation] = NewStation;
				CurrentStation++;
			}
		}

		private static void ConvertBackground(Statement Statement, RouteData RouteData)
		{
			{
				dynamic d = Statement;

				string BackgroundKey = Convert.ToString(d.StructureKey);
				BackgroundKey = BackgroundKey.ToLowerInvariant();


				if (!RouteData.Backgrounds.ContainsKey(BackgroundKey))
				{
					RouteData.Objects.TryGetValue(Convert.ToString(BackgroundKey), out UnifiedObject Object);

					if (Object != null)
					{
						RouteData.Backgrounds.Add(Convert.ToString(BackgroundKey), Object);
					}
				}

				if (RouteData.Backgrounds.ContainsKey(BackgroundKey))
				{
					int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
					RouteData.Blocks[BlockIndex].Background = BackgroundKey;
				}
			}
		}

		private static void ConvertFog(Statement Statement, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;

			{
				switch (Statement.FunctionName)
				{
					case MapFunctionName.Fog:
						{
							double Start = Statement.GetArgumentValueAsDouble(ArgumentName.Start);
							double End = Statement.GetArgumentValueAsDouble(ArgumentName.End);
							double TempRed, TempGreen, TempBlue;
							if (!Statement.HasArgument(ArgumentName.Red) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Red), out TempRed))
							{
								TempRed = 128;
							}
							if (!Statement.HasArgument(ArgumentName.Green) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Green), out TempGreen))
							{
								TempGreen = 128;
							}
							if (!Statement.HasArgument(ArgumentName.Blue) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Blue), out TempBlue))
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
								Blocks[BlockIndex].Fog.Start = (float)Start;
								Blocks[BlockIndex].Fog.End = (float)End;
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
					case MapFunctionName.Interpolate:
					case MapFunctionName.Set:
						{
							double Density, TempRed, TempGreen, TempBlue;
							if (!Statement.HasArgument(ArgumentName.Density) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Density), out Density))
							{
								Density = 0.001;
							}
							if (!Statement.HasArgument(ArgumentName.Red) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Red), out TempRed))
							{
								TempRed = 1.0;
							}
							if (!Statement.HasArgument(ArgumentName.Green) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Green), out TempGreen))
							{
								TempGreen = 1.0;
							}
							if (!Statement.HasArgument(ArgumentName.Blue) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Blue), out TempBlue))
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

		private static void ConvertIrregularity(Statement Statement, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;
			{


				double X = !Statement.HasArgument(ArgumentName.X) ? 0.0 : Statement.GetArgumentValueAsDouble(ArgumentName.X);

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

		private static void ConvertAdhesion(Statement Statement, RouteData RouteData)
		{
			List<Block> Blocks = RouteData.Blocks;

			{

				switch (Statement.GetArgumentNames().Count())
				{
					case 1:
						{
							if (!Statement.HasArgument(ArgumentName.A))
							{
								//Invalid number
								return;
							}
							
							int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
							//Presumably this is just the adhesion coefficent at 0km/h
							Blocks[BlockIndex].AdhesionMultiplier = (int)(Statement.GetArgumentValueAsDouble(ArgumentName.A) * 100 / 0.26) / 100.0;
							Blocks[BlockIndex].AdhesionMultiplierDefined = true;
						}
						break;
					case 3:
						{
							double C0 = Statement.GetArgumentValueAsDouble(ArgumentName.A);
							double C1 = Statement.GetArgumentValueAsDouble(ArgumentName.B);
							double C2 = Statement.GetArgumentValueAsDouble(ArgumentName.C);
							if (C0 != 0.0 && C1 == 0.0 && C2 != 0.0)
							{
								int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
								Blocks[BlockIndex].AdhesionMultiplierDefined = true;

								if (C0 == 0.35 && C2 == 0.01)
								{
									//Default adhesion value of 100
									Blocks[BlockIndex].AdhesionMultiplier = 1;
									return;
								}

								int CA = (int)(C0 * 100 / 0.26);
								double CB = 1.0 / (300 * (CA / 100.0 * 0.259999990463257));

								if (Math.Round(CB, 8) == C2)
								{
									//BVE2 / 4 converted value, so let's use that
									Blocks[BlockIndex].AdhesionMultiplier = CA / 100.0;
									return;
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

		private static void ConvertJointNoise(Statement Statement, RouteData RouteData)
		{
			int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
			RouteData.Blocks[BlockIndex].JointSound = true;
		}
	}
}
