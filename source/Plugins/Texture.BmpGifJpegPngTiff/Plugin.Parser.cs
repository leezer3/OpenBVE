using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using OpenBveApi.Textures;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using Plugin.BMP;
using Plugin.GIF;
using Plugin.PNG;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Plugin {
	public partial class Plugin {
		
		/// <summary>Loads a texture from the specified file.</summary>
		/// <param name="file">The file that holds the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		private bool Parse(string file, out Texture texture) 
		{
			/*
			 * First, check if our file is a GIF by
			 * reading the header bytes to check the signature
			 *
			 * If true, pass to the dedicated GIF decoder to handle
			 * animations etc.
			 */
			try
			{
				using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
				{
					byte[] buffer = new byte[6];
					if (fs.Length > buffer.Length)
					{
						// ReSharper disable once MustUseReturnValue
						fs.Read(buffer, 0, buffer.Length);
					}
					fs.Close();
					if (buffer.SequenceEqual(GifDecoder.GIF87Header) || buffer.SequenceEqual(GifDecoder.GIF89Header))
					{
						using (GifDecoder decoder = new GifDecoder())
						{
							decoder.Read(file);
							int frameCount = decoder.GetFrameCount();
							int duration = 0;
							if (frameCount != 1)
							{
								Vector2 frameSize = decoder.GetFrameSize();
								byte[][] frameBytes = new byte[frameCount][];
								for (int i = 0; i < frameCount; i++)
								{
									int[] framePixels = decoder.GetFrame(i);
									frameBytes[i] = new byte[framePixels.Length * sizeof(int)];
									Buffer.BlockCopy(framePixels, 0, frameBytes[i], 0, frameBytes[i].Length);
									duration += decoder.GetDuration(i);
								}
								texture = new Texture((int)frameSize.X, (int)frameSize.Y, OpenBveApi.Textures.PixelFormat.RGBAlpha, frameBytes, ((double)duration / frameCount) / 10000000.0);
								return true;
							}
						}
						
					}
					
					if (Encoding.ASCII.GetString(buffer, 0, 2) == "BM")
					{
						using (BmpDecoder decoder = new BmpDecoder())
						{
							if (decoder.Read(file))
							{
								texture = new Texture(decoder.Width, decoder.Height, OpenBveApi.Textures.PixelFormat.RGBAlpha, decoder.ImageData, decoder.ColorTable);
								return true;
							}
						}
					}

					if (Encoding.ASCII.GetString(buffer, 1, 3) == "PNG" && !CurrentOptions.UseGDIDecoders)
					{
						// NB: GDI+ decoders are curerntly enabled by default as they are marginally faster (~10ms or so per texture unless massively interlaced which is worse)
						//     If / when mobile device support is added, these will likely be removed
						using (PngDecoder decoder = new PngDecoder())
						{
							if (decoder.Read(file))
							{
								texture = new Texture(decoder.Width, decoder.Height, (OpenBveApi.Textures.PixelFormat)decoder.BytesPerPixel, decoder.pixelBuffer, null);
								return true;
							}
						}
					}
				}
			}
			catch
			{
				texture = null;
				return false;
			}
			/*
			 * Otherwise, read the bitmap. This will be a bitmap of just
			 * any format, not necessarily the one that allows
			 * us to extract the bitmap data easily.
			 */
			using (Bitmap bitmap = new Bitmap(file))
			{
				int width, height;
				byte[] raw = GetRawBitmapData(bitmap, out width, out height);
				if (raw != null)
				{
					texture = new Texture(width, height, OpenBveApi.Textures.PixelFormat.RGBAlpha, raw, null);
					return true;
				}
				texture = null;
				return false;
			}
			
		}

		private byte[] GetRawBitmapData(Bitmap bitmap, out int width, out int height)
		{
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			/* 
			 * If the bitmap format is not already 32-bit BGRA,
			 * then convert it to 32-bit BGRA.
			 */
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
			 */
			BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			if (data.Stride == 4 * data.Width) {
				/*
				 * Copy the data from the bitmap
				 * to the array in BGRA format.
				 */
				byte[] raw = new byte[data.Stride * data.Height];
				System.Runtime.InteropServices.Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
				width = bitmap.Width;
				height = bitmap.Height;
				
				/*
				 * Change the byte order from BGRA to RGBA.
				 */
				for (int i = 0; i < raw.Length; i += 4) {
					byte temp = raw[i];
					raw[i] = raw[i + 2];
					raw[i + 2] = temp;
				}

				return raw;
			}

			/*
			 * The stride is invalid. This indicates that the
			 * CLI either does not implement the conversion to
			 * 32-bit BGRA correctly, or that the CLI has
			 * applied additional padding that we do not
			 * support.
			 */
			bitmap.UnlockBits(data);
			bitmap.Dispose();
			CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Invalid stride encountered.");
			width = 0;
			height = 0;
			return null;
		}
		
	}
}
