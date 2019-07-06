using System;
using System.Globalization;
using System.IO;
using System.Timers;
using SoundEditor.Parsers.Sound;
using SoundEditor.Parsers.Train;
using SoundEditor.Simulation.TrainManager;
using SoundManager;

namespace SoundEditor
{
	public static class ElapsedTime
	{
		public static DateTime StartTime;

		public static double GetElapsedTime(this DateTime time)
		{
			return (time - StartTime).TotalSeconds;
		}
	}

	public partial class FormEditor
	{
		private void CreateCar(string filePath, TrainDat.Motor motorP1, TrainDat.Motor motorP2, TrainDat.Motor motorB1, TrainDat.Motor motorB2)
		{
			DisposeCar();

			TrainManager.PlayerTrain = new TrainManager.Train();
			TrainManager.PlayerTrain.Car.Sounds.Motor.SpeedConversionFactor = 18.0;
			TrainManager.PlayerTrain.Car.Sounds.Motor.Tables = new TrainManager.MotorSoundTable[4];

			for (int i = 0; i < 4; i++)
			{
				TrainDat.Motor motor;

				switch (i)
				{
					case 0:
						motor = motorP1;
						break;
					case 1:
						motor = motorP2;
						break;
					case 2:
						motor = motorB1;
						break;
					case 3:
						motor = motorB2;
						break;
					default:
						motor = new TrainDat.Motor();
						break;
				}

				TrainManager.PlayerTrain.Car.Sounds.Motor.Tables[i].Entries = new TrainManager.MotorSoundTableEntry[motor.Entries.Length];

				for (int j = 0; j < motor.Entries.Length; j++)
				{
					TrainManager.PlayerTrain.Car.Sounds.Motor.Tables[i].Entries[j].SoundIndex = motor.Entries[j].SoundIndex;
					TrainManager.PlayerTrain.Car.Sounds.Motor.Tables[i].Entries[j].Pitch = (float)(0.01 * motor.Entries[j].Pitch);
					TrainManager.PlayerTrain.Car.Sounds.Motor.Tables[i].Entries[j].Gain = (float)Math.Pow((0.0078125 * motor.Entries[j].Volume), 0.25);
				}
			}

			SoundCfg.ApplySoundCfg(Path.GetDirectoryName(filePath), soundCfg, TrainManager.PlayerTrain.Car);
		}

		private void StartSimulation(bool isPaused)
		{
			oldElapsedTime = 0;
			ElapsedTime.StartTime = DateTime.Now;

			if (!isPaused)
			{
				nowSpeed = startSpeed;
			}

			timer.Enabled = true;
		}

		private void RunSimulation(object sender, ElapsedEventArgs e)
		{
			timer.Enabled = false;

			if (TrainManager.PlayerTrain == null)
			{
				return;
			}

			double nowElapsedTime = e.SignalTime.GetElapsedTime();

			if (oldElapsedTime == 0)
			{
				oldElapsedTime = nowElapsedTime;
			}

			double deltaTime = nowElapsedTime - oldElapsedTime;

			double outputAcceleration = Math.Sign(endSpeed - startSpeed) * acceleration;

			nowSpeed += outputAcceleration * deltaTime;
			double minSpeed = Math.Min(startSpeed, endSpeed);
			double maxSpeed = Math.Max(startSpeed, endSpeed);

			if (isLooping)
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

				if (isConstant)
				{
					nowSpeed = startSpeed;
					outputAcceleration = Math.Sign(endSpeed - startSpeed) * acceleration;
				}
			}
			else
			{
				if (nowSpeed < minSpeed || nowSpeed > maxSpeed)
				{
					if (InvokeRequired)
					{
						Invoke(new Action(buttonStop.PerformClick));
					}
					else
					{
						buttonStop.PerformClick();
					}

					return;
				}
			}

			TrainManager.PlayerTrain.Car.Specs.CurrentSpeed = TrainManager.PlayerTrain.Car.Specs.CurrentPerceivedSpeed = nowSpeed / 3.6;
			TrainManager.PlayerTrain.Car.Specs.CurrentAccelerationOutput = outputAcceleration / 3.6;

			TrainManager.PlayerTrain.Car.UpdateRunSounds(deltaTime, runIndex);

			TrainManager.PlayerTrain.Car.UpdateMotorSounds(isPlayTrack1, isPlayTrack2);

			Program.Sounds.Update(deltaTime, SoundModels.Inverse);

			oldElapsedTime = nowElapsedTime;

			timer.Enabled = true;
		}

		private void DrawSimulation(object sender, ElapsedEventArgs e)
		{
			float rangeVelocity = maxVelocity - minVelocity;

			if (startSpeed <= endSpeed)
			{
				if (nowSpeed < minVelocity || nowSpeed > maxVelocity)
				{
					minVelocity = 10.0f * (float)Math.Round(0.1 * nowSpeed);

					if (minVelocity < 0.0f)
					{
						minVelocity = 0.0f;
					}

					maxVelocity = minVelocity + rangeVelocity;
				}
			}
			else
			{
				if (nowSpeed < minVelocity || nowSpeed > maxVelocity)
				{
					maxVelocity = 10.0f * (float)Math.Round(0.1 * nowSpeed);

					if (maxVelocity < rangeVelocity)
					{
						maxVelocity = rangeVelocity;
					}

					minVelocity = maxVelocity - rangeVelocity;
				}
			}

			textBoxMinVelocity.Text = minVelocity.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVelocity.Text = maxVelocity.ToString(CultureInfo.InvariantCulture);

			if (InvokeRequired)
			{
				Invoke(new Action(DrawPictureBoxDrawArea));
			}
			else
			{
				DrawPictureBoxDrawArea();
			}
		}

		private void StopSimulation()
		{
			timer.Enabled = false;
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
