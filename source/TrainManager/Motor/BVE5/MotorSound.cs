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
			double speed = Math.Abs(Car.Specs.PerceivedSpeed);
			int idx = (int) Math.Round(speed * 18.0);
			int ndir = Math.Sign(Car.Specs.MotorAcceleration);


			if (ndir == 1)
			{
				//Acceleration
				for (int i = 0; i < BrakeSoundSources.Length; i++)
				{
					//Stop any playing brake sounds
					TrainManagerBase.currentHost.StopSound(BrakeSoundSources[i]);
				}

				BVE5MotorSoundTableEntry entry = MotorSoundTable[idx];
				for (int i = 0; i < entry.Sounds.Length; i++)
				{
					if (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0)
					{
						TrainManagerBase.currentHost.StopSound(MotorSoundSources[i]);
					}
					else
					{
						MotorSoundSources[i] = TrainManagerBase.currentHost.PlaySound(MotorSoundBuffers[i], entry.Sounds[i].Pitch, entry.Sounds[i].Gain, Position, Car, true) as SoundSource;
					}
				}
			}
			else if (ndir == -1)
			{
				//Brake
				for (int i = 0; i < MotorSoundSources.Length; i++)
				{
					//Stop any playing power sounds
					TrainManagerBase.currentHost.StopSound(MotorSoundSources[i]);
				}

				BVE5MotorSoundTableEntry entry = BrakeSoundTable[idx];
				for (int i = 0; i < entry.Sounds.Length; i++)
				{
					if (entry.Sounds[i].Pitch == 0 || entry.Sounds[i].Gain == 0)
					{
						TrainManagerBase.currentHost.StopSound(BrakeSoundSources[i]);
					}
					else
					{
						BrakeSoundSources[i] = TrainManagerBase.currentHost.PlaySound(BrakeSoundBuffers[i], entry.Sounds[i].Pitch, entry.Sounds[i].Gain, Position, Car, true) as SoundSource;
					}
				}
			}
		}

	}
}
