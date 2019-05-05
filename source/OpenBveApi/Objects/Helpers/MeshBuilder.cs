using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBveApi.Objects
{
	/// <summary>The MeshBuilder is a helper class used when creating a 3D model from a textual object file</summary>
	public class MeshBuilder
	{
		/// <summary>The vertices</summary>
		public VertexTemplate[] Vertices;
		/// <summary>The faces</summary>
		public MeshFace[] Faces;
		/// <summary>The materials present</summary>
		public Material[] Materials;
		/// <summary>The transform matric to be applied</summary>
		public Matrix4D TransformMatrix = Matrix4D.NoTransformation;
		/// <summary>
		/// Whether this was created using the Cylinder command in a B3D / CSV</summary>
		/// <remarks>Used to compensate for some BVE behaviour</remarks>
		public bool isCylinder;

		private readonly Hosts.HostInterface currentHost;

		/// <summary>Creates a new MeshBuilder</summary>
		/// <param name="Host">The callback to the host application allowing for texture loading etc.</param>
		public MeshBuilder(Hosts.HostInterface Host)
		{
			this.currentHost = Host;
			this.Vertices = new VertexTemplate[] {};
			this.Faces = new MeshFace[] { };
			this.Materials = new Material[] {new Material()};
			this.isCylinder = false;
		}

		/// <summary>Applies the MeshBuilder's data to a StaticObject</summary>
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
						Textures.Texture tday;
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
								currentHost.RegisterTexture(Materials[i].DaytimeTexture, new TextureParameters(null,
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
						Textures.Texture tnight;
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
}
