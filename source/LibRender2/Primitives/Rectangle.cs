using System;
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

		/// <summary>Renders an overlay texture</summary>
		/// <param name="texture">The texture</param>
		/// <param name="left">The left co-ordinate</param>
		/// <param name="top">The top co-ordinate</param>
		/// <param name="right">The right co-ordinate</param>
		/// <param name="bottom">The bottom co-ordinate</param>
		public void RenderOverlayTexture(Texture texture, double left, double top, double right, double bottom)
		{
			Draw(texture, new Vector2(left, top), new Vector2((right - left), (bottom - top)));
		}

		/// <summary>Renders a solid color rectangular overlay</summary>
		/// <param name="left">The left co-ordinate</param>
		/// <param name="top">The top co-ordinate</param>
		/// <param name="right">The right co-ordinate</param>
		/// <param name="bottom">The bottom co-ordinate</param>
		public void RenderOverlaySolid(double left, double top, double right, double bottom)
		{
			Draw(null, new Vector2(left, top), new Vector2((right - left), (bottom - top)));
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
			if (texture == null || !renderer.currentHost.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp))
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
			renderer.CurrentShader = Shader;
			if (texture != null && renderer.currentHost.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp))
			{
				Shader.SetIsTexture(true);
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			}
			else
			{
				Shader.SetIsTexture(false);
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
			GL.DrawArrays(PrimitiveType.Quads, 0, 4);
			renderer.dummyVao.UnBind();
			Shader.Deactivate();
		}
	}
}
