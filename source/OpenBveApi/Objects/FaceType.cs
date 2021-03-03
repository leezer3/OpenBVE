using System;

namespace OpenBveApi.Objects
{
	/// <summary>Describes the different OpenGL face drawing types</summary>
	[Flags]
	public enum FaceFlags : byte
	{
		/// <summary>Polygon</summary>
		Polygon = 0,
		/// <summary>Triangles</summary>
		Triangles = 1,
		/// <summary>TriangleStrip</summary>
		TriangleStrip = 2,
		/// <summary>Quads</summary>
		Quads = 3,
		/// <summary>QuadStrip</summary>
		QuadStrip = 4,
		/// <summary>NotSet</summary>
		/// <remarks>The FaceType will be calculated when the mesh is loaded by OpenGL (used internally, not as a flag)</remarks>
		NotSet = 5,
		/// <summary>The mask used for unidirectonal Face commands</summary>
		FaceTypeMask = 7,
		/// <summary>The mask used for bidirectional Face2 commands</summary>
		Face2Mask = 8
	}
}
