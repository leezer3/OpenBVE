using System;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a face consisting of vertices and material attributes.</summary>
	public struct MeshFace
	{
		/// <summary>The array of verticies making up this face</summary>
		public MeshFaceVertex[] Vertices;
		/// <summary>A reference to an element in the Material array of the containing Mesh structure.</summary>
		[CLSCompliant(false)]
		public ushort Material;
		/// <summary>A bit mask combining constants of the MeshFace structure.</summary>
		public byte Flags;

		/// <summary>Creates a new MeshFace using the specified vertex indicies and the default material</summary>
		/// <param name="Vertices">The vertex indicies</param>
		public MeshFace(int[] Vertices)
		{
			this.Vertices = new MeshFaceVertex[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++)
			{
				this.Vertices[i] = new MeshFaceVertex(Vertices[i]);
			}

			this.Material = 0;
			this.Flags = 0;
		}

		/// <summary>Creates a new MeshFace using the specified vertex indices and material</summary>
		/// <param name="verticies">The vertex indicies</param>
		/// <param name="material">The material</param>
		[CLSCompliant(false)]
		public MeshFace(MeshFaceVertex[] verticies, ushort material)
		{
			this.Vertices = verticies;
			this.Material = material;
			this.Flags = 0;
		}

		/// <summary>Flips the MeshFace</summary>
		public void Flip()
		{
			if ((this.Flags & FaceTypeMask) == FaceTypeQuadStrip)
			{
				for (int i = 0; i < this.Vertices.Length; i += 2)
				{
					MeshFaceVertex x = this.Vertices[i];
					this.Vertices[i] = this.Vertices[i + 1];
					this.Vertices[i + 1] = x;
				}
			}
			else
			{
				int n = this.Vertices.Length;
				for (int i = 0; i < (n >> 1); i++)
				{
					MeshFaceVertex x = this.Vertices[i];
					this.Vertices[i] = this.Vertices[n - i - 1];
					this.Vertices[n - i - 1] = x;
				}
			}
		}

		/// <summary>The mask used for unidirectonal Face commands</summary>
		public const int FaceTypeMask = 7;
		/// <summary>The mask used for bidirectional Face2 commands</summary>
		public const int Face2Mask = 8;

		//openGL types

		/// <summary>Polygon</summary>
		public const int FaceTypePolygon = 0;
		/// <summary>Triangles</summary>
		public const int FaceTypeTriangles = 1;
		/// <summary>TriangleStrip</summary>
		public const int FaceTypeTriangleStrip = 2;
		/// <summary>Quads</summary>
		public const int FaceTypeQuads = 3;
		/// <summary>QuadStrip</summary>
		public const int FaceTypeQuadStrip = 4;
		
	}
}
