#pragma warning disable 0659, 0661

using System;

namespace OpenBveApi.Archives {

	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	// --- interfaces ---

	/// <summary>Represents the interface for loading archives. Plugins must implement this interface if they wish to expose archives.</summary>
	public abstract class ArchiveInterface {
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public virtual void Load(Hosts.HostInterface host) { }
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload() { }
		
		/// <summary>Checks whether the plugin can load the specified element.</summary>
		/// <param name="path">The path to the file or folder that contains the archive.</param>
		/// <param name="element">The path to the element.</param>
		/// <returns>Whether the plugin can load the specified element.</returns>
		public abstract bool CanLoadElement(string path, string element);

		/// <summary>Loads an item from the specified archive.</summary>
		/// <param name="path">The path to the file or folder that contains the archive.</param>
		/// <param name="element">The path to the element.</param>
		/// <param name="data">Receives the element data.</param>
		/// <returns>Whether loading the element was successful.</returns>
		public abstract bool LoadElement(string path, string element, out byte[] data);
		
	}
	
}