using LibRender;
using OpenBveApi.Routes;
using OpenBveApi.Textures;

namespace OpenBve.BackgroundManager
{
	/// <summary>Represents a static background, using the default viewing frustrum</summary>
	public class StaticBackground : BackgroundHandle
	{
		/// <summary>The background texture</summary>
		public Texture Texture;
		/// <summary>The number of times the texture is repeated around the viewing frustrum</summary>
		public double Repetition;
		/// <summary>Whether the texture's aspect ratio should be maintained</summary>
		public bool KeepAspectRatio;
		/// <summary>The time taken to transition to this background</summary>
		public readonly double TransitionTime;
		/// <summary> The time at which this background should be displayed (Expressed as the number of seconds since midnight)</summary>
		public readonly double Time;

		/// <summary>Creates a new static background, using the default 0.8s fade-in time</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio)
		{
			this.Texture = Texture;
			this.Repetition = Repetition;
			this.KeepAspectRatio = KeepAspectRatio;
			this.TransitionTime = 0.8;
			this.Time = -1;
			this.Mode = BackgroundTransitionMode.FadeIn;
		}

		/// <summary>Creates a new static background</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		/// <param name="transitionTime">The time taken in seconds for the fade-in transition to occur</param>
		/// <param name="Mode">The transition mode</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode)
		{
			this.Texture = Texture;
			this.Repetition = Repetition;
			this.KeepAspectRatio = KeepAspectRatio;
			this.TransitionTime = transitionTime;
			this.Time = -1;
			this.Mode = Mode;
		}

		/// <summary>Creates a new static background</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		/// <param name="transitionTime">The time taken in seconds for the fade-in transition to occur</param>
		/// <param name="Mode">The transition mode</param>
		/// <param name="Time">The time at which this background is to be displayed, expressed as the number of seconds since midnight</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode, double Time)
		{
			this.Texture = Texture;
			this.Repetition = Repetition;
			this.KeepAspectRatio = KeepAspectRatio;
			this.TransitionTime = transitionTime;
			this.Mode = Mode;
			this.Time = Time;
		}

		public override void UpdateBackground(double ElapsedTime, bool Target)
		{
			if (Target)
			{
				switch (Mode)
				{
					case BackgroundTransitionMode.None:
						CurrentAlpha = 1.0f;
						break;
					case BackgroundTransitionMode.FadeIn:
						CurrentAlpha = (float) (Countdown / TransitionTime);
						break;
					case BackgroundTransitionMode.FadeOut:
						CurrentAlpha = 1.0f - (float) (Countdown / TransitionTime);
						break;
				}
			}
			else
			{
				CurrentAlpha = 1.0f;
			}
		}

		public override void RenderBackground(float Alpha, float Scale)
		{
			Backgrounds.RenderBackground(this, Alpha, Scale);
		}

		public override void RenderBackground(float Scale)
		{
			Backgrounds.RenderBackground(this, 1.0f, Scale);
		}
	}
}
