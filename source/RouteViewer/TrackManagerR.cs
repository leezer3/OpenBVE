// ╔═════════════════════════════════════════════════════════════╗
// ║ TrackManager.cs for the Route Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBve.RouteManager;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve {
    internal static class TrackManager {
		
	    // station pass alarm
        internal class StationPassAlarmEvent : GeneralEvent<TrainManager.Train, AbstractCar> {
            internal StationPassAlarmEvent(double TrackPositionDelta) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, AbstractCar Car) { }
        }
        // station end
        internal class StationEndEvent : GeneralEvent<TrainManager.Train, AbstractCar> {
            internal int StationIndex;
            internal StationEndEvent(double TrackPositionDelta, int StationIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.StationIndex = StationIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, AbstractCar Car) {
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
        internal class SectionChangeEvent : GeneralEvent<TrainManager.Train, AbstractCar> {
            internal int PreviousSectionIndex;
            internal int NextSectionIndex;
            internal SectionChangeEvent(double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousSectionIndex = PreviousSectionIndex;
                this.NextSectionIndex = NextSectionIndex;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, AbstractCar Car) { }
        }
    
        // sound
        internal static bool SuppressSoundEvents = false;
        internal class SoundEvent : GeneralEvent<TrainManager.Train, AbstractCar> {
            internal SoundBuffer SoundBuffer;
            internal bool PlayerTrainOnly;
            internal bool Once;
            internal bool Dynamic;
            internal Vector3 Position;
            internal double Speed;
            internal SoundEvent(double TrackPositionDelta, SoundBuffer SoundBuffer, bool PlayerTrainOnly, bool Once, bool Dynamic, Vector3 Position, double Speed) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.SoundBuffer = SoundBuffer;
                this.PlayerTrainOnly = PlayerTrainOnly;
                this.Once = Once;
                this.Dynamic = Dynamic;
                this.Position = Position;
                this.Speed = Speed;
            }
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, AbstractCar Car) { }
            internal const int SoundIndexTrainPoint = -2;
        }
        // rail sounds change
        internal class RailSoundsChangeEvent : GeneralEvent<TrainManager.Train, AbstractCar> {
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
            public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, AbstractCar Car) { }
        }
        // ================================


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

			internal void UpdateAbsolute(double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) {
				if (CurrentRoute.Tracks[0].Elements == null || CurrentRoute.Tracks[0].Elements.Length == 0) return;
				int i = LastTrackElement;
				while (i >= 0 && NewTrackPosition < CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition) {
					double ta = TrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
					double tb = -0.01;
					CheckEvents(i, -1, ta, tb);
					i--;
				}
				if (i >= 0) {
					while (i < CurrentRoute.Tracks[0].Elements.Length - 1) {
						if (NewTrackPosition < CurrentRoute.Tracks[0].Elements[i + 1].StartingTrackPosition) break;
						double ta = TrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
						double tb = CurrentRoute.Tracks[0].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition + 0.01;
						CheckEvents(i, 1, ta, tb);
						i++;
					}
				} else {
					i = 0;
				}
				double da = TrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
				double db = NewTrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
				// track
				if (UpdateWorldCoordinates) {
					if (db != 0.0) {
						if (CurrentRoute.Tracks[0].Elements[i].CurveRadius != 0.0) {
							// curve
							double r = CurrentRoute.Tracks[0].Elements[i].CurveRadius;
							double p = CurrentRoute.Tracks[0].Elements[i].WorldDirection.Y / Math.Sqrt(CurrentRoute.Tracks[0].Elements[i].WorldDirection.X * CurrentRoute.Tracks[0].Elements[i].WorldDirection.X + CurrentRoute.Tracks[0].Elements[i].WorldDirection.Z * CurrentRoute.Tracks[0].Elements[i].WorldDirection.Z);
							double s = db / Math.Sqrt(1.0 + p * p);
							double h = s * p;
							double b = s / Math.Abs(r);
							double f = 2.0 * r * r * (1.0 - Math.Cos(b));
							double c = (double)Math.Sign(db) * Math.Sqrt(f >= 0.0 ? f : 0.0);
							double a = 0.5 * (double)Math.Sign(r) * b;
							Vector3 D = new Vector3(CurrentRoute.Tracks[0].Elements[i].WorldDirection.X, 0.0, CurrentRoute.Tracks[0].Elements[i].WorldDirection.Z);
							D.Normalize();
							double cosa = Math.Cos(a);
							double sina = Math.Sin(a);
							D.Rotate(Vector3.Down, cosa, sina);
							WorldPosition.X = CurrentRoute.Tracks[0].Elements[i].WorldPosition.X + c * D.X;
							WorldPosition.Y = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y + h;
							WorldPosition.Z = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Z + c * D.Z;
							D.Rotate(Vector3.Down, cosa, sina);
							WorldDirection.X = D.X;
							WorldDirection.Y = p;
							WorldDirection.Z = D.Z;
							WorldDirection.Normalize();
							double cos2a = Math.Cos(2.0 * a);
							double sin2a = Math.Sin(2.0 * a);
							WorldSide = CurrentRoute.Tracks[0].Elements[i].WorldSide;
							WorldSide.Rotate(Vector3.Down, cos2a, sin2a);
							WorldUp = Vector3.Cross(WorldDirection, WorldSide);
							CurveRadius = CurrentRoute.Tracks[0].Elements[i].CurveRadius;
						} else {
							// straight
							WorldPosition.X = CurrentRoute.Tracks[0].Elements[i].WorldPosition.X + db * CurrentRoute.Tracks[0].Elements[i].WorldDirection.X;
							WorldPosition.Y = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y + db * CurrentRoute.Tracks[0].Elements[i].WorldDirection.Y;
							WorldPosition.Z = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Z + db * CurrentRoute.Tracks[0].Elements[i].WorldDirection.Z;
							WorldDirection = CurrentRoute.Tracks[0].Elements[i].WorldDirection;
							WorldUp = CurrentRoute.Tracks[0].Elements[i].WorldUp;
							WorldSide = CurrentRoute.Tracks[0].Elements[i].WorldSide;
							CurveRadius = 0.0;
						}
						// cant
						if (i < CurrentRoute.Tracks[0].Elements.Length - 1) {
							double t = db / (CurrentRoute.Tracks[0].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition);
							if (t < 0.0) {
								t = 0.0;
							} else if (t > 1.0) {
								t = 1.0;
							}
							double t2 = t * t;
							double t3 = t2 * t;
							CurveCant =
								(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentRoute.Tracks[0].Elements[i].CurveCant +
								(t3 - 2.0 * t2 + t) * CurrentRoute.Tracks[0].Elements[i].CurveCantTangent +
								(-2.0 * t3 + 3.0 * t2) * CurrentRoute.Tracks[0].Elements[i + 1].CurveCant +
								(t3 - t2) * CurrentRoute.Tracks[0].Elements[i + 1].CurveCantTangent;
						} else {
							CurveCant = CurrentRoute.Tracks[0].Elements[i].CurveCant;
						}
					} else {
						WorldPosition = CurrentRoute.Tracks[0].Elements[i].WorldPosition;
						WorldDirection = CurrentRoute.Tracks[0].Elements[i].WorldDirection;
						WorldUp = CurrentRoute.Tracks[0].Elements[i].WorldUp;
						WorldSide = CurrentRoute.Tracks[0].Elements[i].WorldSide;
						CurveRadius = CurrentRoute.Tracks[0].Elements[i].CurveRadius;
						CurveCant = CurrentRoute.Tracks[0].Elements[i].CurveCant;
					}
				} else {
					if (db != 0.0) {
						if (CurrentRoute.Tracks[0].Elements[i].CurveRadius != 0.0) {
							CurveRadius = CurrentRoute.Tracks[0].Elements[i].CurveRadius;
						} else {
							CurveRadius = 0.0;
						}
						if (i < CurrentRoute.Tracks[0].Elements.Length - 1) {
							double t = db / (CurrentRoute.Tracks[0].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition);
							if (t < 0.0) {
								t = 0.0;
							} else if (t > 1.0) {
								t = 1.0;
							}
							double t2 = t * t;
							double t3 = t2 * t;
							CurveCant =
								(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentRoute.Tracks[0].Elements[i].CurveCant +
								(t3 - 2.0 * t2 + t) * CurrentRoute.Tracks[0].Elements[i].CurveCantTangent +
								(-2.0 * t3 + 3.0 * t2) * CurrentRoute.Tracks[0].Elements[i + 1].CurveCant +
								(t3 - t2) * CurrentRoute.Tracks[0].Elements[i + 1].CurveCantTangent;
						} else {
							CurveCant = CurrentRoute.Tracks[0].Elements[i].CurveCant;
						}
					} else {
						CurveRadius = CurrentRoute.Tracks[0].Elements[i].CurveRadius;
						CurveCant = CurrentRoute.Tracks[0].Elements[i].CurveCant;
					}
				}
				AdhesionMultiplier = CurrentRoute.Tracks[0].Elements[i].AdhesionMultiplier;
				Pitch = CurrentRoute.Tracks[0].Elements[i].Pitch * 1000;
				// inaccuracy
				if (AddTrackInaccurary) {
					double x, y, c;
					if (i < CurrentRoute.Tracks[0].Elements.Length - 1) {
						double t = db / (CurrentRoute.Tracks[0].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition);
						if (t < 0.0) {
							t = 0.0;
						} else if (t > 1.0) {
							t = 1.0;
						}
						double x1, y1, c1;
						double x2, y2, c2;
						CurrentRoute.Tracks[0].GetInaccuracies(NewTrackPosition, CurrentRoute.Tracks[0].Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
						CurrentRoute.Tracks[0].GetInaccuracies(NewTrackPosition, CurrentRoute.Tracks[0].Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
						x = (1.0 - t) * x1 + t * x2;
						y = (1.0 - t) * y1 + t * y2;
						c = (1.0 - t) * c1 + t * c2;
					} else {
						CurrentRoute.Tracks[0].GetInaccuracies(NewTrackPosition, CurrentRoute.Tracks[0].Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
					}
					WorldPosition.X += x * WorldSide.X + y * WorldUp.X;
					WorldPosition.Y += x * WorldSide.Y + y * WorldUp.Y;
					WorldPosition.Z += x * WorldSide.Z + y * WorldUp.Z;
					CurveCant += c;
					CantDueToInaccuracy = c;
				} else {
					CantDueToInaccuracy = 0.0;
				}
				// events
				CheckEvents(i, Math.Sign(db - da), da, db);
				// finish
				TrackPosition = NewTrackPosition;
				LastTrackElement = i;
			}
			private void CheckEvents(int ElementIndex, int Direction, double OldDelta, double NewDelta) {
				if (Direction < 0) {
					for (int j = 0; j < CurrentRoute.Tracks[0].Elements[ElementIndex].Events.Length; j++)
					{
						dynamic e = CurrentRoute.Tracks[0].Elements[ElementIndex].Events[j];
						if (OldDelta > e.TrackPositionDelta & NewDelta <= e.TrackPositionDelta) {
							e.TryTrigger(-1, this.TriggerType, this.Train, this.Train != null ? this.Train.Cars[CarIndex] : null);
						}
					}
				} else if (Direction > 0) {
					for (int j = 0; j < CurrentRoute.Tracks[0].Elements[ElementIndex].Events.Length; j++)
					{
						dynamic e = CurrentRoute.Tracks[0].Elements[ElementIndex].Events[j];
						if (OldDelta < e.TrackPositionDelta & NewDelta >= e.TrackPositionDelta) {
							e.TryTrigger(1, this.TriggerType, this.Train, this.Train != null ? this.Train.Cars[CarIndex] : null);
						}
					}
				}
			}
		}
		
		
		// check events
        
    }
}
