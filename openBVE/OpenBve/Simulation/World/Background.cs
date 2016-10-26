using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBve
{
	class BackgroundManager
	{
		/// <summary>Represents a handle to an abstract background.</summary>
		internal abstract class BackgroundHandle
		{
			internal abstract void SetBackground(float Alpha, float Scale);
		}

		/// <summary>Represents a static background, using the default viewing frustrum</summary>
		internal class StaticBackground : BackgroundHandle
		{
			internal Textures.Texture Texture;
			internal int Repetition;
			internal bool KeepAspectRatio;
			internal double FadeInTime;
			internal int DisplayTime;
			/// <summary>Creates a new static background, using the default 0.8s fade-in time</summary>
			/// <param name="Texture">The texture to apply</param>
			/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
			/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
			internal StaticBackground(Textures.Texture Texture, int Repetition, bool KeepAspectRatio)
			{
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
				this.FadeInTime = 0.8;
				this.DisplayTime = -1;
			}

			/// <summary>Creates a new static background</summary>
			/// <param name="Texture">The texture to apply</param>
			/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
			/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
			/// <param name="FadeInTime">The time taken in seconds for the fade-in transition to occur</param>
			internal StaticBackground(Textures.Texture Texture, int Repetition, bool KeepAspectRatio, double FadeInTime)
			{
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
				this.FadeInTime = FadeInTime;
				this.DisplayTime = -1;
			}

			/// <summary>Renders a background</summary>
			/// <param name="Alpha">The alpha</param>
			/// <param name="scale">The scale of the background (NOTE: Constant 0.5f at present)</param>
			internal override void SetBackground(float Alpha, float scale)
			{
				Renderer.RenderBackground(this, Alpha, scale);
			}
		}

		/// <summary>Represents a dynamic background, made up of a series of static backgrounds</summary>
		internal class DynamicBackground : BackgroundHandle
		{
			/// <summary>The array of backgrounds for this dynamic background</summary>
			internal StaticBackground[] Backgrounds;
			/// <summary>The current background in use</summary>
			internal int CurrentBackgroundIndex = 0;
			/// <summary>Creates a new dynamic background</summary>
			/// <param name="Backgrounds">The list of static backgrounds which make up the dynamic background</param>
			internal DynamicBackground(StaticBackground[] Backgrounds)
			{
				this.Backgrounds = Backgrounds;
			}

			/// <summary>Renders a background</summary>
			/// <param name="Alpha">The alpha</param>
			/// <param name="scale">The scale of the background (NOTE: Constant 0.5f at present)</param>
			internal override void SetBackground(float Alpha, float scale)
			{
				//Extract background to variable for quick access
				var Data = Backgrounds[CurrentBackgroundIndex];
				Renderer.RenderBackground(Data, Alpha, scale);
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

			internal override void SetBackground(float Alpha, float Scale)
			{
				//Not yet implemented!
				throw new NotImplementedException();
			}
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
