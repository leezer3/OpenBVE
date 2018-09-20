#pragma warning disable 0659, 0661

namespace OpenBveApi.Textures
{
	/// <summary>Represents the origin where the texture can be loaded from.</summary>
	public abstract class TextureOrigin
	{
		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public abstract bool GetTexture(out Texture texture);
		// --- operators ---
		/// <summary>Checks whether two origins are equal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are equal.</returns>
		public static bool operator ==(TextureOrigin a, TextureOrigin b)
		{
			if ((object)a == null && (object)b == null)
			{
				return true; //Two nulls are always equal
			}
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (a is PathOrigin && b is PathOrigin)
			{
				return (PathOrigin)a == (PathOrigin)b;
			}
			if (a is BitmapOrigin && b is BitmapOrigin)
			{
				return (BitmapOrigin)a == (BitmapOrigin)b;
			}
			if (a is RawOrigin && b is RawOrigin)
			{
				return (RawOrigin)a == (RawOrigin)b;
			}
			return false;
		}

		/// <summary>Checks whether two origins are unequal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are unequal.</returns>
		public static bool operator !=(TextureOrigin a, TextureOrigin b)
		{
			if ((object)a == null && (object)b == null)
			{
				return false; //Two nulls are always equal
			}
			if (ReferenceEquals(a, b))
			{
				return false;
			}

			if (a is PathOrigin && b is PathOrigin)
			{
				return (PathOrigin)a != (PathOrigin)b;
			}
			if (a is BitmapOrigin && b is BitmapOrigin)
			{
				return (BitmapOrigin)a != (BitmapOrigin)b;
			}
			if (a is RawOrigin && b is RawOrigin)
			{
				return (RawOrigin)a != (RawOrigin)b;
			}
			return true;
		}

		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false; //Two nulls are always equal
			}
			if (ReferenceEquals(this, obj))
			{
				return false;
			}

			if (this is PathOrigin && obj is PathOrigin)
			{
				return (PathOrigin)this != (PathOrigin)obj;
			}
			if (this is BitmapOrigin && obj is BitmapOrigin)
			{
				return (BitmapOrigin)this != (BitmapOrigin)obj;
			}
			if (this is RawOrigin && obj is RawOrigin)
			{
				return (RawOrigin)this != (RawOrigin)obj;
			}
			return true;
		}
	}
}
