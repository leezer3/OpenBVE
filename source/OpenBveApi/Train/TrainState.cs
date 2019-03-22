namespace OpenBveApi.Trains
{
	/// <summary>The available states for a train</summary>
	public enum TrainState
	{
		/// <summary>The train has not yet been introduced into the simulation</summary>
		Pending = 0,
		/// <summary>The train has been introduced into the simulation</summary>
		Available = 1,
		/// <summary>The train has traversed it's path, and has been disposed of by the simulation</summary>
		Disposed = 2,
		/// <summary>The train is a bogus (non-visble) train created via a .PreTrain command</summary>
		Bogus = 3
	}
}
