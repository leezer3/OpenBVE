namespace OpenBve.BrakeSystems
{
	/// <summary>Defines an emergency brake fitted to a complete train</summary>
    public class EmergencyBrake
    {
        /// <summary>The time of the last EB application in seconds since midnight</summary>
        private double ApplicationTime;
        /// <summary>Whether this is currently applied</summary>
        internal bool Applied;
        /// <summary>Whether a current driver application is in force</summary>
        internal bool DriverApplied;
        /// <summary>Whether a current safety system application is in force</summary>
        internal bool SafetySystemApplied;
        /// <summary>A reference to the base train</summary>
        private readonly TrainManager.Train Train;

		/// <summary>Create a new emergency brake</summary>
		/// <param name="train">The base train</param>
        internal EmergencyBrake(TrainManager.Train train)
        {
            Train = train;
            ApplicationTime = double.MaxValue;
        }

		/// <summary>Called once a frame to update the state of the emergency brake</summary>
        internal void Update()
        {
            if (SafetySystemApplied & !Applied)
            {
                double t = Game.SecondsSinceMidnight;
                if (t < ApplicationTime) ApplicationTime = t;
                if (ApplicationTime <= Game.SecondsSinceMidnight)
                {
                    Applied = true;
                    ApplicationTime = double.MaxValue;
                }
            }
            else if (!SafetySystemApplied)
            {
                Applied = false;
            }
        }

        /// <summary>Applies the emergency brake</summary>
        internal void Apply()
        {
            // sound
            if (!DriverApplied)
            {
                Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Buffer;
                if (buffer != null)
                {
                    OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Position;
                    Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
                }
                for (int i = 0; i < Train.Cars.Length; i++)
                {
                    buffer = Train.Cars[Train.DriverCar].Sounds.EmrBrake.Buffer;
                    if (buffer != null)
                    {
                        OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.EmrBrake.Position;
                        Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
                    }
                }
            }
            // apply
            Train.ApplyNotch(0, !Train.Specs.SingleHandle, Train.Specs.MaximumBrakeNotch, true);
            TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
            DriverApplied = true;
            Train.Specs.CurrentHoldBrake.Driver = false;
            Train.Specs.CurrentConstSpeed = false;
            // plugin
            if (Train.Plugin == null) return;
            Train.Plugin.UpdatePower();
            Train.Plugin.UpdateBrake();
        }

        /// <summary>Releases the emergency brake</summary>
        internal void Release()
        {
            if (!DriverApplied)
            {
                return;
            }
            // sound
            Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
            if (buffer != null)
            {
                OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
                Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
            }
            // apply
            Train.ApplyNotch(0, !Train.Specs.SingleHandle, Train.Specs.MaximumBrakeNotch, true);
            TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
            DriverApplied = false;
            // plugin
            if (Train.Plugin == null) return;
            Train.Plugin.UpdatePower();
            Train.Plugin.UpdateBrake();
        }
    }


}
