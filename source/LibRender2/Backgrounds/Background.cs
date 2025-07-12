using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
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
			switch (Data)
			{
				case DynamicBackground dynamicBackground:
					RenderDynamicBackground(dynamicBackground, Scale);
					break;
				case StaticBackground staticBackground:
					RenderStaticBackground(staticBackground, Scale);
					break;
				case BackgroundObject backgroundObject:
					RenderBackgroundObject(backgroundObject);
					break;
			}
		}

		/// <summary>Renders a background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="Alpha">The alpha level</param>
		/// <param name="Scale">The scale</param>
		public void Render(BackgroundHandle Data, float Alpha, float Scale)
		{
			switch (Data)
			{
				case DynamicBackground dynamicBackground:
					RenderDynamicBackground(dynamicBackground, Alpha, Scale);
					break;
				case StaticBackground staticBackground:
					RenderStaticBackground(staticBackground, Alpha, Scale);
					break;
				case BackgroundObject backgroundObject:
					RenderBackgroundObject(backgroundObject);
					break;
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
			RenderStaticBackgroundRetained(data, alpha, scale);
		}

		/// <summary>Renders a static frustrum based background</summary>
		/// <param name="data">The background to render</param>
		/// <param name="alpha">The alpha level</param>
		/// <param name="scale">The scale</param>
		private void RenderStaticBackgroundRetained(StaticBackground data, float alpha, float scale)
		{
			Texture t = data.Texture;
			if (data.Texture != null && renderer.currentHost.LoadTexture(ref t, OpenGlTextureWrapMode.RepeatClamp))
			{
				renderer.LastBoundTexture = t.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatClamp];
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
					data.CreateVAO(renderer.DefaultShader.VertexLayout, renderer);
					if (data.VAO == null)
					{
						// Failed during creation of the VAO
						return;
					}
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
					renderer.DefaultShader.SetFog(renderer.Fog);
				}

				// texture
				GL.BindTexture(TextureTarget.Texture2D, t.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatClamp].Name);
				renderer.LastBoundTexture = null;

				// alpha test
				renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);

				// blend mode
				renderer.DefaultShader.SetOpacity(alpha);

				// render polygon
				VertexArrayObject VAO = (VertexArrayObject)data.VAO;
				VAO.Bind();
				renderer.lastVAO = VAO.handle;
				for (int i = 0; i + 11 < 32 * 12; i += 12)
				{
					VAO.Draw(PrimitiveType.Triangles, i, 12);
				}
				renderer.RestoreBlendFunc();
			}
		}

		/// <summary>Renders an object based background</summary>
		/// <param name="data">The background object</param>
		private void RenderBackgroundObject(BackgroundObject data)
		{
			GL.Enable(EnableCap.Blend);
			// alpha test
			renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			renderer.DefaultShader.Activate();
			renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);

			if (data.Object.Mesh.VAO == null)
			{
				VAOExtensions.CreateVAO(data.Object.Mesh, false, renderer.DefaultShader.VertexLayout, renderer);
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
				GL.Enable(EnableCap.DepthClamp);
				renderer.RenderFace(renderer.DefaultShader, data.ObjectState, face, Matrix4D.NoTransformation, Matrix4D.Scale(1.0) * renderer.CurrentViewMatrix);
				GL.Disable(EnableCap.DepthClamp);
			}
		}
	}
}
