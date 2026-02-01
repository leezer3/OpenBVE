using OpenBveApi.Math;
using TrainManager.Car;
using TrainManager.Trains;

namespace TrainManager.Motor
{
	public class Bve5MotorCar : TractionModel
	{
		public Bve5PerformanceData PowerPerformanceData;

		public Bve5PerformanceData BrakePerformanceData;

		public Bve5MotorCar(CarBase car, Bve5PerformanceData powerPerformanceData,
			Bve5PerformanceData brakePerformanceData) : base(car)
		{
			PowerPerformanceData = powerPerformanceData;
			BrakePerformanceData = brakePerformanceData;
		}

		public override void Update(double timeElapsed)
		{
		}

		public override double TargetAcceleration
		{
			get
			{
				// first find the load proportion
				// this is used to interpolate between Load and MaxLoad
				double loadingRatio = BaseCar.Cargo.Ratio / 250;
				// BVE5 performance table returns result in newtons
				double newtons = 0;
				if (BaseCar.baseTrain.Handles.Brake.Actual > 0)
				{
					double noLoad = BrakePerformanceData.ForceTable.GetValue(BaseCar.CurrentSpeed,
						BaseCar.baseTrain.Handles.Brake.Actual);
					double maxLoad = BrakePerformanceData.MaxForceTable.GetValue(BaseCar.CurrentSpeed,
						BaseCar.baseTrain.Handles.Brake.Actual);
					newtons = Extensions.LinearInterpolation(0, noLoad, 1, maxLoad, loadingRatio);
				}
				else if (BaseCar.baseTrain.Handles.Power.Actual > 0)
				{
					double noLoad = PowerPerformanceData.ForceTable.GetValue(BaseCar.CurrentSpeed,
						BaseCar.baseTrain.Handles.Brake.Actual);
					double maxLoad = PowerPerformanceData.MaxForceTable.GetValue(BaseCar.CurrentSpeed,
						BaseCar.baseTrain.Handles.Brake.Actual);
					newtons = Extensions.LinearInterpolation(0, noLoad, 1, maxLoad, loadingRatio);
				}

				if (newtons == 0)
				{
					return 0;
				}

				/*
				 * According to Newton's second law, Acceleration = Force / Mass
				 * Load factor has already been taken into account above, so this *should* just be a simple
				 * calculation
				 */

				double totalMass = 0;

				TrainBase baseTrain = BaseCar.baseTrain;
				for (int i = 0; i < baseTrain.Cars.Length; i++)
				{
					totalMass += baseTrain.Cars[i].CurrentMass;
				}
				return totalMass / newtons;
			}
		}
	}
}
