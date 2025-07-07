//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using LibRender2.Shaders;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
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
				renderer.currentHost.AddMessage(MessageType.Error, false, "Initializing the rectangle shader failed.");
			}
		}

		/// <summary>Draws a simple 2D rectangle using two-pass alpha blending.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="size">The size in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		/// <param name="textureCoordinates">The texture coordinates to be applied</param>
		/// <param name="wrapMode">A wrap mode if overriding that of the texture</param>
		public void DrawAlpha(Texture texture, Vector2 point, Vector2 size, Color128? color = null, Vector2? textureCoordinates = null, OpenGlTextureWrapMode? wrapMode = null)
		{
			renderer.UnsetBlendFunc();
			renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
			GL.DepthMask(true);
			Draw(texture, point, size, color, textureCoordinates, wrapMode);
			renderer.SetBlendFunc();
			renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
			GL.DepthMask(false);
			Draw(texture, point, size, color, textureCoordinates, wrapMode);
			renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
		}

		/// <summary>Draws a simple 2D rectangle using two-pass alpha blending, using the texture size.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		/// <param name="textureCoordinates">The texture coordinates to be applied</param>
		public void DrawAlpha(Texture texture, Vector2 point, Color128? color = null, Vector2? textureCoordinates = null)
		{
			if (texture == null)
			{
				return;
			}
			renderer.UnsetBlendFunc();
			renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
			GL.DepthMask(true);
			Draw(texture, point, texture.Size, color, textureCoordinates);
			renderer.SetBlendFunc();
			renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
			GL.DepthMask(false);
			Draw(texture, point, texture.Size, color, textureCoordinates);
			renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
		}

		/// <summary>Draws a simple 2D rectangle.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="size">The size in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		/// <param name="textureCoordinates">The texture coordinates to be applied</param>
		/// <param name="wrapMode">A wrap mode if overriding that of the texture</param>
		public void Draw(Texture texture, Vector2 point, Vector2 size, Color128? color = null, Vector2? textureCoordinates = null, OpenGlTextureWrapMode? wrapMode = null)
		{
			if (textureCoordinates == null)
			{
				DrawWithShader(texture, point, size, color, Vector2.One, wrapMode);
			}
			else
			{
				DrawWithShader(texture, point, size, color, (Vector2)textureCoordinates, wrapMode);
			}
		}

		/// <summary>Draws a simple 2D rectangle, using the texture size.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		/// <param name="textureCoordinates">The texture coordinates to be applied</param>
		public void Draw(Texture texture, Vector2 point, Color128? color = null, Vector2? textureCoordinates = null)
		{
			if (texture == null || Shader == null)
			{
				return;
			}
			if (textureCoordinates == null)
			{
				DrawWithShader(texture, point, texture.Size, color, Vector2.One);
			}
			else
			{
				DrawWithShader(texture, point, texture.Size, color, (Vector2)textureCoordinates);
			}
		}
		
		private void DrawWithShader(Texture texture, Vector2 point, Vector2 size, Color128? color, Vector2 coordinates, OpenGlTextureWrapMode? wrapMode = null)
		{
			Shader.Activate();
			if (wrapMode == null)
			{
				wrapMode = OpenGlTextureWrapMode.ClampClamp;
			}

			if (texture != null && renderer.currentHost.LoadTexture(ref texture, (OpenGlTextureWrapMode)wrapMode))
			{
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)wrapMode].Name);
				renderer.LastBoundTexture = texture.OpenGlTextures[(int)wrapMode];
			}
			else
			{
				Shader.DisableTexturing();
			}
			
			Shader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			Shader.SetCurrentModelViewMatrix(renderer.CurrentViewMatrix);
			Shader.SetColor(color ?? Color128.White);
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
