namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/*
		 * Contains various specifications for the train
		 * TODO: Most of this should be merged into the root train specification.....
		 */
		
		internal struct TrainSpecs
		{
			/// <summary>The base total mass of the train</summary>
			internal double TotalMass;
			/// <summary>The current reverser position</summary>
			internal ReverserHandle CurrentReverser;
			/// <summary>The current average speed across all cars within the train</summary>
			internal double CurrentAverageSpeed;
			/// <summary>The current average acceleration across all cars within the train</summary>
			internal double CurrentAverageAcceleration;
			/// <summary>The current average jerk across all cars within the train</summary>
			internal double CurrentAverageJerk;
			/// <summary>The current air pressure at this train's location</summary>
			internal double CurrentAirPressure;
			/// <summary>The current air density at this train's location</summary>
			internal double CurrentAirDensity;
			/// <summary>The current air temperature at this train's location</summary>
			internal double CurrentAirTemperature;
			/// <summary>The current elevation above sea-level at this train's location</summary>
			internal double CurrentElevation;
			/// <summary>Whether this train has a combined power and brake handle</summary>
			internal bool SingleHandle;
			internal int PowerNotchReduceSteps;
			internal int MaximumPowerNotch;
			internal PowerHandle CurrentPowerNotch;
			internal int MaximumBrakeNotch;
			internal BrakeHandle CurrentBrakeNotch;
			internal EmergencyHandle CurrentEmergencyBrake;
			internal bool HasHoldBrake;
			internal HoldBrakeHandle CurrentHoldBrake;
			internal DefaultSafetySystems DefaultSafetySystems;
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
			internal TrainAirBrake AirBrake;
			internal double DelayPowerUp;
			internal double DelayPowerDown;
			internal double DelayBrakeUp;
			internal double DelayBrakeDown;
			internal PassAlarmType PassAlarm;
			internal DoorMode DoorOpenMode;
			internal DoorMode DoorCloseMode;
			internal DoorInterlockStates DoorInterlockState;
			internal bool DoorClosureAttempted;
		}
	}
}
