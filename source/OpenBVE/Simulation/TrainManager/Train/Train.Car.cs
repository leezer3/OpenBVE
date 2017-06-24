using System;
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

		    internal Train Train;

		    internal Car(Train train, int index)
		    {
		        this.Train = train;
		        this.Index = index;
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

		    // change car section
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
