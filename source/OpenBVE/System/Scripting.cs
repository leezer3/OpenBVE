using System;
using OpenBveApi.Math;
using TrainManager.Handles;

// ReSharper disable UnusedMember.Global

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
                    if (nextSectionIndex >= 0 & nextSectionIndex < Program.CurrentRoute.Sections.Length)
                    {
                        int a = Program.CurrentRoute.Sections[nextSectionIndex].CurrentAspect;
                        if (a >= 0 & a < Program.CurrentRoute.Sections[nextSectionIndex].Aspects.Length)
                        {
                            return Program.CurrentRoute.Sections[nextSectionIndex].Aspects[a].Number;
                        }
                        return 0;
                    }
                }
                else if (SectionIndex >= 0 & SectionIndex < Program.CurrentRoute.Sections.Length)
                {
                    int a = Program.CurrentRoute.Sections[SectionIndex].CurrentAspect;
                    if (a >= 0 & a < Program.CurrentRoute.Sections[SectionIndex].Aspects.Length)
                    {
                        return Program.CurrentRoute.Sections[SectionIndex].Aspects[a].Number;
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
            public static int numberOfCars(TrainManager.Train Train)
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
                if (CarIndex > Train.Cars.Length)
                {
                    return Train.Cars[0].CurrentSpeed;
                }
                return Train.Cars[CarIndex].CurrentSpeed;
            }

            /// <summary>Returns the speed of the selected car, accounting for wheelslip and wheel lock</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The car for which to get the speed</param>
            /// <returns>The speed in m/s</returns>
            public static double speedometer(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null)
                {
                    return 0;
                }
                return CarIndex > Train.Cars.Length
                    ? Train.Cars[0].Specs.CurrentPerceivedSpeed
                    : Train.Cars[CarIndex].Specs.CurrentPerceivedSpeed;
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
                return CarIndex > Train.Cars.Length
                    ? Train.Cars[0].Specs.CurrentAcceleration
                    : Train.Cars[CarIndex].Specs.CurrentAcceleration;
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
                            return Train.Cars[j].Specs.CurrentAccelerationOutput*
                                   (double) Math.Sign(Train.Cars[j].CurrentSpeed);
                        }
                        if (Train.Cars[j].Specs.CurrentAccelerationOutput > 0.0)
                        {
                            return Train.Cars[j].Specs.CurrentAccelerationOutput*
                                   (double) Train.Handles.Reverser.Actual;
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
                               (double) Math.Sign(Train.Cars[CarIndex].CurrentSpeed);
                    }
                    if (Train.Cars[CarIndex].Specs.CurrentAccelerationOutput > 0.0)
                    {
                        return Train.Cars[CarIndex].Specs.CurrentAccelerationOutput*
                               (double) Train.Handles.Reverser.Actual;
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
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    double fx = Train.Cars[j].FrontAxle.Follower.WorldPosition.X - Position.X;
                    double fy = Train.Cars[j].FrontAxle.Follower.WorldPosition.Y - Position.Y;
                    double fz = Train.Cars[j].FrontAxle.Follower.WorldPosition.Z - Position.Z;
                    double f = fx*fx + fy*fy + fz*fz;
                    if (f < dist) dist = f;
                    double rx = Train.Cars[j].RearAxle.Follower.WorldPosition.X - Position.X;
                    double ry = Train.Cars[j].RearAxle.Follower.WorldPosition.Y - Position.Y;
                    double rz = Train.Cars[j].RearAxle.Follower.WorldPosition.Z - Position.Z;
                    double r = rx*rx + ry*ry + rz*rz;
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
                double x = 0.5*
                           (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.X +
                            Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.X) - Position.X;
                double y = 0.5*
                           (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Y +
                            Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Y) - Position.Y;
                double z = 0.5*
                           (Train.Cars[CarIndex].FrontAxle.Follower.WorldPosition.Z +
                            Train.Cars[CarIndex].RearAxle.Follower.WorldPosition.Z) - Position.Z;
                return Math.Sqrt(x*x + y*y + z*z);
            }

            /// <summary>Returns the track distance to the nearest car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="TrackPosition">The object's track position</param>
            /// <returns>The distance to the object</returns>
            public static double trackDistance(TrainManager.Train Train, double TrackPosition)
            {
                if (Train == null) return 0.0;
                double t0 = Train.FrontCarTrackPosition();
                double t1 = Train.RearCarTrackPosition();
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
                double t1 = Train.Cars[CarIndex].RearAxle.Follower.TrackPosition - Train.Cars[CarIndex].RearAxle.Position -
                            0.5*Train.Cars[CarIndex].Length;
                return TrackPosition < t1 ? TrackPosition - t1 : 0.0;
            }

            /// <summary>Returns the main brake reservoir pressure of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>The main brake reservoir pressure in Pa</returns>
            public static double mainReservoir(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length > CarIndex)
                {
                    return 0.0;
                }
                return Train.Cars[CarIndex].CarBrake.mainReservoir.CurrentPressure;
            }

            /// <summary>Returns the brake pipe pressure of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>The brake pipe pressure in Pa</returns>
            public static double brakePipe(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length > CarIndex)
                {
                    return 0.0;
                }
                return Train.Cars[CarIndex].CarBrake.brakePipe.CurrentPressure;
            }

            /// <summary>Returns the brake cylinder pressure of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>The brake cylinder pressure in Pa</returns>
            public static double brakeCylinder(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length > CarIndex)
                {
                    return 0.0;
                }
                return Train.Cars[CarIndex].CarBrake.brakeCylinder.CurrentPressure;
            }

            /// <summary>Returns the brake pipe pressure of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>The brake pipe pressure in Pa</returns>
            public static double straightAirPipe(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length > CarIndex)
                {
                    return 0.0;
                }
                return Train.Cars[CarIndex].CarBrake.straightAirPipe.CurrentPressure;
            }

            /// <summary>Returns the doors state of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>0.0 if all doors are closed, 1.0 if all doors are open, or a value in between</returns>
            public static double doors(TrainManager.Train Train)
            {
                if (Train == null) return 0.0;
                double doorsState = 0.0;
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
                    {
                        if (Train.Cars[j].Doors[k].State > doorsState)
                        {
                            doorsState = Train.Cars[j].Doors[k].State;
                        }
                    }
                }
                return doorsState;
            }

            /// <summary>Returns the doors state of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>0.0 if all doors are closed, 1.0 if all doors are open, or a value in between</returns>
            public static double doors(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length <= CarIndex)
                {
                    double doorsState = 0.0;
                    for (int k = 0; k < Train.Cars[CarIndex].Doors.Length; k++)
                    {
                        if (Train.Cars[CarIndex].Doors[k].State > doorsState)
                        {
                            doorsState = Train.Cars[CarIndex].Doors[k].State;
                        }
                    }
                    return doorsState;
                }
                return 0.0;
            }

            /// <summary>Returns the left-hand doors state of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>0.0 if all doors are closed, 1.0 if all doors are open, or a value in between</returns>
            public static double leftDoors(TrainManager.Train Train)
            {
                if (Train == null) return 0.0;
                double doorsState = 0.0;
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
                    {
                        if (Train.Cars[j].Doors[k].Direction == -1 &
                            Train.Cars[j].Doors[k].State > doorsState)
                        {
                            doorsState = Train.Cars[j].Doors[k].State;
                        }
                    }
                }
                return doorsState;
            }

            /// <summary>Returns the left-hand doors state of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>0.0 if all doors are closed, 1.0 if all doors are open, or a value in between</returns>
            public static double leftDoors(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length <= CarIndex)
                {
                    double doorsState = 0.0;
                    for (int k = 0; k < Train.Cars[CarIndex].Doors.Length; k++)
                    {
                        if (Train.Cars[CarIndex].Doors[k].Direction == -1 &
                            Train.Cars[CarIndex].Doors[k].State > doorsState)
                        {
                            doorsState = Train.Cars[CarIndex].Doors[k].State;
                        }
                    }
                    return doorsState;
                }
                return 0.0;
            }

            /// <summary>Returns the left-hand doors state of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>0.0 if all doors are closed, 1.0 if all doors are open, or a value in between</returns>
            public static double rightDoors(TrainManager.Train Train)
            {
                if (Train == null) return 0.0;
                double doorsState = 0.0;
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
                    {
                        if (Train.Cars[j].Doors[k].Direction == 1 &
                            Train.Cars[j].Doors[k].State > doorsState)
                        {
                            doorsState = Train.Cars[j].Doors[k].State;
                        }
                    }
                }
                return doorsState;
            }

            /// <summary>Returns the left-hand doors state of the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>0.0 if all doors are closed, 1.0 if all doors are open, or a value in between</returns>
            public static double rightDoors(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return 0.0;
                if (Train.Cars.Length <= CarIndex)
                {
                    double doorsState = 0.0;
                    for (int k = 0; k < Train.Cars[CarIndex].Doors.Length; k++)
                    {
                        if (Train.Cars[CarIndex].Doors[k].Direction == 1 &
                            Train.Cars[CarIndex].Doors[k].State > doorsState)
                        {
                            doorsState = Train.Cars[CarIndex].Doors[k].State;
                        }
                    }
                    return doorsState;
                }
                return 0.0;
            }

            /// <summary>Returns whether the left doors are opening for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>True if the doors are opening, false otherwise</returns>
            public static bool leftDoorsTarget(TrainManager.Train Train)
            {
                if (Train == null) return false;
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    for (int k = 0; k < Train.Cars.Length; k++)
                    {
                        if (Train.Cars[j].Doors[0].AnticipatedOpen)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>Returns whether the left doors are opening for the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>True if the doors are opening, false otherwise</returns>
            public static bool leftDoorsTarget(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return false;
                if (Train.Cars.Length <= CarIndex)
                {
                    if (Train.Cars[CarIndex].Doors[0].AnticipatedOpen)
                    {
                        return true;
                    }

                }
                return false;
            }

            /// <summary>Returns whether the left doors are opening for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>True if the doors are opening, false otherwise</returns>
            public static bool rightDoorsTarget(TrainManager.Train Train)
            {
                if (Train == null) return false;
                for (int j = 0; j < Train.Cars.Length; j++)
                {
                    for (int k = 0; k < Train.Cars.Length; k++)
                    {
                        if (Train.Cars[j].Doors[1].AnticipatedOpen)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>Returns whether the left doors are opening for the selected car of the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="CarIndex">The selected car</param>
            /// <returns>True if the doors are opening, false otherwise</returns>
            public static bool rightDoorsTarget(TrainManager.Train Train, int CarIndex)
            {
                if (Train == null) return false;
                if (Train.Cars.Length <= CarIndex)
                {
                    if (Train.Cars[CarIndex].Doors[1].AnticipatedOpen)
                    {
                        return true;
                    }

                }
                return false;
            }

            /// <summary>Returns the driver's selected reverser position for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>-1 for backwards, 0 for neutral, 1 for forwards</returns>
            public static int reverserNotch(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                return (int)Train.Handles.Reverser.Driver;
            }

            /// <summary>Returns the driver's selected power notch for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The driver's selected power notch</returns>
            public static int powerNotch(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                return Train.Handles.Power.Driver;
            }

            /// <summary>Returns the maximum power notch for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The maximum power notch</returns>
            public static int powerNotches(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                return Train.Handles.Power.MaximumNotch;
            }

            /// <summary>Returns the driver's selected brake notch for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The driver's selected power notch</returns>
            public static int brakeNotch(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                return Train.Handles.Brake.Driver;
            }

            /// <summary>Returns the maximum brake notch for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The maximum power notch</returns>
            public static int brakeNotches(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                return Train.Handles.Brake.MaximumNotch;
            }

            /// <summary>Returns the driver's selected brake notch (Including EB) for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The driver's selected brake notch</returns>
            public static int brakeNotchLinear(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                if (Train.Handles.Brake is AirBrakeHandle)
                {
                    if (Train.Handles.EmergencyBrake.Driver)
                    {
                        return 3;
                    }
                    return (int) Train.Handles.Brake.Driver;
                }
                if (Train.Handles.HasHoldBrake)
                {
                    if (Train.Handles.EmergencyBrake.Driver)
                    {
                        return (int) Train.Handles.Brake.MaximumNotch + 2;
                    }
                    if (Train.Handles.Brake.Driver > 0)
                    {
                        return (int) Train.Handles.Brake.Driver + 1;
                    }
                    return Train.Handles.HoldBrake.Driver ? 1 : 0;
                }
                if (Train.Handles.EmergencyBrake.Driver)
                {
                    return (int) Train.Handles.Brake.MaximumNotch + 1;
                }
                return (int) Train.Handles.Brake.Driver;
            }

            /// <summary>Returns the maximum possible brake notch (Including EB) for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>The maximum possible brake notch</returns>
            public static int brakeNotchesLinear(TrainManager.Train Train)
            {
                if (Train == null) return 0;
                if (Train.Handles.Brake is AirBrakeHandle)
                {
                    return 3;
                }
                if (Train.Handles.HasHoldBrake)
                {
                    return 2;
                }
                return Train.Handles.Brake.MaximumNotch + 1;
            }

            /// <summary>Returns whether EB is active for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>Whether EB is active</returns>
            public static bool emergencyBrake(TrainManager.Train Train)
            {
                if (Train == null) return false;
                return Train.Handles.EmergencyBrake.Driver;
            }

            /// <summary>Whether the selected train has an automatic air brake</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>Whether the selected train has an automatic air brake</returns>
            public static bool hasAirBrake(TrainManager.Train Train)
            {
                if (Train == null) return false;
                return Train.Handles.Brake is AirBrakeHandle;
            }

            /// <summary>Whether the hold brake is currently active for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>Whether the hold brake is currently active</returns>
            public static bool holdBrake(TrainManager.Train Train)
            {
                if (Train == null) return false;
                return Train.Handles.HoldBrake.Driver;
            }

            /// <summary>Whether the selected train has a hold brake</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>Whether the selected train has a hold brake</returns>
            public static bool hasHoldBrake(TrainManager.Train Train)
            {
                if (Train == null) return false;
                return Train.Handles.HasHoldBrake;
            }

            /// <summary>Whether the constant speed devicee is currently active for the selected train</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>Whether the constant speed device is currently active</returns>
            public static bool constantSpeed(TrainManager.Train Train)
            {
                if (Train == null) return false;
                return Train.Specs.CurrentConstSpeed;
            }

            /// <summary>Whether the selected train has a constant speed device</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>Whether the selected train has a constant speed device</returns>
            public static bool hasConstantSpeed(TrainManager.Train Train)
            {
                if (Train == null) return false;
                return Train.Specs.HasConstSpeed;
            }

            /// <summary>Whether the selected train uses a custom plugin</summary>
            /// <param name="Train">The selected train</param>
            /// <returns>True if the train uses a custom plugin, false otherwise</returns>
            public static bool hasPlugin(TrainManager.Train Train)
            {
                if (Train == null) return false;
                if (Train.IsPlayerTrain && Train.Plugin != null)
                {
                    return TrainManager.PlayerTrain.Plugin.IsDefault;
                }
                return false;
            }

            /// <summary>Whether the selected train has a hold brake</summary>
            /// <param name="Train">The selected train</param>
            /// <param name="pluginState">The plugin state to query</param>
            /// <returns>The plugin state value</returns>
            public static int pluginState(TrainManager.Train Train, int pluginState)
            {
                if (Train == null || Train.Plugin == null)
                {
                    return 0;
                }

                if (pluginState >= 0 & pluginState < Train.Plugin.Panel.Length)
                {
                    return Train.Plugin.Panel[pluginState];
                }
                return 0;
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
                return Program.CurrentRoute.SecondsSinceMidnight;
            }

            /// <summary>Gets the camera distance in meters from the current object </summary>
            /// <param name="Position">The absolute in-game position of the current object to test against</param>
            /// <returns>The distance in meters</returns>
            public static double CameraDistance(Vector3 Position)
            {
                double dx = Program.Renderer.Camera.AbsolutePosition.X - Position.X;
                double dy = Program.Renderer.Camera.AbsolutePosition.Y - Position.Y;
                double dz = Program.Renderer.Camera.AbsolutePosition.Z - Position.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
        }
    }
}
