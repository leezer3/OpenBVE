using OpenBveApi.Textures;

namespace OpenBveApi.Routes
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
		public double TransitionTime;
		/// <summary> The time at which this background should be displayed (Expressed as the number of seconds since midnight)</summary>
		public double Time;
		/// <summary>If using GL3.0, the OpenGL/OpenTK VAO for the background</summary>
		public object VAO;
		/// <summary>If using GL1.2, the index of the display list</summary>
		public int DisplayList = 0;

		/// <summary>Creates a new static background, using the default 0.8s fade-in time</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		/// <param name="Distance">The viewing distance</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double Distance = 0) : this(Texture, Repetition, KeepAspectRatio, 0.8, BackgroundTransitionMode.FadeIn)
		{
			if (Distance > 0)
			{
				BackgroundImageDistance = Distance;
			}
		}

		/// <summary>Creates a new static background</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		/// <param name="transitionTime">The time taken in seconds for the fade-in transition to occur</param>
		/// <param name="Mode">The transition mode</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode) : this(Texture, Repetition, KeepAspectRatio, transitionTime, Mode, -1.0)
		{
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
			TransitionTime = transitionTime;
			this.Mode = Mode;
			this.Time = Time;
		}
		
		/// <summary>Updates the static background</summary>
		/// <param name="SecondsSinceMidnight">The current in-game time</param>
		/// <param name="ElapsedTime">The elapsed time since the last call to this function</param>
		/// <param name="Target">Whether this is the target background or the current background when applying fading</param>
		public override void UpdateBackground(double SecondsSinceMidnight, double ElapsedTime, bool Target)
		{
			if (Target)
			{
				switch (Mode)
				{
					case BackgroundTransitionMode.None:
						CurrentAlpha = 1.0f;
						break;
					case BackgroundTransitionMode.FadeIn:
						CurrentAlpha = (float)(Countdown / TransitionTime);
						break;
					case BackgroundTransitionMode.FadeOut:
						CurrentAlpha = 1.0f - (float)(Countdown / TransitionTime);
						break;
				}
			}
			else
			{
				CurrentAlpha = 1.0f;
			}
		}
	}
}
