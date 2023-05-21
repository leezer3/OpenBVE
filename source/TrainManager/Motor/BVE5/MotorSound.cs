using System;
using OpenBveApi.Sounds;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVE5MotorSound : AbstractMotorSound
	{
		/// <summary>Contains all sound buffers</summary>
		public SoundBuffer[] SoundBuffers;
		/// <summary>Contains all sound sources</summary>
		public SoundSource[] SoundSources;
		/// <summary>The motor sound table</summary>
		public BVE5MotorSoundTableEntry[] MotorSoundTable;
		/// <summary>The motor sound table</summary>
		public BVE5MotorSoundTableEntry[] BrakeSoundTable;

		public BVE5MotorSound(CarBase car) : base(car)
		{
			SoundBuffers = new SoundBuffer[0];
			SoundSources = new SoundSource[0];
		}

		public override void Update(double TimeElapsed)
		{
			if (!Car.Specs.IsMotorCar)
			{
				return;
			}
			double speed = Math.Abs(Car.Specs.PerceivedSpeed) * 3.6; // km/h
			int ndir = Math.Sign(Car.Specs.MotorAcceleration);

			if (ndir == 1)
			{
				BVE5MotorSoundTableEntry entry = MotorSoundTable[0];
				BVE5MotorSoundTableEntry nextEntry = MotorSoundTable[0];
				for (int i = 0; i < MotorSoundTable.Length; i++)
				{
					double nextSpeed = MotorSoundTable[i].Speed;
					if (i < MotorSoundTable.Length - 1)
					{
						nextSpeed = MotorSoundTable[i + 1].Speed;
					}
					if (MotorSoundTable[i].Speed <= speed && nextSpeed >= speed)
					{
						break;
					}
					entry = MotorSoundTable[i];
					if (i < MotorSoundTable.Length - 1)
					{
						nextEntry = MotorSoundTable[i + 1];
					}
				}
				// Pitch / volume are linearly interpolated to the next entry
				double interpolate = (nextEntry.Speed - speed) / (nextEntry.Speed - entry.Speed);
				int maxSounds = Math.Max(entry.Sounds.Length, SoundSources.Length);
				for (int i = 0; i < maxSounds; i++)
				{
					if (i > entry.Sounds.Length - 1 || i < SoundSources.Length && (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0))
					{
						TrainManagerBase.currentHost.StopSound(SoundSources[i]);
					}
					else
					{
						if (i < SoundBuffers.Length && SoundBuffers[i] != null)
						{
							if (i >= SoundSources.Length)
							{
								Array.Resize(ref SoundSources, i + 1);
							}

							double pitch = entry.Sounds[i].Pitch + (nextEntry.Sounds[i].Pitch - entry.Sounds[i].Pitch) * interpolate;
							double gain = entry.Sounds[i].Gain + (nextEntry.Sounds[i].Gain - entry.Sounds[i].Gain) * interpolate;
							/*
							 * Initial gain is that specified by the speed step of the current curve
							 * Now multiply that by the actual acceleration as opposed to the max acceleration to find the absolute
							 * gain
							 */
							double max = Car.Specs.AccelerationCurveMaximum;
							if (max != 0.0)
							{
								double cur = Car.Specs.MotorAcceleration;
								if (cur < 0.0) cur = 0.0;
								gain *= Math.Pow(cur / max, 0.25);
							}

							if (SoundSources[i] != null && SoundSources[i].State != SoundSourceState.Stopped)
							{

								SoundSources[i].Pitch = pitch;
								SoundSources[i].Volume = gain;
							}
							else
							{
								SoundSources[i] = TrainManagerBase.currentHost.PlaySound(SoundBuffers[i], pitch, gain, Position, Car, true) as SoundSource;	
							}
						}
					}
				}
			}
			else if (ndir == -1)
			{
				//Brake
				BVE5MotorSoundTableEntry entry = BrakeSoundTable[0];
				BVE5MotorSoundTableEntry nextEntry = BrakeSoundTable[0];
				for (int i = 0; i < BrakeSoundTable.Length; i++)
				{
					double nextSpeed = BrakeSoundTable[i].Speed;
					if (i < BrakeSoundTable.Length - 1)
					{
						nextSpeed = BrakeSoundTable[i + 1].Speed;
					}
					if (BrakeSoundTable[i].Speed <= speed && nextSpeed >= speed)
					{
						break;
					}
					entry = BrakeSoundTable[i];
					if (i > 0)
					{
						nextEntry = BrakeSoundTable[i - 1];
					}
				}
				// Pitch / volume are linearly interpolated to the next entry
				double interpolate = (entry.Speed - speed) / (entry.Speed - nextEntry.Speed);
				int maxSounds = Math.Max(entry.Sounds.Length, SoundSources.Length);
				for (int i = 0; i < maxSounds; i++)
				{
					if (i > entry.Sounds.Length || i < SoundSources.Length && (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0))
					{
						TrainManagerBase.currentHost.StopSound(SoundSources[i]);
					}
					else
					{
						if (i < SoundBuffers.Length && SoundBuffers[i] != null)
						{
							if (i >= SoundSources.Length)
							{
								Array.Resize(ref SoundSources, i + 1);
							}

							double pitch = entry.Sounds[i].Pitch + (nextEntry.Sounds[i].Pitch - entry.Sounds[i].Pitch) * interpolate;
							double gain = entry.Sounds[i].Gain + (nextEntry.Sounds[i].Gain - entry.Sounds[i].Gain) * interpolate;
							/*
							 * Initial gain is that specified by the speed step of the current curve
							 * Now multiply that by the actual acceleration as opposed to the max acceleration to find the absolute
							 * gain
							 */
							double max = Car.Specs.AccelerationCurveMaximum;
							if (max != 0.0)
							{
								double cur = Car.Specs.MotorAcceleration;
								if (cur < 0.0) cur = 0.0;
								gain *= Math.Pow(cur / max, 0.25);
							}

							if (SoundSources[i] != null && SoundSources[i].State != SoundSourceState.Stopped)
							{
								SoundSources[i].Pitch = pitch;
								SoundSources[i].Volume = gain;
							}
							else
							{
								SoundSources[i] = TrainManagerBase.currentHost.PlaySound(SoundBuffers[i], pitch, gain, Position, Car, true) as SoundSource;
							}
						}
					}
				}
			}
		}

	}
}
