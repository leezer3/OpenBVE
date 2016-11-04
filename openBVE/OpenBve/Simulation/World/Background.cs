using System;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	class BackgroundManager
	{
		/// <summary>Represents a handle to an abstract background.</summary>
		internal abstract class BackgroundHandle
		{
			internal abstract void UpdateBackground(double ElapsedTime);

		}

		/// <summary>Represents a static background, using the default viewing frustrum</summary>
		internal class StaticBackground : BackgroundHandle
		{
			internal Textures.Texture Texture;
			internal double Repetition;
			internal bool KeepAspectRatio;
			internal double TransitionTime;
			internal double Time;
			internal double Countdown;
			internal BackgroundTransitionMode Mode;
			/// <summary>Creates a new static background, using the default 0.8s fade-in time</summary>
			/// <param name="Texture">The texture to apply</param>
			/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
			/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
			internal StaticBackground(Textures.Texture Texture, double Repetition, bool KeepAspectRatio)
			{
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
				this.TransitionTime = 0.8;
				this.Time = -1;
			}

			/// <summary>Creates a new static background</summary>
			/// <param name="Texture">The texture to apply</param>
			/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
			/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
			/// <param name="transitionTime">The time taken in seconds for the fade-in transition to occur</param>
			/// <param name="Mode">The transition mode</param>
			internal StaticBackground(Textures.Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode)
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
			internal StaticBackground(Textures.Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode, double Time)
			{
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
				this.TransitionTime = transitionTime;
				this.Mode = Mode;
				this.Time = Time;
			}

			internal override void UpdateBackground(double ElapsedTime)
			{
			}
		}

		/// <summary>Represents a dynamic background, made up of a series of static backgrounds</summary>
		internal class DynamicBackground : BackgroundHandle
		{
			/// <summary>The array of backgrounds for this dynamic background</summary>
			internal StaticBackground[] Backgrounds;
			/// <summary>The current background in use</summary>
			internal int CurrentBackgroundIndex = 0;

			internal int PreviousBackgroundIndex = 0;

			internal float Alpha = 0.0f;
			/// <summary>Creates a new dynamic background</summary>
			/// <param name="Backgrounds">The list of static backgrounds which make up the dynamic background</param>
			internal DynamicBackground(StaticBackground[] Backgrounds)
			{
				this.Backgrounds = Backgrounds;
			}


			internal override void UpdateBackground(double ElapsedTime)
			{
				if (Backgrounds.Length < 2)
				{
					CurrentBackgroundIndex = 0;
					Renderer.RenderBackground(Backgrounds[CurrentBackgroundIndex], 1.0f, 0.5f);
					return;
				}
				var Time = Game.SecondsSinceMidnight;
				//Convert to absolute time of day
				//Use a while loop as it's possible to run through two days
				while (Time > 86400)
				{
					Time -= 86400;
				}
				//Run through the array
				for (int i = 0; i < Backgrounds.Length; i++)
				{

					if (Time < Backgrounds[i].Time)
					{
						break;
					}
					CurrentBackgroundIndex = i;
				}
				if (CurrentBackgroundIndex != PreviousBackgroundIndex)
				{
					if (Backgrounds[CurrentBackgroundIndex].Countdown <= 0)
					{
						Backgrounds[CurrentBackgroundIndex].Countdown = Backgrounds[CurrentBackgroundIndex].TransitionTime;
					}
					//Run the timer
					Backgrounds[CurrentBackgroundIndex].Countdown -= ElapsedTime;
					if (Backgrounds[CurrentBackgroundIndex].Countdown < 0)
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
								Alpha = (float)(1.0 - Backgrounds[CurrentBackgroundIndex].Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
								break;
							case BackgroundTransitionMode.FadeOut:
								Alpha = (float)(Backgrounds[CurrentBackgroundIndex].Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
								break;
						}
					}
				}				
			}
		}

		/// <summary>Represents a background object</summary>
		internal class BackgroundObject : BackgroundHandle
		{
			/// <summary>The object used for this background (NOTE: Static objects only)</summary>
			internal ObjectManager.StaticObject ObjectBackground;

			/// <summary>Creates a new background object</summary>
			/// <param name="Object">The object to use for the background</param>
			internal BackgroundObject(ObjectManager.StaticObject Object)
			{
				this.ObjectBackground = Object;
			}

			internal override void UpdateBackground(double TimeElapsed)
			{
				//Not yet implemented!
				throw new NotImplementedException();
			}
		}

		internal enum BackgroundTransitionMode
		{
			None = 0,
			FadeIn = 1,
			FadeOut = 2
		}

		/// <summary>The currently displayed background texture</summary>
		internal static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);
		/// <summary>The new background texture (Currently fading in)</summary>
		internal static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);
		/// <summary>Defines the time in seconds taken for a new background to fade in</summary>
		internal const double TargetBackgroundDefaultCountdown = 0.8;
		/// <summary>The time remaining before the current background is at 100 % opacity</summary>
		internal static double TargetBackgroundCountdown;
	}
}
