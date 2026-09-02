using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		/// <summary>Total time spent decoding texture files, in milliseconds.</summary>
		public static long TextureDecodeTime;

		/// <summary>Number of texture upload requests handled.</summary>
		public static long UploadCount;

		/// <summary>Time spent in texture upload requests, including cached-handle early exits (ms).</summary>
		public static long UploadMs;

		private static Dictionary<TextureOrigin, Texture> animatedTextures;
		// Reused buffer for paletted GIF to avoid per-frame new byte[] leak (Can get StackOverflow in glTexSubImage2D)
		private static byte[] _palettedExpandBuffer;
		private static readonly object _expandLock = new object();

		/// <summary>Holds the registered path-based textures, indexed by path.</summary>
		private static readonly Dictionary<string, List<Texture>> RegisteredTextureLookup = new Dictionary<string, List<Texture>>(StringComparer.OrdinalIgnoreCase);

		private static readonly object TextureLookupLock = new object();

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
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				// shouldn't happen, but handle gracefully
				handle = null;
				return false;
			}
			/* BUG:
			 * The registered textures count very occasional becomes greater than the array length (Texture loader crashes possibly?)
			 * This then crashes when we attempt to itinerate the array, so reset it...
			 */
			if (RegisteredTexturesCount > RegisteredTextures.Length)
			{
				RegisteredTexturesCount = RegisteredTextures.Length;
			}

			/*
			 * Check if the texture is already registered.
			 * If so, return the existing handle.
			 * */
			lock (TextureLookupLock)
			{
				if (RegisteredTextureLookup.TryGetValue(path, out List<Texture> candidates))
				{
					for (int i = 0; i < candidates.Count; i++)
					{
						try
						{
							PathOrigin source = candidates[i].Origin as PathOrigin;

							if (source != null && source.Parameters == parameters)
							{
								handle = candidates[i];
								return true;
							}
						}
						catch
						{
							// ignored
						}
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

			lock (TextureLookupLock)
			{
				/*
				 * Pre-seed the texture cache with the decoded texture (not the handle).
				 * The handle itself has no decoded bytes, so storing it would cause a null
				 * reference when the transparency type is subsequently queried.
				 * */
				if (handle.PixelFormat != PixelFormat.Invalid && handle.DecodedTexture != null && !textureCache.ContainsKey(handle.Origin))
				{
					textureCache.Add(handle.Origin, handle.DecodedTexture);
				}

				/*
				 * Maintain the registration lookup table.
				 * */
				if (!RegisteredTextureLookup.TryGetValue(path, out List<Texture> list))
				{
					list = new List<Texture>();
					RegisteredTextureLookup[path] = list;
				}
				list.Add(handle);
			}
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
			Stopwatch uploadTimer = Stopwatch.StartNew();
			bool result = LoadTextureInternal(ref handle, wrap, currentTicks, Interpolation, AnisotropicFilteringLevel);
			UploadCount++;
			UploadMs += uploadTimer.ElapsedMilliseconds;
			return result;
		}

		private bool LoadTextureInternal(ref Texture handle, OpenGlTextureWrapMode wrap, int currentTicks, InterpolationMode Interpolation, int AnisotropicFilteringLevel)
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
					// Reuse register-time decode from textureCache where possible to avoid decoding the same large animated GIF twice (2× memory). See RegisterTexture pre-seed at line 136.
					lock (TextureLookupLock)
					{
						if (textureCache.TryGetValue(handle.Origin, out Texture cachedTexture) && cachedTexture.MultipleFrames)
						{
							PathOrigin cachedPathOrigin = cachedTexture.Origin as PathOrigin;
							PathOrigin handlePathOrigin = handle.Origin as PathOrigin;
							// PathOrigin equality is path-only, so check Parameters explicitly; ByteArrayOrigin path never hits here (handled below)
							if (cachedPathOrigin != null && handlePathOrigin != null)
							{
								if (cachedPathOrigin.Parameters == handlePathOrigin.Parameters)
									texture = cachedTexture;
							}
							else if (handle.Origin is ByteArrayOrigin || cachedTexture.Origin is ByteArrayOrigin)
							{
								texture = cachedTexture;
							}
							else if (cachedPathOrigin == null && handlePathOrigin == null)
							{
								texture = cachedTexture;
							}
						}
					}
					if (texture == null)
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
					int oldFrame = texture.CurrentFrame;
					texture.CurrentFrame += elapsedFrames;
					texture.CurrentFrame %= texture.TotalFrames;
					handle.LastAccess = currentTicks;
					// If frame changed and GL texture already uploaded, update in-place via TexSubImage2D to avoid creating a lot of GL calls and per-frame alloc leak
					if (oldFrame != texture.CurrentFrame && texture.OpenGlTextures[(int)wrap].Valid && handle.OpenGlTextures[(int)wrap].Valid)
					{
						// Reuse same GL name across frames, update existing texture
						GL.BindTexture(TextureTarget.Texture2D, handle.OpenGlTextures[(int)wrap].Name);
						byte[] subBytes = texture.Bytes; // current frame's bytes (paletted or RGBA)
						// For paletted, expand via reused static buffer to avoid per-frame new byte[] GC pressure. (I think this how web browser do frame discarding)
						if (texture.PixelFormat == PixelFormat.Paletted)
						{
							bool opaque = texture.GetTransparencyType() == TextureTransparencyType.Opaque;
							int need = texture.Width * texture.Height * (opaque ? 3 : 4);
							byte[] pooled;
							lock (_expandLock)
							{
								if (_palettedExpandBuffer == null || _palettedExpandBuffer.Length < need) _palettedExpandBuffer = new byte[need];
								pooled = _palettedExpandBuffer;
							}
							var pal = texture.Palette32;
							if (opaque)
							{
								for (int p = 0; p < texture.Width * texture.Height; p++)
								{
									int idx = subBytes[p] & 0xFF;
									var c = pal != null && idx < pal.Length ? pal[idx] : new OpenBveApi.Colors.Color32(0,0,0,255);
									pooled[p*3] = c.R; pooled[p*3+1] = c.G; pooled[p*3+2] = c.B;
								}
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, pooled);
							}
							else
							{
								for (int p = 0; p < texture.Width * texture.Height; p++)
								{
									int idx = subBytes[p] & 0xFF;
									var c = pal != null && idx < pal.Length ? pal[idx] : new OpenBveApi.Colors.Color32(0,0,0,255);
									pooled[p*4] = c.R; pooled[p*4+1] = c.G; pooled[p*4+2] = c.B; pooled[p*4+3] = c.A;
								}
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, pooled);
							}
							GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
						}
						else
						{
							// RGB/RGBA direct – no expansion
							if (texture.PixelFormat == PixelFormat.RGBAlpha || texture.GetTransparencyType() != TextureTransparencyType.Opaque)
							{
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, subBytes);
							}
							else if (texture.PixelFormat == PixelFormat.RGB)
							{
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, subBytes);
							}
							else
							{
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, subBytes);
							}
							GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
						}
						// Keep handle in sync – both point to same GL call
						handle = texture;
						return true;
					}
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

			if (texture == null)
			{
				/*
				 * Reuse the register-time decode held by the texture cache where possible,
				 * as the cached instance already has the origin's parameters applied,
				 * avoiding a second full decode of the same file.
				 * NB: Origin equality only compares the path, so the cached instance must be
				 * checked against this handle's parameters before reuse- files registered with
				 * differing parameters (e.g. different clip regions) require their own decode.
				 */
				lock (TextureLookupLock)
				{
					Texture cachedTexture;
					if (textureCache.TryGetValue(handle.Origin, out cachedTexture))
					{
						// The cache value is the DecodedTexture (ByteArrayOrigin) created at registration – its Origin is not PathOrigin,
						// so the original check (cachedPathOrigin != null) never succeeds for decoded GIFs and caused a second decode (2× memory).
						// Reuse if the cached entry is animated (GIF video) or its ByteArrayOrigin, and parameters match (or both null).
						if (cachedTexture.MultipleFrames)
						{
							// Animated: reuse single copy to halve memory usage for large GIFs
							texture = cachedTexture;
						}
						else
						{
							PathOrigin cachedPathOrigin = cachedTexture.Origin as PathOrigin;
							PathOrigin handlePathOrigin = handle.Origin as PathOrigin;
							if (cachedPathOrigin != null && handlePathOrigin != null && cachedPathOrigin.Parameters == handlePathOrigin.Parameters)
							{
								texture = cachedTexture;
							}
							else if (cachedTexture.Origin is ByteArrayOrigin && handlePathOrigin != null)
							{
								// DecodedTexture path: key's Parameters are in handle.Origin; value has no Parameters to compare.
								// Reuse when handle has no special parameters to avoid duplicate decode.
								if (handlePathOrigin.Parameters == null)
									texture = cachedTexture;
							}
						}
					}
				}
				if (texture == null)
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
					// Fetch the pixel data once; the getter may lazily re-decode released instances, which must not happen per access
					byte[] textureBytes = texture.Bytes;
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
							case PixelFormat.Paletted:
								{
									// Expand indexed to RGB (alpha discarded for opaque)
									byte[] expanded = new byte[texture.Width * texture.Height * 3];
									var pal = texture.Palette32;
									for (int p = 0; p < texture.Width * texture.Height; p++)
									{
										int idx = textureBytes[p] & 0xFF;
										var c = pal != null && idx < pal.Length ? pal[idx] : new OpenBveApi.Colors.Color32(0,0,0,255);
										expanded[p*3] = c.R; expanded[p*3+1] = c.G; expanded[p*3+2] = c.B;
									}
									GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
									GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, texture.Width, texture.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, expanded);
									break;
								}
							case PixelFormat.Grayscale:
								// send as is to the luminance channel [NOTE: deprecated in GL4, so use Red channel instead]
								// n.b. Make sure to set the unpack alignment as otherwise we corrupt textures where stride > width
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									noLuminanceChannel ? PixelInternalFormat.R8 : PixelInternalFormat.Luminance,
									texture.Width, texture.Height, 0,
									noLuminanceChannel ? OpenTK.Graphics.OpenGL.PixelFormat.Red : OpenTK.Graphics.OpenGL.PixelFormat.Luminance,
									PixelType.UnsignedByte, textureBytes);
								
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
									PixelType.UnsignedByte, textureBytes);
								break;
							case PixelFormat.RGBAlpha:
								/*
								 * Opaque texture, so the alpha channel is discarded by the RGB8 internal format.
								 * Upload the RGBA data directly rather than stripping it CPU-side.
								 */
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexImage2D(TextureTarget.Texture2D, 0,
									PixelInternalFormat.Rgb8,
									texture.Width, texture.Height, 0,
									OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
									PixelType.UnsignedByte, textureBytes);
								break;
						}
					}
					else
					{
						switch (texture.PixelFormat)
						{
						case PixelFormat.Paletted:
							{
								byte[] expanded = new byte[texture.Width * texture.Height * 4];
								var pal = texture.Palette32;
								for (int p = 0; p < texture.Width * texture.Height; p++)
								{
									int idx = textureBytes[p] & 0xFF;
									var c = pal != null && idx < pal.Length ? pal[idx] : new OpenBveApi.Colors.Color32(0,0,0,255);
									expanded[p*4] = c.R; expanded[p*4+1] = c.G; expanded[p*4+2] = c.B; expanded[p*4+3] = c.A;
								}
								GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
								GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, texture.Width, texture.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, expanded);
								break;
							}
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
										newBytes[j + 0] = textureBytes[i + 0];
										newBytes[j + 1] = textureBytes[i + 0];
										newBytes[j + 2] = textureBytes[i + 0];
										newBytes[j + 3] = textureBytes[i + 1];
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
									PixelType.UnsignedByte, textureBytes);
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
									PixelType.UnsignedByte, textureBytes);
								break;
						}
						
					}
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    handle.OpenGlTextures[(int)wrap].Valid = true;
					if (texture.MultipleFrames)
					{
						texture.OpenGlTextures[(int)wrap].Valid = true;
					}
					else
					{
						/*
						 * The pixel data now lives in OpenGL, so release the retained CPU-side copies:
						 * the transient upload instance, the register-time decode held by the handle,
						 * and the entry in the texture cache.
						 * The data is lazily re-decoded from the origin if required again.
						 */
						texture.ReleaseBytes();
						Texture cachedTexture = null;
						lock (TextureLookupLock)
						{
							if (textureCache.TryGetValue(handle.Origin, out cachedTexture))
							{
								// Compute the transparency type whilst the data is still available,
								// as otherwise a later query would have to re-decode the file from disk
								cachedTexture.GetTransparencyType();
								cachedTexture.ReleaseBytes();
							}
						}
						if (handle.DecodedTexture != null && handle.DecodedTexture != cachedTexture)
						{
							handle.DecodedTexture.GetTransparencyType();
							handle.DecodedTexture.ReleaseBytes();
						}
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
				TextureOrigin key = null;
				if (texture.Origin != null && animatedTextures.ContainsKey(texture.Origin))
				{
					key = texture.Origin;
				}
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
				lock (TextureLookupLock)
				{
					textureCache.Remove(handle.Origin);
				}
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
			// Always clear animated texture cache to prevent memory leak on reload:
			// animatedTextures retains decoded frame data for every GIF ever loaded,
			// doubling memory on each reload because old entries are never removed.
			animatedTextures.Clear();

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
				lock (TextureLookupLock)
				{
					foreach (TextureOrigin origin in textureCache.Keys.ToList())
					{
						if (origin is PathOrigin && !TextureFileUnchanged(origin))
						{
							textureCache.Remove(origin);
						}
					}
				}
			}
			else
			{
				lock (TextureLookupLock)
				{
					textureCache.Clear();
				}
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
						if (!RegisteredTextureLookup.TryGetValue(pathOrigin.Path, out List<Texture> list))
						{
							list = new List<Texture>();
							RegisteredTextureLookup[pathOrigin.Path] = list;
						}
						list.Add(texture);
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
