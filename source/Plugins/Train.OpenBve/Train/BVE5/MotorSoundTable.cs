using TrainManager.Car;
using TrainManager.Motor;

namespace Train.OpenBve
{
	class Bve5MotorSoundTableParser
	{
		/// <summary>Parses a set of BVE5 motor sound tables</summary>
		/// <param name="car">The car</param>
		/// <param name="motorSoundPitch">The motor pitch table</param>
		/// <param name="motorSoundGain">The motor gain table</param>
		/// <param name="brakeSoundPitch">The brake pitch table</param>
		/// <param name="brakeSoundGain">The brake gain table</param>
		internal static BVE5MotorSound Parse(CarBase car, string motorSoundPitch, string motorSoundGain, string brakeSoundPitch, string brakeSoundGain)
		{
			BVE5MotorSound motorSound = new BVE5MotorSound(car);
			ParsePitchTable(motorSoundPitch, ref motorSound.MotorSoundTable);
			ParseVolumeTable(motorSoundPitch, ref motorSound.MotorSoundTable);
			ParsePitchTable(motorSoundPitch, ref motorSound.BrakeSoundTable);
			ParseVolumeTable(motorSoundPitch, ref motorSound.BrakeSoundTable);
			return motorSound;
		}

		private static void ParsePitchTable(string pitchFile, ref BVE5MotorSoundTableEntry[] soundTable)
		{

		}

		private static void ParseVolumeTable(string pitchFile, ref BVE5MotorSoundTableEntry[] soundTable)
		{

		}
	}
}
