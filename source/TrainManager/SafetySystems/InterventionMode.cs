namespace TrainManager.SafetySystems
{
	/// <summary>The different intervention modes for a safety system</summary>
	public enum InterventionMode
	{
		/// <summary>No intervention / not fitted</summary>
		None = 0,
		/// <summary>Cuts the power to zero</summary>
		CutsPower,
		/// <summary>Applies the service brakes</summary>
		ApplyBrake,
		/// <summary>Applies the emergency brake</summary>
		ApplyEmergencyBrake
	}
}
