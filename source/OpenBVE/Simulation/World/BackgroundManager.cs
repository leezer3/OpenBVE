using System;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	class BackgroundManager
	{
		/// <summary>Represents a handle to an abstract background.</summary>
		internal abstract class BackgroundHandle
		{
			/// <summary>Called once a frame to update the current state of the background</summary>
			/// <param name="ElapsedTime">The total elapsed frame time</param>
			/// <param name="Target">Whether this is the target background during a transition (Affects alpha rendering)</param>
			internal abstract void UpdateBackground(double ElapsedTime, bool Target);

			/// <summary>Renders the background with the specified level of alpha and scale</summary>
			/// <param name="Alpha">The alpha</param>
			/// <param name="Scale">The scale</param>
			internal abstract void RenderBackground(float Alpha, float Scale);

			/// <summary>Renders the background with the specified scale</summary>
			/// <param name="Scale">The scale</param>
			internal abstract void RenderBackground(float Scale);

			/// <summary>The current transition mode between backgrounds</summary>
			internal BackgroundTransitionMode Mode;

			/// <summary>The current transition countdown</summary>
			internal double Countdown = -1;

			/// <summary>The current transition alpha level</summary>
			internal float Alpha = 1.0f;

		}

		/// <summary>Represents a static background, using the default viewing frustrum</summary>
		internal class StaticBackground : BackgroundHandle
		{
			/// <summary>The background texture</summary>
			internal Texture Texture;
			/// <summary>The number of times the texture is repeated around the viewing frustrum</summary>
			internal double Repetition;
			/// <summary>Whether the texture's aspect ratio should be maintained</summary>
			internal bool KeepAspectRatio;
			/// <summary>The time taken to transition to this background</summary>
			internal readonly double TransitionTime;
			/// <summary> The time at which this background should be displayed (Expressed as the number of seconds since midnight)</summary>
			internal readonly double Time;
			/// <summary>Creates a new static background, using the default 0.8s fade-in time</summary>
			/// <param name="Texture">The texture to apply</param>
			/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
			/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
			internal StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio)
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
			internal StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode)
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
			internal StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode, double Time)
			{
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
				this.TransitionTime = transitionTime;
				this.Mode = Mode;
				this.Time = Time;
			}

			internal override void UpdateBackground(double ElapsedTime, bool Target)
			{
				if (Target)
				{
					switch (Mode)
					{
						case BackgroundTransitionMode.None:
							Alpha = 1.0f;
							break;
						case BackgroundTransitionMode.FadeIn:
							Alpha = (float)(Countdown / TransitionTime);
							break;
						case BackgroundTransitionMode.FadeOut:
							Alpha = 1.0f - (float)(Countdown / TransitionTime);
							break;
					}
				}
				else
				{
					Alpha = 1.0f;
				}
			}

			internal override void RenderBackground(float Alpha, float Scale)
			{
				Renderer.RenderBackground(this, Alpha, Scale);
			}

			internal override void RenderBackground(float Scale)
			{
				Renderer.RenderBackground(this, 1.0f, Scale);
			}
		}

		/// <summary>Represents a dynamic background, made up of a series of static backgrounds</summary>
		internal class DynamicBackground : BackgroundHandle
		{
			/// <summary>The array of backgrounds for this dynamic background</summary>
			internal readonly StaticBackground[] Backgrounds;
			/// <summary>The current background in use</summary>
			internal int CurrentBackgroundIndex = 0;

			internal int PreviousBackgroundIndex = 0;

			/// <summary>Creates a new dynamic background</summary>
			/// <param name="Backgrounds">The list of static backgrounds which make up the dynamic background</param>
			internal DynamicBackground(StaticBackground[] Backgrounds)
			{
				this.Backgrounds = Backgrounds;
			}


			internal override void UpdateBackground(double ElapsedTime, bool Target)
			{
				if (Backgrounds.Length < 2)
				{
					CurrentBackgroundIndex = 0;
					//Renderer.RenderBackground(Backgrounds[CurrentBackgroundIndex], 1.0f, 0.5f);
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
								Alpha = 1.0f - (float)(Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
								break;
							case BackgroundTransitionMode.FadeOut:
								Alpha = (float)(Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
								break;
						}
					}
				}				
			}

			internal override void RenderBackground(float Alpha, float Scale)
			{
				Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], Alpha, Scale);
			}

			internal override void RenderBackground(float Scale)
			{
				if (this.CurrentBackgroundIndex != this.PreviousBackgroundIndex)
				{
					switch (this.Backgrounds[CurrentBackgroundIndex].Mode)
					{
						case BackgroundTransitionMode.FadeIn:
							Renderer.RenderBackground(this.Backgrounds[PreviousBackgroundIndex], 1.0f, Scale);
							Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
							Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], this.Alpha, Scale);
							break;
						case BackgroundTransitionMode.FadeOut:
							Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], 1.0f, Scale);
							Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
							Renderer.RenderBackground(this.Backgrounds[PreviousBackgroundIndex], this.Alpha, Scale);
							break;
					}
				}
				else
				{
					Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], 1.0f, Scale);
				}
			}
		}

		/// <summary>Represents a background object</summary>
		internal class BackgroundObject : BackgroundHandle
		{
			/// <summary>The object used for this background (NOTE: Static objects only)</summary>
			internal readonly ObjectManager.StaticObject ObjectBackground;

			internal double ClipDistance = 0;

			/// <summary>Creates a new background object</summary>
			/// <param name="Object">The object to use for the background</param>
			internal BackgroundObject(ObjectManager.StaticObject Object)
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

			internal override void UpdateBackground(double TimeElapsed, bool Target)
			{
				//No updates required
			}

			internal override void RenderBackground(float Alpha, float Scale)
			{
				Renderer.RenderBackground(this);
			}

			internal override void RenderBackground(float Scale)
			{
				Renderer.RenderBackground(this);
			}
		}

		internal enum BackgroundTransitionMode
		{
			/// <summary>No transition is performed</summary>
			None = 0,
			/// <summary>The new background fades in</summary>
			FadeIn = 1,
			/// <summary>The old background fades out</summary>
			FadeOut = 2
		}

		/// <summary>The currently displayed background texture</summary>
		internal static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);
		/// <summary>The new background texture (Currently fading in)</summary>
		internal static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);
	}
}
