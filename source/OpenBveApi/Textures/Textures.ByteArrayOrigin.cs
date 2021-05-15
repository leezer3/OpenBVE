#pragma warning disable 0659, 0661
using OpenBveApi.Colors;

namespace OpenBveApi.Textures
{
	/// <summary>Represents a texture originating from a byte array</summary>
	public class ByteArrayOrigin : TextureOrigin
	{
		private readonly byte[][] TextureBytes;

		private readonly int Width;

		private readonly int Height;

		private readonly int NumberOfFrames;

		private readonly double FrameInterval;

		/// <summary>Creates a byte array origin</summary>
		/// <param name="width">The width of the underlying texture</param>
		/// <param name="height">The height of the underlying texture</param>
		/// <param name="bytes">The bytes</param>
		/// <param name="frameInterval">The frame interval</param>
		public ByteArrayOrigin(int width, int height, byte[][] bytes, double frameInterval)
		{
			Width = width;
			Height = height;
			TextureBytes = bytes;
			FrameInterval = frameInterval;
			NumberOfFrames = bytes.Length;
		}

		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public override bool GetTexture(out Texture texture)
		{
			if (TextureBytes.Length == 1)
			{
				texture = new Texture(Width, Height, 32, TextureBytes[0], new Color24[0]);
				return true;
			}
			texture = new Texture(Width, Height, 32, TextureBytes, FrameInterval);
			return true;
		}

		/// <summary>Checks whether two origins are equal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are equal.</returns>
		public static bool operator ==(ByteArrayOrigin a, ByteArrayOrigin b)
		{
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
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
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
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
			if (object.ReferenceEquals(this, obj)) return true;
			if (object.ReferenceEquals(this, null)) return false;
			if (object.ReferenceEquals(obj, null)) return false;
			if (!(obj is ByteArrayOrigin)) return false;
			if (FrameInterval == ((ByteArrayOrigin)obj).FrameInterval) return false;
			if (NumberOfFrames == ((ByteArrayOrigin)obj).NumberOfFrames) return false;
			if (Width == ((ByteArrayOrigin)obj).Width) return false;
			if (Height == ((ByteArrayOrigin)obj).Height) return false;
			return TextureBytes != ((ByteArrayOrigin)obj).TextureBytes;
		}

	}
}
