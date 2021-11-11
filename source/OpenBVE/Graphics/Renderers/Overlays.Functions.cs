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
			if (Element.TopLeft.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.TopLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.TopLeft.BackgroundTexture.Width;
					double v = (double)Element.TopLeft.BackgroundTexture.Height;
					if (u > LeftWidth) LeftWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.CenterLeft.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.CenterLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.CenterLeft.BackgroundTexture.Width;
					double v = (double)Element.CenterLeft.BackgroundTexture.Height;
					if (u > LeftWidth) LeftWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.BottomLeft.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.BottomLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.BottomLeft.BackgroundTexture.Width;
					double v = (double)Element.BottomLeft.BackgroundTexture.Height;
					if (u > LeftWidth) LeftWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			// center height
			if (Element.TopMiddle.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.TopMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = (double)Element.TopMiddle.BackgroundTexture.Height;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.CenterMiddle.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = (double)Element.CenterMiddle.BackgroundTexture.Height;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.BottomMiddle.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.BottomMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = (double)Element.BottomMiddle.BackgroundTexture.Height;
					if (v > LCrH) LCrH = v;
				}
			}
			// right width/height
			RightWidth = 0.0;
			if (Element.TopRight.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.TopRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.TopRight.BackgroundTexture.Width;
					double v = (double)Element.TopRight.BackgroundTexture.Height;
					if (u > RightWidth) RightWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.CenterRight.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.CenterRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.CenterRight.BackgroundTexture.Width;
					double v = (double)Element.CenterRight.BackgroundTexture.Height;
					if (u > RightWidth) RightWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.BottomRight.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.BottomRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.BottomRight.BackgroundTexture.Width;
					double v = (double)Element.BottomRight.BackgroundTexture.Height;
					if (u > RightWidth) RightWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
		}
	}
}
