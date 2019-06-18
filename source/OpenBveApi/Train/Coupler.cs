namespace OpenBveApi.Trains
{
	/// <summary>A coupling between two cars</summary>
	public struct Coupler
	{
		/*
		 * NOTE:
		 * Prior versions of openBVE did not support discrete bogies.
		 * A 'hack' was used in some cases to allow a coupler to pull
		 * a car (used as a bogie) into the correct position.
		 *
		 * This means that we cannot validate a coupler's distance as
		 * having to be positive.
		 */

		/// <summary>The minimum distance that the cars may be apart</summary>
		public double MinimumDistanceBetweenCars;
		/// <summary>The maximum distance that the cars may be apart</summary>
		public double MaximumDistanceBetweenCars;
	}
}
