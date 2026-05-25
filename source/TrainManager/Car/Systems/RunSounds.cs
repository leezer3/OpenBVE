using System;
using System.Collections.Generic;
using System.Linq;
using SoundManager;

namespace TrainManager.Car.Systems
{
	public class RunSounds
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;

		/// <summary>The railtype run sounds</summary>
		public Dictionary<int, CarSound> Sounds;
		/// <summary>The next track position at which the run sounds will be faded</summary>
		public double NextReasynchronizationPosition;
		public RunSounds(CarBase car)
		{
			baseCar = car;
			Sounds = new Dictionary<int, CarSound>();
		}

		/// <summary>Updates all run sounds</summary>
		/// <param name="TimeElapsed">The elapsed time</param>
		public void Update(double TimeElapsed)
		{
			if (Sounds.Count == 0)
			{
				return;
			}

			const double factor = 0.04; // 90 km/h -> m/s -> 1/x
			double speed = Math.Abs(baseCar.CurrentSpeed);
			if (baseCar.Derailed)
			{
				speed = 0.0;
			}

			double pitch = speed * factor;
			double basegain;
			if (baseCar.CurrentSpeed == 0.0)
			{
				if (baseCar.Index != 0)
				{
					NextReasynchronizationPosition = baseCar.baseTrain.Cars[0].FrontAxle.Follower.TrackPosition;
				}
			}
			else if (NextReasynchronizationPosition == double.MaxValue & baseCar.FrontAxle.RunIndex >= 0)
			{
				double distance = Math.Abs(baseCar.FrontAxle.Follower.TrackPosition - TrainManagerBase.Renderer.CameraTrackFollower.TrackPosition);
				const double minDistance = 150.0;
				const double maxDistance = 750.0;
				if (distance > minDistance)
				{
					if (Sounds.TryGetValue(baseCar.FrontAxle.RunIndex, out var runSound) && runSound.Buffer != null)
					{
						if (runSound.Buffer.Duration > 0.0)
						{
							double offset = distance > maxDistance ? 25.0 : 300.0;
							NextReasynchronizationPosition = runSound.Buffer.Duration * Math.Ceiling((baseCar.baseTrain.Cars[0].FrontAxle.Follower.TrackPosition + offset) / runSound.Buffer.Duration);
						}
					}
				}
			}

			if (baseCar.FrontAxle.Follower.TrackPosition >= NextReasynchronizationPosition)
			{
				NextReasynchronizationPosition = double.MaxValue;
				basegain = 0.0;
			}
			else
			{
				basegain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;
			}

			for (int j = 0; j < Sounds.Count; j++)
			{
				int key = Sounds.ElementAt(j).Key;
				if (key == baseCar.FrontAxle.RunIndex | key == baseCar.RearAxle.RunIndex)
				{
					Sounds[key].TargetVolume += 3.0 * TimeElapsed;
					if (Sounds[key].TargetVolume > 1.0) Sounds[key].TargetVolume = 1.0;
				}
				else
				{
					Sounds[key].TargetVolume -= 3.0 * TimeElapsed;
					if (Sounds[key].TargetVolume < 0.0) Sounds[key].TargetVolume = 0.0;
				}

				double gain = basegain * Sounds[key].TargetVolume;
				if (Sounds[key].IsPlaying)
				{
					if (pitch > 0.01 & gain > 0.001)
					{
						Sounds[key].Source.Pitch = pitch;
						Sounds[key].Source.Volume = gain;
					}
					else
					{
						if (key == baseCar.FrontAxle.RunIndex)
						{
							Sounds[key].Pause();
						}
						else
						{
							Sounds[key].Stop();
						}
					}
				}
				else if (pitch > 0.02 & gain > 0.01)
				{
					Sounds[key].Play(pitch, gain, baseCar, true);
				}
			}
		}

		/// <summary>Updates the sounds for a specific run index</summary>
		/// <remarks>Used by TE2 only</remarks>
		/// <param name="TimeElapsed">The elapsed time</param>
		/// <param name="RunIndex">The run index to update</param>
		public void Update(double TimeElapsed, int RunIndex)
		{
			if (Sounds.Count == 0)
			{
				return;
			}

			const double factor = 0.04; // 90 km/h -> m/s -> 1/x
			double speed = Math.Abs(baseCar.CurrentSpeed);
			double pitch = speed * factor;
			double baseGain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;

			for (int i = 0; i < Sounds.Count; i++)
			{
				int key = Sounds.ElementAt(i).Key;
				if (key == RunIndex)
				{
					Sounds[key].TargetVolume += 3.0 * TimeElapsed;

					if (Sounds[key].TargetVolume > 1.0)
					{
						Sounds[key].TargetVolume = 1.0;
					}
				}
				else
				{
					Sounds[key].TargetVolume -= 3.0 * TimeElapsed;

					if (Sounds[key].TargetVolume < 0.0)
					{
						Sounds[key].TargetVolume = 0.0;
					}
				}

				double gain = baseGain * Sounds[key].TargetVolume;

				if (Sounds[key].IsPlaying || Sounds[key].IsPaused)
				{
					if (pitch > 0.01 & gain > 0.001)
					{
						Sounds[key].Source.Pitch = pitch;
						Sounds[key].Source.Volume = gain;
					}
					else
					{
						Sounds[key].Stop();
					}
				}
				else if (pitch > 0.02 & gain > 0.01)
				{
					Sounds[key].Play(pitch, gain, baseCar, true);
				}
			}
		}

		public void Stop()
		{
			for (int j = 0; j < Sounds.Count; j++)
			{
				int key =  Sounds.ElementAt(j).Key;
				TrainManagerBase.currentHost.StopSound(Sounds[key].Source);
			}
		}


	}
}
