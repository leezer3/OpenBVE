namespace OpenBveShared
{
	public static partial class Renderer
	{
		public static class Statistics
		{
			/// <summary>The game's current framerate</summary>
			public static double FrameRate = 1.0;
			/// <summary>The total number of OpenGL triangles in the current frame</summary>
			public static int TotalTriangles = 0;
			/// <summary>The total number of OpenGL triangle strips in the current frame</summary>
			public static int TotalTriangleStrip = 0;
			/// <summary>The total number of OpenGL quad strips in the current frame</summary>
			public static int TotalQuadStrip = 0;
			/// <summary>The total number of OpenGL quads in the current frame</summary>
			public static int TotalQuads = 0;
			/// <summary>The total number of OpenGL polygons in the current frame</summary>
			public static int TotalPolygon = 0;
			/// <summary>The total number of static opaque faces in the current frame</summary>
			public static int StaticOpaqueFaceCount = 0;
		}
	}
}
