using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenBveApi.Textures;
using OpenBveApi.Hosts;

namespace Plugin {
	public partial class Plugin : TextureInterface {
		
		/// <summary>Loads a texture from the specified file.</summary>
		/// <param name="file">The file that holds the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		internal bool Parse(string file, out Texture texture) {
			/*
			 * Read the bitmap. This will be a bitmap of just
			 * any format, not necessarily the one that allows
			 * us to extract the bitmap data easily.
			 * */
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(file);
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			/* 
			 * If the bitmap format is not already 32-bit BGRA,
			 * then convert it to 32-bit BGRA.
			 * */
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb) {
				Bitmap compatibleBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
				Graphics graphics = Graphics.FromImage(compatibleBitmap);
				graphics.DrawImage(bitmap, rect, rect, GraphicsUnit.Pixel);
				graphics.Dispose();
				bitmap.Dispose();
				bitmap = compatibleBitmap;
			}
			/*
			 * Extract the raw bitmap data.
			 * */
			BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			if (data.Stride == 4 * data.Width) {
				/*
				 * Copy the data from the bitmap
				 * to the array in BGRA format.
				 * */
				byte[] raw = new byte[data.Stride * data.Height];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
				int width = bitmap.Width;
				int height = bitmap.Height;
				bitmap.Dispose();
				/*
				 * Change the byte order from BGRA to RGBA.
				 * */
				for (int i = 0; i < raw.Length; i += 4) {
					byte temp = raw[i];
					raw[i] = raw[i + 2];
					raw[i + 2] = temp;
				}
				texture = new Texture(width, height, 32, raw);
				return true;
			} else {
				/*
				 * The stride is invalid. This indicates that the
				 * CLI either does not implement the conversion to
				 * 32-bit BGRA correctly, or that the CLI has
				 * applied additional padding that we do not
				 * support.
				 * */
				bitmap.UnlockBits(data);
				bitmap.Dispose();
				CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Invalid stride encountered.");
				texture = null;
				return false;
			}
		}
		
	}
}