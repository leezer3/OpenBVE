using System;
using System.Linq;
using OpenBveApi.Math;
using SoundManager;
using TrainEditor2.Models.Sounds;

namespace TrainEditor2.Simulation.TrainManager
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

					if (Program.SoundApi.IsPlaying(Sounds.Run[j].Source))
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Sounds.Run[j].Source.Pitch = pitch;
							Sounds.Run[j].Source.Volume = gain;
						}
						else
						{
							Program.SoundApi.StopSound(Sounds.Run[j]);
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						SoundBuffer buffer = Sounds.Run[j].Buffer;

						if (buffer != null)
						{
							Vector3 pos = Sounds.Run[j].Position;
							Sounds.Run[j].Source = Program.SoundApi.PlaySound(buffer, pitch, gain, pos, baseTrain, true);
						}
					}
				}
			}

			internal void UpdateMotorSounds(bool[] isPlayPowerTables, bool[] isPlayBrakeTables)
			{
				float speed = (float)Math.Abs(Specs.CurrentPerceivedSpeed);
				int oDir = Sounds.Motor.CurrentAccelerationDirection;
				int nDir = Math.Sign(Specs.CurrentAccelerationOutput);

				if (oDir > 0 & nDir <= 0)
				{
					foreach (MotorSound.Table table in Sounds.Motor.PowerTables)
					{
						Program.SoundApi.StopSound(table.PlayingSource);
						table.PlayingSource = null;
						table.PlayingBuffer = null;
					}
				}
				else if (oDir < 0 & nDir >= 0)
				{
					foreach (MotorSound.Table table in Sounds.Motor.BrakeTables)
					{
						Program.SoundApi.StopSound(table.PlayingSource);
						table.PlayingSource = null;
						table.PlayingBuffer = null;
					}
				}

				if (nDir != 0)
				{
					MotorSound.Table[] tables = nDir > 0 ? Sounds.Motor.PowerTables : Sounds.Motor.BrakeTables;
					bool[] isPlayTables = nDir > 0 ? isPlayPowerTables : isPlayBrakeTables;

					for (int i = 0; i < tables.Length; i++)
					{
						MotorSound.Table table = tables[i];
						MotorSound.Entry entry = table.GetEntry(speed);

						if (!isPlayTables[i])
						{
							entry.Buffer = null;
						}

						if (entry.Buffer != table.PlayingBuffer)
						{
							Program.SoundApi.StopSound(table.PlayingSource);
							if (entry.Buffer != null)
							{
								table.PlayingSource = Program.SoundApi.PlaySound(entry.Buffer, entry.Pitch, entry.Gain, Sounds.Motor.Position, baseTrain, true);
								table.PlayingBuffer = entry.Buffer;
							}
							else
							{
								table.PlayingSource = null;
								table.PlayingBuffer = null;
							}
						}
						else if (entry.Buffer != null)
						{
							if (table.PlayingSource != null)
							{
								table.PlayingSource.Pitch = entry.Pitch;
								table.PlayingSource.Volume = entry.Gain;
							}
						}
						else
						{
							Program.SoundApi.StopSound(table.PlayingSource);
							table.PlayingSource = null;
							table.PlayingBuffer = null;
						}
					}
				}

				Sounds.Motor.CurrentAccelerationDirection = nDir;
			}

			internal void ApplySounds()
			{
				InitializeCarSounds();

				//Default sound positions and radii
				double mediumRadius = 10.0;

				//3D center of the car
				Vector3 center = Vector3.Zero;

				// run sound
				foreach (var element in RunSounds)
				{
					int n = Sounds.Run.Length;

					if (element.Key >= n)
					{
						Array.Resize(ref Sounds.Run, element.Key + 1);

						for (int h = n; h < element.Key; h++)
						{
							Sounds.Run[h] = new CarSound();
						}
					}

					Sounds.Run[element.Key] = new CarSound(Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius), center);
				}

				Sounds.RunVolume = new double[Sounds.Run.Length];


				// motor sound
				Sounds.Motor.Position = center;

				foreach (MotorSound.Table table in Sounds.Motor.PowerTables)
				{
					table.PlayingBuffer = null;
					table.PlayingSource = null;

					foreach (MotorSound.Vertex<int, SoundBuffer> vertex in table.BufferVertices)
					{
						MotorElement element = MotorSounds.FirstOrDefault(x => x.Key == vertex.Y);

						if (element != null)
						{
							vertex.Z = Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius);
						}
					}
				}

				foreach (MotorSound.Table table in Sounds.Motor.BrakeTables)
				{
					table.PlayingBuffer = null;
					table.PlayingSource = null;

					foreach (MotorSound.Vertex<int, SoundBuffer> vertex in table.BufferVertices)
					{
						MotorElement element = MotorSounds.FirstOrDefault(x => x.Key == vertex.Y);

						if (element != null)
						{
							vertex.Z = Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius);
						}
					}
				}
			}
		}
	}
}
