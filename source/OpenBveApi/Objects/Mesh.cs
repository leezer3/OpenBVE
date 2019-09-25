using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a mesh consisting of a series of vertices, faces and material properties.</summary>
	public struct Mesh
	{
		/// <summary>The shared verticies for the mesh</summary>
		public VertexTemplate[] Vertices;
		/// <summary>The shared materials for the mesh</summary>
		public MeshMaterial[] Materials;
		/// <summary>The materials for the mesh</summary>
		public MeshFace[] Faces;
		/// <summary>The bounding box for the mesh</summary>
		/// <remarks>Not currently implemented</remarks>
		public Vector3[] BoundingBox;
		/// <summary>The OpenGL/OpenTK VAO for the mesh</summary>
		public VertexArrayObject VAO;
		/// <summary>The OpenGL/OpenTK VAO for the normals</summary>
		public VertexArrayObject NormalsVAO;

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
				this.Faces[0].Vertices[i].Index = (ushort) i;
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
				int i0 = (int) Faces[FaceIndex].Vertices[0].Index;
				int i1 = (int) Faces[FaceIndex].Vertices[1].Index;
				int i2 = (int) Faces[FaceIndex].Vertices[2].Index;
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
							Faces[FaceIndex].Vertices[j].Normal = new Vector3(0.0f, 1.0f, 0.0f);
						}
					}
				}
			}
		}

		/// <summary>
		/// Create an OpenGL/OpenTK VAO for the mesh
		/// </summary>
		/// <param name="isDynamic"></param>
		public void CreateVAO(bool isDynamic)
		{
			var hint = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

			var vertexData = new List<LibRenderVertex>();
			var indexData = new List<int>();

			var normalsVertexData = new List<LibRenderVertex>();
			var normalsIndexData = new List<int>();

			for (int i = 0; i < Faces.Length; i++)
			{
				Faces[i].IboStartIndex = indexData.Count;
				Faces[i].NormalsIboStartIndex = normalsIndexData.Count;

				foreach (var vertex in Faces[i].Vertices)
				{
					var data = new LibRenderVertex
					{
						Position = new OpenTK.Vector3((float)Vertices[vertex.Index].Coordinates.X, (float)Vertices[vertex.Index].Coordinates.Y, (float)-Vertices[vertex.Index].Coordinates.Z),
						Normal = new OpenTK.Vector3((float)vertex.Normal.X, (float)vertex.Normal.Y, (float)-vertex.Normal.Z),
						UV = new OpenTK.Vector2((float)Vertices[vertex.Index].TextureCoordinates.X, (float)Vertices[vertex.Index].TextureCoordinates.Y)
					};

					var coloredVertex = Vertices[vertex.Index] as ColoredVertex;

					if (coloredVertex != null)
					{
						var color = coloredVertex.Color;
						data.Color = new Color4(color.R, color.G, color.B, color.A);
					}
					else
					{
						data.Color = Color4.White;
					}

					vertexData.Add(data);

					var normalsData = new LibRenderVertex[2];
					normalsData[0].Position = data.Position;
					normalsData[1].Position = data.Position + data.Normal;

					for (int j = 0; j < normalsData.Length; j++)
					{
						normalsData[j].Color = Color4.White;
					}

					normalsVertexData.AddRange(normalsData);
				}

				indexData.AddRange(Enumerable.Range(Faces[i].IboStartIndex, Faces[i].Vertices.Length));
				normalsIndexData.AddRange(Enumerable.Range(Faces[i].NormalsIboStartIndex, Faces[i].Vertices.Length * 2));
			}

			VAO?.UnBind();
			VAO?.Dispose();

			VAO = new VertexArrayObject();
			VAO.Bind();
			VAO.SetVBO(new VertexBufferObject(vertexData.ToArray(), hint));
			VAO.SetIBO(new IndexBufferObject(indexData.ToArray(), hint));
			VAO.UnBind();

			NormalsVAO?.UnBind();
			NormalsVAO?.Dispose();

			NormalsVAO = new VertexArrayObject();
			NormalsVAO.Bind();
			NormalsVAO.SetVBO(new VertexBufferObject(normalsVertexData.ToArray(), hint));
			NormalsVAO.SetIBO(new IndexBufferObject(normalsIndexData.ToArray(), hint));
			NormalsVAO.UnBind();
		}
	}
}
