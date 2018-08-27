using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{

		/// <summary>Measures the size of a string as it would be rendered using the specified font.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <returns>The size of the string.</returns>
		internal static Size MeasureString(Fonts.OpenGlFont font, string text)
		{
			int width = 0;
			int height = 0;
			if (text != null)
			{
				for (int i = 0; i < text.Length; i++)
				{
					int texture;
					Fonts.OpenGlFontChar data;
					i += font.GetCharacterData(text, i, out texture, out data) - 1;
					width += data.TypographicSize.Width;
					if (data.TypographicSize.Height > height)
					{
						height = data.TypographicSize.Height;
					}
				}
			}
			return new Size(width, height);
		}

		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		private static void DrawString(Fonts.OpenGlFont font, string text, Point location, TextAlignment alignment, Color128 color)
		{
			GL.Enable(EnableCap.Blend);
			if (text == null)
			{
				return;
			}
			/*
			 * Prepare the top-left coordinates for rendering, incorporating the
			 * orientation of the string in relation to the specified location.
			 * */
			int left;
			if ((alignment & TextAlignment.Left) == 0)
			{
				int width = 0;
				for (int i = 0; i < text.Length; i++)
				{
					int texture;
					Fonts.OpenGlFontChar data;
					i += font.GetCharacterData(text, i, out texture, out data) - 1;
					width += data.TypographicSize.Width;
				}
				if ((alignment & TextAlignment.Right) != 0)
				{
					left = location.X - width;
				}
				else
				{
					left = location.X - width / 2;
				}
			}
			else
			{
				left = location.X;
			}
			int top;
			if ((alignment & TextAlignment.Top) == 0)
			{
				int height = 0;
				for (int i = 0; i < text.Length; i++)
				{
					int texture;
					Fonts.OpenGlFontChar data;
					i += font.GetCharacterData(text, i, out texture, out data) - 1;
					if (data.TypographicSize.Height > height)
					{
						height = data.TypographicSize.Height;
					}
				}
				if ((alignment & TextAlignment.Bottom) != 0)
				{
					top = location.Y - height;
				}
				else
				{
					top = location.Y - height / 2;
				}
			}
			else
			{
				top = location.Y;
			}
			/*
			 * Render the string.
			 * */
			GL.Enable(EnableCap.Texture2D);
			for (int i = 0; i < text.Length; i++)
			{
				int texture;
				Fonts.OpenGlFontChar data;
				i += font.GetCharacterData(text, i, out texture, out data) - 1;
				TextureManager.UseTexture(texture, TextureManager.UseMode.LoadImmediately);
				
				{
					GL.BindTexture(TextureTarget.Texture2D, TextureManager.Textures[texture].OpenGlTextureIndex);

					int x = left - (data.PhysicalSize.Width - data.TypographicSize.Width) / 2;
					int y = top - (data.PhysicalSize.Height - data.TypographicSize.Height) / 2;
					/*
					 * In the first pass, mask off the background with pure black.
					 * */
					GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.OneMinusSrcColor);
					GL.Begin(PrimitiveType.Polygon);
					GL.Color4(color.A, color.A, color.A, 1.0f);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Top);
					GL.Vertex2(x, y);
					GL.Color4(color.A, color.A, color.A, 1.0f);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Top);
					GL.Vertex2(x + data.PhysicalSize.Width, y);
					GL.Color4(color.A, color.A, color.A, 1.0f);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Bottom);
					GL.Vertex2(x + data.PhysicalSize.Width, y + data.PhysicalSize.Height);
					GL.Color4(color.A, color.A, color.A, 1.0f);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Bottom);
					GL.Vertex2(x, y + data.PhysicalSize.Height);
					GL.End();
					/*
					 * In the second pass, add the character onto the background.
					 * */
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					GL.Begin(PrimitiveType.Polygon);
					GL.Color4(color.R, color.G, color.B, color.A);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Top);
					GL.Vertex2(x, y);
					GL.Color4(color.R, color.G, color.B, color.A);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Top);
					GL.Vertex2(x + data.PhysicalSize.Width, y);
					GL.Color4(color.R, color.G, color.B, color.A);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Bottom);
					GL.Vertex2(x + data.PhysicalSize.Width, y + data.PhysicalSize.Height);
					GL.Color4(color.R, color.G, color.B, color.A);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Bottom);
					GL.Vertex2(x, y + data.PhysicalSize.Height);
					GL.End();
				}
				left += data.TypographicSize.Width;
			}
			GL.Disable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // HACK //
		}

		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <param name="shadow">Whether to draw a shadow.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		internal static void DrawString(Fonts.OpenGlFont font, string text, Point location, TextAlignment alignment, Color128 color, bool shadow)
		{
			if (shadow)
			{
				DrawString(font, text, new Point(location.X - 1, location.Y + 1), alignment, new Color128(0.0f, 0.0f, 0.0f, 0.5f * color.A));
				DrawString(font, text, location, alignment, color);
			}
			else
			{
				DrawString(font, text, location, alignment, color);
			}
		}

	}
}
