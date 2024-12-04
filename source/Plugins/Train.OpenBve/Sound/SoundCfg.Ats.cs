using System.Collections.Generic;
using OpenBveApi.Math;
using SoundManager;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal partial class SoundCfgParser
	{
		/// <summary>Loads the default ATS plugin sound set</summary>
		/// <param name="train">The train</param>
		private void LoadDefaultATSSounds(TrainBase train)
		{
			Vector3 position = new Vector3(train.Cars[train.DriverCar].Driver.X, train.Cars[train.DriverCar].Driver.Y, train.Cars[train.DriverCar].Driver.Z + 1.0);
			const double radius = 2.0;
			train.Cars[train.DriverCar].Sounds.Plugin = new Dictionary<int, CarSound>
			{
				{ 0, new CarSound(Plugin.CurrentHost, train.TrainFolder,"ats.wav", radius, position) },
				{ 1, new CarSound(Plugin.CurrentHost, train.TrainFolder, "atscnt.wav", radius, position) },
				{ 2, new CarSound(Plugin.CurrentHost, train.TrainFolder, "ding.wav", radius, position) },
				{ 3, new CarSound(Plugin.CurrentHost, train.TrainFolder, "toats.wav", radius, position) },
				{ 4, new CarSound(Plugin.CurrentHost, train.TrainFolder, "toatc.wav", radius, position) },
				{ 5, new CarSound(Plugin.CurrentHost, train.TrainFolder, "eb.wav", radius, position) }
			};
		}
	}
}
