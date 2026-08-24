using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Textures
{
	public class VideoTexture : IDisposable
	{
		public string VideoPath { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int TextureId { get; private set; }
		
		private IntPtr mpvHandle;
		private IntPtr renderContext;
		private int fboId;
		private bool initialized;
		private bool shouldResize;
		private bool resolutionResolved;
		private VideoApi.GetProcAddressCallback getProcAddressDelegate;

		private string loopOption = "inf";
		private bool isPaused;

		public void Pause()
		{
			if (initialized && !isPaused)
			{
				VideoApi.mpv_set_option_string(mpvHandle, "pause", "yes");
				isPaused = true;
			}
		}

		public void Resume()
		{
			if (initialized && isPaused)
			{
				VideoApi.mpv_set_option_string(mpvHandle, "pause", "no");
				isPaused = false;
			}
		}

		public VideoTexture(string videoPath, int width = 512, int height = 512, string queryString = "")
		{
			VideoPath = videoPath;
			Width = width;
			Height = height;

			if (!string.IsNullOrEmpty(queryString))
			{
				string[] pairs = queryString.Split('&');
				foreach (string pair in pairs)
				{
					string[] kv = pair.Split('=');
					if (kv.Length == 2)
					{
						string key = kv[0].Trim().ToLowerInvariant();
						string val = kv[1].Trim().ToLowerInvariant();
						if (key == "loop")
						{
							if (val == "no" || val == "false" || val == "0")
							{
								loopOption = "no";
							}
							else
							{
								loopOption = val;
							}
						}
					}
				}
			}
		}

		public void Initialize()
		{
			if (initialized) return;

			try
			{
				mpvHandle = VideoApi.mpv_create();
				if (mpvHandle == IntPtr.Zero) return;

				// Loop video and optimize caching
				VideoApi.mpv_set_option_string(mpvHandle, "loop-file", loopOption);
				VideoApi.mpv_set_option_string(mpvHandle, "keep-open", "yes");
				VideoApi.mpv_set_option_string(mpvHandle, "hwdec", "auto-safe");
				VideoApi.mpv_set_option_string(mpvHandle, "vo", "libmpv");
				VideoApi.mpv_set_option_string(mpvHandle, "ytdl", "no");
				VideoApi.mpv_set_option_string(mpvHandle, "load-scripts", "no");
				VideoApi.mpv_set_option_string(mpvHandle, "aid", "no");
				VideoApi.mpv_set_option_string(mpvHandle, "vd-lavc-skiploopfilter", "all");
				VideoApi.mpv_set_option_string(mpvHandle, "vd-lavc-fast", "yes");
				VideoApi.mpv_set_option_string(mpvHandle, "vd-lavc-threads", "2");
				VideoApi.mpv_set_option_string(mpvHandle, "demuxer-max-bytes", "10485760");
				VideoApi.mpv_set_option_string(mpvHandle, "demuxer-max-back-bytes", "0");

				if (VideoApi.mpv_initialize(mpvHandle) < 0)
				{
					Dispose();
					return;
				}

				// Setup OpenGL context sharing
				getProcAddressDelegate = VideoApi.GetProcAddressOpenGL;
				IntPtr getProcAddressPtr = Marshal.GetFunctionPointerForDelegate(getProcAddressDelegate);

				VideoApi.mpv_opengl_init_params glInitParams = new VideoApi.mpv_opengl_init_params
				{
					get_proc_address = getProcAddressPtr,
					get_proc_address_ctx = IntPtr.Zero,
					extra_fields = IntPtr.Zero
				};

				IntPtr glInitParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(glInitParams));
				Marshal.StructureToPtr(glInitParams, glInitParamsPtr, false);

				VideoApi.mpv_render_param[] renderParams = new VideoApi.mpv_render_param[]
				{
					new VideoApi.mpv_render_param { type = VideoApi.MPV_RENDER_PARAM_API_TYPE, data = Marshal.StringToHGlobalAnsi(VideoApi.MPV_RENDER_API_TYPE_OPENGL) },
					new VideoApi.mpv_render_param { type = VideoApi.MPV_RENDER_PARAM_OPENGL_INIT_PARAMS, data = glInitParamsPtr },
					new VideoApi.mpv_render_param { type = VideoApi.MPV_RENDER_PARAM_TYPE_INVALID, data = IntPtr.Zero }
				};

				if (VideoApi.mpv_render_context_create(out renderContext, mpvHandle, renderParams) < 0)
				{
					Marshal.FreeHGlobal(renderParams[0].data);
					Marshal.FreeHGlobal(glInitParamsPtr);
					Dispose();
					return;
				}

				Marshal.FreeHGlobal(renderParams[0].data);
				Marshal.FreeHGlobal(glInitParamsPtr);

				// Load video
				string commandString = "loadfile";
				IntPtr cmdStringPtr = Marshal.StringToHGlobalAnsi(commandString);
				IntPtr pathPtr = Marshal.StringToHGlobalAnsi(VideoPath);
				IntPtr[] args = new IntPtr[] { cmdStringPtr, pathPtr, IntPtr.Zero };
				VideoApi.mpv_command(mpvHandle, args);
				Marshal.FreeHGlobal(cmdStringPtr);
				Marshal.FreeHGlobal(pathPtr);

				initialized = true;
			}
			catch
			{
				Dispose();
			}
		}

		private void UpdateResolution()
		{
			if (mpvHandle == IntPtr.Zero || resolutionResolved) return;

			IntPtr wPtr = VideoApi.mpv_get_property_string(mpvHandle, "video-params/w");
			IntPtr hPtr = VideoApi.mpv_get_property_string(mpvHandle, "video-params/h");

			if (wPtr != IntPtr.Zero && hPtr != IntPtr.Zero)
			{
				string wStr = Marshal.PtrToStringAnsi(wPtr);
				string hStr = Marshal.PtrToStringAnsi(hPtr);

				if (int.TryParse(wStr, out int w) && int.TryParse(hStr, out int h))
				{
					if (w > 0 && h > 0)
					{
						Width = w;
						Height = h;
						shouldResize = true;
						resolutionResolved = true;
					}
				}
			}

			if (wPtr != IntPtr.Zero) VideoApi.mpv_free(wPtr);
			if (hPtr != IntPtr.Zero) VideoApi.mpv_free(hPtr);
		}

		private int lastRenderTicks = -1;

		public void Render(int glTextureId, int currentTicks)
		{
			if (!initialized) return;
			Resume();
			
			// Limit video rendering to ~30 FPS (33ms) to prevent FPS drops
			int elapsed = currentTicks - lastRenderTicks;
			if (lastRenderTicks != -1 && elapsed >= 0 && elapsed < 33) return;

			// Check if new frame is actually available to render
			ulong flags = VideoApi.mpv_render_context_update(renderContext);
			if ((flags & 1) == 0) // MPV_RENDER_UPDATE_FRAME = 1 << 0
			{
				return;
			}
			lastRenderTicks = currentTicks;

			try
			{
				TextureId = glTextureId;

				UpdateResolution();

				if (shouldResize)
				{
					GL.BindTexture(TextureTarget.Texture2D, glTextureId);
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
					shouldResize = false;
				}

				if (fboId == 0)
				{
					GL.GenFramebuffers(1, out fboId);
				}

				// Backup OpenGL state
				int prevFbo;
				GL.GetInteger(GetPName.FramebufferBinding, out prevFbo);

				int prevVAO, prevVBO, prevEBO, prevProgram, prevTexture, prevActiveTexture;
				GL.GetInteger(GetPName.VertexArrayBinding, out prevVAO);
				GL.GetInteger(GetPName.ArrayBufferBinding, out prevVBO);
				GL.GetInteger(GetPName.ElementArrayBufferBinding, out prevEBO);
				GL.GetInteger(GetPName.CurrentProgram, out prevProgram);
				GL.GetInteger(GetPName.TextureBinding2D, out prevTexture);
				GL.GetInteger(GetPName.ActiveTexture, out prevActiveTexture);

				int[] prevViewport = new int[4];
				GL.GetInteger(GetPName.Viewport, prevViewport);

				bool prevScissor = GL.IsEnabled(EnableCap.ScissorTest);
				int[] prevScissorBox = new int[4];
				GL.GetInteger(GetPName.ScissorBox, prevScissorBox);

				bool prevDepth = GL.IsEnabled(EnableCap.DepthTest);
				bool prevBlend = GL.IsEnabled(EnableCap.Blend);
				bool prevCull = GL.IsEnabled(EnableCap.CullFace);

				// Backup write masks
				bool[] prevColorMask = new bool[4];
				GL.GetBoolean(GetPName.ColorWritemask, prevColorMask);
				bool prevDepthMask;
				GL.GetBoolean(GetPName.DepthWritemask, out prevDepthMask);

				// Backup stencil test
				bool prevStencil = GL.IsEnabled(EnableCap.StencilTest);

				// Backup blend equation and functions
				int prevBlendSrcRgb, prevBlendDstRgb, prevBlendSrcAlpha, prevBlendDstAlpha;
				GL.GetInteger(GetPName.BlendSrcRgb, out prevBlendSrcRgb);
				GL.GetInteger(GetPName.BlendDstRgb, out prevBlendDstRgb);
				GL.GetInteger(GetPName.BlendSrcAlpha, out prevBlendSrcAlpha);
				GL.GetInteger(GetPName.BlendDstAlpha, out prevBlendDstAlpha);
				int prevBlendEqRgb, prevBlendEqAlpha;
				GL.GetInteger(GetPName.BlendEquationRgb, out prevBlendEqRgb);
				GL.GetInteger(GetPName.BlendEquationAlpha, out prevBlendEqAlpha);

				// Backup clear color
				float[] prevClearColor = new float[4];
				GL.GetFloat(GetPName.ColorClearValue, prevClearColor);

				// Bind FBO and texture
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboId);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureId, 0);

				// Set viewport for FBO size
				GL.Viewport(0, 0, Width, Height);
				GL.Disable(EnableCap.ScissorTest);

				mpv_opengl_fbo fboParam = new mpv_opengl_fbo
				{
					fbo = fboId,
					w = Width,
					h = Height,
					internal_format = 0x8058 // GL_RGBA8
				};

				IntPtr fboParamPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fboParam));
				Marshal.StructureToPtr(fboParam, fboParamPtr, false);

				VideoApi.mpv_render_param[] renderParams = new VideoApi.mpv_render_param[]
				{
					new VideoApi.mpv_render_param { type = 3, data = fboParamPtr }, // MPV_RENDER_PARAM_OPENGL_FBO = 3
					new VideoApi.mpv_render_param { type = 0, data = IntPtr.Zero }
				};

				VideoApi.mpv_render_context_render(renderContext, renderParams);

				Marshal.FreeHGlobal(fboParamPtr);

				// Restore OpenGL state
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFbo);
				GL.UseProgram(prevProgram);
				GL.BindVertexArray(prevVAO);
				GL.BindBuffer(BufferTarget.ArrayBuffer, prevVBO);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, prevEBO);
				GL.ActiveTexture((TextureUnit)prevActiveTexture);
				GL.BindTexture(TextureTarget.Texture2D, prevTexture);

				GL.Viewport(prevViewport[0], prevViewport[1], prevViewport[2], prevViewport[3]);
				if (prevScissor) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
				GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
				if (prevDepth) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
				if (prevBlend) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
				if (prevCull) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);

				GL.ColorMask(prevColorMask[0], prevColorMask[1], prevColorMask[2], prevColorMask[3]);
				GL.DepthMask(prevDepthMask);
				if (prevStencil) GL.Enable(EnableCap.StencilTest); else GL.Disable(EnableCap.StencilTest);
				
				// Safely restore blend functions and equations
				GL.BlendFuncSeparate((BlendingFactorSrc)prevBlendSrcRgb, (BlendingFactorDest)prevBlendDstRgb, (BlendingFactorSrc)prevBlendSrcAlpha, (BlendingFactorDest)prevBlendDstAlpha);
				GL.BlendEquationSeparate((BlendEquationMode)prevBlendEqRgb, (BlendEquationMode)prevBlendEqAlpha);

				// Restore clear color
				GL.ClearColor(prevClearColor[0], prevClearColor[1], prevClearColor[2], prevClearColor[3]);
			}
			catch
			{
				// Ignore render errors to avoid crash
			}
		}

		public void Dispose()
		{
			initialized = false;
			if (renderContext != IntPtr.Zero)
			{
				VideoApi.mpv_render_context_free(renderContext);
				renderContext = IntPtr.Zero;
			}
			if (mpvHandle != IntPtr.Zero)
			{
				VideoApi.mpv_terminate_destroy(mpvHandle);
				mpvHandle = IntPtr.Zero;
			}
			if (fboId != 0)
			{
				GL.DeleteFramebuffers(1, ref fboId);
				fboId = 0;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct mpv_opengl_fbo
	{
		public int fbo;
		public int w;
		public int h;
		public int internal_format;
	}
}
