using FontStashSharp.Interfaces;
using LibRender2.Shaders;
using OpenTK.Graphics.OpenGL;
using System;
using OpenBveApi.Math;

namespace LibRender2.Text
{
	internal class FontStashRenderer : IFontStashRenderer2, IDisposable
	{
		private const int MAX_SPRITES = 2048;
		private const int MAX_VERTICES = MAX_SPRITES * 4;
		private const int MAX_INDICES = MAX_SPRITES * 6;
		private BaseRenderer renderer;

		private readonly Shader shader;
		private readonly BufferObject<VertexPositionColorTexture> vertexBuffer;
		private readonly BufferObject<ushort> indexBuffer;
		private readonly FontStashVAO vao;
		private readonly VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[MAX_VERTICES];
		private FontStashTexture lastTexture;
		private int vertexIndex = 0;

		private readonly FontStashTextureManager textureManager;

		public ITexture2DManager TextureManager => textureManager;

		private static readonly ushort[] indexData = GenerateIndexArray();

		public unsafe FontStashRenderer(BaseRenderer rendererReference)
		{
			renderer = rendererReference;
			textureManager = new FontStashTextureManager();
			vertexBuffer = new BufferObject<VertexPositionColorTexture>(MAX_VERTICES, BufferTarget.ArrayBuffer, true);

			indexBuffer = new BufferObject<ushort>(indexData.Length, BufferTarget.ElementArrayBuffer, false);
			indexBuffer.SetData(indexData, 0, indexData.Length);

			shader = new Shader(renderer, "text", "text", true);
			shader.Activate();
			vao = new FontStashVAO(sizeof(VertexPositionColorTexture));
			vao.Bind();

			var location = shader.GetAttribLocation("a_position");
			vao.VertexAttribPointer(location, 3, VertexAttribPointerType.Double, false, 0);

			location = shader.GetAttribLocation("a_color");
			vao.VertexAttribPointer(location, 4, VertexAttribPointerType.UnsignedByte, true, 24);

			location = shader.GetAttribLocation("a_texCoords0");
			vao.VertexAttribPointer(location, 2, VertexAttribPointerType.Double, false, 28);
		}

		~FontStashRenderer() => Dispose(false);
		public void Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			vao.Dispose();
			vertexBuffer.Dispose();
			indexBuffer.Dispose();
			shader.Dispose();
		}

		public void Begin()
		{
			GL.Disable(EnableCap.DepthTest);

			GL.Enable(EnableCap.Blend);

			GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);


			shader.Activate();
			shader.SetUniform("TextureSampler", 0);

			Matrix4D.CreateOrthographicOffCenter(0, renderer.Screen.Width, renderer.Screen.Height, 0, 0, -1, out var transform);
			shader.SetUniform("MatrixTransform", transform);

			vao.Bind();
			indexBuffer.Bind();
			vertexBuffer.Bind();
		}

		public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
		{
			if (lastTexture != (FontStashTexture)texture)
			{
				FlushBuffer();
			}

			vertexData[vertexIndex++] = topLeft;
			vertexData[vertexIndex++] = topRight;
			vertexData[vertexIndex++] = bottomLeft;
			vertexData[vertexIndex++] = bottomRight;
			lastTexture = (FontStashTexture)texture;
		}

		public void End()
		{
			FlushBuffer();
		}

		private unsafe void FlushBuffer()
		{
			if (vertexIndex == 0 || lastTexture == null)
			{
				return;
			}
			vertexBuffer.Bind();
			vertexBuffer.SetData(vertexData, 0, vertexIndex);

			lastTexture.Bind();

			GL.DrawElements(PrimitiveType.Triangles, vertexIndex * 6 / 4, DrawElementsType.UnsignedShort, IntPtr.Zero);
			vertexIndex = 0;
		}

		private static ushort[] GenerateIndexArray()
		{
			ushort[] result = new ushort[MAX_INDICES];
			for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
			{
				result[i] = (ushort)(j);
				result[i + 1] = (ushort)(j + 1);
				result[i + 2] = (ushort)(j + 2);
				result[i + 3] = (ushort)(j + 3);
				result[i + 4] = (ushort)(j + 2);
				result[i + 5] = (ushort)(j + 1);
			}
			return result;
		}
	}
}
