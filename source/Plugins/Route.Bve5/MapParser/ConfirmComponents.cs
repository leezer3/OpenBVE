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
using OpenBveApi.Sounds;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void ConfirmCurve(IList<Block> Blocks)
		{
			// Set a tentative value to a block that has not been decided.

			int LastConfirmBlock = 0;
			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!Blocks[i].Rails["0"].CurveInterpolateStart && !Blocks[i].Rails["0"].CurveInterpolateEnd)
				{
					continue;
				}

				for (int k = LastConfirmBlock + 1; k < i; k++)
				{
					Blocks[k].CurrentTrackState.CurveRadius = Blocks[LastConfirmBlock].CurrentTrackState.CurveRadius;
					Blocks[k].CurrentTrackState.CurveCant = Blocks[LastConfirmBlock].CurrentTrackState.CurveCant;
				}

				LastConfirmBlock = i;
			}

			// Curve transition
			for (int i = 0; i < Blocks.Count; i++)
			{
				if (!Blocks[i].Rails["0"].CurveTransitionEnd)
				{
					continue;
				}

				int StartBlock = i;

				for (int k = i - 1; k >= 1; k--)
				{
					if (Blocks[k].Rails["0"].CurveTransitionEnd)
					{
						break;
					}

					if (Blocks[k].Rails["0"].CurveTransitionStart)
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
				if (!Blocks[i].Rails["0"].CurveInterpolateEnd)
				{
					continue;
				}

				int StartBlock = i;

				for (int k = i - 1; k >= 0; k--)
				{
					if (Blocks[k].Rails["0"].CurveInterpolateEnd && !Blocks[k].Rails["0"].CurveInterpolateStart)
					{
						break;
					}

					if (Blocks[k].Rails["0"].CurveInterpolateStart)
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
			int LastConfirmBlock = 0;
			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!Blocks[i].GradientInterpolateStart && !Blocks[i].GradientInterpolateEnd)
				{
					continue;
				}
				
				if (LastConfirmBlock != -1)
				{
					for (int k = LastConfirmBlock + 1; k < i; k++)
					{
						Blocks[k].Pitch = Blocks[LastConfirmBlock].Pitch;
					}
				}

				LastConfirmBlock = i;
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
				string railKey = RouteData.TrackKeyList[j];

				// last block in which interpolation has been done, hence starting point for next
				int lastInterpolateX = 0, lastInterpolateY = 0;

				for (int i = 1; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[railKey].InterpolateX)
					{
						continue;
					}

					if (Blocks[i].Rails[railKey].InterpolateX)
					{
						double StartDistance = Blocks[lastInterpolateX].StartingDistance;
						double StartX = Blocks[lastInterpolateX].Rails[railKey].Position.X;
						double EndDistance = Blocks[i].StartingDistance;
						double EndX = Blocks[i].Rails[railKey].Position.X;
						double RadiusH = Blocks[lastInterpolateX].Rails[railKey].RadiusH;

						for (int k = lastInterpolateX + 1; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							double CurrentX = GetTrackCoordinate(StartDistance, StartX, EndDistance, EndX, RadiusH, CurrentDistance);

							Blocks[k].Rails[railKey].Position.X = CurrentX;
							Blocks[k].Rails[railKey].RadiusH = RadiusH;
						}

						lastInterpolateX = i;
					}

					if (Blocks[i].Rails[railKey].InterpolateY)
					{
						double StartDistance = Blocks[lastInterpolateY].StartingDistance;
						double StartY = Blocks[lastInterpolateY].Rails[railKey].Position.Y;
						double EndDistance = Blocks[i].StartingDistance;
						double EndY = Blocks[i].Rails[railKey].Position.Y;
						double RadiusV = Blocks[lastInterpolateY].Rails[railKey].RadiusV;

						for (int k = lastInterpolateY + 1; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							double CurrentY = GetTrackCoordinate(StartDistance, StartY, EndDistance, EndY, RadiusV, CurrentDistance);

							Blocks[k].Rails[railKey].Position.Y = CurrentY;
							Blocks[k].Rails[railKey].RadiusV = RadiusV;
						}

						lastInterpolateY = i;
					}
				}
			}

			// Set a tentative value to a block that has not been decided.
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				int LastConfirmBlock = 0;
				for (int i = 1; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[railKey].CurveInterpolateStart && !Blocks[i].Rails[railKey].CurveInterpolateEnd)
					{
						continue;
					}
					
					if (LastConfirmBlock != -1)
					{
						for (int k = LastConfirmBlock + 1; k < i; k++)
						{
							Blocks[k].Rails[railKey].CurveCant = Blocks[LastConfirmBlock].Rails[railKey].CurveCant;
						}
					}

					LastConfirmBlock = i;
				}
			}

			// Curve transition
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				for (int i = 0; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[railKey].CurveTransitionEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 1; k--)
					{
						if (Blocks[k].Rails[railKey].CurveTransitionEnd)
						{
							break;
						}

						if (Blocks[k].Rails[railKey].CurveTransitionStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Blocks[StartBlock].StartingDistance;
						double StartRadius = 0.0;
						double StartCant = Blocks[StartBlock - 1].Rails[railKey].CurveCant;
						double EndDistance = Blocks[i].StartingDistance;
						double EndRadius = 0.0;
						double EndCant = Blocks[i].Rails[railKey].CurveCant;

						for (int k = StartBlock; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out _, out Blocks[k].Rails[railKey].CurveCant);
						}
					}
				}
			}

			// Curve interpolate
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string railKey = RouteData.TrackKeyList[j];
				for (int i = 0; i < Blocks.Count; i++)
				{
					if (!Blocks[i].Rails[railKey].CurveInterpolateEnd)
					{
						continue;
					}

					int StartBlock = i;

					for (int k = i - 1; k >= 0; k--)
					{
						if (Blocks[k].Rails[railKey].CurveInterpolateEnd && !Blocks[k].Rails[railKey].CurveInterpolateStart)
						{
							break;
						}

						if (Blocks[k].Rails[railKey].CurveInterpolateStart)
						{
							StartBlock = k;
							break;
						}
					}

					if (StartBlock != i)
					{
						double StartDistance = Blocks[StartBlock].StartingDistance;
						double StartRadius = 0.0;
						double StartCant = Blocks[StartBlock].Rails[railKey].CurveCant;
						double EndDistance = Blocks[i].StartingDistance;
						double EndRadius = 0.0;
						double EndCant = Blocks[i].Rails[railKey].CurveCant;

						for (int k = StartBlock + 1; k < i; k++)
						{
							double CurrentDistance = Blocks[k].StartingDistance;
							CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out _, out Blocks[k].Rails[railKey].CurveCant);
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

						if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Attempted to place Structure " + Statement.Key + " on the non-existent track " + d.TrackKey + " at track position " + Statement.Distance + "m");
							TrackKey = "0";
						}
						
						double RX = Statement.GetArgumentValueAsDouble(ArgumentName.RX);
						double RY = Statement.GetArgumentValueAsDouble(ArgumentName.RY);
						double RZ = Statement.GetArgumentValueAsDouble(ArgumentName.RZ);
						int Tilt = Statement.GetArgumentValueAsInt(ArgumentName.Tilt);
						double Span = Statement.GetArgumentValueAsDouble(ArgumentName.Span);

						if (Tilt > 3)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid ObjectTransformType for Structure " + Statement.Key + " on track " + d.TrackKey + " at track position " + Statement.Distance + "m");
							Tilt = 0;
						}

						int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);

						if (!Blocks[BlockIndex].FreeObjects.ContainsKey(TrackKey))
						{
							Blocks[BlockIndex].FreeObjects.Add(TrackKey, new List<FreeObj>());
						}

						Vector3 position = new Vector3(Statement.GetArgumentValueAsDouble(ArgumentName.X), Statement.GetArgumentValueAsDouble(ArgumentName.Y), Statement.GetArgumentValueAsDouble(ArgumentName.Z));
						Blocks[BlockIndex].FreeObjects[TrackKey].Add(new FreeObj(Statement.Distance, Statement.Key, position, RY.ToRadians(), -RX.ToRadians(), RZtoRoll(RY, RZ).ToRadians(), (ObjectTransformType)Tilt, Span));
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

						
						if (RouteData.TrackKeyList.Contains(TrackKeys[0], StringComparer.OrdinalIgnoreCase) && RouteData.TrackKeyList.Contains(TrackKeys[1]))
						{
							int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
							Blocks[BlockIndex].Cracks.Add(new Crack(Statement.Key, Statement.Distance, TrackKeys[0], TrackKeys[1]));
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

			List<Repeater> RepeaterList = new List<Repeater>();

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Repeater)
				{
					continue;
				}

				if (!RepeaterList.Exists(Repeater => Repeater.Key.Equals(Statement.Key, StringComparison.InvariantCultureIgnoreCase)))
				{
					RepeaterList.Add(new Repeater(Statement.Key));
				}
			}

			foreach (var Repeater in RepeaterList)
			{
				double lastDistance = -1;
				bool possibleEnd = false;
				foreach (var Statement in ParseData.Statements)
				{
					if (Statement.ElementName != MapElementName.Repeater || !Statement.Key.Equals(Repeater.Key, StringComparison.InvariantCultureIgnoreCase))
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

								if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Attempted to place Repeater " + Statement.Key + " on the non-existent track " + d.TrackKey + " at track position " + Statement.Distance + "m");
									TrackKey = "0";
								}
								double RX = Statement.GetArgumentValueAsDouble(ArgumentName.RX);
								double RY = Statement.GetArgumentValueAsDouble(ArgumentName.RY);
								double RZ = Statement.GetArgumentValueAsDouble(ArgumentName.RZ);
								int Tilt = Statement.GetArgumentValueAsInt(ArgumentName.Tilt);
								double Span = Statement.GetArgumentValueAsDouble(ArgumentName.Span);
								double Interval = Statement.GetArgumentValueAsDouble(ArgumentName.Interval);

								if (Tilt > 3)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid ObjectTransformType for Repeater " + Statement.Key + " on track " + d.TrackKey + " at track position " + Statement.Distance + "m");
									Tilt = 0;
								}

								Repeater.StartingDistance = Statement.Distance;
								Repeater.TrackKey = Convert.ToString(TrackKey);
								Repeater.Position = new Vector3(Statement.GetArgumentValueAsDouble(ArgumentName.X), Statement.GetArgumentValueAsDouble(ArgumentName.Y), Statement.GetArgumentValueAsDouble(ArgumentName.Z));
								Repeater.Yaw = RY.ToRadians();
								Repeater.Pitch = -RX.ToRadians();
								Repeater.Roll = RZtoRoll(RY, RZ).ToRadians();
								Repeater.Type = (ObjectTransformType)Tilt;
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
					double EndTrackPosition = Plugin.CurrentRoute.Stations.Last().Stops.First().TrackPosition + Plugin.CurrentOptions.ViewingDistance;
					Repeater.EndingDistance = EndTrackPosition;
					PutRepeater(RouteData, Repeater);
					Repeater.StartRefreshed = false;
				}
			}
		}

		private static void PutRepeater(RouteData RouteData, Repeater Repeater)
		{
			if (Repeater.Interval <= 0.0)
			{
				return;
			}

			string TrackKey = Repeater.TrackKey;

			int LoopCount = 0;

			for (double i = Repeater.StartingDistance; i < Repeater.EndingDistance; i += Repeater.Interval)
			{
				int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(i);

				if (!RouteData.Blocks[BlockIndex].FreeObjects.ContainsKey(TrackKey))
				{
					RouteData.Blocks[BlockIndex].FreeObjects.Add(TrackKey, new List<FreeObj>());
				}

				/*
				 * The relationship between span and interval is an absolute pain in the neck
				 *
				 * This seems to get stuff on Chuo Rapid Line looking OK in terms of the gradients
				 */
				RouteData.Blocks[BlockIndex].FreeObjects[TrackKey].Add(new FreeObj(i, Repeater.ObjectKeys[LoopCount], Repeater.Position, Repeater.Yaw, Repeater.Pitch, Repeater.Roll, Repeater.Type, Math.Max(Repeater.Interval, Repeater.Span)));

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
						RouteData.Blocks[Index].Sections.Add(new Section(Statement.Distance, aspects.Select(db => db != null ? (int)db : 0).ToArray()));
						int StationIndex = Array.FindLastIndex(Plugin.CurrentRoute.Stations, s => s.Stops.Last().TrackPosition <= Statement.Distance);
						if (StationIndex != -1)
						{
							Station Station = RouteData.StationList[Plugin.CurrentRoute.Stations[StationIndex].Key];
							if (Station != null)
							{
								if (Station.ForceStopSignal && !Station.DepartureSignalUsed)
								{
									RouteData.Blocks[Index].Sections.Last().DepartureStationIndex = StationIndex;
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

				if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Attempted to place Signal " + Statement.Key + " on the non-existent track " + TrackKey + " at track position " + Statement.Distance + "m");
					TrackKey = "0";
				}

				object RX = d.RX;
				object RY = d.RY;
				object RZ = d.RZ;
				ObjectTransformType Tilt = d.Tilt != null ? (ObjectTransformType)d.Tilt : ObjectTransformType.Horizontal;
				double Span = d.Span != null ? (double)d.Span : 0.0;

				if ((int)Tilt > 3)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid ObjectTransformType for Signal " + Statement.Key + " on track " + d.TrackKey + " at track position " + Statement.Distance + "m");
					Tilt = 0;
				}

				int RailIndex = RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKey), StringComparison.OrdinalIgnoreCase);

				if (RailIndex != -1)
				{
					int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);

					if (RouteData.Blocks[BlockIndex].Signals[RailIndex] == null)
					{
						RouteData.Blocks[BlockIndex].Signals[RailIndex] = new List<Signal>();
					}

					int CurrentSection = 0;
					for (int i = BlockIndex; i >= 0; i--)
					{
						CurrentSection += RouteData.Blocks[i].Sections.Count(s => s.TrackPosition <= Statement.Distance);
					}

					Vector3 Position = new Vector3(Statement.GetArgumentValueAsDouble(ArgumentName.X), Statement.GetArgumentValueAsDouble(ArgumentName.Y), Statement.GetArgumentValueAsDouble(ArgumentName.Z));
					RouteData.Blocks[BlockIndex].Signals[RailIndex].Add(new Signal(Statement.Key, Statement.Distance, Tilt, Span, Position)
					{
						SectionIndex = CurrentSection + Convert.ToInt32(Section),
						Yaw = Convert.ToDouble(RY).ToRadians(),
						Pitch = -Convert.ToDouble(RX).ToRadians(),
						Roll = RZtoRoll(Convert.ToDouble(RY), Convert.ToDouble(RZ)).ToRadians(),
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

				int TempSection = Convert.ToInt32(d.Section);
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

				RouteData.Blocks[BlockIndex].Transponders.Add(new Transponder(Statement.Distance, Convert.ToInt32(d.Type), Convert.ToInt32(d.Senddata), Section));
			}
		}

		private static void ConfirmSpeedLimit(Statement Statement, RouteData RouteData)
		{
			double Speed = Statement.GetArgumentValueAsDouble(ArgumentName.V);

			int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
			RouteData.Blocks[BlockIndex].Limits.Add(new Limit(Statement.Distance, Speed <= 0.0 ? double.PositiveInfinity : Convert.ToDouble(Speed) * RouteData.UnitOfSpeed));
		}

		private static void ConfirmPreTrain(Statement Statement)
		{
			dynamic d = Statement;
			TryParseBve5Time(Convert.ToString(d.Time), out double Time);
			int n = Plugin.CurrentRoute.BogusPreTrainInstructions.Length;
			Array.Resize(ref Plugin.CurrentRoute.BogusPreTrainInstructions, n + 1);
			Plugin.CurrentRoute.BogusPreTrainInstructions[n].TrackPosition = Statement.Distance;
			Plugin.CurrentRoute.BogusPreTrainInstructions[n].Time = Time;
		}

		private static void ConfirmLight(Statement Statement)
		{
			switch (Statement.FunctionName)
			{
				case MapFunctionName.Ambient:
				{
					double TempRed, TempGreen, TempBlue;
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

					Plugin.CurrentRoute.Atmosphere.AmbientLightColor = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
				}
					break;
				case MapFunctionName.Diffuse:
				{
					double TempRed, TempGreen, TempBlue;
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

					Plugin.CurrentRoute.Atmosphere.DiffuseLightColor = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
				}
					break;
				case MapFunctionName.Direction:
				{
					double Pitch, Yaw;
					if (!Statement.HasArgument(ArgumentName.Pitch) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Pitch), out Pitch))
					{
						Pitch = 60.0;
					}

					if (!Statement.HasArgument(ArgumentName.Yaw) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Yaw), out Yaw))
					{
						Yaw = -26.565051177078;
					}

					double Theta = Pitch.ToRadians();
					double Phi = Yaw.ToRadians();
					double dx = Math.Cos(Theta) * Math.Sin(Phi);
					double dy = -Math.Sin(Theta);
					double dz = Math.Cos(Theta) * Math.Cos(Phi);
					Plugin.CurrentRoute.Atmosphere.LightPosition = new Vector3((float)-dx, (float)-dy, (float)-dz);
				}
					break;
			}
		}

		private static void ConfirmCabIlluminance(Statement Statement, RouteData RouteData)
		{
			if (!Statement.HasArgument(ArgumentName.Value) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Value), out double illuminanceValue) || illuminanceValue == 0.0)
			{
				illuminanceValue = 1.0;
			}

			if (illuminanceValue < 0.0f || illuminanceValue > 1.0f)
			{
				illuminanceValue = illuminanceValue < 0.0f ? 0.0f : 1.0f;
			}

			int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
			RouteData.Blocks[BlockIndex].BrightnessChanges.Add(new Brightness(Statement.Distance, (float)illuminanceValue));
		}

		private static void ConfirmIrregularity(bool PreviewOnly, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			int StartBlock = -1;
			for (int i = 1; i < RouteData.Blocks.Count; i++)
			{
				if (!RouteData.Blocks[i].AccuracyDefined)
				{
					continue;
				}

				if (StartBlock != -1)
				{
					for (int j = StartBlock+1; j < i; j++)
					{
						RouteData.Blocks[j].Accuracy = RouteData.Blocks[StartBlock].Accuracy;
					}
				}

				StartBlock = i;
			}
		}

		private static void ConfirmAdhesion(bool PreviewOnly, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			int StartBlock = -1;

			for (int i = 1; i < RouteData.Blocks.Count; i++)
			{
				if (!RouteData.Blocks[i].AdhesionMultiplierDefined)
				{
					continue;
				}

				if (StartBlock != -1)
				{
					for (int j = StartBlock + 1; j < i; j++)
					{
						RouteData.Blocks[j].AdhesionMultiplier = RouteData.Blocks[StartBlock].AdhesionMultiplier;
					}
				}

				StartBlock = i;
			}
		}

		private static void ConfirmSound(Statement Statement, RouteData RouteData)
		{
			int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
			RouteData.Blocks[BlockIndex].SoundEvents.Add(new Sound(Statement.Distance, Statement.Key, SoundType.Ambient, Vector2.Null));
		}

		private static void ConfirmSound3D(Statement Statement, RouteData RouteData)
		{
			double X = Statement.GetArgumentValueAsDouble(ArgumentName.X);
			double Y = Statement.GetArgumentValueAsDouble(ArgumentName.Y);

			int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
			RouteData.Blocks[BlockIndex].SoundEvents.Add(new Sound(Statement.Distance, Statement.Key, SoundType.Ambient, new Vector2(X, Y)));
		}

		private static void ConfirmRollingNoise(Statement Statement, RouteData RouteData)
		{
			object Index = Statement.GetArgumentValue(ArgumentName.Index);

			int BlockIndex = RouteData.sortedBlocks.FindBlockIndex(Statement.Distance);
			RouteData.Blocks[BlockIndex].RunSounds.Add(new RunSound(Statement.Distance, Convert.ToInt32(Index)));
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
				RouteData.Blocks[BlockIndex].FlangeSounds.Add(new FlangeSound(Statement.Distance, Convert.ToInt32(Index)));
			}
		}
	}
}
