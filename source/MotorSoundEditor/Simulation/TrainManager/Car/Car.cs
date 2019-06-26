using System;
using MotorSoundEditor.Audio;
using OpenBveApi.Math;
using SoundManager;

namespace MotorSoundEditor.Simulation.TrainManager
{
	public static partial class TrainManager
	{
		/// <summary>The base class containing the properties of a train car</summary>
		internal class Car
		{
			private readonly Train baseTrain;
			internal CarSpecs Specs;
			internal CarSounds Sounds;

			internal Car(Train baseTrain)
			{
				this.baseTrain = baseTrain;
			}

			/// <summary>Initializes a train with the default (empty) set of car sounds</summary>
			internal void InitializeCarSounds()
			{
				Sounds.Run = new CarSound[] { };
				Sounds.RunVolume = new double[] { };
			}

			internal void UpdateRunSounds(double TimeElapsed, int RunIndex)
			{
				if (Sounds.Run == null || Sounds.Run.Length == 0)
				{
					return;
				}

				const double factor = 0.04; // 90 km/h -> m/s -> 1/x
				double speed = Math.Abs(Specs.CurrentSpeed);
				double pitch = speed * factor;
				double baseGain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;

				for (int j = 0; j < Sounds.Run.Length; j++)
				{
					if (j == RunIndex)
					{
						Sounds.RunVolume[j] += 3.0 * TimeElapsed;

						if (Sounds.RunVolume[j] > 1.0)
						{
							Sounds.RunVolume[j] = 1.0;
						}
					}
					else
					{
						Sounds.RunVolume[j] -= 3.0 * TimeElapsed;

						if (Sounds.RunVolume[j] < 0.0)
						{
							Sounds.RunVolume[j] = 0.0;
						}
					}

					double gain = baseGain * Sounds.RunVolume[j];

					if (Program.Sounds.IsPlaying(Sounds.Run[j].Source))
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Sounds.Run[j].Source.Pitch = pitch;
							Sounds.Run[j].Source.Volume = gain;
						}
						else
						{
							Program.Sounds.StopSound(Sounds.Run[j].Source);
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						SoundsBase.SoundBuffer buffer = Sounds.Run[j].Buffer;

						if (buffer != null)
						{
							Vector3 pos = Sounds.Run[j].Position;
							Sounds.Run[j].Source = Program.Sounds.PlaySound(buffer, pitch, gain, pos, baseTrain, 0, true);
						}
					}
				}
			}

			internal void UpdateMotorSounds(bool isPlayTrack1, bool isPlayTrack2)
			{
				Vector3 pos = Sounds.Motor.Position;
				double speed = Math.Abs(Specs.CurrentPerceivedSpeed);
				int idx = (int)Math.Round(speed * Sounds.Motor.SpeedConversionFactor);
				int odir = Sounds.Motor.CurrentAccelerationDirection;
				int ndir = Math.Sign(Specs.CurrentAccelerationOutput);

				for (int h = 0; h < 2; h++)
				{
					int j = h == 0 ? MotorSound.MotorP1 : MotorSound.MotorP2;
					int k = h == 0 ? MotorSound.MotorB1 : MotorSound.MotorB2;

					if (odir > 0 & ndir <= 0)
					{
						if (j < Sounds.Motor.Tables.Length)
						{
							Program.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
							Sounds.Motor.Tables[j].Source = null;
							Sounds.Motor.Tables[j].Buffer = null;
						}
					}
					else if (odir < 0 & ndir >= 0)
					{
						if (k < Sounds.Motor.Tables.Length)
						{
							Program.Sounds.StopSound(Sounds.Motor.Tables[k].Source);
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
								SoundsBase.SoundBuffer obuf = Sounds.Motor.Tables[j].Buffer;
								SoundsBase.SoundBuffer nbuf = Sounds.Motor.Tables[j].Entries[idx2].Buffer;
								double pitch = Sounds.Motor.Tables[j].Entries[idx2].Pitch;
								double gain = Sounds.Motor.Tables[j].Entries[idx2].Gain;

								if (obuf != nbuf)
								{
									Program.Sounds.StopSound(Sounds.Motor.Tables[j].Source);

									if (nbuf != null)
									{
										Sounds.Motor.Tables[j].Source = Program.Sounds.PlaySound(nbuf, pitch, gain, pos, baseTrain, 0, true);
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
									Program.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
									Sounds.Motor.Tables[j].Source = null;
									Sounds.Motor.Tables[j].Buffer = null;
								}
							}
							else
							{
								Program.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
								Sounds.Motor.Tables[j].Source = null;
								Sounds.Motor.Tables[j].Buffer = null;
							}
						}
					}
				}

				Sounds.Motor.CurrentAccelerationDirection = ndir;
			}
		}
	}
}
