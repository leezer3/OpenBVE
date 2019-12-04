using System;
using System.ComponentModel;
using System.Linq;
using OpenBveApi.Interface;
using SoundManager;
using TrainEditor2.Simulation.TrainManager;
using TrainEditor2.Systems;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor
	{
		internal void StartSimulation()
		{
			try
			{
				CreateCar();
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
				CurrentSimState = SimulationState.Disable;
				return;
			}

			if (TrainManager.PlayerTrain == null)
			{
				Interface.AddMessage(MessageType.Error, false, "Failed to create train.");
				CurrentSimState = SimulationState.Disable;
				return;
			}

			oldElapsedTime = 0;
			startTime = DateTime.Now;

			if (CurrentSimState != SimulationState.Paused)
			{
				nowSpeed = StartSpeed;
			}

			CurrentSimState = SimulationState.Started;
		}

		internal void PauseSimulation()
		{
			DisposeCar();
			CurrentSimState = SimulationState.Paused;
		}

		internal void StopSimulation()
		{
			DisposeCar();
			CurrentSimState = SimulationState.Stopped;

			IsRefreshGlControl = true;
		}

		private void CreateCar()
		{
			DisposeCar();

			TrainManager.PlayerTrain = new TrainManager.Train();
			TrainManager.PlayerTrain.Car.Sounds.Motor.SpeedConversionFactor = 18.0;
			TrainManager.PlayerTrain.Car.Sounds.Motor.Tables = Tracks.Select(t => Track.EntriesToMotorSoundTable(Track.TrackToEntries(t))).ToArray();
			TrainManager.PlayerTrain.Car.ApplySounds();
		}

		internal void RunSimulation()
		{
			if (TrainManager.PlayerTrain == null)
			{
				return;
			}

			double nowElapsedTime = (DateTime.Now - startTime).TotalSeconds;

			if (oldElapsedTime == 0.0)
			{
				oldElapsedTime = nowElapsedTime;
			}

			double deltaTime = nowElapsedTime - oldElapsedTime;

			double outputAcceleration = Math.Sign(endSpeed - startSpeed) * Acceleration;

			nowSpeed += outputAcceleration * deltaTime;
			double minSpeed = Math.Min(startSpeed, endSpeed);
			double maxSpeed = Math.Max(startSpeed, endSpeed);

			if (IsLoop)
			{
				if (nowSpeed < minSpeed)
				{
					nowSpeed = maxSpeed;
					outputAcceleration = 0.0;
				}

				if (nowSpeed > maxSpeed)
				{
					nowSpeed = minSpeed;
					outputAcceleration = 0.0;
				}

				if (IsConstant)
				{
					nowSpeed = startSpeed;
					outputAcceleration = Math.Sign(endSpeed - startSpeed) * acceleration;
				}
			}
			else
			{
				if (nowSpeed < minSpeed || nowSpeed > maxSpeed)
				{
					StopSimulation();
					return;
				}
			}

			TrainManager.PlayerTrain.Car.Specs.CurrentSpeed = TrainManager.PlayerTrain.Car.Specs.CurrentPerceivedSpeed = nowSpeed / 3.6;
			TrainManager.PlayerTrain.Car.Specs.CurrentAccelerationOutput = outputAcceleration / 3.6;

			TrainManager.PlayerTrain.Car.UpdateRunSounds(deltaTime, RunIndex);

			TrainManager.PlayerTrain.Car.UpdateMotorSounds(IsPlayTrack1, IsPlayTrack2);

			Program.SoundApi.Update(deltaTime, SoundModels.Inverse);

			oldElapsedTime = nowElapsedTime;

			DrawSimulation();
		}

		internal void DrawSimulation()
		{
			double rangeVelocity = MaxVelocity - MinVelocity;

			if (StartSpeed <= EndSpeed)
			{
				if (nowSpeed < MinVelocity || nowSpeed > MaxVelocity)
				{
					minVelocity = 10.0 * Math.Round(0.1 * nowSpeed);

					if (MinVelocity < 0.0)
					{
						minVelocity = 0.0;
					}

					maxVelocity = MinVelocity + rangeVelocity;

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

					return;
				}
			}
			else
			{
				if (nowSpeed < MinVelocity || nowSpeed > MaxVelocity)
				{
					maxVelocity = 10.0 * Math.Round(0.1 * nowSpeed);

					if (MaxVelocity < rangeVelocity)
					{
						maxVelocity = rangeVelocity;
					}

					minVelocity = MaxVelocity - rangeVelocity;

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

					return;
				}
			}

			IsRefreshGlControl = true;
		}

		private void DisposeCar()
		{
			if (TrainManager.PlayerTrain != null)
			{
				TrainManager.PlayerTrain.Dispose();
				TrainManager.PlayerTrain = null;
			}
		}
	}
}
