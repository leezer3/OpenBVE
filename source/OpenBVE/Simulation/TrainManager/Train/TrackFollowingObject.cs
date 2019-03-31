using System;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBve
{
	public static partial class TrainManager
	{
		public class TrackFollowingObject : Train
		{
			internal double AppearanceTime;
			internal double AppearanceStartPosition;
			internal double AppearanceEndPosition;
			internal double LeaveTime;
			private double InternalTimerTimeElapsed;

			internal TrackFollowingObject(TrainState state) : base(state)
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
				Sounds.StopAllSounds(this);
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
						for (int i = 0; i < Cars.Length; i++)
						{
							if (Cars[i].CarSections.Length != 0)
							{
								Cars[i].ChangeCarSection(CarSectionType.Exterior);
							}
							Cars[i].FrontBogie.ChangeSection(0);
							Cars[i].RearBogie.ChangeSection(0);

							if (Cars[i].Specs.IsMotorCar)
							{
								if (Cars[i].Sounds.Loop.Buffer != null)
								{
									Vector3 pos = Cars[i].Sounds.Loop.Position;
									Cars[i].Sounds.Loop.Source = Sounds.PlaySound(Cars[i].Sounds.Loop.Buffer, 1.0, 1.0, pos, this, i, true);
								}
							}
						}
					}
				}
				else if (State == TrainState.Available)
				{
					// available train
					UpdatePhysicsAndControls(TimeElapsed);
					for (int i = 0; i < Cars.Length; i++)
					{
						byte dnb;
						{
							float b = (float) (Cars[i].Brightness.NextTrackPosition - Cars[i].Brightness.PreviousTrackPosition);

							//1.0f represents a route brightness value of 255
							//0.0f represents a route brightness value of 0

							if (b != 0.0f)
							{
								b = (float) (Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].Brightness.PreviousTrackPosition) / b;
								if (b < 0.0f) b = 0.0f;
								if (b > 1.0f) b = 1.0f;
								b = Cars[i].Brightness.PreviousBrightness * (1.0f - b) + Cars[i].Brightness.NextBrightness * b;
							}
							else
							{
								b = Cars[i].Brightness.PreviousBrightness;
							}

							//Calculate the cab brightness
							double ccb = Math.Round(255.0 * (double) (1.0 - b));
							//DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
							dnb = (byte) Math.Min(Renderer.DynamicCabBrightness, ccb);
						}
						int cs = Cars[i].CurrentCarSection;
						if (cs >= 0 && Cars[i].CarSections.Length > 0 && Cars[i].CarSections.Length >= cs)
						{
							if (Cars[i].CarSections[cs].Groups.Length > 0)
							{
								for (int k = 0; k < Cars[i].CarSections[cs].Groups[0].Elements.Length; k++)
								{
									int o = Cars[i].CarSections[cs].Groups[0].Elements[k].ObjectIndex;
									if (ObjectManager.Objects[o] != null)
									{
										for (int j = 0; j < ObjectManager.Objects[o].Mesh.Materials.Length; j++)
										{
											ObjectManager.Objects[o].Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
										}
									}
								}
							}
						}

						if (AI != null)
						{
							AI.Trigger(TimeElapsed);
						}
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

				// update station and doors
				UpdateTrainDoors(this, TimeElapsed);

				// Update Run and Motor sounds
				foreach (var Car in Cars)
				{
					Car.UpdateRunSounds(TimeElapsed);
					Car.UpdateMotorSounds(TimeElapsed);
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
