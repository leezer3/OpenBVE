using SoundManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represnts a reverser handle</summary>
	public class ReverserHandle
	{
		/// <summary>The notch set by the driver</summary>
		public ReverserPosition Driver;
		/// <summary>The actual notch</summary>
		public ReverserPosition Actual;
		/// <summary>Played when the reverser is moved to F or R</summary>
		public CarSound EngageSound;
		/// <summary>Played when the reverser is moved to N</summary>
		public CarSound ReleaseSound;
		/// <summary>Contains the notch descriptions to be displayed on the in-game UI</summary>
		public string[] NotchDescriptions;
		/// <summary>The max width used in px for the reverser HUD string</summary>
		public int MaxWidth = 48;

		private readonly TrainBase baseTrain;

		public ReverserHandle(TrainBase train)
		{
			Driver = ReverserPosition.Neutral;
			Actual = ReverserPosition.Neutral;
			EngageSound = new CarSound();
			ReleaseSound = new CarSound();
			baseTrain = train;
		}

		public void ApplyState(ReverserPosition Value)
		{
			ApplyState((int)Value, false);
		}

		public void ApplyState(int Value, bool Relative)
		{
			if (baseTrain.Handles.HandleType == HandleType.InterlockedReverserHandle && baseTrain.Handles.Power.Driver != 0)
			{
				return;
			}
			int a = (int)Driver;
			int r = Relative ? a + Value : Value;
			if (r < -1) r = -1;
			if (r > 1) r = 1;
			if (a != r)
			{
				Driver = (ReverserPosition)r;
				if (baseTrain.Plugin != null)
				{
					baseTrain.Plugin.UpdateReverser();
				}
				TrainManagerBase.currentHost.AddBlackBoxEntry();
				// sound
				if (a == 0 & r != 0)
				{
					EngageSound.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
				else if (a != 0 & r == 0)
				{
					ReleaseSound.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}
		}
	}
}
