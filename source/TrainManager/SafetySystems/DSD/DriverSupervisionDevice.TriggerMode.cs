namespace TrainManager.SafetySystems
{
	/// <summary>The triggering modes for the DSD</summary>
	public enum DriverSupervisionDeviceTriggerMode
	{
		/// <summary>The DSD is always active</summary>
		Always,
		/// <summary>The DSD is only active when the train is moving</summary>
		TrainMoving,
		/// <summary>The DSD is only active when a direction is set on the reverser</summary>
		DirectionSet,
	}
}
