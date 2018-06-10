namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Possible states of an air-brake handle</summary>
		internal enum AirBrakeHandleState
		{
			Invalid = -1,
			Release = 0,
			Lap = 1,
			Service = 2,
		}
	}
}
