namespace RouteManager2.SignalManager
{
	/// <summary>A signalling aspect attached to a track section</summary>
	public struct SectionAspect
	{
		/// <summary>The aspect number</summary>
		public int Number;
		/// <summary>The speed limit associated with this aspect number</summary>
		public double Speed;

		/// <summary>Creates a new signalling aspect</summary>
		/// <param name="aspectNumber">The aspect number</param>
		/// <param name="speedLimit">The speed limit</param>
		public SectionAspect(int aspectNumber, double speedLimit)
		{
			this.Number = aspectNumber;
			this.Speed = speedLimit;
		}
	}
}
