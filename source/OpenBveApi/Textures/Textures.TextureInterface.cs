namespace OpenBveApi.Textures
{
	/// <summary>Represents the interface for loading textures. Plugins must implement this interface if they wish to expose textures.</summary>
	public abstract class TextureInterface
	{
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public virtual void Load(Hosts.HostInterface host)
		{
		}

		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload()
		{
		}

		/// <summary>Sets various hacks to workaround buggy textures</summary>
		public virtual void SetCompatabilityHacks(CompatabilityHacks enabledHacks)
		{

		}

		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		public abstract bool CanLoadTexture(string path);

		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public abstract bool QueryTextureDimensions(string path, out int width, out int height);

		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public abstract bool LoadTexture(string path, out Texture texture);
	}

	/// <summary>Controls various hacks used with older content</summary>
	public struct CompatabilityHacks
	{
		/// <summary>Transparency color depth should be reduced to 256 colors</summary>
		public bool ReduceTransparencyColorDepth;
	}
}
