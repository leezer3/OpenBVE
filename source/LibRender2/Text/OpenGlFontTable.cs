using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace LibRender2.Texts
{
	/// <summary>Represents a table of 256 consecutive code points rendered into the same texture.</summary>
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
			Vector2[] physicalSizes = new Vector2[256];
			Vector2[] typographicSizes = new Vector2[256];
			bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			for (int i = 0; i < 256; i++)
			{
				string character = char.ConvertFromUtf32(offset + i);
				SizeF physicalSize = graphics.MeasureString(character, font, int.MaxValue, StringFormat.GenericDefault);
				SizeF typographicSize = graphics.MeasureString(character, font, int.MaxValue, StringFormat.GenericTypographic);
				physicalSizes[i] = new Vector2((int)Math.Ceiling(physicalSize.Width), (int)Math.Ceiling(physicalSize.Height));
				typographicSizes[i] = new Vector2((int)Math.Ceiling(typographicSize.Width == 0.0f ? physicalSize.Width : typographicSize.Width), (int)Math.Ceiling(typographicSize.Height == 0.0f ? physicalSize.Height : typographicSize.Height));
			}

			graphics.Dispose();
			bitmap.Dispose();

			/*
			 * Find suitable bitmap dimensions.
			 * */
			const int border = 1;
			double width = border;
			double height = border;
			double lineWidth = 0;
			double lineHeight = 0;
			Vector2[] coordinates = new Vector2[256];

			for (int i = 0; i < 256; i++)
			{
				if (i % 16 == 0)
				{
					// new line
					if (lineWidth > width)
					{
						width = lineWidth;
					}
					lineWidth = border;

					height += lineHeight;
					lineHeight = 0;
				}

				coordinates[i] = new Vector2(lineWidth, height);

				lineWidth += physicalSizes[i].X + border;

				if (physicalSizes[i].Y + border > lineHeight)
				{
					lineHeight = physicalSizes[i].Y + border;
				}
			}

			if (lineWidth > width)
			{
				width = lineWidth;
			}
			height += lineHeight;
			width = (int)RoundToPowerOfTwo((uint)width);
			height = (int)RoundToPowerOfTwo((uint)height);

			/*
			 * Draw character to bitmap.
			 * */
			bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
			graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			graphics.Clear(Color.Black);
			Characters = new OpenGlFontChar[256];

			for (int i = 0; i < 256; i++)
			{
				graphics.DrawString(char.ConvertFromUtf32(offset + i), font, Brushes.White, new PointF((float)coordinates[i].X, (float)coordinates[i].Y));
				double x0 = (coordinates[i].X - border) / width;
				double x1 = (coordinates[i].X + physicalSizes[i].X + border) / width;
				double y0 = (coordinates[i].Y - border) / height;
				double y1 = (coordinates[i].Y + physicalSizes[i].Y + border) / height;
				Characters[i] = new OpenGlFontChar(new Vector4(x0, y0, x1 - x0, y1 - y0), new Vector2(physicalSizes[i].X + 2 * border, physicalSizes[i].Y + 2 * border), typographicSizes[i]);
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
