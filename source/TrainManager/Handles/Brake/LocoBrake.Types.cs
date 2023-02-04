namespace TrainManager.Handles
{
	/// <summary>Defines the differing types of locomotive brake</summary>
	public enum LocoBrakeType
	{
		/// <summary>The locomotive brakes operate at the HIGHER of the train brake and locomotive brake settings</summary>
		Combined = 0,
		/// <summary>The locomotive brake operates independantly of the train brakes</summary>
		Independant = 1,
		/// <summary>The locomotive brake blocks the release of the train brakes</summary>
		Blocking = 2,
	}
}
