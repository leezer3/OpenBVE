namespace OpenBveApi.Objects
{
	/// <summary>The different modes controlling object loading</summary>
	public enum ObjectLoadMode
	{
		/// <summary>Textures may be loaded / unloaded at will</summary>
		Normal = 0,
		/// <summary>Textures must remain in memory at all times (e.g. Dynamically generated)</summary>
		DontAllowUnloadOfTextures = 1
	}
}
