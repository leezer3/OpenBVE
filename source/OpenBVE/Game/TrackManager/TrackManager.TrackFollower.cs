using System;
using OpenBveApi.Math;

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
				Update(this.TrackPosition, true, AddTrackInaccuracy);
			}

			/// <summary>Call this method to update a single track follower</summary>
			/// <param name="NewTrackPosition">The new track position of the follower</param>
			/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
			/// <param name="AddTrackInaccurary">Whether to add track innacuracy</param>
			internal void Update(double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary)
			{
				if (TrackIndex >= Tracks.Length || Tracks[TrackIndex].Elements.Length == 0) return;
				int i = LastTrackElement;
				while (i >= 0 && NewTrackPosition < Tracks[TrackIndex].Elements[i].StartingTrackPosition)
				{
					double ta = TrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition;
					double tb = -0.01;
					CheckEvents(i, -1, ta, tb);
					i--;
				}
				if (i >= 0)
				{
					while (i < Tracks[TrackIndex].Elements.Length - 1)
					{
						if (NewTrackPosition < Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition) break;
						double ta = TrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition;
						double tb = Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition + 0.01;
						CheckEvents(i, 1, ta, tb);
						i++;
					}
				}
				else
				{
					i = 0;
				}
				double da = TrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition;
				double db = NewTrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition;

				// track
				if (UpdateWorldCoordinates)
				{
					if (db != 0.0)
					{
						if (Tracks[TrackIndex].Elements[i].CurveRadius != 0.0)
						{
							// curve
							double r = Tracks[TrackIndex].Elements[i].CurveRadius;
							double p = Tracks[TrackIndex].Elements[i].WorldDirection.Y / Math.Sqrt(Tracks[TrackIndex].Elements[i].WorldDirection.X * Tracks[TrackIndex].Elements[i].WorldDirection.X + Tracks[TrackIndex].Elements[i].WorldDirection.Z * Tracks[TrackIndex].Elements[i].WorldDirection.Z);
							double s = db / Math.Sqrt(1.0 + p * p);
							double h = s * p;
							double b = s / Math.Abs(r);
							double f = 2.0 * r * r * (1.0 - Math.Cos(b));
							double c = (double)Math.Sign(db) * Math.Sqrt(f >= 0.0 ? f : 0.0);
							double a = 0.5 * (double)Math.Sign(r) * b;
							Vector3 D = new Vector3(Tracks[TrackIndex].Elements[i].WorldDirection.X, 0.0, Tracks[TrackIndex].Elements[i].WorldDirection.Z);
							D.Normalize();
							double cosa = Math.Cos(a);
							double sina = Math.Sin(a);
							D.Rotate(Vector3.Down, cosa, sina);
							WorldPosition.X = Tracks[TrackIndex].Elements[i].WorldPosition.X + c * D.X;
							WorldPosition.Y = Tracks[TrackIndex].Elements[i].WorldPosition.Y + h;
							WorldPosition.Z = Tracks[TrackIndex].Elements[i].WorldPosition.Z + c * D.Z;
							D.Rotate(Vector3.Down, cosa, sina);
							WorldDirection.X = D.X;
							WorldDirection.Y = p;
							WorldDirection.Z = D.Z;
							WorldDirection.Normalize();
							double cos2a = Math.Cos(2.0 * a);
							double sin2a = Math.Sin(2.0 * a);
							WorldSide = Tracks[TrackIndex].Elements[i].WorldSide;
							WorldSide.Rotate(Vector3.Down, cos2a, sin2a);
							WorldUp = Vector3.Cross(WorldDirection, WorldSide);

						}
						else
						{
							// straight
							WorldPosition.X = Tracks[TrackIndex].Elements[i].WorldPosition.X + db * Tracks[TrackIndex].Elements[i].WorldDirection.X;
							WorldPosition.Y = Tracks[TrackIndex].Elements[i].WorldPosition.Y + db * Tracks[TrackIndex].Elements[i].WorldDirection.Y;
							WorldPosition.Z = Tracks[TrackIndex].Elements[i].WorldPosition.Z + db * Tracks[TrackIndex].Elements[i].WorldDirection.Z;
							WorldDirection = Tracks[TrackIndex].Elements[i].WorldDirection;
							WorldUp = Tracks[TrackIndex].Elements[i].WorldUp;
							WorldSide = Tracks[TrackIndex].Elements[i].WorldSide;
							CurveRadius = 0.0;
						}

						// cant
						if (i < Tracks[TrackIndex].Elements.Length - 1)
						{
							double t = db / (Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition);
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
								(2.0 * t3 - 3.0 * t2 + 1.0) * Tracks[TrackIndex].Elements[i].CurveCant +
								(t3 - 2.0 * t2 + t) * Tracks[TrackIndex].Elements[i].CurveCantTangent +
								(-2.0 * t3 + 3.0 * t2) * Tracks[TrackIndex].Elements[i + 1].CurveCant +
								(t3 - t2) * Tracks[TrackIndex].Elements[i + 1].CurveCantTangent;
							CurveRadius = Tracks[TrackIndex].Elements[i].CurveRadius;
						}
						else
						{
							CurveCant = Tracks[TrackIndex].Elements[i].CurveCant;
						}


					}
					else
					{
						WorldPosition = Tracks[TrackIndex].Elements[i].WorldPosition;
						WorldDirection = Tracks[TrackIndex].Elements[i].WorldDirection;
						WorldUp = Tracks[TrackIndex].Elements[i].WorldUp;
						WorldSide = Tracks[TrackIndex].Elements[i].WorldSide;
						CurveRadius = Tracks[TrackIndex].Elements[i].CurveRadius;
						CurveCant = Tracks[TrackIndex].Elements[i].CurveCant;
					}

				}
				else
				{
					if (db != 0.0)
					{
						if (Tracks[TrackIndex].Elements[i].CurveRadius != 0.0)
						{
							CurveRadius = Tracks[TrackIndex].Elements[i].CurveRadius;
						}
						else
						{
							CurveRadius = 0.0;
						}
						if (i < Tracks[TrackIndex].Elements.Length - 1)
						{
							double t = db / (Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition);
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
								(2.0 * t3 - 3.0 * t2 + 1.0) * Tracks[TrackIndex].Elements[i].CurveCant +
								(t3 - 2.0 * t2 + t) * Tracks[TrackIndex].Elements[i].CurveCantTangent +
								(-2.0 * t3 + 3.0 * t2) * Tracks[TrackIndex].Elements[i + 1].CurveCant +
								(t3 - t2) * Tracks[TrackIndex].Elements[i + 1].CurveCantTangent;
						}
						else
						{
							CurveCant = Tracks[TrackIndex].Elements[i].CurveCant;
						}
					}
					else
					{
						CurveRadius = Tracks[TrackIndex].Elements[i].CurveRadius;
						CurveCant = Tracks[TrackIndex].Elements[i].CurveCant;
					}

				}
				AdhesionMultiplier = Tracks[TrackIndex].Elements[i].AdhesionMultiplier;
				//Pitch added for Plugin Data usage
				//Mutliply this by 1000 to get the original value
				Pitch = Tracks[TrackIndex].Elements[i].Pitch * 1000;
				// inaccuracy
				if (AddTrackInaccurary)
				{
					double x, y, c;
					if (i < Tracks[TrackIndex].Elements.Length - 1)
					{
						double t = db / (Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - Tracks[TrackIndex].Elements[i].StartingTrackPosition);
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
						GetInaccuracies(NewTrackPosition, Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
						GetInaccuracies(NewTrackPosition, Tracks[TrackIndex].Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
						x = (1.0 - t) * x1 + t * x2;
						y = (1.0 - t) * y1 + t * y2;
						c = (1.0 - t) * c1 + t * c2;
					}
					else
					{
						GetInaccuracies(NewTrackPosition, Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
					}
					WorldPosition.X += x * WorldSide.X + y * WorldUp.X;
					WorldPosition.Y += x * WorldSide.Y + y * WorldUp.Y;
					WorldPosition.Z += x * WorldSide.Z + y * WorldUp.Z;
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
				if (this.TriggerType == EventTriggerType.None || Tracks[TrackIndex].Elements[ElementIndex].Events.Length == 0)
				{
					return;
				}

				int Index = TrackIndex;
				if (Direction < 0)
				{
					for (int j = Tracks[Index].Elements[ElementIndex].Events.Length - 1; j >= 0; j--)
					{
						if (Tracks[Index].Elements[ElementIndex].Events.Length == 0)
						{
							return;
						}
						if (OldDelta > Tracks[Index].Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta <= Tracks[Index].Elements[ElementIndex].Events[j].TrackPositionDelta)
						{
							Tracks[Index].Elements[ElementIndex].Events[j].TryTrigger(-1, this.TriggerType, this.Train, this.CarIndex);
						}
					}
				}
				else if (Direction > 0)
				{
					for (int j = 0; j < Tracks[Index].Elements[ElementIndex].Events.Length; j++)
					{
						if (OldDelta < Tracks[Index].Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta >= Tracks[Index].Elements[ElementIndex].Events[j].TrackPositionDelta)
						{
							Tracks[Index].Elements[ElementIndex].Events[j].TryTrigger(1, this.TriggerType, this.Train, this.CarIndex);
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
