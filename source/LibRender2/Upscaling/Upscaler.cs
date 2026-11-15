//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace LibRender2.Upscaling
{
        /// <summary>
        /// Spatial upscaler framework (AMD FSR 1.0 fused EASU+RCAS / NVIDIA Image Scaling /
        /// Qualcomm SGSR v1).
        ///
        /// Pipeline (when active):
        ///   1. BeginScene(): the 3D world is rendered into a low-resolution framebuffer
        ///      object (color texture RGBA8 + depth/stencil renderbuffer at scene size).
        ///      If the window context has MSAA samples, multisampled renderbuffers are
        ///      used and resolved into the scene texture via glBlitFramebuffer.
        ///   2. EndScene(): the low-res scene texture is upscaled + sharpened to the
        ///      default framebuffer at native window resolution by a single-pass
        ///      fragment shader driven by a fullscreen triangle. No intermediate
        ///      textures, no per-resize recompilation (uniform updates only).
        ///   3. HUD / overlays / menus are drawn afterwards at native resolution,
        ///      never through the upscaler.
        ///
        /// When the selected mode is Off, the class is fully dormant (no GL state
        /// changes, zero overhead) and the world renders straight to the window as before.
        /// </summary>
        public class Upscaler : IDisposable
        {
                /// <summary>Holds a reference to the base renderer</summary>
                private readonly BaseRenderer renderer;

                // --- GL objects ---
                private int programHandle;
                private int sceneTexture;
                private int sceneFbo;
                private int sceneDepthRbo;
                private int msaaFbo;
                private int msaaColorRbo;
                private int msaaDepthRbo;
                private int nisCoefTexture;

                // --- uniform locations (cached once per program) ---
                private int uSrcLoc;
                private int uCoefLoc;
                private int uEasuCon0Loc;
                private int uInputSizeLoc;
                private int uTexelStepLoc;
                private int uDstResInvLoc;
                private int uSharpnessLoc;
                private int uNisCfg0Loc, uNisCfg1Loc, uNisCfg2Loc, uNisCfg3Loc;
                private int uEdgeSharpnessLoc;

                // --- current configuration state ---
                private UpscalerMode configuredMode = UpscalerMode.Off;
                private bool configuredDenoise;
                private int configuredSceneW;
                private int configuredSceneH;
                private int configuredWindowW;
                private int configuredWindowH;
                private bool configured;

                /// <summary>Whether the upscaler was initialized successfully for the current context</summary>
                public bool Available { get; private set; }
                /// <summary>Whether shader compilation or framebuffer creation failed at runtime; the upscaler disables itself</summary>
                public bool Failed { get; private set; }

                /// <summary>The currently selected upscaler mode</summary>
                public UpscalerMode Mode => renderer.currentOptions.UpscalerMode;

                /// <summary>Whether the upscaler will process the world this frame</summary>
                public bool Active => Mode != UpscalerMode.Off && Available && !Failed;

                /// <summary>Width of the low-resolution scene render target</summary>
                public int SceneWidth => configuredSceneW;

                /// <summary>Height of the low-resolution scene render target</summary>
                public int SceneHeight => configuredSceneH;

                /// <summary>Whether rendering is currently taking place inside the low-resolution scene phase</summary>
                internal bool ScenePhaseActive { get; private set; }

                /// <summary>The number of MSAA samples of the default framebuffer (0 = none)</summary>
                private readonly int msaaSamples;

                public Upscaler(BaseRenderer renderer)
                {
                        this.renderer = renderer;
                        Available = true;
                        try
                        {
                                msaaSamples = renderer.GraphicsMode != null ? Math.Max(0, renderer.GraphicsMode.Samples) : 0;
                        }
                        catch
                        {
                                msaaSamples = 0;
                        }
                }

                /// <summary>
                /// The internal render scale for the selected quality preset, snapped to the
                /// vendor-recommended presets of the selected upscaler. NVIDIA Image Scaling
                /// additionally clamps at 2.0x per axis (its official scale range).
                /// </summary>
                private double GetEffectiveScale()
                {
                        UpscalerMode mode = Mode;
                        UpscalerQuality quality = UpscalerPresets.SnapToMode(mode, renderer.currentOptions.UpscalerQuality);
                        double scale = UpscalerPresets.GetRenderScale(quality);
                        if (mode == UpscalerMode.Nis && scale < 0.5)
                        {
                                // NIS supports arbitrary phases only up to 2.0x per axis
                                scale = 0.5;
                        }
                        return scale;
                }

                /// <summary>
                /// Computes the low-resolution scene size for the given window size from the
                /// current options. Pure math - safe to call outside the render thread / without
                /// touching any GL objects.
                /// </summary>
                public void ComputeSceneSize(int windowWidth, int windowHeight, out int sceneWidth, out int sceneHeight)
                {
                        double scale = GetEffectiveScale();
                        sceneWidth = Math.Max(1, (int)Math.Round(windowWidth * scale));
                        sceneHeight = Math.Max(1, (int)Math.Round(windowHeight * scale));
                }

                /// <summary>
                /// Binds the low-resolution scene framebuffer and switches the viewport.
                /// Call immediately before the world rendering (and before the scene clear).
                /// </summary>
                public void BeginScene()
                {
                        int windowW = renderer.Screen.Width;
                        int windowH = renderer.Screen.Height;

                        ComputeSceneSize(windowW, windowH, out int sceneW, out int sceneH);

                        UpscalerMode mode = Mode;
                        bool denoise = renderer.currentOptions.UpscalerDenoise;

                        bool reconfigure = !configured ||
                                configuredMode != mode ||
                                configuredDenoise != denoise ||
                                configuredSceneW != sceneW ||
                                configuredSceneH != sceneH ||
                                configuredWindowW != windowW ||
                                configuredWindowH != windowH;

                        if (reconfigure)
                        {
                                // Rebuild everything (cheap: happens only on setting changes / resizes)
                                DisposeGlObjects();
                                configuredMode = mode;
                                configuredDenoise = denoise;
                                configuredSceneW = sceneW;
                                configuredSceneH = sceneH;
                                configuredWindowW = windowW;
                                configuredWindowH = windowH;
                                configured = true;

                                try
                                {
                                        CreateProgram(mode, denoise);
                                        if (mode == UpscalerMode.Nis)
                                        {
                                                CreateNisCoefficientTexture();
                                        }
                                        CreateSceneTargets(sceneW, sceneH);
                                        Failed = false;
                                }
                                catch (Exception ex)
                                {
                                        renderer.currentHost.AddMessage(MessageType.Error, false, "The upscaler failed to initialize and has been disabled: " + ex.Message);
                                        Failed = true;
                                        GL.GetError();
                                        DisposeGlObjects();
                                        return;
                                }
                        }

                        ScenePhaseActive = true;

                        if (msaaSamples > 1)
                        {
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, msaaFbo);
                        }
                        else
                        {
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, sceneFbo);
                        }

                        GL.Viewport(0, 0, sceneW, sceneH);
                }

                /// <summary>
                /// Re-binds the low-resolution scene framebuffer after another subsystem
                /// (e.g. the cascaded shadow pass) has bound its own framebuffer and reset
                /// the binding to the default framebuffer. No-op outside the scene phase.
                /// </summary>
                internal void RebindTarget()
                {
                        if (!ScenePhaseActive)
                        {
                                return;
                        }

                        if (msaaSamples > 1)
                        {
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, msaaFbo);
                        }
                        else
                        {
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, sceneFbo);
                        }

                        GL.Viewport(0, 0, configuredSceneW, configuredSceneH);
                }

                /// <summary>
                /// Resolves and upscales the low-resolution scene to the default framebuffer,
                /// then restores the native viewport. Call after the world rendering, before
                /// any HUD / overlay / menu rendering.
                /// </summary>
                public void EndScene()
                {
                        if (!ScenePhaseActive)
                        {
                                return;
                        }

                        ScenePhaseActive = false;

                        int windowW = configuredWindowW;
                        int windowH = configuredWindowH;

                        // 1. Resolve MSAA scene into the scene texture
                        if (msaaSamples > 1)
                        {
                                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, msaaFbo);
                                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, sceneFbo);
                                GL.BlitFramebuffer(0, 0, configuredSceneW, configuredSceneH, 0, 0, configuredSceneW, configuredSceneH,
                                        ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                        }

                        // 2. Upscale + sharpen to the default framebuffer
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        GL.Viewport(0, 0, windowW, windowH);

                        // Save blend factors and set clean raster state for the fullscreen pass
                        BlendingFactor srcFactor = renderer.GetBlendSrcFactor();
                        BlendingFactor dstFactor = renderer.GetBlendDestFactor();
                        bool blendWasEnabled = renderer.IsBlendEnabled();

                        GL.Disable(EnableCap.DepthTest);
                        GL.DepthMask(false);
                        GL.Disable(EnableCap.CullFace);
                        GL.Disable(EnableCap.ScissorTest);
                        GL.Disable(EnableCap.AlphaTest);
                        GL.Disable(EnableCap.Blend);
                        GL.ColorMask(true, true, true, true);

                        // Program switching must go through the renderer's current shader tracking.
                        // The active shader stays referenced (mirrors AbstractShader.Deactivate()):
                        // it is only flagged inactive so the next Activate() call rebinds its
                        // program. Nulling CurrentShader would crash the SetAlphaFunc() /
                        // SetBlendFunc() callers in the overlay pass.
                        if (renderer.CurrentShader != null)
                        {
                                renderer.CurrentShader.IsActive = false;
                        }
                        GL.UseProgram(programHandle);

                        renderer.dummyVao.Bind();

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, sceneTexture);

                        // Uniforms
                        float scaleX = (float)(configuredSceneW / (double)windowW);
                        float scaleY = (float)(configuredSceneH / (double)windowH);

                        if (configuredMode == UpscalerMode.Fsr1)
                        {
                                float sharpness = (float)Math.Min(1.0, Math.Max(0.0, renderer.currentOptions.UpscalerSharpnessFsr));
                                GL.Uniform1(uSrcLoc, 0);
                                GL.Uniform4(uEasuCon0Loc, new Vector4(scaleX, scaleY, 0.5f * scaleX - 0.5f, 0.5f * scaleY - 0.5f));
                                GL.Uniform2(uTexelStepLoc, new Vector2(1.0f / configuredSceneW, 1.0f / configuredSceneH));
                                GL.Uniform2(uDstResInvLoc, new Vector2(1.0f / windowW, 1.0f / windowH));
                                GL.Uniform1(uSharpnessLoc, sharpness);
                        }
                        else if (configuredMode == UpscalerMode.Nis)
                        {
                                // NVIDIA Image Scaling: config constants per NVScalerUpdateConfig (SDR mode)
                                double sharpness = Math.Min(1.0, Math.Max(0.0, renderer.currentOptions.UpscalerSharpnessNis));
                                double sharpenSlider = sharpness - 0.5;
                                double maxScale = sharpenSlider >= 0.0 ? 1.25 : 1.75;
                                double minScale = sharpenSlider >= 0.0 ? 1.25 : 1.0;
                                double limitScale = sharpenSlider >= 0.0 ? 1.25 : 1.0;

                                const double kDetectRatio = 2 * 1127.0 / 1024.0;
                                const double kDetectThres = 64.0 / 1024.0;
                                const double kMinContrastRatio = 2.0;
                                const double kMaxContrastRatio = 10.0;
                                const double kSharpStartY = 0.45;
                                const double kSharpEndY = 0.9;
                                double kSharpStrengthMin = Math.Max(0.0, 0.4 + sharpenSlider * minScale * 1.2);
                                double kSharpStrengthMax = 1.6 + sharpenSlider * maxScale * 1.8;
                                double kSharpLimitMin = Math.Max(0.1, 0.14 + sharpenSlider * limitScale * 0.32);
                                double kSharpLimitMax = 0.5 + sharpenSlider * limitScale * 0.6;
                                double kRatioNorm = 1.0 / (kMaxContrastRatio - kMinContrastRatio);
                                double kSharpScaleY = 1.0 / (kSharpEndY - kSharpStartY);
                                double kSharpStrengthScale = kSharpStrengthMax - kSharpStrengthMin;
                                double kSharpLimitScale = kSharpLimitMax - kSharpLimitMin;

                                // Coefficient texture with the 64x6 filter banks (unit 1)
                                GL.ActiveTexture(TextureUnit.Texture1);
                                GL.BindTexture(TextureTarget.Texture2D, nisCoefTexture);
                                GL.Uniform1(uCoefLoc, 1);

                                GL.Uniform1(uSrcLoc, 0);
                                GL.Uniform4(uNisCfg0Loc, new Vector4((float)kDetectRatio, (float)kDetectThres, (float)kMinContrastRatio, (float)kRatioNorm));
                                GL.Uniform4(uNisCfg1Loc, new Vector4(1.0f, (float)(1.0 / 255.0), (float)kSharpStartY, (float)kSharpScaleY));
                                GL.Uniform4(uNisCfg2Loc, new Vector4((float)kSharpStrengthMin, (float)kSharpStrengthScale, (float)kSharpLimitMin, (float)kSharpLimitScale));
                                GL.Uniform4(uNisCfg3Loc, new Vector4(scaleX, scaleY, (float)(1.0 / configuredSceneW), (float)(1.0 / configuredSceneH)));
                        }
                        else
                        {
                                // Qualcomm SGSR v1 (mobile + edge direction variant)
                                double sharpness = Math.Min(1.0, Math.Max(0.0, renderer.currentOptions.UpscalerSharpnessSgsr));
                                GL.Uniform1(uSrcLoc, 0);
                                GL.Uniform2(uInputSizeLoc, new Vector2(configuredSceneW, configuredSceneH));
                                GL.Uniform2(uTexelStepLoc, new Vector2(1.0f / configuredSceneW, 1.0f / configuredSceneH));
                                GL.Uniform1(uEdgeSharpnessLoc, (float)(1.0 + sharpness));
                        }

                        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

                        // 3. Restore state for the native-resolution overlay pass
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                        GL.ActiveTexture(TextureUnit.Texture1);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.UseProgram(0);
                        renderer.LastBoundTexture = null;
                        renderer.lastVAO = -1;

                        if (blendWasEnabled)
                        {
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFunc(srcFactor, dstFactor);
                        }
                }

                /// <summary>Compiles the shader program for the given mode. The GLSL sources
                /// live in assets/Shaders/*.vert / *.frag and are compiled into LibRender2
                /// as embedded resources (same mechanism as AbstractShader).</summary>
                private void CreateProgram(UpscalerMode mode, bool denoise)
                {
                        string fragmentSource;
                        if (mode == UpscalerMode.Fsr1)
                        {
                                fragmentSource = LoadShaderResource("fsr1.frag");
                                if (denoise)
                                {
                                        // Insert the RCAS denoiser define right after the #version
                                        // line (all preprocessor directives except #version, and
                                        // any amount of comments, must precede nothing else - a
                                        // #define before #version is illegal GLSL).
                                        int versionStart = fragmentSource.IndexOf("#version", StringComparison.Ordinal);
                                        if (versionStart >= 0)
                                        {
                                                int lineEnd = fragmentSource.IndexOf('\n', versionStart);
                                                if (lineEnd >= 0)
                                                {
                                                        fragmentSource = fragmentSource.Substring(0, lineEnd + 1)
                                                                + "// RCAS denoiser enabled via options\n#define FSR_RCAS_DENOISE 1\n"
                                                                + fragmentSource.Substring(lineEnd + 1);
                                                }
                                        }
                                }
                        }
                        else if (mode == UpscalerMode.Nis)
                        {
                                fragmentSource = LoadShaderResource("nis.frag");
                        }
                        else
                        {
                                fragmentSource = LoadShaderResource("sgsr1.frag");
                        }

                        string vertexSource = LoadShaderResource("fullscreen.vert");

                        programHandle = GL.CreateProgram();

                        int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
                        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);
                        GL.AttachShader(programHandle, vertexShader);
                        GL.AttachShader(programHandle, fragmentShader);
                        GL.BindFragDataLocation(programHandle, 0, "fragColor");
                        GL.LinkProgram(programHandle);
                        GL.GetProgram(programHandle, GetProgramParameterName.LinkStatus, out int success);
                        GL.DeleteShader(vertexShader);
                        GL.DeleteShader(fragmentShader);
                        if (success == 0)
                        {
                                string infoLog = GL.GetProgramInfoLog(programHandle);
                                throw new Exception("Shader link error: " + infoLog);
                        }

                        // Cache uniform locations once (queried per frame is wasteful).
                        // Uniforms that do not exist in the active program return -1,
                        // which GL.Uniform* silently ignores.
                        uSrcLoc = GL.GetUniformLocation(programHandle, "uSrc");
                        uCoefLoc = GL.GetUniformLocation(programHandle, "uCoef");
                        uEasuCon0Loc = GL.GetUniformLocation(programHandle, "uEasuCon0");
                        uInputSizeLoc = GL.GetUniformLocation(programHandle, "uInputSize");
                        uTexelStepLoc = GL.GetUniformLocation(programHandle, "uTexelStep");
                        uDstResInvLoc = GL.GetUniformLocation(programHandle, "uDstResInv");
                        uSharpnessLoc = GL.GetUniformLocation(programHandle, "uSharpness");
                        uNisCfg0Loc = GL.GetUniformLocation(programHandle, "uNisCfg0");
                        uNisCfg1Loc = GL.GetUniformLocation(programHandle, "uNisCfg1");
                        uNisCfg2Loc = GL.GetUniformLocation(programHandle, "uNisCfg2");
                        uNisCfg3Loc = GL.GetUniformLocation(programHandle, "uNisCfg3");
                        uEdgeSharpnessLoc = GL.GetUniformLocation(programHandle, "uEdgeSharpness");
                }

                /// <summary>
                /// Creates the RG32F 6x64 coefficient texture holding the NIS filter
                /// banks. One texel (tap, phase) stores (coef_scale, coef_usm) together,
                /// so the shader needs half the coefficient fetches of the official
                /// two-texture layout. The bank values come from NisCoefficients.cs,
                /// a readable transcription of the official NIS_Config.h tables.
                /// Sampling a small cache-resident texture with texelFetch is the fast,
                /// driver-independent way of doing dynamically indexed table lookups -
                /// the official NIS SDK uses the same design.
                /// </summary>
                private void CreateNisCoefficientTexture()
                {
                        float[] data = NisCoefficients.CreateTextureData();

                        GL.GenTextures(1, out nisCoefTexture);
                        GL.BindTexture(TextureTarget.Texture2D, nisCoefTexture);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rg32f,
                                NisCoefficients.FilterSize, NisCoefficients.PhaseCount, 0,
                                OpenTK.Graphics.OpenGL.PixelFormat.Rg, PixelType.Float, data);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                }

                /// <summary>Opens an embedded resource of the LibRender2 assembly</summary>
                private static Stream OpenShaderResource(string name)
                {
                        Stream stream = typeof(Upscaler).Assembly.GetManifestResourceStream("LibRender2." + name);
                        if (stream == null)
                        {
                                throw new Exception("Missing embedded resource: LibRender2." + name);
                        }
                        return stream;
                }

                /// <summary>Reads an embedded shader source file (UTF-8) into a string</summary>
                private static string LoadShaderResource(string name)
                {
                        using (Stream stream = OpenShaderResource(name))
                        {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                        return reader.ReadToEnd();
                                }
                        }
                }

                /// <summary>Compiles an individual shader object</summary>
                private int CompileShader(ShaderType type, string source)
                {
                        int shader = GL.CreateShader(type);
                        GL.ShaderSource(shader, source);
                        GL.CompileShader(shader);
                        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
                        if (status == 0)
                        {
                                string infoLog = GL.GetShaderInfoLog(shader);
                                GL.DeleteShader(shader);
                                throw new Exception(type + " compile error: " + infoLog);
                        }
                        return shader;
                }

                /// <summary>Creates the low-resolution scene texture and framebuffers</summary>
                private void CreateSceneTargets(int width, int height)
                {
                        GL.GenTextures(1, out sceneTexture);
                        GL.BindTexture(TextureTarget.Texture2D, sceneTexture);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,
                                OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

                        GL.GenFramebuffers(1, out sceneFbo);
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, sceneFbo);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, sceneTexture, 0);
                        GL.GenRenderbuffers(1, out sceneDepthRbo);
                        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, sceneDepthRbo);
                        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, sceneDepthRbo);

                        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                        {
                                throw new Exception("Low-resolution scene framebuffer is incomplete: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                        }

                        if (msaaSamples > 1)
                        {
                                GL.GenFramebuffers(1, out msaaFbo);
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, msaaFbo);
                                GL.GenRenderbuffers(1, out msaaColorRbo);
                                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, msaaColorRbo);
                                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, msaaSamples, RenderbufferStorage.Rgba8, width, height);
                                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, msaaColorRbo);
                                GL.GenRenderbuffers(1, out msaaDepthRbo);
                                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, msaaDepthRbo);
                                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, msaaSamples, RenderbufferStorage.Depth24Stencil8, width, height);
                                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, msaaDepthRbo);

                                if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                                {
                                        throw new Exception("MSAA scene framebuffer is incomplete: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                                }
                        }

                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }

                /// <summary>Deletes all GL objects owned by this upscaler</summary>
                private void DisposeGlObjects()
                {
                        if (programHandle != 0)
                        {
                                GL.DeleteProgram(programHandle);
                                programHandle = 0;
                        }
                        if (sceneFbo != 0)
                        {
                                GL.DeleteFramebuffer(sceneFbo);
                                sceneFbo = 0;
                        }
                        if (sceneDepthRbo != 0)
                        {
                                GL.DeleteRenderbuffer(sceneDepthRbo);
                                sceneDepthRbo = 0;
                        }
                        if (msaaFbo != 0)
                        {
                                GL.DeleteFramebuffer(msaaFbo);
                                msaaFbo = 0;
                        }
                        if (msaaColorRbo != 0)
                        {
                                GL.DeleteRenderbuffer(msaaColorRbo);
                                msaaColorRbo = 0;
                        }
                        if (msaaDepthRbo != 0)
                        {
                                GL.DeleteRenderbuffer(msaaDepthRbo);
                                msaaDepthRbo = 0;
                        }
                        if (sceneTexture != 0)
                        {
                                GL.DeleteTexture(sceneTexture);
                                sceneTexture = 0;
                        }
                        if (nisCoefTexture != 0)
                        {
                                GL.DeleteTexture(nisCoefTexture);
                                nisCoefTexture = 0;
                        }
                }

                /// <summary>Releases all GL resources</summary>
                public void Dispose()
                {
                        DisposeGlObjects();
                        ScenePhaseActive = false;
                        GC.SuppressFinalize(this);
                }
        }
}
