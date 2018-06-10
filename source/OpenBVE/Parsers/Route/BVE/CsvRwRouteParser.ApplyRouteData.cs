using System;
using System.Collections.Generic;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void ApplyRouteData(string FileName, ref RouteData Data, bool PreviewOnly)
		{
			if (CompatibilityObjectsUsed != 0)
			{
				Interface.AddMessage(Interface.MessageType.Warning, false, "Warning: " + CompatibilityObjectsUsed + " compatibility objects were used.");
			}
			if (PreviewOnly)
			{
				if (freeObjCount == 0 && railtypeCount == 0)
				{
					throw new Exception(Interface.GetInterfaceString("errors_route_corrupt_noobjects"));
				}
			}
			string SignalPath, LimitPath, LimitGraphicsPath, TransponderPath;
			ObjectManager.StaticObject SignalPost, LimitPostStraight, LimitPostLeft, LimitPostRight, LimitPostInfinite;
			ObjectManager.StaticObject LimitOneDigit, LimitTwoDigits, LimitThreeDigits, StopPost;
			ObjectManager.StaticObject TransponderS, TransponderSN, TransponderFalseStart, TransponderPOrigin, TransponderPStop;
			if (!PreviewOnly)
			{
				//TODO: These need to be shifted into the main compatibility manager
				string CompatibilityFolder = Program.FileSystem.GetDataFolder("Compatibility");
				// load compatibility objects
				SignalPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Signals");
				SignalPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(SignalPath, "signal_post.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Limits");
				LimitGraphicsPath = OpenBveApi.Path.CombineDirectory(LimitPath, "Graphics");
				LimitPostStraight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_straight.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPostLeft = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_left.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPostRight = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_right.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitPostInfinite = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_infinite.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitOneDigit = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_1_digit.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitTwoDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_2_digits.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				LimitThreeDigits = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(LimitPath, "limit_3_digits.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				StopPost = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(CompatibilityFolder, "stop.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderPath = OpenBveApi.Path.CombineDirectory(CompatibilityFolder, "Transponders");
				TransponderS = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "s.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderSN = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "sn.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderFalseStart = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "falsestart.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderPOrigin = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "porigin.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
				TransponderPStop = ObjectManager.LoadStaticObject(OpenBveApi.Path.CombineFile(TransponderPath, "pstop.csv"), System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
			}
			else
			{
				SignalPath = null;
				LimitPath = null;
				LimitGraphicsPath = null;
				TransponderPath = null;
				SignalPost = null;
				LimitPostStraight = null;
				LimitPostLeft = null;
				LimitPostRight = null;
				LimitPostInfinite = null;
				LimitOneDigit = null;
				LimitTwoDigits = null;
				LimitThreeDigits = null;
				StopPost = null;
				TransponderS = null;
				TransponderSN = null;
				TransponderFalseStart = null;
				TransponderPOrigin = null;
				TransponderPStop = null;
			}
			// initialize
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			int LastBlock = (int)Math.Floor((Data.TrackPosition + 600.0) / Data.BlockInterval + 0.001) + 1;
			if (Data.Blocks[Data.Blocks.Length - 1].CurrentTrackState.CurveRadius < 300)
			{
				/*
				 * The track end event is placed 600m after the end of the final block
				 * If our curve radius in the final block is < 300, then our train will
				 * re-appear erroneously if the player is watching the final block
				 */
				Data.Blocks[Data.Blocks.Length - 1].CurrentTrackState.CurveRadius = 0.0;
			}
			int BlocksUsed = Data.Blocks.Length;
			Data.CreateMissingBlocks(ref BlocksUsed, LastBlock, PreviewOnly);
			Array.Resize<Block>(ref Data.Blocks, BlocksUsed);
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
				if (Data.Blocks[0].Background >= 0 & Data.Blocks[0].Background < Data.Backgrounds.Length)
				{
					BackgroundManager.CurrentBackground = Data.Backgrounds[Data.Blocks[0].Background];
				}
				else
				{
					BackgroundManager.CurrentBackground = new BackgroundManager.StaticBackground(null, 6, false);
				}
				BackgroundManager.TargetBackground = BackgroundManager.CurrentBackground;
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
			TrackManager.CurrentTrack = new TrackManager.Track { Elements = new TrackManager.TrackElement[] { } };
			double CurrentSpeedLimit = double.PositiveInfinity;
			int CurrentRunIndex = 0;
			int CurrentFlangeIndex = 0;
			if (Data.FirstUsedBlock < 0) Data.FirstUsedBlock = 0;
			TrackManager.CurrentTrack.Elements = new TrackManager.TrackElement[256];
			int CurrentTrackLength = 0;
			int PreviousFogElement = -1;
			int PreviousFogEvent = -1;
			Game.Fog PreviousFog = new Game.Fog(Game.NoFogStart, Game.NoFogEnd, new Color24(128, 128, 128), -Data.BlockInterval);
			Game.Fog CurrentFog = new Game.Fog(Game.NoFogStart, Game.NoFogEnd, new Color24(128, 128, 128), 0.0);
			// process blocks
			double progressFactor = Data.Blocks.Length - Data.FirstUsedBlock == 0 ? 0.5 : 0.5 / (double)(Data.Blocks.Length - Data.FirstUsedBlock);
			for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++)
			{
				Loading.RouteProgress = 0.6667 + (double)(i - Data.FirstUsedBlock) * progressFactor;
				if ((i & 15) == 0)
				{
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				double StartingDistance = (double)i * Data.BlockInterval;
				double EndingDistance = StartingDistance + Data.BlockInterval;
				// normalize
				World.Normalize(ref Direction.X, ref Direction.Y);
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
				TrackManager.TrackElement WorldTrackElement = Data.Blocks[i].CurrentTrackState;
				int n = CurrentTrackLength;
				if (n >= TrackManager.CurrentTrack.Elements.Length)
				{
					Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, TrackManager.CurrentTrack.Elements.Length << 1);
				}
				CurrentTrackLength++;
				TrackManager.CurrentTrack.Elements[n] = WorldTrackElement;
				TrackManager.CurrentTrack.Elements[n].WorldPosition = Position;
				TrackManager.CurrentTrack.Elements[n].WorldDirection = Vector3.GetVector3(Direction, Data.Blocks[i].Pitch);
				TrackManager.CurrentTrack.Elements[n].WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X);
				World.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection, TrackManager.CurrentTrack.Elements[n].WorldSide, out TrackManager.CurrentTrack.Elements[n].WorldUp);
				TrackManager.CurrentTrack.Elements[n].StartingTrackPosition = StartingDistance;
				TrackManager.CurrentTrack.Elements[n].Events = new TrackManager.GeneralEvent[] { };
				TrackManager.CurrentTrack.Elements[n].AdhesionMultiplier = Data.Blocks[i].AdhesionMultiplier;
				TrackManager.CurrentTrack.Elements[n].CsvRwAccuracyLevel = Data.Blocks[i].Accuracy;
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
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BackgroundChangeEvent(0.0, Data.Backgrounds[typ], Data.Backgrounds[Data.Blocks[i].Background]);
						}
					}
				}
				// brightness
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++)
					{
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						double d = Data.Blocks[i].Brightness[j].TrackPosition - StartingDistance;
						TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BrightnessChangeEvent(d, Data.Blocks[i].Brightness[j].Value, CurrentBrightnessValue, Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition);
						if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
						{
							TrackManager.BrightnessChangeEvent bce = (TrackManager.BrightnessChangeEvent)TrackManager.CurrentTrack.Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
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
							if (i == 0 && StartingDistance == 0)
							{
								//Fog starts at zero position
								PreviousFog = Data.Blocks[i].Fog;
							}
							Data.Blocks[i].Fog.TrackPosition = StartingDistance;
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, Data.Blocks[i].Fog, Data.Blocks[i].Fog);
							if (PreviousFogElement >= 0 & PreviousFogEvent >= 0)
							{
								TrackManager.FogChangeEvent e = (TrackManager.FogChangeEvent)TrackManager.CurrentTrack.Elements[PreviousFogElement].Events[PreviousFogEvent];
								e.NextFog = Data.Blocks[i].Fog;
							}
							else
							{
								Game.PreviousFog = PreviousFog;
								Game.CurrentFog = PreviousFog;
								Game.NextFog = Data.Blocks[i].Fog;
							}
							PreviousFog = Data.Blocks[i].Fog;
							PreviousFogElement = n;
							PreviousFogEvent = m;
						}
					}
					else
					{
						if (i == 0 && StartingDistance == 0)
						{
							//Fog starts at zero position
							CurrentFog = Data.Blocks[i].Fog;
							PreviousFog = CurrentFog;
							Game.PreviousFog = CurrentFog;
							Game.CurrentFog = CurrentFog;
							Game.NextFog = CurrentFog;


						}
						else
						{
							Data.Blocks[i].Fog.TrackPosition = StartingDistance + Data.BlockInterval;
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, CurrentFog, Data.Blocks[i].Fog);
							PreviousFog = CurrentFog;
							CurrentFog = Data.Blocks[i].Fog;
						}
					}
				}
				// rail sounds
				if (!PreviewOnly)
				{
					int j = Data.Blocks[i].RailType[0];
					int r = j < Data.Structure.Run.Length ? Data.Structure.Run[j] : 0;
					int f = j < Data.Structure.Flange.Length ? Data.Structure.Flange[j] : 0;
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.RailSoundsChangeEvent(0.0, CurrentRunIndex, CurrentFlangeIndex, r, f);
					CurrentRunIndex = r;
					CurrentFlangeIndex = f;
				}
				// point sound
				if (!PreviewOnly)
				{
					if (i < Data.Blocks.Length - 1)
					{
						bool q = false;
						for (int j = 0; j < Data.Blocks[i].Rail.Length; j++)
						{
							if (Data.Blocks[i].Rail[j].RailStart & Data.Blocks[i + 1].Rail.Length > j)
							{
								bool qx = Math.Sign(Data.Blocks[i].Rail[j].RailStartX) != Math.Sign(Data.Blocks[i + 1].Rail[j].RailEndX);
								bool qy = Data.Blocks[i].Rail[j].RailStartY * Data.Blocks[i + 1].Rail[j].RailEndY <= 0.0;
								if (qx & qy)
								{
									q = true;
									break;
								}
							}
						}
						if (q)
						{
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.PointSoundEvent(12.5);
						}
					}
				}
				// station
				if (Data.Blocks[i].Station >= 0)
				{
					// station
					int s = Data.Blocks[i].Station;
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.StationStartEvent(0.0, s);
					double dx, dy = 3.0;
					if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
					Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
					Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
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
									m = TrackManager.CurrentTrack.Elements[j].Events.Length;
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[j].Events, m + 1);
									TrackManager.CurrentTrack.Elements[j].Events[m] = new TrackManager.StationPassAlarmEvent(0.0);
								}
							}
						}
					}
				}
				// stop
				for (int j = 0; j < Data.Blocks[i].Stop.Length; j++)
				{
					int s = Data.Blocks[i].Stop[j].Station;
					int t = Game.Stations[s].Stops.Length;
					Array.Resize<Game.StationStop>(ref Game.Stations[s].Stops, t + 1);
					Game.Stations[s].Stops[t].TrackPosition = Data.Blocks[i].Stop[j].TrackPosition;
					Game.Stations[s].Stops[t].ForwardTolerance = Data.Blocks[i].Stop[j].ForwardTolerance;
					Game.Stations[s].Stops[t].BackwardTolerance = Data.Blocks[i].Stop[j].BackwardTolerance;
					Game.Stations[s].Stops[t].Cars = Data.Blocks[i].Stop[j].Cars;
					double dx, dy = 2.0;
					if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors)
					{
						dx = -5.0;
					}
					else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors)
					{
						dx = 5.0;
					}
					else
					{
						dx = 0.0;
					}
					Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
					Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
					Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
				}
				// limit
				for (int j = 0; j < Data.Blocks[i].Limit.Length; j++)
				{
					int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
					Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
					double d = Data.Blocks[i].Limit[j].TrackPosition - StartingDistance;
					TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.LimitChangeEvent(d, CurrentSpeedLimit, Data.Blocks[i].Limit[j].Speed);
					CurrentSpeedLimit = Data.Blocks[i].Limit[j].Speed;
				}
				// marker
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Markers.Length; j++)
					{
						if (Data.Markers[j].StartingPosition >= StartingDistance & Data.Markers[j].StartingPosition < EndingDistance)
						{
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Markers[j].StartingPosition - StartingDistance;
							if (Data.Markers[j].Message != null)
							{
								TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.MarkerStartEvent(d, Data.Markers[j].Message);
							}
						}
						if (Data.Markers[j].EndingPosition >= StartingDistance & Data.Markers[j].EndingPosition < EndingDistance)
						{
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Markers[j].EndingPosition - StartingDistance;
							if (Data.Markers[j].Message != null)
							{
								TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.MarkerEndEvent(d, Data.Markers[j].Message);
							}
						}
					}
				}
				// sound
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].Sound.Length; j++)
					{
						if (Data.Blocks[i].Sound[j].Type == SoundType.TrainStatic | Data.Blocks[i].Sound[j].Type == SoundType.TrainDynamic)
						{
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Blocks[i].Sound[j].TrackPosition - StartingDistance;
							switch (Data.Blocks[i].Sound[j].Type)
							{
								case SoundType.TrainStatic:
									TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(d, Data.Blocks[i].Sound[j].SoundBuffer, true, true, false, new Vector3(0.0, 0.0, 0.0), 0.0);
									break;
								case SoundType.TrainDynamic:
									TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(d, Data.Blocks[i].Sound[j].SoundBuffer, false, false, true, new Vector3(0.0, 0.0, 0.0), Data.Blocks[i].Sound[j].Speed);
									break;
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
					World.Rotate(ref Direction, cosag, sinag);
					World.RotatePlane(ref TrackManager.CurrentTrack.Elements[n].WorldDirection, cosag, sinag);
					World.RotatePlane(ref TrackManager.CurrentTrack.Elements[n].WorldSide, cosag, sinag);
					World.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection, TrackManager.CurrentTrack.Elements[n].WorldSide, out TrackManager.CurrentTrack.Elements[n].WorldUp);
				}
				//Pitch
				if (Data.Blocks[i].Pitch != 0.0)
				{
					TrackManager.CurrentTrack.Elements[n].Pitch = Data.Blocks[i].Pitch;
				}
				else
				{
					TrackManager.CurrentTrack.Elements[n].Pitch = 0.0;
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
					World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
				}
				else if (WorldTrackElement.CurveRadius != 0.0)
				{
					double d = Data.BlockInterval;
					double r = WorldTrackElement.CurveRadius;
					double b = d / Math.Abs(r);
					c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
					a = 0.5 * (double)Math.Sign(r) * b;
					World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
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
				World.Transformation GroundTransformation = new World.Transformation(TrackYaw, 0.0, 0.0);
				World.Transformation TrackTransformation = new World.Transformation(TrackYaw, TrackPitch, 0.0);
				World.Transformation NullTransformation = new World.Transformation(0.0, 0.0, 0.0);
				// ground
				if (!PreviewOnly)
				{
					int cb = (int)Math.Floor((double)i + 0.001);
					int ci = (cb % Data.Blocks[i].Cycle.Length + Data.Blocks[i].Cycle.Length) % Data.Blocks[i].Cycle.Length;
					int gi = Data.Blocks[i].Cycle[ci];
					if (gi >= 0 & Data.Structure.Ground.ContainsKey(gi))
					{
						Data.Structure.Ground[Data.Blocks[i].Cycle[ci]].CreateObject(Position + new Vector3(0.0, -Data.Blocks[i].Height, 0.0), GroundTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
					}
				}
				// ground-aligned free objects
				if (!PreviewOnly)
				{
					for (int j = 0; j < Data.Blocks[i].GroundFreeObj.Length; j++)
					{
						int sttype = Data.Blocks[i].GroundFreeObj[j].Type;
						double d = Data.Blocks[i].GroundFreeObj[j].TrackPosition - StartingDistance;
						double dx = Data.Blocks[i].GroundFreeObj[j].X;
						double dy = Data.Blocks[i].GroundFreeObj[j].Y;
						Vector3 wpos = Position + new Vector3(Direction.X * d + Direction.Y * dx, dy - Data.Blocks[i].Height, Direction.Y * d - Direction.X * dx);
						double tpos = Data.Blocks[i].GroundFreeObj[j].TrackPosition;
						Data.Structure.FreeObjects[sttype].CreateObject(wpos, GroundTransformation, new World.Transformation(Data.Blocks[i].GroundFreeObj[j].Yaw, Data.Blocks[i].GroundFreeObj[j].Pitch, Data.Blocks[i].GroundFreeObj[j].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
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
						World.Transformation RailTransformation;
						double planar, updown;
						if (j == 0)
						{
							// rail 0
							planar = 0.0;
							updown = 0.0;
							RailTransformation = new World.Transformation(TrackTransformation, planar, updown, 0.0);
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
									World.Rotate(ref Direction2, Math.Cos(-a), Math.Sin(-a));
								}
								if (Data.Blocks[i + 1].Turn != 0.0)
								{
									double ag = -Math.Atan(Data.Blocks[i + 1].Turn);
									double cosag = Math.Cos(ag);
									double sinag = Math.Sin(ag);
									World.Rotate(ref Direction2, cosag, sinag);
								}
								double a2 = 0.0;
								// double c2 = Data.BlockInterval;
								// double h2 = 0.0;
								if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0 & Data.Blocks[i + 1].Pitch != 0.0)
								{
									double d2 = Data.BlockInterval;
									double p2 = Data.Blocks[i + 1].Pitch;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double s2 = d2 / Math.Sqrt(1.0 + p2 * p2);
									// h2 = s2 * p2;
									double b2 = s2 / Math.Abs(r2);
									// c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									World.Rotate(ref Direction2, Math.Cos(-a2), Math.Sin(-a2));
								}
								else if (Data.Blocks[i + 1].CurrentTrackState.CurveRadius != 0.0)
								{
									double d2 = Data.BlockInterval;
									double r2 = Data.Blocks[i + 1].CurrentTrackState.CurveRadius;
									double b2 = d2 / Math.Abs(r2);
									// c2 = Math.Sqrt(2.0 * r2 * r2 * (1.0 - Math.Cos(b2)));
									a2 = 0.5 * (double)Math.Sign(r2) * b2;
									World.Rotate(ref Direction2, Math.Cos(-a2), Math.Sin(-a2));
								}
								// else if (Data.Blocks[i + 1].Pitch != 0.0) {
								// double p2 = Data.Blocks[i + 1].Pitch;
								// double d2 = Data.BlockInterval;
								// c2 = d2 / Math.Sqrt(1.0 + p2 * p2);
								// h2 = c2 * p2;
								// }

								//These generate a compiler warning, as secondary tracks do not generate yaw, as they have no
								//concept of a curve, but rather are a straight line between two points
								//TODO: Revist the handling of secondary tracks ==> !!BACKWARDS INCOMPATIBLE!!
								/*
								double TrackYaw2 = Math.Atan2(Direction2.X, Direction2.Y);
								double TrackPitch2 = Math.Atan(Data.Blocks[i + 1].Pitch);
								World.Transformation GroundTransformation2 = new World.Transformation(TrackYaw2, 0.0, 0.0);
								World.Transformation TrackTransformation2 = new World.Transformation(TrackYaw2, TrackPitch2, 0.0);
								 */
								double x2 = Data.Blocks[i + 1].Rail[j].RailEndX;
								double y2 = Data.Blocks[i + 1].Rail[j].RailEndY;
								Vector3 offset2 = new Vector3(Direction2.Y * x2, y2, -Direction2.X * x2);
								Vector3 pos2 = Position2 + offset2;
								double rx = pos2.X - pos.X;
								double ry = pos2.Y - pos.Y;
								double rz = pos2.Z - pos.Z;
								World.Normalize(ref rx, ref ry, ref rz);
								RailTransformation.Z = new Vector3(rx, ry, rz);
								RailTransformation.X = new Vector3(rz, 0.0, -rx);
								World.Normalize(ref RailTransformation.X.X, ref RailTransformation.X.Z);
								RailTransformation.Y = Vector3.Cross(RailTransformation.Z, RailTransformation.X);
								double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
								double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
								planar = Math.Atan(dx / c);
								dh = dy / c;
								updown = Math.Atan(dh);
							}
							else
							{
								planar = 0.0;
								dh = 0.0;
								updown = 0.0;
								RailTransformation = new World.Transformation(TrackTransformation, 0.0, 0.0, 0.0);
							}
						}
						if (Data.Structure.RailObjects.ContainsKey(Data.Blocks[i].RailType[j]))
						{
							if (Data.Structure.RailObjects[Data.Blocks[i].RailType[j]] != null)
							{
								Data.Structure.RailObjects[Data.Blocks[i].RailType[j]].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
						}
						// points of interest
						for (int k = 0; k < Data.Blocks[i].PointsOfInterest.Length; k++)
						{
							if (Data.Blocks[i].PointsOfInterest[k].RailIndex == j)
							{
								double d = Data.Blocks[i].PointsOfInterest[k].TrackPosition - StartingDistance;
								double x = Data.Blocks[i].PointsOfInterest[k].X;
								double y = Data.Blocks[i].PointsOfInterest[k].Y;
								int m = Game.PointsOfInterest.Length;
								Array.Resize<Game.PointOfInterest>(ref Game.PointsOfInterest, m + 1);
								Game.PointsOfInterest[m].TrackPosition = Data.Blocks[i].PointsOfInterest[k].TrackPosition;
								if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j)
								{
									double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
									double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
									dx = Data.Blocks[i].Rail[j].RailStartX + d / Data.BlockInterval * dx;
									dy = Data.Blocks[i].Rail[j].RailStartY + d / Data.BlockInterval * dy;
									Game.PointsOfInterest[m].TrackOffset = new Vector3(x + dx, y + dy, 0.0);
								}
								else
								{
									double dx = Data.Blocks[i].Rail[j].RailStartX;
									double dy = Data.Blocks[i].Rail[j].RailStartY;
									Game.PointsOfInterest[m].TrackOffset = new Vector3(x + dx, y + dy, 0.0);
								}
								Game.PointsOfInterest[m].TrackYaw = Data.Blocks[i].PointsOfInterest[k].Yaw + planar;
								Game.PointsOfInterest[m].TrackPitch = Data.Blocks[i].PointsOfInterest[k].Pitch + updown;
								Game.PointsOfInterest[m].TrackRoll = Data.Blocks[i].PointsOfInterest[k].Roll;
								Game.PointsOfInterest[m].Text = Data.Blocks[i].PointsOfInterest[k].Text;
							}
						}
						// poles
						if (Data.Blocks[i].RailPole.Length > j && Data.Blocks[i].RailPole[j].Exists)
						{
							double dz = StartingDistance / Data.Blocks[i].RailPole[j].Interval;
							dz -= Math.Floor(dz + 0.5);
							if (dz >= -0.01 & dz <= 0.01)
							{
								if (Data.Blocks[i].RailPole[j].Mode == 0)
								{
									if (Data.Blocks[i].RailPole[j].Location <= 0.0)
									{
										Data.Structure.Poles[0][Data.Blocks[i].RailPole[j].Type].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									else
									{
										ObjectManager.UnifiedObject Pole = GetMirroredObject(Data.Structure.Poles[0][Data.Blocks[i].RailPole[j].Type]);
										Pole.CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
								}
								else
								{
									int m = Data.Blocks[i].RailPole[j].Mode;
									double dx = -Data.Blocks[i].RailPole[j].Location * 3.8;
									double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
									double wx = Math.Cos(wa);
									double wy = Math.Tan(updown);
									double wz = Math.Sin(wa);
									World.Normalize(ref wx, ref wy, ref wz);
									double sx = Direction.Y;
									double sy = 0.0;
									double sz = -Direction.X;
									Vector3 wpos = pos + new Vector3(sx * dx + wx * dz, sy * dx + wy * dz, sz * dx + wz * dz);
									int type = Data.Blocks[i].RailPole[j].Type;
									Data.Structure.Poles[m][type].CreateObject(wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
								}
							}
						}
						// walls
						if (Data.Blocks[i].RailWall.Length > j && Data.Blocks[i].RailWall[j].Exists)
						{
							if (Data.Blocks[i].RailWall[j].Direction <= 0)
							{
								Data.Structure.WallL[Data.Blocks[i].RailWall[j].Type].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
							if (Data.Blocks[i].RailWall[j].Direction >= 0)
							{
								Data.Structure.WallR[Data.Blocks[i].RailWall[j].Type].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
						}
						// dikes
						if (Data.Blocks[i].RailDike.Length > j && Data.Blocks[i].RailDike[j].Exists)
						{
							if (Data.Blocks[i].RailDike[j].Direction <= 0)
							{
								Data.Structure.DikeL[Data.Blocks[i].RailDike[j].Type].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
							if (Data.Blocks[i].RailDike[j].Direction >= 0)
							{
								Data.Structure.DikeR[Data.Blocks[i].RailDike[j].Type].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
							}
						}
						// sounds
						if (j == 0)
						{
							for (int k = 0; k < Data.Blocks[i].Sound.Length; k++)
							{
								if (Data.Blocks[i].Sound[k].Type == SoundType.World)
								{
									if (Data.Blocks[i].Sound[k].SoundBuffer != null)
									{
										double d = Data.Blocks[i].Sound[k].TrackPosition - StartingDistance;
										double dx = Data.Blocks[i].Sound[k].X;
										double dy = Data.Blocks[i].Sound[k].Y;
										double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
										double wx = Math.Cos(wa);
										double wy = Math.Tan(updown);
										double wz = Math.Sin(wa);
										World.Normalize(ref wx, ref wy, ref wz);
										double sx = Direction.Y;
										double sy = 0.0;
										double sz = -Direction.X;
										double ux, uy, uz;
										World.Cross(wx, wy, wz, sx, sy, sz, out ux, out uy, out uz);
										Vector3 wpos = pos + new Vector3(sx * dx + ux * dy + wx * d, sy * dx + uy * dy + wy * d, sz * dx + uz * dy + wz * d);
										Sounds.PlaySound(Data.Blocks[i].Sound[k].SoundBuffer, 1.0, 1.0, wpos, true);
									}
								}
							}
						}
						// forms
						for (int k = 0; k < Data.Blocks[i].Form.Length; k++)
						{
							// primary rail
							if (Data.Blocks[i].Form[k].PrimaryRail == j)
							{
								if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailStub)
								{
									if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										Data.Structure.FormL[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										if (Data.Blocks[i].Form[k].RoofType > 0)
										{
											if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											}
											else
											{
												Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
										}
									}
								}
								else if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailL)
								{
									if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										Data.Structure.FormL[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (!Data.Structure.FormCL.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0)
									{
										if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
										if (!Data.Structure.RoofCL.ContainsKey(Data.Blocks[i].Form[k].RoofType))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
								else if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailR)
								{
									if (!Data.Structure.FormR.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										Data.Structure.FormR[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (!Data.Structure.FormCR.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0)
									{
										if (!Data.Structure.RoofR.ContainsKey(Data.Blocks[i].Form[k].RoofType))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
										if (!Data.Structure.RoofCR.ContainsKey(Data.Blocks[i].Form[k].RoofType))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											ObjectManager.CreateStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
								else if (Data.Blocks[i].Form[k].SecondaryRail > 0)
								{
									int p = Data.Blocks[i].Form[k].PrimaryRail;
									double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
									double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
									int s = Data.Blocks[i].Form[k].SecondaryRail;
									if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart)
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex2 is out of range in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
									}
									else
									{
										double sx0 = Data.Blocks[i].Rail[s].RailStartX;
										double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
										double d0 = sx0 - px0;
										double d1 = sx1 - px1;
										if (d0 < 0.0)
										{
											if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											}
											else
											{
												Data.Structure.FormL[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (!Data.Structure.FormCL.ContainsKey(Data.Blocks[i].Form[k].FormType))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											}
											else
											{
												ObjectManager.StaticObject FormC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType], d0, d1);
												ObjectManager.CreateStaticObject(FormC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (Data.Blocks[i].Form[k].RoofType > 0)
											{
												if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType))
												{
													Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												}
												else
												{
													Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
												if (!Data.Structure.RoofCL.ContainsKey(Data.Blocks[i].Form[k].RoofType))
												{
													Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												}
												else
												{
													ObjectManager.StaticObject RoofC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType], d0, d1);
													ObjectManager.CreateStaticObject(RoofC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
											}
										}
										else if (d0 > 0.0)
										{
											if (!Data.Structure.FormR.ContainsKey(Data.Blocks[i].Form[k].FormType))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											}
											else
											{
												Data.Structure.FormR[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (!Data.Structure.FormCR.ContainsKey(Data.Blocks[i].Form[k].FormType))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
											}
											else
											{
												ObjectManager.StaticObject FormC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType], d0, d1);
												ObjectManager.CreateStaticObject(FormC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
											}
											if (Data.Blocks[i].Form[k].RoofType > 0)
											{
												if (!Data.Structure.RoofR.ContainsKey(Data.Blocks[i].Form[k].RoofType))
												{
													Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												}
												else
												{
													Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
												if (!Data.Structure.RoofCR.ContainsKey(Data.Blocks[i].Form[k].RoofType))
												{
													Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
												}
												else
												{
													ObjectManager.StaticObject RoofC = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType], d0, d1);
													ObjectManager.CreateStaticObject(RoofC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
												}
											}
										}
									}
								}
							}
							// secondary rail
							if (Data.Blocks[i].Form[k].SecondaryRail == j)
							{
								int p = Data.Blocks[i].Form[k].PrimaryRail;
								double px = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
								int s = Data.Blocks[i].Form[k].SecondaryRail;
								double sx = Data.Blocks[i].Rail[s].RailStartX;
								double d = px - sx;
								if (d < 0.0)
								{
									if (!Data.Structure.FormL.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										Data.Structure.FormL[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0)
									{
										if (!Data.Structure.RoofL.ContainsKey(Data.Blocks[i].Form[k].RoofType))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
								else
								{
									if (!Data.Structure.FormR.ContainsKey(Data.Blocks[i].Form[k].FormType))
									{
										Interface.AddMessage(Interface.MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
									}
									else
									{
										Data.Structure.FormR[Data.Blocks[i].Form[k].FormType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
									}
									if (Data.Blocks[i].Form[k].RoofType > 0)
									{
										if (!Data.Structure.RoofR.ContainsKey(Data.Blocks[i].Form[k].RoofType))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType].CreateObject(pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
								}
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
									Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
								}
								else
								{
									double sx0 = Data.Blocks[i].Rail[s].RailStartX;
									double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
									double d0 = sx0 - px0;
									double d1 = sx1 - px1;
									if (d0 < 0.0)
									{
										if (!Data.Structure.CrackL.ContainsKey(Data.Blocks[i].Crack[k].Type))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "CrackStructureIndex references a CrackL not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											ObjectManager.StaticObject Crack = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.CrackL[Data.Blocks[i].Crack[k].Type], d0, d1);
											ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
										}
									}
									else if (d0 > 0.0)
									{
										if (!Data.Structure.CrackR.ContainsKey(Data.Blocks[i].Crack[k].Type))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "CrackStructureIndex references a CrackR not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
										}
										else
										{
											ObjectManager.StaticObject Crack = GetTransformedStaticObject((ObjectManager.StaticObject)Data.Structure.CrackR[Data.Blocks[i].Crack[k].Type], d0, d1);
											ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, StartingDistance);
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
								double dx = Data.Blocks[i].RailFreeObj[j][k].X;
								double dy = Data.Blocks[i].RailFreeObj[j][k].Y;
								double dz = Data.Blocks[i].RailFreeObj[j][k].TrackPosition - StartingDistance;
								Vector3 wpos = pos;
								wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
								wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
								wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
								double tpos = Data.Blocks[i].RailFreeObj[j][k].TrackPosition;
								ObjectManager.UnifiedObject obj;
								Data.Structure.FreeObjects.TryGetValue(sttype, out obj);
								obj.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch, Data.Blocks[i].RailFreeObj[j][k].Roll), -1, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, 1.0, false);
							}
						}
						// transponder objects
						if (j == 0)
						{
							for (int k = 0; k < Data.Blocks[i].Transponder.Length; k++)
							{
								ObjectManager.UnifiedObject obj = null;
								if (Data.Blocks[i].Transponder[k].ShowDefaultObject)
								{
									switch (Data.Blocks[i].Transponder[k].Type)
									{
										case 0: obj = TransponderS; break;
										case 1: obj = TransponderSN; break;
										case 2: obj = TransponderFalseStart; break;
										case 3: obj = TransponderPOrigin; break;
										case 4: obj = TransponderPStop; break;
									}
								}
								else
								{
									int b = Data.Blocks[i].Transponder[k].BeaconStructureIndex;
									if (b >= 0 & Data.Structure.Beacon.ContainsKey(b))
									{
										obj = Data.Structure.Beacon[b];
									}
								}
								if (obj != null)
								{
									double dx = Data.Blocks[i].Transponder[k].X;
									double dy = Data.Blocks[i].Transponder[k].Y;
									double dz = Data.Blocks[i].Transponder[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Transponder[k].TrackPosition;
									if (Data.Blocks[i].Transponder[k].ShowDefaultObject)
									{
										double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
										obj.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Transponder[k].Yaw, Data.Blocks[i].Transponder[k].Pitch, Data.Blocks[i].Transponder[k].Roll), -1, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
									}
									else
									{
										obj.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Transponder[k].Yaw, Data.Blocks[i].Transponder[k].Pitch, Data.Blocks[i].Transponder[k].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
									}
								}
							}
							for (int k = 0; k < Data.Blocks[i].DestinationChanges.Length; k++)
							{
								ObjectManager.UnifiedObject obj = null;
								int b = Data.Blocks[i].DestinationChanges[k].BeaconStructureIndex;
								if (b >= 0 & Data.Structure.Beacon.ContainsKey(b))
								{
									obj = Data.Structure.Beacon[b];
								}
								if (obj != null)
								{
									double dx = Data.Blocks[i].DestinationChanges[k].X;
									double dy = Data.Blocks[i].DestinationChanges[k].Y;
									double dz = Data.Blocks[i].DestinationChanges[k].TrackPosition - StartingDistance;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].DestinationChanges[k].TrackPosition;
									obj.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].DestinationChanges[k].Yaw, Data.Blocks[i].DestinationChanges[k].Pitch, Data.Blocks[i].DestinationChanges[k].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos);
								}
							}
						}
						// sections/signals/transponders
						if (j == 0)
						{
							// signals
							for (int k = 0; k < Data.Blocks[i].Signal.Length; k++)
							{
								SignalData sd;
								if (Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex >= 0)
								{
									sd = Data.CompatibilitySignals[Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex];
								}
								else
								{
									sd = Data.Signals[Data.Blocks[i].Signal[k].SignalObjectIndex];
								}
								// objects
								double dz = Data.Blocks[i].Signal[k].TrackPosition - StartingDistance;
								if (Data.Blocks[i].Signal[k].ShowPost)
								{
									// post
									double dx = Data.Blocks[i].Signal[k].X;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Signal[k].TrackPosition;
									double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
									ObjectManager.CreateStaticObject(SignalPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
								}
								if (Data.Blocks[i].Signal[k].ShowObject)
								{
									// signal object
									double dx = Data.Blocks[i].Signal[k].X;
									double dy = Data.Blocks[i].Signal[k].Y;
									Vector3 wpos = pos;
									wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
									wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
									wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
									double tpos = Data.Blocks[i].Signal[k].TrackPosition;
									if (sd is AnimatedObjectSignalData)
									{
										AnimatedObjectSignalData aosd = (AnimatedObjectSignalData)sd;
										aosd.Objects.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, 1.0, false);
									}
									else if (sd is CompatibilitySignalData)
									{
										CompatibilitySignalData csd = (CompatibilitySignalData)sd;
										if (csd.Numbers.Length != 0)
										{
											double brightness = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
											ObjectManager.AnimatedObjectCollection aoc = new ObjectManager.AnimatedObjectCollection();
											aoc.Objects = new ObjectManager.AnimatedObject[1];
											aoc.Objects[0] = new ObjectManager.AnimatedObject();
											aoc.Objects[0].States = new ObjectManager.AnimatedObjectState[csd.Numbers.Length];
											for (int l = 0; l < csd.Numbers.Length; l++)
											{
												aoc.Objects[0].States[l].Object = csd.Objects[l].Clone();
											}
											string expr = "";
											for (int l = 0; l < csd.Numbers.Length - 1; l++)
											{
												expr += "section " + csd.Numbers[l].ToString(Culture) + " <= " + l.ToString(Culture) + " ";
											}
											expr += (csd.Numbers.Length - 1).ToString(Culture);
											for (int l = 0; l < csd.Numbers.Length - 1; l++)
											{
												expr += " ?";
											}
											aoc.Objects[0].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(expr);
											aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Program.RandomNumberGenerator.NextDouble();
											aoc.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, brightness, false);
										}
									}
									else if (sd is Bve4SignalData)
									{
										Bve4SignalData b4sd = (Bve4SignalData)sd;
										if (b4sd.SignalTextures.Length != 0)
										{
											int m = Math.Max(b4sd.SignalTextures.Length, b4sd.GlowTextures.Length);
											int zn = 0;
											for (int l = 0; l < m; l++)
											{
												if (l < b4sd.SignalTextures.Length && b4sd.SignalTextures[l] != null || l < b4sd.GlowTextures.Length && b4sd.GlowTextures[l] != null)
												{
													zn++;
												}
											}
											ObjectManager.AnimatedObjectCollection aoc = new ObjectManager.AnimatedObjectCollection();
											aoc.Objects = new ObjectManager.AnimatedObject[1];
											aoc.Objects[0] = new ObjectManager.AnimatedObject();
											aoc.Objects[0].States = new ObjectManager.AnimatedObjectState[zn];
											int zi = 0;
											string expr = "";
											for (int l = 0; l < m; l++)
											{
												bool qs = l < b4sd.SignalTextures.Length && b4sd.SignalTextures[l] != null;
												bool qg = l < b4sd.GlowTextures.Length && b4sd.GlowTextures[l] != null;
												if (qs & qg)
												{
													ObjectManager.StaticObject so = b4sd.BaseObject.Clone(b4sd.SignalTextures[l], null);
													ObjectManager.StaticObject go = b4sd.GlowObject.Clone(b4sd.GlowTextures[l], null);
													so.JoinObjects(go);
													aoc.Objects[0].States[zi].Object = so;
												}
												else if (qs)
												{
													ObjectManager.StaticObject so = b4sd.BaseObject.Clone(b4sd.SignalTextures[l], null);
													aoc.Objects[0].States[zi].Object = so;
												}
												else if (qg)
												{
													ObjectManager.StaticObject go = b4sd.GlowObject.Clone(b4sd.GlowTextures[l], null);
													aoc.Objects[0].States[zi].Object = go;
												}
												if (qs | qg)
												{
													if (zi < zn - 1)
													{
														expr += "section " + l.ToString(Culture) + " <= " + zi.ToString(Culture) + " ";
													}
													else
													{
														expr += zi.ToString(Culture);
													}
													zi++;
												}
											}
											for (int l = 0; l < zn - 1; l++)
											{
												expr += " ?";
											}
											aoc.Objects[0].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(expr);
											aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Program.RandomNumberGenerator.NextDouble();
											aoc.CreateObject(wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, Data.BlockInterval, tpos, 1.0, false);
										}
									}
								}
							}
							// sections
							for (int k = 0; k < Data.Blocks[i].Section.Length; k++)
							{
								int m = Game.Sections.Length;
								Array.Resize<Game.Section>(ref Game.Sections, m + 1);
								// create associated transponders
								for (int g = 0; g <= i; g++)
								{
									for (int l = 0; l < Data.Blocks[g].Transponder.Length; l++)
									{
										if (Data.Blocks[g].Transponder[l].Type != -1 & Data.Blocks[g].Transponder[l].Section == m)
										{
											int o = TrackManager.CurrentTrack.Elements[n - i + g].Events.Length;
											Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n - i + g].Events, o + 1);
											double dt = Data.Blocks[g].Transponder[l].TrackPosition - StartingDistance + (double)(i - g) * Data.BlockInterval;
											TrackManager.CurrentTrack.Elements[n - i + g].Events[o] = new TrackManager.TransponderEvent(dt, Data.Blocks[g].Transponder[l].Type, Data.Blocks[g].Transponder[l].Data, m, Data.Blocks[g].Transponder[l].ClipToFirstRedSection);
											Data.Blocks[g].Transponder[l].Type = -1;
										}
									}
								}
								// create section
								Game.Sections[m].TrackPosition = Data.Blocks[i].Section[k].TrackPosition;
								Game.Sections[m].Aspects = new Game.SectionAspect[Data.Blocks[i].Section[k].Aspects.Length];
								for (int l = 0; l < Data.Blocks[i].Section[k].Aspects.Length; l++)
								{
									Game.Sections[m].Aspects[l].Number = Data.Blocks[i].Section[k].Aspects[l];
									if (Data.Blocks[i].Section[k].Aspects[l] >= 0 & Data.Blocks[i].Section[k].Aspects[l] < Data.SignalSpeeds.Length)
									{
										Game.Sections[m].Aspects[l].Speed = Data.SignalSpeeds[Data.Blocks[i].Section[k].Aspects[l]];
									}
									else
									{
										Game.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
									}
								}
								Game.Sections[m].Type = Data.Blocks[i].Section[k].Type;
								Game.Sections[m].CurrentAspect = -1;
								if (m > 0)
								{
									Game.Sections[m].PreviousSection = m - 1;
									Game.Sections[m - 1].NextSection = m;
								}
								else
								{
									Game.Sections[m].PreviousSection = -1;
								}
								Game.Sections[m].NextSection = -1;
								Game.Sections[m].StationIndex = Data.Blocks[i].Section[k].DepartureStationIndex;
								Game.Sections[m].Invisible = Data.Blocks[i].Section[k].Invisible;
								Game.Sections[m].Trains = new TrainManager.Train[] { };
								// create section change event
								double d = Data.Blocks[i].Section[k].TrackPosition - StartingDistance;
								int p = TrackManager.CurrentTrack.Elements[n].Events.Length;
								Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, p + 1);
								TrackManager.CurrentTrack.Elements[n].Events[p] = new TrackManager.SectionChangeEvent(d, m - 1, m);
							}
							// transponders introduced after corresponding sections
							for (int l = 0; l < Data.Blocks[i].Transponder.Length; l++)
							{
								if (Data.Blocks[i].Transponder[l].Type != -1)
								{
									int t = Data.Blocks[i].Transponder[l].Section;
									if (t >= 0 & t < Game.Sections.Length)
									{
										int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
										Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
										double dt = Data.Blocks[i].Transponder[l].TrackPosition - StartingDistance;
										TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(dt, Data.Blocks[i].Transponder[l].Type, Data.Blocks[i].Transponder[l].Data, t, Data.Blocks[i].Transponder[l].ClipToFirstRedSection);
										Data.Blocks[i].Transponder[l].Type = -1;
									}
								}
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
										ObjectManager.CreateStaticObject(LimitPostInfinite, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
									}
									else
									{
										if (Data.Blocks[i].Limit[k].Cource < 0)
										{
											ObjectManager.CreateStaticObject(LimitPostLeft, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
										}
										else if (Data.Blocks[i].Limit[k].Cource > 0)
										{
											ObjectManager.CreateStaticObject(LimitPostRight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
										}
										else
										{
											ObjectManager.CreateStaticObject(LimitPostStraight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, false);
										}
										double lim = Data.Blocks[i].Limit[k].Speed / Data.UnitOfSpeed;
										if (lim < 10.0)
										{
											int d0 = (int)Math.Round(lim);
											int o = ObjectManager.CreateStaticObject(LimitOneDigit, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, true);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
										}
										else if (lim < 100.0)
										{
											int d1 = (int)Math.Round(lim);
											int d0 = d1 % 10;
											d1 /= 10;
											int o = ObjectManager.CreateStaticObject(LimitTwoDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, true);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 2)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[1].DaytimeTexture);
											}
										}
										else
										{
											int d2 = (int)Math.Round(lim);
											int d0 = d2 % 10;
											int d1 = (d2 / 10) % 10;
											d2 /= 100;
											int o = ObjectManager.CreateStaticObject(LimitThreeDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, Data.BlockInterval, tpos, b, true);
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 1)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d2 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[0].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 2)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d1 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[1].DaytimeTexture);
											}
											if (ObjectManager.Objects[o].Mesh.Materials.Length >= 3)
											{
												Textures.RegisterTexture(OpenBveApi.Path.CombineFile(LimitGraphicsPath, "limit_" + d0 + ".png"), out ObjectManager.Objects[o].Mesh.Materials[2].DaytimeTexture);
											}
										}
									}
								}
							}
						}
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
					}
				}
				// finalize block
				Position.X += Direction.X * c;
				Position.Y += h;
				Position.Z += Direction.Y * c;
				if (a != 0.0)
				{
					World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
				}
			}
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
							int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
							Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
							double d = Data.Blocks[i].Transponder[j].TrackPosition - TrackManager.CurrentTrack.Elements[n].StartingTrackPosition;
							int s = Data.Blocks[i].Transponder[j].Section;
							if (s >= 0) s = -1;
							TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(d, Data.Blocks[i].Transponder[j].Type, Data.Blocks[i].Transponder[j].Data, s, Data.Blocks[i].Transponder[j].ClipToFirstRedSection);
							Data.Blocks[i].Transponder[j].Type = -1;
						}
					}
					// Destination Change Events
					for (int j = 0; j < Data.Blocks[i].DestinationChanges.Length; j++)
					{
						int n = i - Data.FirstUsedBlock;
						int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
						double d = Data.Blocks[i].DestinationChanges[j].TrackPosition - TrackManager.CurrentTrack.Elements[n].StartingTrackPosition;
						TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.DestinationEvent(d, Data.Blocks[i].DestinationChanges[j].Type, Data.Blocks[i].DestinationChanges[j].NextDestination, Data.Blocks[i].DestinationChanges[j].PreviousDestination, Data.Blocks[i].DestinationChanges[j].TriggerOnce);
					}
				}
			}
			// insert station end events
			for (int i = 0; i < Game.Stations.Length; i++)
			{
				int j = Game.Stations[i].Stops.Length - 1;
				if (j >= 0)
				{
					double p = Game.Stations[i].Stops[j].TrackPosition + Game.Stations[i].Stops[j].ForwardTolerance + Data.BlockInterval;
					int k = (int)Math.Floor(p / (double)Data.BlockInterval) - Data.FirstUsedBlock;
					if (k >= 0 & k < Data.Blocks.Length)
					{
						double d = p - (double)(k + Data.FirstUsedBlock) * (double)Data.BlockInterval;
						int m = TrackManager.CurrentTrack.Elements[k].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[k].Events, m + 1);
						TrackManager.CurrentTrack.Elements[k].Events[m] = new TrackManager.StationEndEvent(d, i);
					}
				}
			}
			// create default point of interests
			if (Game.PointsOfInterest.Length == 0)
			{
				Game.PointsOfInterest = new OpenBve.Game.PointOfInterest[Game.Stations.Length];
				int n = 0;
				for (int i = 0; i < Game.Stations.Length; i++)
				{
					if (Game.Stations[i].Stops.Length != 0)
					{
						Game.PointsOfInterest[n].Text = Game.Stations[i].Name;
						Game.PointsOfInterest[n].TrackPosition = Game.Stations[i].Stops[0].TrackPosition;
						Game.PointsOfInterest[n].TrackOffset = new Vector3(0.0, 2.8, 0.0);
						if (Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors)
						{
							Game.PointsOfInterest[n].TrackOffset.X = -2.5;
						}
						else if (!Game.Stations[i].OpenLeftDoors & Game.Stations[i].OpenRightDoors)
						{
							Game.PointsOfInterest[n].TrackOffset.X = 2.5;
						}
						n++;
					}
				}
				Array.Resize<Game.PointOfInterest>(ref Game.PointsOfInterest, n);
			}
			// convert block-based cant into point-based cant
			for (int i = CurrentTrackLength - 1; i >= 1; i--)
			{
				if (TrackManager.CurrentTrack.Elements[i].CurveCant == 0.0)
				{
					TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i - 1].CurveCant;
				}
				else if (TrackManager.CurrentTrack.Elements[i - 1].CurveCant != 0.0)
				{
					if (Math.Sign(TrackManager.CurrentTrack.Elements[i - 1].CurveCant) == Math.Sign(TrackManager.CurrentTrack.Elements[i].CurveCant))
					{
						if (Math.Abs(TrackManager.CurrentTrack.Elements[i - 1].CurveCant) > Math.Abs(TrackManager.CurrentTrack.Elements[i].CurveCant))
						{
							TrackManager.CurrentTrack.Elements[i].CurveCant = TrackManager.CurrentTrack.Elements[i - 1].CurveCant;
						}
					}
					else
					{
						TrackManager.CurrentTrack.Elements[i].CurveCant = 0.5 * (TrackManager.CurrentTrack.Elements[i].CurveCant + TrackManager.CurrentTrack.Elements[i - 1].CurveCant);
					}
				}
			}
			// finalize
			Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, CurrentTrackLength);
			for (int i = 0; i < Game.Stations.Length; i++)
			{
				if (Game.Stations[i].Stops.Length == 0 & Game.Stations[i].StopMode != StationStopMode.AllPass)
				{
					Interface.AddMessage(Interface.MessageType.Warning, false, "Station " + Game.Stations[i].Name + " expects trains to stop but does not define stop points at track position " + Game.Stations[i].DefaultTrackPosition.ToString(Culture) + " in file " + FileName);
					Game.Stations[i].StopMode = StationStopMode.AllPass;
				}
				if (Game.Stations[i].Type == StationType.ChangeEnds)
				{
					if (i < Game.Stations.Length - 1)
					{
						if (Game.Stations[i + 1].StopMode != StationStopMode.AllStop)
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Station " + Game.Stations[i].Name + " is marked as \"change ends\" but the subsequent station does not expect all trains to stop in file " + FileName);
							Game.Stations[i + 1].StopMode = StationStopMode.AllStop;
						}
					}
					else
					{
						Interface.AddMessage(Interface.MessageType.Warning, false, "Station " + Game.Stations[i].Name + " is marked as \"change ends\" but there is no subsequent station defined in file " + FileName);
						Game.Stations[i].Type = StationType.Terminal;
					}
				}
			}
			if (Game.Stations.Length != 0)
			{
				Game.Stations[Game.Stations.Length - 1].Type = StationType.Terminal;
			}
			if (TrackManager.CurrentTrack.Elements.Length != 0)
			{
				int n = TrackManager.CurrentTrack.Elements.Length - 1;
				int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
				Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
				TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TrackEndEvent(Data.BlockInterval);
			}
			// insert compatibility beacons
			if (!PreviewOnly)
			{
				List<TrackManager.TransponderEvent> transponders = new List<TrackManager.TransponderEvent>();
				bool atc = false;
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
				{
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
					{
						if (!atc)
						{
							if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
							{
								TrackManager.StationStartEvent station = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Atc)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 0, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 1, 0, false);
									atc = true;
								}
							}
						}
						else
						{
							if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
							{
								TrackManager.StationStartEvent station = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Ats)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 2, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 3, 0, false);
								}
							}
							else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationEndEvent)
							{
								TrackManager.StationEndEvent station = (TrackManager.StationEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Atc)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 1, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 2, 0, false);
								}
								else if (Game.Stations[station.StationIndex].SafetySystem == Game.SafetySystem.Ats)
								{
									Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, TrackManager.CurrentTrack.Elements[i].Events.Length + 2);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 2] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 3, 0, false);
									TrackManager.CurrentTrack.Elements[i].Events[TrackManager.CurrentTrack.Elements[i].Events.Length - 1] = new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcTrackStatus, 0, 0, false);
									atc = false;
								}
							}
							else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.LimitChangeEvent)
							{
								TrackManager.LimitChangeEvent limit = (TrackManager.LimitChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
								int speed = (int)Math.Round(Math.Min(4095.0, 3.6 * limit.NextSpeedLimit));
								int distance = Math.Min(1048575, (int)Math.Round(TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + limit.TrackPositionDelta));
								unchecked
								{
									int value = (int)((uint)speed | ((uint)distance << 12));
									transponders.Add(new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtcSpeedLimit, value, 0, false));
								}
							}
						}
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.TransponderEvent)
						{
							TrackManager.TransponderEvent transponder = TrackManager.CurrentTrack.Elements[i].Events[j] as TrackManager.TransponderEvent;
							if (transponder.Type == TrackManager.SpecialTransponderTypes.InternalAtsPTemporarySpeedLimit)
							{
								int speed = Math.Min(4095, transponder.Data);
								int distance = Math.Min(1048575, (int)Math.Round(TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + transponder.TrackPositionDelta));
								unchecked
								{
									int value = (int)((uint)speed | ((uint)distance << 12));
									transponders.Add(new TrackManager.TransponderEvent(0.0, TrackManager.SpecialTransponderTypes.AtsPTemporarySpeedLimit, value, 0, false));
								}
							}
						}
					}
				}
				int n = TrackManager.CurrentTrack.Elements[0].Events.Length;
				Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[0].Events, n + transponders.Count);
				for (int i = 0; i < transponders.Count; i++)
				{
					TrackManager.CurrentTrack.Elements[0].Events[n + i] = transponders[i];
				}
			}
			// cant
			if (!PreviewOnly)
			{
				ComputeCantTangents();
				int subdivisions = (int)Math.Floor(Data.BlockInterval / 5.0);
				if (subdivisions >= 2)
				{
					SmoothenOutTurns(subdivisions);
					ComputeCantTangents();
				}
			}
		}

		// ------------------

		// compute cant tangents
		private static void ComputeCantTangents()
		{
			if (TrackManager.CurrentTrack.Elements.Length == 1)
			{
				TrackManager.CurrentTrack.Elements[0].CurveCantTangent = 0.0;
			}
			else if (TrackManager.CurrentTrack.Elements.Length != 0)
			{
				double[] deltas = new double[TrackManager.CurrentTrack.Elements.Length - 1];
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					deltas[i] = TrackManager.CurrentTrack.Elements[i + 1].CurveCant - TrackManager.CurrentTrack.Elements[i].CurveCant;
				}
				double[] tangents = new double[TrackManager.CurrentTrack.Elements.Length];
				tangents[0] = deltas[0];
				tangents[TrackManager.CurrentTrack.Elements.Length - 1] = deltas[TrackManager.CurrentTrack.Elements.Length - 2];
				for (int i = 1; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					tangents[i] = 0.5 * (deltas[i - 1] + deltas[i]);
				}
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					if (deltas[i] == 0.0)
					{
						tangents[i] = 0.0;
						tangents[i + 1] = 0.0;
					}
					else
					{
						double a = tangents[i] / deltas[i];
						double b = tangents[i + 1] / deltas[i];
						if (a * a + b * b > 9.0)
						{
							double t = 3.0 / Math.Sqrt(a * a + b * b);
							tangents[i] = t * a * deltas[i];
							tangents[i + 1] = t * b * deltas[i];
						}
					}
				}
				for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
				{
					TrackManager.CurrentTrack.Elements[i].CurveCantTangent = tangents[i];
				}
			}
		}

		// ------------------


		// smoothen out turns
		private static void SmoothenOutTurns(int subdivisions)
		{
			if (subdivisions < 2)
			{
				throw new InvalidOperationException();
			}
			// subdivide track
			int length = TrackManager.CurrentTrack.Elements.Length;
			int newLength = (length - 1) * subdivisions + 1;
			double[] midpointsTrackPositions = new double[newLength];
			Vector3[] midpointsWorldPositions = new Vector3[newLength];
			Vector3[] midpointsWorldDirections = new Vector3[newLength];
			Vector3[] midpointsWorldUps = new Vector3[newLength];
			Vector3[] midpointsWorldSides = new Vector3[newLength];
			double[] midpointsCant = new double[newLength];
			for (int i = 0; i < newLength; i++)
			{
				int m = i % subdivisions;
				if (m != 0)
				{
					int q = i / subdivisions;
					TrackManager.TrackFollower follower = new TrackManager.TrackFollower();
					double r = (double)m / (double)subdivisions;
					double p = (1.0 - r) * TrackManager.CurrentTrack.Elements[q].StartingTrackPosition + r * TrackManager.CurrentTrack.Elements[q + 1].StartingTrackPosition;
					follower.Update(-1.0, true, false);
					follower.Update(p, true, false);
					midpointsTrackPositions[i] = p;
					midpointsWorldPositions[i] = follower.WorldPosition;
					midpointsWorldDirections[i] = follower.WorldDirection;
					midpointsWorldUps[i] = follower.WorldUp;
					midpointsWorldSides[i] = follower.WorldSide;
					midpointsCant[i] = follower.CurveCant;
				}
			}
			Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, newLength);
			for (int i = length - 1; i >= 1; i--)
			{
				TrackManager.CurrentTrack.Elements[subdivisions * i] = TrackManager.CurrentTrack.Elements[i];
			}
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
			{
				int m = i % subdivisions;
				if (m != 0)
				{
					int q = i / subdivisions;
					int j = q * subdivisions;
					TrackManager.CurrentTrack.Elements[i] = TrackManager.CurrentTrack.Elements[j];
					TrackManager.CurrentTrack.Elements[i].Events = new TrackManager.GeneralEvent[] { };
					TrackManager.CurrentTrack.Elements[i].StartingTrackPosition = midpointsTrackPositions[i];
					TrackManager.CurrentTrack.Elements[i].WorldPosition = midpointsWorldPositions[i];
					TrackManager.CurrentTrack.Elements[i].WorldDirection = midpointsWorldDirections[i];
					TrackManager.CurrentTrack.Elements[i].WorldUp = midpointsWorldUps[i];
					TrackManager.CurrentTrack.Elements[i].WorldSide = midpointsWorldSides[i];
					TrackManager.CurrentTrack.Elements[i].CurveCant = midpointsCant[i];
					TrackManager.CurrentTrack.Elements[i].CurveCantTangent = 0.0;
				}
			}
			// find turns
			bool[] isTurn = new bool[TrackManager.CurrentTrack.Elements.Length];
			{
				TrackManager.TrackFollower follower = new TrackManager.TrackFollower();
				for (int i = 1; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
				{
					int m = i % subdivisions;
					if (m == 0)
					{
						double p = 0.00000001 * TrackManager.CurrentTrack.Elements[i - 1].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
						follower.Update(p, true, false);
						Vector3 d1 = TrackManager.CurrentTrack.Elements[i].WorldDirection;
						Vector3 d2 = follower.WorldDirection;
						Vector3 d = d1 - d2;
						double t = d.X * d.X + d.Z * d.Z;
						const double e = 0.0001;
						if (t > e)
						{
							isTurn[i] = true;
						}
					}
				}
			}
			// replace turns by curves
			double totalShortage = 0.0;
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
			{
				if (isTurn[i])
				{
					// estimate radius
					Vector3 AP = TrackManager.CurrentTrack.Elements[i - 1].WorldPosition;
					Vector3 AS = TrackManager.CurrentTrack.Elements[i - 1].WorldSide;
					Vector3 BP = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition;
					Vector3 BS = TrackManager.CurrentTrack.Elements[i + 1].WorldSide;
					Vector3 S = AS - BS;
					double rx;
					if (S.X * S.X > 0.000001)
					{
						rx = (BP.X - AP.X) / S.X;
					}
					else
					{
						rx = 0.0;
					}
					double rz;
					if (S.Z * S.Z > 0.000001)
					{
						rz = (BP.Z - AP.Z) / S.Z;
					}
					else
					{
						rz = 0.0;
					}
					if (rx != 0.0 | rz != 0.0)
					{
						double r;
						if (rx != 0.0 & rz != 0.0)
						{
							if (Math.Sign(rx) == Math.Sign(rz))
							{
								double f = rx / rz;
								if (f > -1.1 & f < -0.9 | f > 0.9 & f < 1.1)
								{
									r = Math.Sqrt(Math.Abs(rx * rz)) * Math.Sign(rx);
								}
								else
								{
									r = 0.0;
								}
							}
							else
							{
								r = 0.0;
							}
						}
						else if (rx != 0.0)
						{
							r = rx;
						}
						else
						{
							r = rz;
						}
						if (r * r > 1.0)
						{
							// apply radius
							TrackManager.TrackFollower follower = new TrackManager.TrackFollower();
							TrackManager.CurrentTrack.Elements[i - 1].CurveRadius = r;
							double p = 0.00000001 * TrackManager.CurrentTrack.Elements[i - 1].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
							TrackManager.CurrentTrack.Elements[i].CurveRadius = r;
							TrackManager.CurrentTrack.Elements[i].WorldPosition = follower.WorldPosition;
							TrackManager.CurrentTrack.Elements[i].WorldDirection = follower.WorldDirection;
							TrackManager.CurrentTrack.Elements[i].WorldUp = follower.WorldUp;
							TrackManager.CurrentTrack.Elements[i].WorldSide = follower.WorldSide;
							// iterate to shorten track element length
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
							Vector3 d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
							double bestT = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
							int bestJ = 0;
							int n = 1000;
							double a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
							for (int j = 1; j < n - 1; j++)
							{
								follower.Update(TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
								double t = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
								if (t < bestT)
								{
									bestT = t;
									bestJ = j;
								}
								else
								{
									break;
								}
							}
							double s = (double)bestJ * a;
							for (int j = i + 1; j < TrackManager.CurrentTrack.Elements.Length; j++)
							{
								TrackManager.CurrentTrack.Elements[j].StartingTrackPosition -= s;
							}
							totalShortage += s;
							// introduce turn to compensate for curve
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
							Vector3 AB = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
							Vector3 AC = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							Vector3 BC = follower.WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double sa = Math.Sqrt(BC.X * BC.X + BC.Z * BC.Z);
							double sb = Math.Sqrt(AC.X * AC.X + AC.Z * AC.Z);
							double sc = Math.Sqrt(AB.X * AB.X + AB.Z * AB.Z);
							double denominator = 2.0 * sa * sb;
							if (denominator != 0.0)
							{
								double originalAngle;
								{
									double value = (sa * sa + sb * sb - sc * sc) / denominator;
									if (value < -1.0)
									{
										originalAngle = Math.PI;
									}
									else if (value > 1.0)
									{
										originalAngle = 0;
									}
									else
									{
										originalAngle = Math.Acos(value);
									}
								}
								TrackManager.TrackElement originalTrackElement = TrackManager.CurrentTrack.Elements[i];
								bestT = double.MaxValue;
								bestJ = 0;
								for (int j = -1; j <= 1; j++)
								{
									double g = (double)j * originalAngle;
									double cosg = Math.Cos(g);
									double sing = Math.Sin(g);
									TrackManager.CurrentTrack.Elements[i] = originalTrackElement;
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldDirection, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldUp, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldSide, 0.0, 1.0, 0.0, cosg, sing);
									p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
									follower.Update(p - 1.0, true, false);
									follower.Update(p, true, false);
									d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
									double t = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
									if (t < bestT)
									{
										bestT = t;
										bestJ = j;
									}
								}
								{
									double newAngle = (double)bestJ * originalAngle;
									double cosg = Math.Cos(newAngle);
									double sing = Math.Sin(newAngle);
									TrackManager.CurrentTrack.Elements[i] = originalTrackElement;
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldDirection, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldUp, 0.0, 1.0, 0.0, cosg, sing);
									World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldSide, 0.0, 1.0, 0.0, cosg, sing);
								}
								// iterate again to further shorten track element length
								p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
								follower.Update(p - 1.0, true, false);
								follower.Update(p, true, false);
								d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
								bestT = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
								bestJ = 0;
								n = 1000;
								a = 1.0 / (double)n * (TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - TrackManager.CurrentTrack.Elements[i].StartingTrackPosition);
								for (int j = 1; j < n - 1; j++)
								{
									follower.Update(TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition - (double)j * a, true, false);
									d = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - follower.WorldPosition;
									double t = d.X * d.X + d.Y * d.Y + d.Z * d.Z;
									if (t < bestT)
									{
										bestT = t;
										bestJ = j;
									}
									else
									{
										break;
									}
								}
								s = (double)bestJ * a;
								for (int j = i + 1; j < TrackManager.CurrentTrack.Elements.Length; j++)
								{
									TrackManager.CurrentTrack.Elements[j].StartingTrackPosition -= s;
								}
								totalShortage += s;
							}
							// compensate for height difference
							p = 0.00000001 * TrackManager.CurrentTrack.Elements[i].StartingTrackPosition + 0.99999999 * TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
							follower.Update(p - 1.0, true, false);
							follower.Update(p, true, false);
							Vector3 d1 = TrackManager.CurrentTrack.Elements[i + 1].WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double a1 = Math.Atan(d1.Y / Math.Sqrt(d1.X * d1.X + d1.Z * d1.Z));
							Vector3 d2 = follower.WorldPosition - TrackManager.CurrentTrack.Elements[i].WorldPosition;
							double a2 = Math.Atan(d2.Y / Math.Sqrt(d2.X * d2.X + d2.Z * d2.Z));
							double b = a2 - a1;
							if (b * b > 0.00000001)
							{
								double cosa = Math.Cos(b);
								double sina = Math.Sin(b);
								World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldDirection, TrackManager.CurrentTrack.Elements[i].WorldSide, cosa, sina);
								World.Rotate(ref TrackManager.CurrentTrack.Elements[i].WorldUp, TrackManager.CurrentTrack.Elements[i].WorldSide, cosa, sina);
							}
						}
					}
				}
			}
			// correct events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length - 1; i++)
			{
				double startingTrackPosition = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double endingTrackPosition = TrackManager.CurrentTrack.Elements[i + 1].StartingTrackPosition;
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
				{
					double p = startingTrackPosition + TrackManager.CurrentTrack.Elements[i].Events[j].TrackPositionDelta;
					if (p >= endingTrackPosition)
					{
						int len = TrackManager.CurrentTrack.Elements[i + 1].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i + 1].Events, len + 1);
						TrackManager.CurrentTrack.Elements[i + 1].Events[len] = TrackManager.CurrentTrack.Elements[i].Events[j];
						TrackManager.CurrentTrack.Elements[i + 1].Events[len].TrackPositionDelta += startingTrackPosition - endingTrackPosition;
						for (int k = j; k < TrackManager.CurrentTrack.Elements[i].Events.Length - 1; k++)
						{
							TrackManager.CurrentTrack.Elements[i].Events[k] = TrackManager.CurrentTrack.Elements[i].Events[k + 1];
						}
						len = TrackManager.CurrentTrack.Elements[i].Events.Length;
						Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[i].Events, len - 1);
						j--;
					}
				}
			}
		}
	}
}
