using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace OpenBve
{
	/// <summary>Provides font support.</summary>
	internal static class Fonts
	{

		// --- structures ---

		/// <summary>Represents a single character.</summary>
		internal struct OpenGlFontChar
		{
			// --- members ---
			/// <summary>The texture coordinates that represent the character in the underlying texture.</summary>
			internal RectangleF TextureCoordinates;
			/// <summary>The physical size of the character.</summary>
			internal Size PhysicalSize;
			/// <summary>The typographic size of the character.</summary>
			internal Size TypographicSize;
			// --- constructors ---
			/// <summary>Creates a new character.</summary>
			/// <param name="textureCoordinates">The texture coordinates that represent the character in the underlying texture.</param>
			/// <param name="physicalSize">The physical size of the character.</param>
			/// <param name="typographicSize">The typographic size of the character.</param>
			internal OpenGlFontChar(RectangleF textureCoordinates, Size physicalSize, Size typographicSize)
			{
				this.TextureCoordinates = textureCoordinates;
				this.PhysicalSize = physicalSize;
				this.TypographicSize = typographicSize;
			}
		}

		/// <summary>Represents a table of 256 consecutive codepoints rendered into the same texture.</summary>
		internal class OpenGlFontTable
		{
			// --- members ---
			/// <summary>The characters stored in this table.</summary>
			internal OpenGlFontChar[] Characters;
			/// <summary>The texture that stores the characters.</summary>
			internal int Texture;
			// --- constructors ---
			/// <summary>Creates a new table of characters.</summary>
			/// <param name="font">The font.</param>
			/// <param name="offset">The offset from codepoint U+0000.</param>
			internal OpenGlFontTable(Font font, int offset)
			{
				/*
				 * Measure characters.
				 * */
				Size[] physicalSizes = new Size[256];
				Size[] typographicSizes = new Size[256];
				Bitmap bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
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
				this.Characters = new OpenGlFontChar[256];
				for (int i = 0; i < 256; i++)
				{
					if (x + physicalSizes[i].Width + border > width)
					{
						x = border;
						y += lineHeight;
						lineHeight = 0;
					}
					graphics.DrawString(char.ConvertFromUtf32(offset + i), font, Brushes.White, new PointF(x, y));
					float x0 = (float)(x - border) / (float)width;
					float x1 = (float)(x + physicalSizes[i].Width + border) / (float)width;
					float y0 = (float)(y - border) / (float)height;
					float y1 = (float)(y + physicalSizes[i].Height + border) / (float)height;
					this.Characters[i] = new OpenGlFontChar(new RectangleF(x0, y0, x1 - x0, y1 - y0), new Size(physicalSizes[i].Width + 2 * border, physicalSizes[i].Height + 2 * border), typographicSizes[i]);
					x += physicalSizes[i].Width + 2 * border;
					if (physicalSizes[i].Height + border > lineHeight)
					{
						lineHeight = physicalSizes[i].Height + 2 * border;
					}
				}
				graphics.Dispose();
				this.Texture = TextureManager.RegisterTexture(bitmap, false);
			}
		}

		/// <summary>Represents a font.</summary>
		internal class OpenGlFont : IDisposable
		{
			// --- members ---
			/// <summary>The underlying font.</summary>
			private readonly Font Font;
			/// <summary>The size of the underlying font in pixels.</summary>
			internal float FontSize;
			/// <summary>The 4352 tables containing 256 character each to make up 1114112 codepoints.</summary>
			private readonly OpenGlFontTable[] Tables;
			// --- constructors ---
			/// <summary>Creates a new font.</summary>
			/// <param name="family">The font family.</param>
			/// <param name="size">The size in pixels.</param>
			internal OpenGlFont(FontFamily family, float size)
			{
				this.Font = new Font(family, size, FontStyle.Regular, GraphicsUnit.Pixel);
				this.FontSize = size;
				this.Tables = new OpenGlFontTable[4352];
			}
			// --- functions ---
			/// <summary>Gets data associated with the specified codepoint.</summary>
			/// <param name="text">The string containing the codepoint.</param>
			/// <param name="offset">The offset at which to read the codepoint. For surrogate pairs, two characters are read, and one otherwise.</param>
			/// <param name="texture">Receives the texture that contains the codepoint.</param>
			/// <param name="data">Receives the data that describes the codepoint.</param>
			/// <returns>The number of characters read.</returns>
			internal int GetCharacterData(string text, int offset, out int texture, out OpenGlFontChar data)
			{
				int value = char.ConvertToUtf32(text, offset);
				int hi = value >> 8;
				int lo = value & 0xFF;
				if (this.Tables[hi] == null || this.Tables[hi].Texture <= 0)
				{
					this.Tables[hi] = new OpenGlFontTable(this.Font, hi << 8);
				}
				texture = this.Tables[hi].Texture;
				data = this.Tables[hi].Characters[lo];
				return value >= 0x10000 ? 2 : 1;
			}

			private bool disposed = false;
			protected virtual void Dispose(bool disposing)
			{
				if (!disposed)
				{
					if (disposing)
					{
						if (Font != null)
						{
							Font.Dispose();
						}
					}

					disposed = true;
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}


		// --- read-only fields ---
		/// <summary>Represents a small sans serif font.</summary>
		internal static OpenGlFont SmallFont = new OpenGlFont(FontFamily.GenericSansSerif, 12.0f);


		// --- functions ---

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

	}
}