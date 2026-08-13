namespace OpenBveApi.Textures
{
	/// <summary>Represents texture raw data.</summary>
	public class RawOrigin : TextureOrigin
	{
		// --- members ---
		/// <summary>The texture raw data.</summary>
		public readonly Texture Texture;

		// --- constructors ---
		/// <summary>Creates a new raw data origin.</summary>
		/// <param name="texture">The texture raw data.</param>
		public RawOrigin(Texture texture)
		{
			Texture = texture;
		}

		/// <summary>Checks whether two origins are equal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are equal.</returns>
		public static bool operator ==(RawOrigin a, RawOrigin b)
		{
			return false;
		}

		/// <summary>Checks whether two origins are unequal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are unequal.</returns>
		public static bool operator !=(RawOrigin a, RawOrigin b)
		{
			return true;
		}

		// --- functions ---
		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public override bool GetTexture(out Texture texture)
		{
			texture = Texture;
			return true;
		}
	}
}
