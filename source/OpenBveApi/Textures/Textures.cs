﻿#pragma warning disable 0659, 0661

using System;
using System.Drawing;
using OpenBveApi.Colors;

namespace OpenBveApi.Textures {

	/// <summary>Represents a texture.</summary>
	public class Texture
	{
		// --- members ---
		/// <summary>The width of the texture in pixels.</summary>
		private int MyWidth;
		/// <summary>The height of the texture in pixels.</summary>
		private int MyHeight;
		/// <summary>The number of bits per pixel. Must be 32.</summary>
		private readonly int MyBitsPerPixel;
		/// <summary>The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		private readonly byte[] MyBytes;
		/// <summary>The restricted color palette for this texture, or a null reference if the texture was 24/ 32 bit originally</summary>
		private readonly Color24[] MyPalette;
		/// <summary>Whether the texture is invalid and should be ignored</summary>
		/// <remarks>Set when loading the texture fails unexpectedly</remarks>
		public bool Ignore;
		/// <summary>The last access of this texture in clockticks</summary>
		/// <remarks>Used by the engine when deciding to unload unused textures</remarks>
		public double LastAccess;
		/// <summary>Holds the OpenGL textures</summary>
		public readonly OpenGlTexture[] OpenGlTextures;
		/// <summary>Holds the origin where this texture may be loaded from</summary>
		public readonly TextureOrigin Origin;
		/// <summary>The type of transparency used by this texture</summary>
		public TextureTransparencyType Transparency;

		/// <summary>Whether this texture uses the compatible transparency mode (Matches to the nearest color in a restricted pallete)</summary>
		public bool CompatibleTransparencyMode;

		/// <summary>Gets the color of the given pixel</summary>
		/// <param name="X">The X-coordinate of the pixel</param>
		/// <param name="Y">The Y-coordinate of the pixel</param>
		/// <returns></returns>
		public Color24 GetPixel(int X, int Y)
		{
			if (X > MyWidth)
			{
				throw new ArgumentException("X is outside the bounds of the image");
			}

			if (Y > MyWidth)
			{
				throw new ArgumentException("Y is outside the bounds of the image");
			}

			int firstByte = 4 * X * Y;
			return new Color24(MyBytes[firstByte], MyBytes[firstByte + 1], MyBytes[firstByte + 2]);
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="bitsPerPixel">The number of bits per pixel. Must be 32.</param>
		/// <param name="bytes">The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</param>
		/// <param name="palette">The original color pallete of the image (Before upsampling to 32-bit RGBA)</param>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per pixel is not 32.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the byte array is of unexpected length.</exception>
		public Texture(int width, int height, int bitsPerPixel, byte[] bytes, Color24[] palette)
		{
			if (bitsPerPixel != 32)
			{
				throw new ArgumentException("The number of bits per pixel is supported.");
			}

			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}

			if (bytes.Length != 4 * width * height)
			{
				throw new ArgumentException("The data bytes are not of the expected length.");
			}

			this.MyWidth = width;
			this.MyHeight = height;
			this.MyBitsPerPixel = bitsPerPixel;
			this.MyBytes = bytes;
			this.MyPalette = palette;
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="path">The path to the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="currentHost">The callback function to the host application</param>
		public Texture(string path, TextureParameters parameters, Hosts.HostInterface currentHost)
		{
			this.Origin = new PathOrigin(path, parameters, currentHost);
			this.OpenGlTextures = new OpenGlTexture[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="bitmap">The System.Drawing.Bitmap that contains the texture.</param>
		public Texture(Bitmap bitmap)
		{
			this.Origin = new BitmapOrigin(bitmap);
			this.OpenGlTextures = new OpenGlTexture[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="bitmap">The System.Drawing.Bitmap that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		public Texture(Bitmap bitmap, TextureParameters parameters)
		{
			this.Origin = new BitmapOrigin(bitmap, parameters);
			this.OpenGlTextures = new OpenGlTexture[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		/// <summary>Creates a new texture.</summary>
		/// <param name="texture">The texture raw data.</param>
		public Texture(OpenBveApi.Textures.Texture texture)
		{
			this.Origin = new RawOrigin(texture);
			this.OpenGlTextures = new OpenGlTexture[] {new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture(), new OpenGlTexture()};
		}

		// --- properties ---
		/// <summary>Gets the width of the texture in pixels.</summary>
		public int Width
		{
			get
			{
				return this.MyWidth;
			}
			set
			{
				this.MyWidth = value;
			}
		}
		/// <summary>Gets the height of the texture in pixels.</summary>
		public int Height
		{
			get
			{
				return this.MyHeight;
			}
			set
			{
				this.MyHeight = value;
			}
		}
		/// <summary>Gets the number of bits per pixel.</summary>
		public int BitsPerPixel
		{
			get
			{
				return this.MyBitsPerPixel;
			}
		}

		/// <summary>Gets the restricted color palette for this texture, or a null reference if not applicable</summary>
		public Color24[] Palette
		{
			get
			{
				return this.MyPalette;
			}
		}

		/// <summary>Gets the texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue and alpha.</summary>
		public byte[] Bytes
		{
			get
			{
				return this.MyBytes;
			}
		}

		// --- operators ---
		/// <summary>Checks whether two textures are equal.</summary>
		/// <param name="a">The first texture.</param>
		/// <param name="b">The second texture.</param>
		/// <returns>Whether the two textures are equal.</returns>
		public static bool operator ==(Texture a, Texture b)
		{
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
			if (a.Origin != b.Origin) return false;
			if (a.MyWidth != b.MyWidth) return false;
			if (a.MyHeight != b.MyHeight) return false;
			if (a.MyBitsPerPixel != b.MyBitsPerPixel) return false;
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
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
			if (a.Origin != b.Origin) return true;
			if (a.MyWidth != b.MyWidth) return true;
			if (a.MyHeight != b.MyHeight) return true;
			if (a.MyBitsPerPixel != b.MyBitsPerPixel) return true;
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
			if (object.ReferenceEquals(this, obj)) return true;
			if (object.ReferenceEquals(this, null)) return false;
			if (object.ReferenceEquals(obj, null)) return false;
			if (!(obj is Texture)) return false;
			Texture x = (Texture) obj;
			if (this.Origin != x.Origin) return false;
			if (this.MyWidth != x.MyWidth) return false;
			if (this.MyHeight != x.MyHeight) return false;
			if (this.MyBitsPerPixel != x.MyBitsPerPixel) return false;
			if (this.MyBytes.Length != x.MyBytes.Length) return false;
			for (int i = 0; i < this.MyBytes.Length; i++)
			{
				if (this.MyBytes[i] != x.MyBytes[i]) return false;
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
			if (this.MyBitsPerPixel == 24)
			{
				return TextureTransparencyType.Opaque;
			}

			if (this.MyBitsPerPixel == 32)
			{
				for (int i = 3; i < this.MyBytes.Length; i += 4)
				{
					if (this.MyBytes[i] != 255)
					{
						for (int j = i; j < this.MyBytes.Length; j += 4)
						{
							if (this.MyBytes[j] != 0 & this.MyBytes[j] != 255)
							{
								return TextureTransparencyType.Alpha;
							}
						}

						return TextureTransparencyType.Partial;
					}
				}

				return TextureTransparencyType.Opaque;
			}

			throw new NotSupportedException();
		}
	}
}
