namespace OpenBveApi.Textures
{
	/// <summary>Represents the type of transparency encountered in a texture.</summary>
	public enum TextureTransparencyType
	{
		/// <summary>All pixels in the texture are fully opaque.</summary>
		Opaque = 1,
		/// <summary>All pixels in the texture are either fully opaque or fully transparent.</summary>
		Partial = 2,
		/// <summary>Some pixels in the texture are neither fully opaque nor fully transparent.</summary>
		Alpha = 3
	}
}
