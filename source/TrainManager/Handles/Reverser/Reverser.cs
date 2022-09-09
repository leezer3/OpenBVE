using System;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represnts a reverser handle</summary>
	public class ReverserHandle : AbstractReverser
	{
		/// <summary>The notch set by the driver</summary>
		public new ReverserPosition Driver;
		/// <summary>The actual notch</summary>
		public new ReverserPosition Actual;
		/// <summary>Played when the reverser is moved to F or R</summary>
		public CarSound EngageSound;
		/// <summary>Played when the reverser is moved to N</summary>
		public CarSound ReleaseSound;
		
		public ReverserHandle(TrainBase train) : base(train)
		{
			Driver = ReverserPosition.Neutral;
			Actual = ReverserPosition.Neutral;
			EngageSound = new CarSound();
			ReleaseSound = new CarSound();
		}

		public override string CurrentNotchDescription
		{
			get
			{
				if (NotchDescriptions != null && NotchDescriptions.Length > 2)
				{
					switch (Driver)
					{
						case ReverserPosition.Forwards:
							return NotchDescriptions[3];
						case ReverserPosition.Neutral:
							return NotchDescriptions[2];
						case ReverserPosition.Reverse:
							return NotchDescriptions[1];
					}
				}

				switch (Driver)
				{
					case ReverserPosition.Forwards:
						return "F";
					case ReverserPosition.Neutral:
						return "N";
					default:
						return "B";
				}
			}
		}

		public override void ApplyState(ReverserPosition Value)
		{
			ApplyState((int)Value, false);
		}

		public override void ApplyState(int Value, bool Relative)
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
