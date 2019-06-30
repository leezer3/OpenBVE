﻿// ╔═════════════════════════════════════════════════════════════╗
// ║ TrackManager.cs for the Route Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBve.RouteManager;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using SoundManager;

namespace OpenBve {
    internal static class TrackManager {

        // background change
        internal class BackgroundChangeEvent : GeneralEvent<TrainManager.Train> {
            internal BackgroundHandle PreviousBackground;
            internal BackgroundHandle NextBackground;
            internal BackgroundChangeEvent(double TrackPositionDelta, BackgroundHandle PreviousBackground, BackgroundHandle NextBackground) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousBackground = PreviousBackground;
                this.NextBackground = NextBackground;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        CurrentRoute.TargetBackground = this.PreviousBackground;
                        World.TargetBackgroundCountdown = World.TargetBackgroundDefaultCountdown;
                    } else if (Direction > 0) {
                        CurrentRoute.TargetBackground = this.NextBackground;
                        World.TargetBackgroundCountdown = World.TargetBackgroundDefaultCountdown;
                    }
                }
            }
        }
        // fog change
        internal class FogChangeEvent : GeneralEvent<TrainManager.Train> {
            internal Fog PreviousFog;
            internal Fog CurrentFog;
            internal Fog NextFog;
            internal FogChangeEvent(double TrackPositionDelta, Fog PreviousFog, Fog CurrentFog, Fog NextFog) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousFog = PreviousFog;
                this.CurrentFog = CurrentFog;
                this.NextFog = NextFog;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        CurrentRoute.PreviousFog = this.PreviousFog;
                        CurrentRoute.NextFog = this.CurrentFog;
                    } else if (Direction > 0) {
                        CurrentRoute.PreviousFog = this.CurrentFog;
                        CurrentRoute.NextFog = this.NextFog;
                    }
                }
            }
        }
        // brightness change
        internal class BrightnessChangeEvent : GeneralEvent<TrainManager.Train> {
            internal float CurrentBrightness;
            internal float PreviousBrightness;
            internal double PreviousDistance;
            internal float NextBrightness;
            internal double NextDistance;
            internal BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance, float NextBrightness, double NextDistance) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.CurrentBrightness = CurrentBrightness;
                this.PreviousBrightness = PreviousBrightness;
                this.PreviousDistance = PreviousDistance;
                this.NextBrightness = NextBrightness;
                this.NextDistance = NextDistance;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }
        // marker start
        internal class MarkerStartEvent : GeneralEvent<TrainManager.Train> {
            internal Texture Texture;
            internal MarkerStartEvent(double TrackPositionDelta, Texture Texture) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.Texture = Texture;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        Game.RemoveMarker(this.Texture);
                    } else if (Direction > 0) {
                        Game.AddMarker(this.Texture);
                    }
                }
            }
        }
        // marker end
        internal class MarkerEndEvent : GeneralEvent<TrainManager.Train> {
            internal Texture Texture;
            internal MarkerEndEvent(double TrackPositionDelta, Texture Texture) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.Texture = Texture;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        Game.AddMarker(this.Texture);
                    } else if (Direction > 0) {
                        Game.RemoveMarker(this.Texture);
                    }
                }
            }
        }
        // station pass alarm
        internal class StationPassAlarmEvent : GeneralEvent<TrainManager.Train> {
            internal StationPassAlarmEvent(double TrackPositionDelta) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }
        // station start
        internal class StationStartEvent : GeneralEvent<TrainManager.Train> {
            internal int StationIndex;
            internal StationStartEvent(double TrackPositionDelta, int StationIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.StationIndex = StationIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        if (Program.CurrentStation == this.StationIndex) {
                            Program.CurrentStation = -1;
                        }
                    } else if (Direction > 0) {
                        Program.CurrentStation = this.StationIndex;
                    }
                }
            }
        }
        // station end
        internal class StationEndEvent : GeneralEvent<TrainManager.Train> {
            internal int StationIndex;
            internal StationEndEvent(double TrackPositionDelta, int StationIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.StationIndex = StationIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        Program.CurrentStation = this.StationIndex;
                    } else if (Direction > 0) {
                        if (Program.CurrentStation == this.StationIndex) {
                            Program.CurrentStation = -1;
                        }
                    }
                }
            }
        }
        // section change
        internal class SectionChangeEvent : GeneralEvent<TrainManager.Train> {
            internal int PreviousSectionIndex;
            internal int NextSectionIndex;
            internal SectionChangeEvent(double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousSectionIndex = PreviousSectionIndex;
                this.NextSectionIndex = NextSectionIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }
        // transponder
		internal enum TransponderType {
			None = -1,
			SLong = 0,
			SN = 1,
			AccidentalDeparture = 2,
			AtsPPatternOrigin = 3,
			AtsPImmediateStop = 4,
			AtsPTemporarySpeedRestriction = -2,
			AtsPPermanentSpeedRestriction = -3
		}
        internal enum TransponderSpecialSection {
            NextRedSection = -2,
        }
        internal class TransponderEvent : GeneralEvent<TrainManager.Train> {
            internal TransponderType Type;
            internal bool SwitchSubsystem;
            internal int OptionalInteger;
            internal double OptionalFloat;
            internal int SectionIndex;
            internal TransponderEvent(double TrackPositionDelta, TransponderType Type, bool SwitchSubsystem, int OptionalInteger, double OptionalFloat, int SectionIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.Type = Type;
                this.SwitchSubsystem = SwitchSubsystem;
                this.OptionalInteger = OptionalInteger;
                this.OptionalFloat = OptionalFloat;
                this.SectionIndex = SectionIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }
        // limit change
        internal class LimitChangeEvent : GeneralEvent<TrainManager.Train> {
            internal double PreviousSpeedLimit;
            internal double NextSpeedLimit;
            internal LimitChangeEvent(double TrackPositionDelta, double PreviousSpeedLimit, double NextSpeedLimit) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousSpeedLimit = PreviousSpeedLimit;
                this.NextSpeedLimit = NextSpeedLimit;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }
        // sound
        internal static bool SuppressSoundEvents = false;
        internal class SoundEvent : GeneralEvent<TrainManager.Train> {
            internal SoundsBase.SoundBuffer SoundBuffer;
            internal bool PlayerTrainOnly;
            internal bool Once;
            internal bool Dynamic;
            internal Vector3 Position;
            internal double Speed;
            internal SoundEvent(double TrackPositionDelta, SoundsBase.SoundBuffer SoundBuffer, bool PlayerTrainOnly, bool Once, bool Dynamic, Vector3 Position, double Speed) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.SoundBuffer = SoundBuffer;
                this.PlayerTrainOnly = PlayerTrainOnly;
                this.Once = Once;
                this.Dynamic = Dynamic;
                this.Position = Position;
                this.Speed = Speed;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
            internal const int SoundIndexTrainPoint = -2;
        }
        // rail sounds change
        internal class RailSoundsChangeEvent : GeneralEvent<TrainManager.Train> {
            internal int PreviousRunIndex;
            internal int PreviousFlangeIndex;
            internal int NextRunIndex;
            internal int NextFlangeIndex;
            internal RailSoundsChangeEvent(double TrackPositionDelta, int PreviousRunIndex, int PreviousFlangeIndex, int NextRunIndex, int NextFlangeIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousRunIndex = PreviousRunIndex;
                this.PreviousFlangeIndex = PreviousFlangeIndex;
                this.NextRunIndex = NextRunIndex;
                this.NextFlangeIndex = NextFlangeIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }
        // track end
        internal class TrackEndEvent : GeneralEvent<TrainManager.Train> {
            internal TrackEndEvent(double TrackPositionDelta) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) { }
        }

        // ================================

        // track element
		internal struct TrackElement {
			internal double StartingTrackPosition;
			internal double Pitch;
			internal double CurveRadius;
			internal double CurveCant;
			internal double CurveCantTangent;
			internal double AdhesionMultiplier;
			internal double CsvRwAccuracyLevel;
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
			internal object[] Events;
			internal TrackElement(double StartingTrackPosition) {
				this.StartingTrackPosition = StartingTrackPosition;
				this.Pitch = 0.0;
				this.CurveRadius = 0.0;
				this.CurveCant = 0.0;
				this.CurveCantTangent = 0.0;
				this.AdhesionMultiplier = 1.0;
				this.CsvRwAccuracyLevel = 2.0;
				this.WorldPosition = Vector3.Zero;
				this.WorldDirection = Vector3.Forward;
				this.WorldUp = Vector3.Down;
				this.WorldSide = Vector3.Right;
				this.Events = new object[] { };
			}
		}

        // track
        internal class Track {
            internal TrackElement[] Elements;

            internal double RailGauge = 1.435;

            /// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
            /// <param name="position">The track position</param>
            /// <param name="inaccuracy">The openBVE innacuaracy value</param>
            /// <param name="x">The X (horizontal) co-ordinate to update</param>
            /// <param name="y">The Y (vertical) co-ordinate to update</param>
            /// <param name="c">???</param>
            internal void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c)
            {
	            if (inaccuracy <= 0.0)
	            {
		            x = 0.0;
		            y = 0.0;
		            c = 0.0;
	            }
	            else
	            {
		            double z = System.Math.Pow(0.25 * inaccuracy, 1.2) * position;
		            x = 0.14 * System.Math.Sin(0.5843 * z) + 0.82 * System.Math.Sin(0.2246 * z) + 0.55 * System.Math.Sin(0.1974 * z);
		            x *= 0.0035 * RailGauge * inaccuracy;
		            y = 0.18 * System.Math.Sin(0.5172 * z) + 0.37 * System.Math.Sin(0.3251 * z) + 0.91 * System.Math.Sin(0.3773 * z);
		            y *= 0.0020 * RailGauge * inaccuracy;
		            c = 0.23 * System.Math.Sin(0.3131 * z) + 0.54 * System.Math.Sin(0.5807 * z) + 0.81 * System.Math.Sin(0.3621 * z);
		            c *= 0.0025 * RailGauge * inaccuracy;
	            }
            }
        }
        internal static Track CurrentTrack;

		// track follower
		internal struct TrackFollower {
			internal int LastTrackElement;
			internal double TrackPosition;
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
			internal double CurveRadius;
			internal double CurveCant;
			internal double Pitch;
			internal double CantDueToInaccuracy;
			internal double AdhesionMultiplier;
			internal EventTriggerType TriggerType;
			internal TrainManager.Train Train;
			internal int CarIndex;
		}
		internal static void UpdateTrackFollower(ref TrackFollower Follower, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) {
			if (CurrentTrack.Elements == null || CurrentTrack.Elements.Length == 0) return;
			int i = Follower.LastTrackElement;
			while (i >= 0 && NewTrackPosition < CurrentTrack.Elements[i].StartingTrackPosition) {
				double ta = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
				double tb = -0.01;
				CheckEvents(ref Follower, i, -1, ta, tb);
				i--;
			}
			if (i >= 0) {
				while (i < CurrentTrack.Elements.Length - 1) {
					if (NewTrackPosition < CurrentTrack.Elements[i + 1].StartingTrackPosition) break;
					double ta = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
					double tb = CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition + 0.01;
					CheckEvents(ref Follower, i, 1, ta, tb);
					i++;
				}
			} else {
				i = 0;
			}
			double da = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
			double db = NewTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
			// track
			if (UpdateWorldCoordinates) {
				if (db != 0.0) {
					if (CurrentTrack.Elements[i].CurveRadius != 0.0) {
						// curve
						double r = CurrentTrack.Elements[i].CurveRadius;
						double p = CurrentTrack.Elements[i].WorldDirection.Y / Math.Sqrt(CurrentTrack.Elements[i].WorldDirection.X * CurrentTrack.Elements[i].WorldDirection.X + CurrentTrack.Elements[i].WorldDirection.Z * CurrentTrack.Elements[i].WorldDirection.Z);
						double s = db / Math.Sqrt(1.0 + p * p);
						double h = s * p;
						double b = s / Math.Abs(r);
						double f = 2.0 * r * r * (1.0 - Math.Cos(b));
						double c = (double)Math.Sign(db) * Math.Sqrt(f >= 0.0 ? f : 0.0);
						double a = 0.5 * (double)Math.Sign(r) * b;
						Vector3 D = new Vector3(CurrentTrack.Elements[i].WorldDirection.X, 0.0, CurrentTrack.Elements[i].WorldDirection.Z);
						D.Normalize();
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						D.Rotate(Vector3.Down, cosa, sina);
						Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + c * D.X;
						Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + h;
						Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + c * D.Z;
						D.Rotate(Vector3.Down, cosa, sina);
						Follower.WorldDirection.X = D.X;
						Follower.WorldDirection.Y = p;
						Follower.WorldDirection.Z = D.Z;
						Follower.WorldDirection.Normalize();
						double cos2a = Math.Cos(2.0 * a);
						double sin2a = Math.Sin(2.0 * a);
						Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
						Follower.WorldSide.Rotate(Vector3.Down, cos2a, sin2a);
						Follower.WorldUp = Vector3.Cross(Follower.WorldDirection, Follower.WorldSide);
						Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					} else {
						// straight
						Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + db * CurrentTrack.Elements[i].WorldDirection.X;
						Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + db * CurrentTrack.Elements[i].WorldDirection.Y;
						Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + db * CurrentTrack.Elements[i].WorldDirection.Z;
						Follower.WorldDirection = CurrentTrack.Elements[i].WorldDirection;
						Follower.WorldUp = CurrentTrack.Elements[i].WorldUp;
						Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
						Follower.CurveRadius = 0.0;
					}
					// cant
					if (i < CurrentTrack.Elements.Length - 1) {
						double t = db / (CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition);
						if (t < 0.0) {
							t = 0.0;
						} else if (t > 1.0) {
							t = 1.0;
						}
						double t2 = t * t;
						double t3 = t2 * t;
						Follower.CurveCant =
							(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentTrack.Elements[i].CurveCant +
							(t3 - 2.0 * t2 + t) * CurrentTrack.Elements[i].CurveCantTangent +
							(-2.0 * t3 + 3.0 * t2) * CurrentTrack.Elements[i + 1].CurveCant +
							(t3 - t2) * CurrentTrack.Elements[i + 1].CurveCantTangent;
					} else {
						Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
					}
				} else {
					Follower.WorldPosition = CurrentTrack.Elements[i].WorldPosition;
					Follower.WorldDirection = CurrentTrack.Elements[i].WorldDirection;
					Follower.WorldUp = CurrentTrack.Elements[i].WorldUp;
					Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
					Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
				}
			} else {
				if (db != 0.0) {
					if (CurrentTrack.Elements[i].CurveRadius != 0.0) {
						Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					} else {
						Follower.CurveRadius = 0.0;
					}
					if (i < CurrentTrack.Elements.Length - 1) {
						double t = db / (CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition);
						if (t < 0.0) {
							t = 0.0;
						} else if (t > 1.0) {
							t = 1.0;
						}
						double t2 = t * t;
						double t3 = t2 * t;
						Follower.CurveCant =
							(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentTrack.Elements[i].CurveCant +
							(t3 - 2.0 * t2 + t) * CurrentTrack.Elements[i].CurveCantTangent +
							(-2.0 * t3 + 3.0 * t2) * CurrentTrack.Elements[i + 1].CurveCant +
							(t3 - t2) * CurrentTrack.Elements[i + 1].CurveCantTangent;
					} else {
						Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
					}
				} else {
					Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
				}
			}
			Follower.AdhesionMultiplier = CurrentTrack.Elements[i].AdhesionMultiplier;
			Follower.Pitch = CurrentTrack.Elements[i].Pitch * 1000;
			// inaccuracy
			if (AddTrackInaccurary) {
				double x, y, c;
				if (i < CurrentTrack.Elements.Length - 1) {
					double t = db / (CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition);
					if (t < 0.0) {
						t = 0.0;
					} else if (t > 1.0) {
						t = 1.0;
					}
					double x1, y1, c1;
					double x2, y2, c2;
					CurrentTrack.GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
					CurrentTrack.GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
					x = (1.0 - t) * x1 + t * x2;
					y = (1.0 - t) * y1 + t * y2;
					c = (1.0 - t) * c1 + t * c2;
				} else {
					CurrentTrack.GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
				}
				Follower.WorldPosition.X += x * Follower.WorldSide.X + y * Follower.WorldUp.X;
				Follower.WorldPosition.Y += x * Follower.WorldSide.Y + y * Follower.WorldUp.Y;
				Follower.WorldPosition.Z += x * Follower.WorldSide.Z + y * Follower.WorldUp.Z;
				Follower.CurveCant += c;
				Follower.CantDueToInaccuracy = c;
			} else {
				Follower.CantDueToInaccuracy = 0.0;
			}
			// events
			CheckEvents(ref Follower, i, Math.Sign(db - da), da, db);
			// finish
			Follower.TrackPosition = NewTrackPosition;
			Follower.LastTrackElement = i;
		}
		
		// check events
        private static void CheckEvents(ref TrackFollower Follower, int ElementIndex, int Direction, double OldDelta, double NewDelta) {
            if (Direction < 0) {
                for (int j = 0; j < CurrentTrack.Elements[ElementIndex].Events.Length; j++)
                {
	                dynamic e = CurrentTrack.Elements[ElementIndex].Events[j];
                    if (OldDelta > e.TrackPositionDelta & NewDelta <= e.TrackPositionDelta) {
                        e.TryTrigger(-1, Follower.TriggerType, Follower.Train, Follower.CarIndex);
                    }
                }
            } else if (Direction > 0) {
                for (int j = 0; j < CurrentTrack.Elements[ElementIndex].Events.Length; j++)
                {
	                dynamic e = CurrentTrack.Elements[ElementIndex].Events[j];
                    if (OldDelta < e.TrackPositionDelta & NewDelta >= e.TrackPositionDelta) {
                        e.TryTrigger(1, Follower.TriggerType, Follower.Train, Follower.CarIndex);
                    }
                }
            }
        }

    }
}
