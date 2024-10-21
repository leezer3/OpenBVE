using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Math;
using SoundManager;

namespace TrainManager.Car.Systems
{
	public class Flange
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;
		/// <summary>The flange squeal sounds</summary>
		public Dictionary<int, CarSound> Sounds;
		/// <summary>The current pitch</summary>
		private double Pitch;

		public Flange(CarBase car)
		{
			baseCar = car;
			Sounds = new Dictionary<int, CarSound>();
		}

		public void Update(double TimeElapsed)
		{
			if (Sounds.Count == 0)
			{
				return;
			}

			/*
			 * This determines the amount of flange noise as a result of the angle at which the
			 * line that forms between the axles hits the rail, i.e. the less perpendicular that
			 * line is to the rails, the more flange noise there will be.
			 * */
			Vector3 df = baseCar.FrontAxle.Follower.WorldPosition - baseCar.RearAxle.Follower.WorldPosition;
			df.Normalize();
			double b0 = df.X * baseCar.RearAxle.Follower.WorldSide.X + df.Y * baseCar.RearAxle.Follower.WorldSide.Y + df.Z * baseCar.RearAxle.Follower.WorldSide.Z;
			double b1 = df.X * baseCar.FrontAxle.Follower.WorldSide.X + df.Y * baseCar.FrontAxle.Follower.WorldSide.Y + df.Z * baseCar.FrontAxle.Follower.WorldSide.Z;
			double spd = Math.Abs(baseCar.CurrentSpeed);
			double pitch = 0.5 + 0.04 * spd;
			double b2 = Math.Abs(b0) + Math.Abs(b1);
			double basegain = 0.5 * b2 * b2 * spd * spd;
			/*
			 * This determines additional flange noise as a result of the roll angle of the car
			 * compared to the roll angle of the rails, i.e. if the car bounces due to inaccuracies,
			 * there will be additional flange noise.
			 * */
			double cdti = Math.Abs(baseCar.FrontAxle.Follower.CantDueToInaccuracy) + Math.Abs(baseCar.RearAxle.Follower.CantDueToInaccuracy);
			basegain += 0.2 * spd * spd * cdti * cdti;
			/*
			 * This applies the settings.
			 * */
			if (basegain < 0.0) basegain = 0.0;
			if (basegain > 0.75) basegain = 0.75;
			if (pitch > Pitch)
			{
				Pitch += TimeElapsed;
				if (Pitch > pitch) Pitch = pitch;
			}
			else
			{
				Pitch -= TimeElapsed;
				if (Pitch < pitch) Pitch = pitch;
			}

			pitch = Pitch;
			for (int i = 0; i < Sounds.Count; i++)
			{
				int key = Sounds.ElementAt(i).Key;
				if (Sounds[key] == null)
				{
					continue;
				}

				if (key == baseCar.FrontAxle.FlangeIndex | key == baseCar.RearAxle.FlangeIndex)
				{
					Sounds[key].TargetVolume += TimeElapsed;
					if (Sounds[key].TargetVolume > 1.0) Sounds[key].TargetVolume = 1.0;
				}
				else
				{
					Sounds[key].TargetVolume -= TimeElapsed;
					if (Sounds[key].TargetVolume < 0.0) Sounds[key].TargetVolume = 0.0;
				}

				double gain = basegain * Sounds[key].TargetVolume;
				if (Sounds[key].IsPlaying)
				{
					if (pitch > 0.01 & gain > 0.0001)
					{
						Sounds[key].Source.Pitch = pitch;
						Sounds[key].Source.Volume = gain;
					}
					else
					{
						if (key == baseCar.FrontAxle.FlangeIndex)
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
	}
}
