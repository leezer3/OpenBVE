﻿using System;
using LibRender2.Trains;
using OpenBveApi.Trains;

namespace TrainManager.Trains
{
	/// <summary>A more advanced type of AnimatedObject, which follows a rail and a travel plan</summary>
	public class TrackFollowingObject : TrainBase
	{
		/// <summary>The time the train appears in-game</summary>
		public double AppearanceTime;
		/// <summary>The track position at which the train appears</summary>
		public double AppearanceStartPosition;
		/// <summary>The track position at which the train disappears</summary>
		public double AppearanceEndPosition;
		/// <summary>The time at which the train is removed from the game</summary>
		public double LeaveTime;
		private double InternalTimerTimeElapsed;

		public TrackFollowingObject(TrainState state) : base(state)
		{
		}

		/// <summary>Disposes of the train</summary>
		public override void Dispose()
		{
			State = TrainState.Disposed;
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].ChangeCarSection(CarSectionType.NotVisible);
				Cars[i].FrontBogie.ChangeSection(-1);
				Cars[i].RearBogie.ChangeSection(-1);
				Cars[i].Coupler.ChangeSection(-1);
			}
			TrainManagerBase.currentHost.StopAllSounds(this);
		}

		/// <summary>Call this method to update the train</summary>
		/// <param name="TimeElapsed">The elapsed time this frame</param>
		public override void Update(double TimeElapsed)
		{
			if (State == TrainState.Pending)
			{
				// pending train
				if (TrainManagerBase.currentHost.InGameTime >= AppearanceTime)
				{
					double PlayerTrainTrackPosition = TrainManagerBase.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition + 0.5 * TrainManagerBase.PlayerTrain.Cars[0].Length - TrainManagerBase.PlayerTrain.Cars[0].FrontAxle.Position;
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
						Cars[i].Coupler.ChangeSection(0);

						if (Cars[i].Specs.IsMotorCar && Cars[i].Sounds.Loop != null)
						{
							Cars[i].Sounds.Loop.Play(Cars[i], true);
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
						float b = (float)(Cars[i].Brightness.NextTrackPosition - Cars[i].Brightness.PreviousTrackPosition);

						//1.0f represents a route brightness value of 255
						//0.0f represents a route brightness value of 0

						if (b != 0.0f)
						{
							b = (float)(Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].Brightness.PreviousTrackPosition) / b;
							if (b < 0.0f) b = 0.0f;
							if (b > 1.0f) b = 1.0f;
							b = Cars[i].Brightness.PreviousBrightness * (1.0f - b) + Cars[i].Brightness.NextBrightness * b;
						}
						else
						{
							b = Cars[i].Brightness.PreviousBrightness;
						}

						//Calculate the cab brightness
						double ccb = Math.Round(255.0 * (1.0 - b));
						//DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
						dnb = (byte)Math.Min(TrainManagerBase.Renderer.Lighting.DynamicCabBrightness, ccb);
					}
					int cs = Cars[i].CurrentCarSection;
					if (cs >= 0 && cs < Cars[i].CarSections.Length)
					{
						if (Cars[i].CarSections[cs].Groups.Length > 0)
						{
							for (int k = 0; k < Cars[i].CarSections[cs].Groups[0].Elements.Length; k++)
							{
								if (Cars[i].CarSections[cs].Groups[0].Elements[k].internalObject != null)
								{
									Cars[i].CarSections[cs].Groups[0].Elements[k].internalObject.DaytimeNighttimeBlend = dnb;
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
			UpdateDoors(TimeElapsed);

			// Update Run and Motor sounds
			foreach (var Car in Cars)
			{
				Car.Run.Update(TimeElapsed);
				if (Car.Sounds.Motor != null)
				{
					Car.Sounds.Motor.Update(TimeElapsed);
				}
				
			}

			// infrequent updates
			InternalTimerTimeElapsed += TimeElapsed;
			if (InternalTimerTimeElapsed > 10.0)
			{
				InternalTimerTimeElapsed -= 10.0;
				Synchronize();
			}
		}

		public override void Jump(int stationIndex)
		{
			Dispose();
			State = TrainState.Pending;
			TrainManagerBase.currentHost.ProcessJump(this, stationIndex);
		}
	}
}
