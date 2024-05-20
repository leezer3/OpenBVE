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
using TrainManager.Trains;
using SoundHandle = OpenBveApi.Sounds.SoundHandle;

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
			Plugin.CurrentRoute.Sections[0] = new RouteManager2.SignalManager.Section(0, new SectionAspect[] { new SectionAspect(0, 0.0), new SectionAspect(4, double.PositiveInfinity) }, SectionType.ValueBased);
			Plugin.CurrentRoute.Sections[0].CurrentAspect = 0;
			Plugin.CurrentRoute.Sections[0].StationIndex = -1;

			//FIXME: Quad-tree *should* be better (and we don't require any legacy stuff), but this produces an empty worldspace
			Plugin.CurrentRoute.AccurateObjectDisposal = ObjectDisposalMode.Accurate;
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

			// background
			if (!PreviewOnly)
			{
				if (Data.Blocks[0].Background >= 0 & Data.Blocks[0].Background < Data.Backgrounds.Count)
				{
					Plugin.CurrentRoute.CurrentBackground = Data.Backgrounds[Data.Blocks[0].Background].Handle;
				}
				else
				{
					Plugin.CurrentRoute.CurrentBackground = new StaticBackground(null, 6, false);
				}
				Plugin.CurrentRoute.TargetBackground = Plugin.CurrentRoute.CurrentBackground;
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
			Fog PreviousFog = new Fog(Plugin.CurrentRoute.NoFogStart, Plugin.CurrentRoute.NoFogEnd, Color24.Grey, -InterpolateInterval);
			Plugin.CurrentRoute.Tracks[0].Elements = new TrackElement[256];
			Plugin.CurrentRoute.Tracks[0].Direction = TrackDirection.Forwards;
			for (int j = 0; j < Data.TrackKeyList.Count; j++)
			{
				if (!Plugin.CurrentRoute.Tracks.ContainsKey(j))
				{
					Plugin.CurrentRoute.Tracks.Add(j, new Track());
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
					if (Data.Blocks[i].Background >= 0)
					{
						int typ;
						if (i == 0)
						{
							typ = Data.Blocks[i].Background;
						}
						else
						{
							typ = Data.Backgrounds.Count > 0 ? 0 : -1;
							for (int j = i - 1; j >= 0; j--)
							{
								if (Data.Blocks[j].Background >= 0)
								{
									typ = Data.Blocks[j].Background;
									break;
								}
							}
						}
						if (typ >= 0 & typ < Data.Backgrounds.Count)
						{
							
							Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new BackgroundChangeEvent(Plugin.CurrentRoute, 0.0, Data.Backgrounds[typ].Handle, Data.Backgrounds[Data.Blocks[i].Background].Handle));
						}
					}
				}

				// brightness
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].BrightnessChanges.Count; j++)
					{
						for (int l = 0; l < Plugin.CurrentRoute.Tracks.Count; l++)
						{
							int k = Plugin.CurrentRoute.Tracks.ElementAt(l).Key;
							double d = Data.Blocks[i].BrightnessChanges[j].TrackPosition - StartingDistance;
							Plugin.CurrentRoute.Tracks[k].Elements[n].Events.Add(new BrightnessChangeEvent(d, Data.Blocks[i].BrightnessChanges[j].Value, CurrentBrightnessValue, Data.Blocks[i].BrightnessChanges[j].TrackPosition - CurrentBrightnessTrackPosition));
							if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
							{
								BrightnessChangeEvent bce = (BrightnessChangeEvent)Plugin.CurrentRoute.Tracks[0].Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
								bce.NextBrightness = Data.Blocks[i].BrightnessChanges[j].Value;
								bce.NextDistance = Data.Blocks[i].BrightnessChanges[j].TrackPosition - CurrentBrightnessTrackPosition;
							}
						}
						CurrentBrightnessElement = n;
						CurrentBrightnessEvent = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Count - 1;
						CurrentBrightnessValue = Data.Blocks[i].BrightnessChanges[j].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].BrightnessChanges[j].TrackPosition;
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
						int r = Data.Blocks[i].RunSounds[k].SoundIndex;
						if (r != CurrentRunIndex)
						{
							double d = Data.Blocks[i].RunSounds[k].TrackPosition - StartingDistance;
							if (d > 0.0)
							{
								d = 0.0;
							}
							Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new RailSoundsChangeEvent(d, CurrentRunIndex, CurrentFlangeIndex, r, CurrentFlangeIndex));
							CurrentRunIndex = r;
						}
					}

					for (int k = 0; k < Data.Blocks[i].FlangeSounds.Count; k++)
					{
						int f = Data.Blocks[i].FlangeSounds[k].SoundIndex;
						if (f != CurrentFlangeIndex)
						{
							double d = Data.Blocks[i].FlangeSounds[k].TrackPosition - StartingDistance;
							if (d > 0.0)
							{
								d = 0.0;
							}
							Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new RailSoundsChangeEvent(d, CurrentRunIndex, CurrentFlangeIndex, CurrentRunIndex, f));
							CurrentFlangeIndex = f;
						}
					}

					if (Data.Blocks[i].JointSound)
					{
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new PointSoundEvent());
					}
				}

				// station
				if (Data.Blocks[i].Station >= 0)
				{
					int s = Data.Blocks[i].Station;
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
						double d = Data.Blocks[i].Limits[k].TrackPosition - StartingDistance;
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new LimitChangeEvent(Plugin.CurrentRoute, d, CurrentSpeedLimit, Data.Blocks[i].Limits[k].Speed));
						CurrentSpeedLimit = Data.Blocks[i].Limits[k].Speed;
					}
				}

				// sound
				if (!PreviewOnly)
				{
					for (int k = 0; k < Data.Blocks[i].SoundEvents.Count; k++)
					{
						if (Data.Blocks[i].SoundEvents[k].Type == SoundType.TrainStatic)
						{
							OpenBveApi.Sounds.SoundHandle buffer;
							Data.Sounds.TryGetValue(Data.Blocks[i].SoundEvents[k].Key, out buffer);

							if (buffer != null)
							{
								double d = Data.Blocks[i].SoundEvents[k].TrackPosition - StartingDistance;
								Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new SoundEvent(Plugin.CurrentHost, d, buffer, true, true, false, false, Vector3.Zero, 0.0));
							}
						}
					}
				}

				// sections
				if (!PreviewOnly)
				{
					// sections
					for (int k = 0; k < Data.Blocks[i].Sections.Count; k++)
					{
						int m = Plugin.CurrentRoute.Sections.Length;
						Array.Resize(ref Plugin.CurrentRoute.Sections, m + 1);
						RouteManager2.SignalManager.Section previousSection = null;
						if (m > 0)
						{
							previousSection = Plugin.CurrentRoute.Sections[m - 1];
						}

						Plugin.CurrentRoute.Sections[m] = new RouteManager2.SignalManager.Section(Data.Blocks[i].Sections[k].TrackPosition, new SectionAspect[Data.Blocks[i].Sections[k].Aspects.Length], SectionType.IndexBased, previousSection);

						if (m > 0)
						{
							Plugin.CurrentRoute.Sections[m - 1].NextSection = Plugin.CurrentRoute.Sections[m];
						}
						// create section
						
						for (int l = 0; l < Data.Blocks[i].Sections[k].Aspects.Length; l++)
						{
							Plugin.CurrentRoute.Sections[m].Aspects[l].Number = Data.Blocks[i].Sections[k].Aspects[l];
							if (Data.Blocks[i].Sections[k].Aspects[l] >= 0 & Data.Blocks[i].Sections[k].Aspects[l] < Data.SignalSpeeds.Length)
							{
								Plugin.CurrentRoute.Sections[m].Aspects[l].Speed = Data.SignalSpeeds[Data.Blocks[i].Sections[k].Aspects[l]];
							}
							else
							{
								Plugin.CurrentRoute.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
							}
						}
						Plugin.CurrentRoute.Sections[m].CurrentAspect = -1;
						Plugin.CurrentRoute.Sections[m].StationIndex = Data.Blocks[i].Sections[k].DepartureStationIndex;
						Plugin.CurrentRoute.Sections[m].Invisible = false;
						Plugin.CurrentRoute.Sections[m].Trains = new TrainBase[] { };

						// create section change event
						double d = Data.Blocks[i].Sections[k].TrackPosition - StartingDistance;
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Add(new SectionChangeEvent(Plugin.CurrentRoute, d, m - 1, m));
					}
				}

				// rail-aligned objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Rails.Length; j++)
					{
						// free objects
						if (Data.Blocks[i].FreeObj.Length > j && Data.Blocks[i].FreeObj[j] != null)
						{
							double turn = Data.Blocks[i].Turn;
							double curveRadius = Data.Blocks[i].CurrentTrackState.CurveRadius;
							double curveCant = j == 0 ? Data.Blocks[i].CurrentTrackState.CurveCant : Data.Blocks[i].Rails[j].CurveCant;
							double pitch = Data.Blocks[i].Pitch;
							double x = Data.Blocks[i].Rails[j].RailX;
							double y = Data.Blocks[i].Rails[j].RailY;
							double radiusH = Data.Blocks[i].Rails[j].RadiusH;
							double radiusV = Data.Blocks[i].Rails[j].RadiusV;
							double nextStartingDistance = StartingDistance + BlockInterval;
							double nextX = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[j].RailX : x;
							double nextY = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[j].RailY : y;

							for (int k = 0; k < Data.Blocks[i].FreeObj[j].Count; k++)
							{
								string key = Data.Blocks[i].FreeObj[j][k].Key;
								double span = Data.Blocks[i].FreeObj[j][k].Span;
								int type = Data.Blocks[i].FreeObj[j][k].Type;
								double dx = Data.Blocks[i].FreeObj[j][k].X;
								double dy = Data.Blocks[i].FreeObj[j][k].Y;
								double dz = Data.Blocks[i].FreeObj[j][k].Z;
								double tpos = Data.Blocks[i].FreeObj[j][k].TrackPosition;
								Vector3 wpos;
								Transformation Transformation;
								if (j == 0)
								{
									GetTransformation(Position, StartingDistance, turn, curveRadius, curveCant, pitch, tpos, type, span, Direction, out wpos, out Transformation);
								}
								else
								{
									GetTransformation(Position, StartingDistance, turn, curveRadius, pitch, x, y, radiusH, radiusV, curveCant, nextStartingDistance, nextX, nextY, tpos, type, span, Direction, out wpos, out Transformation);
								}
								wpos += dx * Transformation.X + dy * Transformation.Y + dz * Transformation.Z;
								UnifiedObject obj;
								Data.Objects.TryGetValue(key, out obj);
								if (obj != null)
								{
									obj.CreateObject(wpos, Transformation, new Transformation(Data.Blocks[i].FreeObj[j][k].Yaw, Data.Blocks[i].FreeObj[j][k].Pitch, Data.Blocks[i].FreeObj[j][k].Roll), -1, StartingDistance, EndingDistance, tpos, 1.0, false);
								}
							}
						}

						// cracks
						for (int k = 0; k < Data.Blocks[i].Cracks.Count; k++)
						{
							if (Data.Blocks[i].Cracks[k].PrimaryRail == j)
							{
								double turn = Data.Blocks[i].Turn;
								double curveRadius = Data.Blocks[i].CurrentTrackState.CurveRadius;
								double pitch = Data.Blocks[i].Pitch;
								double nextStartingDistance = StartingDistance + BlockInterval;

								int p = Data.Blocks[i].Cracks[k].PrimaryRail;
								double px0 = Data.Blocks[i].Rails[p].RailX;
								double py0 = Data.Blocks[i].Rails[p].RailY;
								double pRadiusH = Data.Blocks[i].Rails[p].RadiusH;
								double pRadiusV = Data.Blocks[i].Rails[p].RadiusV;
								double py1 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[p].RailY : py0;
								double px1 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[p].RailX : px0;

								int s = Data.Blocks[i].Cracks[k].SecondaryRail;
								double sx0 = Data.Blocks[i].Rails[s].RailX;
								double sRadiusH = Data.Blocks[i].Rails[s].RadiusH;
								double sx1 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[s].RailX : sx0;

								string key = Data.Blocks[i].Cracks[k].Key;
								double tpos = Data.Blocks[i].Cracks[k].TrackPosition;
								Vector3 wpos;
								Transformation Transformation;
								if (j == 0)
								{
									GetTransformation(Position, StartingDistance, turn, curveRadius, 0.0, pitch, tpos, 1, InterpolateInterval, Direction, out wpos, out Transformation);
								}
								else
								{
									GetTransformation(Position, StartingDistance, turn, curveRadius, pitch, px0, py0, pRadiusH, pRadiusV, 0.0, nextStartingDistance, px1, py1, tpos, 1, InterpolateInterval, Direction, out wpos, out Transformation);
								}

								double pInterpolateX0 = GetTrackCoordinate(StartingDistance, px0, nextStartingDistance, px1, pRadiusH, tpos);
								double pInterpolateX1 = GetTrackCoordinate(StartingDistance, px0, nextStartingDistance, px1, pRadiusH, tpos + InterpolateInterval);
								double sInterpolateX0 = GetTrackCoordinate(StartingDistance, sx0, nextStartingDistance, sx1, sRadiusH, tpos);
								double sInterpolateX1 = GetTrackCoordinate(StartingDistance, sx0, nextStartingDistance, sx1, sRadiusH, tpos + InterpolateInterval);
								double d0 = sInterpolateX0 - pInterpolateX0;
								double d1 = sInterpolateX1 - pInterpolateX1;

								UnifiedObject obj;
								Data.Objects.TryGetValue(key, out obj);
								if (obj != null)
								{
									StaticObject crack = GetTransformedStaticObject((StaticObject)obj, d0, d1);
									crack.CreateObject(wpos, Transformation, new Transformation(0.0, 0.0, 0.0), -1, StartingDistance, EndingDistance, BlockInterval, tpos);
								}
							}
						}

						// signals
						if (Data.Blocks[i].Signals.Length > j && Data.Blocks[i].Signals[j] != null)
						{
							double turn = Data.Blocks[i].Turn;
							double curveRadius = Data.Blocks[i].CurrentTrackState.CurveRadius;
							double curveCant = j == 0 ? Data.Blocks[i].CurrentTrackState.CurveCant : Data.Blocks[i].Rails[j].CurveCant;
							double pitch = Data.Blocks[i].Pitch;
							double x = Data.Blocks[i].Rails[j].RailX;
							double y = Data.Blocks[i].Rails[j].RailY;
							double radiusH = Data.Blocks[i].Rails[j].RadiusH;
							double radiusV = Data.Blocks[i].Rails[j].RadiusV;
							double nextStartingDistance = StartingDistance + BlockInterval;
							double nextX = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[j].RailX : x;
							double nextY = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[j].RailY : y;

							for (int k = 0; k < Data.Blocks[i].Signals[j].Count; k++)
							{
								string key = Data.Blocks[i].Signals[j][k].SignalObjectKey;
								double span = Data.Blocks[i].Signals[j][k].Span;
								int type = Data.Blocks[i].Signals[j][k].Type;
								double dx = Data.Blocks[i].Signals[j][k].X;
								double dy = Data.Blocks[i].Signals[j][k].Y;
								double dz = Data.Blocks[i].Signals[j][k].Z;
								double tpos = Data.Blocks[i].Signals[j][k].TrackPosition;
								Vector3 wpos;
								Transformation Transformation;
								if (j == 0)
								{
									GetTransformation(Position, StartingDistance, turn, curveRadius, curveCant, pitch, tpos, type, span, Direction, out wpos, out Transformation);
								}
								else
								{
									GetTransformation(Position, StartingDistance, turn, curveRadius, pitch, x, y, radiusH, radiusV, curveCant, nextStartingDistance, nextX, nextY, tpos, type, span, Direction, out wpos, out Transformation);
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

										aoc.CreateObject(wpos, Transformation, new Transformation(Data.Blocks[i].Signals[j][k].Yaw, Data.Blocks[i].Signals[j][k].Pitch, Data.Blocks[i].Signals[j][k].Roll), Data.Blocks[i].Signals[j][k].SectionIndex, StartingDistance, EndingDistance, tpos, 1.0, false);
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
					double cosag = Math.Cos(ag);
					double sinag = Math.Sin(ag);
					Direction.Rotate(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection.RotatePlane(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide.RotatePlane(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection, Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide);
				}

				//Pitch
				if (Data.Blocks[i].Pitch != 0.0)
				{
					Plugin.CurrentRoute.Tracks[0].Elements[n].Pitch = Data.Blocks[i].Pitch;
				}
				else
				{
					Plugin.CurrentRoute.Tracks[0].Elements[n].Pitch = 0.0;
				}

				// curves
				double a, c, h;
				CalcTransformation(WorldTrackElement.CurveRadius, Data.Blocks[i].Pitch, BlockInterval, ref Direction, out a, out c, out h);

				if (!PreviewOnly)
				{
					for (int j = 1; j < Data.Blocks[i].Rails.Length; j++)
					{
						double x = Data.Blocks[i].Rails[j].RailX;
						double y = Data.Blocks[i].Rails[j].RailY;
						Vector3 offset = new Vector3(Direction.Y * x, y, -Direction.X * x);
						Vector3 pos = Position + offset;

						// take orientation of upcoming block into account
						Vector2 Direction2 = Direction;
						Vector3 Position2 = Position;
						Position2.X += Direction.X * c;
						Position2.Y += h;
						Position2.Z += Direction.Y * c;
						if (a != 0.0)
						{
							Direction2.Rotate(Math.Cos(-a), Math.Sin(-a));
						}

						double StartingDistance2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].StartingDistance : StartingDistance + InterpolateInterval;
						double EndingDistance2 = i < Data.Blocks.Count - 2 ? Data.Blocks[i + 2].StartingDistance : StartingDistance2 + InterpolateInterval;
						double BlockInterval2 = EndingDistance2 - StartingDistance2;
						double Turn2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Turn : 0.0;
						double CurveRadius2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].CurrentTrackState.CurveRadius : 0.0;
						double Pitch2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Pitch : 0.0;

						if (Turn2 != 0.0)
						{
							double ag = -Math.Atan(Turn2);
							double cosag = Math.Cos(ag);
							double sinag = Math.Sin(ag);
							Direction2.Rotate(cosag, sinag);
						}

						double a2, c2, h2;
						CalcTransformation(CurveRadius2, Pitch2, BlockInterval2, ref Direction2, out a2, out c2, out h2);

						double x2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[j].RailX : x;
						double y2 = i < Data.Blocks.Count - 1 ? Data.Blocks[i + 1].Rails[j].RailY : y;
						Vector3 offset2 = new Vector3(Direction2.Y * x2, y2, -Direction2.X * x2);
						Vector3 pos2 = Position2 + offset2;
						Vector3 r = new Vector3(pos2.X - pos.X, pos2.Y - pos.Y, pos2.Z - pos.Z);
						r.Normalize();

						Transformation RailTransformation = new Transformation();
						RailTransformation.Z = r;
						RailTransformation.X = new Vector3(r.Z, 0.0, -r.X);
						Normalize(ref RailTransformation.X.X, ref RailTransformation.X.Z);
						RailTransformation.Y = Vector3.Cross(RailTransformation.Z, RailTransformation.X);
						RailTransformation = new Transformation(RailTransformation, 0.0, 0.0, Math.Atan(Data.Blocks[i].Rails[j].CurveCant));

						Plugin.CurrentRoute.Tracks[j].Elements[n].StartingTrackPosition = StartingDistance;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldPosition = pos;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldDirection = RailTransformation.Z;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldSide = RailTransformation.X;
						Plugin.CurrentRoute.Tracks[j].Elements[n].WorldUp = RailTransformation.Y;
						Plugin.CurrentRoute.Tracks[j].Elements[n].CurveCant = Data.Blocks[i].Rails[j].CurveCant;
					}
				}

				// world sounds
				for (int k = 0; k < Data.Blocks[i].SoundEvents.Count; k++)
				{
					if (Data.Blocks[i].SoundEvents[k].Type == SoundType.World)
					{
						var SoundEvent = Data.Blocks[i].SoundEvents[k];
						SoundHandle buffer;
						Data.Sound3Ds.TryGetValue(SoundEvent.Key, out buffer);
						double d = SoundEvent.TrackPosition - StartingDistance;
						double dx = SoundEvent.X;
						double dy = SoundEvent.Y;
						double wa = Math.Atan2(Direction.Y, Direction.X);
						Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(0.0), Math.Sin(wa));
						w.Normalize();
						Vector3 s = new Vector3(Direction.Y, 0.0, -Direction.X);
						Vector3 u = Vector3.Cross(w, s);
						Vector3 wpos = Position + new Vector3(s.X * dx + u.X * dy + w.X * d, s.Y * dx + u.Y * dy + w.Y * d, s.Z * dx + u.Z * dy + w.Z * d);
						if (buffer != null)
						{
							Plugin.CurrentHost.PlaySound(buffer, 1.0, 1.0, wpos, null, true);
						}
					}
				}

				// finalize block
				Position.X += Direction.X * c;
				Position.Y += h;
				Position.Z += Direction.Y * c;
				if (a != 0.0)
				{
					Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
				}
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
					int k = Data.Blocks.FindLastIndex(Block => Block.StartingDistance <= p);
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
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Station " + Plugin.CurrentRoute.Stations[i].Name + " expects trains to stop but does not define stop points at track position " + Plugin.CurrentRoute.Stations[i].DefaultTrackPosition.ToString(Culture) + " in file " + FileName);
					Plugin.CurrentRoute.Stations[i].StopMode = StationStopMode.AllPass;
				}
				if (Plugin.CurrentRoute.Stations[i].Type == StationType.ChangeEnds)
				{
					if (i < Plugin.CurrentRoute.Stations.Length - 1)
					{
						if (Plugin.CurrentRoute.Stations[i + 1].StopMode != StationStopMode.AllStop)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Station " + Plugin.CurrentRoute.Stations[i].Name + " is marked as \"change ends\" but the subsequent station does not expect all trains to stop in file " + FileName);
							Plugin.CurrentRoute.Stations[i + 1].StopMode = StationStopMode.AllStop;
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Station " + Plugin.CurrentRoute.Stations[i].Name + " is marked as \"change ends\" but there is no subsequent station defined in file " + FileName);
						Plugin.CurrentRoute.Stations[i].Type = StationType.Terminal;
					}
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
