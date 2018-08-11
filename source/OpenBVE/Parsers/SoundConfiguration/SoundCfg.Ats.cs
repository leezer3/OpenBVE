using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class SoundCfgParser
	{
		/// <summary>Loads the default ATS plugin sound set</summary>
		/// <param name="train">The train</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		private static void LoadDefaultATSSounds(TrainManager.Train train, string trainFolder)
		{
			Vector3 position = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);
			const double radius = 2.0;
			train.Cars[train.DriverCar].Sounds.Plugin = new TrainManager.CarSound[] {
				new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "ats.wav"), position, radius),
				new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "atscnt.wav"), position, radius),
				new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "ding.wav"), position, radius),
				new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "toats.wav"), position, radius),
				new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "toatc.wav"), position, radius),
				new TrainManager.CarSound(OpenBveApi.Path.CombineFile(trainFolder, "eb.wav"), position, radius)
			};
		}
	}
}
