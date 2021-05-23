namespace ObjectViewer.Trains
{
	/// <summary>
	/// A structure that represents an element of an array of panel variables for a safety plugin
	/// </summary>
	internal struct PluginState
	{
		internal int Index;
		internal int Value;

		internal PluginState(int index, int value)
		{
			Index = index;
			Value = value;
		}
	}
}
