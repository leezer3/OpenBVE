using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Texts
{
	public class OpenGlString
	{
		private readonly BaseRenderer renderer;

		internal OpenGlString(BaseRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		public void Draw(OpenGlFont font, string text, Point location, TextAlignment alignment, Color128 color)
		{
			if (text == null || font == null)
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
					Texture texture;
					OpenGlFontChar data;
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
					Texture texture;
					OpenGlFontChar data;
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

			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			unsafe
			{
				fixed (double* matrixPointer = &renderer.CurrentProjectionMatrix.Row0.X)
				{
					GL.LoadMatrix(matrixPointer);
				}
				GL.MatrixMode(MatrixMode.Modelview);
				GL.PushMatrix();
				fixed (double* matrixPointer = &renderer.CurrentViewMatrix.Row0.X)
				{
					GL.LoadMatrix(matrixPointer);
				}
			}
			

			for (int i = 0; i < text.Length; i++)
			{
				Texture texture;
				OpenGlFontChar data;
				i += font.GetCharacterData(text, i, out texture, out data) - 1;

				if (renderer.currentHost.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp))
				{
					GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);

					int x = left - (data.PhysicalSize.Width - data.TypographicSize.Width) / 2;
					int y = top - (data.PhysicalSize.Height - data.TypographicSize.Height) / 2;

					/*
					 * In the first pass, mask off the background with pure black.
					 * */
					GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.OneMinusSrcColor);
					GL.Begin(PrimitiveType.Quads);
					GL.Color4(color.A, color.A, color.A, 1.0f);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Top);
					GL.Vertex2(x, y);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Top);
					GL.Vertex2(x + data.PhysicalSize.Width, y);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Bottom);
					GL.Vertex2(x + data.PhysicalSize.Width, y + data.PhysicalSize.Height);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Bottom);
					GL.Vertex2(x, y + data.PhysicalSize.Height);
					GL.End();

					/*
					 * In the second pass, add the character onto the background.
					 * */
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					GL.Begin(PrimitiveType.Quads);
					GL.Color4(color.R, color.G, color.B, color.A);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Top);
					GL.Vertex2(x, y);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Top);
					GL.Vertex2(x + data.PhysicalSize.Width, y);
					GL.TexCoord2(data.TextureCoordinates.Right, data.TextureCoordinates.Bottom);
					GL.Vertex2(x + data.PhysicalSize.Width, y + data.PhysicalSize.Height);
					GL.TexCoord2(data.TextureCoordinates.Left, data.TextureCoordinates.Bottom);
					GL.Vertex2(x, y + data.PhysicalSize.Height);
					GL.End();
				}

				left += data.TypographicSize.Width;
			}

			renderer.RestoreBlendFunc();

			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Disable(EnableCap.Texture2D);

			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
		}

		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <param name="shadow">Whether to draw a shadow.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		public void Draw(OpenGlFont font, string text, Point location, TextAlignment alignment, Color128 color, bool shadow)
		{
			if (shadow)
			{
				Draw(font, text, new Point(location.X - 1, location.Y + 1), alignment, new Color128(0.0f, 0.0f, 0.0f, 0.5f * color.A));
				Draw(font, text, location, alignment, color);
			}
			else
			{
				Draw(font, text, location, alignment, color);
			}
		}
	}
}
