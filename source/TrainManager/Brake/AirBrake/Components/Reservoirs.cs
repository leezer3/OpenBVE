using System;

namespace TrainManager.BrakeSystems
{
	/// <summary>An auxiliary reservoir</summary>
	public class AuxiliaryReservoir : AbstractReservoir
	{
		/// <summary>The co-efficient used when transferring pressure to the brake pipe</summary>
		internal readonly double BrakePipeCoefficient;
		/// <summary>The co-efficient used when transferring pressure to the brake cylinder</summary>
		internal readonly double BrakeCylinderCoefficient;

		public AuxiliaryReservoir(double maximumPressure, double chargeRate, double brakePipeCoefficient, double brakeCylinderCoefficent) : base(chargeRate, maximumPressure, maximumPressure)
		{
			CurrentPressure = maximumPressure;
			BrakePipeCoefficient = brakePipeCoefficient;
			BrakeCylinderCoefficient = brakeCylinderCoefficent;
		}
	}

	/// <summary>An equalising reservoir</summary>
	public class EqualizingReservoir : AbstractReservoir
	{
		/// <summary>The rate when service brakes are applied in Pa/s</summary>
		internal readonly double ServiceRate;
		/// <summary>The rate when EB brakes are applied in Pa/s</summary>
		internal readonly double EmergencyRate;
		/// <summary>The normal pressure</summary>
		public double NormalPressure;
		/// <summary>The internal volume of the reservoir</summary>

		public EqualizingReservoir(double serviceRate, double emergencyRate, double chargeRate) : base(chargeRate, 0.0)
		{
			ServiceRate = serviceRate;
			EmergencyRate = emergencyRate;
		}

		/// <summary>Creates a dummy equalizing reservoir</summary>
		public EqualizingReservoir(double pressure) : base(0.0, pressure)
		{
			ServiceRate = 0.0;
			EmergencyRate = 0.0;
		}
	}

	/// <summary>A main reservoir</summary>
	public class MainReservoir : AbstractReservoir
	{
		/// <summary>The co-efficient used when transferring pressure to the equalizing reservoir</summary>
		internal readonly double EqualizingReservoirCoefficient;
		/// <summary>The co-efficient used when transferring pressure to the brake pipe</summary>
		internal readonly double BrakePipeCoefficient;

		/// <summary>Creates a functional main reservoir</summary>
		public MainReservoir(double minimumPressure, double maximumPressure, double equalizingReservoirCoefficient, double brakePipeCoefficient) : base(0, minimumPressure, maximumPressure)
		{
			EqualizingReservoirCoefficient = equalizingReservoirCoefficient;
			BrakePipeCoefficient = brakePipeCoefficient;
			CurrentPressure = MinimumPressure + (MaximumPressure - MinimumPressure) * new Random().NextDouble();
		}

		/// <summary>Creates a dummy main reservoir</summary>
		public MainReservoir(double pressure) : base(0, pressure)
		{
			EqualizingReservoirCoefficient = 0.0;
			BrakePipeCoefficient = 0.0;
		}
	}
}
