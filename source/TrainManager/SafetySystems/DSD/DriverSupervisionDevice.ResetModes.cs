namespace TrainManager.SafetySystems
{
	/// <summary>The modes for the DSD</summary>
	public enum DriverSupervisionDeviceMode
	{
		/// <summary>The DSD is reset by the power handle</summary>
		Power,
		/// <summary>The DSD is reset by the brake handle</summary>
		Brake,
		/// <summary>The DSD is reset by any handle</summary>
		AnyHandle,
		/// <summary>The DSD is independant</summary>
		Independant,
		/// <summary>The DSD requires the DSD key to be held</summary>
		HeldKey

	}
}
