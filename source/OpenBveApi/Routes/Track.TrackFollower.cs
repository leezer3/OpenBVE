using System.Linq;
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
		/// <summary>The rain intensity at the current location</summary>
		public int RainIntensity;
		/// <summary>The rain intensity at the current location</summary>
		public int SnowIntensity;
		/// <summary>The event types to be triggered</summary>
		public EventTriggerType TriggerType;
		/// <summary>The train the follower is attached to, or a null reference</summary>
		public AbstractTrain Train;
		/// <summary>The car the follower is attached to, or a null reference</summary>
		public AbstractCar Car;
		/// <summary>The track index the follower is currently following</summary>
		public int TrackIndex;
		/// <summary>The station index at the current location</summary>
		public int StationIndex;

		private readonly Hosts.HostInterface currentHost;



		/// <summary>Clones the TrackFollower</summary>
		public TrackFollower Clone()
		{
			TrackFollower t = new TrackFollower(currentHost, Train, Car);
			t.LastTrackElement = LastTrackElement;
			t.TrackPosition = TrackPosition;
			t.WorldPosition = new Vector3(WorldPosition);
			t.WorldDirection = new Vector3(WorldDirection);
			t.WorldUp = new Vector3(WorldUp);
			t.WorldSide = new Vector3(WorldSide);
			t.TriggerType = TriggerType;
			t.TrackIndex = TrackIndex;
			return t;
		}

		/// <summary>Creates a new TrackFollower</summary>
		public TrackFollower(Hosts.HostInterface Host, AbstractTrain train = null, AbstractCar car = null)
		{
			currentHost = Host;
			Train = train;
			Car = car;
			LastTrackElement = 0;
			TrackPosition = 0;
			WorldPosition = new Vector3();
			WorldDirection = new Vector3();
			WorldUp = new Vector3();
			WorldSide = new Vector3();
			Pitch = 0;
			CurveRadius = 0;
			CurveCant = 0;
			Odometer = 0;
			CantDueToInaccuracy = 0;
			AdhesionMultiplier = 0;
			RainIntensity = 0;
			SnowIntensity = 0;
			TriggerType = EventTriggerType.None;
			TrackIndex = 0;
			StationIndex = -1;
		}

		/// <summary>Gets the rail gauge for the track this is following</summary>
		public double RailGauge
		{
			get
			{
				return currentHost.Tracks[TrackIndex].RailGauge;
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
			if (TrackIndex == 0 && (currentHost.Tracks[0].Elements == null || currentHost.Tracks[TrackIndex].Elements.Length == 0))
			{
				/*
				 * Used for displaying trains in Object Viewer
				 * As we have no track, just update the Z value with the new pos
				 */
				WorldPosition.Z = NewTrackPosition;
				return;
			}
			if (!currentHost.Tracks.ContainsKey(TrackIndex) || currentHost.Tracks[TrackIndex].Elements.Length == 0) return;
			int i = System.Math.Min(currentHost.Tracks[TrackIndex].Elements.Length - 1, LastTrackElement);
			while (i >= 0)
			{
				if (currentHost.Tracks[TrackIndex].Elements[i].InvalidElement)
				{
					var prevTrackEnded = currentHost.Tracks[TrackIndex].Elements.Select((x, j) => new { Index = j, Element = x }).Take(i + 1).LastOrDefault(x => !x.Element.InvalidElement);

					if (prevTrackEnded == null)
					{
						break;
					}

					i = prevTrackEnded.Index;

					if (i == 0)
					{
						break;
					}
				}

				if (NewTrackPosition >= currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition)
				{
					break;
				}
				double ta = TrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition;
				const double tb = -0.01;
				CheckEvents(i, -1, ta, tb);
				i--;
			}

			if (i >= 0)
			{
				while (i < currentHost.Tracks[TrackIndex].Elements.Length - 1)
				{
					if (currentHost.Tracks[TrackIndex].Elements[i + 1].InvalidElement)
					{
						var nextTrackStarted = currentHost.Tracks[TrackIndex].Elements.Select((x, j) => new { Index = j, Element = x }).Skip(i + 1).FirstOrDefault(x => !x.Element.InvalidElement);

						if (nextTrackStarted == null)
						{
							break;
						}

						i = nextTrackStarted.Index;

						if (i == currentHost.Tracks[TrackIndex].Elements.Length - 1)
						{
							break;
						}
					}

					if (NewTrackPosition < currentHost.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition) break;
					double ta = TrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition;
					double tb = currentHost.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition + 0.01;
					CheckEvents(i, 1, ta, tb);
					i++;
				}
			}
			else
			{
				i = 0;
			}

			double da = TrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition;
			double db = NewTrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition;

			// track
			if (UpdateWorldCoordinates)
			{
				if (db != 0.0)
				{
					if (currentHost.Tracks[TrackIndex].Elements[i].CurveRadius != 0.0)
					{
						// curve
						double r = currentHost.Tracks[TrackIndex].Elements[i].CurveRadius;
						double p = currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.Y / System.Math.Sqrt(currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.X * currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.X + currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.Z * currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.Z);
						double s = db / System.Math.Sqrt(1.0 + p * p);
						double h = s * p;
						double b = s / System.Math.Abs(r);
						double f = 2.0 * r * r * (1.0 - System.Math.Cos(b));
						double c = (double)System.Math.Sign(db) * System.Math.Sqrt(f >= 0.0 ? f : 0.0);
						double a = 0.5 * (double)System.Math.Sign(r) * b;
						Vector3 D = new Vector3(currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.X, 0.0, currentHost.Tracks[TrackIndex].Elements[i].WorldDirection.Z);
						D.Normalize();
						D.Rotate(Vector3.Down, a);
						WorldPosition.X = currentHost.Tracks[TrackIndex].Elements[i].WorldPosition.X + c * D.X;
						WorldPosition.Y = currentHost.Tracks[TrackIndex].Elements[i].WorldPosition.Y + h;
						WorldPosition.Z = currentHost.Tracks[TrackIndex].Elements[i].WorldPosition.Z + c * D.Z;
						D.Rotate(Vector3.Down, a);
						WorldDirection.X = D.X;
						WorldDirection.Y = p;
						WorldDirection.Z = D.Z;
						WorldDirection.Normalize();
						WorldSide = currentHost.Tracks[TrackIndex].Elements[i].WorldSide;
						WorldSide.Rotate(Vector3.Down, 2.0 * a);
						WorldUp = Vector3.Cross(WorldDirection, WorldSide);

					}
					else
					{
						// straight
						WorldPosition = currentHost.Tracks[TrackIndex].Elements[i].WorldPosition + db * currentHost.Tracks[TrackIndex].Elements[i].WorldDirection;
						WorldDirection = currentHost.Tracks[TrackIndex].Elements[i].WorldDirection;
						WorldUp = currentHost.Tracks[TrackIndex].Elements[i].WorldUp;
						WorldSide = currentHost.Tracks[TrackIndex].Elements[i].WorldSide;
						CurveRadius = 0.0;
					}

					// cant
					if (i < currentHost.Tracks[TrackIndex].Elements.Length - 1)
					{
						double t = db / (currentHost.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition);
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
							(2.0 * t3 - 3.0 * t2 + 1.0) * currentHost.Tracks[TrackIndex].Elements[i].CurveCant +
							(t3 - 2.0 * t2 + t) * currentHost.Tracks[TrackIndex].Elements[i].CurveCantTangent +
							(-2.0 * t3 + 3.0 * t2) * currentHost.Tracks[TrackIndex].Elements[i + 1].CurveCant +
							(t3 - t2) * currentHost.Tracks[TrackIndex].Elements[i + 1].CurveCantTangent;
						CurveRadius = currentHost.Tracks[TrackIndex].Elements[i].CurveRadius;
					}
					else
					{
						CurveCant = currentHost.Tracks[TrackIndex].Elements[i].CurveCant;
					}


				}
				else
				{
					WorldPosition = currentHost.Tracks[TrackIndex].Elements[i].WorldPosition;
					WorldDirection = currentHost.Tracks[TrackIndex].Elements[i].WorldDirection;
					WorldUp = currentHost.Tracks[TrackIndex].Elements[i].WorldUp;
					WorldSide = currentHost.Tracks[TrackIndex].Elements[i].WorldSide;
					CurveRadius = currentHost.Tracks[TrackIndex].Elements[i].CurveRadius;
					CurveCant = currentHost.Tracks[TrackIndex].Elements[i].CurveCant;
				}

			}
			else
			{
				if (db != 0.0)
				{
					CurveRadius = currentHost.Tracks[TrackIndex].Elements[i].CurveRadius != 0.0 ? currentHost.Tracks[TrackIndex].Elements[i].CurveRadius : 0.0;

					if (i < currentHost.Tracks[TrackIndex].Elements.Length - 1)
					{
						double t = db / (currentHost.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition);
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
							(2.0 * t3 - 3.0 * t2 + 1.0) * currentHost.Tracks[TrackIndex].Elements[i].CurveCant +
							(t3 - 2.0 * t2 + t) * currentHost.Tracks[TrackIndex].Elements[i].CurveCantTangent +
							(-2.0 * t3 + 3.0 * t2) * currentHost.Tracks[TrackIndex].Elements[i + 1].CurveCant +
							(t3 - t2) * currentHost.Tracks[TrackIndex].Elements[i + 1].CurveCantTangent;
					}
					else
					{
						CurveCant = currentHost.Tracks[TrackIndex].Elements[i].CurveCant;
					}
				}
				else
				{
					CurveRadius = currentHost.Tracks[TrackIndex].Elements[i].CurveRadius;
					CurveCant = currentHost.Tracks[TrackIndex].Elements[i].CurveCant;
				}

			}

			AdhesionMultiplier = currentHost.Tracks[TrackIndex].Elements[i].AdhesionMultiplier;
			RainIntensity = currentHost.Tracks[TrackIndex].Elements[i].RainIntensity;
			SnowIntensity = currentHost.Tracks[TrackIndex].Elements[i].SnowIntensity;
			//Pitch added for Plugin Data usage
			//Mutliply this by 1000 to get the original value
			Pitch = currentHost.Tracks[TrackIndex].Elements[i].Pitch * 1000;
			// inaccuracy
			if (AddTrackInaccuracy)
			{
				double x, y, c;
				if (i < currentHost.Tracks[TrackIndex].Elements.Length - 1)
				{
					double t = db / (currentHost.Tracks[TrackIndex].Elements[i + 1].StartingTrackPosition - currentHost.Tracks[TrackIndex].Elements[i].StartingTrackPosition);
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
					currentHost.Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, currentHost.Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
					currentHost.Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, currentHost.Tracks[TrackIndex].Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
					x = (1.0 - t) * x1 + t * x2;
					y = (1.0 - t) * y1 + t * y2;
					c = (1.0 - t) * c1 + t * c2;
				}
				else
				{
					currentHost.Tracks[TrackIndex].GetInaccuracies(NewTrackPosition, currentHost.Tracks[TrackIndex].Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
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
			if (this.TriggerType == EventTriggerType.None || currentHost.Tracks[TrackIndex].Elements[ElementIndex].Events.Length == 0)
			{
				return;
			}

			int Index = TrackIndex;
			if (Direction < 0)
			{
				for (int j = currentHost.Tracks[Index].Elements[ElementIndex].Events.Length - 1; j >= 0; j--)
				{
					GeneralEvent e = currentHost.Tracks[Index].Elements[ElementIndex].Events[j];
					if (currentHost.Tracks[Index].Elements[ElementIndex].Events.Length == 0)
					{
						return;
					}

					if (OldDelta > e.TrackPositionDelta & NewDelta <= e.TrackPositionDelta)
					{
						e.TryTrigger(-1, this);
					}
				}
			}
			else if (Direction > 0)
			{
				for (int j = 0; j < currentHost.Tracks[Index].Elements[ElementIndex].Events.Length; j++)
				{
					GeneralEvent e = currentHost.Tracks[Index].Elements[ElementIndex].Events[j];
					if (OldDelta < e.TrackPositionDelta & NewDelta >= e.TrackPositionDelta)
					{
						e.TryTrigger(1, this);
					}
				}
			}

			if (Index != TrackIndex)
			{
				//We have swapped tracks, so need to check the events on the new track also
				CheckEvents(ElementIndex, Direction, OldDelta, NewDelta);
			}
		}

		/// <summary>Called when a follower enters a station</summary>
		/// <param name="stationIndex">The index of the station</param>
		/// <param name="direction">The direction of travel</param>
		public void EnterStation(int stationIndex, int direction)
		{
			if (direction < 0)
			{
				if (StationIndex == stationIndex)
				{
					StationIndex = -1;
				}
			}
			else if (direction > 0)
			{
				StationIndex = stationIndex;
			}
		}

		/// <summary>Called when a follower leaves a station</summary>
		/// <param name="stationIndex">The index of the station</param>
		/// <param name="direction">The direction of travel</param>
		public void LeaveStation(int stationIndex, int direction)
		{
			if (direction < 0)
			{
				StationIndex = stationIndex;
			}
			else if (direction > 0)
			{
				if (StationIndex == stationIndex)
				{
					StationIndex = -1;
				}
			}
		}
	}
}
