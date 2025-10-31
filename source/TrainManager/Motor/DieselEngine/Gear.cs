namespace TrainManager.Motor
{
	public struct Gear
	{
		/// <summary>The maximum speed attainable in this gear</summary>
		public double MaximumSpeed;
		/// <summary>The overspeed failure speed</summary>
		public double OverspeedFailure;
		/// <summary>The maximum tractive force attainable in this gear</summary>
		public double MaxTractiveForce;
	}
}
