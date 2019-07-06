using System;
using System.IO;
using System.Text;
using OpenBveApi.Math;
using SoundEditor.Simulation.TrainManager;
using Path = OpenBveApi.Path;

namespace SoundEditor.Parsers.Sound
{
	internal static partial class SoundCfg
	{
		/// <summary>Parses the sound configuration file for a train</summary>
		/// <param name="TrainPath">The absolute on-disk path to the train's folder</param>
		/// <param name="Encoding">The train's text encoding</param>
		internal static Sounds ParseSoundCfg(string TrainPath, Encoding Encoding, bool IsPreferXml, out bool IsXml)
		{
			Sounds result = new Sounds();

			string xmlFileName = Path.CombineFile(TrainPath, "sound.xml");
			string cfgFileName = Path.CombineFile(TrainPath, "sound.cfg");

			if (IsPreferXml)
			{
				if (File.Exists(xmlFileName))
				{
					ParseXml(xmlFileName, result);
					IsXml = true;
				}
				else
				{
					if (File.Exists(cfgFileName))
					{
						ParseBve4(cfgFileName, Encoding, result);
					}
					else
					{
						ParseBve2(TrainPath, result);
					}

					IsXml = false;
				}
			}
			else
			{
				if (File.Exists(cfgFileName))
				{
					ParseBve4(cfgFileName, Encoding, result);
					IsXml = false;
				}
				else if (File.Exists(xmlFileName))
				{
					ParseXml(xmlFileName, result);
					IsXml = true;
				}
				else
				{
					ParseBve2(TrainPath, result);
					IsXml = false;
				}
			}

			return result;
		}

		internal static void SaveSoundCfg(string TrainPath, Sounds Sounds, bool IsXml = false)
		{
			if (IsXml)
			{
				WriteXml(Sounds, Path.CombineFile(TrainPath, "sound.xml"));
			}
			else
			{
				WriteCfg(Sounds, Path.CombineFile(TrainPath, "sound.cfg"));
			}
		}

		internal static void ApplySoundCfg(string TrainPath, Sounds Sounds, TrainManager.Car Car)
		{
			Car.InitializeCarSounds();

			//Default sound positions and radii
			double mediumRadius = 10.0;

			//3D center of the car
			Vector3 center = Vector3.Zero;

			// run sound
			foreach (IndexedSound sound in Sounds.Run)
			{
				int n = Car.Sounds.Run.Length;

				if (sound.Index >= n)
				{
					Array.Resize(ref Car.Sounds.Run, sound.Index + 1);

					for (int h = n; h < sound.Index; h++)
					{
						Car.Sounds.Run[h] = TrainManager.CarSound.Empty;
					}
				}

				Car.Sounds.Run[sound.Index] = new TrainManager.CarSound(Path.CombineFile(TrainPath, sound.FileName), center, mediumRadius);
			}

			Car.Sounds.RunVolume = new double[Car.Sounds.Run.Length];


			// motor sound
			Car.Sounds.Motor.Position = center;

			for (int i = 0; i < Car.Sounds.Motor.Tables.Length; i++)
			{
				Car.Sounds.Motor.Tables[i].Buffer = null;
				Car.Sounds.Motor.Tables[i].Source = null;

				for (int j = 0; j < Car.Sounds.Motor.Tables[i].Entries.Length; j++)
				{
					int index = Sounds.Motor.FindIndex(s => s.Index == Car.Sounds.Motor.Tables[i].Entries[j].SoundIndex);

					if (index >= 0)
					{
						Car.Sounds.Motor.Tables[i].Entries[j].Buffer = Program.Sounds.RegisterBuffer(Path.CombineFile(TrainPath, Sounds.Motor[index].FileName), mediumRadius);
					}
				}
			}
		}
	}
}
