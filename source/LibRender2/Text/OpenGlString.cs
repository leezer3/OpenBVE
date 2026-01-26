using LibRender2.Shaders;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Text
{
	public class OpenGlString
	{
		private readonly BaseRenderer renderer;

		private readonly Shader Shader;

		internal OpenGlString(BaseRenderer renderer)
		{
			this.renderer = renderer;
			try
			{
				if (!renderer.ForceLegacyOpenGL)
				{
					this.Shader = new Shader(renderer, "text", "rectangle", true);
				}
			}
			catch
			{
				renderer.ForceLegacyOpenGL = true;
			}
			
		}

		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		public void Draw(OpenGlFont font, string text, Vector2 location, TextAlignment alignment, Color128 color)
		{
			if (text == null || font == null)
			{
				return;
			}
			renderer.LastBoundTexture = null;
			/*
			 * Prepare the top-left coordinates for rendering, incorporating the
			 * orientation of the string in relation to the specified location.
			 * */
			double left;

			if ((alignment & TextAlignment.Left) == 0)
			{
				double width = 0;

				for (int i = 0; i < text.Length; i++)
				{
					i += font.GetCharacterData(text, i, out _, out OpenGlFontChar data) - 1;
					width += data.TypographicSize.X;
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

			double top;

			if ((alignment & TextAlignment.Top) == 0)
			{
				double height = 0;

				for (int i = 0; i < text.Length; i++)
				{
					i += font.GetCharacterData(text, i, out _, out OpenGlFontChar data) - 1;

					if (data.TypographicSize.Y > height)
					{
						height = data.TypographicSize.Y;
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

			if (renderer.AvailableNewRenderer && Shader != null)
			{
				DrawWithShader(text, font, left, top, color);
			}
			else
			{
				DrawImmediate(text, font, left, top, color);
			}

		}

		private void DrawImmediate(string text, OpenGlFont font, double left, double top, Color128 color)
		{
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
				i += font.GetCharacterData(text, i, out Texture texture, out OpenGlFontChar data) - 1;

				if (renderer.currentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
				{
					GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);

					double x = left - (data.PhysicalSize.X - data.TypographicSize.X) / 2;
					double y = top - (data.PhysicalSize.Y - data.TypographicSize.Y) / 2;

					/*
					 * In the first pass, mask off the background with pure black.
					 * */
					GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.OneMinusSrcColor);
					GL.Begin(PrimitiveType.Quads);
					GL.Color4(color.A, color.A, color.A, 1.0f);
					GL.TexCoord2(data.TextureCoordinates.X, data.TextureCoordinates.Y);
					GL.Vertex2(x, y);
					GL.TexCoord2(data.TextureCoordinates.X + data.TextureCoordinates.Z, data.TextureCoordinates.Y);
					GL.Vertex2(x + data.PhysicalSize.X, y);
					GL.TexCoord2(data.TextureCoordinates.X + data.TextureCoordinates.Z, data.TextureCoordinates.Y + data.TextureCoordinates.W);
					GL.Vertex2(x + data.PhysicalSize.X, y + data.PhysicalSize.Y);
					GL.TexCoord2(data.TextureCoordinates.X, data.TextureCoordinates.Y + data.TextureCoordinates.W);
					GL.Vertex2(x, y + data.PhysicalSize.Y);
					GL.End();

					/*
					 * In the second pass, add the character onto the background.
					 * */
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					GL.Begin(PrimitiveType.Quads);
					GL.Color4(color.R, color.G, color.B, color.A);
					GL.TexCoord2(data.TextureCoordinates.X, data.TextureCoordinates.Y);
					GL.Vertex2(x, y);
					GL.TexCoord2(data.TextureCoordinates.X + data.TextureCoordinates.Z, data.TextureCoordinates.Y);
					GL.Vertex2(x + data.PhysicalSize.X, y);
					GL.TexCoord2(data.TextureCoordinates.X + data.TextureCoordinates.Z, data.TextureCoordinates.Y + data.TextureCoordinates.W);
					GL.Vertex2(x + data.PhysicalSize.X, y + data.PhysicalSize.Y);
					GL.TexCoord2(data.TextureCoordinates.X, data.TextureCoordinates.Y + data.TextureCoordinates.W);
					GL.Vertex2(x, y + data.PhysicalSize.Y);
					GL.End();
				}

				left += data.TypographicSize.X;
			}

			renderer.RestoreBlendFunc();
			GL.Disable(EnableCap.Texture2D);

			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
		}

		private void DrawWithShader(string text, OpenGlFont font, double left, double top, Color128 color)
		{
			Shader.Activate();
			renderer.CurrentShader = Shader;
			Shader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			Shader.SetCurrentModelViewMatrix(renderer.CurrentViewMatrix);

			for (int i = 0; i < text.Length; i++)
			{
				i += font.GetCharacterData(text, i, out Texture texture, out OpenGlFontChar data) - 1;
				if (renderer.currentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
				{
					GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
					Shader.SetAtlasLocation(data.TextureCoordinates);
					double x = left - (data.PhysicalSize.X - data.TypographicSize.X) / 2;
					double y = top - (data.PhysicalSize.Y - data.TypographicSize.Y) / 2;

					/*
					 * In the first pass, mask off the background with pure black.
					 */
					GL.BlendFunc(BlendingFactor.Zero, BlendingFactor.OneMinusSrcColor);
					Shader.SetColor(new Color128(color.A, color.A, color.A, 1.0f));
					Shader.SetPoint(new Vector2(x, y));
					Shader.SetSize(data.PhysicalSize);
					/*
					 * In order to call GL.DrawArrays with procedural data within the shader,
					 * we first need to bind a dummy VAO
					* If this is not done, it will generate an InvalidOperation error code
					*/
					renderer.dummyVao.Bind();
					GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 6);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					Shader.SetColor(color);
					GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 6);
				}
				left += data.TypographicSize.X;
			}
			renderer.RestoreBlendFunc();
		}

		/// <summary>Renders a string to the screen.</summary>
		/// <param name="font">The font to use.</param>
		/// <param name="text">The string to render.</param>
		/// <param name="location">The location.</param>
		/// <param name="alignment">The alignment.</param>
		/// <param name="color">The color.</param>
		/// <param name="shadow">Whether to draw a shadow.</param>
		/// <remarks>This function sets the OpenGL blend function to glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA).</remarks>
		public void Draw(OpenGlFont font, string text, Vector2 location, TextAlignment alignment, Color128 color, bool shadow)
		{
			if (shadow)
			{
				Draw(font, text, new Vector2(location.X - 1, location.Y + 1), alignment, new Color128(0.0f, 0.0f, 0.0f, 0.5f * color.A));
				Draw(font, text, location, alignment, color);
			}
			else
			{
				Draw(font, text, location, alignment, color);
			}
		}
	}
}
