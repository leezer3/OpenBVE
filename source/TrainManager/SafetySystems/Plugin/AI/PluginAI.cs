using OpenBveApi.Runtime;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents an abstract AI controller handling tasks for a specific plugin</summary>
	internal abstract class PluginAI
	{
		/// <summary>The plugin</summary>
		internal Plugin Plugin;
		/// <summary>Timer variable controlling the next plugin action</summary>
		internal double nextPluginAction;
		/// <summary>Called once a frame to perform any AI specific tasks</summary>
		/// <param name="data"></param>
		internal abstract void Perform(AIData data);
	}
}
