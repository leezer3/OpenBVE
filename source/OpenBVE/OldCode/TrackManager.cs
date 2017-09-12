using System;
using OpenBveApi.Math;

namespace OpenBve {
	internal static partial class TrackManager {
		
		internal static bool SuppressSoundEvents = false;

		// track element
		internal struct TrackElement {
			internal double StartingTrackPosition;
			internal double CurveRadius;
			internal double CurveCant;
			internal double CurveCantTangent;
			internal double AdhesionMultiplier;
			internal double CsvRwAccuracyLevel;
			internal double Pitch;
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
			internal GeneralEvent[] Events;
			internal TrackElement(double StartingTrackPosition) {
				this.StartingTrackPosition = StartingTrackPosition;
				this.Pitch = 0.0;
				this.CurveRadius = 0.0;
				this.CurveCant = 0.0;
				this.CurveCantTangent = 0.0;
				this.AdhesionMultiplier = 1.0;
				this.CsvRwAccuracyLevel = 2.0;
				this.WorldPosition = new Vector3(0.0, 0.0, 0.0);
				this.WorldDirection = new Vector3(0.0, 0.0, 1.0);
				this.WorldUp = new Vector3(0.0, 1.0, 0.0);
				this.WorldSide = new Vector3(1.0, 0.0, 0.0);
				this.Events = new GeneralEvent[] { };
			}
		}

		// track
		internal struct Track {
			internal TrackElement[] Elements;
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
			internal double Pitch;
			internal double CurveRadius;
			internal double CurveCant;
			internal double Odometer;
			internal double CantDueToInaccuracy;
			internal double AdhesionMultiplier;
			internal EventTriggerType TriggerType;
			internal TrainManager.Train Train;
			internal int CarIndex;
			internal void UpdateWorldCoordinates(bool AddTrackInaccuracy) {
				UpdateTrackFollower(ref this, this.TrackPosition, true, AddTrackInaccuracy);
			}
		}

		/// <summary>Call this method to update all track followers attached to a car</summary>
		/// <param name="car">The car for which to update, passed via 'ref'</param>
		/// <param name="NewTrackPosition">The track position change</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccurary">Whether to add track innaccuarcy</param>
		internal static void UpdateCarFollowers(ref TrainManager.Car car, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary)
		{
			//Car axles
			UpdateTrackFollower(ref car.FrontAxle.Follower, car.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			UpdateTrackFollower(ref car.RearAxle.Follower, car.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			//Front bogie axles
			UpdateTrackFollower(ref car.FrontBogie.FrontAxle.Follower, car.FrontBogie.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			UpdateTrackFollower(ref car.FrontBogie.RearAxle.Follower, car.FrontBogie.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			//Rear bogie axles

			UpdateTrackFollower(ref car.RearBogie.FrontAxle.Follower, car.RearBogie.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			UpdateTrackFollower(ref car.RearBogie.RearAxle.Follower, car.RearBogie.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
		}

		/// <summary>Call this method to update a single track follower</summary>
		/// <param name="Follower">The follower to update</param>
		/// <param name="NewTrackPosition">The new track position of the follower</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccurary">Whether to add track innacuracy</param>
		internal static void UpdateTrackFollower(ref TrackFollower Follower, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) {
			if (CurrentTrack.Elements.Length == 0) return;
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
						World.Normalize(ref D.X, ref D.Y, ref D.Z);
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						World.Rotate(ref D, 0.0, 1.0, 0.0, cosa, sina);
						Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + c * D.X;
						Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + h;
						Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + c * D.Z;
						World.Rotate(ref D, 0.0, 1.0, 0.0, cosa, sina);
						Follower.WorldDirection.X = D.X;
						Follower.WorldDirection.Y = p;
						Follower.WorldDirection.Z = D.Z;
						World.Normalize(ref Follower.WorldDirection.X, ref Follower.WorldDirection.Y, ref Follower.WorldDirection.Z);
						double cos2a = Math.Cos(2.0 * a);
						double sin2a = Math.Sin(2.0 * a);
						Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
						World.Rotate(ref Follower.WorldSide, 0.0, 1.0, 0.0, cos2a, sin2a);
						World.Cross(Follower.WorldDirection, Follower.WorldSide, out Follower.WorldUp);
						
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
						Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
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
			//Pitch added for Plugin Data usage
			//Mutliply this by 1000 to get the original value
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
					GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
					GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
					x = (1.0 - t) * x1 + t * x2;
					y = (1.0 - t) * y1 + t * y2;
					c = (1.0 - t) * c1 + t * c2;
				} else {
					GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
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
			//Update the odometer
			if (Follower.TrackPosition != NewTrackPosition)
			{
				//HACK: Reset the odometer if we've moved more than 10m this frame
				if (Math.Abs(NewTrackPosition - Follower.TrackPosition) > 10)
				{
					Follower.Odometer = 0;
				}
				else
				{
					Follower.Odometer += NewTrackPosition - Follower.TrackPosition;
				}
			}
			// finish
			Follower.TrackPosition = NewTrackPosition;
			Follower.LastTrackElement = i;
		}
		
		/// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
		/// <param name="position">The track position</param>
		/// <param name="inaccuracy">The openBVE innacuaracy value</param>
		/// <param name="x">The X (horizontal) co-ordinate to update</param>
		/// <param name="y">The Y (vertical) co-ordinate to update</param>
		/// <param name="c">???</param>
		private static void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c) {
			if (inaccuracy <= 0.0) {
				x = 0.0;
				y = 0.0;
				c = 0.0;
			} else {
				double z = Math.Pow(0.25 * inaccuracy, 1.2) * position;
				x = 0.14 * Math.Sin(0.5843 * z) + 0.82 * Math.Sin(0.2246 * z) + 0.55 * Math.Sin(0.1974 * z);
				x *= 0.0035 * Game.RouteRailGauge * inaccuracy;
				y = 0.18 * Math.Sin(0.5172 * z) + 0.37 * Math.Sin(0.3251 * z) + 0.91 * Math.Sin(0.3773 * z);
				y *= 0.0020 * Game.RouteRailGauge * inaccuracy;
				c = 0.23 * Math.Sin(0.3131 * z) + 0.54 * Math.Sin(0.5807 * z) + 0.81 * Math.Sin(0.3621 * z);
				c *= 0.0025 * Game.RouteRailGauge * inaccuracy;
			}
		}

		// check events
		private static void CheckEvents(ref TrackFollower Follower, int ElementIndex, int Direction, double OldDelta, double NewDelta) {
			if (Follower.TriggerType != EventTriggerType.None) {
				if (Direction < 0) {
					for (int j = CurrentTrack.Elements[ElementIndex].Events.Length - 1; j >= 0; j--) {
						if (OldDelta > CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta <= CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta) {
							TryTriggerEvent(CurrentTrack.Elements[ElementIndex].Events[j], -1, Follower.TriggerType, Follower.Train, Follower.CarIndex);
						}
					}
				} else if (Direction > 0) {
					for (int j = 0; j < CurrentTrack.Elements[ElementIndex].Events.Length; j++) {
						if (OldDelta < CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta >= CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta) {
							TryTriggerEvent(CurrentTrack.Elements[ElementIndex].Events[j], 1, Follower.TriggerType, Follower.Train, Follower.CarIndex);
						}
					}
				}
			}
		}

	}
}
