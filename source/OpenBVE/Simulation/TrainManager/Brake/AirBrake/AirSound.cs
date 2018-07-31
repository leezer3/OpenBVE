namespace OpenBve.BrakeSystems
{
	enum AirSound
	{
		/// <summary>No sound</summary>
		None = -1,
		/// <summary>Played when the pressure in the brake cylinder is decreased to zero</summary>
		AirZero = 0,
		/// <summary>Played when the pressure in the brake cylinder is decreased from a non-high to a non-zero value</summary>
		Air = 1,
		/// <summary>Played when the pressure in the brake cylinder is decreased from a high value</summary>
		AirHigh = 2
	}
}
