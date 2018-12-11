using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>Renders a key overlay, stacked horizontally & vertically</summary>
		/// <param name="Left">The left co-ordinate</param>
		/// <param name="Top">The top co-ordinate</param>
		/// <param name="Width">The width of each key overlay</param>
		/// <param name="Keys">The strings in a 3D array</param>
		public static void RenderKeys(int Left, int Top, int Width, string[][] Keys) {
			int py = Top;
			for (int y = 0; y < Keys.Length; y++) {
				int px = Left;
				for (int x = 0; x < Keys[y].Length; x++) {
					if (Keys[y][x] != null) {
						DrawRectangle(null, new Point(px -1, py -1), new Size(Width + 1, 17),  new Color128(0.25f,0.25f, 0.25f,0.5f));
						DrawRectangle(null, new Point(px - 1, py - 1), new Size(Width - 1, 15), new Color128(0.75f, 0.75f, 0.75f, 0.5f));
						DrawRectangle(null, new Point(px, py), new Size(Width, 16), new Color128(0.5f, 0.5f, 0.5f, 0.5f));
						DrawString(Fonts.SmallFont, Keys[y][x], new Point(px -1 + Width /2, py + 7), TextAlignment.CenterMiddle, Color128.White);
					}
					px += Width + 4;
				}
				py += 20;
			}
		}
	}
}
