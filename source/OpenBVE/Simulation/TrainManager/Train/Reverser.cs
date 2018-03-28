namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Applies a reverser notch</summary>
		/// <param name="Train">The train</param>
		/// <param name="Value">The notch to apply</param>
		/// <param name="Relative">Whether this is an absolute value or relative to the previous</param>
		internal static void ApplyReverser(Train Train, int Value, bool Relative)
		{
			int a = Train.Handles.Reverser.Driver;
			int r = Relative ? a + Value : Value;
			if (r < -1) r = -1;
			if (r > 1) r = 1;
			if (a != r)
			{
				Train.Handles.Reverser.Driver = r;
				if (Train.Plugin != null)
				{
					Train.Plugin.UpdateReverser();
				}
				Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
				// sound
				if (a == 0 & r != 0)
				{
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.ReverserOn.Buffer;
					if (buffer == null) return;
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.ReverserOn.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				else if (a != 0 & r == 0)
				{
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.ReverserOff.Buffer;
					if (buffer == null) return;
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.ReverserOff.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
		}
	}
}
