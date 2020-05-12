using System;

namespace OpenBveApi.Objects
{
	/// <summary>The flags which may be applied to material properties</summary>
	[Flags]
	public enum MaterialFlags
	{
		/// <summary>The material is emissive</summary>
		Emissive = 1,
		/// <summary>The material uses the Transparent Color in its texture</summary>
		TransparentColor = 2,
	}
}
