using OpenBveApi.Runtime;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents an abstract AI controller handling tasks for a specific plugin</summary>
	internal abstract class PluginAI
	{
		/// <summary>The plugin</summary>
		internal Plugin Plugin;
		/// <summary>Control variable used to determine next AI step</summary>
		internal int currentStep;
		/// <summary>Timer variable controlling the next plugin action</summary>
		internal double nextPluginAction;
		/// <summary>Stores the current rain intensity</summary>
		internal int currentRainIntensity;
		/// <summary>Whether the plugin has set the lights</summary>
		internal bool lightsSet;
		/// <summary>Called once a frame to perform any AI specific tasks</summary>
		/// <param name="data">The AI Data</param>
		internal abstract void Perform(AIData data);

		/// <summary>Called before the train jumps to a different location.</summary>
		/// <param name="mode">The initialization mode of the train.</param>
		public abstract void BeginJump(InitializationModes mode);

		/// <summary>Called when the train has finished jumping to a different location.</summary>
		public abstract void EndJump();

		/// <summary>Called when the train passes a beacon.</summary>
		/// <param name="beacon">The beacon data.</param>
		public abstract void SetBeacon(BeaconData beacon);
	}
}
