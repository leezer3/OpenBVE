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
		public FaceFlags Flags;
		/// <summary>The starting position for the face in the VAO</summary>
		public int IboStartIndex;
		/// <summary>The starting position for the normals in the VAO</summary>
		public int NormalsIboStartIndex;

		/// <summary>Returns a representation of the face in string format</summary>
		public override string ToString()
		{
			string s = string.Empty;
			for (int i = 0; i < Vertices.Length; i++)
			{
				s += Vertices[i].Index + ",";
			}
			return s;
		}

		/// <summary>Creates a new MeshFace using the specified vertex indicies and the default material</summary>
		/// <param name="Vertices">The vertex indicies</param>
		/// <param name="Type">The type of OpenGL face drawing to use</param>
		public MeshFace(int[] Vertices, FaceFlags Type = FaceFlags.NotSet)
		{
			this.Vertices = new MeshFaceVertex[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++)
			{
				this.Vertices[i] = new MeshFaceVertex(Vertices[i]);
			}

			this.Material = 0;
			this.Flags = 0;
			IboStartIndex = 0;
			NormalsIboStartIndex = 0;
			if (Type != FaceFlags.NotSet)
			{
				Flags |= Type;
			}
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
			IboStartIndex = 0;
			NormalsIboStartIndex = 0;
		}

		/// <summary>Flips the MeshFace</summary>
		public void Flip()
		{
			if (((FaceFlags)Flags & FaceFlags.FaceTypeMask) == FaceFlags.QuadStrip)
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

		
	}
}
