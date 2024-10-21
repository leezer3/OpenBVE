using System.Runtime.InteropServices;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;

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

		/// <summary>Creates a LibRenderVertex from a position value</summary>
		public LibRenderVertex(Vector3f position)
		{
			Position = position;
			Normal = Vector3f.Zero;
			UV = Vector2f.Null;
			Color = Color128.White;
		}

		/// <summary>Creates a LibRenderVertex from a template vertex and a normal</summary>
		public LibRenderVertex(VertexTemplate template, Vector3f normal)
		{
			Position = template.Coordinates;
			Normal = normal;
			UV = template.TextureCoordinates;
			Color = (template as ColoredVertex)?.Color ?? Color128.White;
		}
	}
}
