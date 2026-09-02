#pragma warning disable 0659, 0661
using System;
using OpenBveApi.Colors;
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Textures
{
	/// <summary>Represents a texture originating from a byte array</summary>
	public class ByteArrayOrigin : TextureOrigin
	{
		private readonly byte[][] TextureBytes;

		private readonly int Width;

		private readonly int Height;

		private readonly PixelFormat Format;

		private readonly Color32[] Palette32;

		private readonly int NumberOfFrames;

		private readonly double FrameInterval;

		/// <summary>Creates a byte array origin</summary>
		/// <param name="width">The width of the underlying texture</param>
		/// <param name="height">The height of the underlying texture</param>
		/// <param name="bytes">The bytes</param>
		/// <param name="frameInterval">The frame interval</param>
		public ByteArrayOrigin(int width, int height, byte[][] bytes, double frameInterval)
			: this(width, height, PixelFormat.RGBAlpha, bytes, frameInterval)
		{
		}

		/// <summary>Creates a byte array origin</summary>
		/// <param name="width">The width of the underlying texture</param>
		/// <param name="height">The height of the underlying texture</param>
		/// <param name="pixelFormat">The pixel format of the data</param>
		/// <param name="bytes">The bytes</param>
		/// <param name="frameInterval">The frame interval</param>
		public ByteArrayOrigin(int width, int height, PixelFormat pixelFormat, byte[][] bytes, double frameInterval)
		{
			Width = width;
			Height = height;
			Format = pixelFormat;
			TextureBytes = bytes;
			Palette32 = null;
			FrameInterval = frameInterval;
			NumberOfFrames = bytes.Length;
		}

		/// <summary>Creates a paletted byte array origin (animated)</summary>
		public ByteArrayOrigin(int width, int height, PixelFormat pixelFormat, byte[][] bytes, Color32[] palette32, double frameInterval)
		{
			Width = width;
			Height = height;
			Format = pixelFormat;
			TextureBytes = bytes;
			Palette32 = palette32;
			FrameInterval = frameInterval;
			NumberOfFrames = bytes.Length;
		}

		/// <summary>Creates a byte array origin</summary>
		/// <param name="width">The width of the underlying texture</param>
		/// <param name="height">The height of the underlying texture</param>
		/// <param name="bytes">The bytes</param>
		public ByteArrayOrigin(int width, int height, byte[] bytes)
			: this(width, height, PixelFormat.RGBAlpha, bytes)
		{
		}

		/// <summary>Creates a byte array origin</summary>
		/// <param name="width">The width of the underlying texture</param>
		/// <param name="height">The height of the underlying texture</param>
		/// <param name="pixelFormat">The pixel format of the data</param>
		/// <param name="bytes">The bytes</param>
		public ByteArrayOrigin(int width, int height, PixelFormat pixelFormat, byte[] bytes)
		{
			Width = width;
			Height = height;
			Format = pixelFormat;
			TextureBytes = new[]
			{
				bytes
			};
			Palette32 = null;
			NumberOfFrames = 1;
		}

		/// <summary>Creates a paletted byte array origin</summary>
		public ByteArrayOrigin(int width, int height, PixelFormat pixelFormat, byte[] bytes, Color32[] palette32)
		{
			Width = width;
			Height = height;
			Format = pixelFormat;
			TextureBytes = new[] { bytes };
			Palette32 = palette32;
			NumberOfFrames = 1;
		}

		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public override bool GetTexture(out Texture texture)
		{
			if (Palette32 != null)
			{
				if (TextureBytes.Length == 1)
				{
					texture = new Texture(Width, Height, Format, TextureBytes[0], Palette32);
					return true;
				}
				texture = new Texture(Width, Height, Format, TextureBytes, Palette32, FrameInterval);
				return true;
			}
			if (TextureBytes.Length == 1)
			{
				texture = new Texture(Width, Height, Format, TextureBytes[0], Array.Empty<Color24>());
				return true;
			}
			texture = new Texture(Width, Height, Format, TextureBytes, FrameInterval);
			return true;
		}

		/// <summary>Checks whether two origins are equal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are equal.</returns>
		public static bool operator ==(ByteArrayOrigin a, ByteArrayOrigin b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (a is null) return false;
			if (b is null) return false;
			if (a.FrameInterval != b.FrameInterval) return false;
			if (a.NumberOfFrames != b.NumberOfFrames) return false;
			if (a.Width != b.Width) return false;
			if (a.Height != b.Height) return false;
			return a.TextureBytes == b.TextureBytes;
		}

		/// <summary>Checks whether two origins are unequal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are unequal.</returns>
		public static bool operator !=(ByteArrayOrigin a, ByteArrayOrigin b)
		{
			if (ReferenceEquals(a, b)) return false;
			if (a is null) return true;
			if (b is null) return true;
			if (a.FrameInterval == b.FrameInterval) return false;
			if (a.NumberOfFrames == b.NumberOfFrames) return false;
			if (a.Width == b.Width) return false;
			if (a.Height == b.Height) return false;
			return a.TextureBytes != b.TextureBytes;
		}

		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;
			if (obj is null) return false;
			if (!(obj is ByteArrayOrigin)) return false;
			if (FrameInterval != ((ByteArrayOrigin)obj).FrameInterval) return false;
			if (NumberOfFrames != ((ByteArrayOrigin)obj).NumberOfFrames) return false;
			if (Width != ((ByteArrayOrigin)obj).Width) return false;
			if (Height != ((ByteArrayOrigin)obj).Height) return false;
			return ReferenceEquals(TextureBytes, ((ByteArrayOrigin)obj).TextureBytes);
		}

		/// <summary>Returns the hash code for this origin.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + Width.GetHashCode();
				hash = hash * 31 + Height.GetHashCode();
				hash = hash * 31 + NumberOfFrames.GetHashCode();
				hash = hash * 31 + FrameInterval.GetHashCode();
				hash = hash * 31 + (TextureBytes?.GetHashCode() ?? 0);
				return hash;
			}
		}

	}
}
