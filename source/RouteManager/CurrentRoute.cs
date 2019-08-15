using LibRender;
using OpenBve.BackgroundManager;
using OpenBve.SignalManager;
using OpenBveApi.Colors;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using OpenTK.Graphics.OpenGL;

namespace OpenBve.RouteManager
{
	/// <summary>The current route</summary>
	public static class CurrentRoute
	{
		/// <summary>The list of tracks available in the simulation.</summary>
		public static Track[] Tracks = { new Track() };
		/// <summary>Holds a reference to the base TrainManager.Trains array</summary>
		public static AbstractTrain[] Trains;
		/// <summary>Holds all signal sections within the current route</summary>
		public static Section[] Sections = { };
		/// <summary>Holds all stations within the current route</summary>
		public static RouteStation[] Stations = { };
		/// <summary>Holds all .PreTrain instructions for the current route</summary>
		/// <remarks>Must be in distance and time ascending order</remarks>
		public static BogusPretrainInstruction[] BogusPretrainInstructions = { };
		/// <summary>Holds all points of interest within the game world</summary>
		public static PointOfInterest[] PointsOfInterest = { };
		/// <summary>The currently displayed background texture</summary>
		public static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);
		/// <summary>The new background texture (Currently fading in)</summary>
		public static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);
		/// <summary>The start of a region without fog</summary>
		/// <remarks>Must not be below the viewing distance (e.g. 600m)</remarks>
		public static float NoFogStart = 800.0f;
		/// <summary>The end of a reigon without fog</summary>
		public static float NoFogEnd = 1600.0f;
		/// <summary>Holds the previous fog</summary>
		public static Fog PreviousFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 0.0);
		/// <summary>Holds the current fog</summary>
		public static Fog CurrentFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 0.5);
		/// <summary>Holds the next fog</summary>
		public static Fog NextFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 1.0);
		/// <summary>The initial elevation in meters</summary>
		public static double InitialElevation = 0.0;
		/// <summary>The current in game time, expressed as the number of seconds since midnight on the first day</summary>
		public static double SecondsSinceMidnight = 0.0;

		/// <summary>Updates the currently displayed background</summary>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		/// <param name="gamePaused">Whether the game is currently paused</param>
		public static void UpdateBackground(double TimeElapsed, bool gamePaused)
		{
			if (gamePaused)
			{
				//Don't update the transition whilst paused
				TimeElapsed = 0.0;
			}
			const float scale = 0.5f, inv255 = 1.0f / 255.0f;
			// fog
			const float fogdistance = 600.0f;
			if (CurrentFog.Start < CurrentFog.End & CurrentFog.Start < fogdistance)
			{
				float cr = inv255 * (float)CurrentFog.Color.R;
				float cg = inv255 * (float)CurrentFog.Color.G;
				float cb = inv255 * (float)CurrentFog.Color.B;
				if (!Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				float ratio = (float)Backgrounds.BackgroundImageDistance / fogdistance;
				GL.Fog(FogParameter.FogStart, CurrentFog.Start * ratio * scale);
				GL.Fog(FogParameter.FogEnd, CurrentFog.End * ratio * scale);
				GL.Fog(FogParameter.FogColor, new float[] { cr, cg, cb, 1.0f });
				if (!Renderer.FogEnabled)
				{
					GL.Enable(EnableCap.Fog); Renderer.FogEnabled = true;
				}
			}
			else if (Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); Renderer.FogEnabled = false;
			}
			//Update the currently displayed background
			CurrentBackground.UpdateBackground(TimeElapsed, false);
			if (TargetBackground == null || TargetBackground == CurrentBackground)
			{
				//No target background, so call the render function
				CurrentBackground.RenderBackground(scale);
				return;
			}
			//Update the target background
			if (TargetBackground is StaticBackground)
			{
				TargetBackground.Countdown += TimeElapsed;
			}
			TargetBackground.UpdateBackground(TimeElapsed, true);
			switch (TargetBackground.Mode)
			{
				//Render, switching on the transition mode
				case BackgroundTransitionMode.FadeIn:
					CurrentBackground.RenderBackground(1.0f, scale);
					Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					TargetBackground.RenderBackground(TargetBackground.CurrentAlpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					TargetBackground.RenderBackground(1.0f, scale);
					Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					CurrentBackground.RenderBackground(TargetBackground.CurrentAlpha, scale);
					break;
			}
			//If our target alpha is greater than or equal to 1.0f, the background is fully displayed
			if (TargetBackground.CurrentAlpha >= 1.0f)
			{
				//Set the current background to the target & reset target to null
				CurrentBackground = TargetBackground;
				TargetBackground = null;
			}
			
		}
	}
}
