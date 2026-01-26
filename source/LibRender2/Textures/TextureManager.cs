using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using LibRender2.Screens;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using InterpolationMode = OpenBveApi.Graphics.InterpolationMode;
using PixelFormat = OpenBveApi.Textures.PixelFormat;

namespace LibRender2.Textures
{
	/// <summary>Provides functions for dealing with textures.</summary>
	public class TextureManager
	{
		private readonly HostInterface currentHost;

		private readonly BaseRenderer renderer;

		/// <summary>Holds all currently registered textures.</summary>
		public static Texture[] RegisteredTextures;
		/// <summary>Holds cached texture origins</summary>
		internal static Dictionary<TextureOrigin, Texture> textureCache = new Dictionary<TextureOrigin, Texture>();

		private static Dictionary<TextureOrigin, Texture> animatedTextures;

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
			if (!File.Exists(path))
			{
				// shouldn't happen, but handle gracefully
				handle = null;
				return false;
			}
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
					for (int i = RegisteredTexturesCount - 1; i >= 0; i--)
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


		// --- load texture ---

		/// <summary>Loads the specified texture into OpenGL if not already loaded.</summary>
		/// <param name="handle">The handle to the registered texture.</param>
		/// <param name="wrap">The texture type indicating the clamp mode.</param>
		/// <param name="currentTicks">The current system clock-ticks</param>
		/// <param name="Interpolation">The interpolation mode to use when loading the texture</param>
		/// <param name="AnisotropicFilteringLevel">The anisotropic filtering level to use when loading the texture</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public bool LoadTexture(ref Texture handle, OpenGlTextureWrapMode wrap, int currentTicks, InterpolationMode Interpolation, int AnisotropicFilteringLevel)
		{

			Texture texture = null;
			//Don't try to load a texture to a null handle, this is a seriously bad idea....
			if (handle == null || handle.OpenGlTextures == null)
			{
				return false;
			}
			
			if (handle.MultipleFrames)
			{
				if (!animatedTextures.ContainsKey(handle.Origin))
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
				int elapsedFrames = (int)(elapsedTime / texture.FrameInterval);
				if (elapsedFrames > 0)
				{
					texture.CurrentFrame += elapsedFrames;
					texture.CurrentFrame %= texture.TotalFrames;
					handle.LastAccess = currentTicks;
				}
			}
			else
			{
				handle.LastAccess = currentTicks;
			}
			//Set last access time

			if (texture != null)
			{
				handle = texture;
			}

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
				}
				//if (texture.BitsPerPixel == 32)
				{
					int[] names = new int[1];
					GL.GenTextures(1, names);
					GL.BindTexture(TextureTarget.Texture2D, names[0]);
					handle.OpenGlTextures[(int)wrap].Name = names[0];
					if (texture.MultipleFrames)
					{
						texture.OpenGlTextures[(int)wrap].Name = names[0];
					}

					handle.Size = texture.Size;
					handle.Transparency = texture.GetTransparencyType();

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
						switch (texture.PixelFormat)
						{
							case PixelFormat.Grayscale:
								// send as is to the luminance channel [NOTE: deprecated in GL4]
								// n.b. Make sure to set the unpack alignment as otherwise we corrupt textures where stride > width
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.Luminance,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.Luminance,
									PixelType.UnsignedByte, texture.Bytes);

								break;
							case PixelFormat.RGB:
								// send as is
								// n.b. Make sure to set the unpack alignment as otherwise we corrupt textures where stride > width
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.Rgb8,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.Rgb,
									PixelType.UnsignedByte, texture.Bytes);
								break;
							case PixelFormat.RGBAlpha:
								/*
								* If the texture is fully opaque, the alpha channel is not used.
								* If the graphics driver and card support 24-bits per channel,
								* it is best to convert the bitmap data to that format in order
								* to save memory on the card. If the card does not support the
								* format, it will likely be upconverted to 32-bits per channel
								* again, and this is wasted effort.
								* */
								int stride = (3 * (texture.Width + 1) >> 2) << 2;
								byte[] oldBytes = texture.Bytes;
								byte[] newBytes = new byte[stride * texture.Height];
								int i = 0, j = 0;

								for (int y = 0; y < texture.Height; y++)
								{
									for (int x = 0; x < texture.Width; x++)
									{
										newBytes[j + 0] = oldBytes[i + 0];
										newBytes[j + 1] = oldBytes[i + 1];
										newBytes[j + 2] = oldBytes[i + 2];
										i += 4;
										j += 3;
									}

									j += stride - 3 * texture.Width;
								}
								// send as is
								// n.b. Must reset the unpack alignment in case of changes
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.Rgb8,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.Rgb,
									PixelType.UnsignedByte, newBytes);
								break;
						}
						
					}
					else
					{
						switch (texture.PixelFormat)
						{
							case PixelFormat.GrayscaleAlpha:
								// NOTE: luminance is deprecated in GL4
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.LuminanceAlpha,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.LuminanceAlpha,
									PixelType.UnsignedByte, texture.Bytes);
								break;
							case PixelFormat.RGBAlpha:
								/*
						 * The texture uses its alpha channel, so send the bitmap data
						 * in 32-bits per channel as-is.
						 * */
								// n.b. Must reset the unpack alignment in case of changes
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.Rgba8,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
									PixelType.UnsignedByte, texture.Bytes);
								break;
						}
						
					}
					if (renderer.ForceLegacyOpenGL == false)
					{
						GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
					}
					handle.OpenGlTextures[(int)wrap].Valid = true;
					if (texture.MultipleFrames)
					{
						texture.OpenGlTextures[(int)wrap].Valid = true;
					}
					return true;
				}
			}

			handle.Ignore = true;
			return false;
		}

		/// <summary>Unloads the specified texture from OpenGL if loaded.</summary>
		/// <param name="handle">The handle to the registered texture.</param>
		public static void UnloadTexture(ref Texture handle)
		{
			//Null check the texture handle, as otherwise this can cause OpenGL to throw a fit
			if (handle == null)
			{
				return;
			}

			if (handle.MultipleFrames)
			{
				for (int i = 0; i < handle.TotalFrames; i++)
				{
					handle.CurrentFrame = i;
					foreach (OpenGlTexture t in handle.OpenGlTextures)
					{
						if (t.Valid)
						{
							GL.DeleteTextures(1, new[] { t.Name });
							t.Valid = false;
						}
					}
				}
				/*
				 * Clone the ref for the search and then re-create the original in the texturemanager array
				 * This allows it to be re-loaded from disk
				 */
				var texture = handle;
				TextureOrigin key = animatedTextures.FirstOrDefault(x => x.Value == texture).Key;
				handle = new Texture(key);
			}
			else
			{
				foreach (OpenGlTexture t in handle.OpenGlTextures)
				{
					if (t.Valid)
					{
						GL.DeleteTextures(1, new[] { t.Name });
						t.Valid = false;
					}
				}
			}
			handle.Ignore = false;
			if (handle.Origin != null)
			{
				textureCache.Remove(handle.Origin);
			}
		}

		/// <summary>Loads all registered textures.</summary>
		public void LoadAllTextures()
		{
			for (int i = 0; i < RegisteredTexturesCount; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					if (RegisteredTextures[i] != null && RegisteredTextures[i].OpenGlTextures[j].Used)
					{
						LoadTexture(ref RegisteredTextures[i], (OpenGlTextureWrapMode)j, CPreciseTimer.GetClockTicks(), renderer.currentOptions.Interpolation, renderer.currentOptions.AnisotropicFilteringLevel);
					}

				}

			}
		}

		/// <summary>Unloads all registered textures.</summary>
		public void UnloadAllTextures(bool currentlyReloading)
		{
			for (int i = 0; i < RegisteredTexturesCount; i++)
			{
				UnloadTexture(ref RegisteredTextures[i]);
			}
			if (currentlyReloading)
			{
				foreach(TextureOrigin origin in textureCache.Keys.ToList())
				{
					if (origin is PathOrigin pathOrigin)
					{
						if (!File.Exists(pathOrigin.Path) || pathOrigin.FileSize != new FileInfo(pathOrigin.Path).Length || pathOrigin.LastModificationTime != File.GetLastWriteTime(pathOrigin.Path))
						{
							textureCache.Remove(origin);
						}
					}
				}
			}
			else
			{
				textureCache.Clear();
			}
			
			GC.Collect(); //Speculative- https://bveworldwide.forumotion.com/t1873-object-routeviewer-out-of-memory#19423
			
		}

		/// <summary>Unloads any textures which have not been accessed</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		public void UnloadUnusedTextures(double TimeElapsed)
		{
#if DEBUG
			//HACK: If when running in debug mode the frame time exceeds 1s, we can assume VS has hit a breakpoint
			//Don't unload textures in this case, as it just causes texture bugs
			if (TimeElapsed > 1000)
			{
				foreach (var Texture in RegisteredTextures)
				{
					if (Texture != null)
					{
						Texture.LastAccess = CPreciseTimer.GetClockTicks();
					}
				}
			}
#endif
			if (renderer.CurrentInterface == InterfaceType.Normal)
			{
				for (int i = 0; i < RegisteredTextures.Length; i++)
				{
					if (RegisteredTextures[i] != null && RegisteredTextures[i].AvailableToUnload && (CPreciseTimer.GetClockTicks() - RegisteredTextures[i].LastAccess) > 20000)
					{
						UnloadTexture(ref RegisteredTextures[i]);
					}
				}
			}
			else
			{
				//Don't unload textures if we are in a menu/ paused, as they may be required immediately after unpause
				foreach (var Texture in TextureManager.RegisteredTextures)
				{
					//Texture can be null in certain cases....
					if (Texture != null)
					{
						Texture.LastAccess = CPreciseTimer.GetClockTicks();
					}
				}
			}
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
				if (RegisteredTextures[i] == null || RegisteredTextures[i].MultipleFrames)
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

		public int GetNumberOfLoadedAnimatedTextures()
		{
			int count = 0;
			for (int i = 0; i < RegisteredTexturesCount; i++)
			{
				if (RegisteredTextures[i] == null || RegisteredTextures[i].MultipleFrames == false)
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
