using System;
using System.Globalization;
using System.IO;
using System.Text;
using MotorSoundEditor.Simulation.TrainManager;
using OpenBveApi.Math;

namespace MotorSoundEditor.Parsers.Sound
{
	internal static class SoundCfgParser
	{
		//Default sound radii
		internal const double mediumRadius = 10.0;

		/// <summary>Parses the sound configuration file for a train</summary>
		/// <param name="TrainPath">The absolute on-disk path to the train's folder</param>
		/// <param name="Encoding">The train's text encoding</param>
		/// <param name="Car">The car to which to apply the new sound configuration</param>
		internal static void ParseSoundConfig(string TrainPath, Encoding Encoding, TrainManager.Car Car)
		{
			Car.InitializeCarSounds();
			string FileName = OpenBveApi.Path.CombineFile(TrainPath, "sound.xml");

			if (File.Exists(FileName))
			{
				if (SoundXmlParser.ParseTrain(FileName, Car))
				{
					return;
				}
			}

			FileName = OpenBveApi.Path.CombineFile(TrainPath, "sound.cfg");

			if (File.Exists(FileName))
			{
				BVE4SoundParser.Parse(FileName, TrainPath, Encoding, Car);
			}
			else
			{
				BVE2SoundParser.Parse(TrainPath, Car);
			}
		}

		/// <summary>Attempts to load an array of sound files into a car-sound array</summary>
		/// <param name="Folder">The folder the sound files are located in</param>
		/// <param name="FileStart">The first sound file</param>
		/// <param name="FileEnd">The last sound file</param>
		/// <param name="Position">The position the sound is to be emitted from within the car</param>
		/// <param name="Radius">The sound radius</param>
		/// <returns>The new car sound array</returns>
		internal static TrainManager.CarSound[] TryLoadSoundArray(string Folder, string FileStart, string FileEnd, Vector3 Position, double Radius)
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			TrainManager.CarSound[] Sounds = { };

			if (!Directory.Exists(Folder))
			{
				//Detect whether the given folder exists before attempting to load from it
				return Sounds;
			}

			string[] Files = Directory.GetFiles(Folder);

			foreach (string file in Files)
			{
				string a = System.IO.Path.GetFileName(file);

				if (string.IsNullOrEmpty(a))
				{
					return Sounds;
				}

				if (a.Length > FileStart.Length + FileEnd.Length)
				{
					if (a.StartsWith(FileStart, StringComparison.OrdinalIgnoreCase) & a.EndsWith(FileEnd, StringComparison.OrdinalIgnoreCase))
					{
						string b = a.Substring(FileStart.Length, a.Length - FileEnd.Length - FileStart.Length);
						int n;

						if (int.TryParse(b, NumberStyles.Integer, Culture, out n))
						{
							if (n >= 0)
							{
								int m = Sounds.Length;

								if (n >= m)
								{
									Array.Resize(ref Sounds, n + 1);

									for (int j = m; j < n; j++)
									{
										Sounds[j] = TrainManager.CarSound.Empty;
										Sounds[j].Source = null;
									}
								}

								Sounds[n] = new TrainManager.CarSound(file, Position, Radius);
							}
						}
					}
				}
			}

			return Sounds;
		}
	}
}
