using System;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    internal static partial class Renderer
    {
		/// <summary>The pixel buffer used for rendering the motion blur</summary>
		/// <remarks>Must be static to avoid re-allocating the array memory every frame</remarks>
	    private static byte[] PixelBuffer = null;
		/// <summary>The OpenGL texture index from which the blurred image is rendered</summary>
	    private static int PixelBufferOpenGlTextureIndex = 0;
		/// <summary>Intializes motion blur</summary>
	    internal static void InitializeMotionBlur()
	    {
		    if (Interface.CurrentOptions.MotionBlur == Interface.MotionBlurMode.None)
		    {
			    return;
		    }

		    if (PixelBufferOpenGlTextureIndex != 0)
		    {
			    GL.DeleteTextures(1, new int[] {PixelBufferOpenGlTextureIndex});
			    PixelBufferOpenGlTextureIndex = 0;
		    }
		    int w = Interface.CurrentOptions.NoTextureResize ? Screen.Width : Textures.RoundUpToPowerOfTwo(Screen.Width);
		    int h = Interface.CurrentOptions.NoTextureResize ? Screen.Height : Textures.RoundUpToPowerOfTwo(Screen.Height);
		    PixelBuffer = new byte[4 * w * h];
		    int[] a = new int[1];
		    GL.GenTextures(1, a);
		    GL.BindTexture(TextureTarget.Texture2D, a[0]);
		    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMagFilter.Linear);
		    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, w, h, 0, PixelFormat.Rgb,
			    PixelType.UnsignedByte, PixelBuffer);
		    PixelBufferOpenGlTextureIndex = a[0];
		    GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 0, 0, w, h, 0);
	    }

	    /// <summary>This function renderers full-screen motion blur if selected</summary>
		private static void RenderFullscreenMotionBlur()
        {
			if(Screen.Minimized)
			{
				/*
				 * HACK:
				 * This breaks if minimized, even if we don't reset the W / H values
				 */
				return;
			}
            int w = Interface.CurrentOptions.NoTextureResize ? Screen.Width : Textures.RoundUpToPowerOfTwo(Screen.Width);
            int h = Interface.CurrentOptions.NoTextureResize ? Screen.Height : Textures.RoundUpToPowerOfTwo(Screen.Height);
            // render
            if (PixelBufferOpenGlTextureIndex >= 0)
            {
                double strength;
                switch (Interface.CurrentOptions.MotionBlur)
                {
                    case Interface.MotionBlurMode.Low: strength = 0.0025; break;
                    case Interface.MotionBlurMode.Medium: strength = 0.0040; break;
                    case Interface.MotionBlurMode.High: strength = 0.0064; break;
                    default: strength = 0.0040; break;
                }
                double speed = Math.Abs(World.CameraSpeed);
                double denominator = strength * Game.InfoFrameRate * Math.Sqrt(speed);
                float factor;
                if (denominator > 0.001)
                {
                    factor = (float)Math.Exp(-1.0 / denominator);
                }
                else
                {
                    factor = 0.0f;
                }
                // initialize
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                if (!BlendEnabled)
                {
                    GL.Enable(EnableCap.Blend);
                    BlendEnabled = true;
                }
                if (LightingEnabled)
                {
                    GL.Disable(EnableCap.Lighting);
                    LightingEnabled = false;
                }
                GL.MatrixMode(MatrixMode.Projection);
                GL.PushMatrix();
                GL.LoadIdentity();
                GL.Ortho(0.0, (double)Screen.Width, 0.0, (double)Screen.Height, -1.0, 1.0);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                GL.LoadIdentity();
                if (!TexturingEnabled)
                {
                    GL.Enable(EnableCap.Texture2D);
                    TexturingEnabled = true;
                }
                // render
                GL.BindTexture(TextureTarget.Texture2D, PixelBufferOpenGlTextureIndex);
                GL.Color4(1.0f, 1.0f, 1.0f, factor);
                GL.Begin(PrimitiveType.Polygon);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex2(0.0, 0.0);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex2(0.0, (double)h);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex2((double)w, (double)h);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex2((double)w, 0.0);
                GL.End();
                // finalize
                GL.PopMatrix();
                GL.MatrixMode(MatrixMode.Projection);
                GL.PopMatrix();
                GL.MatrixMode(MatrixMode.Modelview);
            }
            // retrieve buffer
            {
                GL.BindTexture(TextureTarget.Texture2D, PixelBufferOpenGlTextureIndex);
                GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb8, 0, 0, w, h, 0);
            }
        }
    }
}
