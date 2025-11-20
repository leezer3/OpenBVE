namespace OpenBveApi.Objects
{
	/// <summary>Represents the interface for loading objects. Plugins must implement this interface if they wish to expose objects.</summary>
	public abstract class ObjectInterface
	{
		/// <summary>
		/// Array of supported animated object extensions.
		/// </summary>
		public virtual string[] SupportedAnimatedObjectExtensions => new string[0];

		/// <summary>
		/// Array of supported static object extensions.
		/// </summary>
		public virtual string[] SupportedStaticObjectExtensions => new string[0];

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
		public virtual void SetCompatibilityHacks(CompatabilityHacks EnabledHacks)
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
		/// <param name="textEncoding">The encoding for the object</param>
		/// <returns>Whether loading the object was successful.</returns>
		public abstract bool LoadObject(string path, System.Text.Encoding textEncoding, out UnifiedObject unifiedObject);

		/// <summary>Loads the specified object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="unifiedObject">Receives the object.</param>
		/// <param name="wagonFileDirectory">The path to the wagon file</param>
		/// <param name="Encoding">The encoding for the object</param>
		/// <returns>Whether loading the object was successful.</returns>
		/// <remarks>Useful for loading MSTS content only</remarks>
		public virtual bool LoadObject(string path, string wagonFileDirectory, System.Text.Encoding Encoding, out UnifiedObject unifiedObject)
		{
			unifiedObject = null;
			return false;
		}
	}

	/// <summary>Controls various hacks used with older content</summary>
	public struct CompatabilityHacks
	{
		/// <summary>Looser parsing model is used in various cases</summary>
		public bool BveTsHacks;
		/// <summary>Cylinders are generated with a different face order</summary>
		/// <remarks>Used as tunnel internals by some BVE2 content</remarks>
		public bool CylinderHack;
		/// <summary>Pure black is used as a transparent color</summary>
		/// <remarks>Used to provide color-key transparency in BVE4 X format files</remarks>
		public bool BlackTransparency;
		/// <summary>Semi-transparent faces are disabled</summary>
		/// <remarks>BVE2 did not support semi-transparent faces</remarks>
		public bool DisableSemiTransparentFaces;
		/// <summary>Whether aggressive RW bracket fixing is performed</summary>
		public bool AggressiveRwBrackets;
	}
}
