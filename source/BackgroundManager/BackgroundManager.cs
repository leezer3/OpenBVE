using System;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace BackgroundManager
{
	/// <summary>Represents a handle to an abstract background.</summary>
	public abstract class BackgroundHandle
	{
		/// <summary>Called once a frame to update the current state of the background</summary>
		/// <param name="CurrentTime">The current in-game time</param>
		/// <param name="ElapsedTime">The total elapsed frame time</param>
		/// <param name="Target">Whether this is the target background during a transition (Affects alpha rendering)</param>
		public abstract void UpdateBackground(double CurrentTime, double ElapsedTime, bool Target);

		/// <summary>The current transition mode between backgrounds</summary>
		public BackgroundTransitionMode Mode;

		/// <summary>The current transition countdown</summary>
		public double Countdown = -1;

		/// <summary>The current transition alpha level</summary>
		public float Alpha = 1.0f;

	}

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

		public override void UpdateBackground(double CurrentTime, double ElapsedTime, bool Target)
		{
			if (Target)
			{
				switch (Mode)
				{
					case BackgroundTransitionMode.None:
						Alpha = 1.0f;
						break;
					case BackgroundTransitionMode.FadeIn:
						Alpha = (float) (Countdown / TransitionTime);
						break;
					case BackgroundTransitionMode.FadeOut:
						Alpha = 1.0f - (float) (Countdown / TransitionTime);
						break;
				}
			}
			else
			{
				Alpha = 1.0f;
			}
		}
	}

	/// <summary>Represents a dynamic background, made up of a series of static backgrounds</summary>
	public class DynamicBackground : BackgroundHandle
	{
		/// <summary>The array of backgrounds for this dynamic background</summary>
		public readonly StaticBackground[] Backgrounds;
		/// <summary>The current background in use</summary>
		public int CurrentBackgroundIndex = 0;

		public int PreviousBackgroundIndex = 0;

		/// <summary>Creates a new dynamic background</summary>
		/// <param name="Backgrounds">The list of static backgrounds which make up the dynamic background</param>
		public DynamicBackground(StaticBackground[] Backgrounds)
		{
			this.Backgrounds = Backgrounds;
		}


		public override void UpdateBackground(double CurrentTime, double ElapsedTime, bool Target)
		{
			if (Backgrounds.Length < 2)
			{
				CurrentBackgroundIndex = 0;
				//Renderer.RenderBackground(Backgrounds[CurrentBackgroundIndex], 1.0f, 0.5f);
				return;
			}

			//Convert to absolute time of day
			//Use a while loop as it's possible to run through two days
			while (CurrentTime > 86400)
			{
				CurrentTime -= 86400;
			}

			//Run through the array
			for (int i = 0; i < Backgrounds.Length; i++)
			{

				if (CurrentTime < Backgrounds[i].Time)
				{
					break;
				}

				CurrentBackgroundIndex = i;
			}

			if (CurrentBackgroundIndex != PreviousBackgroundIndex)
			{
				//Update the base class transition mode
				Mode = Backgrounds[CurrentBackgroundIndex].Mode;
				if (Countdown <= 0)
				{
					Countdown = Backgrounds[CurrentBackgroundIndex].TransitionTime;
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
					switch (Backgrounds[CurrentBackgroundIndex].Mode)
					{
						case BackgroundTransitionMode.None:
							//No fade in or out, so just switch
							PreviousBackgroundIndex = CurrentBackgroundIndex;
							break;
						case BackgroundTransitionMode.FadeIn:
							Alpha = 1.0f - (float) (Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
							break;
						case BackgroundTransitionMode.FadeOut:
							Alpha = (float) (Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
							break;
					}
				}
			}
		}
	}

	/// <summary>Represents a background object</summary>
	public class BackgroundObject : BackgroundHandle
	{
		/// <summary>The object used for this background (NOTE: Static objects only)</summary>
		public readonly StaticObject ObjectBackground;

		public readonly double ClipDistance = 0;

		/// <summary>Creates a new background object</summary>
		/// <param name="Object">The object to use for the background</param>
		public BackgroundObject(StaticObject Object)
		{
			this.ObjectBackground = Object;
			//As we are using an object based background, calculate the minimum clip distance
			for (int i = 0; i > Object.Mesh.Vertices.Length; i++)
			{
				double X = Math.Abs(Object.Mesh.Vertices[i].Coordinates.X);
				double Z = Math.Abs(Object.Mesh.Vertices[i].Coordinates.Z);
				if (X < ClipDistance)
				{
					ClipDistance = X;
				}

				if (Z < ClipDistance)
				{
					ClipDistance = Z;
				}
			}
		}

		public override void UpdateBackground(double CurrentTime, double TimeElapsed, bool Target)
		{
			//No updates required
		}
	}

	public enum BackgroundTransitionMode
	{
		/// <summary>No transition is performed</summary>
		None = 0,
		/// <summary>The new background fades in</summary>
		FadeIn = 1,
		/// <summary>The old background fades out</summary>
		FadeOut = 2
	}
}
