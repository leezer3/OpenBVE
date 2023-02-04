using System;
using TrainManager.Car;
using TrainManager.Trains;

namespace TrainManager.TractionModels.BVE
{
	/// <summary>The traction model for a BVE Trailer Car</summary>
	public class TrailerCar : AbstractTractionModel
	{
		public TrailerCar(CarBase car) : base(car)
		{
		}

		public override void Update(double TimeElapsed, out double Speed)
		{
			// friction
			double adjustedFrictionBrakeAcceleration = FrictionBrakeAcceleration;

			double wheelSlipAccelerationBrakeFront = 0.0;
			double wheelSlipAccelerationBrakeRear = 0.0;
			if (!Car.Derailed)
			{
				wheelSlipAccelerationBrakeFront = Car.FrontAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationBrakeRear = Car.RearAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
			}

			Car.FrontAxle.CurrentWheelSlip = false;
			Car.RearAxle.CurrentWheelSlip = false;

			// brake
			bool wheellock = false;
			if (!Car.Derailed)
			{
				// brake
				double a = Car.TractionModel.DecelerationDueToBrake;
				if (Car.CurrentSpeed >= -0.01 & Car.CurrentSpeed <= 0.01)
				{
					double rf = Car.FrontAxle.Follower.WorldDirection.Y;
					double rr = Car.RearAxle.Follower.WorldDirection.Y;
					double ra = Math.Abs(0.5 * (rf + rr) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
					if (a > ra) a = ra;
				}

				double factor = Car.EmptyMass / Car.CurrentMass;
				if (a >= wheelSlipAccelerationBrakeFront)
				{
					wheellock = true;
				}
				else
				{
					adjustedFrictionBrakeAcceleration += 0.5 * a * factor;
				}

				if (a >= wheelSlipAccelerationBrakeRear)
				{
					wheellock = true;
				}
				else
				{
					adjustedFrictionBrakeAcceleration += 0.5 * a * factor;
				}
			}
			else if (Car.Derailed)
			{
				adjustedFrictionBrakeAcceleration += TrainBase.CoefficientOfGroundFriction * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}

			MotorAcceleration = 0.0;

			
			// perceived speed
			{
				double target = Car.CurrentSpeed;
				if (wheellock)
				{
					target = 0.0;
				}

				double diff = target - Car.Specs.PerceivedSpeed;
				double rate = (diff < 0.0 ? 5.0 : 1.0) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TimeElapsed;
				rate *= 1.0 - 0.7 / (diff * diff + 1.0);
				double factor = rate * rate;
				factor = 1.0 - factor / (factor + 1000.0);
				rate *= factor;
				if (diff >= -rate & diff <= rate)
				{
					Car.Specs.PerceivedSpeed = target;
				}
				else
				{
					Car.Specs.PerceivedSpeed += rate * Math.Sign(diff);
				}
			}
			// calculate new speed
			{
				int d = Math.Sign(Car.CurrentSpeed);
				double a = PowerRollingCouplerAcceleration;
				double b = adjustedFrictionBrakeAcceleration;
				if (Math.Abs(a) < b)
				{
					if (Math.Sign(a) == d)
					{
						if (d == 0)
						{
							Speed = 0.0;
						}
						else
						{
							double c = (b - Math.Abs(a)) * TimeElapsed;
							if (Math.Abs(Car.CurrentSpeed) > c)
							{
								Speed = Car.CurrentSpeed - d * c;
							}
							else
							{
								Speed = 0.0;
							}
						}
					}
					else
					{
						double c = (Math.Abs(a) + b) * TimeElapsed;
						if (Math.Abs(Car.CurrentSpeed) > c)
						{
							Speed = Car.CurrentSpeed - d * c;
						}
						else
						{
							Speed = 0.0;
						}
					}
				}
				else
				{
					Speed = Car.CurrentSpeed + (a - b * d) * TimeElapsed;
				}
			}
		}
	}
}
