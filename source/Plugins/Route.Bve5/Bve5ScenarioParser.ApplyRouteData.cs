using System;
using OpenBveApi.Math;
using OpenBveApi.Colors;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using RouteManager2.Climate;
using RouteManager2.Events;
using OpenBveApi.World;
using OpenBveApi.Interface;
using RouteManager2;
using RouteManager2.SignalManager;
using OpenBveApi.Textures;

namespace Bve5RouteParser
{
	internal partial class Parser
	{
		private void ApplyRouteData(string FileName, System.Text.Encoding Encoding, ref RouteData Data, bool PreviewOnly)
		{
			string SignalPath, LimitPath, LimitGraphicsPath;
			StaticObject SignalPost, LimitPostStraight, LimitPostLeft, LimitPostRight, LimitPostInfinite;
			StaticObject LimitOneDigit, LimitTwoDigits, LimitThreeDigits;
			if (!PreviewOnly)
			{
				string CompatibilityFolder = Plugin.FileSystem.GetDataFolder("Compatibility");
				// load compatibility objects
				SignalPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalPath, "signal_post.csv"), Encoding, false, out SignalPost);
				
				LimitPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Limits");
				LimitGraphicsPath = OpenBveApi.Path.CombineDirectory(LimitPath, "Graphics");
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_straight.csv"), Encoding, false, out LimitPostStraight);
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_left.csv"), Encoding, false, out LimitPostLeft);
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_right.csv"), Encoding, false, out LimitPostRight);
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_infinite.csv"), Encoding, false, out LimitPostInfinite);
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_1_digit.csv"), Encoding, false, out LimitOneDigit);
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_2_digit.csv"), Encoding, false, out LimitTwoDigits);
				Plugin.CurrentHost.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_3_digit.csv"), Encoding, false, out LimitThreeDigits);
			}
			else
			{
				SignalPath = null;
				LimitPath = null;
				LimitGraphicsPath = null;
				SignalPost = null;
				LimitPostStraight = null;
				LimitPostLeft = null;
				LimitPostRight = null;
				LimitPostInfinite = null;
				LimitOneDigit = null;
				LimitTwoDigits = null;
				LimitThreeDigits = null;
			}
			// initialize
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			int LastBlock = (int)Math.Floor((Data.TrackPosition + 600.0) / Data.BlockInterval + 0.001) + 1;
			int BlocksUsed = Data.Blocks.Length;
			CreateMissingBlocks(ref Data, ref BlocksUsed, LastBlock, PreviewOnly);
			Array.Resize(ref Data.Blocks, BlocksUsed);
			CurrentRoute.AccurateObjectDisposal = true;
			CurrentRoute.BlockLength = 25;
			// interpolate height
			if (!PreviewOnly)
			{
				int z = 0;
				for (int i = 0; i < Data.Blocks.Length; i++)
				{
					if (!double.IsNaN(Data.Blocks[i].Height))
					{
						for (int j = i - 1; j >= 0; j--)
						{
							if (!double.IsNaN(Data.Blocks[j].Height))
							{
								double a = Data.Blocks[j].Height;
								double b = Data.Blocks[i].Height;
								double d = (b - a) / (double)(i - j);
								for (int k = j + 1; k < i; k++)
								{
									a += d;
									Data.Blocks[k].Height = a;
								}
								break;
							}
						}
						z = i;
					}
				}
				for (int i = z + 1; i < Data.Blocks.Length; i++)
				{
					Data.Blocks[i].Height = Data.Blocks[z].Height;
				}
			}
			// background
			if (!PreviewOnly)
			{
				CurrentRoute.CurrentBackground = new StaticBackground(null, 6, false);
				CurrentRoute.TargetBackground = CurrentRoute.CurrentBackground;
			}
			// brightness
			int CurrentBrightnessElement = -1;
			int CurrentBrightnessEvent = -1;
			float CurrentBrightnessValue = 1.0f;
			double CurrentBrightnessTrackPosition = (double)Data.FirstUsedBlock * Data.BlockInterval;
			if (!PreviewOnly)
			{
				for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
				{
					if (Data.Blocks[i].Brightness != null && Data.Blocks[i].Brightness.Length != 0)
					{
						CurrentBrightnessValue = Data.Blocks[i].Brightness[0].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].Brightness[0].Value;
						break;
					}
				}
			}
			// create objects and track
			Vector3 Position = new Vector3(0.0, 0.0, 0.0);
			Vector2 Direction = new Vector2(0.0, 1.0);
			CurrentRoute.Tracks[0] = new Track { Elements = new TrackElement[] { } };
			double CurrentSpeedLimit = double.PositiveInfinity;
			int CurrentRunIndex = 0;
			int CurrentFlangeIndex = 0;
			if (Data.FirstUsedBlock < 0) Data.FirstUsedBlock = 0;
			CurrentRoute.Tracks[0].Elements = new TrackElement[256];
			int CurrentTrackLength = 0;
			int PreviousFogElement = -1;
			int PreviousFogEvent = -1;
			Fog PreviousFog = new Fog(CurrentRoute.NoFogStart, CurrentRoute.NoFogEnd, new Color24(128, 128, 128), -Data.BlockInterval);
			Fog CurrentFog = new Fog(CurrentRoute.NoFogStart, CurrentRoute.NoFogEnd, new Color24(128, 128, 128), 0.0);
			// process blocks
			double progressFactor = Data.Blocks.Length - Data.FirstUsedBlock == 0 ? 0.5 : 0.5 / (double)(Data.Blocks.Length - Data.FirstUsedBlock);
			for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
			{
				Plugin.CurrentProgress = 0.6667 + (double)(i - Data.FirstUsedBlock) * progressFactor;
				if ((i & 15) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel) return;
				}
				double StartingDistance = (double)i * Data.BlockInterval;
				double EndingDistance = StartingDistance + Data.BlockInterval;
				// normalize
				Direction.Normalize();
				// track
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].Cycle.Length == 1 && Data.Blocks[i].Cycle[0] == -1)
					{
						if (Data.Structure.Cycle.Length == 0 || Data.Structure.Cycle[0] == null)
						{
							Data.Blocks[i].Cycle = new int[] { 0 };
						}
						else
						{
							Data.Blocks[i].Cycle = Data.Structure.Cycle[0];
						}
					}
				}
				TrackElement WorldTrackElement = Data.Blocks[i].CurrentTrackState;
				int n = CurrentTrackLength;
				if (n >= CurrentRoute.Tracks[0].Elements.Length)
				{
					Array.Resize(ref CurrentRoute.Tracks[0].Elements, CurrentRoute.Tracks[0].Elements.Length << 1);
				}
				CurrentTrackLength++;
				CurrentRoute.Tracks[0].Elements[n] = WorldTrackElement;
				CurrentRoute.Tracks[0].Elements[n].WorldPosition = Position;
				CurrentRoute.Tracks[0].Elements[n].WorldDirection = Vector3.GetVector3(Direction, Data.Blocks[i].Pitch);
				CurrentRoute.Tracks[0].Elements[n].WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X);
				CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(CurrentRoute.Tracks[0].Elements[n].WorldDirection, CurrentRoute.Tracks[0].Elements[n].WorldSide);
				CurrentRoute.Tracks[0].Elements[n].StartingTrackPosition = StartingDistance;
				CurrentRoute.Tracks[0].Elements[n].Events = new GeneralEvent[] { };
				CurrentRoute.Tracks[0].Elements[n].AdhesionMultiplier = Data.Blocks[i].AdhesionMultiplier;
				CurrentRoute.Tracks[0].Elements[n].CsvRwAccuracyLevel = Data.Blocks[i].Accuracy;
				// background
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].Background >= 0)
					{
						int typ;
						if (i == Data.FirstUsedBlock)
						{
							typ = Data.Blocks[i].Background;
						}
						else
						{
							typ = Data.Backgrounds.Length > 0 ? 0 : -1;
							for (int j = i - 1; j >= Data.FirstUsedBlock; j--)
							{
								if (Data.Blocks[j].Background >= 0)
								{
									typ = Data.Blocks[j].Background;
									break;
								}
							}
						}
						if (typ >= 0 & typ < Data.Backgrounds.Length)
						{
							int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
							Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
							CurrentRoute.Tracks[0].Elements[n].Events[m] = new BackgroundChangeEvent(CurrentRoute, 0.0, Data.Backgrounds[typ].Handle, Data.Backgrounds[Data.Blocks[i].Background].Handle);
						}
					}
				}
				// brightness
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++)
					{
						int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
						Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
						double d = Data.Blocks[i].Brightness[j].TrackPosition - StartingDistance;
						CurrentRoute.Tracks[0].Elements[n].Events[m] = new BrightnessChangeEvent(d, Data.Blocks[i].Brightness[j].Value, CurrentBrightnessValue, Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition);
						if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
						{
							BrightnessChangeEvent bce = (BrightnessChangeEvent)CurrentRoute.Tracks[0].Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
							bce.NextBrightness = Data.Blocks[i].Brightness[j].Value;
							bce.NextDistance = Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition;
						}
						CurrentBrightnessElement = n;
						CurrentBrightnessEvent = m;
						CurrentBrightnessValue = Data.Blocks[i].Brightness[j].Value;
						CurrentBrightnessTrackPosition = Data.Blocks[i].Brightness[j].TrackPosition;
					}
				}
				// fog
				if (!PreviewOnly)
				{
					if (Data.FogTransitionMode)
					{
						if (Data.Blocks[i].FogDefined)
						{
							Data.Blocks[i].Fog.TrackPosition = StartingDistance;
							int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
							Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
							CurrentRoute.Tracks[0].Elements[n].Events[m] = new FogChangeEvent(CurrentRoute, 0.0, PreviousFog, Data.Blocks[i].Fog, Data.Blocks[i].Fog);
							if (PreviousFogElement >= 0 & PreviousFogEvent >= 0)
							{
								FogChangeEvent e = (FogChangeEvent)CurrentRoute.Tracks[0].Elements[PreviousFogElement].Events[PreviousFogEvent];
								e.NextFog = Data.Blocks[i].Fog;
							}
							else
							{
								CurrentRoute.PreviousFog = PreviousFog;
								CurrentRoute.CurrentFog = PreviousFog;
								CurrentRoute.NextFog = Data.Blocks[i].Fog;
							}
							PreviousFog = Data.Blocks[i].Fog;
							PreviousFogElement = n;
							PreviousFogEvent = m;
						}
					}
					else
					{
						Data.Blocks[i].Fog.TrackPosition = StartingDistance + Data.BlockInterval;
						int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
						Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
						CurrentRoute.Tracks[0].Elements[n].Events[m] = new FogChangeEvent(CurrentRoute, 0.0, PreviousFog, CurrentFog, Data.Blocks[i].Fog);
						PreviousFog = CurrentFog;
						CurrentFog = Data.Blocks[i].Fog;
					}
				}
				// rail sounds
				if (!PreviewOnly)
				{
					for (int k = 0; k < Data.Blocks[i].RunSounds.Length; k++)
					{
						//Get & check our variables
						int r = Data.Blocks[i].RunSounds[k].RunSoundIndex;
						int f = Data.Blocks[i].RunSounds[k].FlangeSoundIndex;
						if (r != CurrentRunIndex || f != CurrentFlangeIndex)
						{
							//If either of these differ, we first need to resize the array for the current block
							int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
							Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
							double d = Data.Blocks[i].RunSounds[k].TrackPosition - StartingDistance;
							if (d > 0.0)
							{
								d = 0.0;
							}
							//Add event
							CurrentRoute.Tracks[0].Elements[n].Events[m] = new RailSoundsChangeEvent(d, CurrentRunIndex, CurrentFlangeIndex, r, f);
							CurrentRunIndex = r;
							CurrentFlangeIndex = f;
						}
					}
				}
				// point sound
				if (!PreviewOnly)
				{
					if (i < Data.Blocks.Length - 1)
					{
						if (Data.Blocks[i].JointNoise)
						{
							int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
							Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
							CurrentRoute.Tracks[0].Elements[n].Events[m] = new SoundEvent(0.0, null, false, false, true, new Vector3(0.0, 0.0, 0.0), 12.5, Plugin.CurrentHost);
						}
					}
				}
				// station
				if (Data.Blocks[i].Station >= 0)
				{
					// station
					int s = Data.Blocks[i].Station;
					int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
					Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
					CurrentRoute.Tracks[0].Elements[n].Events[m] = new StationStartEvent(0.0, s);
					double dx, dy = 3.0;
					if (CurrentRoute.Stations[s].OpenLeftDoors & !CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!CurrentRoute.Stations[s].OpenLeftDoors & CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					CurrentRoute.Stations[s].SoundOrigin.X = Position.X + dx * CurrentRoute.Tracks[0].Elements[n].WorldSide.X + dy * CurrentRoute.Tracks[0].Elements[n].WorldUp.X;
					CurrentRoute.Stations[s].SoundOrigin.Y = Position.Y + dx * CurrentRoute.Tracks[0].Elements[n].WorldSide.Y + dy * CurrentRoute.Tracks[0].Elements[n].WorldUp.Y;
					CurrentRoute.Stations[s].SoundOrigin.Z = Position.Z + dx * CurrentRoute.Tracks[0].Elements[n].WorldSide.Z + dy * CurrentRoute.Tracks[0].Elements[n].WorldUp.Z;
					// passalarm
					if (!PreviewOnly)
					{
						if (Data.Blocks[i].StationPassAlarm)
						{
							int b = i - 6;
							if (b >= 0)
							{
								int j = b - Data.FirstUsedBlock;
								if (j >= 0)
								{
									m = CurrentRoute.Tracks[0].Elements[j].Events.Length;
									Array.Resize(ref CurrentRoute.Tracks[0].Elements[j].Events, m + 1);
									CurrentRoute.Tracks[0].Elements[j].Events[m] = new StationPassAlarmEvent(0.0);
								}
							}
						}
					}
				}
				// stop
				for (int j = 0; j < Data.Blocks[i].Stop.Length; j++)
				{
					int s = Data.Blocks[i].Stop[j].Station;
					int t = CurrentRoute.Stations[s].Stops.Length;
					Array.Resize(ref CurrentRoute.Stations[s].Stops, t + 1);
					CurrentRoute.Stations[s].Stops[t].TrackPosition = Data.Blocks[i].Stop[j].TrackPosition;
					CurrentRoute.Stations[s].Stops[t].ForwardTolerance = Data.Blocks[i].Stop[j].ForwardTolerance;
					CurrentRoute.Stations[s].Stops[t].BackwardTolerance = Data.Blocks[i].Stop[j].BackwardTolerance;
					CurrentRoute.Stations[s].Stops[t].Cars = Data.Blocks[i].Stop[j].Cars;
					double dx, dy = 2.0;
					if (CurrentRoute.Stations[s].OpenLeftDoors & !CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!CurrentRoute.Stations[s].OpenLeftDoors & CurrentRoute.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					CurrentRoute.Stations[s].SoundOrigin = Position + dx * CurrentRoute.Tracks[0].Elements[n].WorldSide + dy * CurrentRoute.Tracks[0].Elements[n].WorldUp;
				}
				// limit
				for (int j = 0; j < Data.Blocks[i].Limit.Length; j++)
				{
					int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
					Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
					double d = Data.Blocks[i].Limit[j].TrackPosition - StartingDistance;
					CurrentRoute.Tracks[0].Elements[n].Events[m] = new LimitChangeEvent(d, CurrentSpeedLimit, Data.Blocks[i].Limit[j].Speed);
					CurrentSpeedLimit = Data.Blocks[i].Limit[j].Speed;
				}
				// turn
				if (Data.Blocks[i].Turn != 0.0)
				{
					double ag = -Math.Atan(Data.Blocks[i].Turn);
					double cosag = Math.Cos(ag);
					double sinag = Math.Sin(ag);
					Direction.Rotate(cosag, sinag);
					CurrentRoute.Tracks[0].Elements[n].WorldDirection.RotatePlane(cosag, sinag);
					CurrentRoute.Tracks[0].Elements[n].WorldSide.RotatePlane(cosag, sinag);
					CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(CurrentRoute.Tracks[0].Elements[n].WorldDirection, CurrentRoute.Tracks[0].Elements[n].WorldSide);
				}
				//Pitch
				if (Data.Blocks[i].Pitch != 0.0)
				{
					CurrentRoute.Tracks[0].Elements[n].Pitch = Data.Blocks[i].Pitch;
				}
				else
				{
					CurrentRoute.Tracks[0].Elements[n].Pitch = 0.0;
				}
				// curves
				double a = 0.0;
				double c = Data.BlockInterval;
				double h = 0.0;
				if (WorldTrackElement.CurveRadius != 0.0 & Data.Blocks[i].Pitch != 0.0)
				{
					double d = Data.BlockInterval;
					double p = Data.Blocks[i].Pitch;
					double r = WorldTrackElement.CurveRadius;
					double s = d / Math.Sqrt(1.0 + p * p);
					h = s * p;
					double b = s / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
				}
				else if (WorldTrackElement.CurveRadius != 0.0)
				{
					double d = Data.BlockInterval;
					double r = WorldTrackElement.CurveRadius;
					double b = d / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
				}
				else if (Data.Blocks[i].Pitch != 0.0)
				{
					double p = Data.Blocks[i].Pitch;
					double d = Data.BlockInterval;
					c = d / Math.Sqrt(1.0 + p * p);
					h = c * p;
				}
				double TrackYaw = Math.Atan2(Direction.X, Direction.Y);
				double TrackPitch = Math.Atan(Data.Blocks[i].Pitch);
				Transformation GroundTransformation = new Transformation(TrackYaw, 0.0, 0.0);
				Transformation TrackGroundTransformation = new Transformation(TrackYaw, 0.0, 0.0);
				Transformation TrackTransformation = new Transformation(TrackYaw, TrackPitch, 0.0);
				Transformation NullTransformation = new Transformation(0.0, 0.0, 0.0);
				//Create final repeater objects and add to arrays
				if (!PreviewOnly)
				{
					if (Data.Blocks[i].Repeaters == null || Data.Blocks[i].Repeaters.Length == 0)
					{
						continue;
					}
					for (int j = 0; j < Data.Blocks[i].Repeaters.Length; j++)
					{
						if (Data.Blocks[i].Repeaters[j].Name == null)
						{
							continue;
						}
						StaticObject curvedObject = new StaticObject(Plugin.CurrentHost);
									
						int numSegments = (int)(Data.BlockInterval / Data.Blocks[i].Repeaters[j].RepetitionInterval);
						for (int k = 0; k < numSegments; k++)
						{
							StaticObject Segment = (StaticObject)Data.Structure.Objects[Data.Blocks[i].Repeaters[j].StructureTypes[0]].Clone();
							Segment.ApplyTranslation(0,0, k * Data.Blocks[i].Repeaters[j].RepetitionInterval);
							Segment.ApplyCurve(Data.Blocks[i].CurrentTrackState.CurveRadius);
							curvedObject.JoinObjects(Segment);
						}
						Array.Resize(ref Data.Structure.Objects, Data.Structure.Objects.Length + 1);
						Data.Structure.Objects[Data.Structure.Objects.Length - 1] = curvedObject;
						int idx = Data.Blocks[i].Repeaters[j].RailIndex;
						if (idx >= Data.Blocks[i].RailFreeObj.Length)
						{
							Array.Resize(ref Data.Blocks[i].RailFreeObj, idx + 1);
						}
						int ol = 0;
						if (Data.Blocks[i].RailFreeObj[idx] == null)
						{
							Data.Blocks[i].RailFreeObj[idx] = new Object[1];
							ol = 0;
						}
						else
						{
							ol = Data.Blocks[i].RailFreeObj[idx].Length;
							Array.Resize(ref Data.Blocks[i].RailFreeObj[idx], ol + 1);
						}

						if (Data.Blocks[i].CurrentTrackState.CurveRadius != 0)
						{
							//Turn object back on itself for use as railtype
							double rot = 0.5 * (180 * Data.BlockInterval / (Math.PI * Data.Blocks[i].CurrentTrackState.CurveRadius));
							curvedObject.ApplyRotation(new Vector3(0,1,0), -rot.ToRadians());
						}
						Data.Blocks[i].RailFreeObj[idx][ol] = new Object(Data.Blocks[i].Repeaters[j].TrackPosition, "",Data.Structure.Objects.Length - 1, Data.Blocks[i].Repeaters[j].Position, 0,0,0, Data.Blocks[i].Repeaters[j].Type);
					}
				}
				// ground-aligned free objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].GroundFreeObj.Length; j++)
					{
						int sttype = Data.Blocks[i].GroundFreeObj[j].Type;
						double d = Data.Blocks[i].GroundFreeObj[j].TrackPosition - StartingDistance + Data.Blocks[i].GroundFreeObj[j].Position.Z;
						double dx = Data.Blocks[i].GroundFreeObj[j].Position.X;
						double dy = Data.Blocks[i].GroundFreeObj[j].Position.Y;
						Vector3 wpos = Position + new Vector3(Direction.X * d + Direction.Y * dx, dy - Data.Blocks[i].Height, Direction.Y * d - Direction.X * dx);
						double tpos = Data.Blocks[i].GroundFreeObj[j].TrackPosition;
						if (sttype > Data.Structure.Objects.Length || Data.Structure.Objects[sttype] == null)
						{
							continue;
						}
						Data.Structure.Objects[sttype].CreateObject(wpos, GroundTransformation, new Transformation(Data.Blocks[i].GroundFreeObj[j].Yaw, Data.Blocks[i].GroundFreeObj[j].Pitch, Data.Blocks[i].GroundFreeObj[j].Roll), StartingDistance, EndingDistance, tpos);
					}
				}
				// rail-aligned objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Rail.Length; j++)
					{
						if (j > 0 && !Data.Blocks[i].Rail[j].RailStart) continue;
						// rail
						Vector3 pos;
						Transformation RailTransformation;
						Transformation RailNullTransformation;
						double planar, updown;
						if (j == 0)
						{
							// rail 0
							planar = 0.0;
							updown = 0.0;
							RailTransformation = new Transformation(TrackTransformation, planar, updown, 0.0);
							RailNullTransformation = new Transformation(RailTransformation);
							RailNullTransformation.Y = new Vector3(0, 1.0, 0);
							pos = Position;
						}
						else
						{
							// rails 1-infinity
							double x = Data.Blocks[i].Rail[j].RailStartX;
							double y = Data.Blocks[i].Rail[j].RailStartY;
							Vector3 offset = new Vector3(Direction.Y * x, y, -Direction.X * x);
							pos = Position + offset;
							double dh;
							if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j)
							{
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
								if (Data.Blocks[i + 1].Turn != 0.0)
								{
									double ag = -Math.Atan(Data.Blocks[i + 1].Turn);
									double cosag = Math.Cos(ag);
									double sinag = Math.Sin(ag);
									Direction2.Rotate(cosag, sinag);
								}
								double a2 = 0.0;
								double c2 = Data.BlockInterval;
								double h2 = 0.0;
								if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0 & Data.Blocks[i + 1].Pitch != 0.0)
								{
									double d2 = Data.BlockInterval;
									double p2 = Data.Blocks[i + 1].Pitch;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double s2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									h2 = s2 * p2;
									double b2 = s2 / Math.Abs(r2);
									c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									Direction2.Rotate(Math.Cos(-a2), Math.Sin(-a2));
								}
								else if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0)
								{
									double d2 = Data.BlockInterval;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double b2 = d2 / Math.Abs(r2);
									c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									Direction2.Rotate(Math.Cos(-a2), Math.Sin(-a2));
								}
								else if (Data.Blocks[i + 1].Pitch != 0.0)
								{
									double p2 = Data.Blocks[i + 1].Pitch;
									double d2 = Data.BlockInterval;
									c2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									h2 = c2 * p2;
								}
								double TrackYaw2 = Math.Atan2(Direction2.X, Direction2.Y);
								double TrackPitch2 = Math.Atan(Data.Blocks[i + 1].Pitch);
								Transformation GroundTransformation2 = new Transformation(TrackYaw2, 0.0, 0.0);
								Transformation TrackTransformation2 = new Transformation(TrackYaw2, TrackPitch2, 0.0);
								RailTransformation = new Transformation(TrackTransformation2, 0.0, 0.0, 0.0);
								TrackGroundTransformation = new Transformation(GroundTransformation2, 0.0, 0.0, 0.0);
								double x2 = Data.Blocks[i + 1].Rail[j].RailEndX;
								double y2 = Data.Blocks[i + 1].Rail[j].RailEndY;
								Vector3 offset2 = new Vector3(Direction2.Y * x2, y2, -Direction2.X * x2);

								/*
								 * Create transform for rails
								 */
								Vector3 pos2 = Position2 + offset2;
								Vector3 r = new Vector3(pos2 - pos);
								RailTransformation.Z = r;
								RailTransformation.X = new Vector3(r.Z, 0.0, -r.X);
								Normalize(ref RailTransformation.X.X, ref RailTransformation.X.Z);
								RailTransformation.Y = Vector3.Cross(RailTransformation.Z, RailTransformation.X);
								/*
								 * Create transform for rail attached grounds
								 */
								Vector3 offset3 = new Vector3(Direction2.Y * x2, 0.0, -Direction2.X * x2);
								Vector3 pos3 = Position2 + offset3;
								Vector3 rr = new Vector3(pos3);
								rr.Normalize();
								TrackGroundTransformation.Z = new Vector3(rr);
								TrackGroundTransformation.X = new Vector3(rr.Z, 0.0, -rr.X);
								Normalize(ref TrackGroundTransformation.X.X, ref TrackGroundTransformation.X.Z);
								TrackGroundTransformation.Y = Vector3.Cross(TrackGroundTransformation.Z, TrackGroundTransformation.X);


								double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
								double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
								planar = Math.Atan(dx / c);
								dh = dy / c;
								updown = Math.Atan(dh);
								RailNullTransformation = new Transformation(RailTransformation);
								RailNullTransformation.Y = new Vector3(0, 1.0, 0);

							}
							else
							{
								planar = 0.0;
								dh = 0.0;
								updown = 0.0;
								RailTransformation = new Transformation(TrackTransformation, 0.0, 0.0, 0.0);
								TrackGroundTransformation = new Transformation(GroundTransformation, 0.0, 0.0, 0.0);
								RailNullTransformation = new Transformation(RailTransformation);
								RailNullTransformation.Y = new Vector3(0, 1.0, 0);
							}
						}
						// cracks
						for (int k = 0; k < Data.Blocks[i].Crack.Length; k++)
						{
							if (Data.Blocks[i].Crack[k].PrimaryRail == j)
							{
								int p = Data.Blocks[i].Crack[k].PrimaryRail;
								double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
								double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
								int s = Data.Blocks[i].Crack[k].SecondaryRail;
								if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
								}
								else
								{
									double sx0 = Data.Blocks[i].Rail[s].RailStartX;
									double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
									double d0 = sx0 - px0;
									double d1 = sx1 - px1;
									if (d0 < 0.0)
									{
										if (Data.Blocks[i].Crack[k].Type >= Data.Structure.Objects.Length || Data.Structure.Objects[Data.Blocks[i].Crack[k].Type] == null)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex references a CrackL not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											StaticObject Crack = (StaticObject)Data.Structure.Objects[Data.Blocks[i].Crack[k].Type].Transform(d0, d1);
											Plugin.CurrentHost.CreateStaticObject(Crack, pos, RailTransformation, Transformation.NullTransformation, 0.0, StartingDistance, EndingDistance, StartingDistance, 1.0);
										}
									}
									else if (d0 > 0.0)
									{
										if (Data.Blocks[i].Crack[k].Type >= Data.Structure.Objects.Length || Data.Structure.Objects[Data.Blocks[i].Crack[k].Type] == null)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex references a CrackR not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											StaticObject Crack = (StaticObject)Data.Structure.Objects[Data.Blocks[i].Crack[k].Type].Transform(d0, d1);
											Plugin.CurrentHost.CreateStaticObject(Crack, pos, RailTransformation, Transformation.NullTransformation, 0.0, StartingDistance, EndingDistance, StartingDistance, 1.0);
										}
									}
								}
							}
						}
						// free objects
						if (Data.Blocks[i].RailFreeObj.Length > j && Data.Blocks[i].RailFreeObj[j] != null)
						{
							for (int k = 0; k < Data.Blocks[i].RailFreeObj[j].Length; k++)
							{
								int sttype = Data.Blocks[i].RailFreeObj[j][k].Type;
								if (sttype != -1)
								{
									double dx = Data.Blocks[i].RailFreeObj[j][k].Position.X;
									double dy = Data.Blocks[i].RailFreeObj[j][k].Position.Y;
									double dz = Data.Blocks[i].RailFreeObj[j][k].TrackPosition - StartingDistance + Data.Blocks[i].RailFreeObj[j][k].Position.Z;

									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].RailFreeObj[j][k].TrackPosition;
									if (sttype > Data.Structure.Objects.Length || Data.Structure.Objects[sttype] == null)
									{
										continue;
									}
									switch (Data.Blocks[i].RailFreeObj[j][k].BaseTransformation)
									{
										case RailTransformationTypes.Flat:
											//As expected, this is used for repeaters with a type value of zero
											//However, to get correct appearance in-game, they actually seem to require the rail transform....

											//TODO: Check how the transform is created; I presume we require a new one which doesn't take into account
											//height or cant, but does X & Y positions of the rail???
											//Sounds remarkably like the ground transform, but this doesn't appear to work....
											Data.Structure.Objects[sttype].CreateObject(wpos, RailNullTransformation,
												new Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch,
													Data.Blocks[i].RailFreeObj[j][k].Roll), -1, StartingDistance, EndingDistance, tpos, 1.0, false);
											break;
										case RailTransformationTypes.FollowsPitch:
											Data.Structure.Objects[sttype].CreateObject(wpos, RailTransformation,
												new Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch,
													Data.Blocks[i].RailFreeObj[j][k].Roll), -1, StartingDistance, EndingDistance, tpos, 1.0, false);
											break;
										case RailTransformationTypes.FollowsCant:
											Data.Structure.Objects[sttype].CreateObject(wpos, TrackGroundTransformation,
												new Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch,
													Data.Blocks[i].RailFreeObj[j][k].Roll), -1, StartingDistance, EndingDistance, tpos, 1.0, false);
											break;
										case RailTransformationTypes.FollowsBoth:
											Data.Structure.Objects[sttype].CreateObject(wpos, RailTransformation,
												new Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch,
													Data.Blocks[i].RailFreeObj[j][k].Roll), -1, StartingDistance, EndingDistance, tpos, 1.0, false);
											break;
									}
								}
							}
						}
						// sections/signals/transponders
						if (j == 0)
						{
							// signals
							for (int k = 0; k < Data.Blocks[i].Signal.Length; k++)
							{
								Data.Blocks[i].Signal[k].Create(new Vector3(pos), RailTransformation, StartingDistance, EndingDistance, 0.27 + 0.75 * GetBrightness(ref Data, Data.Blocks[i].Signal[k].TrackPosition), SignalPost);
							}
							// sections
							for (int k = 0; k < Data.Blocks[i].Section.Length; k++)
							{
								Data.Blocks[i].Section[k].Create(CurrentRoute, Data.Blocks, i, n, Data.SignalSpeeds, StartingDistance, Data.BlockInterval);
							}


						}
						// limit
						if (j == 0)
						{
							for (int k = 0; k < Data.Blocks[i].Limit.Length; k++)
							{
								if (Data.Blocks[i].Limit[k].Direction != 0)
								{
									double dx = 2.2 * (double)Data.Blocks[i].Limit[k].Direction;
									double dz = Data.Blocks[i].Limit[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Limit[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									if (Data.Blocks[i].Limit[k].Speed <= 0.0 | Data.Blocks[i].Limit[k].Speed >= 1000.0)
									{
										Plugin.CurrentHost.CreateStaticObject(LimitPostInfinite, wpos, RailTransformation, NullTransformation, 0.0, StartingDistance, EndingDistance, tpos, b);
									}
									else
									{
										if (Data.Blocks[i].Limit[k].Cource < 0)
										{
											Plugin.CurrentHost.CreateStaticObject(LimitPostLeft, wpos, RailTransformation, NullTransformation, 0.0, StartingDistance, EndingDistance, tpos, b);
										}
										else if (Data.Blocks[i].Limit[k].Cource > 0)
										{
											Plugin.CurrentHost.CreateStaticObject(LimitPostRight, wpos, RailTransformation, NullTransformation, 0.0, StartingDistance, EndingDistance, tpos, b);
										}
										else
										{
											Plugin.CurrentHost.CreateStaticObject(LimitPostStraight, wpos, RailTransformation, NullTransformation, 0.0, StartingDistance, EndingDistance,  tpos, b);
										}
										double lim = Data.Blocks[i].Limit[k].Speed / Data.UnitOfSpeed;
										if (lim < 10.0)
										{
											int d0 = (int)Math.Round(lim);
											StaticObject o = (StaticObject) LimitOneDigit.Clone();
											if (o.Mesh.Materials.Length >= 1)
											{
												Plugin.CurrentHost.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), new TextureParameters(null, null), out o.Mesh.Materials[0].DaytimeTexture);
											}
											o.CreateObject(wpos, RailTransformation, Transformation.NullTransformation, -1, StartingDistance, EndingDistance, tpos, b, false);
										}
										else if (lim < 100.0)
										{
											int d1 = (int)Math.Round(lim);
											int d0 = d1 % 10;
											d1 /= 10;
											StaticObject o = (StaticObject) LimitTwoDigits.Clone();
											if (o.Mesh.Materials.Length >= 1)
											{
												Plugin.CurrentHost.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), new TextureParameters(null, null), out o.Mesh.Materials[0].DaytimeTexture);
											}
											if (o.Mesh.Materials.Length >= 2)
											{
												Plugin.CurrentHost.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), new TextureParameters(null, null), out o.Mesh.Materials[1].DaytimeTexture);
											}
											o.CreateObject(wpos, RailTransformation, Transformation.NullTransformation, -1,  StartingDistance, EndingDistance,  tpos, b, false);
										}
										else
										{
											int d2 = (int)Math.Round(lim);
											int d0 = d2 % 10;
											int d1 = (d2 / 10) % 10;
											d2 /= 100;
											StaticObject o = (StaticObject) LimitThreeDigits.Clone();
											if (o.Mesh.Materials.Length >= 1)
											{
												Plugin.CurrentHost.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d2 + ".png"), new TextureParameters(null, null), out o.Mesh.Materials[0].DaytimeTexture);
											}
											if (o.Mesh.Materials.Length >= 2)
											{
												Plugin.CurrentHost.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), new TextureParameters(null, null), out o.Mesh.Materials[1].DaytimeTexture);
											}
											if (o.Mesh.Materials.Length >= 3)
											{
												Plugin.CurrentHost.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), new TextureParameters(null, null), out o.Mesh.Materials[2].DaytimeTexture);
											}
											o.CreateObject(wpos, RailTransformation, Transformation.NullTransformation, -1, StartingDistance, EndingDistance, tpos, b, false);
										}
									}
								}
							}
						}
						/*
						// stop
						if (j == 0)
						{
							for (int k = 0; k < Data.Blocks[i].Stop.Length; k++)
							{
								if (Data.Blocks[i].Stop[k].Direction != 0)
								{
									double dx = 1.8 * (double)Data.Blocks[i].Stop[k].Direction;
									double dz = Data.Blocks[i].Stop[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Stop[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									ObjectManager.CreateStaticObject(StopPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
								}
							}
						}
						 */
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
			/*
			// orphaned transponders
			if (!PreviewOnly)
			{
				for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
				{
					for (int j = 0; j < Data.Blocks[i].Transponder.Length; j++)
					{
						if (Data.Blocks[i].Transponder[j].Type != -1)
						{
							int n = i - Data.FirstUsedBlock;
							int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
							Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
							double d = Data.Blocks[i].Transponder[j].TrackPosition - CurrentRoute.Tracks[0].Elements[n].StartingTrackPosition;
							int s = Data.Blocks[i].Transponder[j].Section;
							if (s >= 0) s = -1;
							CurrentRoute.Tracks[0].Elements[n].Events[m] = new TransponderEvent(d, Data.Blocks[i].Transponder[j].Type, Data.Blocks[i].Transponder[j].Data, s, Data.Blocks[i].Transponder[j].ClipToFirstRedSection);
							Data.Blocks[i].Transponder[j].Type = -1;
						}
					}
				}
			}
			 */
			// insert station end events
			for (int i = 0; i < CurrentRoute.Stations.Length; i++)
			{
				int j = CurrentRoute.Stations[i].Stops.Length - 1;
				if (j >= 0)
				{
					double p = CurrentRoute.Stations[i].Stops[j].TrackPosition + CurrentRoute.Stations[i].Stops[j].ForwardTolerance + Data.BlockInterval;
					int k = (int)Math.Floor(p / (double)Data.BlockInterval) - Data.FirstUsedBlock;
					if (k >= 0 & k < Data.Blocks.Length)
					{
						double d = p - (double)(k + Data.FirstUsedBlock) * (double)Data.BlockInterval;
						int m = CurrentRoute.Tracks[0].Elements[k].Events.Length;
						Array.Resize(ref CurrentRoute.Tracks[0].Elements[k].Events, m + 1);
						CurrentRoute.Tracks[0].Elements[k].Events[m] = new StationEndEvent(d, i, CurrentRoute, Plugin.CurrentHost);
					}
				}
			}
			// create default point of interests
			if (CurrentRoute.PointsOfInterest.Length == 0)
			{
				CurrentRoute.PointsOfInterest = new PointOfInterest[CurrentRoute.Stations.Length];
				int n = 0;
				for (int i = 0; i < CurrentRoute.Stations.Length; i++)
				{
					if (CurrentRoute.Stations[i].Stops.Length != 0)
					{
						CurrentRoute.PointsOfInterest[n].Text = CurrentRoute.Stations[i].Name;
						CurrentRoute.PointsOfInterest[n].TrackPosition = CurrentRoute.Stations[i].Stops[0].TrackPosition;
						CurrentRoute.PointsOfInterest[n].TrackOffset = new Vector3(0.0, 2.8, 0.0);
						if (CurrentRoute.Stations[i].OpenLeftDoors & !CurrentRoute.Stations[i].OpenRightDoors)
						{
							CurrentRoute.PointsOfInterest[n].TrackOffset.X = -2.5;
						}
						else if (!CurrentRoute.Stations[i].OpenLeftDoors & CurrentRoute.Stations[i].OpenRightDoors)
						{
							CurrentRoute.PointsOfInterest[n].TrackOffset.X = 2.5;
						}
						n++;
					}
				}
				Array.Resize(ref CurrentRoute.PointsOfInterest, n);
			}
			// convert block-based cant into point-based cant
			for (int i = CurrentTrackLength - 1; i >= 1; i--)
			{
				if (CurrentRoute.Tracks[0].Elements[i].CurveCant == 0.0)
				{
					CurrentRoute.Tracks[0].Elements[i].CurveCant = CurrentRoute.Tracks[0].Elements[i - 1].CurveCant;
				}
				else if (CurrentRoute.Tracks[0].Elements[i - 1].CurveCant != 0.0)
				{
					if (Math.Sign(CurrentRoute.Tracks[0].Elements[i - 1].CurveCant) == Math.Sign(CurrentRoute.Tracks[0].Elements[i].CurveCant))
					{
						if (Math.Abs(CurrentRoute.Tracks[0].Elements[i - 1].CurveCant) > Math.Abs(CurrentRoute.Tracks[0].Elements[i].CurveCant))
						{
							CurrentRoute.Tracks[0].Elements[i].CurveCant = CurrentRoute.Tracks[0].Elements[i - 1].CurveCant;
						}
					}
					else
					{
						CurrentRoute.Tracks[0].Elements[i].CurveCant = 0.5 * (CurrentRoute.Tracks[0].Elements[i].CurveCant + CurrentRoute.Tracks[0].Elements[i - 1].CurveCant);
					}
				}
			}
			// finalize
			Array.Resize(ref CurrentRoute.Tracks[0].Elements, CurrentTrackLength);
			for (int i = 0; i < CurrentRoute.Stations.Length; i++)
			{
				if (CurrentRoute.Stations[i].Stops.Length == 0 & CurrentRoute.Stations[i].StopMode != StationStopMode.AllPass)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Station " + CurrentRoute.Stations[i].Name + " expects trains to stop but does not define stop points at track position " + CurrentRoute.Stations[i].DefaultTrackPosition.ToString(Culture) + " in file " + FileName);
					CurrentRoute.Stations[i].StopMode = StationStopMode.AllPass;
				}
				if (CurrentRoute.Stations[i].Type == StationType.ChangeEnds)
				{
					if (i < CurrentRoute.Stations.Length - 1)
					{
						if (CurrentRoute.Stations[i + 1].StopMode != StationStopMode.AllStop)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Station " + CurrentRoute.Stations[i].Name + " is marked as \"change ends\" but the subsequent station does not expect all trains to stop in file " + FileName);
							CurrentRoute.Stations[i + 1].StopMode = StationStopMode.AllStop;
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Station " + CurrentRoute.Stations[i].Name + " is marked as \"change ends\" but there is no subsequent station defined in file " + FileName);
						CurrentRoute.Stations[i].Type = StationType.Terminal;
					}
				}
			}
			if (CurrentRoute.Stations.Length != 0)
			{
				CurrentRoute.Stations[CurrentRoute.Stations.Length - 1].Type = StationType.Terminal;
			}
			if (CurrentRoute.Tracks[0].Elements.Length != 0)
			{
				int n = CurrentRoute.Tracks[0].Elements.Length - 1;
				int m = CurrentRoute.Tracks[0].Elements[n].Events.Length;
				Array.Resize(ref CurrentRoute.Tracks[0].Elements[n].Events, m + 1);
				CurrentRoute.Tracks[0].Elements[n].Events[m] = new TrackEndEvent(Plugin.CurrentHost, Data.BlockInterval);
			}
			// insert compatibility beacons
			if (!PreviewOnly)
			{
				List<TransponderEvent> transponders = new List<TransponderEvent>();
				bool atc = false;
				for (int i = 0; i < CurrentRoute.Tracks[0].Elements.Length; i++)
				{
					for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Length; j++)
					{
						if (!atc)
						{
							if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent)
							{
								StationStartEvent station = (StationStartEvent)CurrentRoute.Tracks[0].Elements[i].Events[j];
								if (CurrentRoute.Stations[station.StationIndex].SafetySystem == SafetySystem.Atc)
								{
									Array.Resize(ref CurrentRoute.Tracks[0].Elements[i].Events, CurrentRoute.Tracks[0].Elements[i].Events.Length + 2);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 2] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 0, 0, false);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 1] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 1, 0, false);
									atc = true;
								}
							}
						}
						else
						{
							if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent)
							{
								StationStartEvent station = (StationStartEvent)CurrentRoute.Tracks[0].Elements[i].Events[j];
								if (CurrentRoute.Stations[station.StationIndex].SafetySystem == SafetySystem.Ats)
								{
									Array.Resize(ref CurrentRoute.Tracks[0].Elements[i].Events, CurrentRoute.Tracks[0].Elements[i].Events.Length + 2);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 2] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 2, 0, false);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 1] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 3, 0, false);
								}
							}
							else if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationEndEvent)
							{
								StationEndEvent station = (StationEndEvent)CurrentRoute.Tracks[0].Elements[i].Events[j];
								if (CurrentRoute.Stations[station.StationIndex].SafetySystem == SafetySystem.Atc)
								{
									Array.Resize(ref CurrentRoute.Tracks[0].Elements[i].Events, CurrentRoute.Tracks[0].Elements[i].Events.Length + 2);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 2] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 1, 0, false);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 1] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 2, 0, false);
								}
								else if (CurrentRoute.Stations[station.StationIndex].SafetySystem == SafetySystem.Ats)
								{
									Array.Resize(ref CurrentRoute.Tracks[0].Elements[i].Events, CurrentRoute.Tracks[0].Elements[i].Events.Length + 2);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 2] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 3, 0, false);
									CurrentRoute.Tracks[0].Elements[i].Events[CurrentRoute.Tracks[0].Elements[i].Events.Length - 1] = new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcTrackStatus, 0, 0, false);
									atc = false;
								}
							}
							else if (CurrentRoute.Tracks[0].Elements[i].Events[j] is LimitChangeEvent)
							{
								LimitChangeEvent limit = (LimitChangeEvent)CurrentRoute.Tracks[0].Elements[i].Events[j];
								int speed = (int)Math.Round(Math.Min(4095.0, 3.6 * limit.NextSpeedLimit));
								int distance = Math.Min(1048575, (int)Math.Round(CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition + limit.TrackPositionDelta));
								unchecked
								{
									int value = (int)((uint)speed | ((uint)distance << 12));
									transponders.Add(new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtcSpeedLimit, value, 0, false));
								}
							}
						}
						if (CurrentRoute.Tracks[0].Elements[i].Events[j] is TransponderEvent)
						{
							TransponderEvent transponder = CurrentRoute.Tracks[0].Elements[i].Events[j] as TransponderEvent;
							if (transponder.Type == (int)TransponderTypes.InternalAtsPTemporarySpeedLimit)
							{
								int speed = Math.Min(4095, transponder.Data);
								int distance = Math.Min(1048575, (int)Math.Round(CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition + transponder.TrackPositionDelta));
								unchecked
								{
									int value = (int)((uint)speed | ((uint)distance << 12));
									transponders.Add(new TransponderEvent(CurrentRoute, 0.0, TransponderTypes.AtsPTemporarySpeedLimit, value, 0, false));
								}
							}
						}
					}
				}
				int n = CurrentRoute.Tracks[0].Elements[0].Events.Length;
				Array.Resize(ref CurrentRoute.Tracks[0].Elements[0].Events, n + transponders.Count);
				for (int i = 0; i < transponders.Count; i++)
				{
					CurrentRoute.Tracks[0].Elements[0].Events[n + i] = transponders[i];
				}
			}
			// cant
			if (!PreviewOnly)
			{
				ComputeCantTangents();
				int subdivisions = (int)Math.Floor(Data.BlockInterval / 5.0);
				if (subdivisions >= 2)
				{
					CurrentRoute.Tracks[0].SmoothTurns(subdivisions, Plugin.CurrentHost);
					ComputeCantTangents();
				}
			}
		}

		private void ComputeCantTangents()
		{
			for (int ii = 0; ii < CurrentRoute.Tracks.Count; ii++)
			{
				int i = CurrentRoute.Tracks.ElementAt(ii).Key;
				CurrentRoute.Tracks[i].ComputeCantTangents();
			}
		}
	}
}
