namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represents a hold-brake handle</summary>
		internal struct HoldBrakeHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal bool Driver;
			/// <summary>The actual notch</summary>
			internal bool Actual;
		}
	}
}
