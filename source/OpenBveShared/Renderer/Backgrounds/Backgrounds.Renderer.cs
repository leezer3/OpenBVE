using System;
using BackgroundManager;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>Whether an openGL display list is available for the current background</summary>
		private static bool BackgroundDisplayListAvailable;
		/// <summary>The index to the openGL display list</summary>
		private static int BackgroundDisplayList;

		/// <summary>Updates the currently displayed background</summary>
		/// <param name="CurrentTime">The current in-game time expressed in seconds since midnight</param>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		public static void UpdateBackground(double CurrentTime, double TimeElapsed)
		{
			const float scale = 0.5f;
			// fog
			const float fogdistance = 600.0f;
			if (CurrentFog.Start < CurrentFog.End & CurrentFog.Start < fogdistance)
			{
				float cr = inv255 * (float)CurrentFog.Color.R;
				float cg = inv255 * (float)CurrentFog.Color.G;
				float cb = inv255 * (float)CurrentFog.Color.B;
				if (!FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				float ratio = (float)BackgroundImageDistance / fogdistance;
				GL.Fog(FogParameter.FogStart, CurrentFog.Start * ratio * scale);
				GL.Fog(FogParameter.FogEnd, CurrentFog.End * ratio * scale);
				GL.Fog(FogParameter.FogColor, new float[] { cr, cg, cb, 1.0f });
				if (!FogEnabled)
				{
					GL.Enable(EnableCap.Fog); FogEnabled = true;
				}
			}
			else if (FogEnabled)
			{
				GL.Disable(EnableCap.Fog); FogEnabled = false;
			}
			//Update the currently displayed background
			CurrentBackground.UpdateBackground(CurrentTime, TimeElapsed, false);
			if (TargetBackground == null || TargetBackground == CurrentBackground)
			{
				//No target background, so call the render function
				RenderBackground(CurrentBackground, scale);
				return;
			}
			//Update the target background
			if (TargetBackground is StaticBackground)
			{
				TargetBackground.Countdown += TimeElapsed;
			}
			TargetBackground.UpdateBackground(CurrentTime, TimeElapsed, true);
			switch (TargetBackground.Mode)
			{
				//Render, switching on the transition mode
				case BackgroundTransitionMode.FadeIn:
					RenderBackground(CurrentBackground, 1.0f, scale);
					SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					RenderBackground(TargetBackground, TargetBackground.Alpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					RenderBackground(TargetBackground, 1.0f, scale);
					SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					RenderBackground(CurrentBackground, TargetBackground.Alpha, scale);
					break;
			}
			//If our target alpha is greater than or equal to 1.0f, the background is fully displayed
			if (TargetBackground.Alpha >= 1.0f)
			{
				//Set the current background to the target & reset target to null
				CurrentBackground = TargetBackground;
				TargetBackground = null;
			}
			
		}

		internal static void RenderBackground(BackgroundHandle handle, float Scale)
		{
			if (handle is StaticBackground)
			{
				RenderBackground((StaticBackground)handle, 1.0f, Scale);
			}
			else if (handle is DynamicBackground)
			{
				DynamicBackground b = (DynamicBackground)handle;
				if (b.CurrentBackgroundIndex != b.PreviousBackgroundIndex)
				{
					switch (b.Backgrounds[b.CurrentBackgroundIndex].Mode)
					{
						case BackgroundTransitionMode.FadeIn:
							RenderBackground(b.Backgrounds[b.PreviousBackgroundIndex], 1.0f, Scale);
							SetAlphaFunc(AlphaFunction.Greater, 0.0f);
							RenderBackground(b.Backgrounds[b.CurrentBackgroundIndex], b.Alpha, Scale);
							break;
						case BackgroundTransitionMode.FadeOut:
							RenderBackground(b.Backgrounds[b.CurrentBackgroundIndex], 1.0f, Scale);
							SetAlphaFunc(AlphaFunction.Greater, 0.0f);
							RenderBackground(b.Backgrounds[b.PreviousBackgroundIndex], b.Alpha, Scale);
							break;
					}
				}
			}
			else if(handle is BackgroundObject)
			{
				RenderBackground((BackgroundObject)handle);
			}
		}

		internal static void RenderBackground(BackgroundHandle handle, float Alpha, float Scale)
		{
			if (handle is StaticBackground)
			{
				RenderBackground((StaticBackground)handle, Alpha, Scale);
			}
			else if (handle is DynamicBackground)
			{
				DynamicBackground b = (DynamicBackground)handle;
				RenderBackground(b.Backgrounds[b.CurrentBackgroundIndex], Alpha, Scale);
			}
			else if(handle is BackgroundObject)
			{
				RenderBackground((BackgroundObject)handle);
			}
		}

		/// <summary>Renders a static frustrum based background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="Alpha">The alpha level</param>
		/// <param name="scale">The scale</param>
		internal static void RenderBackground(StaticBackground Data, float Alpha, float scale)
		{
			if (Data.Texture != null && currentHost.LoadTexture(Data.Texture, OpenGlTextureWrapMode.RepeatClamp))
			{
				if (LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					LightingEnabled = false;
				}
				if (!TexturingEnabled)
				{
					GL.Enable(EnableCap.Texture2D);
					TexturingEnabled = true;
				}
				if (Alpha == 1.0f)
				{
					if (BlendEnabled)
					{
						GL.Disable(EnableCap.Blend);
						BlendEnabled = false;
					}
				}
				else if (!BlendEnabled)
				{
					GL.Enable(EnableCap.Blend);
					BlendEnabled = true;
				}
				GL.BindTexture(TextureTarget.Texture2D, Data.Texture.OpenGlTextures[(int)OpenGlTextureWrapMode.RepeatClamp].Name);
				GL.Color4(1.0f, 1.0f, 1.0f, Alpha);

				if (!BackgroundDisplayListAvailable)
				{
					BackgroundDisplayList = GL.GenLists(1);
					GL.NewList(BackgroundDisplayList, ListMode.Compile);
					float y0, y1;
					if (Data.KeepAspectRatio)
					{
						int tw = Data.Texture.Width;
						int th = Data.Texture.Height;
						double hh = Math.PI * BackgroundImageDistance * (double)th /
									((double)tw * (double)Data.Repetition);
						y0 = (float)(-0.5 * hh);
						y1 = (float)(1.5 * hh);
					}
					else
					{
						y0 = (float)(-0.125 * BackgroundImageDistance);
						y1 = (float)(0.375 * BackgroundImageDistance);
					}
					const int n = 32;
					Vector3[] bottom = new Vector3[n];
					Vector3[] top = new Vector3[n];
					double angleValue = 2.61799387799149 - 3.14159265358979 / (double)n;
					const double angleIncrement = 6.28318530717958 / (double)n;
					/*
				 * To ensure that the whole background cylinder is rendered inside the viewing frustum,
				 * the background is rendered before the scene with z-buffer writes disabled. Then,
				 * the actual distance from the camera is irrelevant as long as it is inside the frustum.
				 * */
					for (int i = 0; i < n; i++)
					{
						float x = (float)(BackgroundImageDistance * Math.Cos(angleValue));
						float z = (float)(BackgroundImageDistance * Math.Sin(angleValue));
						bottom[i] = new Vector3(scale * x, scale * y0, scale * z);
						top[i] = new Vector3(scale * x, scale * y1, scale * z);
						angleValue += angleIncrement;
					}
					float textureStart = 0.5f * (float)Data.Repetition / (float)n;
					float textureIncrement = -(float)Data.Repetition / (float)n;
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
					GL.CallList(BackgroundDisplayList);
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
					if (!BlendEnabled)
					{
						GL.Enable(EnableCap.Blend);
						BlendEnabled = true;
					}

					BackgroundDisplayListAvailable = true;
				}
				else
				{
					GL.CallList(BackgroundDisplayList);
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
					if (!BlendEnabled)
					{
						GL.Enable(EnableCap.Blend);
						BlendEnabled = true;
					}
				}
			}
		}

		/// <summary>Renders a dynamic frustrum based background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="scale">The scale</param>
		internal static void RenderBackground(DynamicBackground Data, float scale)
		{
			if (Data.PreviousBackgroundIndex == Data.CurrentBackgroundIndex)
			{
				RenderBackground(Data.Backgrounds[Data.CurrentBackgroundIndex], 1.0f, scale);
				return;
			}
			SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			switch (Data.Backgrounds[Data.CurrentBackgroundIndex].Mode)
			{
				case BackgroundTransitionMode.FadeIn:
					RenderBackground(Data.Backgrounds[Data.PreviousBackgroundIndex], 1.0f, scale);
					RenderBackground(Data.Backgrounds[Data.CurrentBackgroundIndex], Data.Alpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					RenderBackground(Data.Backgrounds[Data.CurrentBackgroundIndex], 1.0f, scale);
					RenderBackground(Data.Backgrounds[Data.PreviousBackgroundIndex], Data.Alpha, scale);
					break;
			}
		}

		internal static void RenderBackground(BackgroundObject Object)
		{
			if (!TexturingEnabled)
			{
				GL.Enable(EnableCap.Texture2D);
				TexturingEnabled = true;
			}
			int Mat = -1;
			for (int i = 0; i < Object.ObjectBackground.Mesh.Faces.Length; i++)
			{
				int m = Object.ObjectBackground.Mesh.Faces[i].Material;
				if (m != Mat)
				{
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					for (int v = 0; v < Object.ObjectBackground.Mesh.Vertices.Length; v++)
					{
						if (Object.ObjectBackground.Mesh.Vertices[v].TextureCoordinates.X < 0.0f | Object.ObjectBackground.Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
						{
							wrap |= OpenGlTextureWrapMode.RepeatClamp;
						}
						if (Object.ObjectBackground.Mesh.Vertices[v].TextureCoordinates.Y < 0.0f | Object.ObjectBackground.Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
						{
							wrap |= OpenGlTextureWrapMode.ClampRepeat;
						}
					}

					if (Object.ObjectBackground.Mesh.Materials[m].DaytimeTexture != null)
					{
						currentHost.LoadTexture(Object.ObjectBackground.Mesh.Materials[m].DaytimeTexture, wrap);
						GL.BindTexture(TextureTarget.Texture2D, Object.ObjectBackground.Mesh.Materials[m].DaytimeTexture.OpenGlTextures[(int) wrap].Name);
					}
				}
				int FaceType = Object.ObjectBackground.Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
				switch (FaceType)
				{
					case MeshFace.FaceTypeTriangles:
						GL.Begin(PrimitiveType.Triangles);
						break;
					case MeshFace.FaceTypeTriangleStrip:
						GL.Begin(PrimitiveType.TriangleStrip);
						break;
					case MeshFace.FaceTypeQuads:
						GL.Begin(PrimitiveType.Quads);
						break;
					case MeshFace.FaceTypeQuadStrip:
						GL.Begin(PrimitiveType.QuadStrip);
						break;
					default:
						GL.Begin(PrimitiveType.Polygon);
						break;
				}
				
				for (int j = 0; j < Object.ObjectBackground.Mesh.Faces[i].Vertices.Length; j++)
				{
					GL.Color4(inv255 * (float)Object.ObjectBackground.Mesh.Materials[m].Color.R * 1.0f, inv255 * Object.ObjectBackground.Mesh.Materials[m].Color.G * 1.0f, inv255 * (float)Object.ObjectBackground.Mesh.Materials[m].Color.B * 1.0f, inv255 * (float)Object.ObjectBackground.Mesh.Materials[m].Color.A);
					VertexTemplate v = Object.ObjectBackground.Mesh.Vertices[Object.ObjectBackground.Mesh.Faces[i].Vertices[j].Index];
					if (v is ColoredVertex)
					{
						ColoredVertex vv = v as ColoredVertex;
						GL.Color3(vv.Color.R, vv.Color.G, vv.Color.B);
						
					}
					GL.TexCoord2(v.TextureCoordinates.X, v.TextureCoordinates.Y);
					GL.Vertex3(v.Coordinates.X, v.Coordinates.Y, v.Coordinates.Z);
					
				}
				GL.End();
			}
		}
		
	}
}
