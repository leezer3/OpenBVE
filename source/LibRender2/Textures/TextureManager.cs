using System;
using System.Collections.Concurrent;
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
		internal static ConcurrentDictionary<TextureOrigin, Texture> textureCache = new ConcurrentDictionary<TextureOrigin, Texture>();

		/// <summary>Total time spent decoding texture files, in milliseconds.</summary>
		public static long TextureDecodeTime;

		private static Dictionary<TextureOrigin, Texture> animatedTextures;

		/// <summary>Holds the registered path-based textures, indexed by path.
		/// Values are immutable snapshots, replaced atomically on registration.</summary>
		private static readonly ConcurrentDictionary<string, Texture[]> RegisteredTextureLookup = new ConcurrentDictionary<string, Texture[]>(StringComparer.OrdinalIgnoreCase);

		private static readonly object TextureLookupLock = new object();

		/// <summary>The number of currently registered textures.</summary>
		public volatile int RegisteredTexturesCount;

		internal TextureManager(HostInterface CurrentHost, BaseRenderer Renderer)
		{
			currentHost = CurrentHost;
			RegisteredTextures = new Texture[16];
			RegisteredTexturesCount = 0;
			renderer = Renderer;
			animatedTextures = new Dictionary<TextureOrigin, Texture>();
		}


		// --- register texture ---

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or directory that contains the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		public bool RegisterTexture(string path, out Texture handle)
		{
			return RegisterTexture(path, null, out handle);
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives a handle to the texture.</param>
		/// <returns>Whether registering the texture was successful.</returns>
		public bool RegisterTexture(string path, TextureParameters parameters, out Texture handle)
		{
			/*
			 * Check if the texture is already registered.
			 * If so, return the existing handle.
			 * The stored arrays are immutable, so this read is lock-free.
			 * Only a valid registration is reused, so a transient decode failure
			 * can never poison the lookup for a file which is later decoded fine.
			 * */
			if (TryFindRegisteredTexture(path, parameters, true, out handle))
			{
				return true;
			}

			/*
			 * Construct the handle (and decode the file) inside the process-wide
			 * GDI+ lock. The image plugins use GDI+ codecs which are not
			 * thread-safe, and parallel route loading registers textures from many
			 * threads concurrently. Serialising the decode also lets a thread that
			 * loses the registration race reuse the winner's handle instead of
			 * decoding the same file again.
			 * */
			Texture newHandle;
			lock (BaseRenderer.GdiPlusLock)
			{
				// Another thread may have completed the registration while we waited.
				if (TryFindRegisteredTexture(path, parameters, true, out handle))
				{
					return true;
				}

				newHandle = new Texture(path, parameters, currentHost);
				bool valid = newHandle.PixelFormat != PixelFormat.Invalid && newHandle.DecodedTexture != null;

				/*
				 * Reserve a slot, write the handle and publish the lookup snapshot
				 * atomically. The count is only advanced after the slot has been
				 * written and the array guaranteed to have room for it, so concurrent
				 * readers can never observe a count which exceeds the array length.
				 * */
				lock (TextureLookupLock)
				{
					// Prefer an existing valid registration over the handle just decoded.
					if (TryFindRegisteredTexture(path, parameters, true, out handle))
					{
						return true;
					}

					if (!valid)
					{
						// Neither our decode nor any existing one is usable. Reuse the
						// existing invalid handle (if any) rather than registering a
						// duplicate; render time will retry the decode.
						if (TryFindRegisteredTexture(path, parameters, false, out handle))
						{
							return true;
						}
					}

					if (RegisteredTexturesCount >= RegisteredTextures.Length)
					{
						Array.Resize(ref RegisteredTextures, Math.Max(RegisteredTextures.Length << 1, RegisteredTexturesCount + 1));
					}
					int idx = RegisteredTexturesCount;
					RegisteredTexturesCount++;
					RegisteredTextures[idx] = newHandle;
					handle = newHandle;

					/*
					 * Maintain the registration lookup table.
					 * Publish a new immutable snapshot; readers never take a lock.
					 * */
					if (RegisteredTextureLookup.TryGetValue(path, out Texture[] currentList))
					{
						Texture[] newList = new Texture[currentList.Length + 1];
						Array.Copy(currentList, newList, currentList.Length);
						newList[currentList.Length] = newHandle;
						RegisteredTextureLookup[path] = newList;
					}
					else
					{
						RegisteredTextureLookup[path] = new[] { newHandle };
					}
				}
			}

			/*
			 * Pre-seed the texture cache with the decoded texture (not the handle).
			 * The handle itself has no decoded bytes, so storing it would cause a null
			 * reference when the transparency type is subsequently queried.
			 * */
			if (newHandle.PixelFormat != PixelFormat.Invalid && newHandle.DecodedTexture != null)
			{
				textureCache.TryAdd(newHandle.Origin, newHandle.DecodedTexture);
			}
			return true;
		}

		/// <summary>Looks up a registered path-based texture with the specified parameters.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The texture parameters.</param>
		/// <param name="requireValid">Whether an invalid (failed to decode) registration should be ignored.</param>
		/// <param name="handle">Receives the matching handle, if found.</param>
		/// <returns>Whether a matching texture was found.</returns>
		private static bool TryFindRegisteredTexture(string path, TextureParameters parameters, bool requireValid, out Texture handle)
		{
			if (RegisteredTextureLookup.TryGetValue(path, out Texture[] candidates))
			{
				for (int i = 0; i < candidates.Length; i++)
				{
					try
					{
						PathOrigin source = candidates[i].Origin as PathOrigin;

						if (source != null && source.Parameters == parameters)
						{
							if (!requireValid || candidates[i].PixelFormat != PixelFormat.Invalid)
							{
								handle = candidates[i];
								return true;
							}
						}
					}
					catch
					{
						// ignored
					}
				}
			}
			handle = null;
			return false;
		}

		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <returns>The handle to the texture.</returns>
		public Texture RegisterTexture(Texture texture)
		{
			/*
			 * Register the texture and return the newly created handle.
			 * */
			lock (TextureLookupLock)
			{
				int idx = GetNextFreeTexture();
				RegisteredTextures[idx] = new Texture(texture);
				RegisteredTexturesCount++;
				return RegisteredTextures[idx];
			}
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
			lock (TextureLookupLock)
			{
				int idx = GetNextFreeTexture();
				RegisteredTextures[idx] = new Texture(bitmap, parameters);
				RegisteredTexturesCount++;
				return RegisteredTextures[idx];
			}
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
			lock (TextureLookupLock)
			{
				int idx = GetNextFreeTexture();
				RegisteredTextures[idx] = new Texture(bitmap);
				RegisteredTexturesCount++;
				return RegisteredTextures[idx];
			}
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
				if (!animatedTextures.TryGetValue(handle.Origin, out texture))
				{
					lock (BaseRenderer.GdiPlusLock)
					{
						if (!handle.Origin.GetTexture(out texture))
						{
							//Loading animated texture barfed
							return false;
						}
					}
					animatedTextures.Add(handle.Origin, texture);
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
				/*
				 * A previous decode failed. If the source file still exists, the
				 * failure may have been transient (e.g. concurrent GDI+ decode
				 * during parallel loading), so retry rather than leaving the
				 * texture permanently white. Only genuinely missing files are
				 * skipped until the file appears (or is unloaded).
				 * */
				if (handle.Origin is PathOrigin pathOrigin && File.Exists(pathOrigin.Path))
				{
					handle.Ignore = false;
				}
				else
				{
					return false;
				}
			}

			if (texture == null)
			{
				lock (BaseRenderer.GdiPlusLock)
				{
					handle.Origin.GetTexture(out texture);
				}
			}
			if (texture != null)
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

					if (Interpolation == InterpolationMode.AnisotropicFiltering && AnisotropicFilteringLevel > 0)
					{
						GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, AnisotropicFilteringLevel);
					}
					
					bool noLuminanceChannel = currentHost.Platform == HostPlatform.AppleOSX || renderer.currentOptions.ForceForwardsCompatibleContext;
					
					if (handle.Transparency == TextureTransparencyType.Opaque)
					{
						switch (texture.PixelFormat)
						{
							case PixelFormat.Grayscale:
								// send as is to the luminance channel [NOTE: deprecated in GL4, so use Red channel instead]
								// n.b. Make sure to set the unpack alignment as otherwise we corrupt textures where stride > width
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									noLuminanceChannel ? PixelInternalFormat.R8 : PixelInternalFormat.Luminance,
									texture.Width, texture.Height, 0,
									noLuminanceChannel ? OpenTK.Graphics.OpenGL.PixelFormat.Red : OpenTK.Graphics.OpenGL.PixelFormat.Luminance,
									PixelType.UnsignedByte, texture.Bytes);
								
								if (noLuminanceChannel)
								{
									// small cheat: Use GL_RED (6403) to swizzle our R channel when called by the shader
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, new[] { 6403, 6403, 6403, 1});
								}
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
								// down convert to RGB
								int stride = (3 * (texture.Width + 1) >> 2) << 2;
								byte[] newBytes = new byte[stride * texture.Height];
								int i = 0, j = 0;

								for (int y = 0; y < texture.Height; y++)
								{
									for (int x = 0; x < texture.Width; x++)
									{
										newBytes[j + 0] = texture.Bytes[i + 0];
										newBytes[j + 1] = texture.Bytes[i + 1];
										newBytes[j + 2] = texture.Bytes[i + 2];
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
							// NOTE: LuminanceAlpha is deprecated in GL4, so just upconvert to RGBA
							if (noLuminanceChannel)
							{
								int stride = (4 * (texture.Width + 1) >> 2) << 2;
								byte[] newBytes = new byte[stride * texture.Height];
								int i = 0, j = 0;

								for (int y = 0; y < texture.Height; y++)
								{
									for (int x = 0; x < texture.Width; x++)
									{
										newBytes[j + 0] = texture.Bytes[i + 0];
										newBytes[j + 1] = texture.Bytes[i + 0];
										newBytes[j + 2] = texture.Bytes[i + 0];
										newBytes[j + 3] = texture.Bytes[i + 1];
										i += 2;
										j += 4;
									}

									j += stride - 4 * texture.Width;
								}
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.Rgba8,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
									PixelType.UnsignedByte, newBytes);
							}
							else
							{
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.LuminanceAlpha,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.LuminanceAlpha,
									PixelType.UnsignedByte, texture.Bytes);
							}
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
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
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
				textureCache.TryRemove(handle.Origin, out _);
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
				/*
				 * On a route reload, preserve textures whose source file is unchanged,
				 * so that the first frame after the reload does not re-upload every texture.
				 */
				if (currentlyReloading && RegisteredTextures[i] != null && !RegisteredTextures[i].MultipleFrames && TextureFileUnchanged(RegisteredTextures[i].Origin))
				{
					continue;
				}
				UnloadTexture(ref RegisteredTextures[i]);
			}
			if (currentlyReloading)
			{
				foreach (TextureOrigin origin in textureCache.Keys.ToList())
				{
					if (origin is PathOrigin && !TextureFileUnchanged(origin))
					{
						textureCache.TryRemove(origin, out _);
					}
				}
			}
			else
			{
				textureCache.Clear();
			}

			/*
			 * Rebuild the registration lookup table from the surviving textures,
			 * so that it does not retain handles which have been unloaded.
			 * */
			lock (TextureLookupLock)
			{
				RegisteredTextureLookup.Clear();
				for (int i = 0; i < RegisteredTexturesCount; i++)
				{
					Texture texture = RegisteredTextures[i];
					if (texture != null && texture.Origin is PathOrigin pathOrigin)
					{
						if (RegisteredTextureLookup.TryGetValue(pathOrigin.Path, out Texture[] currentList))
						{
							Texture[] newList = new Texture[currentList.Length + 1];
							Array.Copy(currentList, newList, currentList.Length);
							newList[currentList.Length] = texture;
							RegisteredTextureLookup[pathOrigin.Path] = newList;
						}
						else
						{
							RegisteredTextureLookup[pathOrigin.Path] = new[] { texture };
						}
					}
				}
			}

			if (!currentlyReloading)
			{
				// Only force GC on full unload, not on route reload
				GC.Collect(0, GCCollectionMode.Optimized);
			}
			
		}

		/// <summary>Checks whether the on-disk source file of the given texture origin is unchanged.</summary>
		private static bool TextureFileUnchanged(TextureOrigin origin)
		{
			if (!(origin is PathOrigin pathOrigin))
			{
				return false;
			}
			// Refresh() first, as FileSystemInfo caches size/last write time.
			try
			{
				FileInfo info = new FileInfo(pathOrigin.Path);
				info.Refresh();
				return info.Exists && pathOrigin.FileSize == info.Length && pathOrigin.LastModificationTime == info.LastWriteTime;
			}
			catch
			{
				return false;
			}
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
				foreach (Texture Texture in RegisteredTextures)
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
