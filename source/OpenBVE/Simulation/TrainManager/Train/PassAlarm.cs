namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>Defines the differing types of station pass alarm a train may be fitted with</summary>
		internal enum PassAlarmType
		{
			/// <summary>No pass alarm</summary>
			None = 0,
			/// <summary>The alarm sounds once</summary>
			Single = 1,
			/// <summary>The alarm loops until cancelled</summary>
			Loop = 2
		}
	}
}
