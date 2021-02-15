namespace TrainManager.SafetySystems
{
	/// <summary>The different available types of DriverSupervisionDevice</summary>
	public enum DriverSupervisionDeviceTypes
	{
		/// <summary>No driver supervision device is fitted</summary>
		None,
		/// <summary>The driver supervision device cuts the power</summary>
		CutsPower,
		/// <summary>The driver supervision device applies the service brakes</summary>
		ApplyBrake,
		/// <summary>The driver supervision device applys the service brakes</summary>
		ApplyEmergencyBrake
	}
}
