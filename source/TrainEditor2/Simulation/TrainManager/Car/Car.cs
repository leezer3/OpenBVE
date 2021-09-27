using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using SoundManager;
using TrainEditor2.Models.Sounds;
using TrainManager.Car;
using TrainManager.Motor;
using TrainManager.Trains;

namespace TrainEditor2.Simulation.TrainManager
{
	public static partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal class Car : CarBase
		{

			public Car() : base(null, 0)
			{
				Sounds = new CarSounds();
				Specs = new CarPhysics();
				Specs.IsMotorCar = true;
			}

			public Car(TrainBase train, int index, double CoefficientOfFriction, double CoefficientOfRollingResistance, double AerodynamicDragCoefficient) : base(train, index, CoefficientOfFriction, CoefficientOfRollingResistance, AerodynamicDragCoefficient)
			{
				throw new NotSupportedException("Should not be called in TrainEditor2");
			}

			public Car(TrainBase train, int index) : base(train, index)
			{
				throw new NotSupportedException("Should not be called in TrainEditor2");
			}

			/// <summary>Initializes a train with the default (empty) set of car sounds</summary>
			internal void InitializeCarSounds()
			{
				Sounds.Run = new Dictionary<int, CarSound>();
				Sounds.Flange = new Dictionary<int, CarSound>();
			}

			internal void UpdateRunSounds(double TimeElapsed, int RunIndex)
			{
				if (Sounds.Run == null || Sounds.Run.Count == 0)
				{
					return;
				}

				const double factor = 0.04; // 90 km/h -> m/s -> 1/x
				double speed = Math.Abs(CurrentSpeed);
				double pitch = speed * factor;
				double baseGain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;

				for (int i = 0; i < Sounds.Run.Count; i++)
				{
					int key = Sounds.Run.ElementAt(i).Key;
					if (key == RunIndex)
					{
						Sounds.Run[key].TargetVolume += 3.0 * TimeElapsed;

						if (Sounds.Run[key].TargetVolume > 1.0)
						{
							Sounds.Run[key].TargetVolume = 1.0;
						}
					}
					else
					{
						Sounds.Run[key].TargetVolume -= 3.0 * TimeElapsed;

						if (Sounds.Run[key].TargetVolume < 0.0)
						{
							Sounds.Run[key].TargetVolume = 0.0;
						}
					}

					double gain = baseGain * Sounds.Run[key].TargetVolume;

					if (Sounds.Run[key].IsPlaying)
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Sounds.Run[key].Source.Pitch = pitch;
							Sounds.Run[key].Source.Volume = gain;
						}
						else
						{
							Sounds.Run[key].Stop();
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						Sounds.Run[key].Play(pitch, gain, this, true);
					}
				}
			}



			internal void ApplySounds()
			{
				//Default sound positions and radii
				double mediumRadius = 10.0;

				//3D center of the car
				Vector3 center = Vector3.Zero;

				// run sound
				foreach (var element in RunSounds)
				{
					Sounds.Run[element.Key] = new CarSound(Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius), center);
				}
				
				// motor sound
				Sounds.Motor.Position = center;
				if (Sounds.Motor is BVEMotorSound motorSound)
				{
					for (int i = 0; i < motorSound.Tables.Length; i++)
					{
						motorSound.Tables[i].Buffer = null;
						motorSound.Tables[i].Source = null;

						for (int j = 0; j < motorSound.Tables[i].Entries.Length; j++)
						{
							MotorElement element = MotorSounds.FirstOrDefault(x => x.Key == motorSound.Tables[i].Entries[j].SoundIndex);

							if (element != null)
							{
								motorSound.Tables[i].Entries[j].Buffer = Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius);
							}
						}
					}
				}
				
			}
		}
	}
}
