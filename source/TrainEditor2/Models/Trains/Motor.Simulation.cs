using System;
using System.ComponentModel;
using System.Linq;
using OpenBveApi.Interface;
using SoundManager;
using TrainEditor2.Systems;
using TrainManager.Motor;

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

			if (Simulation.TrainManager.TrainManager.PlayerTrain == null)
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

			Simulation.TrainManager.TrainManager.PlayerTrain = new Simulation.TrainManager.TrainManager.Train();
			Simulation.TrainManager.TrainManager.PlayerTrain.Car.TractionModel.MotorSounds = new BVEMotorSound(Simulation.TrainManager.TrainManager.PlayerTrain.Car, 18.0, Tracks.Select(t => Track.EntriesToMotorSoundTable(Track.TrackToEntries(t))).ToArray());
			Simulation.TrainManager.TrainManager.PlayerTrain.Car.ApplySounds();
		}

		internal void RunSimulation()
		{
			if (Simulation.TrainManager.TrainManager.PlayerTrain == null)
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

			Simulation.TrainManager.TrainManager.PlayerTrain.Car.CurrentSpeed = Simulation.TrainManager.TrainManager.PlayerTrain.Car.Specs.PerceivedSpeed = nowSpeed / 3.6;
			Simulation.TrainManager.TrainManager.PlayerTrain.Car.TractionModel.CurrentAcceleration = outputAcceleration / 3.6;

			Simulation.TrainManager.TrainManager.PlayerTrain.Car.Run.Update(deltaTime, RunIndex);

			if (Simulation.TrainManager.TrainManager.PlayerTrain.Car.TractionModel.MotorSounds is BVEMotorSound motorSound)
			{
				motorSound.PlayFirstTrack = IsPlayTrack1;
				motorSound.PlaySecondTrack = IsPlayTrack2;
			}
			Simulation.TrainManager.TrainManager.PlayerTrain.Car.TractionModel.MotorSounds.Update(0.0);

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
			if (Simulation.TrainManager.TrainManager.PlayerTrain != null)
			{
				Program.SoundApi.StopAllSounds();
				Simulation.TrainManager.TrainManager.PlayerTrain.Dispose();
				Simulation.TrainManager.TrainManager.PlayerTrain = null;
			}
		}
	}
}
