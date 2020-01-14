using System.Runtime.InteropServices;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace LibRender2
{
	/// <summary>Vertex structure</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct LibRenderVertex
	{
		/// <summary>Vertex coordinates</summary>
		public Vector3f Position;

		/// <summary>
		/// Vertex normals
		/// </summary>
		public Vector3f Normal;

		/// <summary>Vertex texture coordinates</summary>
		public Vector2f UV;

		/// <summary>Vertex color</summary>
		public Color128 Color;

		/// <summary>Size in bytes</summary>
		public static int SizeInBytes => Marshal.SizeOf(typeof(LibRenderVertex));
	}
}
