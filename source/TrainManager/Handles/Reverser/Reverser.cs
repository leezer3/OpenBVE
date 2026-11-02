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
	public class ReverserHandle : AbstractReverser
	{
		public ReverserHandle(TrainBase train) : base(train)
		{
			Driver = (int)ReverserPosition.Neutral;
			Actual = (int)ReverserPosition.Neutral;
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
			if (baseTrain.CurrentDirection == TrackDirection.Reverse)
			{
				r = 0 - r;
			}
			if (a != r)
			{
				Driver = r;
				baseTrain.Plugin?.UpdateReverser();
				TrainManagerBase.currentHost.AddBlackBoxEntry();
				// sound
				if (a == 0 && r != 0)
				{
					Max.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
				else if (a != 0 && r == 0)
				{
					Min.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}

			if (a == (int)Driver) return;

			if (!TrainManagerBase.CurrentOptions.Accessibility) return;
			TrainManagerBase.currentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
		}
		
		/// <summary>Gets the description string for this notch</summary>
		/// <param name="color">The on-screen display color</param>
		/// <returns>The notch description</returns>
		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (NotchDescriptions == null || NotchDescriptions.Length < 3)
			{
				switch (Driver)
				{
					case (int)ReverserPosition.Reverse:
						if (baseTrain.CurrentDirection == TrackDirection.Reverse)
						{
							color = MessageColor.Blue;
							return Translations.QuickReferences.HandleForward;
						}
						color = MessageColor.Orange;
						return Translations.QuickReferences.HandleBackward;
					case (int)ReverserPosition.Neutral:
						return Translations.QuickReferences.HandleNeutral;
					case (int)ReverserPosition.Forwards:
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
					case (int)ReverserPosition.Reverse:
						if (baseTrain.CurrentDirection == TrackDirection.Reverse)
						{
							color = MessageColor.Blue;
							return NotchDescriptions[1];
						}
						color = MessageColor.Orange;
						return NotchDescriptions[2];
					case (int)ReverserPosition.Neutral:
						return NotchDescriptions[0];
					case (int)ReverserPosition.Forwards:
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

		public override string GetDebugDescription(HandleState state)
		{
			switch (state)
			{
				case HandleState.Actual:
				case HandleState.Safety:
					if (baseTrain.CurrentDirection == TrackDirection.Reverse)
					{
						return Actual == (int)ReverserPosition.Reverse ? "B" : Actual == (int)ReverserPosition.Forwards ? "F" : "N";
					}

					return Actual == (int)ReverserPosition.Reverse ? "F" : Actual == (int)ReverserPosition.Forwards ? "B" : "N";
				case HandleState.Driver:
					if (baseTrain.CurrentDirection == TrackDirection.Reverse)
					{
						return Driver == (int)ReverserPosition.Reverse ? "B" : Driver == (int)ReverserPosition.Forwards ? "F" : "N";
					}
					return Driver == (int)ReverserPosition.Reverse ? "F" : Driver == (int)ReverserPosition.Forwards ? "B" : "N";
			}
			return string.Empty;
		}
	}
}
