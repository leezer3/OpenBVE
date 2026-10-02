using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Camera;
using LibRender2.Cameras;
using LibRender2.Smoke;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.World;
using TrainManager.Brake;
using TrainManager.BrakeSystems;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace TrainManager.Car
{
	public partial class CarBase : AbstractCar
	{
		public void UpdateTopplingCantAndSpring(double TimeElapsed)
		{
			// get direction, up and side vectors
			Vector3 d = FrontAxle.Follower.WorldPosition == RearAxle.Follower.WorldPosition ? FrontAxle.Follower.WorldPosition : new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
			double t = d.Magnitude();
			d *= t;
			t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
			double ex = d.X * t;
			double ez = d.Z * t;
			Vector3 s = new Vector3(ez, 0.0, -ex);
			Up = Vector3.Cross(d, s);
			double r = 0.0, rs = 0.0;
			if (FrontAxle.Follower.CurveRadius != 0.0 & RearAxle.Follower.CurveRadius != 0.0)
			{
				r = Math.Sqrt(Math.Abs(FrontAxle.Follower.CurveRadius * RearAxle.Follower.CurveRadius));
				rs = Math.Sign(FrontAxle.Follower.CurveRadius + RearAxle.Follower.CurveRadius);
			}
			else if (FrontAxle.Follower.CurveRadius != 0.0)
			{
				r = Math.Abs(FrontAxle.Follower.CurveRadius);
				rs = Math.Sign(FrontAxle.Follower.CurveRadius);
			}
			else if (RearAxle.Follower.CurveRadius != 0.0)
			{
				r = Math.Abs(RearAxle.Follower.CurveRadius);
				rs = Math.Sign(RearAxle.Follower.CurveRadius);
			}

			// roll due to shaking
			double a0 = Specs.RollDueToShakingAngle;
			double a1 = 0.0;
			if (Specs.RollShakeDirection != 0.0)
			{
				const double c0 = 0.03;
				const double c1 = 0.15;
				a1 = c1 * Math.Atan(c0 * Specs.RollShakeDirection);
				double dr = 0.5 + Specs.RollShakeDirection * Specs.RollShakeDirection;
				if (Specs.RollShakeDirection < 0.0)
				{
					Specs.RollShakeDirection += dr * TimeElapsed;
					if (Specs.RollShakeDirection > 0.0) Specs.RollShakeDirection = 0.0;
				}
				else
				{
					Specs.RollShakeDirection -= dr * TimeElapsed;
					if (Specs.RollShakeDirection < 0.0) Specs.RollShakeDirection = 0.0;
				}
			}

			double springAcceleration = Derailed ? 15.0 : 1.5 * Math.Abs(a1 - a0);
			double springDeceleration = 0.25 * springAcceleration;

			Specs.RollDueToShakingAngularSpeed += Math.Sign(a1 - a0) * springAcceleration * TimeElapsed;
			double x = Math.Sign(Specs.RollDueToShakingAngularSpeed) * springDeceleration * TimeElapsed;
			if (Math.Abs(x) < Math.Abs(Specs.RollDueToShakingAngularSpeed))
			{
				Specs.RollDueToShakingAngularSpeed -= x;
			}
			else
			{
				Specs.RollDueToShakingAngularSpeed = 0.0;
			}

			a0 += Specs.RollDueToShakingAngularSpeed * TimeElapsed;
			Specs.RollDueToShakingAngle = a0;
			// roll due to cant (incorporates shaking)
			double cantAngle = Math.Atan(Math.Tan(0.5 * (Math.Atan(FrontAxle.Follower.CurveCant) + Math.Atan(RearAxle.Follower.CurveCant))) / TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge);
			Specs.RollDueToCantAngle = cantAngle + Specs.RollDueToShakingAngle;
			// pitch due to acceleration
			for (int i = 0; i < 3; i++)
			{
				double a, v, j;
				switch (i)
				{
					case 0:
						a = Specs.Acceleration;
						v = Specs.PitchDueToAccelerationFastValue;
						j = 1.8;
						break;
					case 1:
						a = Specs.PitchDueToAccelerationFastValue;
						v = Specs.PitchDueToAccelerationMediumValue;
						j = 1.2;
						break;
					default:
						a = Specs.PitchDueToAccelerationFastValue;
						v = Specs.PitchDueToAccelerationSlowValue;
						j = 1.0;
						break;
				}

				double da = a - v;
				if (da < 0.0)
				{
					v -= j * TimeElapsed;
					if (v < a) v = a;
				}
				else
				{
					v += j * TimeElapsed;
					if (v > a) v = a;
				}

				switch (i)
				{
					case 0:
						Specs.PitchDueToAccelerationFastValue = v;
						break;
					case 1:
						Specs.PitchDueToAccelerationMediumValue = v;
						break;
					default:
						Specs.PitchDueToAccelerationSlowValue = v;
						break;
				}
			}

			Specs.PitchDueToAccelerationTargetAngle = 0.03 * Math.Atan(Specs.PitchDueToAccelerationSlowValue - Specs.PitchDueToAccelerationFastValue);
			double aa = 3.0 * Math.Sign(Specs.PitchDueToAccelerationTargetAngle - Specs.PitchDueToAccelerationAngle);
			Specs.PitchDueToAccelerationAngularSpeed += aa * TimeElapsed;
			double ds = Math.Abs(Specs.PitchDueToAccelerationTargetAngle - Specs.PitchDueToAccelerationAngle);
			if (Math.Abs(Specs.PitchDueToAccelerationAngularSpeed) > ds)
			{
				Specs.PitchDueToAccelerationAngularSpeed = ds * Math.Sign(Specs.PitchDueToAccelerationAngularSpeed);
			}

			Specs.PitchDueToAccelerationAngle += Specs.PitchDueToAccelerationAngularSpeed * TimeElapsed;
			// derailment
			if (TrainManagerBase.Derailments && !Derailed)
			{
				double a = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double sa = Math.Sign(a);
				if (a * sa > Specs.CriticalTopplingAngle)
				{
					baseTrain.Derail(Index, TimeElapsed);
				}
			}

			// toppling roll
			if (TrainManagerBase.Toppling | Derailed)
			{
				double ab = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double h = Specs.CenterOfGravityHeight;
				double sr = Math.Abs(CurrentSpeed);
				double rmax = 2.0 * h * sr * sr / (TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge);
				double ta;
				Topples = false;
				if (Derailed)
				{
					double sab = Math.Sign(ab);
					ta = 0.5 * Math.PI * (sab == 0.0 ? TrainManagerBase.currentHost.Random.NextDouble() < 0.5 ? -1.0 : 1.0 : sab);
				}
				else
				{
					if (r != 0.0)
					{
						if (r < rmax)
						{
							double s0 = Math.Sqrt(r * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge / (2.0 * h));
							const double fac = 0.25; // arbitrary coefficient
							ta = -fac * (sr - s0) * rs;
							Topples = true;
							//FIXME: DEBUG MESSAGE
							//baseTrain.Topple(Index, TimeElapsed);
						}
						else
						{
							ta = 0.0;
						}
					}
					else
					{
						ta = 0.0;
					}
				}

				double td = 1.0;
				if (Derailed)
				{
					td = Math.Abs(ab);
					if (td < 0.1) td = 0.1;
				}

				if (Specs.RollDueToTopplingAngle > ta)
				{
					double da = Specs.RollDueToTopplingAngle - ta;
					if (td > da) td = da;
					Specs.RollDueToTopplingAngle -= td * TimeElapsed;
				}
				else if (Specs.RollDueToTopplingAngle < ta)
				{
					double da = ta - Specs.RollDueToTopplingAngle;
					if (td > da) td = da;
					Specs.RollDueToTopplingAngle += td * TimeElapsed;
				}
			}
			else
			{
				Specs.RollDueToTopplingAngle = 0.0;
			}

			// apply position due to cant/toppling
			double ca = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
			double cx = Math.Sign(ca) * 0.5 * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * (1.0 - Math.Cos(ca));
			double cy = Math.Abs(0.5 * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * Math.Sin(ca));
			Vector3 cc = new Vector3(s.X * cx + Up.X * cy, s.Y * cx + Up.Y * cy, s.Z * cx + Up.Z * cy);
			FrontAxle.Follower.WorldPosition += cc;
			RearAxle.Follower.WorldPosition += cc;
			// apply rolling
			s.Rotate(d, -Specs.RollDueToTopplingAngle - Specs.RollDueToCantAngle);
			Up.Rotate(d, -Specs.RollDueToTopplingAngle - Specs.RollDueToCantAngle);
			// apply pitching
			if (CurrentCarSection >= 0 && CarSections[CurrentCarSection].Type == ObjectType.Overlay)
			{
				d.Rotate(s, Specs.PitchDueToAccelerationAngle);
				Up.Rotate(s, Specs.PitchDueToAccelerationAngle);
				Vector3 pc = new Vector3(0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition));
				FrontAxle.Follower.WorldPosition -= pc;
				RearAxle.Follower.WorldPosition -= pc;
				FrontAxle.Follower.WorldPosition.Rotate(s, Specs.PitchDueToAccelerationAngle);
				RearAxle.Follower.WorldPosition.Rotate(s, Specs.PitchDueToAccelerationAngle);
				FrontAxle.Follower.WorldPosition += pc;
				RearAxle.Follower.WorldPosition += pc;
			}

			Suspension.Update(TimeElapsed);
			Flange.Update(TimeElapsed);
		}

		public void UpdateSpeed(double TimeElapsed, double DecelerationDueToMotor, double DecelerationDueToBrake, out double Speed)
		{

			double PowerRollingCouplerAcceleration;
			// rolling on an incline
			{
				double a = FrontAxle.Follower.WorldDirection.Y;
				double b = RearAxle.Follower.WorldDirection.Y;
				PowerRollingCouplerAcceleration = -0.5 * (a + b) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}
			// friction
			double FrictionBrakeAcceleration;
			{
				double v = Math.Abs(CurrentSpeed);
				double t = (Index == 0 && CurrentSpeed >= 0.0) || (Index == baseTrain.NumberOfCars - 1 && CurrentSpeed <= 0.0) ? Specs.ExposedFrontalArea : Specs.UnexposedFrontalArea;

				if (t == 0)
				{
					// if frontal area is zero, multiplication creates NaN so use default BVE value (guarded against in original BVE parser)
					t = 5.616;
				}

				double a = FrontAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				double b = RearAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(RearAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				FrictionBrakeAcceleration = 0.5 * (a + b);
			}
			// power
			double wheelspin = 0.0;
			double wheelSlipAccelerationMotorFront = 0.0;
			double wheelSlipAccelerationMotorRear = 0.0;
			double wheelSlipAccelerationBrakeFront = 0.0;
			double wheelSlipAccelerationBrakeRear = 0.0;
			if (!Derailed)
			{
				if (TrainManagerBase.CurrentOptions.AdhesionHack)
				{
					wheelSlipAccelerationMotorFront = double.MaxValue;
					wheelSlipAccelerationMotorRear = double.MaxValue;
					wheelSlipAccelerationBrakeFront = double.MaxValue;
					wheelSlipAccelerationBrakeRear = double.MaxValue;
				}
				else
				{
					wheelSlipAccelerationMotorFront = FrontAxle.CriticalWheelSlipAccelerationForElectricMotor(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
					wheelSlipAccelerationMotorRear = RearAxle.CriticalWheelSlipAccelerationForElectricMotor(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
					wheelSlipAccelerationBrakeFront = FrontAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
					wheelSlipAccelerationBrakeRear = RearAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				}
			}

			if (DecelerationDueToMotor == 0.0)
			{
				double a;
				if (DecelerationDueToMotor == 0.0 || !TractionModel.ProvidesPower)
				{
					if (baseTrain.Handles.Reverser.Actual != 0 & baseTrain.Handles.Power.Actual > 0 &
					    !baseTrain.Handles.HoldBrake.Actual &
					    !baseTrain.Handles.EmergencyBrake.Actual)
					{
						// target acceleration
						a = TractionModel.TargetAcceleration;
						
						// readhesion device
						if (ReAdhesionDevice is BveReAdhesionDevice device)
						{
							if (a > device.MaximumAccelerationOutput)
							{
								a = device.MaximumAccelerationOutput;
							}
						}
						else if (ReAdhesionDevice is Sanders sanders)
						{
							if (sanders.State == SandersState.Active && CurrentSpeed < sanders.MaximumSpeed)
							{
								wheelSlipAccelerationMotorFront *= 2.0;
								wheelSlipAccelerationMotorRear *= 2.0;
								wheelSlipAccelerationBrakeFront *= 2.0;
								wheelSlipAccelerationBrakeRear *= 2.0;
							}
						}


						// wheel slip
						if (a < wheelSlipAccelerationMotorFront)
						{
							FrontAxle.CurrentWheelSlip = false;
						}
						else
						{
							FrontAxle.CurrentWheelSlip = true;
							wheelspin += (double)baseTrain.Handles.Reverser.Actual * a * CurrentMass;
						}

						if (a < wheelSlipAccelerationMotorRear)
						{
							RearAxle.CurrentWheelSlip = false;
						}
						else
						{
							RearAxle.CurrentWheelSlip = true;
							wheelspin += (double)baseTrain.Handles.Reverser.Actual * a * CurrentMass;
						}

						TractionModel.MaximumCurrentAcceleration = a;
						if (SafetySystems.TryGetTypedValue(SafetySystem.ConstantSpeedDevice, out CarConstSpeed constantSpeed))
						{
							constantSpeed.Update(ref a);
						}
						
						// finalize
						if (wheelspin != 0.0) a = 0.0;
					}
					else
					{
						a = 0.0;
						FrontAxle.CurrentWheelSlip = false;
						RearAxle.CurrentWheelSlip = false;
					}
				}
				else
				{
					// HACK: Use special value here to inform the BVE readhesion device it shouldn't update this frame
					TractionModel.MaximumCurrentAcceleration = -1;
					a = 0.0;
					FrontAxle.CurrentWheelSlip = false;
					RearAxle.CurrentWheelSlip = false;
				}


				if (!Derailed)
				{
					if (TractionModel.CurrentAcceleration < a)
					{
						if (TractionModel.CurrentAcceleration < 0.0)
						{
							TractionModel.CurrentAcceleration += Math.Max(CarBrake.JerkDown, 10) * TimeElapsed;
						}
						else
						{
							TractionModel.CurrentAcceleration += Math.Max(Specs.JerkPowerUp, 10) * TimeElapsed;
						}

						if (TractionModel.CurrentAcceleration > a)
						{
							TractionModel.CurrentAcceleration = a;
						}
					}
					else
					{
						TractionModel.CurrentAcceleration -= Math.Max(Specs.JerkPowerDown, 10) * TimeElapsed;
						if (TractionModel.CurrentAcceleration < a)
						{
							TractionModel.CurrentAcceleration = a;
						}
					}
				}
				else
				{
					TractionModel.CurrentAcceleration = 0.0;
				}
			}

			ReAdhesionDevice?.Update(TimeElapsed);
			// brake
			bool wheellock = wheelspin == 0.0 & Derailed;
			if (!Derailed & wheelspin == 0.0)
			{
				double a;
				// motor
				if (TractionModel.ProvidesPower & DecelerationDueToMotor != 0.0)
				{
					a = -DecelerationDueToMotor;
					if (TractionModel.CurrentAcceleration > a)
					{
						if (TractionModel.CurrentAcceleration > 0.0)
						{
							TractionModel.CurrentAcceleration -= Math.Max(Specs.JerkPowerDown, 10) * TimeElapsed;
						}
						else
						{
							TractionModel.CurrentAcceleration -= Math.Max(CarBrake.JerkUp, 10) * TimeElapsed;
						}

						if (TractionModel.CurrentAcceleration < a)
						{
							TractionModel.CurrentAcceleration = a;
						}
					}
					else
					{
						TractionModel.CurrentAcceleration += Math.Max(CarBrake.JerkDown, 10) * TimeElapsed;
						if (TractionModel.CurrentAcceleration > a)
						{
							TractionModel.CurrentAcceleration = a;
						}
					}
				}

				// brake
				a = DecelerationDueToBrake;
				if (CurrentSpeed >= -0.01 & CurrentSpeed <= 0.01)
				{
					double rf = FrontAxle.Follower.WorldDirection.Y;
					double rr = RearAxle.Follower.WorldDirection.Y;
					double ra = Math.Abs(0.5 * (rf + rr) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
					if (ra != 0 && a > ra) a = ra;
				}

				double factor = 1.0;
				if (EmptyMass != 0 && CurrentMass != 0)
				{
					// zero weight bugs out the factor
					factor = EmptyMass / CurrentMass;
				}
				if (a >= wheelSlipAccelerationBrakeFront)
				{
					wheellock = true;
				}
				else
				{
					FrictionBrakeAcceleration += 0.5 * a * factor;
				}

				if (a >= wheelSlipAccelerationBrakeRear)
				{
					wheellock = true;
				}
				else
				{
					FrictionBrakeAcceleration += 0.5 * a * factor;
				}
			}
			else if (Derailed)
			{
				FrictionBrakeAcceleration += TrainBase.CoefficientOfGroundFriction * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}

			// motor
			if (baseTrain.Handles.Reverser.Actual != 0)
			{
				double factor = 1.0;
				if (EmptyMass != 0 && CurrentMass != 0)
				{
					// zero weight bugs out the factor
					factor = EmptyMass / CurrentMass;
				}
				if (TractionModel.CurrentAcceleration > 0.0)
				{
					PowerRollingCouplerAcceleration += (double) baseTrain.Handles.Reverser.Actual * TractionModel.CurrentAcceleration * factor;
				}
				else
				{
					double a = -TractionModel.CurrentAcceleration;
					if (a >= wheelSlipAccelerationMotorFront)
					{
						FrontAxle.CurrentWheelSlip = true;
					}
					else if (!Derailed)
					{
						FrictionBrakeAcceleration += 0.5 * a * factor;
					}

					if (a >= wheelSlipAccelerationMotorRear)
					{
						RearAxle.CurrentWheelSlip = true;
					}
					else
					{
						FrictionBrakeAcceleration += 0.5 * a * factor;
					}
				}
			}
			else
			{
				TractionModel.CurrentAcceleration = 0.0;
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
					target = CurrentSpeed;
				}
				else
				{
					target = CurrentSpeed + wheelspin / 2500.0;
				}

				double diff = target - Specs.PerceivedSpeed;
				double rate = (diff < 0.0 ? 5.0 : 1.0) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TimeElapsed;
				rate *= 1.0 - 0.7 / (diff * diff + 1.0);
				double factor = rate * rate;
				factor = 1.0 - factor / (factor + 1000.0);
				rate *= factor;
				if (diff >= -rate & diff <= rate)
				{
					Specs.PerceivedSpeed = target;
				}
				else
				{
					Specs.PerceivedSpeed += rate * Math.Sign(diff);
				}
			}
			// calculate new speed
			if (Math.Abs(PowerRollingCouplerAcceleration) < FrictionBrakeAcceleration)
			{
				if (Math.Sign(PowerRollingCouplerAcceleration) == Math.Sign(CurrentSpeed))
				{
					if (CurrentSpeed == 0)
					{
						Speed = 0.0;
					}
					else
					{
						double c = (FrictionBrakeAcceleration - Math.Abs(PowerRollingCouplerAcceleration)) * TimeElapsed;
						if (Math.Abs(CurrentSpeed) > c)
						{
							Speed = CurrentSpeed - Math.Sign(CurrentSpeed) * c;
						}
						else
						{
							Speed = 0.0;
						}
					}
				}
				else
				{
					double c = (Math.Abs(PowerRollingCouplerAcceleration) + FrictionBrakeAcceleration) * TimeElapsed;
					if (Math.Abs(CurrentSpeed) > c)
					{
						Speed = CurrentSpeed - Math.Sign(CurrentSpeed) * c;
					}
					else
					{
						Speed = 0.0;
					}
				}
			}
			else
			{
				Speed = CurrentSpeed + (PowerRollingCouplerAcceleration - FrictionBrakeAcceleration * Math.Sign(CurrentSpeed)) * TimeElapsed;
			}
		}
	}
}
