using System;

namespace OpenBveApi.Routes
{
	/// <summary>Represents the interface for loading routes. Plugins must implement this interface if they wish to expose routes.</summary>
	public abstract class RouteInterface {
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		/// <param name="fileSystem">The filesystem from the host application</param>
		/// <param name="Options">The options supplied by the host program</param>
		/// <param name="trainManagerReference">A reference to the TrainManager in the host application</param>
		public virtual void Load(Hosts.HostInterface host, FileSystem.FileSystem fileSystem, BaseOptions Options, object trainManagerReference) { }
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload() { }
		
		/// <summary>Checks whether the plugin can load the specified route.</summary>
		/// <param name="path">The path to the file or folder that contains the route.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		public abstract bool CanLoadRoute(string path);

		/// <summary>Loads the specified route.</summary>
		/// <param name="path">The path to the file or folder that contains the route.</param>
		/// <param name="textEncoding">The user-selected encoding (if appropriate)</param>
		/// <param name="trainPath">The path to the selected train</param>
		/// <param name="objectPath">The base object folder path</param>
		/// <param name="soundPath">The base sound folder path</param>
		/// <param name="PreviewOnly">Whether this is a preview</param>
		/// <param name="route">Receives the route.</param>
		/// <returns>Whether loading the route was successful.</returns>
		public abstract bool LoadRoute(string path, System.Text.Encoding textEncoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, ref object route);

		/// <summary>Holds whether loading is currently in progress</summary>
		public bool IsLoading;

		/// <summary>The last exception encountered by the plugin</summary>
		public Exception LastException;
		
		/// <summary>Set if loading is to be cancelled</summary>
		public bool Cancel;

		/// <summary>Holds the current loading progress</summary>
		public double CurrentProgress;
	}
}
