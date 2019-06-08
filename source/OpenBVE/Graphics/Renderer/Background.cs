using LibRender;
using OpenBve.BackgroundManager;
using OpenBve.RouteManager;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{

		/* --------------------------------------------------------------
		 * This file contains the drawing routines for the background texture
		 * -------------------------------------------------------------- */

		/// <summary>Updates the currently displayed background</summary>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		private static void UpdateBackground(double TimeElapsed)
		{
			if (Game.CurrentInterface != Game.InterfaceType.Normal)
			{
				//Don't update the transition whilst paused
				TimeElapsed = 0.0;
			}
			const float scale = 0.5f;
			// fog
			const float fogdistance = 600.0f;
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < fogdistance)
			{
				float cr = inv255 * (float)Game.CurrentFog.Color.R;
				float cg = inv255 * (float)Game.CurrentFog.Color.G;
				float cb = inv255 * (float)Game.CurrentFog.Color.B;
				if (!LibRender.Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				float ratio = (float)Backgrounds.BackgroundImageDistance / fogdistance;
				GL.Fog(FogParameter.FogStart, Game.CurrentFog.Start * ratio * scale);
				GL.Fog(FogParameter.FogEnd, Game.CurrentFog.End * ratio * scale);
				GL.Fog(FogParameter.FogColor, new float[] { cr, cg, cb, 1.0f });
				if (!LibRender.Renderer.FogEnabled)
				{
					GL.Enable(EnableCap.Fog); LibRender.Renderer.FogEnabled = true;
				}
			}
			else if (LibRender.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			//Update the currently displayed background
			CurrentRoute.CurrentBackground.UpdateBackground(TimeElapsed, false);
			if (CurrentRoute.TargetBackground == null || CurrentRoute.TargetBackground == CurrentRoute.CurrentBackground)
			{
				//No target background, so call the render function
				CurrentRoute.CurrentBackground.RenderBackground(scale);
				return;
			}
			//Update the target background
			if (CurrentRoute.TargetBackground is StaticBackground)
			{
				CurrentRoute.TargetBackground.Countdown += TimeElapsed;
			}
			CurrentRoute.TargetBackground.UpdateBackground(TimeElapsed, true);
			switch (CurrentRoute.TargetBackground.Mode)
			{
				//Render, switching on the transition mode
				case BackgroundTransitionMode.FadeIn:
					CurrentRoute.CurrentBackground.RenderBackground(1.0f, scale);
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					CurrentRoute.TargetBackground.RenderBackground(CurrentRoute.TargetBackground.CurrentAlpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					CurrentRoute.TargetBackground.RenderBackground(1.0f, scale);
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					CurrentRoute.CurrentBackground.RenderBackground(CurrentRoute.TargetBackground.CurrentAlpha, scale);
					break;
			}
			//If our target alpha is greater than or equal to 1.0f, the background is fully displayed
			if (CurrentRoute.TargetBackground.CurrentAlpha >= 1.0f)
			{
				//Set the current background to the target & reset target to null
				CurrentRoute.CurrentBackground = CurrentRoute.TargetBackground;
				CurrentRoute.TargetBackground = null;
			}
			
		}
	}
}
