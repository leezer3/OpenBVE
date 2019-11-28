namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>Represents a fuel tank</summary>
		public class FuelTank
		{
			/// <summary>The minimum fuel level</summary>
			public readonly double MinimumLevel;
			/// <summary>The maximum fuel level</summary>
			public readonly double MaximumLevel;
			/// <summary>The current fuel level</summary>
			public double CurrentLevel;

			public FuelTank(double min, double max, double current)
			{
				MinimumLevel = min;
				MaximumLevel = max;
				CurrentLevel = current;
			}
		}
	}
}
