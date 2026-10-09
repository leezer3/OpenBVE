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
	internal static partial class Bve5ScenarioParser
	{
		// Blocks with no value of their own take the last confirmed one
		private static void FillTentativeBlocks(IList<Block> Blocks, Func<Block, bool> IsStart, Func<Block, bool> IsEnd, Action<Block, Block> CopyValue)
		{
			int Last = 0;
			for (int i = 1; i < Blocks.Count; i++)
			{
				if (!IsStart(Blocks[i]) && !IsEnd(Blocks[i]))
				{
					continue;
				}

				for (int k = Last + 1; k < i; k++)
				{
					CopyValue(Blocks[k], Blocks[Last]);
				}

				Last = i;
			}
		}

		// Runs Apply over every span that has both a start and an end
		private static void ApplyToSpans(IList<Block> Blocks, Func<Block, bool> IsStart, Func<Block, bool> IsEnd, bool StopAtAnyEnd, Action<int, int> Apply)
		{
			for (int i = 0; i < Blocks.Count; i++)
			{
				if (!IsEnd(Blocks[i]))
				{
					continue;
				}

				int StartBlock = FindSpanStart(Blocks, IsStart, IsEnd, i, StopAtAnyEnd);
				if (StartBlock != i)
				{
					Apply(StartBlock, i);
				}
			}
		}

		// A transition eases out of the value before the span and covers its first block,
		// an interpolation starts from the values inside the span
		private static void SetCurveSpan(IList<Block> Blocks, int StartBlock, int EndBlock, bool StartFromPrevious)
		{
			int From = StartFromPrevious && StartBlock != 0 ? StartBlock - 1 : StartBlock;
			int First = StartFromPrevious ? StartBlock : StartBlock + 1;
			double StartDistance = Blocks[StartBlock].StartingDistance;
			double StartRadius = Blocks[From].CurrentTrackState.CurveRadius;
			double StartCant = Blocks[From].CurrentTrackState.CurveCant;
			double EndDistance = Blocks[EndBlock].StartingDistance;
			double EndRadius = Blocks[EndBlock].CurrentTrackState.CurveRadius;
			double EndCant = Blocks[EndBlock].CurrentTrackState.CurveCant;

			for (int k = First; k < EndBlock; k++)
			{
				CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, Blocks[k].StartingDistance, out Blocks[k].CurrentTrackState.CurveRadius, out Blocks[k].CurrentTrackState.CurveCant);
			}
		}

		private static void SetGradientSpan(IList<Block> Blocks, int StartBlock, int EndBlock, bool StartFromPrevious)
		{
			int From = StartFromPrevious && StartBlock != 0 ? StartBlock - 1 : StartBlock;
			int First = StartFromPrevious ? StartBlock : StartBlock + 1;
			double StartDistance = Blocks[StartBlock].StartingDistance;
			double StartPitch = Blocks[From].Pitch;
			double EndDistance = Blocks[EndBlock].StartingDistance;
			double EndPitch = Blocks[EndBlock].Pitch;

			for (int k = First; k < EndBlock; k++)
			{
				Blocks[k].Pitch = LinearInterpolation(StartDistance, StartPitch, EndDistance, EndPitch, Blocks[k].StartingDistance);
			}
		}

		private static void SetCantSpan(IList<Block> Blocks, string RailKey, int StartBlock, int EndBlock, bool StartFromPrevious)
		{
			int From = StartFromPrevious && StartBlock != 0 ? StartBlock - 1 : StartBlock;
			int First = StartFromPrevious ? StartBlock : StartBlock + 1;
			double StartDistance = Blocks[StartBlock].StartingDistance;
			double StartCant = Blocks[From].Rails[RailKey].CurveCant;
			double EndDistance = Blocks[EndBlock].StartingDistance;
			double EndCant = Blocks[EndBlock].Rails[RailKey].CurveCant;

			for (int k = First; k < EndBlock; k++)
			{
				CalcCurveTransition(StartDistance, 0.0, StartCant, EndDistance, 0.0, EndCant, Blocks[k].StartingDistance, out _, out Blocks[k].Rails[RailKey].CurveCant);
			}
		}

		private static void ConfirmCurve(IList<Block> Blocks)
		{
			FillTentativeBlocks(Blocks,
				b => b.Rails["0"].CurveInterpolateStart,
				b => b.Rails["0"].CurveInterpolateEnd,
				(block, last) =>
				{
					block.CurrentTrackState.CurveRadius = last.CurrentTrackState.CurveRadius;
					block.CurrentTrackState.CurveCant = last.CurrentTrackState.CurveCant;
				});

			// Curve transition
			ApplyToSpans(Blocks,
				b => b.Rails["0"].CurveTransitionStart,
				b => b.Rails["0"].CurveTransitionEnd,
				StopAtAnyEnd: true,
				(start, end) => SetCurveSpan(Blocks, start, end, StartFromPrevious: true));

			// Curve interpolate
			ApplyToSpans(Blocks,
				b => b.Rails["0"].CurveInterpolateStart,
				b => b.Rails["0"].CurveInterpolateEnd,
				StopAtAnyEnd: false,
				(start, end) => SetCurveSpan(Blocks, start, end, StartFromPrevious: false));
		}

		private static void ConfirmGradient(IList<Block> Blocks)
		{
			FillTentativeBlocks(Blocks,
				b => b.GradientInterpolateStart,
				b => b.GradientInterpolateEnd,
				(block, last) => block.Pitch = last.Pitch);

			// Gradient transition
			ApplyToSpans(Blocks,
				b => b.GradientTransitionStart,
				b => b.GradientTransitionEnd,
				StopAtAnyEnd: true,
				(start, end) => SetGradientSpan(Blocks, start, end, StartFromPrevious: true));

			// Gradient interpolate
			ApplyToSpans(Blocks,
				b => b.GradientInterpolateStart,
				b => b.GradientInterpolateEnd,
				StopAtAnyEnd: false,
				(start, end) => SetGradientSpan(Blocks, start, end, StartFromPrevious: false));
		}

		private static void ConfirmTrack(RouteData RouteData)
		{
			IList<Block> Blocks = RouteData.Blocks;

			// X and Y positions of every rail, the player track included
			for (int j = 0; j < RouteData.TrackKeyList.Count; j++)
			{
				string RailKey = RouteData.TrackKeyList[j];
				InterpolateRailPosition(Blocks, RailKey, Horizontal: true);
				InterpolateRailPosition(Blocks, RailKey, Horizontal: false);
			}

			// Cant of the secondary rails
			for (int j = 1; j < RouteData.TrackKeyList.Count; j++)
			{
				string RailKey = RouteData.TrackKeyList[j];

				FillTentativeBlocks(Blocks,
					b => b.Rails[RailKey].CurveInterpolateStart,
					b => b.Rails[RailKey].CurveInterpolateEnd,
					(block, last) => block.Rails[RailKey].CurveCant = last.Rails[RailKey].CurveCant);

				ApplyToSpans(Blocks,
					b => b.Rails[RailKey].CurveTransitionStart,
					b => b.Rails[RailKey].CurveTransitionEnd,
					StopAtAnyEnd: true,
					(start, end) => SetCantSpan(Blocks, RailKey, start, end, StartFromPrevious: true));

				ApplyToSpans(Blocks,
					b => b.Rails[RailKey].CurveInterpolateStart,
					b => b.Rails[RailKey].CurveInterpolateEnd,
					StopAtAnyEnd: false,
					(start, end) => SetCantSpan(Blocks, RailKey, start, end, StartFromPrevious: false));
			}
		}

		// Fills in the X (or Y) position of a rail between the stated positions
		private static void InterpolateRailPosition(IList<Block> Blocks, string RailKey, bool Horizontal)
		{
			int Last = 0;
			for (int i = 1; i < Blocks.Count; i++)
			{
				Rail Rail = Blocks[i].Rails[RailKey];
				if (Horizontal ? !Rail.InterpolateX : !Rail.InterpolateY)
				{
					continue;
				}

				Rail From = Blocks[Last].Rails[RailKey];
				double StartDistance = Blocks[Last].StartingDistance;
				double EndDistance = Blocks[i].StartingDistance;
				double StartValue = Horizontal ? From.Position.X : From.Position.Y;
				double EndValue = Horizontal ? Rail.Position.X : Rail.Position.Y;
				double Radius = Horizontal ? From.RadiusH : From.RadiusV;

				for (int k = Last + 1; k < i; k++)
				{
					double Value = GetTrackCoordinate(StartDistance, StartValue, EndDistance, EndValue, Radius, Blocks[k].StartingDistance);
					if (Horizontal)
					{
						Blocks[k].Rails[RailKey].Position.X = Value;
						Blocks[k].Rails[RailKey].RadiusH = Radius;
					}
					else
					{
						Blocks[k].Rails[RailKey].Position.Y = Value;
						Blocks[k].Rails[RailKey].RadiusV = Radius;
					}
				}

				Last = i;
			}
		}

		private static void ConfirmStructure(bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			IList<Block> Blocks = RouteData.Blocks;

			foreach (Statement Statement in ParseData.Statements)
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
						string TrackKey = Statement.GetArgumentValueAsString(ArgumentName.TrackKey);
						if (string.IsNullOrEmpty(TrackKey))
						{
							TrackKey = "0";
						}

						if (!RouteData.Objects.ContainsKey(Statement.Key))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "BVE5: Structure " + Statement.Key + " was not found on Track " + TrackKey + " at track position " + Statement.Distance + "m");
							continue;
						}

						if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Attempted to place Structure " + Statement.Key + " on the non-existent track " + TrackKey + " at track position " + Statement.Distance + "m");
							TrackKey = "0";
						}
						
						double RX = Statement.GetArgumentValueAsDouble(ArgumentName.RX);
						double RY = Statement.GetArgumentValueAsDouble(ArgumentName.RY);
						double RZ = Statement.GetArgumentValueAsDouble(ArgumentName.RZ);
						int Tilt = Statement.GetArgumentValueAsInt(ArgumentName.Tilt);
						double Span = Statement.GetArgumentValueAsDouble(ArgumentName.Span);

						if (Tilt > 3)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Invalid ObjectTransformType for Structure " + Statement.Key + " on track " + TrackKey + " at track position " + Statement.Distance + "m");
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

						if (!RouteData.Objects.ContainsKey(Statement.Key))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, "BVE5: Structure " + Statement.Key + " was not found for PutBetween Track " + TrackKeys[0] + " and Track " + TrackKeys[1] + " at track position " + Statement.Distance + "m");
							continue;
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

			foreach (Statement Statement in ParseData.Statements)
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

			foreach (Repeater Repeater in RepeaterList)
			{
				double lastDistance = -1;
				bool possibleEnd = false;
				foreach (Statement Statement in ParseData.Statements)
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
								string TrackKey = Statement.GetArgumentValueAsString(ArgumentName.TrackKey);
								if (string.IsNullOrEmpty(TrackKey))
								{
									TrackKey = "0";
								}

								if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Attempted to place Repeater " + Statement.Key + " on the non-existent track " + TrackKey + " at track position " + Statement.Distance + "m");
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
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Invalid ObjectTransformType for Repeater " + Statement.Key + " on track " + TrackKey + " at track position " + Statement.Distance + "m");
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
								HashSet<string> missingObjectKeys = new HashSet<string>();
								d.StructureKeys.CopyTo(Repeater.ObjectKeys, 0);
								for (int i = 0; i < Repeater.ObjectKeys.Length; i++)
								{
									// empty string == no object placed
									// also only add the error once per position (even if the object appears multiple times in the cycle)
									if (!RouteData.Objects.ContainsKey(Repeater.ObjectKeys[i]) && missingObjectKeys.Add(Repeater.ObjectKeys[i]) && !string.IsNullOrEmpty(Repeater.ObjectKeys[i]))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BVE5: Structure " + Repeater.ObjectKeys[i] + " was not found in Repeater " + Statement.Key + " on track " + TrackKey + " at track position " + Statement.Distance + "m");
									}
								}
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

					if (possibleEnd && Repeater.StartRefreshed)
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


			foreach (Statement Statement in ParseData.Statements)
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

			foreach (Statement Statement in ParseData.Statements)
			{
				if (Statement.ElementName != MapElementName.Signal || Statement.FunctionName != MapFunctionName.Put)
				{
					continue;
				}

				dynamic d = Statement;

				string TrackKey = Statement.GetArgumentValueAsString(ArgumentName.TrackKey);
				object Section = d.Section;
				if (string.IsNullOrEmpty(TrackKey))
				{
					TrackKey = "0";
				}

				if (!RouteData.TrackKeyList.Contains(TrackKey, StringComparer.OrdinalIgnoreCase))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Attempted to place Signal " + Statement.Key + " on the non-existent track " + TrackKey + " at track position " + Statement.Distance + "m");
					TrackKey = "0";
				}

				object RX = d.RX;
				object RY = d.RY;
				object RZ = d.RZ;
				ObjectTransformType Tilt = d.Tilt != null ? (ObjectTransformType)d.Tilt : ObjectTransformType.Horizontal;
				double Span = d.Span != null ? (double)d.Span : 0.0;

				if ((int)Tilt > 3)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "BVE5: Invalid ObjectTransformType for Signal " + Statement.Key + " on track " + TrackKey + " at track position " + Statement.Distance + "m");
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
						Roll = RZtoRoll(Convert.ToDouble(RY), Convert.ToDouble(RZ)).ToRadians()
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

			foreach (Statement Statement in ParseData.Statements)
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
					Plugin.CurrentRoute.Atmosphere.AmbientLightColor = ParseLightColor(Statement);
					break;
				case MapFunctionName.Diffuse:
					Plugin.CurrentRoute.Atmosphere.DiffuseLightColor = ParseLightColor(Statement);
					break;
				case MapFunctionName.Direction:
					{
						if (!Statement.HasArgument(ArgumentName.Pitch) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Pitch), out double Pitch))
						{
							Pitch = 60.0;
						}

						if (!Statement.HasArgument(ArgumentName.Yaw) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(ArgumentName.Yaw), out double Yaw))
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

		// Parses an RGB light color, defaulting each missing / invalid component to 1.0
		private static Color24 ParseLightColor(Statement Statement)
		{
			double Red = ParseLightColorComponent(Statement, ArgumentName.Red);
			double Green = ParseLightColorComponent(Statement, ArgumentName.Green);
			double Blue = ParseLightColorComponent(Statement, ArgumentName.Blue);
			return new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
		}

		private static double ParseLightColorComponent(Statement Statement, ArgumentName Arg)
		{
			if (!Statement.HasArgument(Arg) || !NumberFormats.TryParseDoubleVb6(Statement.GetArgumentValueAsString(Arg), out double value))
			{
				return 1.0;
			}
			return Math.Max(0.0, Math.Min(1.0, value));
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

			FillTentativeBlocks(RouteData.Blocks,
				b => false,
				b => b.AccuracyDefined,
				(Target, Source) => Target.Accuracy = Source.Accuracy);
		}

		private static void ConfirmAdhesion(bool PreviewOnly, RouteData RouteData)
		{
			if (PreviewOnly)
			{
				return;
			}

			FillTentativeBlocks(RouteData.Blocks,
				b => false,
				b => b.AdhesionMultiplierDefined,
				(Target, Source) => Target.AdhesionMultiplier = Source.AdhesionMultiplier);
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

			foreach (Statement Statement in ParseData.Statements)
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
