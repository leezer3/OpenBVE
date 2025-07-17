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
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.World;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.Events;
using RouteManager2.SignalManager;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		internal static Plugin plugin;
		private static void ApplyRouteData(string FileName, bool PreviewOnly, RouteData Data)
		{
			Plugin.CurrentOptions.UnitOfSpeed = "km/h";
			Plugin.CurrentOptions.SpeedConversionFactor = 0.0;

			Plugin.CurrentRoute.Sections = new RouteManager2.SignalManager.Section[1];
			Plugin.CurrentRoute.Sections[0] = new RouteManager2.SignalManager.Section(0, new[] { new SectionAspect(0, 0.0), new SectionAspect(4, double.PositiveInfinity) }, SectionType.ValueBased)
			{
				CurrentAspect = 0,
				StationIndex = -1
			};

			//FIXME: Quad-tree *should* be better (and we don't require any legacy stuff), but this produces an empty worldspace
			Plugin.CurrentRoute.AccurateObjectDisposal = ObjectDisposalMode.Accurate;
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			BackgroundHandle currentBackground = null;

			// background
			if (!PreviewOnly)
			{
				if (!string.IsNullOrEmpty(Data.Blocks[0].Background))
				{
					Plugin.CurrentRoute.CurrentBackground = new BackgroundObject((StaticObject)Data.Backgrounds[Data.Blocks[0].Background], Plugin.CurrentOptions.ViewingDistance, Data.Blocks[0].Background.IndexOf("dome", StringComparison.InvariantCultureIgnoreCase) == -1);
				}
				else
				{
					// find first block with valid background
					// as backgrounds are objects, we *must* have one, as opposed to the BVE2 / BVE4 default cylinder
					for (int i = 0; i < Data.Blocks.Count; i++)
					{
						if (!string.IsNullOrEmpty(Data.Blocks[i].Background))
						{
							Plugin.CurrentRoute.CurrentBackground = new BackgroundObject((StaticObject)Data.Backgrounds[Data.Blocks[i].Background], Plugin.CurrentOptions.ViewingDistance, Data.Blocks[0].Background.IndexOf("dome", StringComparison.InvariantCultureIgnoreCase) == -1);
							break;
						}
					}
				}
				Plugin.CurrentRoute.TargetBackground = Plugin.CurrentRoute.CurrentBackground;
				currentBackground = Plugin.CurrentRoute.CurrentBackground;
			}

			// brightness
			int CurrentBrightnessElement = -1;
			int CurrentBrightnessEvent = -1;
			float CurrentBrightnessValue = 1.0f;
			double CurrentBrightnessTrackPosition = 0.0;
			if (!PreviewOnly)
			{
				for (int i = 0; i < Data.Blocks.Count; i++)
				{
					if (Data.Blocks[i].BrightnessChanges != null && Data.Blocks[i].BrightnessChanges.Any())
					{
						CurrentBrightnessValue = Data.Blocks[i].BrightnessChanges[0].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].BrightnessChanges[0].Value;
						break;
					}
				}
			}

			// create objects and track
			Vector3 Position = Vector3.Zero;
			Vector2 Direction = new Vector2(0.0, 1.0);
			double CurrentSpeedLimit = double.PositiveInfinity;
			int CurrentRunIndex = 0;
			int CurrentFlangeIndex = 0;
			int CurrentTrackLength = 0;
			int PreviousFogElement = -1;
			int PreviousFogEvent = -1;
			Fog PreviousFog = new Fog(Plugin.CurrentRoute.NoFogStart, Plugin.CurrentRoute.NoFogEnd, Color24.Grey, -InterpolateInterval, false);
			Plugin.CurrentRoute.Tracks[0].Elements = new TrackElement[256];
			Plugin.CurrentRoute.Tracks[0].Direction = TrackDirection.Forwards;
			for (int j = 0; j < Data.TrackKeyList.Count; j++)
			{
				if (!Plugin.CurrentRoute.Tracks.ContainsKey(j))
				{
					Plugin.CurrentRoute.Tracks.Add(j, new Track(Data.TrackKeyList[j]));
				}
				if (Plugin.CurrentRoute.Tracks[j].Elements == null)
				{
					Plugin.CurrentRoute.Tracks[j].Elements = new TrackElement[256];
				}
			}

			// process blocks
			double progressFactor = Data.Blocks.Count == 0 ? 0.5 : 0.5 / Data.Blocks.Count;
			for (int i = 0; i < Data.Blocks.Count; i++)
			{
				plugin.CurrentProgress = 0.6667 + i * progressFactor;
				if ((i & 15) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (plugin.Cancel) return;
				}

				double StartingDistance = Data.Blocks[i].StartingDistance;
				double EndingDistance = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].StartingDistance : StartingDistance + InterpolateInterval;
				double BlockInterval = EndingDistance - StartingDistance;

				// normalize
				Direction.Normalize();

				TrackElement WorldTrackElement = Data.Blocks[i].CurrentTrackState;
				int n = CurrentTrackLength;
				for (int k = 0; k < Plugin.CurrentRoute.Tracks.Count; k++)
				{
					int j = Plugin.CurrentRoute.Tracks.ElementAt(k).Key;
					if (n >= Plugin.CurrentRoute.Tracks[j].Elements.Length)
					{
						Array.Resize(ref Plugin.CurrentRoute.Tracks[j].Elements, Plugin.CurrentRoute.Tracks[j].Elements.Length << 1);
					}
				}
				CurrentTrackLength++;
				Plugin.CurrentRoute.Tracks[0].Elements[n] = WorldTrackElement;
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldPosition = Position;
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection = Vector3.GetVector3(Direction, Data.Blocks[i].Pitch);
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X);
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection, Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide);
				Plugin.CurrentRoute.Tracks[0].Elements[n].StartingTrackPosition = StartingDistance;
				Plugin.CurrentRoute.Tracks[0].Elements[n].AdhesionMultiplier = Data.Blocks[i].AdhesionMultiplier;
				Plugin.CurrentRoute.Tracks[0].Elements[n].CsvRwAccuracyLevel = Data.Blocks[i].Accuracy;
				for (int k = 0; k < Plugin.CurrentRoute.Tracks.Count; k++)
				{
					int j = Plugin.CurrentRoute.Tracks.ElementAt(k).Key;
					Plugin.CurrentRoute.Tracks[j].Elements[n].Events = new List<GeneralEvent>();
				}

				// background
				if (!PreviewOnly)
				{
					if (!string.IsNullOrEmpty(Data.Blocks[i].Background))
					{
						if (Data.Backgrounds.ContainsKey(Data.Blocks[i].Background))
						{
							// HACK: Assume that backgrounds containing DOME in the filename have a complete dome, and so do not need caps generated
							BackgroundHandle nextBackground = new BackgroundObject((StaticObject)Data.Backgrounds[Data.Blocks[i].Background], Plugin.CurrentOptions.ViewingDistance, Data.Blocks[0].Background.IndexOf("dome", StringComparison.InvariantCultureIgnoreCase) == -1);
							Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new BackgroundChangeEvent(Plugin.CurrentRoute, 0.0, currentBackground, nextBackground));
						}
					}
				}

				// brightness
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].BrightnessChanges.Count; j++)
					{
						Data.Blocks[i].BrightnessChanges[j].Create(n, StartingDistance, ref CurrentBrightnessElement, ref CurrentBrightnessEvent, ref CurrentBrightnessValue, ref CurrentBrightnessTrackPosition);
					}
				}

				// fog
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].FogDefined)
					{
						if (i == 0 && StartingDistance == 0)
						{
							//Fog starts at zero position
							PreviousFog = Data.Blocks[i].Fog;
						}
						Data.Blocks[i].Fog.TrackPosition = StartingDistance;
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new FogChangeEvent(Plugin.CurrentRoute, 0.0, PreviousFog, Data.Blocks[i].Fog, Data.Blocks[i].Fog));
						if (PreviousFogElement >= 0 & PreviousFogEvent >= 0)
						{
							FogChangeEvent e = (FogChangeEvent)Plugin.CurrentRoute.Tracks[0].Elements[PreviousFogElement].Events[PreviousFogEvent];
							e.NextFog = Data.Blocks[i].Fog;
						}
						else
						{
							Plugin.CurrentRoute.PreviousFog = PreviousFog;
							Plugin.CurrentRoute.CurrentFog = PreviousFog;
							Plugin.CurrentRoute.NextFog = Data.Blocks[i].Fog;
						}
						PreviousFog = Data.Blocks[i].Fog;
						PreviousFogElement = n;
						PreviousFogEvent = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Count - 1;
					}
				}

				// rail sounds
				if (!PreviewOnly)
				{
					for (int k = 0; k < Data.Blocks[i].RunSounds.Count; k++)
					{
						Data.Blocks[i].RunSounds[k].Create(n, StartingDistance, ref CurrentRunIndex, CurrentFlangeIndex);
					}

					for (int k = 0; k < Data.Blocks[i].FlangeSounds.Count; k++)
					{
						Data.Blocks[i].FlangeSounds[k].Create(n, StartingDistance, CurrentRunIndex, ref CurrentFlangeIndex);
					}

					if (Data.Blocks[i].JointSound)
					{
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new PointSoundEvent());
					}
				}

				// station
				if (Data.Blocks[i].StationIndex >= 0)
				{
					int s = Data.Blocks[i].StationIndex;
					Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new StationStartEvent(Plugin.CurrentRoute, 0.0, s));
					double dx, dy = 3.0;
					if (Plugin.CurrentRoute.Stations[s].OpenLeftDoors & !Plugin.CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!Plugin.CurrentRoute.Stations[s].OpenLeftDoors & Plugin.CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					Plugin.CurrentRoute.Stations[s].SoundOrigin = Position + dx * Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide + dy * Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp;
				}

				// stop
				if (Data.Blocks[i].Stop >= 0)
				{
					int s = Data.Blocks[i].Stop;
					double dx, dy = 3.0;
					if (Plugin.CurrentRoute.Stations[s].OpenLeftDoors & !Plugin.CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!Plugin.CurrentRoute.Stations[s].OpenLeftDoors & Plugin.CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					Plugin.CurrentRoute.Stations[s].SoundOrigin = Position + dx * Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide + dy * Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp;
				}

				// limit
				if (!PreviewOnly)
				{
					for (int k = 0; k < Data.Blocks[i].Limits.Count; k++)
					{
						Data.Blocks[i].Limits[k].Create(n, StartingDistance, ref CurrentSpeedLimit);
					}
				}


				// sections
				if (!PreviewOnly)
				{
					// sections
					for (int k = 0; k < Data.Blocks[i].Sections.Count; k++)
					{
						Data.Blocks[i].Sections[k].Create(Data, n, StartingDistance);
					}
				}

				// rail-aligned objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Rails.Count; j++)
					{
						// free objects (including placed repeaters)
						string railKey = Data.Blocks[i].Rails.ElementAt(j).Key;
						if (Data.Blocks[i].FreeObjects.ContainsKey(railKey))
						{
							for (int k = 0; k < Data.Blocks[i].FreeObjects[railKey].Count; k++)
							{
								string key = Data.Blocks[i].FreeObjects[railKey][k].Key;
								double dx = Data.Blocks[i].FreeObjects[railKey][k].Position.X;
								double dy = Data.Blocks[i].FreeObjects[railKey][k].Position.Y;
								double dz = Data.Blocks[i].FreeObjects[railKey][k].Position.Z;
								double tpos = Data.Blocks[i].FreeObjects[railKey][k].TrackPosition;
								Vector3 wpos;
								Transformation Transformation;
								if (j == 0)
								{
									GetPrimaryRailTransformation(Position, Data.Blocks, i, Data.Blocks[i].FreeObjects[railKey][k], Direction, out wpos, out Transformation);
								}
								else
								{
									GetSecondaryRailTransformation(Position, Direction, Data.Blocks, i, railKey, Data.Blocks[i].FreeObjects[railKey][k], out wpos, out Transformation);
								}
								wpos += dx * Transformation.X + dy * Transformation.Y + dz * Transformation.Z;
								Data.Objects.TryGetValue(key, out UnifiedObject obj);
								obj?.CreateObject(wpos, Transformation, new Transformation(Data.Blocks[i].FreeObjects[railKey][k].Yaw, Data.Blocks[i].FreeObjects[railKey][k].Pitch, Data.Blocks[i].FreeObjects[railKey][k].Roll), -1, StartingDistance, EndingDistance, tpos, 1.0);
							}
						}

						// cracks
						for (int k = 0; k < Data.Blocks[i].Cracks.Count; k++)
						{
							if (Data.Blocks[i].Cracks[k].PrimaryRail == railKey)
							{
								double nextStartingDistance = StartingDistance + BlockInterval;
								string p = Data.Blocks[i].Cracks[k].PrimaryRail;
								double px0 = Data.Blocks[i].Rails[p].Position.X;
								double pRadiusH = Data.Blocks[i].Rails[p].RadiusH;
								double px1 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[p].Position.X : px0;
								string s = Data.Blocks[i].Cracks[k].SecondaryRail;
								double sx0 = Data.Blocks[i].Rails[s].Position.X;
								double sRadiusH = Data.Blocks[i].Rails[s].RadiusH;
								double sx1 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[s].Position.X : sx0;

								string key = Data.Blocks[i].Cracks[k].Key;
								double tpos = Data.Blocks[i].Cracks[k].TrackPosition;
								Vector3 wpos;
								Transformation Transformation;
								if (j == 0)
								{
									GetPrimaryRailTransformation(Position, Data.Blocks, i, Data.Blocks[i].Cracks[k], Direction, out wpos, out Transformation);
								}
								else
								{
									GetSecondaryRailTransformation(Position, Direction, Data.Blocks, i, railKey, Data.Blocks[i].Cracks[k], out wpos, out Transformation);
								}

								double pInterpolateX0 = GetTrackCoordinate(StartingDistance, px0, nextStartingDistance, px1, pRadiusH, tpos);
								double pInterpolateX1 = GetTrackCoordinate(StartingDistance, px0, nextStartingDistance, px1, pRadiusH, tpos + InterpolateInterval);
								double sInterpolateX0 = GetTrackCoordinate(StartingDistance, sx0, nextStartingDistance, sx1, sRadiusH, tpos);
								double sInterpolateX1 = GetTrackCoordinate(StartingDistance, sx0, nextStartingDistance, sx1, sRadiusH, tpos + InterpolateInterval);
								double d0 = sInterpolateX0 - pInterpolateX0;
								double d1 = sInterpolateX1 - pInterpolateX1;

								if(Data.Objects.TryGetValue(key, out UnifiedObject obj) && obj != null)
								{
									UnifiedObject crack = d0 < 0.0 ? obj.TransformRight(d0, d1) : obj.TransformLeft(d0, d1);
									crack.CreateObject(wpos, Transformation, new Transformation(0.0, 0.0, 0.0), -1, StartingDistance, EndingDistance, tpos, 1.0);
								}
							}
						}

						// signals
						if (Data.Blocks[i].Signals.Length > j && Data.Blocks[i].Signals[j] != null)
						{
							for (int k = 0; k < Data.Blocks[i].Signals[j].Count; k++)
							{
								string key = Data.Blocks[i].Signals[j][k].Key;
								double dx = Data.Blocks[i].Signals[j][k].Position.X;
								double dy = Data.Blocks[i].Signals[j][k].Position.Y;
								double dz = Data.Blocks[i].Signals[j][k].Position.Z;
								double tpos = Data.Blocks[i].Signals[j][k].TrackPosition;
								Vector3 wpos;
								Transformation Transformation;
								if (j == 0)
								{
									GetPrimaryRailTransformation(Position, Data.Blocks, i, Data.Blocks[i].Signals[j][k], Direction, out wpos, out Transformation);
								}
								else
								{
									GetSecondaryRailTransformation(Position, Direction, Data.Blocks, i, railKey, Data.Blocks[i].Signals[j][k], out wpos, out Transformation);
								}
								wpos += dx * Transformation.X + dy * Transformation.Y + dz * Transformation.Z;

								SignalData sd = Data.SignalObjects.Find(data => data.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
								if (sd != null)
								{
									if (sd.Numbers.Any())
									{
										AnimatedObjectCollection aoc = new AnimatedObjectCollection(Plugin.CurrentHost);
										aoc.Objects = new AnimatedObject[2];
										for (int m = 0; m < aoc.Objects.Length; m++)
										{
											aoc.Objects[m] = new AnimatedObject(Plugin.CurrentHost);
											aoc.Objects[m].States = new ObjectState[sd.Numbers.Length];
										}
										for (int m = 0; m < sd.Numbers.Length; m++)
										{
											aoc.Objects[0].States[m] = new ObjectState((StaticObject)sd.BaseObjects[m].Clone());
											if (sd.GlowObjects != null && m < sd.GlowObjects.Length)
											{
												aoc.Objects[1].States[m] = new ObjectState((StaticObject)sd.GlowObjects[m].Clone());
											}
											else
											{
												aoc.Objects[1].States[m] = new ObjectState(new StaticObject(Plugin.CurrentHost));
											}
										}
										string expr = "";
										for (int m = 0; m < sd.Numbers.Length - 1; m++)
										{
											expr += "section " + sd.Numbers[m].ToString(Culture) + " <= " + m.ToString(Culture) + " ";
										}
										expr += (sd.Numbers.Length - 1).ToString(Culture);
										for (int m = 0; m < sd.Numbers.Length - 1; m++)
										{
											expr += " ?";
										}

										double refreshRate = 1.0 + 0.01 * Plugin.RandomNumberGenerator.NextDouble();
										for (int m = 0; m < aoc.Objects.Length; m++)
										{
											aoc.Objects[m].StateFunction = new FunctionScript(Plugin.CurrentHost, expr, false);
											aoc.Objects[m].RefreshRate = refreshRate;
										}

										aoc.CreateObject(wpos, Transformation, new Transformation(Data.Blocks[i].Signals[j][k].Yaw, Data.Blocks[i].Signals[j][k].Pitch, Data.Blocks[i].Signals[j][k].Roll), Data.Blocks[i].Signals[j][k].SectionIndex, StartingDistance, EndingDistance, tpos, 1.0);
									}
								}
							}
						}
					}
				}

				// turn
				if (Data.Blocks[i].Turn != 0.0)
				{
					double ag = -Math.Atan(Data.Blocks[i].Turn);
					Direction.Rotate(ag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection.RotatePlane(ag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide.RotatePlane(ag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection, Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide);
				}

				//Pitch
				Plugin.CurrentRoute.Tracks[0].Elements[n].Pitch = Data.Blocks[i].Pitch;

				// curves
				CalcTransformation(WorldTrackElement.CurveRadius, Data.Blocks[i].Pitch, BlockInterval, ref Direction, out double a, out double c, out double h);

				if (!PreviewOnly)
				{
					for (int j = 1; j < Data.TrackKeyList.Count; j++)
					{
						string railKey = Data.TrackKeyList[j];
						double x = Data.Blocks[i].Rails[railKey].Position.X;
						double y = Data.Blocks[i].Rails[railKey].Position.Y;
						Vector3 offset = new Vector3(Direction.Y * x, y, -Direction.X * x);
						Vector3 pos = Position + offset;

						// take orientation of upcoming block into account
						Vector2 Direction2 = Direction;
						Vector3 Position2 = Position;
						Position2.X += Direction.X * c;
						Position2.Y += h;
						Position2.Z += Direction.Y * c;
						Direction2.Rotate(-a);

						double StartingDistance2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].StartingDistance : StartingDistance + InterpolateInterval;
						double EndingDistance2 = i < Data.Blocks.Count - 2 ? Data.Blocks[i + 2].StartingDistance : StartingDistance2 + InterpolateInterval;
						double BlockInterval2 = EndingDistance2 - StartingDistance2;
						double Turn2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Turn : 0.0;
						double CurveRadius2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].CurrentTrackState.CurveRadius : 0.0;
						double Pitch2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Pitch : 0.0;

						Direction2.Rotate(-Math.Atan(Turn2));

						CalcTransformation(CurveRadius2, Pitch2, BlockInterval2, ref Direction2, out _, out _, out _);

						double x2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[railKey].Position.X : x;
						double y2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[railKey].Position.Y : y;
						Vector3 offset2 = new Vector3(Direction2.Y * x2, y2, -Direction2.X * x2);
						Vector3 pos2 = Position2 + offset2;
						Vector3 r = new Vector3(pos2.X - pos.X, pos2.Y - pos.Y, pos2.Z - pos.Z);
						r.Normalize();

						Transformation RailTransformation = new Transformation(r, Vector3.Down, new Vector3(r.Z, 0.0, -r.X));
						Normalize(ref RailTransformation.X.X, ref RailTransformation.X.Z);
						RailTransformation.Y = Vector3.Cross(RailTransformation.Z, RailTransformation.X);
						RailTransformation = new Transformation(RailTransformation, 0.0, 0.0, Math.Atan(Data.Blocks[i].Rails[railKey].CurveCant));

						Plugin.CurrentRoute.Tracks[j].Elements[n].StartingTrackPosition = StartingDistance;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldPosition = pos;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldDirection = RailTransformation.Z;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldSide = RailTransformation.X;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldUp = RailTransformation.Y;
						Plugin.CurrentRoute.Tracks[j].Elements[n].CurveCant = Data.Blocks[i].Rails[railKey].CurveCant;
					}
				}

				// world sounds
				for (int k = 0; k < Data.Blocks[i].SoundEvents.Count; k++)
				{
					Data.Blocks[i].SoundEvents[k].Create(Data, n, StartingDistance, Position, Direction);
				}

				// finalize block
				Position.X += Direction.X * c;
				Position.Y += h;
				Position.Z += Direction.Y * c;
				Direction.Rotate(-a);
			}

			// transponders
			if (!PreviewOnly)
			{
				for (int i = 0; i < Data.Blocks.Count; i++)
				{
					for (int k = 0; k < Data.Blocks[i].Transponders.Count; k++)
					{
						if (Data.Blocks[i].Transponders[k].Type != -1)
						{
							int n = i;
							double d = Data.Blocks[i].Transponders[k].TrackPosition - Plugin.CurrentRoute.Tracks[0].Elements[n].StartingTrackPosition;
							int s = Data.Blocks[i].Transponders[k].SectionIndex;
							if (s < 0 || s >= Plugin.CurrentRoute.Sections.Length)
							{
								s = -1;
							}
							Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new TransponderEvent(Plugin.CurrentRoute, d, Data.Blocks[i].Transponders[k].Type, Data.Blocks[i].Transponders[k].Data, s, false));
							Data.Blocks[i].Transponders[k].Type = -1;
						}
					}
				}
			}

			// insert station end events
			for (int i = 0; i < Plugin.CurrentRoute.Stations.Length; i++)
			{
				int j = Plugin.CurrentRoute.Stations[i].Stops.Length - 1;
				if (j >= 0)
				{
					double p = Plugin.CurrentRoute.Stations[i].Stops[j].TrackPosition + Plugin.CurrentRoute.Stations[i].Stops[j].ForwardTolerance;
					int k = Data.sortedBlocks.FindBlockIndex(p);
					if (k != -1)
					{
						double d = p - Data.Blocks[k].StartingDistance;
						Plugin.CurrentRoute.Tracks[0].Elements[k].Events.Add(new StationEndEvent(Plugin.CurrentHost, Plugin.CurrentRoute, d, i));
					}
				}
			}

			// create default point of interests
			if (!PreviewOnly)
			{
				if (Plugin.CurrentRoute.PointsOfInterest.Length == 0)
				{
					Plugin.CurrentRoute.PointsOfInterest = new PointOfInterest[Plugin.CurrentRoute.Stations.Length];
					int n = 0;
					for (int i = 0; i < Plugin.CurrentRoute.Stations.Length; i++)
					{
						if (Plugin.CurrentRoute.Stations[i].Stops.Length != 0)
						{
							Plugin.CurrentRoute.PointsOfInterest[n].Text = Plugin.CurrentRoute.Stations[i].Name;
							Plugin.CurrentRoute.PointsOfInterest[n].TrackPosition = Plugin.CurrentRoute.Stations[i].Stops[0].TrackPosition;
							Plugin.CurrentRoute.PointsOfInterest[n].TrackOffset = new Vector3(0.0, 2.8, 0.0);
							if (Plugin.CurrentRoute.Stations[i].OpenLeftDoors & !Plugin.CurrentRoute.Stations[i].OpenRightDoors)
							{
								Plugin.CurrentRoute.PointsOfInterest[n].TrackOffset.X = -2.5;
							}
							else if (!Plugin.CurrentRoute.Stations[i].OpenLeftDoors & Plugin.CurrentRoute.Stations[i].OpenRightDoors)
							{
								Plugin.CurrentRoute.PointsOfInterest[n].TrackOffset.X = 2.5;
							}
							n++;
						}
					}
					Array.Resize(ref Plugin.CurrentRoute.PointsOfInterest, n);
				}
			}

			// convert block-based cant into point-based cant
			if (!PreviewOnly)
			{
				for (int k = 0; k < Plugin.CurrentRoute.Tracks.Count; k++)
				{
					int i = Plugin.CurrentRoute.Tracks.ElementAt(k).Key;
					for (int j = CurrentTrackLength - 1; j >= 1; j--)
					{
						if (Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant == 0.0)
						{
							Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant = Plugin.CurrentRoute.Tracks[i].Elements[j - 1].CurveCant;
						}
						else if (Plugin.CurrentRoute.Tracks[i].Elements[j - 1].CurveCant != 0.0)
						{
							if (Math.Sign(Plugin.CurrentRoute.Tracks[i].Elements[j - 1].CurveCant) == Math.Sign(Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant))
							{
								if (Math.Abs(Plugin.CurrentRoute.Tracks[i].Elements[j - 1].CurveCant) > Math.Abs(Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant))
								{
									Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant = Plugin.CurrentRoute.Tracks[i].Elements[j - 1].CurveCant;
								}
							}
							else
							{
								Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant = 0.5 * (Plugin.CurrentRoute.Tracks[i].Elements[j].CurveCant + Plugin.CurrentRoute.Tracks[i].Elements[j - 1].CurveCant);
							}
						}
					}
				}
			}

			// finalize
			for (int j = 0; j < Plugin.CurrentRoute.Tracks.Count; j++)
			{
				int i = Plugin.CurrentRoute.Tracks.ElementAt(j).Key;
				Array.Resize(ref Plugin.CurrentRoute.Tracks[i].Elements, CurrentTrackLength);
			}
			for (int i = 0; i < Plugin.CurrentRoute.Stations.Length; i++)
			{
				if (Plugin.CurrentRoute.Stations[i].Stops.Length == 0 & Plugin.CurrentRoute.Stations[i].StopMode != StationStopMode.AllPass)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "StationIndex " + Plugin.CurrentRoute.Stations[i].Name + " expects trains to stop but does not define stop points at track position " + Plugin.CurrentRoute.Stations[i].DefaultTrackPosition.ToString(Culture) + " in file " + FileName);
					Plugin.CurrentRoute.Stations[i].StopMode = StationStopMode.AllPass;
				}
			}
			if (Plugin.CurrentRoute.Stations.Any())
			{
				Plugin.CurrentRoute.Stations.Last().Type = StationType.Terminal;
			}
			if (Plugin.CurrentRoute.Tracks[0].Elements.Any())
			{
				int n = Plugin.CurrentRoute.Tracks[0].Elements.Length - 1;
				Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new TrackEndEvent(Plugin.CurrentHost, InterpolateInterval));
			}

			// cant
			if (!PreviewOnly)
			{
				ComputeCantTangents();
			}
		}

		private static void ComputeCantTangents()
		{
			for (int ii = 0; ii < Plugin.CurrentRoute.Tracks.Count; ii++)
			{
				int i = Plugin.CurrentRoute.Tracks.ElementAt(ii).Key;
				Plugin.CurrentRoute.Tracks[i].ComputeCantTangents();
			}
		}
	}
}
