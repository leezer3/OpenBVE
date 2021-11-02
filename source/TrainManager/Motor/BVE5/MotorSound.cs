using System;
using OpenBveApi.Sounds;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVE5MotorSound : AbstractMotorSound
	{
		/// <summary>Contains all sound buffers</summary>
		public SoundBuffer[] MotorSoundBuffers;
		/// <summary>Contains all sound sources</summary>
		public SoundSource[] MotorSoundSources;
		/// <summary>The motor sound table</summary>
		public BVE5MotorSoundTableEntry[] MotorSoundTable;
		/// <summary>Contains all sound buffers</summary>
		public SoundBuffer[] BrakeSoundBuffers;
		/// <summary>Contains all sound sources</summary>
		public SoundSource[] BrakeSoundSources;
		/// <summary>The brake sound table</summary>
		public BVE5MotorSoundTableEntry[] BrakeSoundTable;

		public BVE5MotorSound(CarBase car) : base(car)
		{
			MotorSoundBuffers = new SoundBuffer[0];
			MotorSoundSources = new SoundSource[0];
			BrakeSoundBuffers = new SoundBuffer[0];
			BrakeSoundSources = new SoundSource[0];
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
				//Acceleration
				for (int i = 0; i < BrakeSoundSources.Length; i++)
				{
					//Stop any playing brake sounds
					TrainManagerBase.currentHost.StopSound(BrakeSoundSources[i]);
				}

				BVE5MotorSoundTableEntry entry = MotorSoundTable[0];
				BVE5MotorSoundTableEntry nextEntry = MotorSoundTable[0];
				for (int i = 0; i < MotorSoundTable.Length; i++)
				{
					if (MotorSoundTable[i].Speed <= speed && MotorSoundTable[i + 1].Speed >= speed)
					{
						break;
					}
					entry = MotorSoundTable[i];
					if (i < MotorSoundTable.Length)
					{
						nextEntry = MotorSoundTable[i + 1];
					}
				}
				// Pitch / volume are linearly interpolated to the next entry
				double interpolate = (nextEntry.Speed - speed) / (nextEntry.Speed - entry.Speed);
				for (int i = 0; i < entry.Sounds.Length; i++)
				{
					if (i < MotorSoundSources.Length && (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0))
					{
						TrainManagerBase.currentHost.StopSound(MotorSoundSources[i]);
					}
					else
					{
						if (i < MotorSoundBuffers.Length && MotorSoundBuffers[i] != null)
						{
							if (i >= MotorSoundSources.Length)
							{
								Array.Resize(ref MotorSoundSources, i + 1);
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

							if (MotorSoundSources[i] != null && MotorSoundSources[i].State != SoundSourceState.Stopped)
							{

								MotorSoundSources[i].Pitch = pitch;
								MotorSoundSources[i].Volume = gain;
							}
							else
							{
								MotorSoundSources[i] = TrainManagerBase.currentHost.PlaySound(MotorSoundBuffers[i], pitch, gain, Position, Car, true) as SoundSource;	
							}
						}
					}
				}
			}
			else if (ndir == -1)
			{
				//Brake
				for (int i = 0; i < MotorSoundSources.Length; i++)
				{
					//Stop any playing motor sounds
					TrainManagerBase.currentHost.StopSound(MotorSoundSources[i]);
				}

				BVE5MotorSoundTableEntry entry = BrakeSoundTable[0];
				BVE5MotorSoundTableEntry nextEntry = BrakeSoundTable[0];
				for (int i = 0; i < BrakeSoundTable.Length; i++)
				{
					if (BrakeSoundTable[i].Speed <= speed && BrakeSoundTable[i + 1].Speed >= speed)
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
				for (int i = 0; i < entry.Sounds.Length; i++)
				{
					if (i < BrakeSoundBuffers.Length && BrakeSoundBuffers[i] != null)
					{
						if (i >= BrakeSoundSources.Length)
						{
							Array.Resize(ref BrakeSoundSources, i + 1);
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

						if (BrakeSoundSources[i] != null && BrakeSoundSources[i].State != SoundSourceState.Stopped)
						{
							BrakeSoundSources[i].Pitch = pitch;
							BrakeSoundSources[i].Volume = gain;
						}
						else
						{
							BrakeSoundSources[i] = TrainManagerBase.currentHost.PlaySound(BrakeSoundBuffers[i], pitch, gain, Position, Car, true) as SoundSource;
						}
					}
				}
			}
		}

	}
}
