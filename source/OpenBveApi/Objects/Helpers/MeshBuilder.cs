using System;
using System.Collections.Generic;
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
		public List<VertexTemplate> Vertices;
		/// <summary>The faces</summary>
		public List<MeshFace> Faces;
		/// <summary>The materials present</summary>
		public Material[] Materials;
		/// <summary>The transform matrix to be applied</summary>
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
			this.Vertices = new List<VertexTemplate>();
			this.Faces = new List<MeshFace>();
			this.Materials = new[] {new Material()};
			this.isCylinder = false;
		}

		/// <summary>Applies the MeshBuilder's data to a StaticObject</summary>
		public void Apply(ref StaticObject Object, bool EnableHacks = false, bool IgnoreW = true)
		{
			if (TransformMatrix != Matrix4D.NoTransformation)
			{
				for (int i = 0; i < Vertices.Count; i++)
				{
					Vertices[i].Coordinates.Transform(TransformMatrix, IgnoreW);
				}
			}
			if (Faces.Count != 0)
			{
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize(ref Object.Mesh.Faces, mf + Faces.Count);
				Array.Resize(ref Object.Mesh.Materials, mm + Materials.Length);
				Array.Resize(ref Object.Mesh.Vertices, mv + Vertices.Count);
				Vertices.CopyTo(0, Object.Mesh.Vertices, mv, Vertices.Count);

				for (int i = 0; i < Faces.Count; i++)
				{
					Object.Mesh.Faces[mf + i] = Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++)
					{
						Object.Mesh.Faces[mf + i].Vertices[j].Index += mv;
					}

					Object.Mesh.Faces[mf + i].Material += (ushort) mm;
				}

				for (int i = 0; i < Materials.Length; i++)
				{
					if (EnableHacks && !string.IsNullOrEmpty(Materials[i].DaytimeTexture))
					{
						if (!string.IsNullOrEmpty(Materials[i].NighttimeTexture))
						{
							/*
							 * Versions of openBVE prior to 1.7.0 rendered polygons with two defined textures as unlit
							 * The new GL 3.2 renderer corrects this behaviour
							 * Horrid workaround....
							 */
							Materials[i].Flags |= MaterialFlags.DisableLighting;
						}
					}

					Object.Mesh.Materials[mm + i].Flags = Materials[i].Flags;
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
							if ((Materials[i].Flags & MaterialFlags.TransparentColor) != 0)
							{
								currentHost.RegisterTexture(Materials[i].DaytimeTexture, new TextureParameters(null,
										new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G,
											Materials[i].TransparentColor.B)), out tday);
							}
							else
							{
								currentHost.RegisterTexture(Materials[i].DaytimeTexture, TextureParameters.NoChange, out tday);
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
						if ((Materials[i].Flags & MaterialFlags.TransparentColor) != 0)
						{
							currentHost.RegisterTexture(Materials[i].NighttimeTexture, new TextureParameters(null, new Color24(Materials[i].TransparentColor.R, Materials[i].TransparentColor.G, Materials[i].TransparentColor.B)), out tnight);
						}
						else
						{
							currentHost.RegisterTexture(Materials[i].NighttimeTexture, TextureParameters.NoChange, out tnight);
						}

						Object.Mesh.Materials[mm + i].NighttimeTexture = tnight;
					}
					else
					{
						Object.Mesh.Materials[mm + i].NighttimeTexture = null;
					}

					if (Materials[i].LightMap != null)
					{
						currentHost.RegisterTexture(Materials[i].LightMap, new TextureParameters(null, Color24.White), out Texture lightMap);
						Object.Mesh.Materials[mm + i].LightMapTexture = lightMap;
					}

					Object.Mesh.Materials[mm + i].BlendMode = Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Materials[i].GlowAttenuationData;
					Object.Mesh.Materials[mm + i].WrapMode = Materials[i].WrapMode;
				}
			}
		}

		/// <summary>Translates the MeshBuilder by the given values</summary>
		public void ApplyTranslation(double x, double y, double z)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vertices[i].Coordinates.X += x;
				Vertices[i].Coordinates.Y += y;
				Vertices[i].Coordinates.Z += z;
			}
		}

		/// <summary>Scales the MeshBuilder by the given value</summary>
		public void ApplyScale(Vector3 scale)
		{
			ApplyScale(scale.X, scale.Y, scale.Z);
		}

		/// <summary>Scales the MeshBuilder by the given values</summary>
		public void ApplyScale(double x, double y, double z)
		{
			float rx = (float) (1.0 / x);
			float ry = (float) (1.0 / y);
			float rz = (float) (1.0 / z);
			float rx2 = rx * rx;
			float ry2 = ry * ry;
			float rz2 = rz * rz;
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vertices[i].Coordinates.X *= x;
				Vertices[i].Coordinates.Y *= y;
				Vertices[i].Coordinates.Z *= z;
			}

			for (int i = 0; i < Faces.Count; i++)
			{
				for (int j = 0; j < Faces[i].Vertices.Length; j++)
				{
					double nx2 = Faces[i].Vertices[j].Normal.X * Faces[i].Vertices[j].Normal.X;
					double ny2 = Faces[i].Vertices[j].Normal.Y * Faces[i].Vertices[j].Normal.Y;
					double nz2 = Faces[i].Vertices[j].Normal.Z * Faces[i].Vertices[j].Normal.Z;
					double u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
					if (u != 0.0)
					{
						u = (float) System.Math.Sqrt((nx2 + ny2 + nz2) / u);
						Faces[i].Vertices[j].Normal.X *= rx * u;
						Faces[i].Vertices[j].Normal.Y *= ry * u;
						Faces[i].Vertices[j].Normal.Z *= rz * u;
					}
				}
			}

			if (x * y * z < 0.0)
			{
				for (int i = 0; i < Faces.Count; i++)
				{
					Faces[i].Flip();
				}
			}
		}

		/// <summary>Rotates the MeshBuilder along the Rotation vector using the given angle</summary>
		public void ApplyRotation(Vector3 Rotation, double Angle)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vertices[i].Coordinates.Rotate(Rotation, Angle);
			}

			for (int i = 0; i < Faces.Count; i++)
			{
				for (int j = 0; j < Faces[i].Vertices.Length; j++)
				{
					Faces[i].Vertices[j].Normal.Rotate(Rotation, Angle);
				}
			}
		}

		/// <summary>Mirrors the MeshBuilder using the given parameters</summary>
		public void ApplyMirror(bool vX, bool vY, bool vZ, bool nX, bool nY, bool nZ)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (vX)
				{
					Vertices[i].Coordinates.X *= -1;
				}
				if (vY)
				{
					Vertices[i].Coordinates.Y *= -1;
				}
				if (vZ)
				{
					Vertices[i].Coordinates.Z *= -1;
				}
			}
			for (int i = 0; i < Faces.Count; i++)
			{
				for (int j = 0; j < Faces[i].Vertices.Length; j++)
				{
					if (nX)
					{
						Faces[i].Vertices[j].Normal.X *= -1;
					}
					if (nY)
					{
						Faces[i].Vertices[j].Normal.Y *= -1;
					}
					if (nZ)
					{
						Faces[i].Vertices[j].Normal.X *= -1;
					}
				}
			}
			int numFlips = 0;
			if (vX)
			{
				numFlips++;
			}
			if (vY)
			{
				numFlips++;
			}
			if (vZ)
			{
				numFlips++;
			}

			if (numFlips % 2 != 0)
			{
				for (int i = 0; i < Faces.Count; i++)
				{
					Array.Reverse(Faces[i].Vertices);
				}
			}
		}

		/// <summary>Shears the MeshBuilder along the given vectors</summary>
		public void ApplyShear(Vector3 d, Vector3 s, double r)
		{
			for (int j = 0; j < Vertices.Count; j++)
			{
				double n = r * (d.X * Vertices[j].Coordinates.X + d.Y * Vertices[j].Coordinates.Y + d.Z * Vertices[j].Coordinates.Z);
				Vertices[j].Coordinates.X += s.X * n;
				Vertices[j].Coordinates.Y += s.Y * n;
				Vertices[j].Coordinates.Z += s.Z * n;
			}

			for (int j = 0; j < Faces.Count; j++)
			{
				for (int k = 0; k < Faces[j].Vertices.Length; k++)
				{
					if (Faces[j].Vertices[k].Normal.X != 0.0f | Faces[j].Vertices[k].Normal.Y != 0.0f | Faces[j].Vertices[k].Normal.Z != 0.0f)
					{
						double n = r * (s.X * Faces[j].Vertices[k].Normal.X + s.Y * Faces[j].Vertices[k].Normal.Y + s.Z * Faces[j].Vertices[k].Normal.Z);
						Faces[j].Vertices[k].Normal -= d * n;
						Faces[j].Vertices[k].Normal.Normalize();
					}
				}
			}
		}

		/// <summary>Applies a color to all materials in the MeshBuilder</summary>
		public void ApplyColor(Color32 color, bool emissive) {
			for (int i = 0; i < Materials.Length; i++) {
				if (emissive) {
					Materials[i].EmissiveColor = (Color24)color;
					Materials[i].Flags |= MaterialFlags.Emissive;
				} else {
					Materials[i].Color = color;
				}
			}
		}
	}
}
