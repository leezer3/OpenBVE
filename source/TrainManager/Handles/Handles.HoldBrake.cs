namespace TrainManager.Handles
{
	/// <summary>Represents a hold-brake handle</summary>
	public struct HoldBrakeHandle
	{
		/// <summary>The notch set by the driver</summary>
		public bool Driver;
		/// <summary>The actual notch</summary>
		public bool Actual;
	}
}
