using System;

namespace OpenBveApi.Routes
{
	/// <summary>The different types of power supply available</summary>
	/// <remarks>Stored as a bitmask on the track element</remarks>
	[Flags]
	public enum PowerSupplyTypes
	{
		/// <summary>No additional power supply is provided</summary>
		None = 0,
		/// <summary>Power supply is provided via the overhead line</summary>
		OverheadLine = 1,
		/// <summary>Power supply is provided by the third rail</summary>
		ThirdRail = 2,
		/// <summary>Power supply is provided by the fourth rail</summary>
		FourthRail = 4
	}

	
}
