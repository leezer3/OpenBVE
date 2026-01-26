using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents a reverser handle</summary>
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
		public double MaxWidth = 48;

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
			if (baseTrain.CurrentDirection == TrackDirection.Reverse)
			{
				r = 0 - r;
			}
			if (a != r)
			{
				Driver = (ReverserPosition)r;
				baseTrain.Plugin?.UpdateReverser();
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

			if (a == (int)Driver) return;

			if (!TrainManagerBase.CurrentOptions.Accessibility) return;
			TrainManagerBase.currentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
		}

		/// <summary>Gets the description string for this notch</summary>
		/// <param name="color">The on-screen display color</param>
		/// <returns>The notch description</returns>
		public string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (NotchDescriptions == null || NotchDescriptions.Length < 3)
			{
				switch (Driver)
				{
					case ReverserPosition.Reverse:
						if (baseTrain.CurrentDirection == TrackDirection.Reverse)
						{
							color = MessageColor.Blue;
							return Translations.QuickReferences.HandleForward;
						}
						color = MessageColor.Orange;
						return Translations.QuickReferences.HandleBackward;
					case ReverserPosition.Neutral:
						return Translations.QuickReferences.HandleNeutral;
					case ReverserPosition.Forwards:
						if (baseTrain.CurrentDirection == TrackDirection.Reverse)
						{
							color = MessageColor.Orange;
							return Translations.QuickReferences.HandleBackward;
						}
						color = MessageColor.Blue;
						return Translations.QuickReferences.HandleForward;
				}
			}
			else
			{
				switch (Driver)
				{
					case ReverserPosition.Reverse:
						if (baseTrain.CurrentDirection == TrackDirection.Reverse)
						{
							color = MessageColor.Blue;
							return NotchDescriptions[1];
						}
						color = MessageColor.Orange;
						return NotchDescriptions[2];
					case ReverserPosition.Neutral:
						return NotchDescriptions[0];
					case ReverserPosition.Forwards:
						if (baseTrain.CurrentDirection == TrackDirection.Reverse)
						{
							color = MessageColor.Orange;
							return NotchDescriptions[2];
						}
						color = MessageColor.Blue;
						return NotchDescriptions[1];
				}
			}

			return string.Empty;
		}
	}
}
