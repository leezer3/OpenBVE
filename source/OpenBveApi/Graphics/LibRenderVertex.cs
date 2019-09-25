using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;

namespace OpenBveApi.Graphics
{
	/// <summary>
	/// Vertex structure
	/// </summary>
	public struct LibRenderVertex
	{
		/// <summary>
		/// Vertex coordinates
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// Vertex normals
		/// </summary>
		public Vector3 Normal;

		/// <summary>
		/// Vertex texture coordinates
		/// </summary>
		public Vector2 UV;

		/// <summary>
		/// Vertex color
		/// </summary>
		public Color4 Color;

		/// <summary>
		/// Size in bytes
		/// </summary>
		public static int SizeInBytes => Marshal.SizeOf(typeof(LibRenderVertex));
	}
}
