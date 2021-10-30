using System;
using System.Collections.Generic;
using System.Linq;
using Bve5Parser.MapGrammar;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void ConfirmCurve(List<Block> Blocks)
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
						double CurrentRadius;
						double CurrentCant;

						CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out CurrentRadius, out CurrentCant);

						Blocks[k].CurrentTrackState.CurveRadius = CurrentRadius;
						Blocks[k].CurrentTrackState.CurveCant = CurrentCant;
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
						double CurrentRadius;
						double CurrentCant;

						CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out CurrentRadius, out CurrentCant);

						Blocks[k].CurrentTrackState.CurveRadius = CurrentRadius;
						Blocks[k].CurrentTrackState.CurveCant = CurrentCant;
					}
				}
			}
		}

		private static void ConfirmGradient(List<Block> Blocks)
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
			List<Block> Blocks = RouteData.Blocks;

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
							double CurrentRadius;
							double CurrentCant;

							CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out CurrentRadius, out CurrentCant);

							Blocks[k].Rails[j].CurveCant = CurrentCant;
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
							double CurrentRadius;
							double CurrentCant;

							CalcCurveTransition(StartDistance, StartRadius, StartCant, EndDistance, EndRadius, EndCant, CurrentDistance, out CurrentRadius, out CurrentCant);

							Blocks[k].Rails[j].CurveCant = CurrentCant;
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

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "structure")
				{
					continue;
				}

				switch (Statement.Function)
				{
					case "put":
					case "put0":
						{
							object TrackKey, X, Y, Z, RX, RY, RZ, Tilt, Span;
							if (!Statement.Arguments.TryGetValue("trackkey", out TrackKey) || Convert.ToString(TrackKey) == string.Empty)
							{
								TrackKey = "0";
							}
							Statement.Arguments.TryGetValue("x", out X);
							Statement.Arguments.TryGetValue("y", out Y);
							Statement.Arguments.TryGetValue("z", out Z);
							Statement.Arguments.TryGetValue("rx", out RX);
							Statement.Arguments.TryGetValue("ry", out RY);
							Statement.Arguments.TryGetValue("rz", out RZ);
							Statement.Arguments.TryGetValue("tilt", out Tilt);
							Statement.Arguments.TryGetValue("span", out Span);

							int RailIndex = RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKey));

							if (RailIndex != -1)
							{
								int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);

								if (Blocks[BlockIndex].FreeObj[RailIndex] == null)
								{
									Blocks[BlockIndex].FreeObj[RailIndex] = new List<FreeObj>();
								}

								Blocks[BlockIndex].FreeObj[RailIndex].Add(new FreeObj
								{
									TrackPosition = Statement.Distance,
									Key = Statement.Key,
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
						break;
					case "putbetween":
						{
							object[] TrackKeys = new object[2];
							if (!Statement.Arguments.TryGetValue("trackkey1", out TrackKeys[0]) || Convert.ToString(TrackKeys[0]) == string.Empty)
							{
								TrackKeys[0] = "0";
							}
							if (!Statement.Arguments.TryGetValue("trackkey2", out TrackKeys[1]) || Convert.ToString(TrackKeys[1]) == string.Empty)
							{
								TrackKeys[1] = "0";
							}

							int[] RailsIndex = new int[]
							{
								RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKeys[0])),
								RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKeys[1]))
							};

							if (RailsIndex[0] != -1 && RailsIndex[1] > 0)
							{
								int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);

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

			List<Repeater> RepeaterList = new List<Repeater>();

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "repeater")
				{
					continue;
				}

				if (!RepeaterList.Exists(Repeater => Repeater.Key.Equals(Statement.Key, StringComparison.InvariantCultureIgnoreCase)))
				{
					RepeaterList.Add(new Repeater
					{
						Key = Statement.Key
					});
				}
			}

			foreach (var Repeater in RepeaterList)
			{
				foreach (var Statement in ParseData.Statements)
				{
					if (Statement.MapElement[0] != "repeater" || !Statement.Key.Equals(Repeater.Key, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}

					switch (Statement.Function)
					{
						case "begin":
						case "begin0":
							{
								if (Repeater.StartRefreshed)
								{
									Repeater.EndingDistance = Statement.Distance;
									PutRepeater(RouteData, Repeater);
									Repeater.StartRefreshed = false;
								}

								object TrackKey, X, Y, Z, RX, RY, RZ, Tilt, Span, Interval;
								List<string> ObjectKeys = new List<string>();
								if (!Statement.Arguments.TryGetValue("trackkey", out TrackKey) || Convert.ToString(TrackKey) == string.Empty)
								{
									TrackKey = "0";
								}
								Statement.Arguments.TryGetValue("x", out X);
								Statement.Arguments.TryGetValue("y", out Y);
								Statement.Arguments.TryGetValue("z", out Z);
								Statement.Arguments.TryGetValue("rx", out RX);
								Statement.Arguments.TryGetValue("ry", out RY);
								Statement.Arguments.TryGetValue("rz", out RZ);
								Statement.Arguments.TryGetValue("tilt", out Tilt);
								Statement.Arguments.TryGetValue("span", out Span);
								Statement.Arguments.TryGetValue("interval", out Interval);

								Repeater.StartingDistance = Statement.Distance;
								Repeater.TrackKey = Convert.ToString(TrackKey);
								Repeater.X = Convert.ToDouble(X);
								Repeater.Y = Convert.ToDouble(Y);
								Repeater.Z = Convert.ToDouble(Z);
								Repeater.Yaw = Convert.ToDouble(RY) * 0.0174532925199433;
								Repeater.Pitch = Convert.ToDouble(RX) * 0.0174532925199433;
								Repeater.Roll = RZtoRoll(Convert.ToDouble(RY), Convert.ToDouble(RZ)) * 0.0174532925199433;
								Repeater.Type = Convert.ToInt32(Tilt);
								Repeater.Span = Convert.ToDouble(Span);
								Repeater.Interval = Convert.ToDouble(Interval);
								Repeater.StartRefreshed = true;

								{
									bool IsReadable = true;
									int j = 1;

									while (IsReadable)
									{
										object ObjectKey;
										IsReadable = Statement.Arguments.TryGetValue("key" + j, out ObjectKey);

										if (IsReadable)
										{
											ObjectKeys.Add(Convert.ToString(ObjectKey));
											j++;
										}
									}
								}

								Repeater.ObjectKeys = ObjectKeys.ToArray();
							}
							break;
						case "end":
							if (Repeater.StartRefreshed)
							{
								Repeater.EndingDistance = Statement.Distance;
								PutRepeater(RouteData, Repeater);
								Repeater.StartRefreshed = false;
							}
							break;
					}
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
			int RailIndex = RouteData.TrackKeyList.IndexOf(Repeater.TrackKey);

			if (Repeater.Interval <= 0.0 || RailIndex == -1)
			{
				return;
			}

			List<Block> Blocks = RouteData.Blocks;
			int LoopCount = 0;

			for (double i = Repeater.StartingDistance; i < Repeater.EndingDistance; i += Repeater.Interval)
			{
				int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= i);

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
			RouteData.SignalSpeeds = new double[] { 0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity };

			if (PreviewOnly)
			{
				return;
			}

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "section" && !(Statement.MapElement[0] == "signal" && Statement.Function == "speedlimit"))
				{
					continue;
				}

				switch (Statement.Function)
				{
					case "begin":
					case "beginnew":
						{
							List<int> Signals = new List<int>();

							{
								bool IsReadable = true;
								int j = 0;

								while (IsReadable)
								{
									object Signal;
									IsReadable = Statement.Arguments.TryGetValue("signal" + j, out Signal);

									if (IsReadable)
									{
										Signals.Add(Convert.ToInt32(Signal));
										j++;
									}
								}
							}

							int Index = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
							Blocks[Index].Sections.Add(new Section
							{
								TrackPosition = Statement.Distance,
								Aspects = Signals.ToArray(),
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
					case "setspeedlimit":
					case "speedlimit":
						{
							List<double> SpeedLimits = new List<double>();

							{
								bool IsReadable = true;
								int j = 0;

								while (IsReadable)
								{
									object SpeedLimit;
									IsReadable = Statement.Arguments.TryGetValue("v" + j, out SpeedLimit);

									if (IsReadable)
									{
										if (SpeedLimit == null || Convert.ToString(SpeedLimit).Equals("null", StringComparison.InvariantCultureIgnoreCase))
										{
											SpeedLimit = double.PositiveInfinity;
										}
										SpeedLimits.Add(Convert.ToDouble(SpeedLimit) * RouteData.UnitOfSpeed);
										j++;
									}
								}
							}

							RouteData.SignalSpeeds = SpeedLimits.ToArray();
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

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "signal" || Statement.Function != "put")
				{
					continue;
				}

				object Section, TrackKey, X, Y, Z, RX, RY, RZ, Tilt, Span;
				Statement.Arguments.TryGetValue("section", out Section);
				if (!Statement.Arguments.TryGetValue("trackkey", out TrackKey) || Convert.ToString(TrackKey) == string.Empty)
				{
					TrackKey = "0";
				}
				Statement.Arguments.TryGetValue("x", out X);
				Statement.Arguments.TryGetValue("y", out Y);
				Statement.Arguments.TryGetValue("z", out Z);
				Statement.Arguments.TryGetValue("rx", out RX);
				Statement.Arguments.TryGetValue("ry", out RY);
				Statement.Arguments.TryGetValue("rz", out RZ);
				Statement.Arguments.TryGetValue("tilt", out Tilt);
				Statement.Arguments.TryGetValue("span", out Span);

				int RailIndex = RouteData.TrackKeyList.IndexOf(Convert.ToString(TrackKey));

				if (RailIndex != -1)
				{
					int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);

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

		private static void ConfirmBeacon(bool PreviewOnly, MapData ParseData, List<Block> Blocks)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "beacon")
				{
					continue;
				}

				object Type, TempSection, SendData;
				Statement.Arguments.TryGetValue("type", out Type);
				Statement.Arguments.TryGetValue("section", out TempSection);
				Statement.Arguments.TryGetValue("senddata", out SendData);

				int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);

				int Section = Convert.ToInt32(TempSection);
				int CurrentSection = 0;
				for (int i = BlockIndex; i >= 0; i--)
				{
					CurrentSection += Blocks[i].Sections.Count(s => s.TrackPosition <= Statement.Distance);
				}

				if (Section < -1)
				{
					Section = CurrentSection + 1;
				}
				else if (Section > -1)
				{
					Section += CurrentSection;
				}

				Blocks[BlockIndex].Transponders.Add(new Transponder
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

			List<Block> Blocks = RouteData.Blocks;

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "speedlimit")
				{
					continue;
				}

				object Speed;
				Statement.Arguments.TryGetValue("v", out Speed);

				int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
				Blocks[BlockIndex].Limits.Add(new Limit
				{
					TrackPosition = Statement.Distance,
					Speed = Convert.ToDouble(Speed) <= 0.0 ? double.PositiveInfinity : Convert.ToDouble(Speed) * RouteData.UnitOfSpeed
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
				if (Statement.MapElement[0] != "pretrain")
				{
					continue;
				}

				object TempTime;
				double Time;
				Statement.Arguments.TryGetValue("time", out TempTime);

				TryParseBve5Time(Convert.ToString(TempTime), out Time);

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
				if (Statement.MapElement[0] != "light")
				{
					continue;
				}

				switch (Statement.Function)
				{
					case "ambient":
						{
							object TempRed, TempGreen, TempBlue;
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

							Plugin.CurrentRoute.Atmosphere.AmbientLightColor = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
						}
						break;
					case "diffuse":
						{
							object TempRed, TempGreen, TempBlue;
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

							Plugin.CurrentRoute.Atmosphere.DiffuseLightColor = new Color24((byte)(Red * 255), (byte)(Green * 255), (byte)(Blue * 255));
						}
						break;
					case "direction":
						{
							object Pitch, Yaw;
							if (!Statement.Arguments.TryGetValue("pitch", out Pitch))
							{
								Pitch = 60.0;
							}
							if (!Statement.Arguments.TryGetValue("yaw", out Yaw))
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

		private static void ConfirmCabIlluminance(bool PreviewOnly, MapData ParseData, List<Block> Blocks)
		{
			if (PreviewOnly)
			{
				return;
			}

			foreach (var Statement in ParseData.Statements)
			{
				if (Statement.MapElement[0] != "cabilluminance")
				{
					continue;
				}

				object TempValue;
				if (!Statement.Arguments.TryGetValue("value", out TempValue) || Convert.ToSingle(TempValue) == 0.0f)
				{
					TempValue = 1.0f;
				}

				float Value = Convert.ToSingle(TempValue);
				if (Value < 0.0f || Value > 1.0f)
				{
					Value = Value < 0.0f ? 0.0f : 1.0f;
				}

				int BlockIndex = Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
				Blocks[BlockIndex].BrightnessChanges.Add(new Brightness
				{
					TrackPosition = Statement.Distance,
					Value = Value
				});
			}
		}

		private static void ConfirmIrregularity(bool PreviewOnly, List<Block> Blocks)
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

		private static void ConfirmAdhesion(bool PreviewOnly, List<Block> Blocks)
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
				if (Statement.MapElement[0] != "sound")
				{
					continue;
				}

				int BlockIndex = RouteData.Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
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
				if (Statement.MapElement[0] != "sound3d")
				{
					continue;
				}

				object X, Y;
				Statement.Arguments.TryGetValue("x", out X);
				Statement.Arguments.TryGetValue("y", out Y);

				int BlockIndex = RouteData.Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
				RouteData.Blocks[BlockIndex].SoundEvents.Add(new Sound
				{
					TrackPosition = Statement.Distance,
					Key = Statement.Key,
					Type = SoundType.World,
					X = Convert.ToDouble(X),
					Y = Convert.ToDouble(Y)
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
				if (Statement.MapElement[0] != "rollingnoise")
				{
					continue;
				}

				object Index;
				Statement.Arguments.TryGetValue("index", out Index);

				int BlockIndex = RouteData.Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
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
				if (Statement.MapElement[0] != "flangenoise")
				{
					continue;
				}

				object Index;
				Statement.Arguments.TryGetValue("index", out Index);

				int BlockIndex = RouteData.Blocks.FindLastIndex(Block => Block.StartingDistance <= Statement.Distance);
				RouteData.Blocks[BlockIndex].FlangeSounds.Add(new TrackSound
				{
					TrackPosition = Statement.Distance,
					SoundIndex = Convert.ToInt32(Index)
				});
			}
		}
	}
}
