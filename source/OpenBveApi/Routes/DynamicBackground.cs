namespace OpenBveApi.Routes
{
	/// <summary>Represents a dynamic background, made up of a series of static backgrounds</summary>
	public class DynamicBackground : BackgroundHandle
	{
		/// <summary>The array of backgrounds for this dynamic background</summary>
		public readonly StaticBackground[] StaticBackgrounds;
		/// <summary>The current background in use</summary>
		public int CurrentBackgroundIndex = 0;
		/// <summary>The previous background in use</summary>
		public int PreviousBackgroundIndex = 0;

		/// <summary>Creates a new dynamic background</summary>
		/// <param name="staticBackgrounds">The list of static backgrounds which make up the dynamic background</param>
		public DynamicBackground(StaticBackground[] staticBackgrounds)
		{
			StaticBackgrounds = staticBackgrounds;
		}

		/// <inheritdoc/>
		public override void UpdateBackground(double SecondsSinceMidnight, double ElapsedTime, bool Target)
		{
			if (StaticBackgrounds.Length < 2)
			{
				CurrentBackgroundIndex = 0;
				return;
			}

			double Time = SecondsSinceMidnight;
			
			//Convert to absolute time of day
			Time %= 86400.0;

			//Run through the array
			for (int i = 0; i < StaticBackgrounds.Length; i++)
			{
				if (Time < StaticBackgrounds[i].Time)
				{
					break;
				}

				CurrentBackgroundIndex = i;
			}

			if (CurrentBackgroundIndex != PreviousBackgroundIndex)
			{
				//Update the base class transition mode
				Mode = StaticBackgrounds[CurrentBackgroundIndex].Mode;

				if (Countdown <= 0)
				{
					Countdown = StaticBackgrounds[CurrentBackgroundIndex].TransitionTime;
				}

				//Run the timer
				Countdown -= ElapsedTime;

				if (Countdown < 0)
				{
					//Countdown has expired
					PreviousBackgroundIndex = CurrentBackgroundIndex;
				}
				else
				{
					switch (StaticBackgrounds[CurrentBackgroundIndex].Mode)
					{
						case BackgroundTransitionMode.None:
							//No fade in or out, so just switch
							PreviousBackgroundIndex = CurrentBackgroundIndex;
							break;
						case BackgroundTransitionMode.FadeIn:
							CurrentAlpha = 1.0f - (float)(Countdown / StaticBackgrounds[CurrentBackgroundIndex].TransitionTime);
							break;
						case BackgroundTransitionMode.FadeOut:
							CurrentAlpha = (float)(Countdown / StaticBackgrounds[CurrentBackgroundIndex].TransitionTime);
							break;
					}
				}
			}
		}
	}
}
