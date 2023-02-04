using System;

namespace OpenBveApi.Trains
{
	/// <summary>The different traction types</summary>
	[Flags]
	public enum TractionType
	{
		/// <summary>An abstract diesel engine</summary>
		Diesel = 0,
		/// <summary>A diesel with electric transmission</summary>
		DieselElectric = 1,
		/// <summary>A diesel with hydralic transmission</summary>
		DieselHydralic = 2,
		/// <summary>A diesel with mechanical transmission</summary>
		DieselMechanical = 4,
		/// <summary>An electric</summary>
		Electric = 8,
		/// <summary>A steam locomotive</summary>
		Steam = 16
	}
}
