namespace OpenBve.Formats.MsTs
{
	/// <summary>Describes the different types of coupling that may be fitted</summary>
	public enum CouplingType
	{
		/// <summary>Unknown or not fitted</summary>
		Unknown = 0,
		/// <summary>Automatic electric couplers, e.g. Dellner</summary>
		Automatic = 1,
		/// <summary>Fixed bar coupler</summary>
		Bar = 2,
		/// <summary>Three-link hook / chain couplings</summary>
		Chain = 3
	}
}
