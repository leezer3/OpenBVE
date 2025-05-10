using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace LibRender2.Text
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
		private readonly byte[] myBytes;
		/// <summary>The border around glpyhs on the font bitmap</summary>
		const int drawBorder = 20;
		/// <summary>The border used when calculating texture co-ordinates</summary>
		const int coordinateBorder = 1;

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
			Bitmap bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			double lineHeight = 0;
			for (int i = 0; i < 256; i++)
			{
				string character = char.ConvertFromUtf32(offset + i);
				SizeF physicalSize = graphics.MeasureString(character, font, int.MaxValue, StringFormat.GenericDefault);
				SizeF typographicSize = graphics.MeasureString(character, font, int.MaxValue, StringFormat.GenericTypographic);
				physicalSizes[i] = new Vector2((int)Math.Ceiling(physicalSize.Width), (int)Math.Ceiling(physicalSize.Height));
				typographicSizes[i] = new Vector2((int)Math.Ceiling(typographicSize.Width == 0.0f ? physicalSize.Width : typographicSize.Width), (int)Math.Ceiling(typographicSize.Height == 0.0f ? physicalSize.Height : typographicSize.Height));

				if (typographicSizes[i].Y + drawBorder > lineHeight)
				{
					// Find line height here in case of an oversize glyph in non-zero line
					lineHeight = typographicSizes[i].Y + drawBorder;
				}
			}

			graphics.Dispose();
			bitmap.Dispose();

			/*
			 * Find suitable bitmap dimensions.
			 *
			 * NOTE:
			 * Draw the characters with a 20px border around each character to prevent bleeding in some fonts with massive tails (e.g. Miama Nuera)
			 * however, use a 1px clamp on the texture border
			 *
			 * Massive tails may still overlap / glitch, but this gives us no bleed
			 * */
			
			double width = drawBorder;
			double height = drawBorder;
			double lineWidth = 0;
			
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
					lineWidth = drawBorder;

					height += lineHeight;
				}

				coordinates[i] = new Vector2(lineWidth, height);

				lineWidth += physicalSizes[i].X + drawBorder;
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
				double x0 = (coordinates[i].X - coordinateBorder) / width;
				double y0 = (coordinates[i].Y - coordinateBorder) / height;
				double x1 = (coordinates[i].X + physicalSizes[i].X + coordinateBorder) / width;
				double y1 = (coordinates[i].Y + physicalSizes[i].Y + coordinateBorder) / height;
				Characters[i] = new OpenGlFontChar(new Vector4(x0, y0, x1 - x0, y1 - y0), new Vector2(physicalSizes[i].X + 2.0 * coordinateBorder, physicalSizes[i].Y + 2.0 * coordinateBorder), typographicSizes[i]);
			}
			graphics.Dispose();
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
			if (data.Stride == 4 * data.Width) {
				/*
				 * Copy the data from the bitmap
				 * to the array in BGRA format.
				 */
				myBytes = new byte[data.Stride * data.Height];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, myBytes, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
			}
			Texture = new Texture(bitmap.Width, bitmap.Height, OpenBveApi.Textures.PixelFormat.RGBAlpha, myBytes, null);
			bitmap.Dispose();
		}

		/// <summary>Rounds the specified value to the next-highest power of two.</summary>
		/// <param name="value">The value.</param>
		/// <returns>The value rounded to the next-highest power of two.</returns>
		private static uint RoundToPowerOfTwo(uint value)
		{
			if (value == 0)
			{
				throw new ArgumentException("Unable to round a value of zero.", nameof(value));
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
			GC.SuppressFinalize(this);
		}
	}
}
