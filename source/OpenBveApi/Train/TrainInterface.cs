using System;
using OpenBveApi.Interface;

namespace OpenBveApi.Trains
{
	/// <summary>Represents the interface for loading trains. Plugins must implement this interface if they wish to expose trains.</summary>
	public abstract class TrainInterface {
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		/// <param name="fileSystem">The filesystem from the host application</param>
		/// <param name="Options">The options supplied by the host program</param>
		/// <param name="trainManagerReference">A reference to the TrainManager in the host application</param>
		public virtual void Load(Hosts.HostInterface host, FileSystem.FileSystem fileSystem, BaseOptions Options, object trainManagerReference) { }
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload() { }
		
		/// <summary>Checks whether the plugin can load the specified train.</summary>
		/// <param name="path">The path to the file or folder that contains the train.</param>
		/// <returns>Whether the plugin can load the specified train.</returns>
		public abstract bool CanLoadTrain(string path);

		/// <summary>Loads the specified train.</summary>
		/// <param name="path">The path to the file or folder that contains the train.</param>
		/// <param name="Encoding">The user-selected encoding (if appropriate)</param>
		/// <param name="trainPath">The path to the selected train</param>
		/// <param name="train">Receives the train.</param>
		/// <param name="currentControls">The current control array (modified if touch elements are present)</param>
		/// <returns>Whether loading the route was successful.</returns>
		public abstract bool LoadTrain(System.Text.Encoding Encoding, string trainPath, ref AbstractTrain train, ref Control[] currentControls);

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
