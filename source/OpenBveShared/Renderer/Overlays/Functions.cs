using System.Drawing;
using OpenBveApi.Textures;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>Renders an overlay texture</summary>
		/// <param name="texture">The texture</param>
		/// <param name="left">The left co-ordinate</param>
		/// <param name="top">The top co-ordinate</param>
		/// <param name="right">The right co-ordinate</param>
		/// <param name="bottom">The bottom co-ordinate</param>
		public static void RenderOverlayTexture(Texture texture, double left, double top, double right, double bottom)
		{
			DrawRectangle(texture, new Point((int)left, (int)top), new Size((int)(right - left), (int)(bottom - top)), null);
		}

		/// <summary>Renders a solid color rectangular overlay</summary>
		/// <param name="left">The left co-ordinate</param>
		/// <param name="top">The top co-ordinate</param>
		/// <param name="right">The right co-ordinate</param>
		/// <param name="bottom">The bottom co-ordinate</param>
		public static void RenderOverlaySolid(double left, double top, double right, double bottom)
		{
			DrawRectangle(null, new Point((int)left, (int)top), new Size((int)(right - left), (int)(bottom - top)), null);
		}
	}
}
