using System;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    internal static partial class Renderer
    {
        /// <summary>
        /// This function renderers full-screen motion blur if selected
        /// </summary>
        private static void RenderFullscreenMotionBlur()
        {
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
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
                GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, 0, 0, w, h, 0);
            }
        }
    }
}
