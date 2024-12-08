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

		/// <summary>The matrix chain</summary>
		public int MatrixChain;

		/// <summary>Size in bytes</summary>
		public static int SizeInBytes => Marshal.SizeOf(typeof(LibRenderVertex));

		/// <summary>Creates a LibRenderVertex from a position value</summary>
		public LibRenderVertex(Vector3f position)
		{
			Position = position;
			Normal = Vector3f.Zero;
			UV = Vector2f.Null;
			Color = Color128.White;
			MatrixChain = -1;
		}

		/// <summary>Creates a LibRenderVertex from a template vertex and a normal</summary>
		public LibRenderVertex(VertexTemplate template, Vector3f normal)
		{
			Position = template.Coordinates;
			Normal = normal;
			UV = template.TextureCoordinates;
			Color = (template as ColoredVertex)?.Color ?? Color128.White;
			if (template is AnimatedVertex av)
			{
				if (av.MatrixChain.Length <= 4)
				{
					int n0 = -1, n1 = -1, n2 = -2, n3 = -3;
					for (int i = 0; i < av.MatrixChain.Length; i++)
					{
						switch (i)
						{
							case 0:
								n0 = av.MatrixChain[0] << 24;
								break;
							case 1:
								n1 = av.MatrixChain[1] << 16;
								break;
							case 2:
								n2 = av.MatrixChain[2] << 8;
								break;
							case 3:
								n3 = av.MatrixChain[3];
								break;
						}
					}

					MatrixChain = n0 | n1 | n2 | n3;
				}
				else
				{
					MatrixChain = 0;
				}
			}
			else
			{
				MatrixChain = 0;
			}
		}
	}
}
