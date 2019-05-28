using System;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.Routes;

namespace OpenBve
{
	class BackgroundManager
	{
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

			public override void RenderBackground(float Alpha, float Scale)
			{
				Renderer.RenderBackground(this, Alpha, Scale);
			}

			public override void RenderBackground(float Scale)
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


			public override void UpdateBackground(double ElapsedTime, bool Target)
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
								CurrentAlpha = 1.0f - (float)(Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
								break;
							case BackgroundTransitionMode.FadeOut:
								CurrentAlpha = (float)(Countdown / Backgrounds[CurrentBackgroundIndex].TransitionTime);
								break;
						}
					}
				}				
			}

			public override void RenderBackground(float Alpha, float Scale)
			{
				Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], Alpha, Scale);
			}

			public override void RenderBackground(float Scale)
			{
				if (this.CurrentBackgroundIndex != this.PreviousBackgroundIndex)
				{
					switch (this.Backgrounds[CurrentBackgroundIndex].Mode)
					{
						case BackgroundTransitionMode.FadeIn:
							Renderer.RenderBackground(this.Backgrounds[PreviousBackgroundIndex], 1.0f, Scale);
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
							Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], this.CurrentAlpha, Scale);
							break;
						case BackgroundTransitionMode.FadeOut:
							Renderer.RenderBackground(this.Backgrounds[CurrentBackgroundIndex], 1.0f, Scale);
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
							Renderer.RenderBackground(this.Backgrounds[PreviousBackgroundIndex], this.CurrentAlpha, Scale);
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
			internal readonly StaticObject ObjectBackground;
			/// <summary>The clipping distance required to fully render the object</summary>
			internal readonly double ClipDistance = 0;

			/// <summary>Creates a new background object</summary>
			/// <param name="Object">The object to use for the background</param>
			internal BackgroundObject(StaticObject Object)
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

			public override void UpdateBackground(double TimeElapsed, bool Target)
			{
				//No updates required
			}

			public override void RenderBackground(float Alpha, float Scale)
			{
				Renderer.RenderBackground(this);
			}

			public override void RenderBackground(float Scale)
			{
				Renderer.RenderBackground(this);
			}
		}

		

		/// <summary>The currently displayed background texture</summary>
		internal static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);
		/// <summary>The new background texture (Currently fading in)</summary>
		internal static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);
	}
}
