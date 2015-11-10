using System;
using OpenBveApi.Math;

namespace OpenBve
{
    /// <summary>This is a proxy class, which provides read-only access to variables, in a manner analagous to that found in .animated files </summary>
    /// Notes:
    /// A proxy class is required, as otherwise we start making a horrendous amount of stuff public which shouldn't be, when working with cross-assembly access.
    /// Doing it this way, also allows for sensible scripting access, rather than climbing through multiple levels.
    public static class Scripting
    {
        /// <summary>
        /// Provides scripting access to signals
        /// </summary>
        public static class Signal
        {
            /// <summary>This method returns the current aspect of the selected section</summary>
            /// <param name="Train">The train for which we wish to get the section index</param>
            /// <param name="SectionIndex">The section to get the aspect for</param>
            /// <param name="IsPartOfTrain">Defines whether we wish to get the section index for the selected train, or for when placed via a .SigF command</param>
            /// <returns>The aspect of the selected signal</returns>
            public static int CurrentAspect(TrainManager.Train Train, int SectionIndex, bool IsPartOfTrain)
            {
                if (IsPartOfTrain)
                {
                    int nextSectionIndex = Train.CurrentSectionIndex + 1;
                    if (nextSectionIndex >= 0 & nextSectionIndex < Game.Sections.Length)
                    {
                        int a = Game.Sections[nextSectionIndex].CurrentAspect;
                        if (a >= 0 & a < Game.Sections[nextSectionIndex].Aspects.Length)
                        {
                            return Game.Sections[nextSectionIndex].Aspects[a].Number;
                        }
                        return 0;
                    }
                }
                else if (SectionIndex >= 0 & SectionIndex < Game.Sections.Length)
                {
                    int a = Game.Sections[SectionIndex].CurrentAspect;
                    if (a >= 0 & a < Game.Sections[SectionIndex].Aspects.Length)
                    {
                        return Game.Sections[SectionIndex].Aspects[a].Number;
                    }
                }
                return 0;
            }
        }
        /// <summary>
        /// Provides scripting access to trains internal variables
        /// </summary>
        public static class Train
        {
            /// <summary>Returns the number of cars in this train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The number of cars</returns>
            public static int cars(TrainManager.Train Train)
            {
                if (Train != null)
                {
                    return Train.Cars.Length;
                }
                return 0;
            }

            /// <summary>Returns the speed of the selected car </summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The car for which to get the speed</param>
            /// <returns>The speed in m/s</returns>
            public static double speed(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null)
                {
                    return 0;
                }
                if (CarIndex >Train.Cars.Length)
                {
                    return Train.Cars[0].Specs.CurrentSpeed;
                }
                return Train.Cars[CarIndex].Specs.CurrentSpeed;
            }

            /// <summary>Returns the speed of the selected car, accounting for wheelslip & wheel lock</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The car for which to get the speed</param>
            /// <returns>The speed in m/s</returns>
            public static double speedometer(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null)
                {
                    return 0;
                }
                return CarIndex >Train.Cars.Length ? Train.Cars[0].Specs.CurrentPerceivedSpeed : Train.Cars[CarIndex].Specs.CurrentPerceivedSpeed;
            }

            /// <summary>Returns the acceleration of the selected car</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The car for which to get the acceleration</param>
            /// <returns>The acceleration in m/s</returns>
            public static double acceleration(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null)
                {
                    return 0;
                }
                return CarIndex >Train.Cars.Length ? Train.Cars[0].Specs.CurrentAcceleration : Train.Cars[CarIndex].Specs.CurrentAcceleration;
            }

            /// <summary>Returns the acceleration that the first motor car is currently generating in m/s</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The acceleration in m/s</returns>
            public static double accelerationMotor(TrainManager.Train Train)
            {
                if (Train == null) return 0.0;
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    if (Train.Cars[j].Specs.IsMotorCar)
                    {
                        // hack: CurrentAccelerationOutput does not distinguish between forward/backward
                        if (Train.Cars[j].Specs.CurrentAccelerationOutput < 0.0)
                        {
                            return Train.Cars[j].Specs.CurrentAccelerationOutput * (double) Math.Sign(Train.Cars[j].Specs.CurrentSpeed);
                        }
                        if (Train.Cars[j].Specs.CurrentAccelerationOutput >0.0)
                        {
                            return Train.Cars[j].Specs.CurrentAccelerationOutput*
                                   (double) Train.Specs.CurrentReverser.Actual;
                        }
                    }
                }
                return 0.0;
            }

            /// <summary>Returns the acceleration that the selected car's motor is currently generating in m/s</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>The acceleration in m/s</returns>
            public static double accelerationMotor(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null || Train.Cars.Length <= CarIndex) return 0.0;
                if (Train.Cars[CarIndex].Specs.IsMotorCar)
                {
                    // hack: CurrentAccelerationOutput does not distinguish between forward/backward
                    if (Train.Cars[CarIndex].Specs.CurrentAccelerationOutput < 0.0)
                    {
                        return Train.Cars[CarIndex].Specs.CurrentAccelerationOutput*
                               (double) Math.Sign(Train.Cars[CarIndex].Specs.CurrentSpeed);
                    }
                    if (Train.Cars[CarIndex].Specs.CurrentAccelerationOutput >0.0)
                    {
                        return Train.Cars[CarIndex].Specs.CurrentAccelerationOutput*
                               (double) Train.Specs.CurrentReverser.Actual;
                    }
                }
                return 0.0;
            }

            /// <summary>Returns the cartesian distance to the nearest car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="Position">The object's absolute in-game position</param>
            /// <returns>The distance to the object</returns>
            public static double distance(TrainManager.Train Train, Vector3 Position)
            {
                if (Train == null) return 0.0;
                double dist = double.MaxValue;
                for (int j = 0; j < Train.Cars.Length; j++) {
                    double fx = Train.Cars[j].FrontAxle.Follower.WorldPosition.X - Position.X;
                    double fy = Train.Cars[j].FrontAxle.Follower.WorldPosition.Y - Position.Y;
                    double fz = Train.Cars[j].FrontAxle.Follower.WorldPosition.Z - Position.Z;
                    double f = fx * fx + fy * fy + fz * fz;
                    if (f < dist) dist = f;
                    double rx = Train.Cars[j].RearAxle.Follower.WorldPosition.X - Position.X;
                    double ry = Train.Cars[j].RearAxle.Follower.WorldPosition.Y - Position.Y;
                    double rz = Train.Cars[j].RearAxle.Follower.WorldPosition.Z - Position.Z;
                    double r = rx * rx + ry * ry + rz * rz;
                    if (r < dist) dist = r;
                }
                return Math.Sqrt(dist);
            }

            /// <summary>Returns the cartesian distance to the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <param name="Position">The object's absolute in-game position</param>
            /// <returns>The distance to the object</returns>
            public static double distance(TrainManager.Train Train, int CarIndex, Vector3 Position)
            {
                if (Train == null || Train.Cars.Length <= CarIndex) return 0.0;
                double x = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X) - Position.X;
                double y = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y) - Position.Y;
                double z = 0.5 * (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z + Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z) - Position.Z;
                return Math.Sqrt(x * x + y * y + z * z);
            }

            /// <summary>Returns the track distance to the nearest car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="TrackPosition">The object's track position</param>
            /// <returns>The distance to the object</returns>
            public static double trackDistance(TrainManager.Train Train, double TrackPosition)
            {
                if (Train == null) return 0.0;
                int r = Train.Cars.Length - 1;
                double t0 = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
                double t1 = Train.Cars[r].RearAxle.Follower.TrackPosition - Train.Cars[r].RearAxlePosition - 0.5 * Train.Cars[r].Length;
                return TrackPosition > t0 ? TrackPosition - t0 : TrackPosition < t1 ? TrackPosition - t1 : 0.0;
            }

            /// <summary>Returns the track distance to the nearest car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <param name="TrackPosition">The object's track position</param>
            /// <returns>The distance to the object</returns>
            public static double trackDistance(TrainManager.Train Train, int CarIndex, double TrackPosition)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length > CarIndex)
                {
                    CarIndex = Train.Cars.Length - 1;
                }
                double t1 = Train.Cars[CarIndex].RearAxle.Follower.TrackPosition - Train.Cars[CarIndex].RearAxlePosition - 0.5 * Train.Cars[CarIndex].Length;
                return TrackPosition < t1 ? TrackPosition - t1 : 0.0;
            }


        }

        /// <summary>
        /// Provides scripting access to in simulation variables
        /// </summary>
        public static class Simulation
        {
            /// <summary>
            /// Gets the current in-simulation time
            /// </summary>
            /// <returns>Returns the number of seconds elapsed since midnight on the first day</returns>
            public static double time()
            {
                return Game.SecondsSinceMidnight;
            }

            /// <summary>Gets the camera distance in meters from the current object </summary>
            /// <param name="Position">The absolute in-game position of the current object to test against</param>
            /// <returns>The distance in meters</returns>
            public static double CameraDistance(Vector3 Position)
            {
                double dx = World.AbsoluteCameraPosition.X - Position.X;
                double dy = World.AbsoluteCameraPosition.Y - Position.Y;
                double dz = World.AbsoluteCameraPosition.Z - Position.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }
    }
}