using System;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Trains;

namespace TrainManager.TractionModels.BVE
{
	/// <summary>The traction model for a BVE Motor Car</summary>
	public class BVEMotorCar : AbstractTractionModel
	{
		public BVEMotorCar(CarBase car) : base(car)
		{
		}

		public override void Update(double TimeElapsed, out double Speed)
		{
			double adjustedFrictionBrakeAcceleration = FrictionBrakeAcceleration;
			double adjustedPowerRollingCouplerAcceleration = PowerRollingCouplerAcceleration;
			// power
			double wheelspin = 0.0;
			double wheelSlipAccelerationMotorFront = 0.0;
			double wheelSlipAccelerationMotorRear = 0.0;
			double wheelSlipAccelerationBrakeFront = 0.0;
			double wheelSlipAccelerationBrakeRear = 0.0;
			if (!Car.Derailed)
			{
				wheelSlipAccelerationMotorFront = Car.FrontAxle.CriticalWheelSlipAccelerationForElectricMotor(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationMotorRear = Car.RearAxle.CriticalWheelSlipAccelerationForElectricMotor(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationBrakeFront = Car.FrontAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationBrakeRear = Car.RearAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
			}

			if (DecelerationDueToTraction == 0.0)
			{
				double a;
				if (Car.baseTrain.Handles.Reverser.Actual != 0 & Car.baseTrain.Handles.Power.Actual > 0 & !Car.baseTrain.Handles.HoldBrake.Actual & !Car.baseTrain.Handles.EmergencyBrake.Actual)
				{
					// target acceleration
					if (Car.baseTrain.Handles.Power.Actual - 1 < AccelerationCurves.Length)
					{
						// Load factor for a BVE2 / BVE4 / non-XML OpenBVE car is a constant 1.0
						a = AccelerationCurves[Car.baseTrain.Handles.Power.Actual - 1].GetAccelerationOutput((double)Car.baseTrain.Handles.Reverser.Actual * Car.CurrentSpeed, 1.0);
					}
					else
					{
						a = 0.0;
					}

					// readhesion device
					if (a > Car.ReAdhesionDevice.MaximumAccelerationOutput)
					{
						a = Car.ReAdhesionDevice.MaximumAccelerationOutput;
					}

					// wheel slip
					if (a < wheelSlipAccelerationMotorFront)
					{
						Car.FrontAxle.CurrentWheelSlip = false;
					}
					else
					{
						Car.FrontAxle.CurrentWheelSlip = true;
						wheelspin += (double)Car.baseTrain.Handles.Reverser.Actual * a * Car.CurrentMass;
					}

					if (a < wheelSlipAccelerationMotorRear)
					{
						Car.RearAxle.CurrentWheelSlip = false;
					}
					else
					{
						Car.RearAxle.CurrentWheelSlip = true;
						wheelspin += (double)Car.baseTrain.Handles.Reverser.Actual * a * Car.CurrentMass;
					}

					// Update readhesion device
					Car.ReAdhesionDevice.Update(a);
					// Update constant speed device

					Car.ConstSpeed.Update(ref a, Car.baseTrain.Specs.CurrentConstSpeed, (ReverserPosition)Car.baseTrain.Handles.Reverser.Actual);

					// finalize
					if (wheelspin != 0.0) a = 0.0;
				}
				else
				{
					a = 0.0;
					Car.FrontAxle.CurrentWheelSlip = false;
					Car.RearAxle.CurrentWheelSlip = false;
				}


				if (!Car.Derailed)
				{
					if (MotorAcceleration < a)
					{
						if (MotorAcceleration < 0.0)
						{
							MotorAcceleration += Car.CarBrake.JerkDown * TimeElapsed;
						}
						else
						{
							MotorAcceleration += Car.Specs.JerkPowerUp * TimeElapsed;
						}

						if (MotorAcceleration > a)
						{
							MotorAcceleration = a;
						}
					}
					else
					{
						MotorAcceleration -= Car.Specs.JerkPowerDown * TimeElapsed;
						if (MotorAcceleration < a)
						{
							MotorAcceleration = a;
						}
					}
				}
				else
				{
					MotorAcceleration = 0.0;
				}
			}

			// brake
			bool wheellock = wheelspin == 0.0 & Car.Derailed;
			if (!Car.Derailed & wheelspin == 0.0)
			{
				double a;
				// motor
				if (DecelerationDueToTraction != 0.0)
				{
					a = -DecelerationDueToTraction;
					if (MotorAcceleration > a)
					{
						if (MotorAcceleration > 0.0)
						{
							MotorAcceleration -= Car.Specs.JerkPowerDown * TimeElapsed;
						}
						else
						{
							MotorAcceleration -= Car.CarBrake.JerkUp * TimeElapsed;
						}

						if (MotorAcceleration < a)
						{
							MotorAcceleration = a;
						}
					}
					else
					{
						MotorAcceleration += Car.CarBrake.JerkDown * TimeElapsed;
						if (MotorAcceleration > a)
						{
							MotorAcceleration = a;
						}
					}
				}

				// brake
				a = DecelerationDueToBrake;
				if (Car.CurrentSpeed >= -0.01 & Car.CurrentSpeed <= 0.01)
				{
					double rf = Car.FrontAxle.Follower.WorldDirection.Y;
					double rr = Car.RearAxle.Follower.WorldDirection.Y;
					double ra = Math.Abs(0.5 * (rf + rr) *
					                     TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
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

			// motor
			if (Car.baseTrain.Handles.Reverser.Actual != 0)
			{
				double factor = Car.EmptyMass / Car.CurrentMass;
				if (MotorAcceleration > 0.0)
				{
					adjustedPowerRollingCouplerAcceleration += (double) Car.baseTrain.Handles.Reverser.Actual * MotorAcceleration * factor;
				}
				else
				{
					double a = -MotorAcceleration;
					if (a >= wheelSlipAccelerationMotorFront)
					{
						Car.FrontAxle.CurrentWheelSlip = true;
					}
					else if (!Car.Derailed)
					{
						adjustedFrictionBrakeAcceleration += 0.5 * a * factor;
					}

					if (a >= wheelSlipAccelerationMotorRear)
					{
						Car.RearAxle.CurrentWheelSlip = true;
					}
					else
					{
						adjustedFrictionBrakeAcceleration += 0.5 * a * factor;
					}
				}
			}
			else
			{
				MotorAcceleration = 0.0;
			}

			
			// perceived speed
			{
				double target;
				if (wheellock)
				{
					target = 0.0;
				}
				else if (wheelspin == 0.0)
				{
					target = Car.CurrentSpeed;
				}
				else
				{
					target = Car.CurrentSpeed + wheelspin / 2500.0;
				}

				double diff = target - Car.Specs.PerceivedSpeed;
				double rate = (diff < 0.0 ? 5.0 : 1.0) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity *
				              TimeElapsed;
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
				double a = adjustedPowerRollingCouplerAcceleration;
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
			if (Sounds != null)
			{
				Sounds.Update(TimeElapsed);
			}
		}
		

	}
}
