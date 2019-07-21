using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MotorSoundEditor.Audio;
using MotorSoundEditor.Simulation.TrainManager;
using OpenBveApi.Math;
using SoundManager;

namespace MotorSoundEditor.Parsers.Sound
{
	internal class BVE4SoundParser
	{
		/// <summary>Loads the sound set for a BVE4 or openBVE sound.cfg based train</summary>
		/// <param name="FileName">The absolute on-disk path to the sound.cfg file</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		/// <param name="Encoding">The text encoding for the sound.cfg file</param>
		/// <param name="car">The car</param>
		internal static void Parse(string FileName, string trainFolder, Encoding Encoding, TrainManager.Car car)
		{
			//Default sound positions and radii

			//3D center of the car
			Vector3 center = Vector3.Zero;

			// parse configuration file
			CultureInfo Culture = CultureInfo.InvariantCulture;
			List<string> Lines = File.ReadAllLines(FileName, Encoding).ToList();

			for (int i = Lines.Count - 1; i >= 0; i--)
			{
				/*
				 * Strip comments and remove empty resulting lines etc.
				 *
				 * This fixes an error with some NYCTA content, which has
				 * a copyright notice instead of the file header specified....
				 */
				int j = Lines[i].IndexOf(';');

				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim();
				}
				else
				{
					Lines[i] = Lines[i].Trim();
				}

				if (string.IsNullOrEmpty(Lines[i]))
				{
					Lines.RemoveAt(i);
				}
			}

			string[] MotorFiles = new string[] { };

			for (int i = 0; i < Lines.Count; i++)
			{
				switch (Lines[i].ToLowerInvariant())
				{
					case "[run]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;

								if (int.TryParse(a, NumberStyles.Integer, Culture, out k))
								{
									if (b.Length != 0 && !OpenBveApi.Path.ContainsInvalidChars(b))
									{
										if (k >= 0)
										{
											int n = car.Sounds.Run.Length;

											if (k >= n)
											{
												Array.Resize(ref car.Sounds.Run, k + 1);

												for (int h = n; h < k; h++)
												{
													car.Sounds.Run[h] = new CarSound();
												}
											}

											car.Sounds.Run[k] = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, b), SoundCfgParser.mediumRadius), center);
										}
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[motor]":
						i++;

						while (i < Lines.Count && !Lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = Lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;

								if (int.TryParse(a, NumberStyles.Integer, Culture, out k))
								{
									if (b.Length != 0 && !OpenBveApi.Path.ContainsInvalidChars(b))
									{
										if (k >= 0)
										{
											if (k >= MotorFiles.Length)
											{
												Array.Resize(ref MotorFiles, k + 1);
											}

											MotorFiles[k] = OpenBveApi.Path.CombineFile(trainFolder, b);

											if (!File.Exists(MotorFiles[k]))
											{
												MotorFiles[k] = null;
											}
										}
									}
								}
							}

							i++;
						}

						i--;
						break;
				}
			}

			car.Sounds.RunVolume = new double[car.Sounds.Run.Length];

			// motor sound
			car.Sounds.Motor.Position = center;

			for (int i = 0; i < car.Sounds.Motor.Tables.Length; i++)
			{
				car.Sounds.Motor.Tables[i].Buffer = null;
				car.Sounds.Motor.Tables[i].Source = null;

				for (int j = 0; j < car.Sounds.Motor.Tables[i].Entries.Length; j++)
				{
					int index = car.Sounds.Motor.Tables[i].Entries[j].SoundIndex;

					if (index >= 0 && index < MotorFiles.Length && MotorFiles[index] != null)
					{
						car.Sounds.Motor.Tables[i].Entries[j].Buffer = Program.Sounds.RegisterBuffer(MotorFiles[index], SoundCfgParser.mediumRadius);
					}
				}
			}

		}
	}
}
