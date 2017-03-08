namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents the auxuilliary reservoir of an air-brake system</summary>
		internal class AuxillaryReservoir
		{
			/// <summary>The current pressure in Pa</summary>
			internal double CurrentPressure;
			/// <summary>The maxiumum pressure in Pa</summary>
			internal double MaximumPressure;
			/// <summary>The rate at which the brake pipe fills from this reservoir in Pa per second</summary>
			internal double ChargeRate;
			/// <summary>The co-efficient used for pressure transfer to the brake pipe</summary>
			internal double BrakePipeCoefficient;
			/// <summary>The co-efficient used for pressure transfer to the brake cylinder</summary>
			internal double BrakeCylinderCoefficient;
		}
	}
}
