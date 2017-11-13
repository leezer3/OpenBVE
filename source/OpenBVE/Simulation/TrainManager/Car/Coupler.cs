namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		// coupler
		internal struct Coupler
		{
			internal double MinimumDistanceBetweenCars;
			internal double MaximumDistanceBetweenCars;
		}
	}
}
