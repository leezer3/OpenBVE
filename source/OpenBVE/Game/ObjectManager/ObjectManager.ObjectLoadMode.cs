namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		internal enum ObjectLoadMode
		{
			/// <summary>Textures may be loaded / unloaded at will</summary>
			Normal = 0,
			/// <summary>Textures must remain in memory at all time (e.g. Dynamically generated)</summary>
			DontAllowUnloadOfTextures = 1
		}
	}
}
