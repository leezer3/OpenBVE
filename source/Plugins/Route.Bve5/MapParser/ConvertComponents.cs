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
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2.Stations;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void ConvertCurve(Statement Statement, RouteData RouteData)
		{
			{
				dynamic d = Statement;
				RouteData.FindOrAddBlock(Statement.Distance);
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
							RouteData.sortedBlocks[Statement.Distance].Rails[0].CurveInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].Rails[0].CurveTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
					case MapFunctionName.BeginCircular:
					case MapFunctionName.Change:
					case MapFunctionName.Curve:
						{
							object Radius = d.Radius;
							object Cant = d.Cant;
							if (Statement.FunctionName == MapFunctionName.Curve)
							{
								Cant = Convert.ToDouble(Cant) / 1000.0;
							}

							RouteData.sortedBlocks[Statement.Distance].CurrentTrackState.CurveRadius = Convert.ToDouble(Radius);
							RouteData.sortedBlocks[Statement.Distance].CurrentTrackState.CurveCant = Math.Abs(Convert.ToDouble(Cant)) * Math.Sign(Convert.ToDouble(Radius));
							RouteData.sortedBlocks[Statement.Distance].Rails[0].CurveInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].Rails[0].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.End:
						{
							RouteData.sortedBlocks[Statement.Distance].Rails[0].CurveInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].Rails[0].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.Interpolate:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							int LastInterpolateIndex = -1;
							if (Index > 0)
							{
								LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[0].CurveInterpolateEnd);
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
								Cant = d.Cant;
							}

							RouteData.Blocks[Index].CurrentTrackState.CurveRadius = Convert.ToDouble(Radius);
							RouteData.Blocks[Index].CurrentTrackState.CurveCant = Math.Abs(Convert.ToDouble(Cant)) * Math.Sign(Convert.ToDouble(Radius));
							RouteData.Blocks[Index].Rails[0].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails[0].CurveInterpolateEnd = true;
							RouteData.Blocks[Index].Rails[0].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.Turn:
						{
							object Slope = d.Slope;
							RouteData.sortedBlocks[Statement.Distance].Turn = Convert.ToDouble(Slope);
						}
						break;
				}
			}

			{
				for (int i = 0; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].Rails[0].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails[0].CurveTransitionEnd)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails[0].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					double dist = RouteData.sortedBlocks.ElementAt(i).Key;

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = RouteData.Blocks[i].StartingDistance;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					i = RouteData.sortedBlocks.IndexOfKey(dist);

				}
			}

			{
				for (int i = 0; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].Rails[0].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails[0].CurveInterpolateEnd && !RouteData.Blocks[k].Rails[0].CurveInterpolateStart)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails[0].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}
					double dist = RouteData.sortedBlocks.ElementAt(i).Key;
					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartRadius = RouteData.Blocks[StartBlock].CurrentTrackState.CurveRadius;
						double StartCant = RouteData.Blocks[StartBlock].CurrentTrackState.CurveCant;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndRadius = RouteData.Blocks[i].CurrentTrackState.CurveRadius;
						double EndCant = RouteData.Blocks[i].CurrentTrackState.CurveCant;

						if (StartRadius == EndRadius && StartCant == EndCant)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}
					i = RouteData.sortedBlocks.IndexOfKey(dist);
				}
			}
		}

		private static void ConvertGradient(Statement Statement, RouteData RouteData)
		{
			IList<Block> Blocks = RouteData.Blocks;

			{
				dynamic d = Statement;
				RouteData.FindOrAddBlock(Statement.Distance);
				switch (Statement.FunctionName)
				{
					case MapFunctionName.BeginTransition:
						{
							RouteData.sortedBlocks[Statement.Distance].GradientInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].GradientTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
					case MapFunctionName.BeginConst:
					case MapFunctionName.Pitch:
						{
							object Gradient = Statement.FunctionName == MapFunctionName.Pitch ? d.Rate : d.Gradient;
							RouteData.sortedBlocks[Statement.Distance].Pitch = Convert.ToDouble(Gradient) / 1000.0;
							RouteData.sortedBlocks[Statement.Distance].GradientInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].GradientTransitionEnd = true;
						}
						break;
					case MapFunctionName.End:
						{
							RouteData.sortedBlocks[Statement.Distance].GradientInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].GradientTransitionEnd = true;
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

							RouteData.sortedBlocks[Statement.Distance].Pitch = Convert.ToDouble(Gradient) / 1000.0;
							RouteData.sortedBlocks[Statement.Distance].GradientInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].GradientInterpolateEnd = true;
							RouteData.sortedBlocks[Statement.Distance].GradientTransitionEnd = true;
						}
						break;
				}
			}

			{
				for (int i = 0; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].GradientTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].GradientTransitionEnd)
						{
							break;
						}

						if (RouteData.Blocks[k].GradientTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					double dist = RouteData.sortedBlocks.ElementAt(i).Key;

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = RouteData.Blocks[i].StartingDistance;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					i = RouteData.sortedBlocks.IndexOfKey(dist);
				}
			}

			{

				for (int i = 0; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].GradientInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].GradientInterpolateEnd && !RouteData.Blocks[k].GradientInterpolateStart)
						{
							break;
						}

						if (RouteData.Blocks[k].GradientInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					double dist = RouteData.sortedBlocks.ElementAt(i).Key;

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartPitch = RouteData.Blocks[StartBlock].Pitch;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndPitch = RouteData.Blocks[i].Pitch;

						if (StartPitch == EndPitch)
						{
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					i = RouteData.sortedBlocks.IndexOfKey(dist);
				}
			}
		}

		private static void ConvertTrack(Statement Statement, RouteData RouteData)
		{
			
			int railIndex = RouteData.TrackKeyList.IndexOf(Statement.Key);
			if (railIndex <= 0)
			{
				// Track 0 or not in the key list
				return;
			}
			dynamic d = Statement;
			if (Statement.FunctionName == MapFunctionName.Position || (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.X))
			{
				int Index = RouteData.FindOrAddBlock(Statement.Distance);
				int LastInterpolateIndex = -1;
				if (Index > 0)
				{
					LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railIndex].InterpolateX);
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
						X = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RailX : 0.0;
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
						RadiusH = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RadiusH : 0.0;
					}
					else
					{
						RadiusH = d.Radius;
					}
				}

				RouteData.Blocks[Index].Rails[railIndex].RailX = Convert.ToDouble(X);
				RouteData.Blocks[Index].Rails[railIndex].RadiusH = Convert.ToDouble(RadiusH);
				RouteData.Blocks[Index].Rails[railIndex].InterpolateX = true;
			}

			if (Statement.FunctionName == MapFunctionName.Position || (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.Y))
			{
				int Index = RouteData.FindOrAddBlock(Statement.Distance);
				int LastInterpolateIndex = -1;
				if (Index > 0)
				{
					LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railIndex].InterpolateY);
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
						Y = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RailY : 0.0;
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
						RadiusV = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RadiusV : 0.0;
					}
					else
					{
						RadiusV = d.Radius;
					}
				}

				RouteData.Blocks[Index].Rails[railIndex].RailY = Convert.ToDouble(Y);
				RouteData.Blocks[Index].Rails[railIndex].RadiusV = Convert.ToDouble(RadiusV);
				RouteData.Blocks[Index].Rails[railIndex].InterpolateY = true;
			}

			if (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.Cant)
			{
				switch (Statement.FunctionName)
				{
					case MapFunctionName.BeginTransition:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[Index].Rails[railIndex].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails[railIndex].CurveTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[Index].Rails[railIndex].CurveCant = Statement.GetArgumentValueAsDouble(ArgumentName.Cant);
							RouteData.Blocks[Index].Rails[railIndex].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails[railIndex].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.End:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							RouteData.Blocks[Index].Rails[railIndex].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails[railIndex].CurveTransitionEnd = true;
						}
						break;
					case MapFunctionName.Interpolate:
						{
							int Index = RouteData.FindOrAddBlock(Statement.Distance);
							int LastInterpolateIndex = -1;
							if (Index > 0)
							{
								LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railIndex].CurveInterpolateEnd);
							}

							double Cant;
							if (d.Cant == null)
							{
								Cant = LastInterpolateIndex != -1
									? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].CurveCant
									: 0.0;
							}
							else
							{
								Cant = d.Cant;
							}

							RouteData.Blocks[Index].Rails[railIndex].CurveCant = Convert.ToDouble(Cant);
							RouteData.Blocks[Index].Rails[railIndex].CurveInterpolateStart = true;
							RouteData.Blocks[Index].Rails[railIndex].CurveInterpolateEnd = true;
							RouteData.Blocks[Index].Rails[railIndex].CurveTransitionEnd = true;
						}
						break;
				}
			}

			if (!RouteData.Blocks[0].Rails[railIndex].InterpolateX)
			{
				int FirstInterpolateIndex = -1;
				if (RouteData.Blocks.Count > 1)
				{
					FirstInterpolateIndex = RouteData.Blocks.FindIndex(1, Block => Block.Rails[railIndex].InterpolateX);
				}
				RouteData.Blocks[0].Rails[railIndex].RailX = FirstInterpolateIndex != -1 ? RouteData.Blocks[FirstInterpolateIndex].Rails[railIndex].RailX : 0.0;
				//Blocks[0].Rails[j].RadiusH = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusH : 0.0;
				RouteData.Blocks[0].Rails[railIndex].InterpolateX = true;
			}

			if (!RouteData.Blocks[0].Rails[railIndex].InterpolateY)
			{
				int FirstInterpolateIndex = -1;
				if (RouteData.Blocks.Count > 1)
				{
					FirstInterpolateIndex = RouteData.Blocks.FindIndex(1, Block => Block.Rails[railIndex].InterpolateY);
				}
				RouteData.Blocks[0].Rails[railIndex].RailY = FirstInterpolateIndex != -1 ? RouteData.Blocks[FirstInterpolateIndex].Rails[railIndex].RailY : 0.0;
				//Blocks[0].Rails[j].RadiusV = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusV : 0.0;
				RouteData.Blocks[0].Rails[railIndex].InterpolateY = true;
			}

			if (!RouteData.Blocks.Last().Rails[railIndex].InterpolateX)
			{
				int LastInterpolateIndex = -1;
				if (RouteData.Blocks.Count > 1)
				{
					LastInterpolateIndex = RouteData.Blocks.FindLastIndex(RouteData.Blocks.Count - 2, RouteData.Blocks.Count - 1, Block => Block.Rails[railIndex].InterpolateX);
				}
				RouteData.Blocks.Last().Rails[railIndex].RailX = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RailX : 0.0;
				RouteData.Blocks.Last().Rails[railIndex].RadiusH = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RadiusH : 0.0;
				RouteData.Blocks.Last().Rails[railIndex].InterpolateX = true;
			}

			if (!RouteData.Blocks.Last().Rails[railIndex].InterpolateY)
			{
				int LastInterpolateIndex = -1;
				if (RouteData.Blocks.Count > 1)
				{
					LastInterpolateIndex = RouteData.Blocks.FindLastIndex(RouteData.Blocks.Count - 2, RouteData.Blocks.Count - 1, Block => Block.Rails[railIndex].InterpolateY);
				}
				RouteData.Blocks.Last().Rails[railIndex].RailY = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RailY : 0.0;
				RouteData.Blocks.Last().Rails[railIndex].RadiusV = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railIndex].RadiusV : 0.0;
				RouteData.Blocks.Last().Rails[railIndex].InterpolateY = true;
			}

		}



		private static void ConvertTrack(RouteData RouteData)
		{
			// Own track is excluded.
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 1; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].Rails[j].InterpolateX)
					{
						continue;
					}

					int StartBlock = RouteData.Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].InterpolateX);

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartX = RouteData.Blocks[StartBlock].Rails[j].RailX;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndX = RouteData.Blocks[i].Rails[j].RailX;

						if (StartX == EndX)
						{
							continue;
						}

						double dist = RouteData.sortedBlocks.ElementAt(i).Key;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}

						i = RouteData.sortedBlocks.IndexOfKey(dist);
					}
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{


				for (int i = 1; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].Rails[j].InterpolateY)
					{
						continue;
					}

					int StartBlock = RouteData.Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].InterpolateY);

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartY = RouteData.Blocks[StartBlock].Rails[j].RailY;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndY = RouteData.Blocks[i].Rails[j].RailY;

						if (StartY == EndY)
						{
							continue;
						}

						double dist = RouteData.sortedBlocks.ElementAt(i).Key;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}

						i = RouteData.sortedBlocks.IndexOfKey(dist);
					}
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 0; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].Rails[j].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails[j].CurveTransitionEnd)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails[j].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = RouteData.Blocks[i].StartingDistance;

						double dist = RouteData.sortedBlocks.ElementAt(i).Key;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}

						i = RouteData.sortedBlocks.IndexOfKey(dist);
					}
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 0; i < RouteData.Blocks.Count; i++)
				{
					if (!RouteData.Blocks[i].Rails[j].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails[j].CurveInterpolateEnd && !RouteData.Blocks[k].Rails[j].CurveInterpolateStart)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails[j].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartCant = RouteData.Blocks[StartBlock].Rails[j].CurveCant;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndCant = RouteData.Blocks[i].Rails[j].CurveCant;

						if (StartCant == EndCant)
						{
							continue;
						}

						double dist = RouteData.sortedBlocks.ElementAt(i).Key;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}

						i = RouteData.sortedBlocks.IndexOfKey(dist);
					}
				}
			}
		}

		private static int CurrentStation = 0;

		private static void ConvertStation(Statement Statement, RouteData RouteData)
		{
			int Index = RouteData.StationList.FindIndex(Station => Station.Key.Equals(Statement.Key, StringComparison.InvariantCultureIgnoreCase));
			if (Index == -1)
			{
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
			NewStation.Name = RouteData.StationList[Index].Name;
			NewStation.ArrivalTime = RouteData.StationList[Index].ArrivalTime;
			RouteData.Sounds.TryGetValue(RouteData.StationList[Index].ArrivalSoundKey, out NewStation.ArrivalSoundBuffer);
			NewStation.StopMode = RouteData.StationList[Index].StopMode;
			NewStation.DepartureTime = RouteData.StationList[Index].DepartureTime;
			RouteData.Sounds.TryGetValue(RouteData.StationList[Index].DepartureSoundKey, out NewStation.DepartureSoundBuffer);
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
			RouteData.Blocks[StationBlockIndex].StationIndex = CurrentStation;

			int StopBlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
			RouteData.Blocks[StopBlockIndex].Stop = CurrentStation;

			Array.Resize(ref Plugin.CurrentRoute.Stations, CurrentStation + 1);
			Plugin.CurrentRoute.Stations[CurrentStation] = NewStation;
			CurrentStation++;

		}

		private static void ConvertBackground(Statement Statement, RouteData RouteData)
		{
			dynamic d = Statement;

			object BackgroundKey = d.StructureKey;

			int BackgroundIndex = RouteData.Backgrounds.FindIndex(Background => Background.Key.Equals(Convert.ToString(BackgroundKey), StringComparison.InvariantCultureIgnoreCase));

			if (BackgroundIndex == -1)
			{
				RouteData.Objects.TryGetValue(Convert.ToString(BackgroundKey), out UnifiedObject Object);

				if (Object != null)
				{
					BackgroundIndex = RouteData.Backgrounds.Count;
					RouteData.Backgrounds.Add(new Background(Convert.ToString(BackgroundKey), new BackgroundObject((StaticObject)Object)));
				}
			}

			if (BackgroundIndex != -1)
			{
				int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
				RouteData.Blocks[BlockIndex].BackgroundIndex = BackgroundIndex;
			}
			else
			{
				//Failed to load the background
			}

		}

		private static void ConvertFog(Statement Statement, RouteData RouteData)
		{
			switch (Statement.FunctionName)
			{
				case MapFunctionName.Fog:
				{
					double Start = Statement.GetArgumentValueAsDouble(ArgumentName.Start);
					double End = Statement.GetArgumentValueAsDouble(ArgumentName.End);
					double TempRed, TempGreen, TempBlue;
					if (!Statement.HasArgument(ArgumentName.Red) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Red), out TempRed))
					{
						TempRed = 128;
					}

					if (!Statement.HasArgument(ArgumentName.Green) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Green), out TempGreen))
					{
						TempGreen = 128;
					}

					if (!Statement.HasArgument(ArgumentName.Blue) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Blue), out TempBlue))
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
						RouteData.Blocks[BlockIndex].Fog.Start = (float)Start;
						RouteData.Blocks[BlockIndex].Fog.End = (float)End;
					}
					else
					{
						RouteData.Blocks[BlockIndex].Fog.Start = Plugin.CurrentRoute.NoFogStart;
						RouteData.Blocks[BlockIndex].Fog.End = Plugin.CurrentRoute.NoFogEnd;
					}

					RouteData.Blocks[BlockIndex].Fog.Color = new Color24((byte)Red, (byte)Green, (byte)Blue);
					RouteData.Blocks[BlockIndex].FogDefined = true;
				}
					break;
				case MapFunctionName.Interpolate:
				case MapFunctionName.Set:
				{
					double Density, TempRed, TempGreen, TempBlue;
					if (!Statement.HasArgument(ArgumentName.Density) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Density), out Density))
					{
						Density = 0.001;
					}

					if (!Statement.HasArgument(ArgumentName.Red) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Red), out TempRed))
					{
						TempRed = 1.0;
					}

					if (!Statement.HasArgument(ArgumentName.Green) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Green), out TempGreen))
					{
						TempGreen = 1.0;
					}

					if (!Statement.HasArgument(ArgumentName.Blue) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Blue), out TempBlue))
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

					RouteData.Blocks[BlockIndex].Fog.Start = 0.0f;
					RouteData.Blocks[BlockIndex].Fog.End = 1.0f / Convert.ToSingle(Density);
					RouteData.Blocks[BlockIndex].Fog.Color = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
					RouteData.Blocks[BlockIndex].FogDefined = true;
				}
					break;
			}
		}

		private static void ConvertIrregularity(Statement Statement, RouteData RouteData)
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
			RouteData.Blocks[BlockIndex].Accuracy = Accuracy;
			RouteData.Blocks[BlockIndex].AccuracyDefined = true;

			if (!RouteData.Blocks.Last().AccuracyDefined)
			{
				int LastDefinedIndex = -1;
				if (RouteData.Blocks.Count > 1)
				{
					LastDefinedIndex = RouteData.Blocks.FindLastIndex(RouteData.Blocks.Count - 2, RouteData.Blocks.Count - 1, Block => Block.AccuracyDefined);
				}

				RouteData.Blocks.Last().Accuracy = LastDefinedIndex != -1 ? RouteData.Blocks[LastDefinedIndex].Accuracy : 2.0;
				RouteData.Blocks.Last().AccuracyDefined = true;
			}
		}

		private static void ConvertAdhesion(Statement Statement, RouteData RouteData)
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
					RouteData.Blocks[BlockIndex].AdhesionMultiplier = (int)(Statement.GetArgumentValueAsDouble(ArgumentName.A) * 100 / 0.26) / 100.0;
					RouteData.Blocks[BlockIndex].AdhesionMultiplierDefined = true;
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
						RouteData.Blocks[BlockIndex].AdhesionMultiplierDefined = true;

						if (C0 == 0.35 && C2 == 0.01)
						{
							//Default adhesion value of 100
							RouteData.Blocks[BlockIndex].AdhesionMultiplier = 1;
							return;
						}

						int CA = (int)(C0 * 100 / 0.26);
						double CB = 1.0 / (300 * (CA / 100.0 * 0.259999990463257));

						if (Math.Round(CB, 8) == C2)
						{
							//BVE2 / 4 converted value, so let's use that
							RouteData.Blocks[BlockIndex].AdhesionMultiplier = CA / 100.0;
							return;
						}

						//They don't match.....
						//Use an average of the two to get something reasonable
						//TODO: Implement adhesion based upon speed formula
						RouteData.Blocks[BlockIndex].AdhesionMultiplier = (CA + CB) / 2 / 100;
					}
				}
					break;
			}

			if (!RouteData.Blocks.Last().AdhesionMultiplierDefined)
			{
				int LastDefinedIndex = -1;
				if (RouteData.Blocks.Count > 1)
				{
					LastDefinedIndex = RouteData.Blocks.FindLastIndex(RouteData.Blocks.Count - 2, RouteData.Blocks.Count - 1, Block => Block.AdhesionMultiplierDefined);
				}
				RouteData.Blocks.Last().AdhesionMultiplier = LastDefinedIndex != -1 ? RouteData.Blocks[LastDefinedIndex].AdhesionMultiplier : 1.0;
				RouteData.Blocks.Last().AdhesionMultiplierDefined = true;
			}
		}

		private static void ConvertJointNoise(Statement Statement, RouteData RouteData)
		{
			int BlockIndex = RouteData.FindOrAddBlock(Statement.Distance);
			RouteData.Blocks[BlockIndex].JointSound = true;
		}
	}
}
