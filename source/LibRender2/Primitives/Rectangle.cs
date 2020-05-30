using LibRender2.Shaders;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Primitives
{
	public class Rectangle
	{
		/// <summary>Holds a reference to the base renderer</summary>
		private readonly BaseRenderer renderer;
		/// <summary>If using GL3, the shader to draw the rectangle with</summary>
		private readonly Shader Shader;

		internal Rectangle(BaseRenderer renderer)
		{
			this.renderer = renderer;
			try
			{
				Shader = new Shader(renderer, "rectangle", "rectangle", true);
			}
			catch
			{
				renderer.ForceLegacyOpenGL = true;
			}
		}

		/// <summary>Draws a simple 2D rectangle using two-pass alpha blending.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="size">The size in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		/// <param name="textureCoordinates">The texture coordinates to be applied</param>
		public void DrawAlpha(Texture texture, Vector2 point, Vector2 size, Color128? color = null, Vector2? textureCoordinates = null)
		{
			renderer.UnsetBlendFunc();
			renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
			GL.DepthMask(true);
			Draw(texture, point, size, color, textureCoordinates);
			renderer.SetBlendFunc();
			renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
			GL.DepthMask(false);
			Draw(texture, point, size, color, textureCoordinates);
			renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
		}

		/// <summary>Draws a simple 2D rectangle.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="size">The size in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		/// <param name="textureCoordinates">The texture coordinates to be applied</param>
		public void Draw(Texture texture, Vector2 point, Vector2 size, Color128? color = null, Vector2? textureCoordinates = null)
		{
			if (renderer.AvailableNewRenderer && Shader != null)
			{
				if (textureCoordinates == null)
				{
					DrawWithShader(texture, point, size, color, Vector2.One);
				}
				else
				{
					DrawWithShader(texture, point, size, color, (Vector2)textureCoordinates);
				}
			}
			else
			{
				DrawImmediate(texture, point, size, color, textureCoordinates);	
			}
		}

		private void DrawImmediate(Texture texture, Vector2 point, Vector2 size, Color128? color, Vector2? textureCoordinates = null)
		{
			renderer.LastBoundTexture = null;
			// TODO: Remove Nullable<T> from color once RenderOverlayTexture and RenderOverlaySolid are fully replaced.
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
			if (texture == null || !renderer.currentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.Disable(EnableCap.Texture2D);

				if (color.HasValue)
				{
					GL.Color4(color.Value.R, color.Value.G, color.Value.B, color.Value.A);
				}

				GL.Begin(PrimitiveType.Quads);
				GL.Vertex2(point.X, point.Y);
				GL.Vertex2(point.X + size.X, point.Y);
				GL.Vertex2(point.X + size.X, point.Y + size.Y);
				GL.Vertex2(point.X, point.Y + size.Y);
				GL.End();
			}
			else
			{
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);

				if (color.HasValue)
				{
					GL.Color4(color.Value.R, color.Value.G, color.Value.B, color.Value.A);
				}

				GL.Begin(PrimitiveType.Quads);
				if (textureCoordinates == null)
				{
					GL.TexCoord2(0,0);
					GL.Vertex2(point.X, point.Y);
					GL.TexCoord2(1,0);
					GL.Vertex2(point.X + size.X, point.Y);
					GL.TexCoord2(1,1);
					GL.Vertex2(point.X + size.X, point.Y + size.Y);
					GL.TexCoord2(0,1);
					GL.Vertex2(point.X, point.Y + size.Y);
				}
				else
				{
					Vector2 v = (Vector2) textureCoordinates;
					GL.TexCoord2(0,0);
					GL.Vertex2(point.X, point.Y);
					GL.TexCoord2(v.X, 0);
					GL.Vertex2(point.X + size.X, point.Y);
					GL.TexCoord2(v.X, v.Y);
					GL.Vertex2(point.X + size.X, point.Y + size.Y);
					GL.TexCoord2(0, v.Y);
					GL.Vertex2(point.X, point.Y + size.Y);
				}
				GL.End();
				GL.Disable(EnableCap.Texture2D);
			}

			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
		}

		private void DrawWithShader(Texture texture, Vector2 point, Vector2 size, Color128? color, Vector2 coordinates)
		{
			Shader.Activate();
			if (texture != null && renderer.currentHost.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
				renderer.LastBoundTexture = texture.OpenGlTextures[(int) OpenGlTextureWrapMode.ClampClamp];
			}
			else
			{
				Shader.DisableTexturing();
			}
			
			Shader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			Shader.SetCurrentModelViewMatrix(renderer.CurrentViewMatrix);
			Shader.SetColor(color == null ? Color128.White : color.Value);
			Shader.SetPoint(point);
			Shader.SetSize(size);
			Shader.SetCoordinates(coordinates);
			/*
			 * In order to call GL.DrawArrays with procedural data within the shader,
			 * we first need to bind a dummy VAO
			 * If this is not done, it will generate an InvalidOperation error code
			 */
			renderer.dummyVao.Bind();
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 6);
		}
	}
}
