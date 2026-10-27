using System;
using System.Drawing;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using LibRender2.Shaders;
using OpenBveApi.Math;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace LibRender2.Text
{
	internal sealed class FontStashRenderer : IFontStashRenderer2, IDisposable
	{
		private const int MaxSprites = 2048;
		private const int MaxVertices = MaxSprites * 4;
		private const int MaxIndices = MaxSprites * 6;

		private readonly BaseRenderer renderer;
		private readonly Shader shader;
		private readonly int vertexBuffer;
		private readonly int indexBuffer;
		private readonly int vertexArray;
		private readonly int vertexStride;
		private readonly VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[MaxVertices];
		private readonly ushort[] indexData = new ushort[MaxIndices];
		private readonly FontStashTextureManager textureManager = new FontStashTextureManager();
		private FontStashTexture lastTexture;
		private int vertexIndex;
		private bool disposed;

		public ITexture2DManager TextureManager => textureManager;

		internal FontStashRenderer(BaseRenderer renderer)
		{
			this.renderer = renderer;
			vertexStride = Marshal.SizeOf(typeof(VertexPositionColorTexture));

			for (int i = 0, j = 0; i < MaxIndices; i += 6, j += 4)
			{
				indexData[i] = (ushort)j;
				indexData[i + 1] = (ushort)(j + 1);
				indexData[i + 2] = (ushort)(j + 2);
				indexData[i + 3] = (ushort)(j + 3);
				indexData[i + 4] = (ushort)(j + 2);
				indexData[i + 5] = (ushort)(j + 1);
			}

			shader = new Shader(renderer, "fontstash", "fontstash", true);

			GL.GenBuffers(1, out vertexBuffer);
			GL.GenBuffers(1, out indexBuffer);
			GL.GenVertexArrays(1, out vertexArray);

			GL.BindVertexArray(vertexArray);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(MaxVertices * vertexStride), IntPtr.Zero, BufferUsageHint.StreamDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
			GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(MaxIndices * sizeof(ushort)), indexData, BufferUsageHint.StaticDraw);

			int location = GL.GetAttribLocation(shader.Handle, "iPosition");
			GL.EnableVertexAttribArray(location);
			GL.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, vertexStride, IntPtr.Zero);

			location = GL.GetAttribLocation(shader.Handle, "iColor");
			GL.EnableVertexAttribArray(location);
			GL.VertexAttribPointer(location, 4, VertexAttribPointerType.UnsignedByte, true, vertexStride, new IntPtr(12));

			location = GL.GetAttribLocation(shader.Handle, "iUv");
			GL.EnableVertexAttribArray(location);
			GL.VertexAttribPointer(location, 2, VertexAttribPointerType.Float, false, vertexStride, new IntPtr(16));
		}

		internal void Begin()
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			shader.Activate();
			shader.SetTexture(0);

			Matrix4D.CreateOrthographicOffCenter(0.0f, renderer.Screen.Width, renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Matrix4D projectionMatrix);
			shader.SetCurrentProjectionMatrix(projectionMatrix);
			shader.SetCurrentModelViewMatrix(Matrix4D.Identity);

			GL.BindVertexArray(vertexArray);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

			renderer.LastBoundTexture = null;
		}

		public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
		{
			FontStashTexture fontTexture = (FontStashTexture)texture;

			if (lastTexture != null && lastTexture != fontTexture)
			{
				FlushBuffer();
			}

			if (vertexIndex + 4 > MaxVertices)
			{
				FlushBuffer();
			}

			vertexData[vertexIndex++] = topLeft;
			vertexData[vertexIndex++] = topRight;
			vertexData[vertexIndex++] = bottomLeft;
			vertexData[vertexIndex++] = bottomRight;
			lastTexture = fontTexture;
		}

		internal void End()
		{
			FlushBuffer();
			renderer.LastBoundTexture = null;
		}

		private void FlushBuffer()
		{
			if (vertexIndex == 0 || lastTexture == null)
			{
				return;
			}

			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
			GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, new IntPtr(vertexIndex * vertexStride), vertexData);

			lastTexture.Bind();
			GL.DrawElements(PrimitiveType.Triangles, vertexIndex * 6 / 4, DrawElementsType.UnsignedShort, IntPtr.Zero);
			vertexIndex = 0;
		}

		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			GL.DeleteVertexArray(vertexArray);
			GL.DeleteBuffer(vertexBuffer);
			GL.DeleteBuffer(indexBuffer);
			shader.Dispose();
			disposed = true;
		}
	}

	internal sealed class FontStashTextureManager : ITexture2DManager
	{
		public object CreateTexture(int width, int height)
		{
			return new FontStashTexture(width, height);
		}

		public Point GetTextureSize(object texture)
		{
			FontStashTexture fontTexture = (FontStashTexture)texture;
			return new Point(fontTexture.Width, fontTexture.Height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{
			((FontStashTexture)texture).SetData(bounds, data);
		}
	}

	internal sealed class FontStashTexture : IDisposable
	{
		private readonly int handle;

		internal readonly int Width;
		internal readonly int Height;

		internal FontStashTexture(int width, int height)
		{
			Width = width;
			Height = height;

			GL.GenTextures(1, out handle);
			Bind();
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		}

		internal void Bind()
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, handle);
		}

		internal void SetData(Rectangle bounds, byte[] data)
		{
			Bind();
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, bounds.Left, bounds.Top, bounds.Width, bounds.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
		}

		public void Dispose()
		{
			GL.DeleteTexture(handle);
		}
	}
}
