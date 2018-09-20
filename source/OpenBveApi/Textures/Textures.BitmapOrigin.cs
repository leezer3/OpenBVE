using System.Drawing;
using System.Drawing.Imaging;
using OpenBveApi.Colors;

namespace OpenBveApi.Textures
{
	/// <summary>Represents a System.Drawing.Bitmap where the texture can be loaded from.</summary>
	public class BitmapOrigin : TextureOrigin
	{
		/// <summary>The bitmap.</summary>
		public readonly Bitmap Bitmap;
		/// <summary>The texture parameters to be applied when loading the texture to OpenGL</summary>
		public readonly TextureParameters Parameters;

		// --- constructors ---
		/// <summary>Creates a new bitmap origin.</summary>
		/// <param name="bitmap">The bitmap.</param>
		public BitmapOrigin(Bitmap bitmap)
		{
			this.Bitmap = bitmap;
		}

		/// <summary>Creates a new bitmap origin.</summary>
		/// <param name="bitmap">The bitmap.</param>
		/// <param name="parameters">The texture parameters</param>
		public BitmapOrigin(Bitmap bitmap, TextureParameters parameters)
		{
			this.Bitmap = bitmap;
			this.Parameters = parameters;
		}

		// --- functions ---
		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public override bool GetTexture(out Texture texture)
		{
			Bitmap bitmap = this.Bitmap;
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			/* 
			 * If the bitmap format is not already 32-bit BGRA,
			 * then convert it to 32-bit BGRA.
			 * */
			Color24[] p = null;
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb && bitmap.PixelFormat != PixelFormat.Format24bppRgb)
			{
				/* Only store the color palette data for
				 * textures using a restricted palette
				 * With a large number of textures loaded at
				 * once, this can save a decent chunk of memory
				 * */
				p = new Color24[bitmap.Palette.Entries.Length];
				for (int i = 0; i < bitmap.Palette.Entries.Length; i++)
				{
					p[i] = bitmap.Palette.Entries[i];
				}
			}

			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				Bitmap compatibleBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(compatibleBitmap);
				graphics.DrawImage(bitmap, rect, rect, GraphicsUnit.Pixel);
				graphics.Dispose();
				bitmap = compatibleBitmap;
			}

			/*
			 * Extract the raw bitmap data.
			 * */
			BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			if (data.Stride == 4 * data.Width)
			{
				/*
				 * Copy the data from the bitmap
				 * to the array in BGRA format.
				 * */
				byte[] raw = new byte[data.Stride * data.Height];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
				int width = bitmap.Width;
				int height = bitmap.Height;
				/*
				 * Change the byte order from BGRA to RGBA.
				 * */
				for (int i = 0; i < raw.Length; i += 4)
				{
					byte temp = raw[i];
					raw[i] = raw[i + 2];
					raw[i + 2] = temp;
				}

				texture = new Texture(width, height, 32, raw, p);
				texture = texture.ApplyParameters(this.Parameters);
				return true;
			}

			/*
				   * The stride is invalid. This indicates that the
				   * CLI either does not implement the conversion to
				   * 32-bit BGRA correctly, or that the CLI has
				   * applied additional padding that we do not
				   * support.
				   * */
			bitmap.UnlockBits(data);
			texture = null;
			return false;
		}
	}
}
