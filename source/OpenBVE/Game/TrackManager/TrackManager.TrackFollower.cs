using System;
using OpenBve.RouteManager;
using OpenBveApi.Math;
using OpenBveApi.Routes;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		// track follower
		internal struct TrackFollower
		{
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
			internal int TrackIndex;

			internal void UpdateWorldCoordinates(bool AddTrackInaccuracy)
			{
				UpdateAbsolute(this.TrackPosition, true, AddTrackInaccuracy);
			}

			/// <summary>Call this method to update a single track follower on a relative basis</summary>
			/// <param name="RelativeTrackPosition">The new absolute track position of the follower</param>
			/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
			/// <param name="AddTrackInaccuracy">Whether to add track innacuracy</param>
			internal void UpdateRelative(double RelativeTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccuracy)
			{
				UpdateAbsolute(TrackPosition + RelativeTrackPosition, UpdateWorldCoordinates, AddTrackInaccuracy);
			}

			/// <summary>Call this method to update a single track follower on an absolute basis</summary>
			/// <param name="NewTrackPosition">The new absolute track position of the follower</param>
			/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
			/// <param name="AddTrackInaccuracy">Whether to add track innacuracy</param>
			internal void UpdateAbsolute(double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccuracy)
			{
				if (TrackIndex >= CurrentRoute.Tracks.Length || CurrentRoute.Tracks[TrackIndex].Elements.Length == 0) return;
				int i = LastTrackElement;
				while (i >= 0 && NewTrackPosition < CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition)
				{
					double ta = TrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition;
					double tb = -0.01;
					CheckEvents(i, -1, ta, tb);
					i--;
				}
				if (i >= 0)
				{
					while (i < CurrentRoute.Tracks[TrackIndex].Elements.Length - 1)
					{
						if (NewTrackPosition < CurrentRoute.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition) break;
						double ta = TrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition;
						double tb = CurrentRoute.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition + 0.01;
						CheckEvents(i, 1, ta, tb);
						i++;
					}
				}
				else
				{
					i = 0;
				}
				double da = TrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition;
				double db = NewTrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition;

				// track
				if (UpdateWorldCoordinates)
				{
					if (db != 0.0)
					{
						if (CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius != 0.0)
						{
							// curve
							double r = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius;
							double p = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.Y / Math.Sqrt(CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.X * CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.X + CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.Z * CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.Z);
							double s = db / Math.Sqrt(1.0 + p * p);
							double h = s * p;
							double b = s / Math.Abs(r);
							double f = 2.0 * r * r * (1.0 - Math.Cos(b));
							double c = (double)Math.Sign(db) * Math.Sqrt(f >= 0.0 ? f : 0.0);
							double a = 0.5 * (double)Math.Sign(r) * b;
							Vector3 D = new Vector3(CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.X, 0.0, CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection.Z);
							D.Normalize();
							double cosa = Math.Cos(a);
							double sina = Math.Sin(a);
							D.Rotate(Vector3.Down, cosa, sina);
							WorldPosition.X = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldPosition.X + c * D.X;
							WorldPosition.Y = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldPosition.Y + h;
							WorldPosition.Z = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldPosition.Z + c * D.Z;
							D.Rotate(Vector3.Down, cosa, sina);
							WorldDirection.X = D.X;
							WorldDirection.Y = p;
							WorldDirection.Z = D.Z;
							WorldDirection.Normalize();
							double cos2a = Math.Cos(2.0 * a);
							double sin2a = Math.Sin(2.0 * a);
							WorldSide = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldSide;
							WorldSide.Rotate(Vector3.Down, cos2a, sin2a);
							WorldUp = Vector3.Cross(WorldDirection, WorldSide);

						}
						else
						{
							// straight
							WorldPosition = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldPosition + db * CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection;
							WorldDirection = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection;
							WorldUp = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldUp;
							WorldSide = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldSide;
							CurveRadius = 0.0;
						}

						// cant
						if (i < CurrentRoute.Tracks[TrackIndex].Elements.Length - 1)
						{
							double t = db / (CurrentRoute.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition);
							if (t < 0.0)
							{
								t = 0.0;
							}
							else if (t > 1.0)
							{
								t = 1.0;
							}
							double t2 = t * t;
							double t3 = t2 * t;
							CurveCant =
								(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCant +
								(t3 - 2.0 * t2 + t) * CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCantTangent +
								(-2.0 * t3 + 3.0 * t2) * CurrentRoute.Tracks[TrackIndex].Elements[i + 1].CurveCant +
								(t3 - t2) * CurrentRoute.Tracks[TrackIndex].Elements[i + 1].CurveCantTangent;
							CurveRadius = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius;
						}
						else
						{
							CurveCant = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCant;
						}


					}
					else
					{
						WorldPosition = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldPosition;
						WorldDirection = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldDirection;
						WorldUp = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldUp;
						WorldSide = CurrentRoute.Tracks[TrackIndex].Elements[i].WorldSide;
						CurveRadius = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius;
						CurveCant = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCant;
					}

				}
				else
				{
					if (db != 0.0)
					{
						if (CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius != 0.0)
						{
							CurveRadius = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius;
						}
						else
						{
							CurveRadius = 0.0;
						}
						if (i < CurrentRoute.Tracks[TrackIndex].Elements.Length - 1)
						{
							double t = db / (CurrentRoute.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition);
							if (t < 0.0)
							{
								t = 0.0;
							}
							else if (t > 1.0)
							{
								t = 1.0;
							}
							double t2 = t * t;
							double t3 = t2 * t;
							CurveCant =
								(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCant +
								(t3 - 2.0 * t2 + t) * CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCantTangent +
								(-2.0 * t3 + 3.0 * t2) * CurrentRoute.Tracks[TrackIndex].Elements[i + 1].CurveCant +
								(t3 - t2) * CurrentRoute.Tracks[TrackIndex].Elements[i + 1].CurveCantTangent;
						}
						else
						{
							CurveCant = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCant;
						}
					}
					else
					{
						CurveRadius = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveRadius;
						CurveCant = CurrentRoute.Tracks[TrackIndex].Elements[i].CurveCant;
					}

				}
				AdhesionMultiplier = CurrentRoute.Tracks[TrackIndex].Elements[i].AdhesionMultiplier;
				//Pitch added for Plugin Data usage
				//Mutliply this by 1000 to get the original value
				Pitch = CurrentRoute.Tracks[TrackIndex].Elements[i].Pitch * 1000;
				// inaccuracy
				if (AddTrackInaccuracy)
				{
					double x, y, c;
					if (i < CurrentRoute.Tracks[TrackIndex].Elements.Length - 1)
					{
						double t = db / (CurrentRoute.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - CurrentRoute.Tracks[TrackIndex].Elements[i].StartingTrackPosition);
						if (t < 0.0)
						{
							t = 0.0;
						}
						else if (t > 1.0)
						{
							t = 1.0;
						}
						double x1, y1, c1;
						double x2, y2, c2;
						CurrentRoute.Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, CurrentRoute.Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
						CurrentRoute.Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, CurrentRoute.Tracks[TrackIndex].Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
						x = (1.0 - t) * x1 + t * x2;
						y = (1.0 - t) * y1 + t * y2;
						c = (1.0 - t) * c1 + t * c2;
					}
					else
					{
						CurrentRoute.Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, CurrentRoute.Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
					}
					WorldPosition += x * WorldSide + y * WorldUp;
					CurveCant += c;
					CantDueToInaccuracy = c;
				}
				else
				{
					CantDueToInaccuracy = 0.0;
				}
				// events
				CheckEvents(i, Math.Sign(db - da), da, db);
				//Update the odometer
				if (TrackPosition != NewTrackPosition)
				{
					//HACK: Reset the odometer if we've moved more than 10m this frame
					if (Math.Abs(NewTrackPosition - TrackPosition) > 10)
					{
						Odometer = 0;
					}
					else
					{
						Odometer += NewTrackPosition - TrackPosition;
					}
				}
				// finish
				TrackPosition = NewTrackPosition;
				LastTrackElement = i;
			}

			private void CheckEvents(int ElementIndex, int Direction, double OldDelta, double NewDelta)
			{
				if (this.TriggerType == EventTriggerType.None || CurrentRoute.Tracks[TrackIndex].Elements[ElementIndex].Events.Length == 0)
				{
					return;
				}

				int Index = TrackIndex;
				if (Direction < 0)
				{
					for (int j = CurrentRoute.Tracks[Index].Elements[ElementIndex].Events.Length - 1; j >= 0; j--)
					{
						dynamic e = CurrentRoute.Tracks[Index].Elements[ElementIndex].Events[j];
						if (CurrentRoute.Tracks[Index].Elements[ElementIndex].Events.Length == 0)
						{
							return;
						}
						if (OldDelta > e.TrackPositionDelta & NewDelta <= e.TrackPositionDelta)
						{
							e.TryTrigger(-1, this.TriggerType, this.Train, this.Train != null ? this.Train.Cars[CarIndex] : null);
						}
					}
				}
				else if (Direction > 0)
				{
					for (int j = 0; j < CurrentRoute.Tracks[Index].Elements[ElementIndex].Events.Length; j++)
					{
						dynamic e = CurrentRoute.Tracks[Index].Elements[ElementIndex].Events[j];
						if (OldDelta < e.TrackPositionDelta & NewDelta >= e.TrackPositionDelta)
						{
							e.TryTrigger(1, this.TriggerType, this.Train, this.Train != null ? this.Train.Cars[CarIndex] : null);
						}
					}
				}

				if (Index != TrackIndex)
				{
					//We have swapped tracks, so need to check the events on the new track also
					CheckEvents(ElementIndex, Direction, OldDelta, NewDelta);
				}
			}
		}
	}
}
