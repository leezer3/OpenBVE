#pragma warning disable 0659, 0661

using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBveApi.Textures {

	/// <summary>Represents a texture.</summary>
	public class Texture
	{
		/// <summary>The size of the texture in pixels</summary>
		public Vector2 Size;
		/// <summary>The pixel format of the texture.</summary>
		public readonly PixelFormat PixelFormat;
		/// <summary>The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		private byte[][] MyBytes;
		/// <summary>The restricted color palette for this texture, or a null reference if the texture was 24/ 32 bit originally</summary>
		public readonly Color24[] Palette;
		/// <summary>The palette for indexed (Paletted) textures as 32-bit colors (includes alpha)</summary>
		public readonly Color32[] Palette32;
		/// <summary>Whether the texture is invalid and should be ignored</summary>
		/// <remarks>Set when loading the texture fails unexpectedly</remarks>
		public bool Ignore;
		/// <summary>The last access of this texture in clockticks</summary>
		/// <remarks>Used by the engine when deciding to unload unused textures</remarks>
		public int LastAccess;
		/// <summary>Whether the texture is available to unload</summary>
		public bool AvailableToUnload;
		/// <summary>Holds the OpenGL textures</summary>
		/// <remarks>An 3D array containing the OpenGL texture array for each frame</remarks>
		private readonly OpenGlTexture[][] MyOpenGlTextures;
		/// <summary>Holds the origin where this texture may be loaded from</summary>
		public readonly TextureOrigin Origin;
		/// <summary>The type of transparency used by this texture</summary>
		public TextureTransparencyType Transparency;
		/// <summary>Whether the texture has multiple frames</summary>
		public bool MultipleFrames;
		/// <summary>The current frame if multiple frames</summary>
		public int CurrentFrame = 0;
		/// <summary>The current frame interval in milliseconds if multiple frames</summary>
		public double FrameInterval = -1;
		/// <summary>The total number of frames</summary>
		public int TotalFrames;
		/// <summary>Whether this texture uses the compatible transparency mode (Matches to the nearest color in a restricted palette)</summary>
		public bool CompatibleTransparencyMode;
		/// <summary>The decoded texture, or a null reference if this texture could not be decoded.</summary>
		/// <remarks>Only set for textures registered from a path; allows the texture cache to be pre-seeded without re-reading and re-decoding the file.</remarks>
		public readonly Texture DecodedTexture;

		/// <summary>Gets the color of the given pixel</summary>
		/// <param name="pix">The pixel index</param>
		/// <param name="frame">The frame</param>
		private byte[] GetBytesForFrame(int frame)
		{
			if (Origin is StreamingGifOrigin sgo) return sgo.GetFrameBytes(frame) ?? MyBytes[0];
			if (MyBytes == null) return null;
			if (frame < MyBytes.Length) return MyBytes[frame];
			return MyBytes[0];
		}
		public Color24 GetPixel(int pix, int frame = 0)
		{
			if (pix > Size.X * Size.Y)
			{
				throw new ArgumentException("Pixel is outside the bounds of the image");
			}

			int firstByte;
			byte[] fBytes = GetBytesForFrame(frame);
			switch (PixelFormat)
			{
				case PixelFormat.Grayscale:
					firstByte = pix;
					return new Color24(fBytes[firstByte], fBytes[firstByte], fBytes[firstByte]);
				case PixelFormat.GrayscaleAlpha:
					firstByte = 2 * pix;
					return new Color24(fBytes[firstByte], fBytes[firstByte], fBytes[firstByte]);
				case PixelFormat.RGB:
					firstByte = 3 * pix;
					return new Color24(fBytes[firstByte], fBytes[firstByte + 1], fBytes[firstByte + 2]);
				case PixelFormat.RGBAlpha:
					firstByte = 4 * pix;
					return new Color24(fBytes[firstByte], fBytes[firstByte + 1], fBytes[firstByte + 2]);
				case PixelFormat.Paletted:
					{
						int idx = fBytes[pix] & 0xFF;
						if (Palette32 != null && idx < Palette32.Length) return new Color24(Palette32[idx].R, Palette32[idx].G, Palette32[idx].B);
						if (Palette != null && idx < Palette.Length) return Palette[idx];
						return Color24.Black;
					}
				default:
					throw new Exception("Unable to get a pixel value with invalid data.");
			}
		}

		/// <summary>Gets the alpha value of the given pixel</summary>
		/// <param name="pix">The pixel index</param>
		/// <param name="frame">The frame</param>
		/// <returns></returns>
		public byte GetAlpha(int pix, int frame = 0)
		{
			switch (PixelFormat)
			{
				case PixelFormat.Grayscale:
				case PixelFormat.RGB:
					return 255;
				case PixelFormat.GrayscaleAlpha:
					return GetBytesForFrame(frame)[2 * pix + 1];
				case PixelFormat.RGBAlpha:
					return GetBytesForFrame(frame)[4 * pix + 3];
				case PixelFormat.Paletted:
					{
						int idx = GetBytesForFrame(frame)[pix] & 0xFF;
						if (Palette32 != null && idx < Palette32.Length) return Palette32[idx].A;
						return 255;
					}
				default:
					return 255;
			}
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="pixelFormat">The pixel format.</param>
		/// <param name="bytes">The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</param>
		/// <param name="palette">The original color palette of the image (Before upsampling to 32-bit RGBA)</param>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per pixel is not 32.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the byte array is of unexpected length.</exception>
		public Texture(int width, int height, PixelFormat pixelFormat, byte[] bytes, Color24[] palette)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (bytes.Length != width * height * pixelFormat.BytesPerPixel())
			{
				throw new ArgumentException("The data bytes are not of the expected length.");
			}
			this.Origin = new ByteArrayOrigin(width, height, pixelFormat, bytes);
			this.MyOpenGlTextures = new OpenGlTexture[1][];
			this.MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
			this.Size.X = width;
			this.Size.Y = height;
			this.PixelFormat = pixelFormat;
			this.MyBytes = new byte[1][];
			this.MyBytes[0] = bytes;
			this.Palette = palette;
			this.Palette32 = palette != null ? Array.ConvertAll(palette, c => new Color32(c.R, c.G, c.B, 255)) : null;
		}

		/// <summary>Creates a new paletted texture with 32-bit palette (with alpha)</summary>
		public Texture(int width, int height, PixelFormat pixelFormat, byte[] bytes, Color32[] palette32)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (pixelFormat != PixelFormat.Paletted) throw new ArgumentException("Palette32 constructor requires Paletted format");
			if (bytes.Length != width * height * pixelFormat.BytesPerPixel())
				throw new ArgumentException("The data bytes are not of the expected length.");
			this.Origin = new ByteArrayOrigin(width, height, pixelFormat, bytes, palette32);
			this.MyOpenGlTextures = new OpenGlTexture[1][];
			this.MyOpenGlTextures[0] = new[] { new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture() };
			this.Size.X = width;
			this.Size.Y = height;
			this.PixelFormat = pixelFormat;
			this.MyBytes = new byte[1][];
			this.MyBytes[0] = bytes;
			this.Palette32 = palette32;
			this.Palette = palette32 != null ? Array.ConvertAll(palette32, c => new Color24(c.R, c.G, c.B)) : null;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="pixelFormat">The pixel format.</param>
		/// <param name="bytes">The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</param>
		/// <param name="frameInterval">The frame interval</param>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per pixel is not 32.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the byte array is of unexpected length.</exception>
		public Texture(int width, int height, PixelFormat pixelFormat, byte[][] bytes, double frameInterval)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (bytes[0].Length != width * height * pixelFormat.BytesPerPixel())
			{
				throw new ArgumentException("The data bytes are not of the expected length.");
			}

			Origin = new ByteArrayOrigin(width, height, pixelFormat, bytes, frameInterval);
			Size.X = width;
			Size.Y = height;
			PixelFormat = pixelFormat;
			MyBytes = bytes;
			Palette = null;
			Palette32 = null;
			MultipleFrames = true;
			FrameInterval = frameInterval;
			TotalFrames = bytes.Length;
			// Single GL name for all frames – TexSubImage2D updates in place to avoid GL leak
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new paletted animated texture</summary>
		public Texture(int width, int height, PixelFormat pixelFormat, byte[][] bytes, Color32[] palette32, double frameInterval)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (pixelFormat != PixelFormat.Paletted) throw new ArgumentException("Palette32 constructor requires Paletted format");
			if (bytes[0].Length != width * height * pixelFormat.BytesPerPixel())
				throw new ArgumentException("The data bytes are not of the expected length.");
			Origin = new ByteArrayOrigin(width, height, pixelFormat, bytes, palette32, frameInterval);
			Size.X = width;
			Size.Y = height;
			PixelFormat = pixelFormat;
			MyBytes = bytes;
			Palette32 = palette32;
			Palette = palette32 != null ? Array.ConvertAll(palette32, c => new Color24(c.R, c.G, c.B)) : null;
			MultipleFrames = true;
			FrameInterval = frameInterval;
			TotalFrames = bytes.Length;
			// Single GL name for all frames – TexSubImage2D updates in place to avoid GL leak
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] { new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture() };
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="currentHost">The callback function to the host application</param>
		public Texture(string path, TextureParameters parameters, Hosts.HostInterface currentHost)
		{
			Origin = new PathOrigin(path, parameters, currentHost);
			PixelFormat = Origin.GetTexture(out Texture t) ? t.PixelFormat : PixelFormat.Invalid;
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
			DecodedTexture = PixelFormat == PixelFormat.Invalid ? null : t;
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="bitmap">The System.Drawing.Bitmap that contains the texture.</param>
		public Texture(Bitmap bitmap)
		{
			Origin = new BitmapOrigin(bitmap);
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="bitmap">The System.Drawing.Bitmap that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		public Texture(Bitmap bitmap, TextureParameters parameters)
		{
			Origin = new BitmapOrigin(bitmap, parameters);
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="texture">The texture raw data.</param>
		public Texture(Texture texture)
		{
			Origin = new RawOrigin(texture);
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new texture from a texture origin.</summary>
		/// <param name="origin">The texture raw data.</param>
		public Texture(TextureOrigin origin)
		{
			Origin = origin;
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Streaming GIF – desktop OpenGL (not browser) keeps file bytes 36 MB instead of 1205×172k 208 MB, decodes on demand via TexSubImage</summary>
		public Texture(StreamingGifOrigin origin)
		{
			Origin = origin;
			Size = origin.GetSize();
			PixelFormat = PixelFormat.Paletted;
			Palette32 = origin.GetPalette();
			Palette = Palette32 != null ? Array.ConvertAll(Palette32, c => new Color24(c.R, c.G, c.B)) : null;
			MultipleFrames = true;
			TotalFrames = origin.GetFrameCount();
			FrameInterval = origin.GetInterval();
			MyBytes = new byte[1][];
			MyBytes[0] = origin.GetFrameBytes(0) ?? new byte[(int)Size.X * (int)Size.Y];
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Gets the width of the texture in pixels.</summary>
		public int Width
		{
			get => (int)Size.X;
			set => Size.X = value;
		}
		/// <summary>Gets the height of the texture in pixels.</summary>
		public int Height
		{
			get => (int)Size.Y;
			set => Size.Y = value;
		}
		
		/// <summary>Gets the aspect ratio of the texture</summary>
		public double AspectRatio => Size.X / Size.Y;
		
		/// <summary>Gets the texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		public byte[] Bytes
		{
			get
			{
				if (Origin is StreamingGifOrigin sgo && MultipleFrames)
				{
					return sgo.GetFrameBytes(CurrentFrame) ?? MyBytes[0];
				}
				if (MyBytes == null && Origin != null)
				{
					Origin.GetTexture(out Texture t);
					return t.Bytes;
				}
				if (MultipleFrames == false)
				{
					return MyBytes[0];
				}
				// Streaming: MyBytes holds 1 entry but TotalFrames clamped to avoid Out of range
				if (MyBytes.Length == 1 && TotalFrames > 1) return MyBytes[0];
				return MyBytes[CurrentFrame];
			}
		}

		/// <summary>Gets the OpenGL textures for this texture</summary>
		/// <remarks>If multiple frames, returns those for the current frame</remarks>
		public OpenGlTexture[] OpenGlTextures
		{
			get
			{
				if (Origin is StreamingGifOrigin) return MyOpenGlTextures[0]; // single GL name + TexSubImage
				if (MultipleFrames == false)
				{
					return MyOpenGlTextures[0];
				}
				if (MyOpenGlTextures.Length == 1) return MyOpenGlTextures[0];
				return MyOpenGlTextures[CurrentFrame];
			}
		}

		/// <summary>Checks whether two textures are equal.</summary>
		/// <param name="a">The first texture.</param>
		/// <param name="b">The second texture.</param>
		/// <returns>Whether the two textures are equal.</returns>
		public static bool operator ==(Texture a, Texture b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (a is null) return false;
			if (b is null) return false;
			if (a.MultipleFrames != b.MultipleFrames) return false;
			if (a.Origin != b.Origin) return false;
			if (a.Size != b.Size) return false;
			if (a.PixelFormat != b.PixelFormat) return false;
			if (a.MyBytes == null || b.MyBytes == null)
			{
				// The CPU-side data of one instance has been released after uploading to OpenGL, so fall back to reference equality
				return false;
			}
			if (a.MyBytes.Length != b.MyBytes.Length) return false;
			for (int i = 0; i < a.MyBytes.Length; i++)
			{
				if (a.MyBytes[i] != b.MyBytes[i]) return false;
			}
			return true;
		}

		/// <summary>Checks whether two textures are unequal.</summary>
		/// <param name="a">The first texture.</param>
		/// <param name="b">The second texture.</param>
		/// <returns>Whether the two textures are unequal.</returns>
		public static bool operator !=(Texture a, Texture b)
		{
			if (ReferenceEquals(a, b)) return false;
			if (a is null) return true;
			if (b is null) return true;
			if (a.MultipleFrames != b.MultipleFrames) return true;
			if (a.Origin != b.Origin) return true;
			if (a.Size != b.Size) return true;
			if (a.PixelFormat != b.PixelFormat) return true;
			if (a.MyBytes == null || b.MyBytes == null)
			{
				// The CPU-side data of one instance has been released after uploading to OpenGL, so fall back to reference equality
				return true;
			}
			if (a.MyBytes.Length != b.MyBytes.Length) return true;
			for (int i = 0; i < a.MyBytes.Length; i++)
			{
				if (a.MyBytes[i] != b.MyBytes[i]) return true;
			}
			return false;
		}

		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;
			if (obj is null) return false;
			if (!(obj is Texture x)) return false;
			if (MultipleFrames != x.MultipleFrames) return false;
			if (Origin != x.Origin) return false;
			if (Size != x.Size) return false;
			if (PixelFormat != x.PixelFormat) return false;
			if (MyBytes == null)
			{
				return x.MyBytes == null;
			}
			if (MyBytes.Length != x.MyBytes.Length) return false;
			for (int i = 0; i < MyBytes.Length; i++)
			{
				if (MyBytes[i] != x.MyBytes[i]) return false;
			}
			return true;
		}

		// --- functions ---
		/// <summary>Releases the retained CPU-side pixel data of this texture.</summary>
		/// <remarks>Once released, the data is lazily re-decoded from the origin if required again.
		/// Must not be used on multi-frame (animated) textures, whose frames are re-uploaded individually.</remarks>
		public void ReleaseBytes()
		{
			MyBytes = null;
		}

		/// <summary>Applies the specified parameters onto this texture.</summary>
		/// <param name="parameters">The parameters, or a null reference.</param>
		/// <returns>The texture with the parameters applied.</returns>
		/// <exception cref="System.ArgumentException">Raised when the clip region is outside the texture bounds.</exception>
		/// <exception cref="System.NotSupportedException">Raised when the bits per pixel in the texture is not supported.</exception>
		public Texture ApplyParameters(TextureParameters parameters)
		{
			return Functions.ApplyParameters(this, parameters);
		}

		/// <summary>Gets the type of transparency encountered in this texture.</summary>
		/// <returns>The type of transparency encountered in this texture.</returns>
		/// <exception cref="System.NotSupportedException">Raised when the bits per pixel in the texture is not supported.</exception>
		public TextureTransparencyType GetTransparencyType()
		{
			if (knownTransparencyType)
			{
				return transparencyType;
			}

			knownTransparencyType = true;
			switch (PixelFormat)
			{
				case PixelFormat.Paletted:
				if (Palette32 != null)
				{
					bool hasTransparent = false, hasOpaque = false, hasAlpha = false;
					// scan used indices vs palette alpha
					byte[] indices = null;
					if (Origin is StreamingGifOrigin sgo2) indices = sgo2.GetFrameBytes(CurrentFrame);
					else if (MyBytes != null) indices = CurrentFrame < MyBytes.Length ? MyBytes[CurrentFrame] : MyBytes[0];
					if (indices == null)
					{
						if (Origin == null || !Origin.GetTexture(out Texture released) || released == null) return TextureTransparencyType.Opaque;
						return released.GetTransparencyType();
					}
					bool[] used = new bool[256];
					for (int i = 0; i < indices.Length; i++) used[indices[i] & 0xFF] = true;
					for (int i = 0; i < 256; i++)
					{
						if (!used[i]) continue;
						byte a = i < Palette32.Length ? Palette32[i].A : (byte)255;
						if (a == 0) hasTransparent = true;
						else if (a == 255) hasOpaque = true;
						else hasAlpha = true;
					}
					if (hasAlpha) { transparencyType = TextureTransparencyType.Alpha; return transparencyType; }
					if (hasTransparent && hasOpaque) { transparencyType = TextureTransparencyType.Partial; return transparencyType; }
					if (hasTransparent && !hasOpaque) { transparencyType = TextureTransparencyType.Transparent; return transparencyType; }
				}
				transparencyType = TextureTransparencyType.Opaque;
				break;
				case PixelFormat.RGB:
					transparencyType = TextureTransparencyType.Opaque;
					break;
				case PixelFormat.RGBAlpha:
					transparencyType = TextureTransparencyType.Opaque;
					byte[] frameBytes = MyBytes?[CurrentFrame];
					if (frameBytes == null)
					{
						/*
						 * The CPU-side copy has been released after uploading to OpenGL.
						 * Obtain the data from the origin instead; the scan is then performed
						 * on the returned texture, which also caches the result there.
						 */
						if (Origin == null || !Origin.GetTexture(out Texture released) || released == null)
						{
							return transparencyType;
						}
						return released.GetTransparencyType();
					}
					for (int i = 3; i < frameBytes.Length; i += 4)
					{

						switch (frameBytes[i])
						{
							case 0:
								if (i == 3)
								{
									transparencyType = TextureTransparencyType.Transparent;
								}
								else
								{
									if (transparencyType == TextureTransparencyType.Opaque)
									{
										transparencyType = TextureTransparencyType.Partial;
									}
								}
								break;
							case 255:
								if (transparencyType != TextureTransparencyType.Opaque)
								{
									transparencyType = TextureTransparencyType.Partial;
								}
								// nothing
								break;
							default:
								transparencyType = TextureTransparencyType.Alpha;
								return transparencyType;
						}
					}

					return transparencyType;
				
			}
			return TextureTransparencyType.Opaque;
		}

		private bool knownTransparencyType;
		private TextureTransparencyType transparencyType;

		/// <summary>Inverts the lightness values of a texture used for a glow</summary>
		public void InvertLightness()
		{
			for (int frame = 0; frame < MyBytes.Length; frame++)
			{
				for (int i = 0; i < MyBytes[frame].Length; i += 4)
				{
					if (MyBytes[frame][i] != 0 | MyBytes[frame][i + 1] != 0 | MyBytes[frame][i + 2] != 0)
					{
						int r = MyBytes[frame][i + 0];
						int g = MyBytes[frame][i + 1];
						int b = MyBytes[frame][i + 2];
						if (g <= r & r <= b | b <= r & r <= g)
						{
							MyBytes[frame][i + 0] = (byte)(255 + r - g - b);
							MyBytes[frame][i + 1] = (byte)(255 - b);
							MyBytes[frame][i + 2] = (byte)(255 - g);
						}
						else if (r <= g & g <= b | b <= g & g <= r)
						{
							MyBytes[frame][i + 0] = (byte)(255 - b);
							MyBytes[frame][i + 1] = (byte)(255 + g - r - b);
							MyBytes[frame][i + 2] = (byte)(255 - r);
						}
						else
						{
							MyBytes[frame][i + 0] = (byte)(255 - g);
							MyBytes[frame][i + 1] = (byte)(255 - r);
							MyBytes[frame][i + 2] = (byte)(255 + b - r - g);
						}
					}
				}
			}
		}
	}
}
