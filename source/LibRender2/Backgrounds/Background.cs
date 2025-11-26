using System;
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
			if (renderer.AvailableNewRenderer)
			{
				RenderStaticBackgroundRetained(data, alpha, scale);
			}
			else
			{
				RenderStaticBackgroundImmediate(data, alpha, scale);
			}
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
				if (renderer.Fog.Enabled)
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

		/// <summary>Renders a static frustrum based background</summary>
		/// <param name="data">The background to render</param>
		/// <param name="alpha">The alpha level</param>
		/// <param name="scale">The scale</param>
		private void RenderStaticBackgroundImmediate(StaticBackground data, float alpha, float scale)
		{
			Texture t = data.Texture;
			if (data.Texture != null && renderer.currentHost.LoadTexture(ref t, OpenGlTextureWrapMode.RepeatClamp))
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.PushMatrix();
				GL.LoadIdentity();
				OpenTK.Matrix4d perspective = OpenTK.Matrix4d.Perspective(renderer.Camera.VerticalViewingAngle, -renderer.Screen.AspectRatio, 0.2, 1000.0);
				GL.MultMatrix(ref perspective);
				double dx = renderer.Camera.AbsoluteDirection.X;
				double dy = renderer.Camera.AbsoluteDirection.Y;
				double dz = renderer.Camera.AbsoluteDirection.Z;
				double ux = renderer.Camera.AbsoluteUp.X;
				double uy = renderer.Camera.AbsoluteUp.Y;
				double uz = renderer.Camera.AbsoluteUp.Z;
				OpenTK.Matrix4d lookat = OpenTK.Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.PushMatrix();
				GL.LoadMatrix(ref lookat);
				GL.Disable(EnableCap.Lighting);
				GL.Enable(EnableCap.Texture2D);
				if (alpha == 1.0f)
				{
					GL.Disable(EnableCap.Blend);
				}
				else
				{
					GL.Enable(EnableCap.Blend);
					renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				}
				GL.BindTexture(TextureTarget.Texture2D, t.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatClamp].Name);
				renderer.LastBoundTexture = t.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatClamp];
				GL.Color4(1.0f, 1.0f, 1.0f, alpha);
				if (renderer.Fog.Enabled)
				{
					GL.Enable(EnableCap.Fog);
				}
				if (data.DisplayList > 0)
				{
					GL.CallList(data.DisplayList);
					GL.Disable(EnableCap.Texture2D);
					GL.Enable(EnableCap.Blend);
					GL.PopMatrix();
					GL.MatrixMode(MatrixMode.Projection);
					GL.PopMatrix();
					return;
				}

				data.DisplayList = GL.GenLists(1);
				GL.NewList(data.DisplayList, ListMode.Compile);
				float y0, y1;
				if (data.KeepAspectRatio)
				{
					double hh = Math.PI * data.BackgroundImageDistance * data.Texture.Height / (data.Texture.Width * data.Repetition);
					y0 = (float)(-0.5 * hh);
					y1 = (float)(1.5 * hh);
				}
				else
				{
					y0 = (float)(-0.125 * data.BackgroundImageDistance);
					y1 = (float)(0.375 * data.BackgroundImageDistance);
				}
				const int n = 32;
				Vector3[] bottom = new Vector3[n];
				Vector3[] top = new Vector3[n];
				double angleValue = 2.61799387799149 - 3.14159265358979 / n;
				const double angleIncrement = 6.28318530717958 / n;
				/*
				 * To ensure that the whole background cylinder is rendered inside the viewing frustum,
				 * the background is rendered before the scene with z-buffer writes disabled. Then,
				 * the actual distance from the camera is irrelevant as long as it is inside the frustum.
				 * */
				for (int i = 0; i < n; i++)
				{
					float x = (float)(data.BackgroundImageDistance * Math.Cos(angleValue));
					float z = (float)(data.BackgroundImageDistance * Math.Sin(angleValue));
					bottom[i] = new Vector3(scale * x, scale * y0, scale * z);
					top[i] = new Vector3(scale * x, scale * y1, scale * z);
					angleValue += angleIncrement;
				}
				float textureStart = 0.5f * (float)data.Repetition / n;
				float textureIncrement = -(float)data.Repetition / n;
				double textureX = textureStart;
				for (int i = 0; i < n; i++)
				{
					int j = (i + 1) % n;
					// side wall
					GL.Begin(PrimitiveType.Quads);
					GL.TexCoord2(textureX, 0.005f);
					GL.Vertex3(top[i].X, top[i].Y, top[i].Z);
					GL.TexCoord2(textureX, 0.995f);
					GL.Vertex3(bottom[i].X, bottom[i].Y, bottom[i].Z);
					GL.TexCoord2(textureX + textureIncrement, 0.995f);
					GL.Vertex3(bottom[j].X, bottom[j].Y, bottom[j].Z);
					GL.TexCoord2(textureX + textureIncrement, 0.005f);
					GL.Vertex3(top[j].X, top[j].Y, top[j].Z);
					GL.End();
					// top cap
					GL.Begin(PrimitiveType.Triangles);
					GL.TexCoord2(textureX, 0.005f);
					GL.Vertex3(top[i].X, top[i].Y, top[i].Z);
					GL.TexCoord2(textureX + textureIncrement, 0.005f);
					GL.Vertex3(top[j].X, top[j].Y, top[j].Z);
					GL.TexCoord2(textureX + 0.5 * textureIncrement, 0.1f);
					GL.Vertex3(0.0f, top[i].Y, 0.0f);
					// bottom cap
					GL.TexCoord2(textureX + 0.5 * textureIncrement, 0.9f);
					GL.Vertex3(0.0f, bottom[i].Y, 0.0f);
					GL.TexCoord2(textureX + textureIncrement, 0.995f);
					GL.Vertex3(bottom[j].X, bottom[j].Y, bottom[j].Z);
					GL.TexCoord2(textureX, 0.995f);
					GL.Vertex3(bottom[i].X, bottom[i].Y, bottom[i].Z);
					GL.End();
					// finish
					textureX += textureIncrement;
				}
				GL.EndList();
				GL.CallList(data.DisplayList);
				GL.Disable(EnableCap.Texture2D);
				GL.Enable(EnableCap.Blend);
				GL.PopMatrix();
				GL.MatrixMode(MatrixMode.Projection);
				GL.PopMatrix();
			}
		}

		/// <summary>Renders an object based background</summary>
		/// <param name="data">The background object</param>
		private void RenderBackgroundObject(BackgroundObject data)
		{
			GL.Enable(EnableCap.Blend);
			// alpha test
			renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if (renderer.AvailableNewRenderer)
			{
				renderer.DefaultShader.Activate();
				renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);

				if (data.Object.Mesh.VAO == null)
				{
					VAOExtensions.CreateVAO(data.Object.Mesh, false, renderer.DefaultShader.VertexLayout, renderer);
				}
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
				if (renderer.AvailableNewRenderer)
				{
					renderer.RenderFace(renderer.DefaultShader, data.ObjectState, face, Matrix4D.NoTransformation, Matrix4D.Scale(1.0) * renderer.CurrentViewMatrix);
				}
				else
				{
					renderer.RenderFaceImmediateMode(data.ObjectState, face, Matrix4D.NoTransformation, Matrix4D.Scale(1.0) * renderer.CurrentViewMatrix);
				}
				GL.Disable(EnableCap.DepthClamp);
			}
		}
	}
}
