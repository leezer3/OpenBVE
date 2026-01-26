namespace TrainManager.SafetySystems
{
	/// <summary>The different available types of SafetySystem</summary>
	public enum SafetySystemType
	{
		/// <summary>No SafetySystem device is fitted</summary>
		None,
		/// <summary>The SafetySystem cuts the power</summary>
		CutsPower,
		/// <summary>The SafetySystem applies the service brakes</summary>
		ApplyBrake,
		/// <summary>The SafetySystem applies the service brakes</summary>
		ApplyEmergencyBrake
	}
}
