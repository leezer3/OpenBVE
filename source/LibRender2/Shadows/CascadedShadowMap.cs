using System;
using OpenBveApi.Math;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.ShadowMapping
{
    /// <summary>
    /// A single non-square depth atlas shared by all cascades. Each cascade gets a rectangle
    /// matching its light-space AABB; Shadows folds the rectangle transform into the light
    /// matrix, so the shaders only ever see one texture and need no changes.
    /// </summary>
    public class CascadedShadowMap : IDisposable
    {
        private struct CascadeRect
        {
            public int X, Y, Width, Height;

            public CascadeRect(int x, int y, int width, int height)
            {
                X = x; Y = y; Width = width; Height = height;
            }
        }

        private const int Gutter = 2;
        private readonly CascadeRect[] slots = new CascadeRect[4];

        private int depthTexture;
        private int fbo;
        private int atlasWidth;
        private int atlasHeight;
        private bool created;
        private bool autoResolution;

        /// <summary>Number of cascades (shadow splits).</summary>
        public int CascadeCount { get; private set; }

        /// <summary>Base resolution of the nearest cascade's depth texture.</summary>
        public int Resolution { get; private set; }

        /// <summary>Per-cascade atlas slot widths (near cascades sharper, far cascades cheaper).</summary>
        public int[] Resolutions { get; private set; }

        /// <summary>Height/width ratio of each slot (0.25, 0.5 or 1.0).</summary>
        public float[] SlotAspects { get; private set; }

        /// <summary>Creates the cascade descriptors; the atlas is allocated on the first frame.</summary>
        public CascadedShadowMap(int cascadeCount = 3, int resolution = 2048, bool autoResolution = true)
        {
            Configure(cascadeCount, resolution, autoResolution);
        }

        private void Configure(int cascadeCount, int resolution, bool taper)
        {
            CascadeCount = cascadeCount;
            Resolution = resolution;
            autoResolution = taper;
            Resolutions = BuildResolutions(cascadeCount, resolution, taper);
            SlotAspects = new float[cascadeCount];
            for (int i = 0; i < cascadeCount; i++)
            {
                SlotAspects[i] = 1.0f;
            }
            created = false;
            depthTexture = 0;
            fbo = 0;
        }

        /// <summary>Drops the old atlas and prepares new descriptors when shadow settings change.</summary>
        public void Resize(int newCascadeCount, int newResolution, bool newAutoResolution = true)
        {
            Dispose();
            Configure(newCascadeCount, newResolution, newAutoResolution);
            Console.WriteLine($"[CSM] Resized to {newCascadeCount} cascades at {newResolution}├ù{newResolution}");
        }

        /// <summary>Builds the atlas once the caster has measured the first frame's AABB aspects.</summary>
        public void EnsureLayout(float[] aspects)
        {
            if (created) return;
            QuantizeAspects(aspects);
            ComputeSlotLayout();
            CreateDepthTexture();
            CreateFramebuffer();
            created = true;
        }

        private void QuantizeAspects(float[] aspects)
        {
            if (aspects == null) return;
            for (int i = 0; i < CascadeCount && i < aspects.Length; i++)
            {
                SlotAspects[i] = aspects[i] >= 0.75f ? 1.0f : (aspects[i] >= 0.375f ? 0.5f : 0.25f);
            }
        }

        /// <summary>Shelf-packs the slots into the smallest non-square atlas that fits them.</summary>
        private void ComputeSlotLayout()
        {
            int widest = 0;
            for (int i = 0; i < CascadeCount; i++)
            {
                widest = Math.Max(widest, Resolutions[i]);
            }

            int x = 0, y = 0, rowHeight = 0;
            for (int i = 0; i < CascadeCount; i++)
            {
                int width = Resolutions[i];
                int height = Math.Max(64, (int)(Resolutions[i] * SlotAspects[i]));
                if (x > 0 && x + width + Gutter > widest)
                {
                    x = 0;
                    y += rowHeight + Gutter;
                    rowHeight = 0;
                }
                slots[i] = new CascadeRect(x, y, width, height);
                x += width + Gutter;
                rowHeight = Math.Max(rowHeight, height);
            }
            atlasWidth = widest + Gutter;
            atlasHeight = y + rowHeight + Gutter;
        }

        private void CreateDepthTexture()
        {
            // 16-bit depth halves VRAM; precision is sufficient because the caster uses
            // a tight ortho depth range around each sub-frustum.
            depthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.DepthComponent16,
                atlasWidth, atlasHeight, 0,
                PixelFormat.DepthComponent, PixelType.UnsignedShort, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureBorderColor, new float[] { 1, 1, 1, 1 });
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureCompareFunc, (int)All.Lequal);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void CreateFramebuffer()
        {
            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Console.Error.WriteLine("[CSM] Atlas FBO incomplete");
            }
            GL.Disable(EnableCap.ScissorTest);
            GL.ClearDepth(1.0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>Binds the atlas and restricts drawing/clearing to one cascade's rectangle.</summary>
        public void BindCascadeForWriting(int cascadeIndex)
        {
            CascadeRect r = slots[cascadeIndex];
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Viewport(r.X, r.Y, r.Width, r.Height);
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(r.X, r.Y, r.Width, r.Height);
        }

        /// <summary>Restores the default framebuffer and turns off the atlas scissor.</summary>
        public void Unbind()
        {
            GL.Disable(EnableCap.ScissorTest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Binds the same atlas to every cascade sampler; each light matrix already targets
        /// its own rectangle. E.g. baseUnit = Texture4 ΓåÆ all four samplers read that atlas.
        /// </summary>
        public void BindAllCascadesForReading(TextureUnit baseUnit)
        {
            for (int i = 0; i < 4; i++)
            {
                GL.ActiveTexture(baseUnit + i);
                GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            }
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        /// <summary>Maps light clip space into a slot's UV range (row-vector convention).</summary>
        public Matrix4D GetAtlasScale(int cascadeIndex)
        {
            CascadeRect r = slots[cascadeIndex];
            float u0 = (float)r.X / atlasWidth;
            float u1 = (float)(r.X + r.Width) / atlasWidth;
            float v0 = (float)r.Y / atlasHeight;
            float v1 = (float)(r.Y + r.Height) / atlasHeight;
            return new Matrix4D(
                new Vector4(u1 - u0, 0, 0, 0),
                new Vector4(0, v1 - v0, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(u0 + u1 - 1, v0 + v1 - 1, 0, 1));
        }

        /// <summary>Compact atlas summary for the renderer stats overlay.</summary>
        public string GetStatsString()
        {
            if (!created) return "CSM: pending";
            string[] rectangles = new string[CascadeCount];
            for (int i = 0; i < CascadeCount; i++)
            {
                rectangles[i] = $"{slots[i].Width}x{slots[i].Height}";
            }
            return $"CSM atlas {atlasWidth}x{atlasHeight}: {string.Join("|", rectangles)} (auto={(autoResolution ? "On" : "Off")})";
        }

        /// <summary>
        /// With auto-resolution on, farther cascades step down to cut fill rate;
        /// otherwise every cascade uses the full requested resolution.
        /// </summary>
        private static int[] BuildResolutions(int cascadeCount, int baseResolution, bool taper)
        {
            int[] resolutions = new int[cascadeCount];
            for (int i = 0; i < cascadeCount; i++)
            {
                int scale = taper ? (i == 0 ? 1 : (i == 1 ? 2 : 4)) : 1;
                resolutions[i] = Math.Max(512, baseResolution / scale);
            }
            return resolutions;
        }

        public void Dispose()
        {
            if (fbo != 0) { GL.DeleteFramebuffer(fbo); fbo = 0; }
            if (depthTexture != 0) { GL.DeleteTexture(depthTexture); depthTexture = 0; }
            created = false;
        }
    }
}
