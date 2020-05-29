using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using InterpolationMode = OpenBveApi.Graphics.InterpolationMode;

namespace LibRender2.Textures
{
	/// <summary>Provides functions for dealing with textures.</summary>
	public class TextureManager
	{
		private readonly HostInterface currentHost;

		private readonly BaseRenderer renderer;

		/// <summary>Holds all currently registered textures.</summary>
		public Texture[] RegisteredTextures;

		private Dictionary<TextureOrigin, Texture> animatedTextures;

		/// <summary>The number of currently registered textures.</summary>
		public int RegisteredTexturesCount;

		internal TextureManager(HostInterface CurrentHost, BaseRenderer Renderer)
		{
			currentHost = CurrentHost;
			RegisteredTextures = new Texture[16];
			RegisteredTexturesCount = 0;
			renderer = Renderer;
			animatedTextures = new Dictionary<TextureOrigin, Texture>();
		}


		// --- register texture ---

		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or directory that contains the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		public bool RegisterTexture(string path, out Texture handle)
		{
			return RegisterTexture(path, null, out handle);
		}

		/// <summary>Registeres a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		public bool RegisterTexture(string path, TextureParameters parameters, out Texture handle)
		{
			/* BUG:
			 * Attempt to delete null texture handles from the end of the array
			 * These sometimes seem to end up there
			 * 
			 * Have also seen a registered textures count of 72 and an array length of 64
			 * Is it possible for a texture to fail to register, but still increment the registered textures count?
			 * 
			 * There appears to be a timing issue somewhere whilst loading, as this only happens intermittantly
			 */
			if (RegisteredTexturesCount > RegisteredTextures.Length)
			{
				/* BUG:
				 * The registered textures count very occasional becomes greater than the array length (Texture loader crashses possibly?)
				 * This then crashes when we attempt to itinerate the array, so reset it...
				 */
				RegisteredTexturesCount = RegisteredTextures.Length;
			}

			if (RegisteredTexturesCount != 0)
			{
				try
				{
					for (int i = RegisteredTexturesCount - 1; i > 0; i--)
					{
						if (RegisteredTextures[i] != null)
						{
							break;
						}

						Array.Resize(ref RegisteredTextures, RegisteredTextures.Length - 1);
					}
				}
				catch
				{
					// ignored
				}
			}

			/*
			 * Check if the texture is already registered.
			 * If so, return the existing handle.
			 * */
			for (int i = 0; i < RegisteredTexturesCount; i++)
			{
				if (RegisteredTextures[i] != null)
				{
					try
					{
						//The only exceptions thrown were these when it barfed
						PathOrigin source = RegisteredTextures[i].Origin as PathOrigin;

						if (source != null && source.Path == path && source.Parameters == parameters)
						{
							handle = RegisteredTextures[i];
							return true;
						}
					}
					catch
					{
						// ignored
					}
				}

			}

			/*
			 * Register the texture and return the newly created handle.
			 * */
			int idx = GetNextFreeTexture();
			RegisteredTextures[idx] = new Texture(path, parameters, currentHost);
			RegisteredTexturesCount++;
			handle = RegisteredTextures[idx];
			return true;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <returns>The handle to the texture.</returns>
		public Texture RegisterTexture(Texture texture)
		{
			/*
			 * Register the texture and return the newly created handle.
			 * */
			int idx = GetNextFreeTexture();
			RegisteredTextures[idx] = new Texture(texture);
			RegisteredTexturesCount++;
			return RegisteredTextures[idx];
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="bitmap">The bitmap that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <returns>The handle to the texture.</returns>
		/// <remarks>Be sure not to dispose of the bitmap after calling this function.</remarks>
		public Texture RegisterTexture(Bitmap bitmap, TextureParameters parameters)
		{
			/*
			 * Register the texture and return the newly created handle.
			 * */
			int idx = GetNextFreeTexture();
			RegisteredTextures[idx] = new Texture(bitmap, parameters);
			RegisteredTexturesCount++;
			return RegisteredTextures[idx];
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="bitmap">The bitmap that contains the texture.</param>
		/// <returns>The handle to the texture.</returns>
		/// <remarks>Be sure not to dispose of the bitmap after calling this function.</remarks>
		public Texture RegisterTexture(Bitmap bitmap)
		{
			/*
			 * Register the texture and return the newly created handle.
			 * */
			int idx = GetNextFreeTexture();
			RegisteredTextures[idx] = new Texture(bitmap);
			RegisteredTexturesCount++;
			return RegisteredTextures[idx];
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="bitmap">The bitmap that contains the texture.</param>
		/// <param name="alpha">A second bitmap containing the alpha channel for this texture</param>
		/// <returns>The handle to the texture.</returns>
		/// <remarks>Be sure not to dispose of the bitmap after calling this function.</remarks>
		public Texture RegisterTexture(Bitmap bitmap, Bitmap alpha)
		{
			/*
			 * Register the texture and return the newly created handle.
			 * */
			int idx = GetNextFreeTexture();
			RegisteredTextures[idx] = new Texture(bitmap);
			RegisteredTexturesCount++;
			return RegisteredTextures[idx];
		}


		// --- load texture ---

		/// <summary>Loads the specified texture into OpenGL if not already loaded.</summary>
		/// <param name="handle">The handle to the registered texture.</param>
		/// <param name="wrap">The texture type indicating the clamp mode.</param>
		/// <param name="currentTicks">The current system clock-ticks</param>
		/// <param name="Interpolation">The interpolation mode to use when loading the texture</param>
		/// <param name="AnisotropicFilteringLevel">The anisotropic filtering level to use when loading the texture</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public bool LoadTexture(Texture handle, OpenGlTextureWrapMode wrap, int currentTicks, InterpolationMode Interpolation, int AnisotropicFilteringLevel)
		{

			Texture texture = null;
			//Don't try to load a texture to a null handle, this is a seriously bad idea....
			if (handle == null || handle.OpenGlTextures == null)
			{
				return false;
			}
			
			if (handle.MultipleFrames)
			{
				if (!(animatedTextures.ContainsKey(handle.Origin)))
				{
					if (!handle.Origin.GetTexture(out texture))
					{
						//Loading animated texture barfed
						return false;
					}
					animatedTextures.Add(handle.Origin, texture);
				}
				else
				{
					texture = animatedTextures[handle.Origin];
				}
				double elapsedTime = CPreciseTimer.GetElapsedTime(handle.LastAccess, currentTicks);
				int elapsedFrames = (int)(elapsedTime / handle.FrameInterval);
				if (elapsedFrames > 0)
				{
					handle.CurrentFrame += elapsedFrames;
					handle.LastAccess = currentTicks;
					for (int i = 0; i < 4; i++)
					{
						GL.DeleteTexture(handle.OpenGlTextures[i].Name);
						handle.OpenGlTextures[i].Valid = false;
					}
				}
			}
			else
			{
				handle.LastAccess = currentTicks;	
			}
			//Set last access time
			

			if (handle.OpenGlTextures[(int)wrap].Valid)
			{
				return true;
			}

			
			
			if (handle.Ignore)
			{
				return false;
			}

			if (texture == null && handle.Origin.GetTexture(out texture) || texture != null)
			{
				if (texture.MultipleFrames)
				{
					handle.MultipleFrames = true;
					handle.FrameInterval = texture.FrameInterval;
					texture.CurrentFrame = handle.CurrentFrame % texture.TotalFrames;
				}
				if (texture.BitsPerPixel == 32)
				{
					int[] names = new int[1];
					GL.GenTextures(1, names);
					GL.BindTexture(TextureTarget.Texture2D, names[0]);
					handle.OpenGlTextures[(int)wrap].Name = names[0];
					handle.Width = texture.Width;
					handle.Height = texture.Height;
					handle.Transparency = texture.GetTransparencyType();
					//texture = ResizeToPowerOfTwo(texture);

					switch (Interpolation)
					{
						case InterpolationMode.NearestNeighbor:
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
							break;
						case InterpolationMode.Bilinear:
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
							break;
						case InterpolationMode.NearestNeighborMipmapped:
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.NearestMipmapNearest);
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
							break;
						case InterpolationMode.BilinearMipmapped:
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.NearestMipmapLinear);
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
							break;
						case InterpolationMode.TrilinearMipmapped:
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
							break;
						default:
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
							break;
					}

					if ((wrap & OpenGlTextureWrapMode.RepeatClamp) != 0)
					{
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.Repeat);
					}
					else
					{
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
					}

					if ((wrap & OpenGlTextureWrapMode.ClampRepeat) != 0)
					{
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.Repeat);
					}
					else
					{
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
					}

					if (renderer.ForceLegacyOpenGL)
					{
						if (Interpolation == InterpolationMode.NearestNeighbor || Interpolation == InterpolationMode.Bilinear)
						{
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);
						}
						else
						{
							GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
						}
					}

					if (Interpolation == InterpolationMode.AnisotropicFiltering && AnisotropicFilteringLevel > 0)
					{
						GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, AnisotropicFilteringLevel);
					}

					if (handle.Transparency == TextureTransparencyType.Opaque)
					{
						/*
						 * If the texture is fully opaque, the alpha channel is not used.
						 * If the graphics driver and card support 24-bits per channel,
						 * it is best to convert the bitmap data to that format in order
						 * to save memory on the card. If the card does not support the
						 * format, it will likely be upconverted to 32-bits per channel
						 * again, and this is wasted effort.
						 * */
						int width = texture.Width;
						int height = texture.Height;
						int stride = (3 * (width + 1) >> 2) << 2;
						byte[] oldBytes = texture.Bytes;
						byte[] newBytes = new byte[stride * texture.Height];
						int i = 0, j = 0;

						for (int y = 0; y < height; y++)
						{
							for (int x = 0; x < width; x++)
							{
								newBytes[j + 0] = oldBytes[i + 0];
								newBytes[j + 1] = oldBytes[i + 1];
								newBytes[j + 2] = oldBytes[i + 2];
								i += 4;
								j += 3;
							}

							j += stride - 3 * width;
						}

						GL.TexImage2D(TextureTarget.Texture2D, 0,
							PixelInternalFormat.Rgb8,
							texture.Width, texture.Height, 0,
							OpenTK.Graphics.OpenGL.PixelFormat.Rgb,
							PixelType.UnsignedByte, newBytes);
					}
					else
					{
						/*
						 * The texture uses its alpha channel, so send the bitmap data
						 * in 32-bits per channel as-is.
						 * */
						GL.TexImage2D(TextureTarget.Texture2D, 0,
							PixelInternalFormat.Rgba8,
							texture.Width, texture.Height, 0,
							OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
							PixelType.UnsignedByte, texture.Bytes);
					}
					if (renderer.ForceLegacyOpenGL == false)
					{
						GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
					}
					handle.OpenGlTextures[(int)wrap].Valid = true;
					return true;
				}
			}

			handle.Ignore = true;
			return false;
		}

		// --- save texture ---

		/// <summary>Saves a texture to a file.</summary>
		/// <param name="file">The file.</param>
		/// <param name="texture">The texture.</param>
		/// <remarks>The texture is always saved in PNG format.</remarks>
		public void SaveTexture(string file, Texture texture)
		{
			Bitmap bitmap = new Bitmap(texture.Width, texture.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
			byte[] bytes = new byte[texture.Bytes.Length];

			for (int i = 0; i < bytes.Length; i += 4)
			{
				bytes[i] = texture.Bytes[i + 2];
				bytes[i + 1] = texture.Bytes[i + 1];
				bytes[i + 2] = texture.Bytes[i];
				bytes[i + 3] = texture.Bytes[i + 3];
			}

			Marshal.Copy(bytes, 0, data.Scan0, texture.Bytes.Length);
			bitmap.UnlockBits(data);
			bitmap.Save(file, ImageFormat.Png);
			bitmap.Dispose();
		}


		// --- upsize texture ---

		/// <summary>Resizes the specified texture to a power of two size and returns the result.</summary>
		/// <param name="texture">The texture.</param>
		/// <returns>The upsized texture, or the original if already a power of two size.</returns>
		/// <exception cref="System.NotSupportedException">The bits per pixel in the texture is not supported.</exception>
		public Texture ResizeToPowerOfTwo(Texture texture)
		{
			int width = RoundUpToPowerOfTwo(texture.Width);
			int height = RoundUpToPowerOfTwo(texture.Height);

			//HACK: Some routes use non-power of two textures which upscale to stupid numbers
			//At least round down if we're over 1024 px....
			if (width != texture.Width && width > 1024)
			{
				width /= 2;
			}

			if (height != texture.Height && height > 1024)
			{
				height /= 2;
			}

			return Resize(texture, width, height);
		}

		/// <summary>Resizes the specified texture to the specified width and height and returns the result.</summary>
		/// <param name="texture">The texture.</param>
		/// <param name="width">The new width.</param>
		/// <param name="height">The new height.</param>
		/// <returns>The resize texture, or the original if already of the specified size.</returns>
		/// <exception cref="System.NotSupportedException">The bits per pixel in the source texture is not supported.</exception>
		/// <exception cref="System.OverflowException">The resized texture would exceed the maximum possible size.</exception>
		public static Texture Resize(Texture texture, int width, int height)
		{
			if (width == texture.Width && height == texture.Height)
			{
				return texture;
			}

			if (texture.BitsPerPixel != 32)
			{
				throw new NotSupportedException("The number of bits per pixel is not supported.");
			}

			TextureTransparencyType type = texture.GetTransparencyType();

			/*
			 * Convert the texture into a bitmap.
			 * */
			Bitmap bitmap = new Bitmap(texture.Width, texture.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
			Marshal.Copy(texture.Bytes, 0, data.Scan0, texture.Bytes.Length);
			bitmap.UnlockBits(data);

			/*
			 * Scale the bitmap.
			 * */
			Bitmap scaledBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(scaledBitmap);
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			graphics.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, texture.Width, texture.Height), GraphicsUnit.Pixel);
			graphics.Dispose();
			bitmap.Dispose();

			/*
			 * Convert the bitmap into a texture.
			 * */
			data = scaledBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, scaledBitmap.PixelFormat);
			int newSize;

			try
			{
				newSize = checked(4 * width * height);
			}
			catch (OverflowException)
			{
				throw new OverflowException("The resized texture would exceed the maximum possible size.");
			}

			byte[] bytes = new byte[newSize];
			Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
			scaledBitmap.UnlockBits(data);
			scaledBitmap.Dispose();

			/*
			 * Ensure opaque and partially transparent
			 * textures have valid alpha components.
			 * */
			if (type == TextureTransparencyType.Opaque)
			{
				for (int i = 3; i < bytes.Length; i += 4)
				{
					bytes[i] = 255;
				}
			}
			else if (type == TextureTransparencyType.Partial)
			{
				for (int i = 3; i < bytes.Length; i += 4)
				{
					bytes[i] = bytes[i] < 128 ? (byte)0 : (byte)255;
				}
			}

			Texture result = new Texture(width, height, 32, bytes, texture.Palette);
			return result;
		}


		// --- unload texture ---

		/// <summary>Unloads the specified texture from OpenGL if loaded.</summary>
		/// <param name="handle">The handle to the registered texture.</param>
		public static void UnloadTexture(Texture handle)
		{
			//Null check the texture handle, as otherwise this can cause OpenGL to throw a fit
			if (handle == null)
			{
				return;
			}

			foreach (OpenGlTexture t in handle.OpenGlTextures)
			{
				if (t.Valid)
				{
					GL.DeleteTextures(1, new[] { t.Name });
					t.Valid = false;
				}
			}

			handle.Ignore = false;
		}

		/// <summary>Unloads all registered textures.</summary>
		public void UnloadAllTextures()
		{
			for (int i = 0; i < RegisteredTexturesCount; i++)
			{
				UnloadTexture(RegisteredTextures[i]);
			}
			GC.Collect(); //Speculative- https://bveworldwide.forumotion.com/t1873-object-routeviewer-out-of-memory#19423
		}


		// --- statistics ---

		/// <summary>Gets the number of registered textures.</summary>
		/// <returns>The number of registered textures.</returns>
		public int GetNumberOfRegisteredTextures()
		{
			return RegisteredTexturesCount;
		}

		/// <summary>Gets the number of loaded textures.</summary>
		/// <returns>The number of loaded textures.</returns>
		public int GetNumberOfLoadedTextures()
		{
			int count = 0;

			for (int i = 0; i < RegisteredTexturesCount; i++)
			{
				if (RegisteredTextures[i] == null)
				{
					continue;
				}

				if (RegisteredTextures[i].OpenGlTextures.Any(t => t.Valid))
				{
					count++;
				}
			}

			return count;
		}


		/// <summary>Gets the next free texture, resizing the base textures array if appropriate</summary>
		/// <returns>The index of the next free texture</returns>
		public int GetNextFreeTexture()
		{
			if (RegisteredTextures.Length == RegisteredTexturesCount)
			{
				Array.Resize(ref RegisteredTextures, RegisteredTextures.Length << 1);
			}
			else if (RegisteredTexturesCount > RegisteredTextures.Length)
			{
				/* BUG:
				 * The registered textures count very occasional becomes greater than the array length (Texture loader crashes possibly?)
				 * This then crashes when we attempt to itinerate the array, so reset it...
				 */
				RegisteredTexturesCount = RegisteredTextures.Length;
				Array.Resize(ref RegisteredTextures, RegisteredTextures.Length << 1);
			}

			return RegisteredTexturesCount;
		}


		// --- functions ---

		/// <summary>Takes a positive value and rounds it up to the next highest power of two.</summary>
		/// <param name="value">The value.</param>
		/// <returns>The next highest power of two, or the original value if already a power of two.</returns>
		public int RoundUpToPowerOfTwo(int value)
		{
			if (value <= 0)
			{
				throw new ArgumentException("The specified value is not positive.");
			}

			value -= 1;

			for (int i = 1; i < sizeof(int) * 8; i <<= 1)
			{
				value |= value >> i;
			}

			return value + 1;

		}
	}
}
