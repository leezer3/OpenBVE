namespace TrainManager.SafetySystems
{
	/// <summary>The triggering modes for a SafetySystem</summary>
	public enum SafetySystemTriggerMode
	{
		/// <summary>The SafetySystem is always active</summary>
		Always,
		/// <summary>The SafetySystem is only active when the train is moving</summary>
		TrainMoving,
		/// <summary>The SafetySystem is only active when a direction is set on the reverser</summary>
		DirectionSet,
	}
}
