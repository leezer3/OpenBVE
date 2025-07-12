using FontStashSharp.Interfaces;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;


namespace LibRender2.Text
{
	internal class FontStashTextureManager : ITexture2DManager
	{
		public object CreateTexture(int width, int height) => new FontStashTexture(width, height);

		public Point GetTextureSize(object texture)
		{
			var t = (FontStashTexture)texture;
			return new Point(t.Width, t.Height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{
			var t = (FontStashTexture)texture;
			t.SetData(bounds, data);
		}
	}

	public unsafe class FontStashTexture : IDisposable
	{
		private readonly int _handle;

		public readonly int Width;
		public readonly int Height;

		public FontStashTexture(int width, int height)
		{
			Width = width;
			Height = height;

			_handle = GL.GenTexture();

			Bind();

			//Reserve enough memory from the gpu for the whole image
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);


			SetParameters();
		}

		private void SetParameters()
		{
			//Setting some texture perameters so the texture behaves as expected.
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);


			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);


			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);


			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);


			//Generating mipmaps.
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

		}

		public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
		{
			//When we bind a texture we can choose which textureslot we can bind it to.
			GL.ActiveTexture(textureSlot);


			GL.BindTexture(TextureTarget.Texture2D, _handle);

		}

		public void Dispose()
		{
			//In order to dispose we need to delete the opengl handle for the texure.
			GL.DeleteTexture(_handle);

		}

		public void SetData(Rectangle bounds, byte[] data)
		{
			Bind();
			fixed (byte* ptr = data)
			{
				GL.TexSubImage2D(
					target: TextureTarget.Texture2D,
					level: 0,
					xoffset: bounds.Left,
					yoffset: bounds.Top,
					width: bounds.Width,
					height: bounds.Height,
					format: PixelFormat.Rgba,
					type: PixelType.UnsignedByte,
					pixels: new IntPtr(ptr)
				);
			}
		}
	}
}
