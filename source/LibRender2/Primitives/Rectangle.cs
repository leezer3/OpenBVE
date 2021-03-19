using System;
using System.Drawing;
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

		private readonly VertexArrayObject dummyVao;

		internal Rectangle(BaseRenderer renderer, Shader shader)
		{
			this.renderer = renderer;
			this.Shader = shader;
			this.dummyVao = new VertexArrayObject();
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
		public void Draw(Texture texture, Vector2 point, Vector2 size, Color128? color = null)
		{
			if (renderer.AvailableNewRenderer && Shader != null)
			{
				DrawWithShader(texture, point, size, color);	
			}
			else
			{
				DrawImmediate(texture, point, size, color);	
			}
			
		}

		private void DrawImmediate(Texture texture, Vector2 point, Vector2 size, Color128? color)
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
				GL.TexCoord2(0.0f, 0.0f);
				GL.Vertex2(point.X, point.Y);
				GL.TexCoord2(1.0f, 0.0f);
				GL.Vertex2(point.X + size.X, point.Y);
				GL.TexCoord2(1.0f, 1.0f);
				GL.Vertex2(point.X + size.X, point.Y + size.Y);
				GL.TexCoord2(0.0f, 1.0f);
				GL.Vertex2(point.X, point.Y + size.Y);
				GL.End();
				GL.Disable(EnableCap.Texture2D);
			}

			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
		}

		private void DrawWithShader(Texture texture, Vector2 point, Vector2 size, Color128? color)
		{
			ErrorCode error = GL.GetError();
			if (error != ErrorCode.NoError)
			{
				throw new Exception();
			}
			Shader.Activate();
			renderer.CurrentShader = Shader;
			if (texture != null && renderer.currentHost.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp))
			{
				Shader.SetIsTexture(true);
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			}
			else
			{
				error = GL.GetError();
				if (error != ErrorCode.NoError)
				{
					throw new Exception();
				}
				Shader.SetIsTexture(false);
				
			}
			
			Shader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			Shader.SetCurrentModelViewMatrix(renderer.CurrentViewMatrix);
			Shader.SetColor(color == null ? Color128.White : color.Value);
			Shader.SetPoint(point);
			Shader.SetSize(size);
			/*
			 * In order to call GL.DrawArrays with procedural data within the shader,
			 * we first need to bind a dummy VAO
			 * If this is not done, it will generate an InvalidOperation error code
			 */
			dummyVao.Bind();
			GL.DrawArrays(PrimitiveType.Quads, 0, 4);
			dummyVao.UnBind();
			Shader.Deactivate();
		}
	}
}
