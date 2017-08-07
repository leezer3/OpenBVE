﻿using System;
using OpenBve.BrakeSystems;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/*
		 * Contains the base definition of a train car
		 */
		/// <summary>The root class for a train car within the simulation</summary>
		internal class Car
		{
			/// <summary>The width of the car in m</summary>
			internal double Width;
			/// <summary>The height of the car in m</summary>
			internal double Height;
			/// <summary>The length of the car in m</summary>
			internal double Length;
			/// <summary>The front axle</summary>
			internal Axle FrontAxle;
			/// <summary>The front bogie</summary>
			internal Bogie FrontBogie;
			/// <summary>The rear axle</summary>
			internal Axle RearAxle;
			/// <summary>The rear bogie</summary>
			internal Bogie RearBogie;
            /// <summary>The car re-adhesion device</summary>
		    internal ReAdhesionDevice reAdhesionDevice;
            /// <summary>The car constant speed device</summary>
		    internal ConstantSpeedDevice constantSpeedDevice;
			/// <summary>The car's hold brake</summary>
			internal CarHoldBrake HoldBrake;
			/// <summary>The car's brake type</summary>
			internal CarBrakeType BrakeType;
			/// <summary>The car's electropnuematic brake type</summary>
			internal EletropneumaticBrakeType ElectropneumaticType;
			/// <summary>The car's air brake</summary>
			internal AirBrake.CarAirBrake AirBrake;
			/// doors
			internal Door[] Doors;
			/// <summary>The car index within the train</summary>
			internal int Index;

			internal Horn[] Horns;

			internal Vector3 Up;
			/// <summary>The car sections (Objects associated with the car)</summary>
			internal CarSection[] CarSections;
			/// <summary>The index of the current car section</summary>
			internal int CurrentCarSection;
			/// <summary>The position of the driver's eyes within the car</summary>
			internal Vector3 DriverPosition;
			/// <summary>The current camera yaw for the driver's eyes</summary>
			internal double DriverYaw;
			/// <summary>The current camera pitch for the driver's eyes</summary>
			internal double DriverPitch;
			/// <summary>The performance specifications associated with the car</summary>
			internal CarSpecs Specs;
			/// <summary>The car sounds</summary>
			internal CarSounds Sounds;
			/// <summary>Whether the car is currently visible</summary>
			internal bool CurrentlyVisible;
			/// <summary>Whether the car is currently derailed</summary>
			internal bool Derailed;
			/// <summary>Whether the car is currently toppled</summary>
			internal bool Topples;
			/// <summary>The current brightness value for the driver's view</summary>
			internal CarBrightness Brightness;
			/// <summary>The position of the beacon reciever in the car</summary>
			internal double BeaconReceiverPosition;
			/// <summary>The beacon reciever track follower</summary>
			internal TrackManager.TrackFollower BeaconReceiver;
			/// <summary>Whether this car is currently in a station</summary>
			/// TODO: This appears only to be set by the station start/ end events and not checked elsewhere, why???
			internal bool CurrentlyInStation;
			/// <summary>Holds a reference to the base train</summary>
		    internal Train Train;

			/// <summary>Creates a new car</summary>
			/// <param name="train">The base train</param>
			/// <param name="index">The car index within the train</param>
		    internal Car(Train train, int index)
		    {
		        this.Train = train;
		        this.Index = index;
		    }

			/// <summary>Call this method to move a car</summary>
			/// <param name="Delta">The length to move</param>
			/// <param name="TimeElapsed">The elapsed time</param>
			internal void Move(double Delta, double TimeElapsed)
			{
				if (Train.State != TrainState.Disposed)
				{
					TrackManager.UpdateTrackFollower(ref FrontAxle.Follower, FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref FrontBogie.FrontAxle.Follower, FrontBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref FrontBogie.RearAxle.Follower, FrontBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
					if (Train.State != TrainState.Disposed)
					{
						TrackManager.UpdateTrackFollower(ref RearAxle.Follower, RearAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref RearBogie.FrontAxle.Follower, RearBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref RearBogie.RearAxle.Follower, RearBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
						if (Train.State != TrainState.Disposed)
						{
							TrackManager.UpdateTrackFollower(ref BeaconReceiver, BeaconReceiver.TrackPosition + Delta, true, true);
						}
					}
				}
			}

			/// <summary>Gets the current acceleration output for this car</summary>
			/// <param name="CurveIndex">The acceleration curve to use</param>
			/// <param name="Speed">The speed in km/h</param>
			/// <returns>The acceleration output in m/s</returns>
			internal double GetAccelerationOutput(int CurveIndex, double Speed)
		    {
		        if (CurveIndex < Specs.AccelerationCurves.Length)
		        {
		            double a0 = Specs.AccelerationCurves[CurveIndex].StageZeroAcceleration;
		            double s1 = Specs.AccelerationCurves[CurveIndex].StageOneSpeed;
		            double a1 = Specs.AccelerationCurves[CurveIndex].StageOneAcceleration;
		            double s2 = Specs.AccelerationCurves[CurveIndex].StageTwoSpeed;
		            double e2 = Specs.AccelerationCurves[CurveIndex].StageTwoExponent;
		            double f = Specs.AccelerationCurvesMultiplier;
		            if (Speed <= 0.0)
		            {
		                return f * a0;
		            }
		            if (Speed < s1)
		            {
		                double t = Speed / s1;
		                return f * (a0 * (1.0 - t) + a1 * t);
		            }
		            if (Speed < s2)
		            {
		                return f * s1 * a1 / Speed;
		            }
		            return f * s1 * a1 * Math.Pow(s2, e2 - 1.0) * Math.Pow(Speed, -e2);

		        }
		        return 0.0;
		    }

			/// <summary>Is called once a frame to get the new speed for the car</summary>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			/// <param name="DecelerationDueToMotor">The decelleration applied by the regeneretive braking of the electric motor</param>
			/// <param name="DecelerationDueToBrake">The deceleration applied by the brakes</param>
			/// <returns>The new speed</returns>
		    internal double GetSpeed(double TimeElapsed, double DecelerationDueToMotor, double DecelerationDueToBrake)
		    {
		        double newSpeed = 0.0;
		        double PowerRollingCouplerAcceleration;
		        // rolling on an incline
		        {
		            double a = FrontAxle.Follower.WorldDirection.Y;
		            double b = RearAxle.Follower.WorldDirection.Y;
		            PowerRollingCouplerAcceleration = -0.5 * (a + b) * Game.RouteAccelerationDueToGravity;
		        }
		        // friction
		        double FrictionBrakeAcceleration;
		        {
		            double a = GetResistance(true);
		            double b = GetResistance(false);
		            FrictionBrakeAcceleration = 0.5 * (a + b);
		        }
		        // power
		        double wheelspin = 0.0;
		        double wheelSlipAccelerationMotorFront;
		        double wheelSlipAccelerationMotorRear;
		        double wheelSlipAccelerationBrakeFront;
		        double wheelSlipAccelerationBrakeRear;
		        if (Derailed)
		        {
		            wheelSlipAccelerationMotorFront = 0.0;
		            wheelSlipAccelerationBrakeFront = 0.0;
		            wheelSlipAccelerationMotorRear = 0.0;
		            wheelSlipAccelerationBrakeRear = 0.0;
		        }
		        else
		        {
		            wheelSlipAccelerationMotorFront = GetCriticalWheelSlipAccelerationForElectricMotor(true);
		            wheelSlipAccelerationMotorRear = GetCriticalWheelSlipAccelerationForElectricMotor(false);
		            wheelSlipAccelerationBrakeFront = GetCriticalWheelSlipAccelerationForFrictionBrake(true);
		            wheelSlipAccelerationBrakeRear = GetCriticalWheelSlipAccelerationForFrictionBrake(false);
		        }
		        if (DecelerationDueToMotor == 0.0)
		        {
		            double a;
		            if (Specs.IsMotorCar)
		            {
		                if (DecelerationDueToMotor == 0.0)
		                {
		                    //Check if our current power, reverser and brake settings allow acceleration
		                    if (Train.Specs.CurrentReverser.Actual != 0 & Train.Specs.CurrentPowerNotch.Actual > 0 & !Train.Specs.CurrentHoldBrake.Actual & !Train.EmergencyBrake.Applied)
		                    {
		                        // target acceleration
		                        a = GetAccelerationOutput(Train.Specs.CurrentPowerNotch.Actual - 1, (double)Train.Specs.CurrentReverser.Actual * Specs.CurrentSpeed);

		                        if (a > reAdhesionDevice.MaximumAccelerationOutput)
		                        {
		                            //Clamp max acceleration to that of the re-adhesion device
		                            a = reAdhesionDevice.MaximumAccelerationOutput;
		                        }
		                        // wheel slip
		                        if (a < wheelSlipAccelerationMotorFront)
		                        {
		                            FrontAxle.CurrentWheelSlip = false;
		                        }
		                        else
		                        {
		                            FrontAxle.CurrentWheelSlip = true;
		                            wheelspin += (double)Train.Specs.CurrentReverser.Actual * a * Specs.MassCurrent;
		                        }
		                        if (a < wheelSlipAccelerationMotorRear)
		                        {
		                            RearAxle.CurrentWheelSlip = false;
		                        }
		                        else
		                        {
		                            RearAxle.CurrentWheelSlip = true;
		                            wheelspin += (double)Train.Specs.CurrentReverser.Actual * a * Specs.MassCurrent;
		                        }
		                        //Update the re-adhesion device
		                        reAdhesionDevice.Update(a);
		                        //Update the constant-speed device
		                        constantSpeedDevice.Update(ref a, Train.Specs.CurrentConstSpeed, Train.Specs.CurrentReverser.Actual);
		                        // finalize
		                        if (wheelspin != 0.0)
		                        {
		                            //If wheels are spinning, acceleration output is zero
		                            a = 0.0;
		                        }
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
		                    a = 0.0;
		                    FrontAxle.CurrentWheelSlip = false;
		                    RearAxle.CurrentWheelSlip = false;
		                }
		            }
		            else
		            {
		                a = 0.0;
		                FrontAxle.CurrentWheelSlip = false;
		                RearAxle.CurrentWheelSlip = false;
		            }
		            if (!Derailed)
		            {
		                if (Specs.CurrentAccelerationOutput < a)
		                {
		                    if (Specs.CurrentAccelerationOutput < 0.0)
		                    {
		                        Specs.CurrentAccelerationOutput += Specs.JerkBrakeDown * TimeElapsed;
		                    }
		                    else
		                    {
		                        Specs.CurrentAccelerationOutput += Specs.JerkPowerUp * TimeElapsed;
		                    }
		                    if (Specs.CurrentAccelerationOutput > a)
		                    {
		                        Specs.CurrentAccelerationOutput = a;
		                    }
		                }
		                else
		                {
		                    Specs.CurrentAccelerationOutput -= Specs.JerkPowerDown * TimeElapsed;
		                    if (Specs.CurrentAccelerationOutput < a)
		                    {
		                        Specs.CurrentAccelerationOutput = a;
		                    }
		                }
		            }
		            else
		            {
		                Specs.CurrentAccelerationOutput = 0.0;
		            }
		        }
		        // brake
		        bool wheellock = wheelspin == 0.0 & Derailed;
		        if (!Derailed & wheelspin == 0.0)
		        {
		            double a;
		            // motor
		            if (Specs.IsMotorCar & DecelerationDueToMotor != 0.0)
		            {
		                a = -DecelerationDueToMotor;
		                if (Specs.CurrentAccelerationOutput > a)
		                {
		                    if (Specs.CurrentAccelerationOutput > 0.0)
		                    {
		                        Specs.CurrentAccelerationOutput -= Specs.JerkPowerDown * TimeElapsed;
		                    }
		                    else
		                    {
		                        Specs.CurrentAccelerationOutput -= Specs.JerkBrakeUp * TimeElapsed;
		                    }
		                    if (Specs.CurrentAccelerationOutput < a)
		                    {
		                        Specs.CurrentAccelerationOutput = a;
		                    }
		                }
		                else
		                {
		                    Specs.CurrentAccelerationOutput += Specs.JerkBrakeDown * TimeElapsed;
		                    if (Specs.CurrentAccelerationOutput > a)
		                    {
		                        Specs.CurrentAccelerationOutput = a;
		                    }
		                }
		            }
		            // brake
		            a = DecelerationDueToBrake;
		            if (Specs.CurrentSpeed >= -0.01 & Specs.CurrentSpeed <= 0.01)
		            {
		                double rf = FrontAxle.Follower.WorldDirection.Y;
		                double rr = RearAxle.Follower.WorldDirection.Y;
		                double ra = Math.Abs(0.5 * (rf + rr) * Game.RouteAccelerationDueToGravity);
		                if (a > ra) a = ra;
		            }
		            double factor = Specs.MassEmpty / Specs.MassCurrent;
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
		            FrictionBrakeAcceleration += Game.CoefficientOfGroundFriction * Game.RouteAccelerationDueToGravity;
		        }
		        // motor
		        if (Train.Specs.CurrentReverser.Actual != 0)
		        {
		            double factor = Specs.MassEmpty / Specs.MassCurrent;
		            if (Specs.CurrentAccelerationOutput > 0.0)
		            {
		                PowerRollingCouplerAcceleration += (double)Train.Specs.CurrentReverser.Actual * Specs.CurrentAccelerationOutput * factor;
		            }
		            else
		            {
		                double a = -Specs.CurrentAccelerationOutput;
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
		            Specs.CurrentAccelerationOutput = 0.0;
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
		                target = Specs.CurrentSpeed;
		            }
		            else
		            {
		                target = Specs.CurrentSpeed + wheelspin / 2500.0;
		            }
		            double diff = target - Specs.CurrentPerceivedSpeed;
		            double rate = (diff < 0.0 ? 5.0 : 1.0) * Game.RouteAccelerationDueToGravity * TimeElapsed;
		            rate *= 1.0 - 0.7 / (diff * diff + 1.0);
		            double factor = rate * rate;
		            factor = 1.0 - factor / (factor + 1000.0);
		            rate *= factor;
		            if (diff >= -rate & diff <= rate)
		            {
		                Specs.CurrentPerceivedSpeed = target;
		            }
		            else
		            {
		                Specs.CurrentPerceivedSpeed += rate * (double)Math.Sign(diff);
		            }
		        }
		        // perceived traveled distance
		        Specs.CurrentPerceivedTraveledDistance += Math.Abs(Specs.CurrentPerceivedSpeed) * TimeElapsed;
		        // calculate new speed
		        {
		            int d = Math.Sign(Specs.CurrentSpeed);
		            double a = PowerRollingCouplerAcceleration;
		            double b = FrictionBrakeAcceleration;
		            if (Math.Abs(a) < b)
		            {
		                if (Math.Sign(a) == d)
		                {
		                    if (d == 0)
		                    {
		                        newSpeed = 0.0;
		                    }
		                    else
		                    {
		                        double c = (b - Math.Abs(a)) * TimeElapsed;
		                        if (Math.Abs(Specs.CurrentSpeed) > c)
		                        {
		                            newSpeed = Specs.CurrentSpeed - (double)d * c;
		                        }
		                        else
		                        {
		                            newSpeed = 0.0;
		                        }
		                    }
		                }
		                else
		                {
		                    double c = (Math.Abs(a) + b) * TimeElapsed;
		                    if (Math.Abs(Specs.CurrentSpeed) > c)
		                    {
		                        newSpeed = Specs.CurrentSpeed - (double)d * c;
		                    }
		                    else
		                    {
		                        newSpeed = 0.0;
		                    }
		                }
		            }
		            else
		            {
		                newSpeed = Specs.CurrentSpeed + (a - b * (double)d) * TimeElapsed;
		            }
		        }
		        return newSpeed;
		    }

			/// <summary>Is called once a frame to update the toppling, cant and spring angles for the car</summary>
			/// <param name="TimeElapsed">The frame time elapsed</param>
		    internal void UpdateTopplingCantAndSpring(double TimeElapsed)
		    {
		        if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
		        {
		            return;
		        }
		        // get direction, up and side vectors
		        double dx, dy, dz;
		        double ux, uy, uz;
		        double sx, sy, sz;
		        {
		            dx = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
		            dy = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
		            dz = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
		            double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
		            dx *= t; dy *= t; dz *= t;
		            t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
		            double ex = dx * t;
		            double ez = dz * t;
		            sx = ez;
		            sy = 0.0;
		            sz = -ex;
		            World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
		        }
		        // cant and radius
		        double c;
		        {
		            double ca = FrontAxle.Follower.CurveCant;
		            double cb = RearAxle.Follower.CurveCant;
		            c = Math.Tan(0.5 * (Math.Atan(ca) + Math.Atan(cb)));
		        }
		        double r, rs;
		        if (FrontAxle.Follower.CurveRadius != 0.0 & RearAxle.Follower.CurveRadius != 0.0)
		        {
		            r = Math.Sqrt(Math.Abs(FrontAxle.Follower.CurveRadius * RearAxle.Follower.CurveRadius));
		            rs = (double)Math.Sign(FrontAxle.Follower.CurveRadius + RearAxle.Follower.CurveRadius);
		        }
		        else if (FrontAxle.Follower.CurveRadius != 0.0)
		        {
		            r = Math.Abs(FrontAxle.Follower.CurveRadius);
		            rs = (double)Math.Sign(FrontAxle.Follower.CurveRadius);
		        }
		        else if (RearAxle.Follower.CurveRadius != 0.0)
		        {
		            r = Math.Abs(RearAxle.Follower.CurveRadius);
		            rs = (double)Math.Sign(RearAxle.Follower.CurveRadius);
		        }
		        else
		        {
		            r = 0.0;
		            rs = 0.0;
		        }
		        // roll due to shaking
		        {

		            double a0 = Specs.CurrentRollDueToShakingAngle;
		            double a1;
		            if (Specs.CurrentRollShakeDirection != 0.0)
		            {
		                const double c0 = 0.03;
		                const double c1 = 0.15;
		                a1 = c1 * Math.Atan(c0 * Specs.CurrentRollShakeDirection);
		                double d = 0.5 + Specs.CurrentRollShakeDirection * Specs.CurrentRollShakeDirection;
		                if (Specs.CurrentRollShakeDirection < 0.0)
		                {
		                    Specs.CurrentRollShakeDirection += d * TimeElapsed;
		                    if (Specs.CurrentRollShakeDirection > 0.0) Specs.CurrentRollShakeDirection = 0.0;
		                }
		                else
		                {
		                    Specs.CurrentRollShakeDirection -= d * TimeElapsed;
		                    if (Specs.CurrentRollShakeDirection < 0.0) Specs.CurrentRollShakeDirection = 0.0;
		                }
		            }
		            else
		            {
		                a1 = 0.0;
		            }
		            double SpringAcceleration;
		            if (!Derailed)
		            {
		                SpringAcceleration = 15.0 * Math.Abs(a1 - a0);
		            }
		            else
		            {
		                SpringAcceleration = 1.5 * Math.Abs(a1 - a0);
		            }
		            double SpringDeceleration = 0.25 * SpringAcceleration;
		            Specs.CurrentRollDueToShakingAngularSpeed += (double)Math.Sign(a1 - a0) * SpringAcceleration * TimeElapsed;
		            double x = (double)Math.Sign(Specs.CurrentRollDueToShakingAngularSpeed) * SpringDeceleration * TimeElapsed;
		            if (Math.Abs(x) < Math.Abs(Specs.CurrentRollDueToShakingAngularSpeed))
		            {
		                Specs.CurrentRollDueToShakingAngularSpeed -= x;
		            }
		            else
		            {
		                Specs.CurrentRollDueToShakingAngularSpeed = 0.0;
		            }
		            a0 += Specs.CurrentRollDueToShakingAngularSpeed * TimeElapsed;
		            Specs.CurrentRollDueToShakingAngle = a0;
		        }
		        // roll due to cant (incorporates shaking)
		        {
		            double cantAngle = Math.Atan(c / Game.RouteRailGauge);
		            Specs.CurrentRollDueToCantAngle = cantAngle + Specs.CurrentRollDueToShakingAngle;
		        }
		        // pitch due to acceleration
		        {
		            for (int i = 0; i < 3; i++)
		            {
		                double a, v, j;
		                if (i == 0)
		                {
		                    a = Specs.CurrentAcceleration;
		                    v = Specs.CurrentPitchDueToAccelerationFastValue;
		                    j = 1.8;
		                }
		                else if (i == 1)
		                {
		                    a = Specs.CurrentPitchDueToAccelerationFastValue;
		                    v = Specs.CurrentPitchDueToAccelerationMediumValue;
		                    j = 1.2;
		                }
		                else
		                {
		                    a = Specs.CurrentPitchDueToAccelerationFastValue;
		                    v = Specs.CurrentPitchDueToAccelerationSlowValue;
		                    j = 1.0;
		                }
		                double d = a - v;
		                if (d < 0.0)
		                {
		                    v -= j * TimeElapsed;
		                    if (v < a) v = a;
		                }
		                else
		                {
		                    v += j * TimeElapsed;
		                    if (v > a) v = a;
		                }
		                if (i == 0)
		                {
		                    Specs.CurrentPitchDueToAccelerationFastValue = v;
		                }
		                else if (i == 1)
		                {
		                    Specs.CurrentPitchDueToAccelerationMediumValue = v;
		                }
		                else
		                {
		                    Specs.CurrentPitchDueToAccelerationSlowValue = v;
		                }
		            }
		            {
		                double d = Specs.CurrentPitchDueToAccelerationSlowValue - Specs.CurrentPitchDueToAccelerationFastValue;
		                Specs.CurrentPitchDueToAccelerationTargetAngle = 0.03 * Math.Atan(d);
		            }
		            {
		                double a = 3.0 * (double)Math.Sign(Specs.CurrentPitchDueToAccelerationTargetAngle - Specs.CurrentPitchDueToAccelerationAngle);
		                Specs.CurrentPitchDueToAccelerationAngularSpeed += a * TimeElapsed;
		                double s = Math.Abs(Specs.CurrentPitchDueToAccelerationTargetAngle - Specs.CurrentPitchDueToAccelerationAngle);
		                if (Math.Abs(Specs.CurrentPitchDueToAccelerationAngularSpeed) > s)
		                {
		                    Specs.CurrentPitchDueToAccelerationAngularSpeed = s * (double)Math.Sign(Specs.CurrentPitchDueToAccelerationAngularSpeed);
		                }
		                Specs.CurrentPitchDueToAccelerationAngle += Specs.CurrentPitchDueToAccelerationAngularSpeed * TimeElapsed;
		            }
		        }
		        // derailment
		        if (Interface.CurrentOptions.Derailments & !Derailed)
		        {
		            double a = Specs.CurrentRollDueToTopplingAngle + Specs.CurrentRollDueToCantAngle;
		            double sa = (double)Math.Sign(a);
		            double tc = Specs.CriticalTopplingAngle;
		            if (a * sa > tc)
		            {
		                Train.Derail(Index, TimeElapsed);
		            }
		        }
		        // toppling roll
		        if (Interface.CurrentOptions.Toppling | Derailed)
		        {
		            double a = Specs.CurrentRollDueToTopplingAngle;
		            double ab = Specs.CurrentRollDueToTopplingAngle + Specs.CurrentRollDueToCantAngle;
		            double h = Specs.CenterOfGravityHeight;
		            double s = Math.Abs(Specs.CurrentSpeed);
		            double rmax = 2.0 * h * s * s / (Game.RouteAccelerationDueToGravity * Game.RouteRailGauge);
		            double ta;
		            Topples = false;
		            if (Derailed)
		            {
		                double sab = (double)Math.Sign(ab);
		                ta = 0.5 * Math.PI * (sab == 0.0 ? Program.RandomNumberGenerator.NextDouble() < 0.5 ? -1.0 : 1.0 : sab);
		            }
		            else
		            {
		                if (r != 0.0)
		                {
		                    if (r < rmax)
		                    {
		                        double s0 = Math.Sqrt(r * Game.RouteAccelerationDueToGravity * Game.RouteRailGauge / (2.0 * h));
		                        const double fac = 0.25; // arbitrary coefficient
		                        ta = -fac * (s - s0) * rs;
		                        Train.Topple(Index, TimeElapsed);
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
		            double td;
		            if (Derailed)
		            {
		                td = Math.Abs(ab);
		                if (td < 0.1) td = 0.1;
		            }
		            else
		            {
		                td = 1.0;
		            }
		            if (a > ta)
		            {
		                double d = a - ta;
		                if (td > d) td = d;
		                a -= td * TimeElapsed;
		            }
		            else if (a < ta)
		            {
		                double d = ta - a;
		                if (td > d) td = d;
		                a += td * TimeElapsed;
		            }
		            Specs.CurrentRollDueToTopplingAngle = a;
		        }
		        else
		        {
		            Specs.CurrentRollDueToTopplingAngle = 0.0;
		        }
		        // apply position due to cant/toppling
		        {
		            double a = Specs.CurrentRollDueToTopplingAngle + Specs.CurrentRollDueToCantAngle;
		            double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
		            double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
		            double cx = sx * x + ux * y;
		            double cy = sy * x + uy * y;
		            double cz = sz * x + uz * y;
		            FrontAxle.Follower.WorldPosition.X += cx;
		            FrontAxle.Follower.WorldPosition.Y += cy;
		            FrontAxle.Follower.WorldPosition.Z += cz;
		            RearAxle.Follower.WorldPosition.X += cx;
		            RearAxle.Follower.WorldPosition.Y += cy;
		            RearAxle.Follower.WorldPosition.Z += cz;
		        }
		        // apply rolling
		        {
		            double a = -Specs.CurrentRollDueToTopplingAngle - Specs.CurrentRollDueToCantAngle;
		            double cosa = Math.Cos(a);
		            double sina = Math.Sin(a);
		            World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
		            World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
		            Up.X = ux;
		            Up.Y = uy;
		            Up.Z = uz;
		        }
		        // apply pitching
		        if (CurrentCarSection >= 0 && CarSections[CurrentCarSection].Overlay)
		        {
		            double a = Specs.CurrentPitchDueToAccelerationAngle;
		            double cosa = Math.Cos(a);
		            double sina = Math.Sin(a);
		            World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
		            World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
		            double cx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
		            double cy = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
		            double cz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
		            FrontAxle.Follower.WorldPosition.X -= cx;
		            FrontAxle.Follower.WorldPosition.Y -= cy;
		            FrontAxle.Follower.WorldPosition.Z -= cz;
		            RearAxle.Follower.WorldPosition.X -= cx;
		            RearAxle.Follower.WorldPosition.Y -= cy;
		            RearAxle.Follower.WorldPosition.Z -= cz;
		            World.Rotate(ref FrontAxle.Follower.WorldPosition, sx, sy, sz, cosa, sina);
		            World.Rotate(ref RearAxle.Follower.WorldPosition, sx, sy, sz, cosa, sina);
		            FrontAxle.Follower.WorldPosition.X += cx;
		            FrontAxle.Follower.WorldPosition.Y += cy;
		            FrontAxle.Follower.WorldPosition.Z += cz;
		            RearAxle.Follower.WorldPosition.X += cx;
		            RearAxle.Follower.WorldPosition.Y += cy;
		            RearAxle.Follower.WorldPosition.Z += cz;
		            Up.X = ux;
		            Up.Y = uy;
		            Up.Z = uz;
		        }
		        // spring sound
		        {
		            double a = Specs.CurrentRollDueToShakingAngle;
		            double diff = a - Sounds.SpringPlayedAngle;
		            const double angleTolerance = 0.001;
		            if (diff < -angleTolerance)
		            {
		                Sounds.SoundBuffer buffer = Sounds.SpringL.Buffer;
		                if (buffer != null)
		                {
		                    if (!OpenBve.Sounds.IsPlaying(Sounds.SpringL.Source))
		                    {
		                        OpenBveApi.Math.Vector3 pos = Sounds.SpringL.Position;
		                        Sounds.SpringL.Source = OpenBve.Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Index, false);
		                    }
		                }
		                Sounds.SpringPlayedAngle = a;
		            }
		            else if (diff > angleTolerance)
		            {
		                Sounds.SoundBuffer buffer = Sounds.SpringR.Buffer;
		                if (buffer != null)
		                {
		                    if (!OpenBve.Sounds.IsPlaying(Sounds.SpringR.Source))
		                    {
		                        OpenBveApi.Math.Vector3 pos = Sounds.SpringR.Position;
		                        Sounds.SpringR.Source = OpenBve.Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Index, false);
		                    }
		                }
		                Sounds.SpringPlayedAngle = a;
		            }
		        }
		        // flange sound
		        {
		            /*
                     * This determines the amount of flange noise as a result of the angle at which the
                     * line that forms between the axles hits the rail, i.e. the less perpendicular that
                     * line is to the rails, the more flange noise there will be.
                     * */
		            Vector3 d = FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition;
		            World.Normalize(ref d.X, ref d.Y, ref d.Z);
		            double b0 = d.X * RearAxle.Follower.WorldSide.X + d.Y * RearAxle.Follower.WorldSide.Y + d.Z * RearAxle.Follower.WorldSide.Z;
		            double b1 = d.X * FrontAxle.Follower.WorldSide.X + d.Y * FrontAxle.Follower.WorldSide.Y + d.Z * FrontAxle.Follower.WorldSide.Z;
		            double spd = Math.Abs(Specs.CurrentSpeed);
		            double pitch = 0.5 + 0.04 * spd;
		            double b2 = Math.Abs(b0) + Math.Abs(b1);
		            double basegain = 0.5 * b2 * b2 * spd * spd;
		            /*
                     * This determines additional flange noise as a result of the roll angle of the car
                     * compared to the roll angle of the rails, i.e. if the car bounces due to inaccuracies,
                     * there will be additional flange noise.
                     * */
		            double cdti = Math.Abs(FrontAxle.Follower.CantDueToInaccuracy) + Math.Abs(RearAxle.Follower.CantDueToInaccuracy);
		            basegain += 0.2 * spd * spd * cdti * cdti;
		            /*
                     * This applies the settings.
                     * */
		            if (basegain < 0.0) basegain = 0.0;
		            if (basegain > 0.75) basegain = 0.75;
		            if (pitch > Sounds.FlangePitch)
		            {
		                Sounds.FlangePitch += TimeElapsed;
		                if (Sounds.FlangePitch > pitch) Sounds.FlangePitch = pitch;
		            }
		            else
		            {
		                Sounds.FlangePitch -= TimeElapsed;
		                if (Sounds.FlangePitch < pitch) Sounds.FlangePitch = pitch;
		            }
		            pitch = Sounds.FlangePitch;
		            for (int i = 0; i < Sounds.Flange.Length; i++)
		            {
		                if (i == FrontAxle.currentFlangeIdx | i == RearAxle.currentFlangeIdx)
		                {
		                    Sounds.FlangeVolume[i] += TimeElapsed;
		                    if (Sounds.FlangeVolume[i] > 1.0) Sounds.FlangeVolume[i] = 1.0;
		                }
		                else
		                {
		                    Sounds.FlangeVolume[i] -= TimeElapsed;
		                    if (Sounds.FlangeVolume[i] < 0.0) Sounds.FlangeVolume[i] = 0.0;
		                }
		                double gain = basegain * Sounds.FlangeVolume[i];
		                if (OpenBve.Sounds.IsPlaying(Sounds.Flange[i].Source))
		                {
		                    if (pitch > 0.01 & gain > 0.0001)
		                    {
		                        Sounds.Flange[i].Source.Pitch = pitch;
		                        Sounds.Flange[i].Source.Volume = gain;
		                    }
		                    else
		                    {
		                        Sounds.Flange[i].Source.Stop();
		                    }
		                }
		                else if (pitch > 0.02 & gain > 0.01)
		                {
		                    Sounds.SoundBuffer buffer = Sounds.Flange[i].Buffer;
		                    if (buffer != null)
		                    {
		                        OpenBveApi.Math.Vector3 pos = Sounds.Flange[i].Position;
		                        Sounds.Flange[i].Source = OpenBve.Sounds.PlaySound(buffer, pitch, gain, pos, Train, Index, true);
		                    }
		                }
		            }
		        }
		    }

			/// <summary>Gets the aerodynamic resistance for this car</summary>
			/// <param name="frontAxle">NOT USED</param>
			/// <returns>The aerodynamic resistance value</returns>
		    internal double GetResistance(bool frontAxle)
		    {
		        double t;
		        if (Index == 0 & Specs.CurrentSpeed >= 0.0 || Index == Train.Cars.Length - 1 & Specs.CurrentSpeed <= 0.0)
		        {
		            t = Specs.ExposedFrontalArea;
		        }
		        else
		        {
		            t = Specs.UnexposedFrontalArea;
		        }
		        double f = t * Specs.AerodynamicDragCoefficient * Train.Specs.CurrentAirDensity / (2.0 * Specs.MassCurrent);
		        double a = Game.RouteAccelerationDueToGravity * Specs.CoefficientOfRollingResistance + f * Specs.CurrentSpeed * Specs.CurrentSpeed;
		        return a;
		    }

            /// <summary>Updates the BVE motor sounds for this car</summary>
		    internal void UpdateBVEMotorSounds()
		    {
		        Vector3 pos = Sounds.Motor.Position;
		        double speed = Math.Abs(Specs.CurrentPerceivedSpeed);
		        int idx = (int)Math.Round(speed * Sounds.Motor.SpeedConversionFactor);
		        int odir = Sounds.Motor.CurrentAccelerationDirection;
		        int ndir = Math.Sign(Specs.CurrentAccelerationOutput);
		        for (int h = 0; h < 2; h++)
		        {
		            int j = h == 0 ? TrainManager.MotorSound.MotorP1 : TrainManager.MotorSound.MotorP2;
		            int k = h == 0 ? TrainManager.MotorSound.MotorB1 : TrainManager.MotorSound.MotorB2;
		            if (odir > 0 & ndir <= 0)
		            {
		                if (j < Sounds.Motor.Tables.Length)
		                {
		                    OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
		                    Sounds.Motor.Tables[j].Source = null;
		                    Sounds.Motor.Tables[j].Buffer = null;
		                }
		            }
		            else if (odir < 0 & ndir >= 0)
		            {
		                if (k < Sounds.Motor.Tables.Length)
		                {
		                    OpenBve.Sounds.StopSound(Sounds.Motor.Tables[k].Source);
		                    Sounds.Motor.Tables[k].Source = null;
		                    Sounds.Motor.Tables[k].Buffer = null;
		                }
		            }
		            if (ndir != 0)
		            {
		                if (ndir < 0) j = k;
		                if (j < Sounds.Motor.Tables.Length)
		                {
		                    int idx2 = idx;
		                    if (idx2 >= Sounds.Motor.Tables[j].Entries.Length)
		                    {
		                        idx2 = Sounds.Motor.Tables[j].Entries.Length - 1;
		                    }
		                    if (idx2 >= 0)
		                    {
		                        Sounds.SoundBuffer obuf = Sounds.Motor.Tables[j].Buffer;
		                        Sounds.SoundBuffer nbuf = Sounds.Motor.Tables[j].Entries[idx2].Buffer;
		                        double pitch = Sounds.Motor.Tables[j].Entries[idx2].Pitch;
		                        double gain = Sounds.Motor.Tables[j].Entries[idx2].Gain;
		                        if (ndir == 1)
		                        {
		                            // power
		                            double max = Specs.AccelerationCurveMaximum;
		                            if (max != 0.0)
		                            {
		                                double cur = Specs.CurrentAccelerationOutput;
		                                if (cur < 0.0) cur = 0.0;
		                                gain *= Math.Pow(cur / max, 0.25);
		                            }
		                        }
		                        else if (ndir == -1)
		                        {
		                            // brake
		                            double max = AirBrake.DecelerationAtServiceMaximumPressure;
		                            if (max != 0.0)
		                            {
		                                double cur = -Specs.CurrentAccelerationOutput;
		                                if (cur < 0.0) cur = 0.0;
		                                gain *= Math.Pow(cur / max, 0.25);
		                            }
		                        }
		                        if (obuf != nbuf)
		                        {
		                            OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
		                            if (nbuf != null)
		                            {
		                                Sounds.Motor.Tables[j].Source = OpenBve.Sounds.PlaySound(nbuf, pitch, gain, pos, Train, Index, true);
		                                Sounds.Motor.Tables[j].Buffer = nbuf;
		                            }
		                            else
		                            {
		                                Sounds.Motor.Tables[j].Source = null;
		                                Sounds.Motor.Tables[j].Buffer = null;
		                            }
		                        }
		                        else if (nbuf != null)
		                        {
		                            if (Sounds.Motor.Tables[j].Source != null)
		                            {
		                                Sounds.Motor.Tables[j].Source.Pitch = pitch;
		                                Sounds.Motor.Tables[j].Source.Volume = gain;
		                            }
		                        }
		                        else
		                        {
		                            OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
		                            Sounds.Motor.Tables[j].Source = null;
		                            Sounds.Motor.Tables[j].Buffer = null;
		                        }
		                    }
		                    else
		                    {
		                        OpenBve.Sounds.StopSound(Sounds.Motor.Tables[j].Source);
		                        Sounds.Motor.Tables[j].Source = null;
		                        Sounds.Motor.Tables[j].Buffer = null;
		                    }
		                }
		            }
		        }
		        Sounds.Motor.CurrentAccelerationDirection = ndir;
		    }

            /// <summary>Updates the BVE run sounds for this car</summary>
            /// <param name="TimeElapsed">The elapsed time for this frame</param>
		    internal void UpdateBVERunSounds(double TimeElapsed)
		    {
		        const double factor = 0.04; // 90 km/h -> m/s -> 1/x
		        double speed = Math.Abs(Specs.CurrentSpeed);
		        if (Derailed)
		        {
		            speed = 0.0;
		        }
		        double pitch = speed * factor;
		        double basegain;
		        if (Specs.CurrentSpeed == 0.0)
		        {
		            if (Index != 0)
		            {
		                Sounds.RunNextReasynchronizationPosition = Train.Cars[0].FrontAxle.Follower.TrackPosition;
		            }
		        }
		        else if (Sounds.RunNextReasynchronizationPosition == double.MaxValue & FrontAxle.currentRunIdx >= 0)
		        {
		            double distance = Math.Abs(FrontAxle.Follower.TrackPosition - World.CameraTrackFollower.TrackPosition);
		            const double minDistance = 150.0;
		            const double maxDistance = 750.0;
		            if (distance > minDistance)
		            {
		                if (FrontAxle.currentRunIdx < Sounds.Run.Length)
		                {
		                    Sounds.SoundBuffer buffer = Sounds.Run[FrontAxle.currentRunIdx].Buffer;
		                    if (buffer != null)
		                    {
		                        double duration = OpenBve.Sounds.GetDuration(buffer);
		                        if (duration > 0.0)
		                        {
		                            double offset = distance > maxDistance ? 25.0 : 300.0;
		                            Sounds.RunNextReasynchronizationPosition = duration * Math.Ceiling((Train.Cars[0].FrontAxle.Follower.TrackPosition + offset) / duration);
		                        }
		                    }
		                }
		            }
		        }
		        if (FrontAxle.Follower.TrackPosition >= Sounds.RunNextReasynchronizationPosition)
		        {
		            Sounds.RunNextReasynchronizationPosition = double.MaxValue;
		            basegain = 0.0;
		        }
		        else
		        {
		            basegain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;
		        }
		        for (int j = 0; j < Sounds.Run.Length; j++)
		        {
		            if (j == FrontAxle.currentRunIdx | j == RearAxle.currentRunIdx)
		            {
		                Sounds.RunVolume[j] += 3.0 * TimeElapsed;
		                if (Sounds.RunVolume[j] > 1.0) Sounds.RunVolume[j] = 1.0;
		            }
		            else
		            {
		                Sounds.RunVolume[j] -= 3.0 * TimeElapsed;
		                if (Sounds.RunVolume[j] < 0.0) Sounds.RunVolume[j] = 0.0;
		            }
		            double gain = basegain * Sounds.RunVolume[j];
		            if (OpenBve.Sounds.IsPlaying(Sounds.Run[j].Source))
		            {
		                if (pitch > 0.01 & gain > 0.001)
		                {
		                    Sounds.Run[j].Source.Pitch = pitch;
		                    Sounds.Run[j].Source.Volume = gain;
		                }
		                else
		                {
		                    OpenBve.Sounds.StopSound(Sounds.Run[j].Source);
		                }
		            }
		            else if (pitch > 0.02 & gain > 0.01)
		            {
		                Sounds.SoundBuffer buffer = Sounds.Run[j].Buffer;
		                if (buffer != null)
		                {
		                    Vector3 pos = Sounds.Run[j].Position;
		                    Sounds.Run[j].Source = OpenBve.Sounds.PlaySound(buffer, pitch, gain, pos, Train, Index, true);
		                }
		            }
		        }
            }

		    internal double GetCriticalWheelSlipAccelerationForElectricMotor(bool frontAxle)
		    {
		        if (frontAxle)
		        {
		            double NormalForceAcceleration = FrontAxle.Follower.WorldUp.Y * Game.RouteAccelerationDueToGravity;
		            // TODO: Implement formula that depends on speed here.
		            double coefficient = Specs.CoefficientOfStaticFriction;
		            return coefficient * FrontAxle.Follower.AdhesionMultiplier * NormalForceAcceleration;
		        }
		        else
		        {
		            double NormalForceAcceleration = RearAxle.Follower.WorldUp.Y * Game.RouteAccelerationDueToGravity;
		            // TODO: Implement formula that depends on speed here.
		            double coefficient = Specs.CoefficientOfStaticFriction;
		            return coefficient * RearAxle.Follower.AdhesionMultiplier * NormalForceAcceleration;
                }
		    }

		    internal double GetCriticalWheelSlipAccelerationForFrictionBrake(bool frontAxle)
		    {
		        if (frontAxle)
		        {
		            double NormalForceAcceleration = FrontAxle.Follower.WorldUp.Y * Game.RouteAccelerationDueToGravity;
		            // TODO: Implement formula that depends on speed here.
		            double coefficient = Specs.CoefficientOfStaticFriction;
		            return coefficient * FrontAxle.Follower.AdhesionMultiplier * NormalForceAcceleration;
		        }
		        else
		        {
		            double NormalForceAcceleration = RearAxle.Follower.WorldUp.Y * Game.RouteAccelerationDueToGravity;
		            // TODO: Implement formula that depends on speed here.
		            double coefficient = Specs.CoefficientOfStaticFriction;
		            return coefficient * RearAxle.Follower.AdhesionMultiplier * NormalForceAcceleration;
		        }
            }

			/// <summary>Call once to initialize the car</summary>
            internal void Initialize()
		    {
		        for (int i = 0; i < CarSections.Length; i++)
		        {
		            CarSections[i].Initialize(CurrentlyVisible);
		        }
		        for (int i = 0; i < FrontBogie.CarSections.Length; i++)
		        {
		            FrontBogie.CarSections[i].Initialize(CurrentlyVisible);
		        }
		        for (int i = 0; i < RearBogie.CarSections.Length; i++)
		        {
		            RearBogie.CarSections[i].Initialize(CurrentlyVisible);
		        }
		        Brightness.PreviousBrightness = 1.0f;
		        Brightness.NextBrightness = 1.0f;
		    }

			/// <summary>Call once to initialize the car sounds</summary>
			internal void InitializeSounds()
			{
				Sounds.Run = new TrainManager.CarSound[] { };
				Sounds.Flange = new TrainManager.CarSound[] { };
				Sounds.Adjust = TrainManager.CarSound.Empty;
				Sounds.BrakeHandleApply = TrainManager.CarSound.Empty;
				Sounds.BrakeHandleMin = TrainManager.CarSound.Empty;
				Sounds.BrakeHandleMax = TrainManager.CarSound.Empty;
				Sounds.BrakeHandleRelease = TrainManager.CarSound.Empty;
				Sounds.BreakerResume = TrainManager.CarSound.Empty;
				Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				Sounds.DoorCloseL = TrainManager.CarSound.Empty;
				Sounds.DoorCloseR = TrainManager.CarSound.Empty;
				Sounds.DoorOpenL = TrainManager.CarSound.Empty;
				Sounds.DoorOpenR = TrainManager.CarSound.Empty;
				Sounds.EmrBrake = TrainManager.CarSound.Empty;
				Sounds.Flange = new TrainManager.CarSound[] { };
				Sounds.FlangeVolume = new double[] { };
				Sounds.Halt = TrainManager.CarSound.Empty;
				Horns = new TrainManager.Horn[]
				{
					new TrainManager.Horn(),
					new TrainManager.Horn(),
					new TrainManager.Horn()
				};
				Sounds.Loop = TrainManager.CarSound.Empty;
				Sounds.MasterControllerUp = TrainManager.CarSound.Empty;
				Sounds.MasterControllerDown = TrainManager.CarSound.Empty;
				Sounds.MasterControllerMin = TrainManager.CarSound.Empty;
				Sounds.MasterControllerMax = TrainManager.CarSound.Empty;
				Sounds.PilotLampOn = TrainManager.CarSound.Empty;
				Sounds.PilotLampOff = TrainManager.CarSound.Empty;
				FrontAxle.PointSounds = new TrainManager.CarSound[] { };
				RearAxle.PointSounds = new TrainManager.CarSound[] { };
				Sounds.ReverserOn = TrainManager.CarSound.Empty;
				Sounds.ReverserOff = TrainManager.CarSound.Empty;
				Sounds.Rub = TrainManager.CarSound.Empty;
				Sounds.Run = new TrainManager.CarSound[] { };
				Sounds.RunVolume = new double[] { };
				Sounds.SpringL = TrainManager.CarSound.Empty;
				Sounds.SpringR = TrainManager.CarSound.Empty;
				Sounds.Plugin = new TrainManager.CarSound[] { };
			}

		    /// <summary>Changes the currently visible car section</summary>
			/// <param name="SectionIndex">The new car section to show</param>
		    internal void ChangeCarSection(int SectionIndex)
		    {
		        if (CarSections.Length == 0)
		        {
		            SectionIndex = -1;
		        }
		        for (int i = 0; i < CarSections.Length; i++)
		        {
		            for (int j = 0; j < CarSections[i].Elements.Length; j++)
		            {
		                int o = CarSections[i].Elements[j].ObjectIndex;
		                Renderer.HideObject(o);
		            }
		        }
		        if (SectionIndex >= 0)
		        {
		            CarSections[SectionIndex].Initialize(CurrentlyVisible);
		            for (int j = 0; j < CarSections[SectionIndex].Elements.Length; j++)
		            {
		                int o = CarSections[SectionIndex].Elements[j].ObjectIndex;
		                if (CarSections[SectionIndex].Overlay)
		                {
		                    Renderer.ShowObject(o, Renderer.ObjectType.Overlay);
		                }
		                else
		                {
		                    Renderer.ShowObject(o, Renderer.ObjectType.Dynamic);
		                }
		            }
		        }
		        CurrentCarSection = SectionIndex;
		        //When changing car section, do not apply damping
		        //This stops objects from spinning if the last position before they were hidden is different
		        UpdateObjects(0.0, true, false);
		    }

			/// <summary>Is called to update any animated objects attached to the car</summary>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			/// <param name="ForceUpdate">Whether this is a forced update (camera change etc.)</param>
			/// <param name="EnableDamping">Whether to enable daming</param>
		    internal void UpdateObjects(double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		    {
		        // calculate positions and directions for section element update
		        double dx = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
		        double dy = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
		        double dz = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
		        double t = dx * dx + dy * dy + dz * dz;
		        double ux, uy, uz, sx, sy, sz;
		        if (t != 0.0)
		        {
		            t = 1.0 / Math.Sqrt(t);
		            dx *= t; dy *= t; dz *= t;
		            ux = Up.X;
		            uy = Up.Y;
		            uz = Up.Z;
		            sx = dz * uy - dy * uz;
		            sy = dx * uz - dz * ux;
		            sz = dy * ux - dx * uy;
		        }
		        else
		        {
		            ux = 0.0; uy = 1.0; uz = 0.0;
		            sx = 1.0; sy = 0.0; sz = 0.0;
		        }
		        double px = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
		        double py = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
		        double pz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
		        double d = 0.5 * (FrontAxle.Position + RearAxle.Position);
		        px -= dx * d;
		        py -= dy * d;
		        pz -= dz * d;
		        // determine visibility
		        double cdx = px - World.AbsoluteCameraPosition.X;
		        double cdy = py - World.AbsoluteCameraPosition.Y;
		        double cdz = pz - World.AbsoluteCameraPosition.Z;
		        double dist = cdx * cdx + cdy * cdy + cdz * cdz;
		        double bid = Interface.CurrentOptions.ViewingDistance + Length;
		        CurrentlyVisible = dist < bid * bid;
		        // Updates the brightness value
		        byte dnb;
		        {
		            float bb = (float)(Brightness.NextTrackPosition - Brightness.PreviousTrackPosition);

		            //1.0f represents a route brightness value of 255
		            //0.0f represents a route brightness value of 0

		            if (bb != 0.0f)
		            {
		                bb = (float)(FrontAxle.Follower.TrackPosition - Brightness.PreviousTrackPosition) / bb;
		                if (bb < 0.0f) bb = 0.0f;
		                if (bb > 1.0f) bb = 1.0f;
		                bb = Brightness.PreviousBrightness * (1.0f - bb) + Brightness.NextBrightness * bb;
		            }
		            else
		            {
		                bb = Brightness.PreviousBrightness;
		            }
		            //Calculate the cab brightness
		            double ccb = Math.Round(255.0 * (double)(1.0 - bb));
		            //DNB then must equal the smaller of the cab brightness value & the dynamic brightness value
		            dnb = (byte)Math.Min(Renderer.DynamicCabBrightness, ccb);
		        }
		        // update current section
		        int s = CurrentCarSection;
		        if (s >= 0)
		        {
		            for (int i = 0; i < CarSections[s].Elements.Length; i++)
		            {
		                UpdateSectionElement(s, i, new Vector3(px, py, pz), new Vector3(dx, dy, dz), new Vector3(ux, uy, uz), new Vector3(sx, sy, sz), CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

		                // brightness change
		                int o = CarSections[s].Elements[i].ObjectIndex;
		                if (ObjectManager.Objects[o] != null)
		                {
		                    for (int j = 0; j < ObjectManager.Objects[o].Mesh.Materials.Length; j++)
		                    {
		                        ObjectManager.Objects[o].Mesh.Materials[j].DaytimeNighttimeBlend = dnb;
		                    }
		                }
		            }
		        }
		    }

			/// <summary>Is called to update a car section element</summary>
			/// <param name="SectionIndex">The car section</param>
			/// <param name="ElementIndex">The element index</param>
			/// <param name="Position">The camera position</param>
			/// <param name="Direction">The camera direction</param>
			/// <param name="Up">The up vector</param>
			/// <param name="Side">The side vector</param>
			/// <param name="Show">Whether we are showing or hiding this object</param>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			/// <param name="ForceUpdate">Whether this is a forced update</param>
			/// <param name="EnableDamping">Whether to enable damping</param>
		    private void UpdateSectionElement(int SectionIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		    {
		        Vector3 p;
		        if (CarSections[SectionIndex].Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
		        {
		            p = new Vector3(DriverPosition.X, DriverPosition.Y, DriverPosition.Z);
		        }
		        else
		        {
		            p = Position;
		        }
		        double timeDelta;
		        bool updatefunctions;
		        if (CarSections[SectionIndex].Elements[ElementIndex].RefreshRate != 0.0)
		        {
		            if (CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate >= CarSections[SectionIndex].Elements[ElementIndex].RefreshRate)
		            {
		                timeDelta = CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
		                CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
		                updatefunctions = true;
		            }
		            else
		            {
		                timeDelta = TimeElapsed;
		                CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
		                updatefunctions = false;
		            }
		        }
		        else
		        {
		            timeDelta = CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
		            CarSections[SectionIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
		            updatefunctions = true;
		        }
		        if (ForceUpdate)
		        {
		            updatefunctions = true;
		        }
		        ObjectManager.UpdateAnimatedObject(ref CarSections[SectionIndex].Elements[ElementIndex], true, Train, Index, CurrentCarSection, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, CarSections[SectionIndex].Overlay, updatefunctions, Show, timeDelta, EnableDamping);
		    }

		    internal void CreateWorldCoordinates(double CarX, double CarY, double CarZ, out double PositionX, out double PositionY, out double PositionZ, out double DirectionX, out double DirectionY, out double DirectionZ)
		    {
		        DirectionX = FrontAxle.Follower.WorldPosition.X - RearAxle.Follower.WorldPosition.X;
		        DirectionY = FrontAxle.Follower.WorldPosition.Y - RearAxle.Follower.WorldPosition.Y;
		        DirectionZ = FrontAxle.Follower.WorldPosition.Z - RearAxle.Follower.WorldPosition.Z;
		        double t = DirectionX * DirectionX + DirectionY * DirectionY + DirectionZ * DirectionZ;
		        if (t != 0.0)
		        {
		            t = 1.0 / Math.Sqrt(t);
		            DirectionX *= t; DirectionY *= t; DirectionZ *= t;
		            double ux = Up.X;
		            double uy = Up.Y;
		            double uz = Up.Z;
		            double sx = DirectionZ * uy - DirectionY * uz;
		            double sy = DirectionX * uz - DirectionZ * ux;
		            double sz = DirectionY * ux - DirectionX * uy;
		            double rx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
		            double ry = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
		            double rz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
		            PositionX = rx + sx * CarX + ux * CarY + DirectionX * CarZ;
		            PositionY = ry + sy * CarX + uy * CarY + DirectionY * CarZ;
		            PositionZ = rz + sz * CarX + uz * CarY + DirectionZ * CarZ;
		        }
		        else
		        {
		            PositionX = FrontAxle.Follower.WorldPosition.X;
		            PositionY = FrontAxle.Follower.WorldPosition.Y;
		            PositionZ = FrontAxle.Follower.WorldPosition.Z;
		            DirectionX = 0.0;
		            DirectionY = 1.0;
		            DirectionZ = 0.0;
		        }
		    }
        }
	}
}
