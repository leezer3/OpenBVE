using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using System.Collections.Generic;
using static System.Windows.Forms.AxHost;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a mesh consisting of a series of vertices, faces and material properties.</summary>
	public class Mesh
	{
		/// <summary>The shared vertices for the mesh</summary>
		public VertexTemplate[] Vertices;
		/// <summary>The shared materials for the mesh</summary>
		public MeshMaterial[] Materials;
		/// <summary>The materials for the mesh</summary>
		public MeshFace[] Faces;
		/// <summary>The bounding box for the mesh</summary>
		/// <remarks>Not currently implemented</remarks>
		public Vector3[] BoundingBox;
		/// <summary>The OpenGL/OpenTK VAO for the mesh</summary>
		public object VAO;
		/// <summary>The OpenGL/OpenTK VAO for the normals</summary>
		public object NormalsVAO;

		/// <summary>Creates a new empty mesh</summary>
		public Mesh()
		{
			Faces = new MeshFace[] { };
			Materials = new MeshMaterial[] { };
			Vertices = new VertexTemplate[] { };
		}

		/// <summary>Creates a mesh consisting of one face, which is represented by individual vertices, and a color.</summary>
		/// <param name="Vertices">The vertices that make up one face.</param>
		/// <param name="Color">The color to be applied on the face.</param>
		public Mesh(VertexTemplate[] Vertices, Color32 Color)
		{
			this.Vertices = Vertices;
			this.Materials = new MeshMaterial[1];
			this.Materials[0].Color = Color;
			this.Faces = new MeshFace[1];
			this.Faces[0].Material = 0;
			this.Faces[0].Vertices = new MeshFaceVertex[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++)
			{
				this.Faces[0].Vertices[i].Index = i;
			}

			this.BoundingBox = new Vector3[2];
			VAO = null;
			NormalsVAO = null;
		}

		/// <summary>Creates a mesh consisting of the specified vertices, faces and color.</summary>
		/// <param name="Vertices">The vertices used.</param>
		/// <param name="FaceVertices">A list of faces represented by a list of references to vertices.</param>
		/// <param name="Color">The color to be applied on all of the faces.</param>
		public Mesh(VertexTemplate[] Vertices, int[][] FaceVertices, Color32 Color)
		{
			this.Vertices = Vertices;
			this.Materials = new MeshMaterial[1];
			this.Materials[0].Color = Color;
			this.Faces = new MeshFace[FaceVertices.Length];
			for (int i = 0; i < FaceVertices.Length; i++)
			{
				this.Faces[i] = new MeshFace(FaceVertices[i]);
			}

			this.BoundingBox = new Vector3[2];
			VAO = null;
			NormalsVAO = null;
		}

		/// <summary>Finalizes the mesh</summary>
		/// <remarks>This generates any missing normals, and sets the texture wrap modes appropriately</remarks>
		public void FinalizeMesh()
		{
			for (int i = 0; i < Faces.Length; i++)
			{
				CreateNormals(i);
			}

			// create list of potential vertices (set backing array to the length of our vertices)
			List<MeshFaceVertex> materialVerts = new List<MeshFaceVertex>(Vertices.Length);

			for (int i = 0; i < Materials.Length; i++)
			{
				if (Materials[i].WrapMode == null)
				{
					/*
					 * If the object does not have a stored wrapping mode, determine it now
					 * https://github.com/leezer3/OpenBVE/issues/971
					 *
					 * Unfortunately, there appear to be X objects in the wild which expect a non-default wrapping mode
					 * which means the best fast exit we can do is to check for RepeatRepeat
					 *
					 * We also need to check for all faces in the mesh using this material
					 * (as otherwise non-contiguous faces mapped to the same texture may not work as expected)
					 */
					for (int j = 0; j < Faces.Length; j++)
					{
						materialVerts.AddRange(Faces[j].Vertices);
					}

					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;

					foreach (MeshFaceVertex vertex in materialVerts)
					{
						if (Vertices[vertex].TextureCoordinates.X < 0.0f || Vertices[vertex].TextureCoordinates.X > 1.0f)
						{
							wrap |= OpenGlTextureWrapMode.RepeatClamp;
						}

						if (Vertices[vertex.Index].TextureCoordinates.Y < 0.0f || Vertices[vertex].TextureCoordinates.Y > 1.0f)
						{
							wrap |= OpenGlTextureWrapMode.ClampRepeat;
						}

						if (wrap == OpenGlTextureWrapMode.RepeatRepeat)
						{
							break;
						}
					}

					Materials[i].WrapMode = wrap;
					materialVerts.Clear();
				}
			}
		}

		/// <summary>Creates the normals for the specified face index</summary>
		private void CreateNormals(int FaceIndex)
		{
			if (Faces[FaceIndex].Vertices.Length >= 3)
			{
				int i0 = Faces[FaceIndex].Vertices[0];
				int i1 = Faces[FaceIndex].Vertices[1];
				int i2 = Faces[FaceIndex].Vertices[2];
				double ax = Vertices[i1].Coordinates.X - Vertices[i0].Coordinates.X;
				double ay = Vertices[i1].Coordinates.Y - Vertices[i0].Coordinates.Y;
				double az = Vertices[i1].Coordinates.Z - Vertices[i0].Coordinates.Z;
				double bx = Vertices[i2].Coordinates.X - Vertices[i0].Coordinates.X;
				double by = Vertices[i2].Coordinates.Y - Vertices[i0].Coordinates.Y;
				double bz = Vertices[i2].Coordinates.Z - Vertices[i0].Coordinates.Z;
				double nx = ay * bz - az * by;
				double ny = az * bx - ax * bz;
				double nz = ax * by - ay * bx;
				double t = nx * nx + ny * ny + nz * nz;
				if (t != 0.0)
				{
					t = 1.0 / System.Math.Sqrt(t);
					float mx = (float) (nx * t);
					float my = (float) (ny * t);
					float mz = (float) (nz * t);
					for (int j = 0; j < Faces[FaceIndex].Vertices.Length; j++)
					{
						if (Vector3.IsZero(Faces[FaceIndex].Vertices[j].Normal))
						{
							Faces[FaceIndex].Vertices[j].Normal = new Vector3(mx, my, mz);
						}
					}
				}
				else
				{
					for (int j = 0; j < Faces[FaceIndex].Vertices.Length; j++)
					{
						if (Vector3.IsZero(Faces[FaceIndex].Vertices[j].Normal))
						{
							Faces[FaceIndex].Vertices[j].Normal = Vector3.Down;
						}
					}
				}
			}
		}

		
	}
}
