using System;
using System.Drawing;
using OpenBveApi.Math;

namespace OpenBveApi.Textures
{
	/// <summary>Provides functions for dyamically overlaying text onto a texture</summary>
	public class TextOverlay
	{
		/// <summary>Adds a textual string to a bitmap image</summary>
		/// <param name="bitmap">The bitmap image to add the text to, or a null reference if a new image is to be created</param>
		/// <param name="txt">The text to overlay</param>
		/// <param name="fontname">The name of the font to use</param>
		/// <param name="fontsize">The size in points of the font</param>
		/// <param name="bgcolor">The background color to use (Only relevant if creating a new image)</param>
		/// <param name="fcolor">The font color to use</param>
		/// <param name="Padding">The padding to use, or alternatively the X,Y inset if overlaying text</param>
		public static Bitmap AddTextToBitmap(Bitmap bitmap, string txt, string fontname, int fontsize, Color bgcolor,Color fcolor, Vector2 Padding)
		{
			bool overlay = true;
			SizeF size;
			if (bitmap == null)
			{
				bitmap = new Bitmap(1024, 1024);
				overlay = false;
			}
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				Font font = new Font(fontname, fontsize);
				size = graphics.MeasureString(txt, font);
				if (!overlay)
				{
					graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, size.Width + (int) Padding.X*2,size.Height + (int) Padding.Y*2);
				}
				graphics.DrawString(txt, font, new SolidBrush(fcolor), (int) Padding.X, (int) Padding.Y);
				graphics.Flush();
				font.Dispose();
				graphics.Dispose();
			}
			if (!overlay)
			{
				Rectangle cropArea = new Rectangle(0, 0, (int) size.Width + (int) Padding.X*2, (int) size.Height + (int) Padding.Y*2);
				return bitmap.Clone(cropArea, bitmap.PixelFormat);
			}
			return bitmap;
		}

		/// <summary>Checks whether the specified font name is present on the system</summary>
		/// <param name="fontName">The font name to check</param>
		public static bool FontAvailable(string fontName)
		{
			using (var testFont = new Font(fontName, 8))
			{
				return 0 == string.Compare(
					fontName,
					testFont.Name,
					StringComparison.InvariantCultureIgnoreCase);
			}
		}
	}
}
