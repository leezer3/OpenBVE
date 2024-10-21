using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Math;
using SoundManager;
using TrainEditor2.Models.Sounds;
using TrainManager.Car;
using TrainManager.Motor;
using TrainManager.Trains;

namespace TrainEditor2.Simulation.TrainManager
{
	public partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal class Car : CarBase
		{

			public Car() : base(null, 0)
			{
				Sounds = new CarSounds();
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
				Flange.Sounds = new Dictionary<int, CarSound>();
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
					Run.Sounds[element.Key] = new CarSound(Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius), center);
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
