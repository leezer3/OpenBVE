using System;
using System.Drawing;
using FontStashSharp;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace LibRender2.Text
{
	/// <summary>Represents a font.</summary>
	public sealed class OpenGlFont : IDisposable
	{
		private static readonly Lazy<FontSystem> defaultFontSystem = new Lazy<FontSystem>(() => CreateFontSystem(null));

		/// <summary>The underlying font.</summary>
		public Font Font { get; private set; }

		/// <summary>The size of the underlying font in pixels.</summary>
		public float FontSize { get; private set; }

		/// <summary>The line height of the font in pixels, as measured by the rasterizer.</summary>
		public float LineHeight => SpriteFont.LineHeight;

		internal SpriteFontBase SpriteFont { get; private set; }

		private bool disposed;

		// --- constructors ---
		/// <summary>Creates a new font.</summary>
		/// <param name="family">The font family.</param>
		/// <param name="size">The size in pixels.</param>
		public OpenGlFont(FontFamily family, float size)
		{
			Font = new Font(family, size, FontStyle.Regular, GraphicsUnit.Pixel);
			FontSize = size;
			SpriteFont = defaultFontSystem.Value.GetFont(size);
		}

		internal OpenGlFont(FontFamily family, float size, SpriteFontBase spriteFont)
		{
			Font = new Font(family, size, FontStyle.Regular, GraphicsUnit.Pixel);
			FontSize = size;
			SpriteFont = spriteFont;
		}

		internal static FontSystem CreateFontSystem(byte[] fontData)
		{
			FontSystemSettings settings = new FontSystemSettings
			{
				FontResolutionFactor = 2,
				KernelWidth = 2,
				KernelHeight = 2,
				PremultiplyAlpha = false,
				// Size fonts by em square, not ascent→descent box, so all fonts render at the same visual size
				UseEmToPixelsScale = true
			};

			FontSystem fontSystem = new FontSystem(settings);
			// StbTrueTypeSharp only ever loads the font at offset 0, so TrueType collections (.ttc, e.g. Noto Sans CJK on Linux)
			// must be split into their individual faces first or they fail to load entirely
			byte[] data = fontData ?? FontFileResolver.BundledFontData;
			if (!AddFontFaces(fontSystem, data))
			{
				// Loading the selected font failed, so fall back to the bundled font
				fontSystem.AddFont(FontFileResolver.BundledFontData);
			}

			return fontSystem;
		}

		/// <summary>Adds every face of a font file to the font system, splitting TrueType collections first.</summary>
		/// <returns>True if at least one face was loaded successfully.</returns>
		private static bool AddFontFaces(FontSystem fontSystem, byte[] data)
		{
			bool addedFace = false;

			if (IsFontCollection(data))
			{
				int faces = (int)ReadUInt32BE(data, 8);
				for (int i = 0; i < faces; i++)
				{
					int faceOffset = (int)ReadUInt32BE(data, 12 + i * 4);
					if (faceOffset <= 0 || faceOffset >= data.Length)
					{
						continue;
					}

					int faceEnd = data.Length;
					if (i + 1 < faces)
					{
						int nextOffset = (int)ReadUInt32BE(data, 12 + (i + 1) * 4);
						if (nextOffset > faceOffset && nextOffset <= data.Length)
						{
							faceEnd = nextOffset;
						}
					}

					byte[] face = new byte[faceEnd - faceOffset];
					Buffer.BlockCopy(data, faceOffset, face, 0, face.Length);
					try
					{
						fontSystem.AddFont(face);
						addedFace = true;
					}
					catch
					{
						// Skip faces the rasterizer cannot parse
					}
				}
			}
			else
			{
				try
				{
					fontSystem.AddFont(data);
					addedFace = true;
				}
				catch
				{
					// Caller falls back to the bundled font
				}
			}

			return addedFace;
		}

		private static bool IsFontCollection(byte[] data)
		{
			return data.Length >= 12 && data[0] == 't' && data[1] == 't' && data[2] == 'c' && data[3] == 'f';
		}

		private static uint ReadUInt32BE(byte[] data, int offset)
		{
			return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
		}

		/// <summary>Measures the size of a string as it would be rendered using this font.</summary>
		/// <param name="text">The string to render.</param>
		/// <returns>The size of the string.</returns>
		public Vector2 MeasureString(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new Vector2();
			}

			System.Numerics.Vector2 size = SpriteFont.MeasureString(text);
			return new Vector2(size.X, size.Y);
		}

		internal void SetFont(float size, SpriteFontBase spriteFont)
		{
			FontSize = size;
			SpriteFont = spriteFont;
		}

		/// <summary>Replaces the GDI+ font and the sprite font in place, keeping the current size.</summary>
		/// <remarks>Everything that can throw happens before any state is changed.</remarks>
		internal void ReplaceFont(FontFamily family, FontSystem fontSystem)
		{
			Font newGdi = new Font(family, FontSize, FontStyle.Regular, GraphicsUnit.Pixel);
			SpriteFontBase newSprite = fontSystem.GetFont(FontSize);
			Font oldGdi = Font;
			Font = newGdi;
			oldGdi.Dispose();
			SpriteFont = newSprite;
		}

		internal void DrawString(BaseRenderer renderer, string text, double left, double top, Color128 color)
		{
			if (string.IsNullOrEmpty(text) || renderer.FontStashRenderer == null)
			{
				return;
			}

			FSColor fontColor = new FSColor(ToByte(color.R), ToByte(color.G), ToByte(color.B), ToByte(color.A));
			renderer.FontStashRenderer.Begin();
			SpriteFont.DrawText(renderer.FontStashRenderer, text, new System.Numerics.Vector2((float)left, (float)top), fontColor);
			renderer.FontStashRenderer.End();
			renderer.RestoreBlendFunc();
		}

		private static byte ToByte(float value)
		{
			int result = (int)(value * 255.0f + 0.5f);

			if (result < 0)
			{
				return 0;
			}

			return result > 255 ? (byte)255 : (byte)result;
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Font?.Dispose();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~OpenGlFont()
		{
			Dispose(true);
		}
	}
}
