using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using SoundManager;
using TrainEditor2.Models.Sounds;
using TrainManager.Car;
using TrainManager.Motor;

namespace TrainEditor2.Simulation.TrainManager
{
	public static partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal class Car : AbstractCar
		{
			private readonly Train baseTrain;
			internal CarPhysics Specs;
			internal CarSounds Sounds;

			internal Car(Train baseTrain)
			{
				this.baseTrain = baseTrain;
				this.Specs = new CarPhysics();
				this.Sounds = new CarSounds();
				InitializeCarSounds();
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

			internal void UpdateMotorSounds(bool isPlayTrack1, bool isPlayTrack2)
			{
				Vector3 pos = Sounds.Motor.Position;
				double speed = Math.Abs(Specs.PerceivedSpeed);
				int idx = (int)Math.Round(speed * Sounds.Motor.SpeedConversionFactor);
				int odir = Sounds.Motor.CurrentAccelerationDirection;
				int ndir = Math.Sign(Specs.Acceleration);

				for (int h = 0; h < 2; h++)
				{
					int j = h == 0 ? BVEMotorSound.MotorP1 : BVEMotorSound.MotorP2;
					int k = h == 0 ? BVEMotorSound.MotorB1 : BVEMotorSound.MotorB2;

					if (odir > 0 & ndir <= 0)
					{
						if (j < Sounds.Motor.Tables.Length)
						{
							Program.SoundApi.StopSound(Sounds.Motor.Tables[j].Source);
							Sounds.Motor.Tables[j].Source = null;
							Sounds.Motor.Tables[j].Buffer = null;
						}
					}
					else if (odir < 0 & ndir >= 0)
					{
						if (k < Sounds.Motor.Tables.Length)
						{
							Program.SoundApi.StopSound(Sounds.Motor.Tables[k].Source);
							Sounds.Motor.Tables[k].Source = null;
							Sounds.Motor.Tables[k].Buffer = null;
						}
					}

					if (ndir != 0)
					{
						if (ndir < 0)
						{
							j = k;
						}

						if (j < Sounds.Motor.Tables.Length)
						{
							int idx2 = idx;

							if (idx2 >= Sounds.Motor.Tables[j].Entries.Length)
							{
								idx2 = Sounds.Motor.Tables[j].Entries.Length - 1;
							}

							if ((!isPlayTrack1 && h == 0) || (!isPlayTrack2 && h == 1))
							{
								idx2 = -1;
							}

							if (idx2 >= 0)
							{
								SoundBuffer obuf = Sounds.Motor.Tables[j].Buffer;
								SoundBuffer nbuf = Sounds.Motor.Tables[j].Entries[idx2].Buffer;
								double pitch = Sounds.Motor.Tables[j].Entries[idx2].Pitch;
								double gain = Sounds.Motor.Tables[j].Entries[idx2].Gain;

								if (obuf != nbuf)
								{
									Program.SoundApi.StopSound(Sounds.Motor.Tables[j].Source);

									if (nbuf != null)
									{
										Sounds.Motor.Tables[j].Source = Program.SoundApi.PlaySound(nbuf, pitch, gain, pos, baseTrain, true);
										Sounds.Motor.Tables[j].Buffer = nbuf;
									}
									else
									{
										Sounds.Motor.Tables[j].Source = null;
										Sounds.Motor.Tables[j].Buffer = null;
									}
								}
								else if (nbuf != null)
								{
									if (Sounds.Motor.Tables[j].Source != null)
									{
										Sounds.Motor.Tables[j].Source.Pitch = pitch;
										Sounds.Motor.Tables[j].Source.Volume = gain;
									}
								}
								else
								{
									Program.SoundApi.StopSound(Sounds.Motor.Tables[j].Source);
									Sounds.Motor.Tables[j].Source = null;
									Sounds.Motor.Tables[j].Buffer = null;
								}
							}
							else
							{
								Program.SoundApi.StopSound(Sounds.Motor.Tables[j].Source);
								Sounds.Motor.Tables[j].Source = null;
								Sounds.Motor.Tables[j].Buffer = null;
							}
						}
					}
				}

				Sounds.Motor.CurrentAccelerationDirection = ndir;
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

				for (int i = 0; i < Sounds.Motor.Tables.Length; i++)
				{
					Sounds.Motor.Tables[i].Buffer = null;
					Sounds.Motor.Tables[i].Source = null;

					for (int j = 0; j < Sounds.Motor.Tables[i].Entries.Length; j++)
					{
						MotorElement element = MotorSounds.FirstOrDefault(x => x.Key == Sounds.Motor.Tables[i].Entries[j].SoundIndex);

						if (element != null)
						{
							Sounds.Motor.Tables[i].Entries[j].Buffer = Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius);
						}
					}
				}
			}
		}
	}
}
