namespace OpenBve.BrakeSystems
{
	/// <summary>An auxiliary reservoir</summary>
	class AuxiliaryReservoir
	{
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The current pressure</summary>
		internal double CurrentPressure;
		/// <summary>The maximum pressure</summary>
		internal readonly double MaximumPressure;
		/// <summary>The co-efficient used when transferring pressure to the brake pipe</summary>
		internal readonly double BrakePipeCoefficient;
		/// <summary>The co-efficient used when transferring pressure to the brake cylinder</summary>
		internal readonly double BrakeCylinderCoefficient;

		internal AuxiliaryReservoir(double maximumPressure, double chargeRate, double brakePipeCoefficient, double brakeCylinderCoefficent)
		{
			ChargeRate = chargeRate;
			MaximumPressure = maximumPressure;
			CurrentPressure = maximumPressure;
			BrakePipeCoefficient = brakePipeCoefficient;
			BrakeCylinderCoefficient = brakeCylinderCoefficent;
		}
	}

	/// <summary>An equalising reservoir</summary>
	class EqualizingReservoir
	{
		/// <summary>The rate when service brakes are applied in Pa/s</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate when EB brakes are applied in Pa/s</summary>
		internal readonly double EmergencyRate;
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The current pressure</summary>
		internal double CurrentPressure;
		/// <summary>The normal pressure</summary>
		internal double NormalPressure;

		internal EqualizingReservoir(double serviceRate, double emergencyRate, double chargeRate)
		{
			ServiceRate = serviceRate;
			EmergencyRate = emergencyRate;
			ChargeRate = chargeRate;
			CurrentPressure = 0.0;
		}
	}

	/// <summary>A main reservoir</summary>
	class MainReservoir
	{
		/// <summary>The current pressure</summary>
		internal double CurrentPressure;
		/// <summary>The minimum pressure</summary>
		internal readonly double MinimumPressure;
		/// <summary>The maximum pressure</summary>
		internal readonly double MaximumPressure;
		/// <summary>The co-efficient used when transferring pressure to the equalizing reservoir</summary>
		internal readonly double EqualizingReservoirCoefficient;
		/// <summary>The co-efficient used when transferring pressure to the brake pipe</summary>
		internal readonly double BrakePipeCoefficient;

		internal MainReservoir(double minimumPressure, double maximumPressure, double equalizingReservoirCoefficient, double brakePipeCoefficient)
		{
			MinimumPressure = minimumPressure;
			MaximumPressure = maximumPressure;
			EqualizingReservoirCoefficient = equalizingReservoirCoefficient;
			BrakePipeCoefficient = brakePipeCoefficient;
			CurrentPressure = MinimumPressure + (MaximumPressure - MinimumPressure) * Program.RandomNumberGenerator.NextDouble();
		}
	}
}
