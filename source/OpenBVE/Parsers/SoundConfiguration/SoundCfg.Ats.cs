using OpenBveApi.Math;
using SoundManager;

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
			train.Cars[train.DriverCar].Sounds.Plugin = new CarSound[] {
				new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "ats.wav"), radius), position),
				new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "atscnt.wav"), radius), position),
				new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "ding.wav"), radius), position),
				new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "toats.wav"), radius), position),
				new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "toatc.wav"), radius), position),
				new CarSound(Program.Sounds.RegisterBuffer(OpenBveApi.Path.CombineFile(trainFolder, "eb.wav"), radius), position)
			};
		}
	}
}
