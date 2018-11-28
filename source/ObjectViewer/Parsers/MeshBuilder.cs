using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace OpenBve
{
	internal class MeshBuilder
	{
		internal VertexTemplate[] Vertices;
		internal World.MeshFace[] Faces;
		internal Material[] Materials;
		internal Matrix4D TransformMatrix = Matrix4D.NoTransformation;

		internal MeshBuilder()
		{
			this.Vertices = new VertexTemplate[] {};
			this.Faces = new World.MeshFace[] { };
			this.Materials = new Material[] {new Material()};
		}

		internal void Apply(ref ObjectManager.StaticObject Object)
		{
			if (TransformMatrix != Matrix4D.NoTransformation)
			{
				for (int i = 0; i < Vertices.Length; i++)
				{
					Vertices[i].Coordinates.Transform(TransformMatrix);
				}
			}
			if (Faces.Length != 0)
			{
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Materials.Length);
				Array.Resize<VertexTemplate>(ref Object.Mesh.Vertices, mv + Vertices.Length);
				for (int i = 0; i < Vertices.Length; i++)
				{
					Object.Mesh.Vertices[mv + i] = Vertices[i];
				}

				for (int i = 0; i < Faces.Length; i++)
				{
					Object.Mesh.Faces[mf + i] = Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++)
					{
						Object.Mesh.Faces[mf + i].Vertices[j].Index += (ushort) mv;
					}

					Object.Mesh.Faces[mf + i].Material += (ushort) mm;
				}

				for (int i = 0; i < Materials.Length; i++)
				{
					Object.Mesh.Materials[mm + i].Flags = (byte) ((Materials[i].EmissiveColorUsed ? World.MeshMaterial.EmissiveColorMask : 0) | (Materials[i].TransparentColorUsed ? World.MeshMaterial.TransparentColorMask : 0));
					Object.Mesh.Materials[mm + i].Color = Materials[i].Color;
					Object.Mesh.Materials[mm + i].TransparentColor = Materials[i].TransparentColor;
					if (Materials[i].DaytimeTexture != null || Materials[i].Text != null)
					{
						Texture tday;
						if (Materials[i].Text != null)
						{
							Bitmap bitmap = null;
							if (Materials[i].DaytimeTexture != null)
							{
								bitmap = new Bitmap(Materials[i].DaytimeTexture);
							}

							Bitmap texture = TextOverlay.AddTextToBitmap(bitmap, Materials[i].Text, Materials[i].Font, 12, Materials[i].BackgroundColor, Materials[i].TextColor, Materials[i].TextPadding);
							tday = Textures.RegisterTexture(texture, new TextureParameters(null, new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G, Materials[i].TransparentColor.B)));
						}
						else
						{
							if (Materials[i].TransparentColorUsed)
							{
								Textures.RegisterTexture(Materials[i].DaytimeTexture,
									new TextureParameters(null,
										new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G,
											Materials[i].TransparentColor.B)), out tday);
							}
							else
							{
								Textures.RegisterTexture(Materials[i].DaytimeTexture, out tday);
							}
						}

						Object.Mesh.Materials[mm + i].DaytimeTexture = tday;
					}
					else
					{
						Object.Mesh.Materials[mm + i].DaytimeTexture = null;
					}

					Object.Mesh.Materials[mm + i].EmissiveColor = Materials[i].EmissiveColor;
					if (Materials[i].NighttimeTexture != null)
					{
						Texture tnight;
						if (Materials[i].TransparentColorUsed)
						{
							Textures.RegisterTexture(Materials[i].NighttimeTexture, new TextureParameters(null, new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G, Materials[i].TransparentColor.B)), out tnight);
						}
						else
						{
							Textures.RegisterTexture(Materials[i].NighttimeTexture, out tnight);
						}

						Object.Mesh.Materials[mm + i].NighttimeTexture = tnight;
					}
					else
					{
						Object.Mesh.Materials[mm + i].NighttimeTexture = null;
					}

					Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
					Object.Mesh.Materials[mm + i].BlendMode = Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Materials[i].GlowAttenuationData;
					Object.Mesh.Materials[mm + i].WrapMode = Materials[i].WrapMode;
				}
			}
		}

	}

	internal class Material {
		internal Color32 Color;
		internal Color24 EmissiveColor;
		internal bool EmissiveColorUsed;
		internal Color24 TransparentColor;
		internal bool TransparentColorUsed;
		internal string DaytimeTexture;
		internal string NighttimeTexture;
		internal string TransparencyTexture;
		internal World.MeshMaterialBlendMode BlendMode;
		internal OpenGlTextureWrapMode? WrapMode;
		internal ushort GlowAttenuationData;
		internal string Text;
		internal Color TextColor;
		internal Color BackgroundColor;
		internal string Font;
		internal Vector2 TextPadding; 
		internal Material() {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.EmissiveColorUsed = false;
			this.TransparentColor = Color24.Black;
			this.TransparentColorUsed = false;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = World.MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = System.Drawing.Color.Black;
			this.BackgroundColor = System.Drawing.Color.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
		}
		internal Material(string Texture) {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.EmissiveColorUsed = false;
			this.TransparentColor = Color24.Black;
			this.TransparentColorUsed = false;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = World.MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = System.Drawing.Color.Black;
			this.BackgroundColor = System.Drawing.Color.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
			this.DaytimeTexture = Texture;
		}
		internal Material(Material Prototype) {
			this.Color = Prototype.Color;
			this.EmissiveColor = Prototype.EmissiveColor;
			this.EmissiveColorUsed = Prototype.EmissiveColorUsed;
			this.TransparentColor = Prototype.TransparentColor;
			this.TransparentColorUsed = Prototype.TransparentColorUsed;
			this.DaytimeTexture = Prototype.DaytimeTexture;
			this.NighttimeTexture = Prototype.NighttimeTexture;
			this.BlendMode = Prototype.BlendMode;
			this.GlowAttenuationData = Prototype.GlowAttenuationData;
			this.TextColor = Prototype.TextColor;
			this.BackgroundColor = Prototype.BackgroundColor;
			this.TextPadding = Prototype.TextPadding;
			this.Font = Prototype.Font;
			this.WrapMode = Prototype.WrapMode;
		}
	}
}
