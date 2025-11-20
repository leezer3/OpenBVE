namespace TrainManager.Motor
{
	public enum GearboxOperation
	{
		/// <summary>The gearbox operation mode is unknown</summary>
		Unknown = 0,
		/// <summary>The gearbox is operated manually using the gear up / gear down keys</summary>
		Manual = 1,
		/// <summary>The gearbox is automatic, but can be operated using the keys</summary>
		SemiAutomatic = 2,
		/// <summary>The gearbox is fully automatic</summary>
		Automatic = 3
	}
}
