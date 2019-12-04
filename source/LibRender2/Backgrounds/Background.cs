using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Backgrounds
{
	public class Background
	{
		private readonly BaseRenderer renderer;

		internal Background(BaseRenderer Renderer)
		{
			renderer = Renderer;
		}

		/// <summary>Renders a background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="Scale">The scale</param>
		public void Render(BackgroundHandle Data, float Scale)
		{
			DynamicBackground dynamicBackground = Data as DynamicBackground;
			StaticBackground staticBackground = Data as StaticBackground;
			BackgroundObject backgroundObject = Data as BackgroundObject;

			if (dynamicBackground != null)
			{
				RenderDynamicBackground(dynamicBackground, Scale);
			}

			if (staticBackground != null)
			{
				RenderStaticBackground(staticBackground, Scale);
			}

			if (backgroundObject != null)
			{
				RenderBackgroundObject(backgroundObject);
			}
		}

		/// <summary>Renders a background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="Alpha">The alpha level</param>
		/// <param name="Scale">The scale</param>
		public void Render(BackgroundHandle Data, float Alpha, float Scale)
		{
			DynamicBackground dynamicBackground = Data as DynamicBackground;
			StaticBackground staticBackground = Data as StaticBackground;
			BackgroundObject backgroundObject = Data as BackgroundObject;

			if (dynamicBackground != null)
			{
				RenderDynamicBackground(dynamicBackground, Alpha, Scale);
			}

			if (staticBackground != null)
			{
				RenderStaticBackground(staticBackground, Alpha, Scale);
			}

			if (backgroundObject != null)
			{
				RenderBackgroundObject(backgroundObject);
			}
		}

		/// <summary>Renders a dynamic frustrum based background</summary>
		/// <param name="data">The background to render</param>
		/// <param name="scale">The scale</param>
		private void RenderDynamicBackground(DynamicBackground data, float scale)
		{
			if (data.PreviousBackgroundIndex == data.CurrentBackgroundIndex)
			{
				RenderStaticBackground(data.StaticBackgrounds[data.CurrentBackgroundIndex], 1.0f, scale);
				return;
			}

			switch (data.StaticBackgrounds[data.CurrentBackgroundIndex].Mode)
			{
				case BackgroundTransitionMode.FadeIn:
					RenderStaticBackground(data.StaticBackgrounds[data.PreviousBackgroundIndex], 1.0f, scale);
					RenderStaticBackground(data.StaticBackgrounds[data.CurrentBackgroundIndex], data.CurrentAlpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					RenderStaticBackground(data.StaticBackgrounds[data.CurrentBackgroundIndex], 1.0f, scale);
					RenderStaticBackground(data.StaticBackgrounds[data.PreviousBackgroundIndex], data.CurrentAlpha, scale);
					break;
			}
		}

		/// <summary>Renders a dynamic frustrum based background</summary>
		/// <param name="data">The background to render</param>
		/// <param name="alpha">The alpha level</param>
		/// <param name="scale">The scale</param>
		private void RenderDynamicBackground(DynamicBackground data, float alpha, float scale)
		{
			RenderStaticBackground(data.StaticBackgrounds[data.CurrentBackgroundIndex], alpha, scale);
		}

		/// <summary>Renders a static frustrum based background</summary>
		/// <param name="data">The background to render</param>
		/// <param name="scale">The scale</param>
		private void RenderStaticBackground(StaticBackground data, float scale)
		{
			RenderStaticBackground(data, 1.0f, scale);
		}

		/// <summary>Renders a static frustrum based background</summary>
		/// <param name="data">The background to render</param>
		/// <param name="alpha">The alpha level</param>
		/// <param name="scale">The scale</param>
		private void RenderStaticBackground(StaticBackground data, float alpha, float scale)
		{
			if (data.Texture != null && renderer.currentHost.LoadTexture(data.Texture, OpenGlTextureWrapMode.RepeatClamp))
			{
				GL.Enable(EnableCap.Texture2D);

				if (alpha == 1.0f)
				{
					GL.Disable(EnableCap.Blend);
				}
				else
				{
					GL.Enable(EnableCap.Blend);
				}

				if (data.VAO == null)
				{
					data.CreateVAO();
				}

				renderer.DefaultShader.Activate();
				renderer.ResetShader(renderer.DefaultShader);

				// matrix
				renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
				renderer.DefaultShader.SetCurrentModelViewMatrix(Matrix4D.Scale(scale) * renderer.CurrentViewMatrix);

				// fog
				if (renderer.OptionFog)
				{
					renderer.DefaultShader.SetIsFog(true);
					renderer.DefaultShader.SetFogStart(renderer.Fog.Start);
					renderer.DefaultShader.SetFogEnd(renderer.Fog.End);
					renderer.DefaultShader.SetFogColor(renderer.Fog.Color);
				}

				// texture
				renderer.DefaultShader.SetIsTexture(true);
				renderer.DefaultShader.SetTexture(0);
				GL.BindTexture(TextureTarget.Texture2D, data.Texture.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatClamp].Name);

				// alpha test
				renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);

				// blend mode
				renderer.DefaultShader.SetOpacity(alpha);

				// render polygon
				VertexArrayObject VAO = (VertexArrayObject) data.VAO;
				VAO.Bind();

				for (int i = 0; i + 9 < 32 * 10; i += 10)
				{
					VAO.Draw(renderer.DefaultShader.VertexLayout, PrimitiveType.Quads, i, 4);
					VAO.Draw(renderer.DefaultShader.VertexLayout, PrimitiveType.Triangles, i + 4, 3);
					VAO.Draw(renderer.DefaultShader.VertexLayout, PrimitiveType.Triangles, i + 7, 3);
				}

				VAO.UnBind();

				GL.BindTexture(TextureTarget.Texture2D, 0);
				renderer.DefaultShader.Deactivate();

				GL.Disable(EnableCap.Texture2D);
				renderer.RestoreBlendFunc();
			}
		}

		/// <summary>Renders an object based background</summary>
		/// <param name="data">The background object</param>
		private void RenderBackgroundObject(BackgroundObject data)
		{
			if (data.Object.Mesh.VAO == null)
			{
				VAOExtensions.CreateVAO(ref data.Object.Mesh, false);
			}

			foreach (MeshFace face in data.Object.Mesh.Faces)
			{
				OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;

				if (data.Object.Mesh.Materials[face.Material].DaytimeTexture != null)
				{
					if (data.Object.Mesh.Materials[face.Material].WrapMode == null)
					{
						foreach (VertexTemplate vertex in data.Object.Mesh.Vertices)
						{
							if (vertex.TextureCoordinates.X < 0.0f || vertex.TextureCoordinates.X > 1.0f)
							{
								wrap |= OpenGlTextureWrapMode.RepeatClamp;
							}

							if (vertex.TextureCoordinates.Y < 0.0f || vertex.TextureCoordinates.Y > 1.0f)
							{
								wrap |= OpenGlTextureWrapMode.ClampRepeat;
							}
						}

						data.Object.Mesh.Materials[face.Material].WrapMode = wrap;
					}
				}

				renderer.DefaultShader.Activate();
				renderer.ResetShader(renderer.DefaultShader);
				renderer.RenderFace(renderer.DefaultShader, new ObjectState { Prototype = data.Object }, face);
				renderer.DefaultShader.Deactivate();
			}
		}
	}
}
