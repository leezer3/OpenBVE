using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBveShared {
	public static partial class Renderer
	{
		/// <summary>Draws a rectangle.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="size">The size in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		public static void DrawRectangle(Texture texture, Point point, Size size, Color128? color) {
			// TODO: Remove Nullable<T> from color once RenderOverlayTexture and RenderOverlaySolid are fully replaced.
			if (texture == null || !currentHost.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp)) {
				GL.Disable(EnableCap.Texture2D);
				if (color.HasValue) {
					GL.Color4(color.Value.R, color.Value.G, color.Value.B, color.Value.A);
				}
				GL.Begin(PrimitiveType.Quads);
				GL.Vertex2(point.X, point.Y);
				GL.Vertex2(point.X + size.Width, point.Y);
				GL.Vertex2(point.X + size.Width, point.Y + size.Height);
				GL.Vertex2(point.X, point.Y + size.Height);
				GL.End();
			} else {
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
				if (color.HasValue) {
					GL.Color4(color.Value.R, color.Value.G, color.Value.B, color.Value.A);
				}
				GL.Begin(PrimitiveType.Quads);
				GL.TexCoord2(0.0f, 0.0f);
				GL.Vertex2(point.X, point.Y);
				GL.TexCoord2(1.0f, 0.0f);
				GL.Vertex2(point.X + size.Width, point.Y);
				GL.TexCoord2(1.0f, 1.0f);
				GL.Vertex2(point.X + size.Width, point.Y + size.Height);
				GL.TexCoord2(0.0f, 1.0f);
				GL.Vertex2(point.X, point.Y + size.Height);
				GL.End();
			}
		}
		
		/// <summary>Draws a cube</summary>
		/// <param name="Position">The position vector</param>
		/// <param name="Direction">The direction vector</param>
		/// <param name="Up">The up vector</param>
		/// <param name="Side">The side vector</param>
		/// <param name="Size">The size of the cube</param>
		/// <param name="CameraPosition">The position of the camera</param>
		/// <param name="Texture">The texture for the cube or a null reference</param>
		public static void DrawCube(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, double Size, Vector3 CameraPosition, Texture Texture)
		{
			
			Vector3[] v = new Vector3[8];
			v[0] = new Vector3(Size, Size, -Size);
			v[1] = new Vector3(Size, -Size, -Size);
			v[2] = new Vector3(-Size, -Size, -Size);
			v[3] = new Vector3(-Size, Size, -Size);
			v[4] = new Vector3(Size, Size, Size);
			v[5] = new Vector3(Size, -Size, Size);
			v[6] = new Vector3(-Size, -Size, Size);
			v[7] = new Vector3(-Size, Size, Size);
			for (int i = 0; i < 8; i++)
			{
				v[i].Rotate(Direction, Up, Side);
				v[i].X += Position.X - CameraPosition.X;
				v[i].Y += Position.Y - CameraPosition.Y;
				v[i].Z += Position.Z - CameraPosition.Z;
			}
			int[][] Faces = new int[6][];
			Faces[0] = new int[] { 0, 1, 2, 3 };
			Faces[1] = new int[] { 0, 4, 5, 1 };
			Faces[2] = new int[] { 0, 3, 7, 4 };
			Faces[3] = new int[] { 6, 5, 4, 7 };
			Faces[4] = new int[] { 6, 7, 3, 2 };
			Faces[5] = new int[] { 6, 2, 1, 5 };
			if (Texture == null || !currentHost.LoadTexture(Texture, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.Disable(EnableCap.Texture2D);
				for (int i = 0; i < 6; i++)
				{
					GL.Begin(PrimitiveType.Quads);
					GL.Color3(1.0, 1.0, 1.0);
					for (int j = 0; j < 4; j++)
					{
						GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
					}
					GL.End();
				}
				return;
			}
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, Texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			Vector2[][] t = new Vector2[6][];
				t[0] = new Vector2[] { new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0) };
				t[1] = new Vector2[] { new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0) };
				t[2] = new Vector2[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
				t[3] = new Vector2[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
				t[4] = new Vector2[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
				t[5] = new Vector2[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
			for (int i = 0; i < 6; i++)
			{
				GL.Begin(PrimitiveType.Quads);
				GL.Color3(1.0, 1.0, 1.0);
				for (int j = 0; j < 4; j++)
				{
					GL.TexCoord2(t[i][j].X, t[i][j].Y);
					GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
				}
				GL.End();
			}
		}
	}
}
