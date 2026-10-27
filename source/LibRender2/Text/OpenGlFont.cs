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
		public readonly Font Font;

		/// <summary>The size of the underlying font in pixels.</summary>
		public readonly float FontSize;

		internal readonly DynamicSpriteFont SpriteFont;

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

		internal OpenGlFont(FontFamily family, float size, DynamicSpriteFont spriteFont)
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
				PremultiplyAlpha = false
			};

			FontSystem fontSystem = new FontSystem(settings);
			fontSystem.AddFont(fontData ?? FontFileResolver.BundledFontData);
			return fontSystem;
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
