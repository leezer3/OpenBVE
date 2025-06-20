using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;

namespace LibRender2.Overlays
{
	public class Keys
	{
		private readonly BaseRenderer renderer;

		internal Keys(BaseRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>Draws a key overlay to the screen</summary>
		/// <param name="Left">The left co-ordinate of the top key</param>
		/// <param name="Top">The top co-ordinate of the top key</param>
		/// <param name="Width">The width of the key overlay</param>
		/// <param name="Font">The font to draw</param>
		/// <param name="Keys">The key names</param>
		public void Render(int Left, int Top, int Width, OpenGlFont Font, string[][] Keys)
		{
			int py = Top;

			Width = (int)System.Math.Ceiling(renderer.ScaleFactor.X * Width);

			foreach (string[] key in Keys)
			{
				int px = Left;

				foreach (string text in key)
				{
					if (text != null)
					{
						renderer.Rectangle.Draw(null, new Vector2(px - 1, py - 1), new Vector2(Width + 1, 17), new Color128(0.25f, 0.25f, 0.25f, 0.5f));
						renderer.Rectangle.Draw(null, new Vector2(px - 1, py - 1), new Vector2(Width - 1, 15), new Color128(0.75f, 0.75f, 0.75f, 0.5f));
						renderer.Rectangle.Draw(null, new Vector2(px, py), new Vector2(Width, 16), Color128.SemiTransparentGrey);
						renderer.OpenGlString.Draw(Font, text, new Vector2(px - 1 + Width / 2.0, py + 7), TextAlignment.CenterMiddle, Color128.White);
					}

					px += Width + 4;
				}

				py += 20;
			}
		}
	}
}
