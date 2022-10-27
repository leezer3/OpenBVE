using OpenBveApi.Colors;
using OpenBveApi.Interface;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represnts a reverser handle</summary>
	public class ReverserHandle : AbstractReverser
	{
		/// <summary>Played when the reverser is moved to F or R</summary>
		public CarSound EngageSound;
		/// <summary>Played when the reverser is moved to N</summary>
		public CarSound ReleaseSound;

		public ReverserHandle(TrainBase train) : base(train)
		{
			Driver = (int)ReverserPosition.Neutral;
			Actual = (int)ReverserPosition.Neutral;
			EngageSound = new CarSound();
			ReleaseSound = new CarSound();
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
				Driver = r;
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

		public override void Update()
		{
		}

		/// <summary>Gets the description string for this notch</summary>
		/// <param name="color">The on-screen display color</param>
		/// <returns>The notch description</returns>
		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (NotchDescriptions == null || NotchDescriptions.Length < 3)
			{
				switch ((ReverserPosition)Driver)
				{
					case ReverserPosition.Reverse:
						color = MessageColor.Orange;
						return Translations.QuickReferences.HandleBackward;
					case ReverserPosition.Neutral:
						return Translations.QuickReferences.HandleNeutral;
					case ReverserPosition.Forwards:
						color = MessageColor.Blue;
						return Translations.QuickReferences.HandleForward;
				}
			}
			else
			{
				switch ((ReverserPosition)Driver)
				{
					case ReverserPosition.Reverse:
						color = MessageColor.Orange;
						return NotchDescriptions[2];
					case ReverserPosition.Neutral:
						return NotchDescriptions[0];
					case ReverserPosition.Forwards:
						color = MessageColor.Blue;
						return NotchDescriptions[1];
				}
			}

			return string.Empty;
		}
	}
}
