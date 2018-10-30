namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The available modes for doors within a train</summary>
		internal enum DoorMode
		{
			/// <summary>The doors function automatically, but the player may override them</summary>
			AutomaticManualOverride = 0,
			/// <summary>The doors function wholely automatically</summary>
			Automatic = 1,
			/// <summary>The doors must be manually activated by the player</summary>
			Manual = 2
		}
	}
}
