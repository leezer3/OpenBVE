using System.Drawing;
using LibRender2.Texts;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;

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

			foreach (string[] key in Keys)
			{
				int px = Left;

				foreach (string text in key)
				{
					if (text != null)
					{
						renderer.Rectangle.Draw(null, new Point(px - 1, py - 1), new Size(Width + 1, 17), new Color128(0.25f, 0.25f, 0.25f, 0.5f));
						renderer.Rectangle.Draw(null, new Point(px - 1, py - 1), new Size(Width - 1, 15), new Color128(0.75f, 0.75f, 0.75f, 0.5f));
						renderer.Rectangle.Draw(null, new Point(px, py), new Size(Width, 16), new Color128(0.5f, 0.5f, 0.5f, 0.5f));
						renderer.OpenGlString.Draw(Font, text, new Point(px - 1 + Width / 2, py + 7), TextAlignment.CenterMiddle, Color128.White);
					}

					px += Width + 4;
				}

				py += 20;
			}
		}
	}
}
