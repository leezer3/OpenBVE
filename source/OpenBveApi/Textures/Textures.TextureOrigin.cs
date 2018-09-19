namespace OpenBveApi.Textures
{
	/// <summary>Represents the origin where the texture can be loaded from.</summary>
	public abstract class TextureOrigin
	{
		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public abstract bool GetTexture(out OpenBveApi.Textures.Texture texture);
		// --- operators ---
		/// <summary>Checks whether two origins are equal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are equal.</returns>
		public static bool operator ==(TextureOrigin a, TextureOrigin b)
		{
			if (a != null && b != null && a.GetType() == b.GetType())
			{
				return a == b;
			}
			return object.ReferenceEquals(a, b);
		}

		/// <summary>Checks whether two origins are unequal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are unequal.</returns>
		public static bool operator !=(TextureOrigin a, TextureOrigin b)
		{
			if (a != null && b != null && a.GetType() == b.GetType())
			{
				return a != b;
			}
			return !object.ReferenceEquals(a, b);
		}

		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj)
		{
			//if (obj != null && this.GetType() == obj.GetType())
			//{
			//	return this == obj;
			//}
			return object.ReferenceEquals(this, obj);
		}
	}
}
