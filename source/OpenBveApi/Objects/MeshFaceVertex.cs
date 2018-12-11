#pragma warning disable 0660, 0661
using System;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a reference to a vertex and the normal to be used for that vertex.</summary>
	public struct MeshFaceVertex
	{
		/// <summary>A reference to an element in the Vertex array of the contained Mesh structure.</summary>
		[CLSCompliant(false)]
		public ushort Index;
		/// <summary>The normal to be used at the vertex.</summary>
		public Vector3 Normal;

		/// <summary>Creates a new MeshFaceVertex with the default normal vector</summary>
		/// <param name="Index">The index to the vertex</param>
		public MeshFaceVertex(int Index)
		{
			this.Index = (ushort) Index;
			this.Normal = new Vector3(0.0f, 0.0f, 0.0f);
		}

		/// <summary>Creates a new MeshFaceVertex with the specified normal vector</summary>
		/// <param name="Index">The index to the vertex</param>
		/// <param name="Normal">The normal to use</param>
		public MeshFaceVertex(int Index, Vector3 Normal)
		{
			this.Index = (ushort) Index;
			this.Normal = Normal;
		}

		/// <summary>Determines whether two MeshFaceVertex are equal</summary>
		public static bool operator ==(MeshFaceVertex A, MeshFaceVertex B)
		{
			if (A.Index != B.Index) return false;
			if (A.Normal.X != B.Normal.X) return false;
			if (A.Normal.Y != B.Normal.Y) return false;
			if (A.Normal.Z != B.Normal.Z) return false;
			return true;
		}

		/// <summary>Determines whether two MeshFaceVertex are unequal</summary>
		public static bool operator !=(MeshFaceVertex A, MeshFaceVertex B)
		{
			if (A.Index != B.Index) return true;
			if (A.Normal.X != B.Normal.X) return true;
			if (A.Normal.Y != B.Normal.Y) return true;
			if (A.Normal.Z != B.Normal.Z) return true;
			return false;
		}
	}
}
