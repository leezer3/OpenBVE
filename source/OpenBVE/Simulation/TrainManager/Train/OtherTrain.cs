namespace OpenBve
{
	public static partial class TrainManager
	{
		public class OtherTrain : Train
		{
			internal double AppearanceTime;
			internal double AppearanceStartPosition;
			internal double AppearanceEndPosition;
			internal double LeaveTime;
			private double InternalTimerTimeElapsed;

			internal OtherTrain(TrainState state) : base(state)
			{
			}

			internal new void Initialize()
			{
				foreach (var Car in Cars)
				{
					Car.Initialize();
				}
				UpdateAtmosphericConstants();
				Update(0.0);
			}

			/// <summary>Disposes of the train</summary>
			internal new void Dispose()
			{
				State = TrainState.Disposed;
				foreach (var Car in Cars)
				{
					Car.ChangeCarSection(CarSectionType.NotVisible);
					Car.FrontBogie.ChangeSection(-1);
					Car.RearBogie.ChangeSection(-1);
				}
			}

			/// <summary>Call this method to update the train</summary>
			/// <param name="TimeElapsed">The elapsed time this frame</param>
			internal new void Update(double TimeElapsed)
			{
				if (State == TrainState.Pending)
				{
					// pending train
					if (Game.SecondsSinceMidnight >= AppearanceTime)
					{
						double PlayerTrainTrackPosition = PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition + 0.5 * PlayerTrain.Cars[0].Length - PlayerTrain.Cars[0].FrontAxle.Position;
						if (PlayerTrainTrackPosition < AppearanceStartPosition || (PlayerTrainTrackPosition > AppearanceEndPosition && AppearanceEndPosition > AppearanceStartPosition))
						{
							return;
						}

						// train is introduced
						State = TrainState.Available;
						foreach (var Car in Cars)
						{
							if (Car.CarSections.Length != 0)
							{
								Car.ChangeCarSection(CarSectionType.Exterior);
							}
							Car.FrontBogie.ChangeSection(0);
							Car.RearBogie.ChangeSection(0);
						}
					}
				}
				else if (State == TrainState.Available)
				{
					// available train
					UpdatePhysicsAndControls(TimeElapsed);
					if (AI != null)
					{
						AI.Trigger(TimeElapsed);
					}
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					if (AI != null)
					{
						AI.Trigger(TimeElapsed);
					}
				}
			}

			/// <summary>Updates the physics and controls for this train</summary>
			/// <param name="TimeElapsed">The time elapsed</param>
			private void UpdatePhysicsAndControls(double TimeElapsed)
			{
				if (TimeElapsed == 0.0 || TimeElapsed > 1000)
				{
					//HACK: The physics engine really does not like update times above 1000ms
					//This works around a bug experienced when jumping to a station on a steep hill
					//causing exessive acceleration
					return;
				}
				// infrequent updates
				InternalTimerTimeElapsed += TimeElapsed;
				if (InternalTimerTimeElapsed > 10.0)
				{
					InternalTimerTimeElapsed -= 10.0;
					Synchronize();
					UpdateAtmosphericConstants();
				}
			}
		}
	}
}
