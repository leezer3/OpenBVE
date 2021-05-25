using System;

namespace TrainManager.BrakeSystems
{
	/// <summary>An auxiliary reservoir</summary>
	public class AuxiliaryReservoir
	{
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The current pressure</summary>
		public double CurrentPressure;
		/// <summary>The maximum pressure</summary>
		public readonly double MaximumPressure;
		/// <summary>The co-efficient used when transferring pressure to the brake pipe</summary>
		internal readonly double BrakePipeCoefficient;
		/// <summary>The co-efficient used when transferring pressure to the brake cylinder</summary>
		internal readonly double BrakeCylinderCoefficient;

		public AuxiliaryReservoir(double maximumPressure, double chargeRate, double brakePipeCoefficient, double brakeCylinderCoefficent)
		{
			ChargeRate = chargeRate;
			MaximumPressure = maximumPressure;
			CurrentPressure = maximumPressure;
			BrakePipeCoefficient = brakePipeCoefficient;
			BrakeCylinderCoefficient = brakeCylinderCoefficent;
		}
	}

	/// <summary>An equalising reservoir</summary>
	public class EqualizingReservoir
	{
		/// <summary>The rate when service brakes are applied in Pa/s</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate when EB brakes are applied in Pa/s</summary>
		internal readonly double EmergencyRate;
		/// <summary>The charge rate in Pa/s</summary>
		internal readonly double ChargeRate;
		/// <summary>The current pressure</summary>
		public double CurrentPressure;
		/// <summary>The normal pressure</summary>
		public double NormalPressure;

		public EqualizingReservoir(double serviceRate, double emergencyRate, double chargeRate)
		{
			ServiceRate = serviceRate;
			EmergencyRate = emergencyRate;
			ChargeRate = chargeRate;
			CurrentPressure = 0.0;
		}

		/// <summary>Creates a dummy equalizing reservoir</summary>
		public EqualizingReservoir(double pressure)
		{
			ServiceRate = 0.0;
			EmergencyRate = 0.0;
			ChargeRate = 0.0;
			CurrentPressure = pressure;
		}
	}

	/// <summary>A main reservoir</summary>
	public class MainReservoir
	{
		/// <summary>The current pressure</summary>
		public double CurrentPressure;
		/// <summary>The minimum pressure</summary>
		internal readonly double MinimumPressure;
		/// <summary>The maximum pressure</summary>
		public readonly double MaximumPressure;
		/// <summary>The co-efficient used when transferring pressure to the equalizing reservoir</summary>
		internal readonly double EqualizingReservoirCoefficient;
		/// <summary>The co-efficient used when transferring pressure to the brake pipe</summary>
		internal readonly double BrakePipeCoefficient;

		/// <summary>Creates a functional main reservoir</summary>
		public MainReservoir(double minimumPressure, double maximumPressure, double equalizingReservoirCoefficient, double brakePipeCoefficient)
		{
			MinimumPressure = minimumPressure;
			MaximumPressure = maximumPressure;
			EqualizingReservoirCoefficient = equalizingReservoirCoefficient;
			BrakePipeCoefficient = brakePipeCoefficient;
			CurrentPressure = MinimumPressure + (MaximumPressure - MinimumPressure) * new Random().NextDouble();
		}

		/// <summary>Creates a dummy main reservoir</summary>
		public MainReservoir(double pressure)
		{
			MinimumPressure = pressure;
			MaximumPressure = pressure;
			EqualizingReservoirCoefficient = 0.0;
			BrakePipeCoefficient = 0.0;
			CurrentPressure = pressure;
		}
	}
}
