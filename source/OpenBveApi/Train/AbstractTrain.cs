namespace OpenBveApi
{
	/// <summary>An abstract train</summary>
	public abstract class Train
	{
		/// <summary>The current state of this train</summary>
		public TrainState State;
		/// <summary>The cars making up this train</summary>
		public Car[] Cars;
		/// <summary>The current destination (Accessible via animated objects etc.)</summary>
		public int Destination;
		/// <summary>The index of the current signalling section for the train</summary>
		public int CurrentSectionIndex;
		/// <summary>The speed limit for the current signalling section in km/h</summary>
		public double CurrentSectionLimit;
	}
}
