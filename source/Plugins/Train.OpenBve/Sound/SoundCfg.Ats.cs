using OpenBveApi;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal partial class SoundCfgParser
	{
		/// <summary>Loads the default ATS plugin sound set</summary>
		/// <param name="train">The train</param>
		/// <param name="trainFolder">The absolute on-disk path to the train's folder</param>
		private void LoadDefaultATSSounds(TrainBase train, string trainFolder)
		{
			Vector3 position = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);
			const double radius = 2.0;
			train.Cars[train.DriverCar].Sounds.Plugin = new CarSound[] {
				new CarSound(Plugin.currentHost, Path.CombineFile(trainFolder, "ats.wav"), radius, position),
				new CarSound(Plugin.currentHost, Path.CombineFile(trainFolder, "atscnt.wav"), radius, position),
				new CarSound(Plugin.currentHost, Path.CombineFile(trainFolder, "ding.wav"), radius, position),
				new CarSound(Plugin.currentHost, Path.CombineFile(trainFolder, "toats.wav"), radius, position),
				new CarSound(Plugin.currentHost, Path.CombineFile(trainFolder, "toatc.wav"), radius, position),
				new CarSound(Plugin.currentHost, Path.CombineFile(trainFolder, "eb.wav"), radius, position)
			};
		}
	}
}
