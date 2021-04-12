namespace TrainManager.Car
{
	/// <summary>Represents a raindrop on a windscreen</summary>
	public struct Raindrop
	{
		/// <summary>Whether the raindrop is currently visible</summary>
		public bool Visible;
		/// <summary>The remaining life before the drop dries</summary>
		public double RemainingLife;
		/// <summary>Whether this drop should show the alternate snowflake texture</summary>
		public bool IsSnowFlake;
	}
}
