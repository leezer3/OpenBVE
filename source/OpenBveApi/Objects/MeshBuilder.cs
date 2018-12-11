using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBveApi.Objects
{
	/// <summary>The MeshBuilder is a helper class used by parsers when constructing an object</summary>
	public class MeshBuilder
	{
		/// <summary>A reference to the current host application, used for loading textures etc.</summary>
		private readonly Hosts.HostInterface currentHost;
		/// <summary>The verticies within the MeshBuilder</summary>
		public VertexTemplate[] Vertices;
		/// <summary>The faces within the MeshBuilder</summary>
		public MeshFace[] Faces;
		/// <summary>The materials within the MeshBuilder</summary>
		public Material[] Materials;
		/// <summary>The TransformMatrix to be applied</summary>
		public Matrix4D TransformMatrix = Matrix4D.NoTransformation;
		/// <summary>Whether this MeshBuilder is a cylinder</summary>
		/// <remarks>Used by certain compatibility functions</remarks>
		public bool isCylinder;

		/// <summary>Creates a new MeshBuilder</summary>
		public MeshBuilder(Hosts.HostInterface host)
		{
			this.currentHost = host;
			this.Vertices = new VertexTemplate[] {};
			this.Faces = new MeshFace[] { };
			this.Materials = new Material[] {new Material()};
		}

		/// <summary>Applies the data within the MeshBuilder to a StaticObject</summary>
		public void Apply(ref StaticObject Object)
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
				Array.Resize<MeshFace>(ref Object.Mesh.Faces, mf + Faces.Length);
				Array.Resize<MeshMaterial>(ref Object.Mesh.Materials, mm + Materials.Length);
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
					Object.Mesh.Materials[mm + i].Flags = (byte) ((Materials[i].EmissiveColorUsed ? MeshMaterial.EmissiveColorMask : 0) | (Materials[i].TransparentColorUsed ? MeshMaterial.TransparentColorMask : 0));
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
							currentHost.RegisterTexture(texture, new TextureParameters(null, new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G, Materials[i].TransparentColor.B)), out tday);
						}
						else
						{
							if (Materials[i].TransparentColorUsed)
							{
								currentHost.RegisterTexture(Materials[i].DaytimeTexture,
									new TextureParameters(null,
										new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G,
											Materials[i].TransparentColor.B)), out tday);
							}
							else
							{
								currentHost.RegisterTexture(Materials[i].DaytimeTexture, new TextureParameters(null, null), out tday);
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
							currentHost.RegisterTexture(Materials[i].NighttimeTexture, new TextureParameters(null, new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G, Materials[i].TransparentColor.B)), out tnight);
						}
						else
						{
							currentHost.RegisterTexture(Materials[i].NighttimeTexture, new TextureParameters(null, null), out tnight);
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

	/// <summary>A material to be applied to a face within a MeshBuilder</summary>
	public class Material {
		/// <summary>The base color</summary>
		public Color32 Color;
		/// <summary>The emissive color</summary>
		public Color24 EmissiveColor;
		/// <summary>Whether the emissive color is used</summary>
		public bool EmissiveColorUsed;
		/// <summary>The transparent color used for the texture</summary>
		public Color24 TransparentColor;
		/// <summary>Whether the transparent color has been used</summary>
		public bool TransparentColorUsed;
		/// <summary>The daytime texture</summary>
		public string DaytimeTexture;
		/// <summary>The nighttime texture</summary>
		public string NighttimeTexture;
		/// <summary>The alpha texture to be applied to both the daytime and the nighttime texture</summary>
		public string TransparencyTexture;
		/// <summary>The blend mode to be used</summary>
		public MeshMaterialBlendMode BlendMode;
		/// <summary>The wrap mode to be used OR null if the renderer should determine this at runtime</summary>
		public OpenGlTextureWrapMode? WrapMode;
		/// <summary>The glow attenuation data (if used)</summary>
		public ushort GlowAttenuationData;
		/// <summary>The text to be overlaid onto the texture</summary>
		public string Text;
		/// <summary>The color of the text</summary>
		public Color TextColor;
		/// <summary>The background color for the text</summary>
		public Color BackgroundColor;
		/// <summary>The font to be used for the text</summary>
		public string Font;
		/// <summary>The text padding to be applied</summary>
		public Vector2 TextPadding;

		/// <summary>Creates a new material</summary>
		public Material() {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.EmissiveColorUsed = false;
			this.TransparentColor = Color24.Black;
			this.TransparentColorUsed = false;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = System.Drawing.Color.Black;
			this.BackgroundColor = System.Drawing.Color.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
		}
		/// <summary>Creates a new material with the specified texture</summary>
		public Material(string Texture) {
			this.Color = Color32.White;
			this.EmissiveColor = Color24.Black;
			this.EmissiveColorUsed = false;
			this.TransparentColor = Color24.Black;
			this.TransparentColorUsed = false;
			this.DaytimeTexture = null;
			this.NighttimeTexture = null;
			this.BlendMode = MeshMaterialBlendMode.Normal;
			this.GlowAttenuationData = 0;
			this.TextColor = System.Drawing.Color.Black;
			this.BackgroundColor = System.Drawing.Color.White;
			this.TextPadding = new Vector2(0, 0);
			this.Font = "Arial";
			this.WrapMode = null;
			this.DaytimeTexture = Texture;
		}
		/// <summary>Clones a material from a prototype material</summary>
		public Material(Material Prototype) {
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
