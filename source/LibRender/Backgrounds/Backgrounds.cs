using System;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender
{
	public static class Backgrounds
	{
		/// <summary>The user-selected viewing distance.</summary>
		public static double BackgroundImageDistance;
		/// <summary>Whether an openGL display list is available for the current background</summary>
		private static bool BackgroundDisplayListAvailable;
		/// <summary>The index to the openGL display list</summary>
		private static int BackgroundDisplayList;

		/// <summary>Renders a static frustrum based background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="Alpha">The alpha level</param>
		/// <param name="scale">The scale</param>
		public static void RenderBackground(dynamic Data, float Alpha, float scale)
		{
			if (Data.Texture != null && Renderer.currentHost.LoadTexture(Data.Texture, OpenGlTextureWrapMode.RepeatClamp))
			{
				if (Renderer.LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					Renderer.LightingEnabled = false;
				}
				if (!Renderer.TexturingEnabled)
				{
					GL.Enable(EnableCap.Texture2D);
					Renderer.TexturingEnabled = true;
				}
				if (Alpha == 1.0f)
				{
					if (Renderer.BlendEnabled)
					{
						GL.Disable(EnableCap.Blend);
						Renderer.BlendEnabled = false;
					}
				}
				else if (!Renderer.BlendEnabled)
				{
					GL.Enable(EnableCap.Blend);
					Renderer.BlendEnabled = true;
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
					Renderer.TexturingEnabled = false;
					if (!Renderer.BlendEnabled)
					{
						GL.Enable(EnableCap.Blend);
						Renderer.BlendEnabled = true;
					}

					BackgroundDisplayListAvailable = true;
				}
				else
				{
					GL.CallList(BackgroundDisplayList);
					GL.Disable(EnableCap.Texture2D);
					Renderer.TexturingEnabled = false;
					if (!Renderer.BlendEnabled)
					{
						GL.Enable(EnableCap.Blend);
						Renderer.BlendEnabled = true;
					}
				}
			}
		}

		/// <summary>Renders a dynamic frustrum based background</summary>
		/// <param name="Data">The background to render</param>
		/// <param name="scale">The scale</param>
		public static void RenderBackground(dynamic Data, float scale)
		{
			if (Data.PreviousBackgroundIndex == Data.CurrentBackgroundIndex)
			{
				RenderBackground(Data.Backgrounds[Data.CurrentBackgroundIndex], 1.0f, scale);
				return;
			}
			Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			BackgroundTransitionMode Mode = Data.Backgrounds[Data.CurrentBackgroundIndex].Mode; //Must do this to make the switch work correctly using a dynamic
			switch (Mode)
			{
				case BackgroundTransitionMode.FadeIn:
					RenderBackground(Data.Backgrounds[Data.PreviousBackgroundIndex], 1.0f, scale);
					RenderBackground(Data.Backgrounds[Data.CurrentBackgroundIndex], Data.CurrentAlpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					RenderBackground(Data.Backgrounds[Data.CurrentBackgroundIndex], 1.0f, scale);
					RenderBackground(Data.Backgrounds[Data.PreviousBackgroundIndex], Data.CurrentAlpha, scale);
					break;
			}
		}

		/// <summary>Renders an object based background</summary>
		/// <param name="Object">The background object</param>
		public static void RenderBackground(dynamic Object)
		{
			if (!Renderer.TexturingEnabled)
			{
				GL.Enable(EnableCap.Texture2D);
				Renderer.TexturingEnabled = true;
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
						Renderer.currentHost.LoadTexture(Object.ObjectBackground.Mesh.Materials[m].DaytimeTexture, wrap);
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
					GL.Color4(Renderer.inv255 * (float)Object.ObjectBackground.Mesh.Materials[m].Color.R * 1.0f, Renderer.inv255 * Object.ObjectBackground.Mesh.Materials[m].Color.G * 1.0f, Renderer.inv255 * (float)Object.ObjectBackground.Mesh.Materials[m].Color.B * 1.0f, Renderer.inv255 * (float)Object.ObjectBackground.Mesh.Materials[m].Color.A);
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
