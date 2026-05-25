using System;
using LibRender2.Trains;
using OpenBveApi.Trains;
using TrainManager.SafetySystems;

namespace TrainManager.Trains
{
	/// <summary>A scripted train, which follows a rail and a travel plan</summary>
	public class ScriptedTrain : TrainBase
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

		public ScriptedTrain(TrainState state) : base(state, TrainType.ScriptedTrain)
		{
			SafetySystems.PassAlarm = new PassAlarm(PassAlarmType.None, null);
		}

		/// <summary>Disposes of the train</summary>
		public override void Dispose()
		{
			State = TrainState.DisposePending;
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].ChangeCarSection(CarSectionType.NotVisible);
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
						Cars[i].ChangeCarSection(CarSectionType.Exterior);

						if (Cars[i].TractionModel.ProvidesPower && Cars[i].Sounds.Loop != null)
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
					
					if (Cars[i].CarSections.TryGetValue(Cars[i].CurrentCarSection, out CarSection currentCarSection) && currentCarSection.Groups.Length > 0)
					{
						for (int k = 0; k < currentCarSection.Groups[0].Elements.Length; k++)
						{
							if (currentCarSection.Groups[0].Elements[k].internalObject != null)
							{
								currentCarSection.Groups[0].Elements[k].internalObject.DaytimeNighttimeBlend = dnb;
							}
						}
					}

					AI?.Trigger(TimeElapsed);
				}
			}
			else if (State == TrainState.Bogus)
			{
				// bogus train
				AI?.Trigger(TimeElapsed);
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

			if (Type == TrainType.PreTrain)
			{
				UpdateStation(TimeElapsed);
			}
			// update station and doors
			UpdateDoors(TimeElapsed);

			// Update Run and Motor sounds
			foreach (var Car in Cars)
			{
				Car.Run.Update(TimeElapsed);
				Car.TractionModel?.Update(TimeElapsed);
				for (int j = 0; j < Car.Sounds.ControlledSounds.Count; j++)
				{
					Car.Sounds.ControlledSounds[j].Update(TimeElapsed);
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

		public override void Jump(int stationIndex, int trackIndex)
		{
			Dispose();
			State = TrainState.Pending;
			TrainManagerBase.currentHost.ProcessJump(this, stationIndex, 0);
		}
	}
}
