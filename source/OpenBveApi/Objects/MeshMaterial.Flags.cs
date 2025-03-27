using System;

namespace OpenBveApi.Objects
{
	/// <summary>The flags which may be applied to material properties</summary>
	[Flags]
	public enum MaterialFlags
	{
		/// <summary>The material has no special properties</summary>
		None = 0,
		/// <summary>The material is emissive</summary>
		Emissive = 1,
		/// <summary>The material uses the Transparent Color in its texture</summary>
		TransparentColor = 2,
		/// <summary>The material is to be rendered with disabled lighting</summary>
		DisableLighting = 4,
		/// <summary>The daytime and night-time textures are to be cross-faded</summary>
		/// <remarks>Disabled by default</remarks>
		CrossFadeTexture = 8,
		/// <summary>Alpha is disabled when rendering</summary>
		DisableTextureAlpha = 16
	}
}
