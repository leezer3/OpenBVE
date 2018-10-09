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
			this.Texture = texture;
		}

		// --- functions ---
		/// <summary>Gets the texture from this origin.</summary>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether the texture could be obtained successfully.</returns>
		public override bool GetTexture(out Texture texture)
		{
			texture = this.Texture;
			return true;
		}
	}
}
