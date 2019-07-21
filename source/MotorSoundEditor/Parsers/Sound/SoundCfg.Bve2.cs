using System.Globalization;
using MotorSoundEditor.Simulation.TrainManager;
using OpenBveApi.Math;
using SoundManager;

namespace MotorSoundEditor.Parsers.Sound
{
	internal static class BVE2SoundParser
	{
		/// <summary>Loads the sound set for a BVE2 based train</summary>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		/// <param name="car">The car</param>
		internal static void Parse(string trainFolder, TrainManager.Car car)
		{
			// set sound positions and radii
			Vector3 center = Vector3.Zero;

			// load sounds for all cars
			car.Sounds.Run = SoundCfgParser.TryLoadSoundArray(trainFolder, "Run", ".wav", center, SoundCfgParser.mediumRadius);
			car.Sounds.RunVolume = new double[car.Sounds.Run.Length];

			// motor sound
			CultureInfo Culture = CultureInfo.InvariantCulture;
			car.Sounds.Motor.Position = center;

			for (int j = 0; j < car.Sounds.Motor.Tables.Length; j++)
			{
				for (int k = 0; k < car.Sounds.Motor.Tables[j].Entries.Length; k++)
				{
					int idx = car.Sounds.Motor.Tables[j].Entries[k].SoundIndex;

					if (idx >= 0)
					{
						CarSound snd = new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "Motor" + idx.ToString(Culture) + ".wav"), SoundCfgParser.mediumRadius), center);
						car.Sounds.Motor.Tables[j].Entries[k].Buffer = snd.Buffer;
					}
				}
			}
		}
	}
}
