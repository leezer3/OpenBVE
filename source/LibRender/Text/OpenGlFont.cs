using System;
using System.Drawing;
using OpenBveApi.Textures;

namespace LibRender
{
	/// <summary>Represents a font.</summary>
	public sealed class OpenGlFont : IDisposable
	{
		// --- members ---
		/// <summary>The underlying font.</summary>
		private readonly Font Font;
		/// <summary>The size of the underlying font in pixels.</summary>
		public readonly float FontSize;
		/// <summary>The 4352 tables containing 256 character each to make up 1114112 codepoints.</summary>
		private readonly OpenGlFontTable[] Tables;

		// --- constructors ---
		/// <summary>Creates a new font.</summary>
		/// <param name="family">The font family.</param>
		/// <param name="size">The size in pixels.</param>
		public OpenGlFont(FontFamily family, float size)
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
		public int GetCharacterData(string text, int offset, out Texture texture, out OpenGlFontChar data)
		{
			int value = char.ConvertToUtf32(text, offset);
			int hi = value >> 8;
			int lo = value & 0xFF;
			if (this.Tables[hi] == null || this.Tables[hi].Texture == null)
			{
				lock (Renderer.gdiPlusLock)
				{
					this.Tables[hi] = new OpenGlFontTable(this.Font, hi << 8);
				}
			}

			texture = this.Tables[hi].Texture;
			data = this.Tables[hi].Characters[lo];
			return value >= 0x10000 ? 2 : 1;
		}

		/// <summary>Measures the size of a string as it would be rendered using this font.</summary>
		/// <param name="text">The string to render.</param>
		/// <returns>The size of the string.</returns>
		public Size MeasureString(string text) {
			int width = 0;
			int height = 0;
			if (text != null) {
				for (int i = 0; i < text.Length; i++) {
					// ReSharper disable once NotAccessedVariable
					Texture texture;
					OpenGlFontChar data;
					i += GetCharacterData(text, i, out texture, out data) - 1;
					width += data.TypographicSize.Width;
					if (data.TypographicSize.Height > height) {
						height = data.TypographicSize.Height;
					}
				}
			}
			return new Size(width, height);
		}

		private bool disposed = false;

		private void Dispose(bool disposing)
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
}
