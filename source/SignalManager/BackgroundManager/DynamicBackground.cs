using LibRender;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;

namespace OpenBve.BackgroundManager
{
	/// <summary>Represents a dynamic background, made up of a series of static backgrounds</summary>
	public class DynamicBackground : BackgroundHandle
	{
		/// <summary>The array of backgrounds for this dynamic background</summary>
		public readonly StaticBackground[] staticBackgrounds;
		/// <summary>The current background in use</summary>
		public int CurrentBackgroundIndex = 0;

		public int PreviousBackgroundIndex = 0;

		/// <summary>Creates a new dynamic background</summary>
		/// <param name="staticBackgrounds">The list of static backgrounds which make up the dynamic background</param>
		public DynamicBackground(StaticBackground[] staticBackgrounds)
		{
			this.staticBackgrounds = staticBackgrounds;
		}


		public override void UpdateBackground(double ElapsedTime, bool Target)
		{
			if (staticBackgrounds.Length < 2)
			{
				CurrentBackgroundIndex = 0;
				//Renderer.RenderBackground(Backgrounds[CurrentBackgroundIndex], 1.0f, 0.5f);
				return;
			}

			var Time = 0; //TODO
			//Convert to absolute time of day
			//Use a while loop as it's possible to run through two days
			while (Time > 86400)
			{
				Time -= 86400;
			}

			//Run through the array
			for (int i = 0; i < staticBackgrounds.Length; i++)
			{

				if (Time < staticBackgrounds[i].Time)
				{
					break;
				}

				CurrentBackgroundIndex = i;
			}

			if (CurrentBackgroundIndex != PreviousBackgroundIndex)
			{
				//Update the base class transition mode
				Mode = staticBackgrounds[CurrentBackgroundIndex].Mode;
				if (Countdown <= 0)
				{
					Countdown = staticBackgrounds[CurrentBackgroundIndex].TransitionTime;
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
					switch (staticBackgrounds[CurrentBackgroundIndex].Mode)
					{
						case BackgroundTransitionMode.None:
							//No fade in or out, so just switch
							PreviousBackgroundIndex = CurrentBackgroundIndex;
							break;
						case BackgroundTransitionMode.FadeIn:
							CurrentAlpha = 1.0f - (float) (Countdown / staticBackgrounds[CurrentBackgroundIndex].TransitionTime);
							break;
						case BackgroundTransitionMode.FadeOut:
							CurrentAlpha = (float) (Countdown / staticBackgrounds[CurrentBackgroundIndex].TransitionTime);
							break;
					}
				}
			}
		}

		public override void RenderBackground(float Alpha, float Scale)
		{
			Backgrounds.RenderBackground(this.staticBackgrounds[CurrentBackgroundIndex], Alpha, Scale);
		}

		public override void RenderBackground(float Scale)
		{
			if (this.CurrentBackgroundIndex != this.PreviousBackgroundIndex)
			{
				switch (this.staticBackgrounds[CurrentBackgroundIndex].Mode)
				{
					case BackgroundTransitionMode.FadeIn:
						Backgrounds.RenderBackground(this.staticBackgrounds[PreviousBackgroundIndex], 1.0f, Scale);
						Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
						Backgrounds.RenderBackground(this.staticBackgrounds[CurrentBackgroundIndex], this.CurrentAlpha, Scale);
						break;
					case BackgroundTransitionMode.FadeOut:
						Backgrounds.RenderBackground(this.staticBackgrounds[CurrentBackgroundIndex], 1.0f, Scale);
						Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
						Backgrounds.RenderBackground(this.staticBackgrounds[PreviousBackgroundIndex], this.CurrentAlpha, Scale);
						break;
				}
			}
			else
			{
				LibRender.Backgrounds.RenderBackground(this.staticBackgrounds[CurrentBackgroundIndex], 1.0f, Scale);
			}
		}
	}
}
