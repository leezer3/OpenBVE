using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Routes
{
	/// <summary>A Track Follower follows a track</summary>
	public class TrackFollower
	{
		/// <summary>The last track element the follower entered</summary>
		public int LastTrackElement;
		/// <summary>The track position</summary>
		public double TrackPosition;
		/// <summary>The world position vector</summary>
		public Vector3 WorldPosition;
		/// <summary>The world Direction vector</summary>
		public Vector3 WorldDirection;
		/// <summary>The world Up vector</summary>
		public Vector3 WorldUp;
		/// <summary>The world Side vector</summary>
		public Vector3 WorldSide;
		/// <summary>The pitch at the current location</summary>
		public double Pitch;
		/// <summary>The curve radius at the current location</summary>
		public double CurveRadius;
		/// <summary>The curve cant at the current location</summary>
		public double CurveCant;
		/// <summary>The distance the follower has travelled</summary>
		public double Odometer;
		/// <summary>The adjusted cant value due to the Route.Innacuracy value</summary>
		public double CantDueToInaccuracy;
		/// <summary>The adhesion multiplier at the current location</summary>
		public double AdhesionMultiplier;
		/// <summary>The event types to be triggered</summary>
		public EventTriggerType TriggerType;
		/// <summary>The train the follower is attached to, or a null reference</summary>
		public AbstractTrain Train;
		/// <summary>The car the follower is attached to, or a null reference</summary>
		public AbstractCar Car;
		/// <summary>The track index the follower is currently following</summary>
		public int TrackIndex;
		/// <summary>Stores a reference to the CurrentRoute.Tracks array</summary>
		private readonly Track[] Tracks;

		/// <summary>Creates a new TrackFollower</summary>
		public TrackFollower(Track[] tracks, AbstractTrain train = null, AbstractCar car = null)
		{
			Tracks = tracks;
			Train = train;
			Car = car;
		}

		/// <summary>Gets the rail gauge for the track this is following</summary>
		public double RailGauge
		{
			get
			{
				return Tracks[TrackIndex].RailGauge;
			}
		}

		/// <summary>Updates the World Coordinates</summary>
		/// <param name="AddTrackInaccuracy"></param>
		public void UpdateWorldCoordinates(bool AddTrackInaccuracy)
		{
			UpdateAbsolute(this.TrackPosition, true, AddTrackInaccuracy);
		}

		/// <summary>Call this method to update a single track follower on a relative basis</summary>
		/// <param name="RelativeTrackPosition">The new absolute track position of the follower</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccuracy">Whether to add track innacuracy</param>
		public void UpdateRelative(double RelativeTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccuracy)
		{
			UpdateAbsolute(TrackPosition + RelativeTrackPosition, UpdateWorldCoordinates, AddTrackInaccuracy);
		}

		/// <summary>Call this method to update a single track follower on an absolute basis</summary>
		/// <param name="NewTrackPosition">The new absolute track position of the follower</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccuracy">Whether to add track innacuracy</param>
		public void UpdateAbsolute(double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccuracy)
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
						double p = Tracks[TrackIndex].Elements[i].WorldDirection.Y / System.Math.Sqrt(Tracks[TrackIndex].Elements[i].WorldDirection.X * Tracks[TrackIndex].Elements[i].WorldDirection.X + Tracks[TrackIndex].Elements[i].WorldDirection.Z * Tracks[TrackIndex].Elements[i].WorldDirection.Z);
						double s = db / System.Math.Sqrt(1.0 + p * p);
						double h = s * p;
						double b = s / System.Math.Abs(r);
						double f = 2.0 * r * r * (1.0 - System.Math.Cos(b));
						double c = (double) System.Math.Sign(db) * System.Math.Sqrt(f >= 0.0 ? f : 0.0);
						double a = 0.5 * (double) System.Math.Sign(r) * b;
						Vector3 D = new Vector3(Tracks[TrackIndex].Elements[i].WorldDirection.X, 0.0, Tracks[TrackIndex].Elements[i].WorldDirection.Z);
						D.Normalize();
						double cosa = System.Math.Cos(a);
						double sina = System.Math.Sin(a);
						D.Rotate(Vector3.Down, cosa, sina);
						WorldPosition.X = Tracks[TrackIndex].Elements[i].WorldPosition.X + c * D.X;
						WorldPosition.Y = Tracks[TrackIndex].Elements[i].WorldPosition.Y + h;
						WorldPosition.Z = Tracks[TrackIndex].Elements[i].WorldPosition.Z + c * D.Z;
						D.Rotate(Vector3.Down, cosa, sina);
						WorldDirection.X = D.X;
						WorldDirection.Y = p;
						WorldDirection.Z = D.Z;
						WorldDirection.Normalize();
						double cos2a = System.Math.Cos(2.0 * a);
						double sin2a = System.Math.Sin(2.0 * a);
						WorldSide = Tracks[TrackIndex].Elements[i].WorldSide;
						WorldSide.Rotate(Vector3.Down, cos2a, sin2a);
						WorldUp = Vector3.Cross(WorldDirection, WorldSide);

					}
					else
					{
						// straight
						WorldPosition = Tracks[TrackIndex].Elements[i].WorldPosition + db * Tracks[TrackIndex].Elements[i].WorldDirection;
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
			if (AddTrackInaccuracy)
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
					Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
					Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, Tracks[TrackIndex].Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
					x = (1.0 - t) * x1 + t * x2;
					y = (1.0 - t) * y1 + t * y2;
					c = (1.0 - t) * c1 + t * c2;
				}
				else
				{
					Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
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
			CheckEvents(i, System.Math.Sign(db - da), da, db);
			//Update the odometer
			if (TrackPosition != NewTrackPosition)
			{
				//HACK: Reset the odometer if we've moved more than 10m this frame
				if (System.Math.Abs(NewTrackPosition - TrackPosition) > 10)
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
					GeneralEvent e = Tracks[Index].Elements[ElementIndex].Events[j];
					if (Tracks[Index].Elements[ElementIndex].Events.Length == 0)
					{
						return;
					}

					if (OldDelta > e.TrackPositionDelta & NewDelta <= e.TrackPositionDelta)
					{
						e.TryTrigger(-1, this.TriggerType, this.Train, this.Car);
					}
				}
			}
			else if (Direction > 0)
			{
				for (int j = 0; j < Tracks[Index].Elements[ElementIndex].Events.Length; j++)
				{
					GeneralEvent e = Tracks[Index].Elements[ElementIndex].Events[j];
					if (OldDelta < e.TrackPositionDelta & NewDelta >= e.TrackPositionDelta)
					{
						e.TryTrigger(1, this.TriggerType, this.Train, this.Car);
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
