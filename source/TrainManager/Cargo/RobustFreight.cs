using System;
using TrainManager.Car;

namespace TrainManager.Cargo
{
	/// <summary>Represents a robust freight cargo, e.g. steel coils</summary>
	/// <remarks>This cargo type changes weight with station loadings, but does not damage</remarks>
	public class RobustFreight : CargoBase
	{
		private readonly CarBase baseCar;

		private double freightMass;

		public override double Mass => freightMass;

		public RobustFreight(CarBase car)
		{
			baseCar = car;
		}
		
		public override void UpdateLoading(double ratio)
		{
			// NOTE: This is the same calculation as the passenger mass at present for backwards compatibility
			Ratio = ratio;
			double area = baseCar.Width * baseCar.Length;
			const double freightPerArea = 1.0; //Nominal 1 freight unit per meter of interior space
			double randomFactor = 0.9 + 0.2 * TrainManagerBase.RandomNumberGenerator.NextDouble();
			double freight = Math.Round(randomFactor * Ratio * freightPerArea * area);
			const double massPerFreight = 70.0; //70kg mass per freight unit
			freightMass = freight * massPerFreight;
		}
	}
}
