using System;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Shadows
{
    /// <summary>
    /// Manages multiple depth-only FBOs for Cascaded Shadow Mapping.
    /// Each cascade gets its own FBO and depth texture at the same resolution.
    /// </summary>
    public class CascadedShadowMap : IDisposable
    {
        /// <summary>Number of cascades (shadow splits).</summary>
        public int CascadeCount { get; private set; }

        /// <summary>Shadow map resolution per cascade.</summary>
        public int Resolution { get; private set; }

        /// <summary>Per-cascade FBO handles.</summary>
        public int[] FBOs { get; private set; }

        /// <summary>Per-cascade depth texture handles.</summary>
        public int[] DepthTextures { get; private set; }

        /// <summary>
        /// Creates cascaded shadow maps.
        /// </summary>
        /// <param name="cascadeCount">Number of cascades (typically 3 or 4).</param>
        /// <param name="resolution">Resolution of each cascade's depth texture.</param>
        public CascadedShadowMap(int cascadeCount = 3, int resolution = 2048)
        {
            CascadeCount = cascadeCount;
            Resolution = resolution;
            FBOs = new int[cascadeCount];
            DepthTextures = new int[cascadeCount];

            for (int i = 0; i < cascadeCount; i++)
            {
                CreateCascade(i);
            }
        }

        /// <summary>
        /// Recreates all cascade FBOs/textures with a new cascade count and resolution.
        /// Call this when the user changes shadow settings.
        /// </summary>
        /// <param name="newCascadeCount">New number of cascades.</param>
        /// <param name="newResolution">New resolution per cascade.</param>
        public void Resize(int newCascadeCount, int newResolution)
        {
            // Dispose old resources
            Dispose();

            // Reallocate
            CascadeCount = newCascadeCount;
            Resolution = newResolution;
            FBOs = new int[newCascadeCount];
            DepthTextures = new int[newCascadeCount];

            for (int i = 0; i < newCascadeCount; i++)
            {
                CreateCascade(i);
            }

            Console.WriteLine(
                $"[CSM] Resized to {newCascadeCount} cascades at {newResolution}×{newResolution}");
        }

        private void CreateCascade(int index)
        {
            // Generate depth texture
            DepthTextures[index] = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTextures[index]);
            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.DepthComponent24,
                Resolution, Resolution, 0,
                PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            // Anything outside the shadow map reads as "fully lit"
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureBorderColor, borderColor);
            // Enable hardware PCF (compare mode)
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureCompareMode,
                (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureCompareFunc,
                (int)All.Lequal);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Generate FBO
            FBOs[index] = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBOs[index]);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, DepthTextures[index], 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Console.Error.WriteLine(
                    $"[CSM] Cascade {index} FBO incomplete: {status}");
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>Binds a specific cascade's FBO for depth writing.</summary>
        public void BindCascadeForWriting(int cascadeIndex)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBOs[cascadeIndex]);
            GL.Viewport(0, 0, Resolution, Resolution);
        }

        /// <summary>Unbinds shadow FBO.</summary>
        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Binds all cascade depth textures sequentially starting from the given texture unit.
        /// E.g., if baseUnit = TextureUnit.Texture4, cascade 0 → unit 4, cascade 1 → unit 5, etc.
        /// </summary>
        public void BindAllCascadesForReading(TextureUnit baseUnit)
        {
            for (int i = 0; i < CascadeCount; i++)
            {
                GL.ActiveTexture(baseUnit + i);
                GL.BindTexture(TextureTarget.Texture2D, DepthTextures[i]);
            }
            // Restore active texture to 0 so normal rendering isn't affected
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void Dispose()
        {
            for (int i = 0; i < CascadeCount; i++)
            {
                if (FBOs[i] != 0)
                {
                    GL.DeleteFramebuffer(FBOs[i]);
                    FBOs[i] = 0;
                }
                if (DepthTextures[i] != 0)
                {
                    GL.DeleteTexture(DepthTextures[i]);
                    DepthTextures[i] = 0;
                }
            }
        }
    }
}
