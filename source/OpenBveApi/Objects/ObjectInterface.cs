namespace OpenBveApi.Objects
{
	/// <summary>Represents the interface for loading objects. Plugins must implement this interface if they wish to expose objects.</summary>
	public abstract class ObjectInterface
	{
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		/// <param name="fileSystem">The program filesystem object</param>
		public virtual void Load(Hosts.HostInterface host, FileSystem.FileSystem fileSystem)
		{
		}
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload()
		{
		}

		/// <summary>Sets various hacks to workaround buggy objects</summary>
		public virtual void SetCompatibilityHacks(bool BveTsHacks, bool CylinderHack)
		{
		}

		/// <summary>Sets the parser type to use</summary>
		/// <remarks>The actual type of parserType should be checked by the consumer and ignored if not relevant</remarks>
		public virtual void SetObjectParser(object parserType)
		{

		}

		/// <summary>Checks whether the plugin can load the specified object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <returns>Whether the plugin can load the specified object.</returns>
		public abstract bool CanLoadObject(string path);

		/// <summary>Loads the specified object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="unifiedObject">Receives the object.</param>
		/// <param name="Encoding">The encoding for the object</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public abstract bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject unifiedObject);
	}
}
