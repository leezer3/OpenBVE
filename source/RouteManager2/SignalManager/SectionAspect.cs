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
		/// <param name="Number">The aspect number</param>
		/// <param name="Speed">The speed limit</param>
		public SectionAspect(int Number, double Speed)
		{
			this.Number = Number;
			this.Speed = Speed;
		}
	}
}
