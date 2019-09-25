using OpenTK.Graphics.OpenGL;

namespace LibRender2.MotionBlurs
{
	public class MotionBlur
	{
		private readonly BaseRenderer renderer;

		/// <summary>The pixel buffer used for rendering the motion blur</summary>
		/// <remarks>Must be static to avoid re-allocating the array memory every frame</remarks>
		private byte[] PixelBuffer;
		/// <summary>The OpenGL texture index from which the blurred image is rendered</summary>
		private int PixelBufferOpenGlTextureIndex;

		internal MotionBlur(BaseRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>Initializes motion blur</summary>
		public void Initialize(MotionBlurMode mode)
		{
			if (mode == MotionBlurMode.None)
			{
				return;
			}

			if (PixelBufferOpenGlTextureIndex != 0)
			{
				GL.DeleteTextures(1, new int[] { PixelBufferOpenGlTextureIndex });
				PixelBufferOpenGlTextureIndex = 0;
			}

			PixelBuffer = new byte[4 * renderer.Screen.Width * renderer.Screen.Height];
			int[] a = new int[1];

			GL.GenTextures(1, a);
			GL.BindTexture(TextureTarget.Texture2D, a[0]);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, renderer.Screen.Width, renderer.Screen.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, PixelBuffer);
			PixelBufferOpenGlTextureIndex = a[0];
			GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 0, 0, renderer.Screen.Width, renderer.Screen.Height, 0);
		}

		/// <summary>This function renderers full-screen motion blur if selected</summary>
		public void RenderFullscreen(MotionBlurMode mode, double frameRate, double speed)
		{

		}
	}
}
