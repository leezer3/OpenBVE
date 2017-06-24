using System;

namespace OpenBve
{
    public static partial class TrainManager
    {
        /// <summary>Represents a re-adhesion device fitted to a train car</summary>
        internal class ReAdhesionDevice
        {
            /// <summary>The update interval</summary>
            internal double UpdateInterval;
            /// <summary>The maximum acceleration output</summary>
            internal double MaximumAccelerationOutput;
            internal double ApplicationFactor;
            internal double ReleaseInterval;
            internal double ReleaseFactor;
            /// <summary>The next update time</summary>
            private double NextUpdateTime;
            /// <summary>The time the wheels have not been slipping for</summary>
            private double TimeStable;
            /// <summary>A reference to the base car</summary>
            private readonly Car Car;
            /// <summary>Creates a new re-adhesion device</summary>
            /// <param name="car">The base car</param>
            internal ReAdhesionDevice(Car car)
            {
                UpdateInterval = 0.0;
                MaximumAccelerationOutput = Double.PositiveInfinity;
                ApplicationFactor = 0.0;
                ReleaseInterval = 0.0;
                ReleaseFactor = 0.0;
                NextUpdateTime = Game.SecondsSinceMidnight;
                TimeStable = 0.0;
                Car = car;
            }
            /// <summary>Updates the re-adhesion device</summary>
            /// <param name="currentAcceleration">The current acceleration</param>
            internal void Update(double currentAcceleration)
            {
                if (Game.SecondsSinceMidnight >= NextUpdateTime)
                {
                    NextUpdateTime = Game.SecondsSinceMidnight + UpdateInterval;
                    if (Car.FrontAxle.CurrentWheelSlip | Car.RearAxle.CurrentWheelSlip)
                    {
                        MaximumAccelerationOutput = currentAcceleration * ApplicationFactor;
                        TimeStable = 0.0;
                    }
                    else
                    {
                        TimeStable += UpdateInterval;
                        if (TimeStable >= ReleaseInterval)
                        {
                            TimeStable -= ReleaseInterval;
                            if (ReleaseFactor != 0.0 & MaximumAccelerationOutput <= currentAcceleration + 1.0)
                            {
                                if (MaximumAccelerationOutput < 0.025)
                                {
                                    MaximumAccelerationOutput = 0.025;
                                }
                                else
                                {
                                    MaximumAccelerationOutput *= ReleaseFactor;
                                }
                            }
                            else
                            {
                                MaximumAccelerationOutput = double.PositiveInfinity;
                            }
                        }
                    }
                }
            }
        }
    }
}
