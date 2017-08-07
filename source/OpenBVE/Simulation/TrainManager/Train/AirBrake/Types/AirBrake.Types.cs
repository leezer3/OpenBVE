namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Determines the type of air brake fitted to a car</summary>
		internal enum BrakeType
		{
			/// <summary>A main air brake with compressor</summary>
			Main,
			/// <summary>An auxiliary air brake, with no compressor</summary>
			Auxillary
		}
	}
}
