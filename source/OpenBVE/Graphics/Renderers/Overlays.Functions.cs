using OpenBveApi.Textures;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/* --------------------------------------------------------------
		 * This file contains functions used when rendering screen overlays
		 * -------------------------------------------------------------- */
		
		/// <summary>Calculates the viewing plane size for the given HUD element</summary>
		/// <param name="Element">The element</param>
		/// <param name="LeftWidth">The left width of the viewing plane</param>
		/// <param name="RightWidth">The right width of the viewing plane</param>
		/// <param name="LCrH">The center point of the viewing plane</param>
		private static void CalculateViewingPlaneSize(HUD.Element Element, out double LeftWidth, out double RightWidth, out double LCrH)
		{
			LCrH = 0.0;
			// left width/height
			LeftWidth = 0.0;
			if (Program.CurrentHost.LoadTexture(ref Element.TopLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double u = Element.TopLeft.BackgroundTexture.Width;
				double v = Element.TopLeft.BackgroundTexture.Height;
				if (u > LeftWidth) LeftWidth = u;
				if (v > LCrH) LCrH = v;
			}
			if (Program.CurrentHost.LoadTexture(ref Element.CenterLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double u = Element.CenterLeft.BackgroundTexture.Width;
				double v = Element.CenterLeft.BackgroundTexture.Height;
				if (u > LeftWidth) LeftWidth = u;
				if (v > LCrH) LCrH = v;
			}
			if (Program.CurrentHost.LoadTexture(ref Element.BottomLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double u = Element.BottomLeft.BackgroundTexture.Width;
				double v = Element.BottomLeft.BackgroundTexture.Height;
				if (u > LeftWidth) LeftWidth = u;
				if (v > LCrH) LCrH = v;
			}
			// center height
			if (Program.CurrentHost.LoadTexture(ref Element.TopMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double v = Element.TopMiddle.BackgroundTexture.Height;
				if (v > LCrH) LCrH = v;
			}
			if (Program.CurrentHost.LoadTexture(ref Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double v = Element.CenterMiddle.BackgroundTexture.Height;
				if (v > LCrH) LCrH = v;
			}
			if (Program.CurrentHost.LoadTexture(ref Element.BottomMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double v = Element.BottomMiddle.BackgroundTexture.Height;
				if (v > LCrH) LCrH = v;
			}
			// right width/height
			RightWidth = 0.0;
			if (Program.CurrentHost.LoadTexture(ref Element.TopRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double u = Element.TopRight.BackgroundTexture.Width;
				double v = Element.TopRight.BackgroundTexture.Height;
				if (u > RightWidth) RightWidth = u;
				if (v > LCrH) LCrH = v;
			}
			if (Program.CurrentHost.LoadTexture(ref Element.CenterRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double u = Element.CenterRight.BackgroundTexture.Width;
				double v = Element.CenterRight.BackgroundTexture.Height;
				if (u > RightWidth) RightWidth = u;
				if (v > LCrH) LCrH = v;
			}
			if (Program.CurrentHost.LoadTexture(ref Element.BottomRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
			{
				double u = Element.BottomRight.BackgroundTexture.Width;
				double v = Element.BottomRight.BackgroundTexture.Height;
				if (u > RightWidth) RightWidth = u;
				if (v > LCrH) LCrH = v;
			}
		}
	}
}
