namespace OpenBve.BrakeSystems
{
	/// <summary>Defines the different types of air sound</summary>
	internal enum AirSound
	{
		/// <summary>No air sound is to be played</summary>
		None = -1,
		/// <summary>Played when the pressure in the brake cylinder is decreased to zero</summary>
		Zero = 0,
		/// <summary>Played when the pressure in the brake cylinder is decreased to a non-zero value</summary>
		Normal = 1,
		/// <summary>Played when the pressure in the brake cylinder is decreased from a high value to a normal value</summary>
		High = 2
	}
}
