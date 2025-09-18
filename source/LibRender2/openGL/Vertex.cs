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
		public Vector3i MatrixChain;

		/// <summary>Size in bytes</summary>
		public static int SizeInBytes => Marshal.SizeOf(typeof(LibRenderVertex));

		/// <summary>Creates a LibRenderVertex from a position value</summary>
		public LibRenderVertex(Vector3f position)
		{
			Position = position;
			Normal = Vector3f.Zero;
			UV = Vector2f.Null;
			Color = Color128.White;
			MatrixChain = new Vector3i(-1, 0, 0);
		}

		/// <summary>Creates a LibRenderVertex from a template vertex and a normal</summary>
		public LibRenderVertex(VertexTemplate template, Vector3f normal)
		{
			MatrixChain = new Vector3i(-1, 0, 0);
			Position = template.Coordinates;
			Normal = normal;
			UV = template.TextureCoordinates;
			Color = (template as ColoredVertex)?.Color ?? Color128.White;
			if (template is AnimatedVertex av)
			{
				for (int i = 0; i < av.MatrixChain.Length; i += 4)
				{
					int n0 = -1, n1 = -1, n2 = -2, n3 = -3;
					for (int j = 0; j < av.MatrixChain.Length; j++)
					{
						switch (j)
						{
							case 0:
								n0 = av.MatrixChain[i] << 24;
								break;
							case 1:
								n1 = av.MatrixChain[i + 1] << 16;
								break;
							case 2:
								n2 = av.MatrixChain[i + 2] << 8;
								break;
							case 3:
								n3 = av.MatrixChain[i + 3];
								break;
						}
					}

					switch (i)
					{
						case 0:
							MatrixChain.X = n0 | n1 | n2 | n3;
							break;
						case 4:
							MatrixChain.Y = n0 | n1 | n2 | n3;
							break;
						case 8:
							MatrixChain.Z = n0 | n1 | n2 | n3;
							break;
					}
					
				}
			}
		}
	}
}
