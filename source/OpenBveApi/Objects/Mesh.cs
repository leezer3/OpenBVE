using OpenBveApi.Colors;
using OpenBveApi.Math;

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
		/// <summary>The radius of the bounding sphere</summary>
		public double BoundingSphereRadius;

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

		/// <summary>Creates the normals for all faces within this mesh</summary>
		public void CreateNormals()
		{
			for (int i = 0; i < Faces.Length; i++)
			{
				CreateNormals(i);
			}
		}

		/// <summary>Creates the normals for the specified face index</summary>
		private void CreateNormals(int FaceIndex)
		{
			if (Faces[FaceIndex].Vertices.Length >= 3)
			{
				int i0 = Faces[FaceIndex].Vertices[0].Index;
				int i1 = Faces[FaceIndex].Vertices[1].Index;
				int i2 = Faces[FaceIndex].Vertices[2].Index;
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
		/// <summary>Creates the bounding box and bounding sphere for this mesh</summary>
		public void CreateBoundingBox()
		{
			if (Vertices.Length == 0)
			{
				BoundingBox = new[] { Vector3.Zero, Vector3.Zero };
				BoundingSphereRadius = 0;
				return;
			}

			Vector3 min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
			Vector3 max = new Vector3(double.MinValue, double.MinValue, double.MinValue);

			for (int i = 0; i < Vertices.Length; i++)
			{
				if (Vertices[i].Coordinates.X < min.X) min.X = Vertices[i].Coordinates.X;
				if (Vertices[i].Coordinates.Y < min.Y) min.Y = Vertices[i].Coordinates.Y;
				if (Vertices[i].Coordinates.Z < min.Z) min.Z = Vertices[i].Coordinates.Z;

				if (Vertices[i].Coordinates.X > max.X) max.X = Vertices[i].Coordinates.X;
				if (Vertices[i].Coordinates.Y > max.Y) max.Y = Vertices[i].Coordinates.Y;
				if (Vertices[i].Coordinates.Z > max.Z) max.Z = Vertices[i].Coordinates.Z;
			}

			BoundingBox = new[] { min, max };
			
			// Compute bounding sphere radius (from center of box)
			Vector3 center = (min + max) * 0.5;
			double maxRadiusSq = 0;
			for (int i = 0; i < Vertices.Length; i++)
			{
				double distSq = (Vertices[i].Coordinates - center).NormSquared();
				if (distSq > maxRadiusSq) maxRadiusSq = distSq;
			}
			BoundingSphereRadius = System.Math.Sqrt(maxRadiusSq) + center.Norm(); // Add center norm for radius from origin if needed, but standard radius is from center
			// Actually, OpenBVE uses origin as anchor usually. Let's use max distance from origin for simplest culling.
			double maxDistFromOriginSq = 0;
			for (int i = 0; i < Vertices.Length; i++)
			{
				double distSq = Vertices[i].Coordinates.NormSquared();
				if (distSq > maxDistFromOriginSq) maxDistFromOriginSq = distSq;
			}
			BoundingSphereRadius = System.Math.Sqrt(maxDistFromOriginSq);
		}
	}
}
