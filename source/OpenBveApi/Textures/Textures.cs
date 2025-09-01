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
		private Vector2 MySize;
		/// <summary>The pixel format of the texture.</summary>
		private readonly PixelFormat MyPixelFormat;
		/// <summary>The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		private readonly byte[][] MyBytes;
		/// <summary>The restricted color palette for this texture, or a null reference if the texture was 24/ 32 bit originally</summary>
		private readonly Color24[] MyPalette;
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
		/// <summary>Whether this texture uses the compatible transparency mode (Matches to the nearest color in a restricted pallete)</summary>
		public bool CompatibleTransparencyMode;

		/// <summary>Gets the color of the given pixel</summary>
		/// <param name="X">The X-coordinate of the pixel</param>
		/// <param name="Y">The Y-coordinate of the pixel</param>
		/// <param name="frame">The frame</param>
		/// <returns></returns>
		public Color24 GetPixel(int X, int Y, int frame = 0)
		{
			if (X > MySize.X)
			{
				throw new ArgumentException("X is outside the bounds of the image");
			}

			if (Y > MySize.Y)
			{
				throw new ArgumentException("Y is outside the bounds of the image");
			}

			int firstByte = 4 * X * Y;
			return new Color24(MyBytes[frame][firstByte], MyBytes[frame][firstByte + 1], MyBytes[frame][firstByte + 2]);
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="pixelFormat">The pixel format.</param>
		/// <param name="bytes">The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</param>
		/// <param name="palette">The original color pallete of the image (Before upsampling to 32-bit RGBA)</param>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per pixel is not 32.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the byte array is of unexpected length.</exception>
		public Texture(int width, int height, PixelFormat pixelFormat, byte[] bytes, Color24[] palette)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (bytes.Length != width * height * (int)pixelFormat)
			{
				throw new ArgumentException("The data bytes are not of the expected length.");
			}
			this.Origin = new ByteArrayOrigin(width, height, bytes);
			this.MyOpenGlTextures = new OpenGlTexture[1][];
			this.MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
			this.MySize.X = width;
			this.MySize.Y = height;
			this.MyPixelFormat = pixelFormat;
			this.MyBytes = new byte[1][];
			this.MyBytes[0] = bytes;
			this.MyPalette = palette;
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

			if (bytes[0].Length != width * height * (int)pixelFormat)
			{
				throw new ArgumentException("The data bytes are not of the expected length.");
			}

			Origin = new ByteArrayOrigin(width, height, bytes, frameInterval);
			MySize.X = width;
			MySize.Y = height;
			MyPixelFormat = pixelFormat;
			MyBytes = bytes;
			MyPalette = null;
			MultipleFrames = true;
			FrameInterval = frameInterval;
			TotalFrames = bytes.Length;
			MyOpenGlTextures = new OpenGlTexture[bytes.Length][];
			for (int i = 0; i < bytes.Length; i++)
			{
				MyOpenGlTextures[i] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
			}
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="currentHost">The callback function to the host application</param>
		public Texture(string path, TextureParameters parameters, Hosts.HostInterface currentHost)
		{
			Origin = new PathOrigin(path, parameters, currentHost);
			MyOpenGlTextures = new OpenGlTexture[1][];
			MyOpenGlTextures[0] = new[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
			
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

		/// <summary>Gets the width of the texture in pixels.</summary>
		public int Width
		{
			get => (int)MySize.X;
			set => MySize.X = value;
		}
		/// <summary>Gets the height of the texture in pixels.</summary>
		public int Height
		{
			get => (int)MySize.Y;
			set => MySize.Y = value;
		}

		/// <summary>Gets the size of the texture in pixels</summary>
		public Vector2 Size
		{
			get => MySize;
			set => MySize = value;
		}

		/// <summary>Gets the aspect ratio of the texture</summary>
		public double AspectRatio => MySize.X / MySize.Y;

		/// <summary>Gets the pixel format.</summary>
		public PixelFormat PixelFormat => MyPixelFormat;

		/// <summary>Gets the restricted color palette for this texture, or a null reference if not applicable</summary>
		public Color24[] Palette => MyPalette;

		/// <summary>Gets the texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		public byte[] Bytes
		{
			get
			{
				if (MultipleFrames == false)
				{
					return MyBytes[0];
				}
				return MyBytes[CurrentFrame];
			}
		}

		/// <summary>Gets the OpenGL textures for this texture</summary>
		/// <remarks>If multiple frames, returns those for the current frame</remarks>
		public OpenGlTexture[] OpenGlTextures
		{
			get
			{
				if (MultipleFrames == false)
				{
					return MyOpenGlTextures[0];
				}
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
			if (a.MySize.X != b.MySize.X) return false;
			if (a.MySize.Y != b.MySize.Y) return false;
			if (a.MyPixelFormat != b.MyPixelFormat) return false;
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
			if (a.MySize.X != b.MySize.X) return true;
			if (a.MySize.Y != b.MySize.Y) return true;
			if (a.MyPixelFormat != b.MyPixelFormat) return true;
			if (a.MyBytes == null)
			{
				return b.MyBytes != null;
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
			if (MySize.X != x.MySize.X) return false;
			if (MySize.Y != x.MySize.Y) return false;
			if (MyPixelFormat != x.MyPixelFormat) return false;
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
			switch (MyPixelFormat)
			{
				case PixelFormat.RGB:
					transparencyType = TextureTransparencyType.Opaque;
					break;
				case PixelFormat.RGBAlpha:
					for (int i = 3; i < this.MyBytes[CurrentFrame].Length; i += 4)
					{
						if (this.MyBytes[CurrentFrame][i] != 255)
						{
							for (int j = i; j < this.MyBytes[CurrentFrame].Length; j += 4)
							{
								if (this.MyBytes[CurrentFrame][j] != 0 & this.MyBytes[CurrentFrame][j] != 255)
								{
									transparencyType = TextureTransparencyType.Alpha;
									return TextureTransparencyType.Alpha;
								}
							}

							transparencyType = TextureTransparencyType.Partial;
							return TextureTransparencyType.Partial;
						}
					}
					break;
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
				for (int i = 0; i < MyBytes.Length; i += 4)
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
