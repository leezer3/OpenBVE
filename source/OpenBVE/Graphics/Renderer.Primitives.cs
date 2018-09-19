using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBve {
	internal static partial class Renderer {

		/// <summary>Draws a rectangle.</summary>
		/// <param name="texture">The texture, or a null reference.</param>
		/// <param name="point">The top-left coordinates in pixels.</param>
		/// <param name="size">The size in pixels.</param>
		/// <param name="color">The color, or a null reference.</param>
		internal static void DrawRectangle(Texture texture, Point point, Size size, Color128? color) {
			// TODO: Remove Nullable<T> from color once RenderOverlayTexture and RenderOverlaySolid are fully replaced.
			if (texture == null || !Textures.LoadTexture(texture, OpenGlTextureWrapMode.ClampClamp)) {
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
		
	}
}
