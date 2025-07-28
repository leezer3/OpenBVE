using TrainManager.Car;
using TrainManager.Power;

namespace TrainManager.Motor
{
	public class BVEMotorCar : TractionModel
	{
		public BVEMotorCar(CarBase car, AccelerationCurve[] accelerationCurves) : base(car, accelerationCurves, true)
		{
			IsRunning = true;
			Message = @"n/a";
		}

		public override void Update(double timeElapsed)
		{
			MotorSounds?.Update(timeElapsed);
		}

		public override double CurrentPower => CurrentAcceleration / MaximumPossibleAcceleration;

		public override double TargetAcceleration
		{
			get
			{
				if (AccelerationCurves == null)
				{
					return 0.0;
				}

				// NOTE: LoadFactor is constant 1.0 for BVE2 / BVE4
				if (BaseCar.baseTrain.Handles.Power.Actual - 1 < AccelerationCurves.Length)
				{
					return AccelerationCurves[BaseCar.baseTrain.Handles.Power.Actual - 1].GetAccelerationOutput((double)BaseCar.baseTrain.Handles.Reverser.Actual * BaseCar.CurrentSpeed);
				}

				// acceleration curve per power notch
				if (BaseCar.baseTrain.Handles.Power.Actual - 1 < AccelerationCurves.Length)
				{
					return AccelerationCurves[BaseCar.baseTrain.Handles.Power.Actual - 1].GetAccelerationOutput((double)BaseCar.baseTrain.Handles.Reverser.Actual * BaseCar.CurrentSpeed);
				}

				return 0.0;
			}
		}
	}
}
