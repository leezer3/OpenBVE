#pragma warning disable 0659, 0661
using System;

namespace OpenBveApi.Textures
{
	/// <summary>Represents a region in a texture to be extracted.</summary>
	public class TextureClipRegion
	{
		// --- members ---
		/// <summary>The left coordinate.</summary>
		private int MyLeft;
		/// <summary>The top coordinate.</summary>
		private int MyTop;
		/// <summary>The width.</summary>
		private int MyWidth;
		/// <summary>The height.</summary>
		private int MyHeight;
		// --- properties ---
		/// <summary>Gets the left coordinate.</summary>
		public int Left
		{
			get
			{
				return this.MyLeft;
			}
		}
		/// <summary>Gets the top coordinate.</summary>
		public int Top
		{
			get
			{
				return this.MyTop;
			}
		}
		/// <summary>Gets the width.</summary>
		public int Width
		{
			get
			{
				return this.MyWidth;
			}
		}
		/// <summary>Gets the height.</summary>
		public int Height
		{
			get
			{
				return this.MyHeight;
			}
		}

		// --- constructors ---
		/// <summary>Creates a new clip region.</summary>
		/// <param name="left">The left coordinate.</param>
		/// <param name="top">The top coordinate.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <exception cref="System.ArgumentException">Raised when the left or top are negative.</exception>
		/// <exception cref="System.ArgumentException">Raised when the width or height are non-positive.</exception>
		public TextureClipRegion(int left, int top, int width, int height)
		{
			if (left < 0 | top < 0)
			{
				throw new ArgumentException("The left or top coordinates are negative.");
			}
			else if (width <= 0 | height <= 0)
			{
				throw new ArgumentException("The width or height are non-positive.");
			}
			else
			{
				this.MyLeft = left;
				this.MyTop = top;
				this.MyWidth = width;
				this.MyHeight = height;
			}
		}

		// --- operators ---
		/// <summary>Checks whether two clip regions are equal.</summary>
		/// <param name="a">The first clip region.</param>
		/// <param name="b">The second clip region.</param>
		/// <returns>Whether the two clip regions are equal.</returns>
		public static bool operator ==(TextureClipRegion a, TextureClipRegion b)
		{
			if (object.ReferenceEquals(a, b)) return true;
			if (object.ReferenceEquals(a, null)) return false;
			if (object.ReferenceEquals(b, null)) return false;
			if (a.MyLeft != b.MyLeft) return false;
			if (a.MyTop != b.MyTop) return false;
			if (a.MyWidth != b.MyWidth) return false;
			if (a.MyHeight != b.MyHeight) return false;
			return true;
		}

		/// <summary>Checks whether two clip regions are unequal.</summary>
		/// <param name="a">The first clip region.</param>
		/// <param name="b">The second clip region.</param>
		/// <returns>Whether the two clip regions are unequal.</returns>
		public static bool operator !=(TextureClipRegion a, TextureClipRegion b)
		{
			if (object.ReferenceEquals(a, b)) return false;
			if (object.ReferenceEquals(a, null)) return true;
			if (object.ReferenceEquals(b, null)) return true;
			if (a.MyLeft != b.MyLeft) return true;
			if (a.MyTop != b.MyTop) return true;
			if (a.MyWidth != b.MyWidth) return true;
			if (a.MyHeight != b.MyHeight) return true;
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
			if (!(obj is TextureClipRegion)) return false;
			TextureClipRegion x = (TextureClipRegion) obj;
			if (this.MyLeft != x.MyLeft) return false;
			if (this.MyTop != x.MyTop) return false;
			if (this.MyWidth != x.MyWidth) return false;
			if (this.MyHeight != x.MyHeight) return false;
			return true;
		}
	}
}
