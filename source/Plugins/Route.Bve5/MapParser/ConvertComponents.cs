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
using System.Linq;
using Bve5_Parsing.MapGrammar;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using RouteManager2.Climate;
using RouteManager2.Stations;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static double lastLegacyCurvePosition = double.MinValue;
		private static double lastLegacyGradientPosition = double.MinValue;
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

							if (Gauge > 1000)
							{
								/*
								 * BVE5 documentation states that gauge should be in M
								 *
								 * However, some routes actually use MM.
								 * No practical effect (other than basically zero roll) but correct.
								 */
								Gauge /= 1000;
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
							RouteData.TryAddBlock(Statement.Distance);
							RouteData.sortedBlocks[Statement.Distance].Rails["0"].CurveInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].Rails["0"].CurveTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
					case MapFunctionName.BeginCircular:
					case MapFunctionName.Change:
					case MapFunctionName.Curve:
						{
							/*
							 * NOTE: The behaviour of Legacy commands is not properly documented, and *does not* match prior versions of BVE.
							 *		 From observation of BVE5 behavior:
							 *		 * When the distance between curves is greater than 25m, interpolation is used from -10m to +15m of the statement position
							 *		 * Otherwise, it's somewhat glitchy, e.g. https://github.com/leezer3/OpenBVE/issues/1045#issuecomment-2275286076
							 *         For the minute, let's assume that is fundamentally bugged, and just issue an immediate change in this case
							 */
							double Radius = d.Radius;
							double Cant = Statement.GetArgumentValueAsDouble(ArgumentName.Cant) / 1000.0;
							if (Statement.ElementName == MapElementName.Legacy && Statement.Distance - lastLegacyCurvePosition >= 15)
							{
								int firstIndex = RouteData.FindOrAddBlock(Math.Max(0, Statement.Distance - 10));
								int secondIndex = RouteData.FindOrAddBlock(Statement.Distance + 15);
								RouteData.Blocks[secondIndex].CurrentTrackState.CurveRadius = Radius;
								RouteData.Blocks[secondIndex].CurrentTrackState.CurveCant = Math.Abs(Cant) * Math.Sign(Radius);
								RouteData.Blocks[firstIndex].Rails["0"].CurveInterpolateStart = true;
								RouteData.Blocks[firstIndex].Rails["0"].CurveInterpolateEnd = true;
								RouteData.Blocks[secondIndex].Rails["0"].CurveInterpolateEnd = true;
								RouteData.Blocks[secondIndex].Rails["0"].CurveTransitionEnd = true;
							}
							else
							{
								int Index = RouteData.FindOrAddBlock(Statement.Distance);
								RouteData.Blocks[Index].CurrentTrackState.CurveRadius = Radius;
								RouteData.Blocks[Index].CurrentTrackState.CurveCant = Math.Abs(Cant) * Math.Sign(Radius);
								RouteData.Blocks[Index].Rails["0"].CurveInterpolateStart = true;
								RouteData.Blocks[Index].Rails["0"].CurveTransitionEnd = true;
							}

							if (Statement.ElementName == MapElementName.Legacy)
							{
								lastLegacyCurvePosition = Statement.Distance;
							}

						}
						break;
					case MapFunctionName.End:
						{
							RouteData.TryAddBlock(Statement.Distance);
							RouteData.sortedBlocks[Statement.Distance].Rails["0"].CurveInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].Rails["0"].CurveTransitionEnd = true;
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
							RouteData.TryAddBlock(Statement.Distance);
							RouteData.sortedBlocks[Statement.Distance].Turn = Convert.ToDouble(Slope);
						}
						break;
				}
			}

			{
				int i = 0;
				while (i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].Rails["0"].CurveTransitionEnd)
					{
						i++;
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails["0"].CurveTransitionEnd)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails["0"].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					// save distance of current block idx
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

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}

			{

				int i = 0;
				while (i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].Rails["0"].CurveInterpolateEnd)
					{
						i++;
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails["0"].CurveInterpolateEnd && !RouteData.Blocks[k].Rails["0"].CurveInterpolateStart)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails["0"].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					// save distance of current block idx
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
							i++;
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.TryAddBlock(k);
						}
					}

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}
		}

		private static void ConvertGradient(Statement Statement, RouteData RouteData)
		{
			{
				dynamic d = Statement;
				switch (Statement.FunctionName)
				{
					case MapFunctionName.BeginTransition:
						{
							RouteData.TryAddBlock(Statement.Distance);
							RouteData.sortedBlocks[Statement.Distance].GradientInterpolateStart = true;
							RouteData.sortedBlocks[Statement.Distance].GradientTransitionStart = true;
						}
						break;
					case MapFunctionName.Begin:
					case MapFunctionName.BeginConst:
					case MapFunctionName.Pitch:
						{
							object newGradientValue = Statement.FunctionName == MapFunctionName.Pitch ? d.Rate : d.Gradient;
							/*
							 * NOTE: The behaviour of Legacy commands is not properly documented, and *does not* match prior versions of BVE.
							 *		 From observation of BVE5 behavior:
							 *		 * When the distance between gradients is greater than 25m, interpolation is used from -10m to +15m of the statement position
							 *		 * Otherwise, it's somewhat glitchy, e.g. https://github.com/leezer3/OpenBVE/issues/1045#issuecomment-2275286076
							 *         For the minute, let's assume that is fundamentally bugged, and just issue an immediate change in this case
							 */
							if (Statement.ElementName == MapElementName.Legacy && Statement.Distance - lastLegacyGradientPosition >= 15)
							{
								int firstIndex = RouteData.FindOrAddBlock(Math.Max(0, Statement.Distance - 10));
								int secondIndex = RouteData.FindOrAddBlock(Statement.Distance + 15);
								RouteData.Blocks[secondIndex].Pitch = Convert.ToDouble(newGradientValue) / 1000.0;
								RouteData.Blocks[firstIndex].GradientInterpolateStart = true;
								RouteData.Blocks[firstIndex].GradientInterpolateEnd = true;
								RouteData.Blocks[secondIndex].GradientInterpolateEnd = true;
								RouteData.Blocks[secondIndex].GradientTransitionEnd = true;
							}
							else
							{
								int Index = RouteData.FindOrAddBlock(Statement.Distance);
								RouteData.Blocks[Index].Pitch = Convert.ToDouble(newGradientValue) / 1000.0;
								RouteData.Blocks[Index].GradientInterpolateStart = true;
								RouteData.Blocks[Index].GradientTransitionEnd = true;
							}

							if (Statement.ElementName == MapElementName.Legacy)
							{
								lastLegacyGradientPosition = Statement.Distance;
							}

						}
						break;
					case MapFunctionName.End:
						{
							RouteData.TryAddBlock(Statement.Distance);
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
								LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.GradientInterpolateEnd);
							}

							double Gradient;
							if (d.Gradient == null)
							{
								Gradient = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Pitch * 1000.0 : 0.0;
							}
							else
							{
								Gradient = d.Gradient;
							}

							RouteData.Blocks[Index].Pitch = Convert.ToDouble(Gradient) / 1000.0;
							RouteData.Blocks[Index].GradientInterpolateStart = true;
							RouteData.Blocks[Index].GradientInterpolateEnd = true;
							RouteData.Blocks[Index].GradientTransitionEnd = true;
						}
						break;
				}
			}

			{

				int i = 0;
				while (i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].GradientTransitionEnd)
					{
						i++;
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

					// save distance of current block idx
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

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}

			{
				int i = 0;
				while (i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].GradientInterpolateEnd)
					{
						i++;
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

					// save distance of current block idx
					double dist = RouteData.sortedBlocks.ElementAt(i).Key;

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartPitch = RouteData.Blocks[StartBlock].Pitch;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndPitch = RouteData.Blocks[i].Pitch;

						if (StartPitch == EndPitch)
						{
							i++;
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}
		}

		private static void ConvertTrack(MapData ParseData, RouteData RouteData)
		{

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
							LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railKey].InterpolateX);
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
								X = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].Position.X : 0.0;
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
								RadiusH = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].RadiusH : 0.0;
							}
							else
							{
								RadiusH = d.Radius;
							}
						}

						RouteData.Blocks[Index].Rails[railKey].Position.X = Convert.ToDouble(X);
						RouteData.Blocks[Index].Rails[railKey].RadiusH = Convert.ToDouble(RadiusH);
						RouteData.Blocks[Index].Rails[railKey].InterpolateX = true;
					}

					if (Statement.FunctionName == MapFunctionName.Position || (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.Y))
					{
						int Index = RouteData.FindOrAddBlock(Statement.Distance);
						int LastInterpolateIndex = -1;
						if (Index > 0)
						{
							LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railKey].InterpolateY);
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
								Y = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].Position.Y : 0.0;
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
								RadiusV = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].RadiusV : 0.0;
							}
							else
							{
								RadiusV = d.Radius;
							}
						}

						RouteData.Blocks[Index].Rails[railKey].Position.Y = Convert.ToDouble(Y);
						RouteData.Blocks[Index].Rails[railKey].RadiusV = Convert.ToDouble(RadiusV);
						RouteData.Blocks[Index].Rails[railKey].InterpolateY = true;
					}

					if (Statement.HasSubElement && Statement.SubElementName == MapSubElementName.Cant)
					{
						switch (Statement.FunctionName)
						{
							case MapFunctionName.BeginTransition:
								{
									RouteData.TryAddBlock(Statement.Distance);
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveInterpolateStart = true;
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveTransitionStart = true;
								}
								break;
							case MapFunctionName.Begin:
								{
									RouteData.TryAddBlock(Statement.Distance);
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveCant = Statement.GetArgumentValueAsDouble(ArgumentName.Cant) / 1000.0;
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveInterpolateStart = true;
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveTransitionEnd = true;
								}
								break;
							case MapFunctionName.End:
								{
									RouteData.TryAddBlock(Statement.Distance);
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveInterpolateStart = true;
									RouteData.sortedBlocks[Statement.Distance].Rails[railKey].CurveTransitionEnd = true;
								}
								break;
							case MapFunctionName.Interpolate:
								{
									int Index = RouteData.FindOrAddBlock(Statement.Distance);
									int LastInterpolateIndex = -1;
									if (Index > 0)
									{
										LastInterpolateIndex = RouteData.Blocks.FindLastIndex(Index - 1, Index, Block => Block.Rails[railKey].CurveInterpolateEnd);
									}

									double Cant;
									if (d.Cant == null)
									{
										Cant = LastInterpolateIndex != -1
											? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].CurveCant
											: 0.0;
									}
									else
									{
										Cant = d.Cant / 1000.0;
									}

									RouteData.Blocks[Index].Rails[railKey].CurveCant = Cant;
									RouteData.Blocks[Index].Rails[railKey].CurveInterpolateStart = true;
									RouteData.Blocks[Index].Rails[railKey].CurveInterpolateEnd = true;
									RouteData.Blocks[Index].Rails[railKey].CurveTransitionEnd = true;
								}
								break;
						}
					}
				}

				if (!RouteData.Blocks.First().Rails[railKey].InterpolateX)
				{
					int FirstInterpolateIndex = -1;
					if (RouteData.Blocks.Count > 1)
					{
						FirstInterpolateIndex = RouteData.Blocks.FindIndex(1, Block => Block.Rails[railKey].InterpolateX);
					}
					RouteData.Blocks.First().Rails[railKey].Position.X = FirstInterpolateIndex != -1 ? RouteData.Blocks[FirstInterpolateIndex].Rails[railKey].Position.X : 0.0;
					//Blocks.First().Rails[j].RadiusH = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusH : 0.0;
					RouteData.Blocks.First().Rails[railKey].InterpolateX = true;
				}

				if (!RouteData.Blocks.First().Rails[railKey].InterpolateY)
				{
					int FirstInterpolateIndex = -1;
					if (RouteData.Blocks.Count > 1)
					{
						FirstInterpolateIndex = RouteData.Blocks.FindIndex(1, Block => Block.Rails[railKey].InterpolateY);
					}
					RouteData.Blocks.First().Rails[railKey].Position.Y = FirstInterpolateIndex != -1 ? RouteData.Blocks[FirstInterpolateIndex].Rails[railKey].Position.Y : 0.0;
					//Blocks.First().Rails[j].RadiusV = FirstInterpolateIndex != -1 ? Blocks[FirstInterpolateIndex].Rails[j].RadiusV : 0.0;
					RouteData.Blocks.First().Rails[railKey].InterpolateY = true;
				}

				if (!RouteData.Blocks.Last().Rails[railKey].InterpolateX)
				{
					int LastInterpolateIndex = -1;
					if (RouteData.Blocks.Count > 1)
					{
						LastInterpolateIndex = RouteData.Blocks.FindLastIndex(RouteData.Blocks.Count - 2, RouteData.Blocks.Count - 1, Block => Block.Rails[railKey].InterpolateX);
					}
					RouteData.Blocks.Last().Rails[railKey].Position.X = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].Position.X : 0.0;
					RouteData.Blocks.Last().Rails[railKey].RadiusH = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].RadiusH : 0.0;
					RouteData.Blocks.Last().Rails[railKey].InterpolateX = true;
				}

				if (!RouteData.Blocks.Last().Rails[railKey].InterpolateY)
				{
					int LastInterpolateIndex = -1;
					if (RouteData.Blocks.Count > 1)
					{
						LastInterpolateIndex = RouteData.Blocks.FindLastIndex(RouteData.Blocks.Count - 2, RouteData.Blocks.Count - 1, Block => Block.Rails[railKey].InterpolateY);
					}
					RouteData.Blocks.Last().Rails[railKey].Position.Y = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].Position.Y : 0.0;
					RouteData.Blocks.Last().Rails[railKey].RadiusV = LastInterpolateIndex != -1 ? RouteData.Blocks[LastInterpolateIndex].Rails[railKey].RadiusV : 0.0;
					RouteData.Blocks.Last().Rails[railKey].InterpolateY = true;
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				int i = 1;
				while (i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].Rails[railKey].InterpolateX)
					{
						i++;
						continue;
					}

					int StartBlock = RouteData.Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[railKey].InterpolateX);

					// save distance of current block idx
					double dist = RouteData.sortedBlocks.Keys[i];

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartX = RouteData.Blocks[StartBlock].Rails[railKey].Position.X;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndX = RouteData.Blocks[i].Rails[railKey].Position.X;

						if (StartX == EndX)
						{
							i++;
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				int i = 1;
				while(i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].Rails[railKey].InterpolateY)
					{
						i++;
						continue;
					}

					int StartBlock = RouteData.Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[railKey].InterpolateY);

					// save distance of current block idx
					double dist = RouteData.sortedBlocks.Keys[i];

					if (StartBlock != -1)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartY = RouteData.Blocks[StartBlock].Rails[railKey].Position.Y;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndY = RouteData.Blocks[i].Rails[railKey].Position.Y;

						if (StartY == EndY)
						{
							i++;
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				int i = 0;
				while(i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].Rails[railKey].CurveTransitionEnd)
					{
						i++;
						continue;
					}

					int StartBlock = i;

					// save distance of current block idx
					double dist = RouteData.sortedBlocks.Keys[i];

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails[railKey].CurveTransitionEnd)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails[railKey].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double EndDistance = RouteData.Blocks[i].StartingDistance;

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
				}
			}

			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				int i = 0;
				while (i < RouteData.Blocks.Count)
				{
					if (!RouteData.Blocks[i].Rails[railKey].CurveInterpolateEnd)
					{
						i++;
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (RouteData.Blocks[k].Rails[railKey].CurveInterpolateEnd && !RouteData.Blocks[k].Rails[railKey].CurveInterpolateStart)
						{
							break;
						}

						if (RouteData.Blocks[k].Rails[railKey].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					double dist = RouteData.sortedBlocks.ElementAt(i).Key;

					if (StartBlock != i)
					{
						double StartDistance = Multiple(RouteData.Blocks[StartBlock].StartingDistance, InterpolateInterval);
						double StartCant = RouteData.Blocks[StartBlock].Rails[railKey].CurveCant;
						double EndDistance = RouteData.Blocks[i].StartingDistance;
						double EndCant = RouteData.Blocks[i].Rails[railKey].CurveCant;

						if (StartCant == EndCant)
						{
							i++;
							continue;
						}

						for (double k = StartDistance; k < EndDistance; k += InterpolateInterval)
						{
							RouteData.FindOrAddBlock(k);
						}
					}

					// now use distance to retrieve the *new* index of said block after insertions (+1 to carry on with loop)
					i = RouteData.sortedBlocks.IndexOfKey(dist) + 1;
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
				
				RouteStation NewStation = new RouteStation(stationKey);
				RouteData.StationList[stationKey].Create(ref NewStation);
				RouteData.Sounds.TryGetValue(RouteData.StationList[stationKey].ArrivalSoundKey, out NewStation.ArrivalSoundBuffer);
				RouteData.Sounds.TryGetValue(RouteData.StationList[stationKey].DepartureSoundKey, out NewStation.DepartureSoundBuffer);
				NewStation.OpenLeftDoors = Convert.ToDouble(Doors) < 0.0;
				NewStation.OpenRightDoors = Convert.ToDouble(Doors) > 0.0;
				NewStation.Stops = new StationStop[1];
				NewStation.Stops[0].TrackPosition = Statement.Distance;
				NewStation.Stops[0].BackwardTolerance = Math.Abs(Convert.ToDouble(BackwardTolerance));
				NewStation.Stops[0].ForwardTolerance = Convert.ToDouble(ForwardTolerance);
				NewStation.DefaultTrackPosition = DefaultTrackPosition;
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
					if(RouteData.Objects.TryGetValue(Convert.ToString(BackgroundKey), out UnifiedObject unifiedObject) && unifiedObject != null)
					{
						RouteData.Backgrounds.Add(Convert.ToString(BackgroundKey), unifiedObject);
					}
				}

				if (RouteData.Backgrounds.ContainsKey(BackgroundKey))
				{
					RouteData.TryAddBlock(Statement.Distance);
					RouteData.sortedBlocks[Statement.Distance].Background = BackgroundKey;
				}
			}
		}

		private static void ConvertFog(Statement Statement, RouteData RouteData)
		{
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

							if (Start > End)
							{
								Start = Plugin.CurrentRoute.NoFogStart;
								End = Plugin.CurrentRoute.NoFogEnd;
							}

							RouteData.Blocks[BlockIndex].Fog = new Fog((float)Start, (float)End, new Color24((byte)Red, (byte)Green, (byte)Blue), Statement.Distance);
							RouteData.Blocks[BlockIndex].FogDefined = true;
						}
						break;
					case MapFunctionName.Interpolate:
					case MapFunctionName.Set:
					{
						RouteData.TryAddBlock(Statement.Distance);
						float Density = 1.0f, r = 1.0f, g = 1.0f, b = 1.0f;
						if (!Statement.HasArgument(ArgumentName.Red) && !Statement.HasArgument(ArgumentName.Green) && !Statement.HasArgument(ArgumentName.Blue))
						{
							if (!Statement.HasArgument(ArgumentName.Density))
							{
								// Has the same effect as setting the fog value twice, hence no interpolation
								RouteData.sortedBlocks[Statement.Distance].FogDefined = true;
								RouteData.sortedBlocks[Statement.Distance].Fog.TrackPosition = Statement.Distance;
							}
							else
							{
								// Changes the density, but not the color
								if (!NumberFormats.TryParseFloatVb6(Statement.GetArgumentValueAsString(ArgumentName.Density), out Density))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FogDensity value at track position " + Statement.Distance);
									break;
								}
								RouteData.sortedBlocks[Statement.Distance].FogDefined = true;
								RouteData.sortedBlocks[Statement.Distance].Fog.TrackPosition = Statement.Distance;
								RouteData.sortedBlocks[Statement.Distance].Fog.Density = Density;
							}
						}
						else
						{
							if (!NumberFormats.TryParseFloatVb6(Statement.GetArgumentValueAsString(ArgumentName.Density), out Density))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FogDensity value at track position " + Statement.Distance);
								break;
							}
							if (!Statement.HasArgument(ArgumentName.Red) || !NumberFormats.TryParseFloatVb6(Statement.GetArgumentValueAsString(ArgumentName.Red), out r))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FogColor R value at track position " + Statement.Distance);
							}
							if (!Statement.HasArgument(ArgumentName.Green) || !NumberFormats.TryParseFloatVb6(Statement.GetArgumentValueAsString(ArgumentName.Green), out g))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FogColor G value at track position " + Statement.Distance);
							}
							if (!Statement.HasArgument(ArgumentName.Blue) || !NumberFormats.TryParseFloatVb6(Statement.GetArgumentValueAsString(ArgumentName.Blue), out b))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FogColor B value at track position " + Statement.Distance);
							}
						}

						RouteData.sortedBlocks[Statement.Distance].Fog = new Fog(0, 1, new Color24((byte)(r * 255), (byte)(g * 255), (byte)(b * 255)), Statement.Distance, false, Density);
						RouteData.sortedBlocks[Statement.Distance].FogDefined = true;
					}
					break;
				}
			}
		}

		private static void ConvertIrregularity(Statement Statement, RouteData RouteData)
		{
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
			}

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
