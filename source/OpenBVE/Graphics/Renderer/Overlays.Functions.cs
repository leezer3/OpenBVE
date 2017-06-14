namespace OpenBve
{
	internal static partial class Renderer
	{
		/* --------------------------------------------------------------
		 * This file contains functions used when rendering screen overlays
		 * -------------------------------------------------------------- */

		/// <summary>Renders an overlay texture</summary>
		/// <param name="texture">The texture</param>
		/// <param name="left">The left co-ordinate</param>
		/// <param name="top">The top co-ordinate</param>
		/// <param name="right">The right co-ordinate</param>
		/// <param name="bottom">The bottom co-ordinate</param>
		internal static void RenderOverlayTexture(Textures.Texture texture, double left, double top, double right, double bottom)
		{
			DrawRectangle(texture, new System.Drawing.Point((int)left, (int)top), new System.Drawing.Size((int)(right - left), (int)(bottom - top)), null);
		}

		/// <summary>Renders a solid color rectangular overlay</summary>
		/// <param name="left">The left co-ordinate</param>
		/// <param name="top">The top co-ordinate</param>
		/// <param name="right">The right co-ordinate</param>
		/// <param name="bottom">The bottom co-ordinate</param>
		internal static void RenderOverlaySolid(double left, double top, double right, double bottom)
		{
			DrawRectangle(null, new System.Drawing.Point((int)left, (int)top), new System.Drawing.Size((int)(right - left), (int)(bottom - top)), null);
		}

		/// <summary>Calculates the viewing plane size for the given HUD element</summary>
		/// <param name="Element">The element</param>
		/// <param name="LeftWidth">The left width of the viewing plane</param>
		/// <param name="RightWidth">The right width of the viewing plane</param>
		/// <param name="LCrH">The center point of the viewing plane</param>
		private static void CalculateViewingPlaneSize(Interface.HudElement Element, out double LeftWidth, out double RightWidth, out double LCrH)
		{
			LCrH = 0.0;
			// left width/height
			LeftWidth = 0.0;
			if (Element.TopLeft.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.TopLeft.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.TopLeft.BackgroundTexture.Width;
					double v = (double)Element.TopLeft.BackgroundTexture.Height;
					if (u > LeftWidth) LeftWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.CenterLeft.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.CenterLeft.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.CenterLeft.BackgroundTexture.Width;
					double v = (double)Element.CenterLeft.BackgroundTexture.Height;
					if (u > LeftWidth) LeftWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.BottomLeft.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.BottomLeft.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
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
				if (Textures.LoadTexture(Element.TopMiddle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double v = (double)Element.TopMiddle.BackgroundTexture.Height;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.CenterMiddle.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.CenterMiddle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double v = (double)Element.CenterMiddle.BackgroundTexture.Height;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.BottomMiddle.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.BottomMiddle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double v = (double)Element.BottomMiddle.BackgroundTexture.Height;
					if (v > LCrH) LCrH = v;
				}
			}
			// right width/height
			RightWidth = 0.0;
			if (Element.TopRight.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.TopRight.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.TopRight.BackgroundTexture.Width;
					double v = (double)Element.TopRight.BackgroundTexture.Height;
					if (u > RightWidth) RightWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.CenterRight.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.CenterRight.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					double u = (double)Element.CenterRight.BackgroundTexture.Width;
					double v = (double)Element.CenterRight.BackgroundTexture.Height;
					if (u > RightWidth) RightWidth = u;
					if (v > LCrH) LCrH = v;
				}
			}
			if (Element.BottomRight.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.BottomRight.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
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
