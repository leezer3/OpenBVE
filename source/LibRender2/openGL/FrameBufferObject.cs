using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace LibRender2
{
	public class FrameBufferObject : IDisposable
	{
		public static List<FrameBufferObject> Disposable = new List<FrameBufferObject>();

		private readonly int handle;
		private readonly Dictionary<int, I2dPixelArray> colorBuffers;
		private I2dPixelArray depthBuffer;
		private I2dPixelArray stencilBuffer;
		private bool disposed;

		public FrameBufferObject()
		{
			GL.GenFramebuffers(1, out handle);
			colorBuffers = new Dictionary<int, I2dPixelArray>();

			Disposable.Add(this);
		}

		public void Bind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
		}

		public void SetTextureBuffer(TargetBuffer Target, PixelInternalFormat InternalFormat, PixelFormat Format, PixelType Type, int Width, int Height, int Number = 0)
		{
			TextureBuffer buffer = new TextureBuffer(InternalFormat, Format, Type, Width, Height);

			switch (Target)
			{
				case TargetBuffer.Color:
					if (colorBuffers.ContainsKey(Number))
					{
						UnBind();
						colorBuffers[Number].Dispose();
						Bind();
					}
					colorBuffers[Number] = buffer;
					GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, (FramebufferAttachment)((int)FramebufferAttachment.ColorAttachment0 + Number), TextureTarget.Texture2D, buffer.handle, 0);
					break;
				case TargetBuffer.Depth:
					if (depthBuffer != null)
					{
						UnBind();
						depthBuffer.Dispose();
						Bind();
					}
					depthBuffer = buffer;
					GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, buffer.handle, 0);
					break;
				case TargetBuffer.Stencil:
					if (stencilBuffer != null)
					{
						UnBind();
						stencilBuffer.Dispose();
						Bind();
					}
					stencilBuffer = buffer;
					GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, TextureTarget.Texture2D, buffer.handle, 0);
					break;
			}
		}

		public void SetRenderBuffer(TargetBuffer Target, RenderbufferStorage Format, int Width, int Height, int Number = 0)
		{
			RenderBuffer buffer = new RenderBuffer(Format, Width, Height);

			switch (Target)
			{
				case TargetBuffer.Color:
					if (colorBuffers.ContainsKey(Number))
					{
						UnBind();
						colorBuffers[Number].Dispose();
						Bind();
					}
					colorBuffers[Number] = buffer;
					GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferAttachment)((int)FramebufferAttachment.ColorAttachment0 + Number), RenderbufferTarget.Renderbuffer, buffer.handle);
					break;
				case TargetBuffer.Depth:
					if (depthBuffer != null)
					{
						UnBind();
						depthBuffer.Dispose();
						Bind();
					}
					depthBuffer = buffer;
					GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, buffer.handle);
					break;
				case TargetBuffer.Stencil:
					if (stencilBuffer != null)
					{
						UnBind();
						stencilBuffer.Dispose();
						Bind();
					}
					stencilBuffer = buffer;
					GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, buffer.handle);
					break;
			}
		}

		public void DrawBuffers(DrawBuffersEnum[] Targets)
		{
			GL.DrawBuffers(Targets.Length, Targets);
		}

		public void UnBind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public void BindBuffer(TargetBuffer Target, int Number = 0)
		{
			switch (Target)
			{
				case TargetBuffer.Color:
					colorBuffers[Number].Bind();
					break;
				case TargetBuffer.Depth:
					depthBuffer.Bind();
					break;
				case TargetBuffer.Stencil:
					stencilBuffer.Bind();
					break;
			}
		}

		public void UnBindBuffer(TargetBuffer Target, int Number = 0)
		{
			switch (Target)
			{
				case TargetBuffer.Color:
					colorBuffers[Number].UnBind();
					break;
				case TargetBuffer.Depth:
					depthBuffer.UnBind();
					break;
				case TargetBuffer.Stencil:
					stencilBuffer.UnBind();
					break;
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				stencilBuffer?.Dispose();

				depthBuffer?.Dispose();

				foreach (KeyValuePair<int, I2dPixelArray> buffer in colorBuffers)
				{
					buffer.Value?.Dispose();
				}

				GL.DeleteFramebuffer(handle);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}

		public enum TargetBuffer
		{
			Color,
			Depth,
			Stencil
		}

		private abstract class I2dPixelArray : IDisposable
		{
			protected bool disposed;
			internal abstract void Bind();
			internal abstract void UnBind();
			public abstract void Dispose();
		}

		private class TextureBuffer : I2dPixelArray
		{
			internal readonly int handle;

			internal TextureBuffer(PixelInternalFormat InternalFormat, PixelFormat Format, PixelType Type, int Width, int Height)
			{
				GL.GenTextures(1, out handle);
				GL.BindTexture(TextureTarget.Texture2D, handle);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
				GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, Type, IntPtr.Zero);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}

			internal override void Bind()
			{
				GL.BindTexture(TextureTarget.Texture2D, handle);
			}

			internal override void UnBind()
			{
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}

			public override void Dispose()
			{
				if (!disposed)
				{
					GL.DeleteTexture(handle);
					GC.SuppressFinalize(this);
					disposed = true;
				}
			}
		}

		private class RenderBuffer : I2dPixelArray
		{
			internal readonly int handle;

			internal RenderBuffer(RenderbufferStorage Format, int Width, int Height)
			{
				GL.GenRenderbuffers(1, out handle);
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handle);
				GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Format, Width, Height);
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
			}

			internal override void Bind()
			{
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handle);
			}

			internal override void UnBind()
			{
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
			}

			public override void Dispose()
			{
				if (!disposed)
				{
					GL.DeleteRenderbuffer(handle);
					GC.SuppressFinalize(this);
					disposed = true;
				}
			}
		}
	}
}
