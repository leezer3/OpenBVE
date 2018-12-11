using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBveShared
{
	public static partial class Renderer
	{

		/// <summary>Renders a face using it's default material & properties</summary>
		/// <param name="Face">The face</param>
		/// <param name="Camera">The camera position</param>
		public static void RenderFace(ref ObjectFace Face, Vector3 Camera)
		{
			if (CullEnabled)
			{
				if (!OptionBackfaceCulling || (GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
				{
					GL.Disable(EnableCap.CullFace);
					CullEnabled = false;
				}
			}
			else if (OptionBackfaceCulling)
			{
				if ((GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
					CullEnabled = true;
				}
			}
			int r = (int)GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			RenderFace(ref GameObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], GameObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], Camera);
		}

		/// <summary>Renders a face using the specified material and properties</summary>
		/// <param name="Material">The material to apply to the face</param>
		/// <param name="Vertices">The verticies for the face</param>
		/// <param name="wrap">The texture wrap mode for the face</param>
		/// <param name="Face">The actual face data</param>
		/// <param name="Camera">The camera position</param>
		public static void RenderFace(ref MeshMaterial Material, VertexTemplate[] Vertices, OpenGlTextureWrapMode wrap, ref MeshFace Face, Vector3 Camera)
		{
			// texture
			if (Material.DaytimeTexture != null)
			{
				if (currentHost.LoadTexture(Material.DaytimeTexture, wrap))
				{
					if (!TexturingEnabled)
					{
						GL.Enable(EnableCap.Texture2D);
						TexturingEnabled = true;
					}
					if (Material.DaytimeTexture.OpenGlTextures[(int)wrap] != LastBoundTexture)
					{
						GL.BindTexture(TextureTarget.Texture2D, Material.DaytimeTexture.OpenGlTextures[(int)wrap].Name);
						LastBoundTexture = Material.DaytimeTexture.OpenGlTextures[(int)wrap];
					}
				}
				else
				{
					if (TexturingEnabled)
					{
						GL.Disable(EnableCap.Texture2D);
						TexturingEnabled = false;
						LastBoundTexture = null;
					}
				}
			}
			else
			{
				if (TexturingEnabled)
				{
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
					LastBoundTexture = null;
				}
			}
			// blend mode
			float factor;
			if (Material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				factor = 1.0f;
				if (!BlendEnabled) GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
				if (FogEnabled)
				{
					GL.Disable(EnableCap.Fog);
				}
			}
			else if (Material.NighttimeTexture == null)
			{
				float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
				if (blend > 1.0f) blend = 1.0f;
				factor = 1.0f - 0.7f * blend;
			}
			else
			{
				factor = 1.0f;
			}
			if (Material.NighttimeTexture != null)
			{
				if (LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					LightingEnabled = false;
				}
			}
			else
			{
				if (OptionLighting & !LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting);
					LightingEnabled = true;
				}
			}
			// render daytime polygon
			int FaceType = Face.Flags & MeshFace.FaceTypeMask;
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
			if (Material.GlowAttenuationData != 0)
			{
				float alphafactor = (float)Glow.GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, Camera);
				if (OptionWireframe)
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
				}
			}
			else
			{
				if (OptionWireframe)
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A);
				}
				
			}
			if ((Material.Flags & MeshMaterial.EmissiveColorMask) != 0)
			{
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
			}
			else
			{
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
			}
			if (Material.DaytimeTexture != null)
			{
				if (LightingEnabled)
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						GL.Normal3(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
				else
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
			}
			else
			{
				if (LightingEnabled)
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						GL.Normal3(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
				else
				{
					for (int j = 0; j < Face.Vertices.Length; j++)
					{
						if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
						{
							ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
							GL.Color3(v.Color.R, v.Color.G, v.Color.B);
						}
						GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					}
				}
			}
			GL.End();
			// render nighttime polygon
			if (Material.NighttimeTexture != null && currentHost.LoadTexture(Material.NighttimeTexture, wrap))
			{
				if (!TexturingEnabled)
				{
					GL.Enable(EnableCap.Texture2D);
					TexturingEnabled = true;
				}
				if (!BlendEnabled)
				{
					GL.Enable(EnableCap.Blend);
				}
				GL.BindTexture(TextureTarget.Texture2D, Material.NighttimeTexture.OpenGlTextures[(int)wrap].Name);
				LastBoundTexture = null;
				GL.AlphaFunc(AlphaFunction.Greater, 0.0f);
				GL.Enable(EnableCap.AlphaTest);
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
				float alphafactor;
				if (Material.GlowAttenuationData != 0)
				{
					alphafactor = (float)Glow.GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, Camera);
					float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (blend > 1.0f) blend = 1.0f;
					alphafactor *= blend;
				}
				else
				{
					alphafactor = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (alphafactor > 1.0f) alphafactor = 1.0f;
				}
				if (OptionWireframe)
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
				}
				
				if ((Material.Flags & MeshMaterial.EmissiveColorMask) != 0)
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
				}
				else
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				}
				for (int j = 0; j < Face.Vertices.Length; j++)
				{
					GL.TexCoord2(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
					if (Vertices[Face.Vertices[j].Index] is ColoredVertex)
					{
						ColoredVertex v = (ColoredVertex) Vertices[Face.Vertices[j].Index];
						GL.Color3(v.Color.R, v.Color.G, v.Color.B);
					}
					GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
				}
				GL.End();
				RestoreAlphaFunc();
				if (!BlendEnabled)
				{
					GL.Disable(EnableCap.Blend);
				}
			}
			// normals
			if (OptionNormals)
			{
				if (TexturingEnabled)
				{
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
				}
				for (int j = 0; j < Face.Vertices.Length; j++)
				{
					GL.Begin(PrimitiveType.Lines);
					GL.Color4(inv255 * (float)Material.Color.R, inv255 * (float)Material.Color.G, inv255 * (float)Material.Color.B, 1.0f);
					GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - Camera.Z));
					GL.Vertex3((float)(Vertices[Face.Vertices[j].Index].Coordinates.X + Face.Vertices[j].Normal.X - Camera.X), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y + Face.Vertices[j].Normal.Y - Camera.Y), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z + Face.Vertices[j].Normal.Z - Camera.Z));
					GL.End();
				}
			}
			// finalize
			if (Material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				if (!BlendEnabled) GL.Disable(EnableCap.Blend);
				if (FogEnabled)
				{
					GL.Enable(EnableCap.Fog);
				}
			}
		}
	}
}
