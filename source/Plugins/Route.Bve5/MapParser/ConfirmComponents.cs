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
using OpenBveApi.Math;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void ConfirmCurve(IList<Block> Blocks)
		{
			// Set a tentative value to a block that has not been decided.
			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!Blocks[i].Rails[0].CurveInterpolateStart && !Blocks[i].Rails[0].CurveInterpolateEnd)
				{
					continue;
				}

				int LastConfirmBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[0].CurveInterpolateStart || Block.Rails[0].CurveInterpolateEnd);

				if (LastConfirmBlock != -1)
				{
					for (int k = LastConfirmBlock + 1; k < i; k++)
					{
						Blocks[k].CurrentTrackState.CurveRadius = Blocks[LastConfirmBlock].CurrentTrackState.CurveRadius;
						Blocks[k].CurrentTrackState.CurveCant = Blocks[LastConfirmBlock].CurrentTrackState.CurveCant;
					}
				}
			}

			// Curve transition
			for (int i = 0; i < Blocks.Count; i++)
			{
				if (!Blocks[i].Rails[0].CurveTransitionEnd)
				{
					continue;
				}

				int StartBlock = i;

				for (int k = i - 1; k >= 1; k--)
				{
					if (Blocks[k].Rails[0].CurveTransitionEnd)
					{
						break;
					}

					if (Blocks[k].Rails[0].CurveTransitionStart)
					{
						StartBlock = k;
						break;
					}
				}

				if (StartBlock != i)
				{
					double StartDistance = Blocks[StartBlock].StartingDistance;
					double StartRadius = StartBlock != 0 ? Blocks[StartBlock - 1].CurrentTrackState.CurveRadius : Blocks[0].CurrentTrackState.CurveRadius;
					double StartCant = StartBlock != 0 ? Blocks[StartBlock - 1].CurrentTrackState.CurveCant : Blocks[0].CurrentTrackState.CurveCant;
					double EndDistance = Blocks[i].StartingDistance;
					double EndRadius = Blocks[i].CurrentTrackState.CurveRadius;
					double EndCant = Blocks[i].CurrentTrackState.CurveCant;

					for (int k = StartBlock; k < i; k++)
					{
						double CurrentDistance = Blocks[k].StartingDistance;
						CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out Blocks[k].CurrentTrackState.CurveRadius, out Blocks[k].CurrentTrackState.CurveCant);
					}
				}
			}

			// Curve interpolate
			for (int i = 0; i < Blocks.Count; i++)
			{
				if (!Blocks[i].Rails[0].CurveInterpolateEnd)
				{
					continue;
				}

				int StartBlock = i;

				for (int k = i - 1; k >= 0; k--)
				{
					if (Blocks[k].Rails[0].CurveInterpolateEnd && !Blocks[k].Rails[0].CurveInterpolateStart)
					{
						break;
					}

					if (Blocks[k].Rails[0].CurveInterpolateStart)
					{
						StartBlock = k;
						break;
					}
				}

				if (StartBlock != i)
				{
					double StartDistance = Blocks[StartBlock].StartingDistance;
					double StartRadius = Blocks[StartBlock].CurrentTrackState.CurveRadius;
					double StartCant = Blocks[StartBlock].CurrentTrackState.CurveCant;
					double EndDistance = Blocks[i].StartingDistance;
					double EndRadius = Blocks[i].CurrentTrackState.CurveRadius;
					double EndCant = Blocks[i].CurrentTrackState.CurveCant;

					for (int k = StartBlock + 1; k < i; k++)
					{
						double CurrentDistance = Blocks[k].StartingDistance;
						CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out Blocks[k].CurrentTrackState.CurveRadius, out Blocks[k].CurrentTrackState.CurveCant);
					}
				}
			}
		}

		private static void ConfirmGradient(IList<Block> Blocks)
		{
			// Set a tentative value to a block that has not been decided.
			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!Blocks[i].GradientInterpolateStart && !Blocks[i].GradientInterpolateEnd)
				{
					continue;
				}

				int LastConfirmBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.GradientInterpolateStart || Block.GradientInterpolateEnd);

				if (LastConfirmBlock != -1)
				{
					for (int k = LastConfirmBlock + 1; k < i; k++)
					{
						Blocks[k].Pitch = Blocks[LastConfirmBlock].Pitch;
					}
				}
			}

			// Gradient transition
			// Provisional implementation
			for (int i = 0; i < Blocks.Count; i++)
			{
				if (!Blocks[i].GradientTransitionEnd)
				{
					continue;
				}

				int StartBlock = i;

				for (int k = i - 1; k >= 0; k--)
				{
					if (Blocks[k].GradientTransitionEnd)
					{
						break;
					}

					if (Blocks[k].GradientTransitionStart)
					{
						StartBlock = k;
						break;
					}
				}

				if (StartBlock != i)
				{
					double StartDistance = Blocks[StartBlock].StartingDistance;
					double StartPitch = StartBlock != 0 ? Blocks[StartBlock - 1].Pitch : Blocks[0].Pitch;
					double EndDistance = Blocks[i].StartingDistance;
					double EndPitch = Blocks[i].Pitch;

					for (int k = StartBlock; k < i; k++)
					{
						double CurrentDistance = Blocks[k].StartingDistance;
						double CurrentPitch = LinearInterpolation(StartDistance, StartPitch, EndDistance, EndPitch, CurrentDistance);

						Blocks[k].Pitch = CurrentPitch;
					}
				}
			}

			// Gradient interpolate
			for (int i = 0; i < Blocks.Count; i++)
			{
				if (!Blocks[i].GradientInterpolateEnd)
				{
					continue;
				}

				int StartBlock = i;

				for (int k = i - 1; k >= 0; k--)
				{
					if (Blocks[k].GradientInterpolateEnd && !Blocks[k].GradientInterpolateStart)
					{
						break;
					}

					if (Blocks[k].GradientInterpolateStart)
					{
						StartBlock = k;
						break;
					}
				}

				if (StartBlock != i)
				{
					double StartDistance = Blocks[StartBlock].StartingDistance;
					double StartPitch = Blocks[StartBlock].Pitch;
					double EndDistance = Blocks[i].StartingDistance;
					double EndPitch = Blocks[i].Pitch;

					for (int k = StartBlock + 1; k < i; k++)
					{
						double CurrentDistance = Blocks[k].StartingDistance;
						double CurrentPitch = LinearInterpolation(StartDistance, StartPitch, EndDistance, EndPitch, CurrentDistance);

						Blocks[k].Pitch = CurrentPitch;
					}
				}
			}
		}

		private static void ConfirmTrack(RouteData RouteData)
		{
			IList<Block> Blocks = RouteData.Blocks;

			for (int j = 0; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 1; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[j].InterpolateX)
					{
						continue;
					}

					int StartBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].InterpolateX);

					if (StartBlock != -1)
					{
						double StartDistance = Blocks[StartBlock].StartingDistance;
						double StartX = Blocks[StartBlock].Rails[j].RailX;
						double EndDistance = Blocks[i].StartingDistance;
						double EndX = Blocks[i].Rails[j].RailX;
						double RadiusH = Blocks[StartBlock].Rails[j].RadiusH;

						for (int k = StartBlock + 1; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							double CurrentX = GetTrackCoordinate(StartDistance, StartX, EndDistance, EndX, RadiusH, CurrentDistance);

							Blocks[k].Rails[j].RailX = CurrentX;
							Blocks[k].Rails[j].RadiusH = RadiusH;
						}
					}
				}
			}

			for (int j = 0; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 1; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[j].InterpolateY)
					{
						continue;
					}

					int StartBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].InterpolateY);

					if (StartBlock != -1)
					{
						double StartDistance = Blocks[StartBlock].StartingDistance;
						double StartY = Blocks[StartBlock].Rails[j].RailY;
						double EndDistance = Blocks[i].StartingDistance;
						double EndY = Blocks[i].Rails[j].RailY;
						double RadiusV = Blocks[StartBlock].Rails[j].RadiusV;

						for (int k = StartBlock + 1; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							double CurrentY = GetTrackCoordinate(StartDistance, StartY, EndDistance, EndY, RadiusV, CurrentDistance);

							Blocks[k].Rails[j].RailY = CurrentY;
							Blocks[k].Rails[j].RadiusV = RadiusV;
						}
					}
				}
			}

			// Set a tentative value to a block that has not been decided.
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 1; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[j].CurveInterpolateStart && !Blocks[i].Rails[j].CurveInterpolateEnd)
					{
						continue;
					}

					int LastConfirmBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.Rails[j].CurveInterpolateStart || Block.Rails[j].CurveInterpolateEnd);

					if (LastConfirmBlock != -1)
					{
						for (int k = LastConfirmBlock + 1; k < i; k++)
						{
							Blocks[k].Rails[j].CurveCant = Blocks[LastConfirmBlock].Rails[j].CurveCant;
						}
					}
				}
			}

			// Curve transition
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 0; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[j].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 1; k--)
					{
						if (Blocks[k].Rails[j].CurveTransitionEnd)
						{
							break;
						}

						if (Blocks[k].Rails[j].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Blocks[StartBlock].StartingDistance;
						double StartRadius = 0.0;
						double StartCant = Blocks[StartBlock - 1].Rails[j].CurveCant;
						double EndDistance = Blocks[i].StartingDistance;
						double EndRadius = 0.0;
						double EndCant = Blocks[i].Rails[j].CurveCant;

						for (int k = StartBlock; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out _, out Blocks[k].Rails[j].CurveCant);
						}
					}
				}
			}

			// Curve interpolate
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				for (int i = 0; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[j].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (Blocks[k].Rails[j].CurveInterpolateEnd && !Blocks[k].Rails[j].CurveInterpolateStart)
						{
							break;
						}

						if (Blocks[k].Rails[j].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Blocks[StartBlock].StartingDistance;
						double StartRadius = 0.0;
						double StartCant = Blocks[StartBlock].Rails[j].CurveCant;
						double EndDistance = Blocks[i].StartingDistance;
						double EndRadius = 0.0;
						double EndCant = Blocks[i].Rails[j].CurveCant;

						for (int k = StartBlock + 1; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out _, out Blocks[k].Rails[j].CurveCant);
						}
					}
				}
			}
		}

		private static void ConfirmStructure(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			IList<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Structure)
				{
					continue;
				}

				switch (Statement.FunctionName)
				{
					case MapFunctionName.Put:
					case MapFunctionName.Put0:
					{
						dynamic d = Statement; // HACK: as we don't know which type
						string TrackKey = d.TrackKey;
						if (string.IsNullOrEmpty(TrackKey))
						{
							TrackKey = "0";
						}
						double RX = Statement.GetArgumentValueAsDouble(ArgumentName.RX);
						double RY = Statement.GetArgumentValueAsDouble(ArgumentName.RY);
						double RZ = Statement.GetArgumentValueAsDouble(ArgumentName.RZ);
						double Tilt = Statement.GetArgumentValueAsDouble(ArgumentName.Tilt);
						double Span = Statement.GetArgumentValueAsDouble(ArgumentName.Span);
						int RailIndex = RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKey));

						if (RailIndex != -1)
						{
							int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);

							if (Blocks[BlockIndex].FreeObj[RailIndex] == null)
							{
								Blocks[BlockIndex].FreeObj[RailIndex] = new List<FreeObj>();
							}

							Blocks[BlockIndex].FreeObj[RailIndex].Add(new FreeObj
							{
								TrackPosition = Statement.Distance,
								Key = Statement.Key,
								X = Statement.GetArgumentValueAsDouble(ArgumentName.X),
								Y = Statement.GetArgumentValueAsDouble(ArgumentName.Y),
								Z = Statement.GetArgumentValueAsDouble(ArgumentName.Z),
								Yaw = RY * 0.0174532925199433,
								Pitch = RX * 0.0174532925199433,
								Roll = RZtoRoll(RY, RZ) * 0.0174532925199433,
								Type = Convert.ToInt32(Tilt),
								Span = Convert.ToDouble(Span)
							});
						}
					}
						break;
					case MapFunctionName.PutBetween:
					{
						string[] TrackKeys = new string[2];
						if (!Statement.HasArgument(ArgumentName.TrackKey1) || string.IsNullOrEmpty(TrackKeys[0] = Statement.GetArgumentValueAsString(ArgumentName.TrackKey1)))
						{
							TrackKeys[0] = "0";
						}
						if (!Statement.HasArgument(ArgumentName.TrackKey2) || string.IsNullOrEmpty(TrackKeys[1] = Statement.GetArgumentValueAsString(ArgumentName.TrackKey2)))
						{
							TrackKeys[1] = "0";
						}

						int[] RailsIndex =
						{
							RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKeys[0])),
							RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKeys[1]))
						};

						if (RailsIndex[0] != -1 && RailsIndex[1] > 0)
						{
							int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);

								Blocks[BlockIndex].Cracks.Add(new Crack
							{
								TrackPosition = Statement.Distance,
								Key = Statement.Key,
								PrimaryRail = RailsIndex[0],
								SecondaryRail = RailsIndex[1]
							});
						}
					}
						break;
				}
			}
		}

		private static void ConfirmRepeater(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			for(int repeaterIndex = 0; repeaterIndex < RouteData.RepeaterList.Count; repeaterIndex++)
			{
				string key = RouteData.RepeaterList.ElementAt(repeaterIndex).Key;
				Repeater Repeater = RouteData.RepeaterList[key];
				double lastDistance = -1;
				bool possibleEnd = false;
				foreach (var Statement in ParseData.Statements)
				{
					if (Statement.ElementName != MapElementName.Repeater || !Statement.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}

					switch (Statement.FunctionName)
					{
						case MapFunctionName.Begin:
						case MapFunctionName.Begin0:
							{
								if (Repeater.StartRefreshed)
								{
									Repeater.EndingDistance = Statement.Distance;
									PutRepeater(RouteData, Repeater);
									Repeater.StartRefreshed = false;
								}

								dynamic d = Statement; // HACK: as we don't know which type
								string TrackKey = d.TrackKey;
								if (string.IsNullOrEmpty(TrackKey))
								{
									TrackKey = "0";
								}
								double RX = Statement.GetArgumentValueAsDouble(ArgumentName.RX);
								double RY = Statement.GetArgumentValueAsDouble(ArgumentName.RY);
								double RZ = Statement.GetArgumentValueAsDouble(ArgumentName.RZ);
								double Tilt = Statement.GetArgumentValueAsDouble(ArgumentName.Tilt);
								double Span = Statement.GetArgumentValueAsDouble(ArgumentName.Span);
								double Interval = Statement.GetArgumentValueAsDouble(ArgumentName.Interval);

								Repeater.StartingDistance = Statement.Distance;
								Repeater.TrackKey = Convert.ToString(TrackKey);
								Repeater.X = Statement.GetArgumentValueAsDouble(ArgumentName.X);
								Repeater.Y = Statement.GetArgumentValueAsDouble(ArgumentName.Y);
								Repeater.Z = Statement.GetArgumentValueAsDouble(ArgumentName.Z);
								Repeater.Yaw = RY * 0.0174532925199433;
								Repeater.Pitch = RX * 0.0174532925199433;
								Repeater.Roll = RZtoRoll(RY, RZ) * 0.0174532925199433;
								Repeater.Type = (int)Tilt;
								Repeater.Span = Span;
								Repeater.Interval = Interval;
								Repeater.StartRefreshed = true;

								
								Repeater.ObjectKeys = new string[d.StructureKeys.Count];
								d.StructureKeys.CopyTo(Repeater.ObjectKeys, 0);
								possibleEnd = false;
							}
							break;
						case MapFunctionName.End:
							possibleEnd = true;
							break;
					}

					/*
					 * HACK: Commands may no longer be in order after sort by TPos (in BVE5_Parsing), but we can
					 * work around that by triggering the end on the next track position instead
					 */

					if (possibleEnd && lastDistance != Statement.Distance && Repeater.StartRefreshed)
					{
						Repeater.EndingDistance = Statement.Distance;
						PutRepeater(RouteData, Repeater);
						Repeater.StartRefreshed = false;
						possibleEnd = false;
					}

					lastDistance = Statement.Distance;

				}

				// Hack:
				if (Repeater.StartRefreshed)
				{
					double EndTrackPosition = Plugin.CurrentRoute.Stations.Last().Stops[0].TrackPosition + Plugin.CurrentOptions.ViewingDistance;
					Repeater.EndingDistance = EndTrackPosition;
					PutRepeater(RouteData, Repeater);
					Repeater.StartRefreshed = false;
				}
			}
		}

		private static void PutRepeater(RouteData RouteData, Repeater Repeater)
		{
			int RailIndex = RouteData.TrackKeyList.IndexOf(Repeater.TrackKey);

			if (Repeater.Interval <= 0.0)
			{
				return;
			}

			if (RailIndex == -1)
			{
				// at least in converted legacy stuff (2.02 map format) unknown track indexes map to rail 0
				RailIndex = 0;
			}

			IList<Block> Blocks = RouteData.Blocks;
			int LoopCount = 0;

			for (double i = Repeater.StartingDistance; i < Repeater.EndingDistance; i += Repeater.Interval)
			{
				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(i);

				if (Blocks[BlockIndex].FreeObj[RailIndex] == null)
				{
					Blocks[BlockIndex].FreeObj[RailIndex] = new List<FreeObj>();
				}

				Blocks[BlockIndex].FreeObj[RailIndex].Add(new FreeObj
				{
					TrackPosition = i,
					Key = Repeater.ObjectKeys[LoopCount],
					X = Repeater.X,
					Y = Repeater.Y,
					Z = Repeater.Z,
					Yaw = Repeater.Yaw,
					Pitch = Repeater.Pitch,
					Roll = Repeater.Roll,
					Type = Repeater.Type,
					Span = Repeater.Span
				});

				if (LoopCount >= Repeater.ObjectKeys.Length - 1)
				{
					LoopCount = 0;
				}
				else
				{
					LoopCount++;
				}
			}
		}

		private static void ConfirmSection(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			// These are the speed limits for the default Japanese signal aspects, and in most cases will be overwritten
			RouteData.SignalSpeeds = new[] { 0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity };

			if (PreviewOnly)
			{
				return;
			}

			IList<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Section && !(Statement.ElementName == MapElementName.Signal && Statement.FunctionName == MapFunctionName.SpeedLimit))
				{
					continue;
				}

				dynamic d = Statement;

				switch (Statement.FunctionName)
				{
					case MapFunctionName.Begin:
					case MapFunctionName.BeginNew:
					{
						double?[] aspects = new double?[d.SignalAspects.Count];
						d.SignalAspects.CopyTo(aspects, 0); // Yuck: Stored as nullable doubles
						int Index = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
						Blocks[Index].Sections.Add(new Section
						{
							TrackPosition = Statement.Distance,
							Aspects = aspects.Select(db => db != null ? (int)db : 0).ToArray(),
							DepartureStationIndex = -1
						});
						int StationIndex = Array.FindLastIndex(Plugin.CurrentRoute.Stations, s => s.Stops.Last().TrackPosition <= Statement.Distance);
						if (StationIndex != -1)
						{
							Station Station = RouteData.StationList.Find(s => s.Name.Equals(Plugin.CurrentRoute.Stations[StationIndex].Name, StringComparison.InvariantCultureIgnoreCase));
							if (Station != null)
							{
								if (Station.ForceStopSignal && !Station.DepartureSignalUsed)
								{
									Blocks[Index].Sections.Last().DepartureStationIndex = StationIndex;
									Station.DepartureSignalUsed = true;
								}
							}
						}
					}
					break;
					case MapFunctionName.SetSpeedLimit:
					case MapFunctionName.SpeedLimit:
					{
						double?[] limits = new double?[d.SpeedLimits.Count];
						d.SpeedLimits.CopyTo(limits, 0); // Yuck: Stored as nullable doubles
						RouteData.SignalSpeeds = limits.Select(db => db ?? double.PositiveInfinity).ToArray();
					} 
					break;
				}
			}
		}

		private static void ConfirmSignal(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			IList<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Signal || Statement.FunctionName != MapFunctionName.Put)
				{
					continue;
				}

				dynamic d = Statement;

				string TrackKey = d.TrackKey;
				object Section = d.Section;
				if (string.IsNullOrEmpty(TrackKey))
				{
					TrackKey = "0";
				}

				object X = d.X;
				object Y = d.Y;
				object Z = d.Z;
				object RX = d.RX;
				object RY = d.RY;
				object RZ = d.RZ;
				object Tilt = d.Tilt;
				object Span = d.Span;

				int RailIndex = RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKey));

				if (RailIndex != -1)
				{
					int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);

					if (Blocks[BlockIndex].Signals[RailIndex] == null)
					{
						Blocks[BlockIndex].Signals[RailIndex] = new List<Signal>();
					}

					int CurrentSection = 0;
					for (int i = BlockIndex; i >= 0; i--)
					{
						CurrentSection += Blocks[i].Sections.Count(s => s.TrackPosition <= Statement.Distance);
					}

					Blocks[BlockIndex].Signals[RailIndex].Add(new Signal
					{
						TrackPosition = Statement.Distance,
						SignalObjectKey = Statement.Key,
						SectionIndex = CurrentSection + Convert.ToInt32(Section),
						X = Convert.ToDouble(X),
						Y = Convert.ToDouble(Y),
						Z = Convert.ToDouble(Z),
						Yaw = Convert.ToDouble(RY) * 0.0174532925199433,
						Pitch = Convert.ToDouble(RX) * 0.0174532925199433,
						Roll = RZtoRoll(Convert.ToDouble(RY), Convert.ToDouble(RZ)) * 0.0174532925199433,
						Type = Convert.ToInt32(Tilt),
						Span = Convert.ToDouble(Span)
					});
				}
			}
		}

		private static void ConfirmBeacon(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Beacon)
				{
					continue;
				}

				dynamic d = Statement;

				object Type = d.Type;
				object TempSection = d.Section;
				object SendData = d.Senddata;

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);

				int Section = Convert.ToInt32(TempSection);
				int CurrentSection = 0;
				for (int i = BlockIndex; i >= 0; i--)
				{
					CurrentSection += RouteData.Blocks[i].Sections.Count(s => s.TrackPosition <= Statement.Distance);
				}

				if (Section < -1)
				{
					Section = CurrentSection + 1;
				}
				else if (Section > -1)
				{
					Section += CurrentSection;
				}

				RouteData.Blocks[BlockIndex].Transponders.Add(new Transponder
				{
					TrackPosition = Statement.Distance,
					Type = Convert.ToInt32(Type),
					SectionIndex = Section,
					Data = Convert.ToInt32(SendData)
				});
			}
		}

		private static void ConfirmSpeedLimit(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			IList<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.SpeedLimit)
				{
					continue;
				}

				double Speed = Statement.GetArgumentValueAsDouble(ArgumentName.V);

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
				Blocks[BlockIndex].Limits.Add(new Limit
				{
					TrackPosition = Statement.Distance,
					Speed = Speed <= 0.0 ? double.PositiveInfinity : Convert.ToDouble(Speed) * RouteData.UnitOfSpeed
				});
			}
		}

		private static void ConfirmPreTrain(bool PreviewOnly, MapData ParseData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Pretrain)
				{
					continue;
				}

				dynamic d = Statement;
				double Time;
				TryParseBve5Time(Convert.ToString(d.Time), out Time);

				int n = Plugin.CurrentRoute.BogusPreTrainInstructions.Length;
				Array.Resize(ref Plugin.CurrentRoute.BogusPreTrainInstructions, n + 1);

				Plugin.CurrentRoute.BogusPreTrainInstructions[n].TrackPosition = Statement.Distance;
				Plugin.CurrentRoute.BogusPreTrainInstructions[n].Time = Time;
			}
		}

		private static void ConfirmLight(bool PreviewOnly, MapData ParseData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Light)
				{
					continue;
				}

				switch (Statement.FunctionName)
				{
					case MapFunctionName.Ambient:
						{
							double TempRed, TempGreen, TempBlue;
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

							Plugin.CurrentRoute.Atmosphere.AmbientLightColor = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
						}
						break;
					case MapFunctionName.Diffuse:
						{
							double TempRed, TempGreen, TempBlue;
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

							Plugin.CurrentRoute.Atmosphere.DiffuseLightColor = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
						}
						break;
					case MapFunctionName.Direction:
						{
							double Pitch, Yaw;
							if (!Statement.HasArgument(ArgumentName.Pitch) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Pitch), out Pitch))
							{
								Pitch = 60.0;
							}
							if (!Statement.HasArgument(ArgumentName.Yaw) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Yaw), out Yaw))
							{
								Yaw = -26.565051177078;
							}

							double Theta = Convert.ToDouble(Pitch) * 0.0174532925199433;
							double Phi =Convert.ToDouble(Yaw) * 0.0174532925199433;
							double dx = Math.Cos(Theta) * Math.Sin(Phi);
							double dy = -Math.Sin(Theta);
							double dz = Math.Cos(Theta) * Math.Cos(Phi);
							Plugin.CurrentRoute.Atmosphere.LightPosition = new Vector3((float)-dx, (float)-dy, (float)-dz);
						}
						break;
				}
			}
		}

		private static void ConfirmCabIlluminance(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.CabIlluminance)
				{
					continue;
				}

				double TempValue;
				if (!Statement.HasArgument(ArgumentName.Value) || !double.TryParse(Statement.GetArgumentValueAsString(ArgumentName.Value), out TempValue) || TempValue == 0.0)
				{
					TempValue = 1.0;
				}

				float Value = Convert.ToSingle(TempValue);
				if (Value < 0.0f || Value > 1.0f)
				{
					Value = Value < 0.0f ? 0.0f : 1.0f;
				}

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
				RouteData.Blocks[BlockIndex].BrightnessChanges.Add(new Brightness
				{
					TrackPosition = Statement.Distance,
					Value = Value
				});
			}
		}

		private static void ConfirmIrregularity(bool PreviewOnly, IList<Block> Blocks)
		{
			if (PreviewOnly)
			{
				return;
			}

			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!Blocks[i].AccuracyDefined)
				{
					continue;
				}

				int StartBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.AccuracyDefined);

				if (StartBlock != -1)
				{
					for (int j = StartBlock+1; j < i; j++)
					{
						Blocks[j].Accuracy = Blocks[StartBlock].Accuracy;
					}
				}
			}
		}

		private static void ConfirmAdhesion(bool PreviewOnly, IList<Block> Blocks)
		{
			if (PreviewOnly)
			{
				return;
			}

			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!Blocks[i].AdhesionMultiplierDefined)
				{
					continue;
				}

				int StartBlock = Blocks.FindLastIndex(i - 1, i, Block => Block.AdhesionMultiplierDefined);

				if (StartBlock != -1)
				{
					for (int j = StartBlock + 1; j < i; j++)
					{
						Blocks[j].AdhesionMultiplier = Blocks[StartBlock].AdhesionMultiplier;
					}
				}
			}
		}

		private static void ConfirmSound(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Sound)
				{
					continue;
				}

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
				RouteData.Blocks[BlockIndex].SoundEvents.Add(new Sound
				{
					TrackPosition = Statement.Distance,
					Key = Statement.Key,
					Type = SoundType.TrainStatic
				});
			}
		}

		private static void ConfirmSound3D(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Sound3d)
				{
					continue;
				}

				double X = Statement.GetArgumentValueAsDouble(ArgumentName.X);
				double Y = Statement.GetArgumentValueAsDouble(ArgumentName.Y);

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
				RouteData.Blocks[BlockIndex].SoundEvents.Add(new Sound
				{
					TrackPosition = Statement.Distance,
					Key = Statement.Key,
					Type = SoundType.World,
					X = X,
					Y = Y
				});
			}
		}

		private static void ConfirmRollingNoise(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.RollingNoise)
				{
					continue;
				}

				object Index = Statement.GetArgumentValue(ArgumentName.Index);

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
				RouteData.Blocks[BlockIndex].RunSounds.Add(new TrackSound
				{
					TrackPosition = Statement.Distance,
					SoundIndex = Convert.ToInt32(Index)
				});
			}
		}

		private static void ConfirmFlangeNoise(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.FlangeNoise)
				{
					continue;
				}

				object Index = Statement.GetArgumentValue(ArgumentName.Index);

				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
				RouteData.Blocks[BlockIndex].FlangeSounds.Add(new TrackSound
				{
					TrackPosition = Statement.Distance,
					SoundIndex = Convert.ToInt32(Index)
				});
			}
		}
	}
}
