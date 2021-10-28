using System;
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
		}

		public override void Update(double TimeElapsed)
		{
			if (!Car.Specs.IsMotorCar)
			{
				return;
			}
			double speed = Math.Round(Math.Abs(Car.Specs.PerceivedSpeed) * 3.6); // km/h
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
				for (int i = 0; i < MotorSoundTable.Length; i++)
				{
					if (MotorSoundTable[i].Speed < speed)
					{
						break;
					}
					entry = MotorSoundTable[i];
				}
				for (int i = 0; i < entry.Sounds.Length; i++)
				{
					if (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0)
					{
						TrainManagerBase.currentHost.StopSound(MotorSoundSources[i]);
					}
					else
					{
						if (MotorSoundBuffers[i] != null)
						{
							MotorSoundSources[i] = TrainManagerBase.currentHost.PlaySound(MotorSoundBuffers[i], entry.Sounds[i].Pitch, entry.Sounds[i].Gain, Position, Car, true) as SoundSource;
						}
					}
				}
			}
			else if (ndir == -1)
			{
				//Brake
				for (int i = 0; i < BrakeSoundSources.Length; i++)
				{
					//Stop any playing brake sounds
					TrainManagerBase.currentHost.StopSound(BrakeSoundSources[i]);
				}

				BVE5MotorSoundTableEntry entry = BrakeSoundTable[0];
				for (int i = 0; i < BrakeSoundTable.Length; i++)
				{
					if (BrakeSoundTable[i].Speed < speed)
					{
						break;
					}
					entry = BrakeSoundTable[i];
				}
				for (int i = 0; i < entry.Sounds.Length; i++)
				{
					if (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0)
					{
						TrainManagerBase.currentHost.StopSound(BrakeSoundSources[i]);
					}
					else
					{
						if (BrakeSoundBuffers[i] != null)
						{
							BrakeSoundSources[i] = TrainManagerBase.currentHost.PlaySound(BrakeSoundBuffers[i], entry.Sounds[i].Pitch, entry.Sounds[i].Gain, Position, Car, true) as SoundSource;
						}
					}
				}
			}
		}

	}
}
