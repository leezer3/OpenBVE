using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenBveApi.Textures;

namespace LibRender2.Texts
{
	/// <summary>Represents a table of 256 consecutive codepoints rendered into the same texture.</summary>
	public class OpenGlFontTable : IDisposable
	{
		// --- members ---
		/// <summary>The characters stored in this table.</summary>
		public readonly OpenGlFontChar[] Characters;

		/// <summary>The texture that stores the characters.</summary>
		public readonly Texture Texture;

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		// Must remain, else will be disposed of by the GC as this is a separate assembly
		private readonly Bitmap bitmap;

		// --- constructors ---
		/// <summary>Creates a new table of characters.</summary>
		/// <param name="font">The font.</param>
		/// <param name="offset">The offset from codepoint U+0000.</param>
		public OpenGlFontTable(Font font, int offset)
		{
			/*
			 * Measure characters.
			 * */
			Size[] physicalSizes = new Size[256];
			Size[] typographicSizes = new Size[256];
			bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			for (int i = 0; i < 256; i++)
			{
				SizeF physicalSize = graphics.MeasureString(char.ConvertFromUtf32(offset + i), font, int.MaxValue, StringFormat.GenericDefault);
				SizeF typographicSize = graphics.MeasureString(char.ConvertFromUtf32(offset + i), font, int.MaxValue, StringFormat.GenericTypographic);
				physicalSizes[i] = new Size((int)Math.Ceiling(physicalSize.Width), (int)Math.Ceiling(physicalSize.Height));
				typographicSizes[i] = new Size((int)Math.Ceiling(typographicSize.Width == 0.0f ? physicalSize.Width : typographicSize.Width), (int)Math.Ceiling(typographicSize.Height == 0.0f ? physicalSize.Height : typographicSize.Height));
			}

			/*
			 * Find suitable bitmap dimensions.
			 * */
			const int width = 256;
			const int border = 1;
			int x = border;
			int y = border;
			int lineHeight = 0;

			for (int i = 0; i < 256; i++)
			{
				if (x + physicalSizes[i].Width + border > width)
				{
					x = border;
					y += lineHeight;
					lineHeight = 0;
				}
				else
				{
					x += physicalSizes[i].Width + 2 * border;
				}

				if (physicalSizes[i].Height + border > lineHeight)
				{
					lineHeight = physicalSizes[i].Height + 2 * border;
				}
			}

			y += lineHeight;
			int height = (int)RoundToPowerOfTwo((uint)y);
			graphics.Dispose();
			bitmap.Dispose();
			
			/*
			 * Draw character to bitmap.
			 * */
			bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			graphics.Clear(Color.Black);
			x = border;
			y = border;
			lineHeight = 0;
			Characters = new OpenGlFontChar[256];

			for (int i = 0; i < 256; i++)
			{
				if (x + physicalSizes[i].Width + border > width)
				{
					x = border;
					y += lineHeight;
					lineHeight = 0;
				}

				graphics.DrawString(char.ConvertFromUtf32(offset + i), font, Brushes.White, new PointF(x, y));
				float x0 = (x - border) / (float)width;
				float x1 = (x + physicalSizes[i].Width + border) / (float)width;
				float y0 = (y - border) / (float)height;
				float y1 = (y + physicalSizes[i].Height + border) / (float)height;
				Characters[i] = new OpenGlFontChar(new RectangleF(x0, y0, x1 - x0, y1 - y0), new Size(physicalSizes[i].Width + 2 * border, physicalSizes[i].Height + 2 * border), typographicSizes[i]);
				x += physicalSizes[i].Width + 2 * border;

				if (physicalSizes[i].Height + border > lineHeight)
				{
					lineHeight = physicalSizes[i].Height + 2 * border;
				}
			}

			graphics.Dispose();
			Texture = new Texture(bitmap);
		}

		/// <summary>Rounds the specified value to the next-highest power of two.</summary>
		/// <param name="value">The value.</param>
		/// <returns>The value rounded to the next-highest power of two.</returns>
		private static uint RoundToPowerOfTwo(uint value)
		{
			if (value == 0)
			{
				throw new ArgumentException();
			}

			value -= 1;

			for (int i = 1; i < sizeof(int) << 3; i <<= 1)
			{
				value |= value >> i;
			}

			return value + 1;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool currentlyDisposing)
		{
			if (currentlyDisposing)
			{
				bitmap.Dispose();
			}
		}
	}
}
