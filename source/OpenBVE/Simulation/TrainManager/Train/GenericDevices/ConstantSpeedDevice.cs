namespace OpenBve
{
    public static partial class TrainManager
    {
        /// <summary>Represents a constant speed device fitted to a train car</summary>
        internal class ConstantSpeedDevice
        {
            /// <summary>The current acceleration output</summary>
            internal double CurrentAccelerationOutput;
            /// <summary>The next update time</summary>
            private double NextUpdateTime;
            /// <summary>The update interval</summary>
            internal double UpdateInterval;
            /// <summary>A reference to the base car</summary>
            internal Car Car;
            /// <summary>Creates a new constant speed device</summary>
            /// <param name="car">The base car</param>
            internal ConstantSpeedDevice(Car car)
            {
                Car = car;
                NextUpdateTime = 0.0;
                UpdateInterval = 0.0;
            }
            /// <summary>Updates the constant speed device</summary>
            /// <param name="a">The acceleration output</param>
            /// <param name="Enabled">Whether this device is currently enabled</param>
            /// <param name="currentReverserPosition">The current reverser position</param>
            internal void Update(ref double a, bool Enabled, int currentReverserPosition)
            {
                if (Enabled)
                {
                    if (Game.SecondsSinceMidnight >= NextUpdateTime)
                    {
                        NextUpdateTime = Game.SecondsSinceMidnight + UpdateInterval;
                        CurrentAccelerationOutput -= 0.8 * Car.Specs.CurrentAcceleration * (double)currentReverserPosition;
                        if (CurrentAccelerationOutput < 0.0) CurrentAccelerationOutput = 0.0;
                    }
                    if (a > CurrentAccelerationOutput)
                    {
                        a = CurrentAccelerationOutput;
                    }
                    if (a < 0.0)
                    {
                        a = 0.0;
                    }
                }
                else
                {
                    CurrentAccelerationOutput = a;
                }
            }
        }
    }
}
