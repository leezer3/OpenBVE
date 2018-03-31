namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal struct TrainSpecs
		{
			
			internal double CurrentAverageSpeed;
			internal double CurrentAverageAcceleration;
			internal double CurrentAirPressure;
			internal double CurrentAirDensity;
			internal double CurrentAirTemperature;
			internal double CurrentElevation;
			internal bool SingleHandle;
			internal int PowerNotchReduceSteps;
			internal int MaximumPowerNotch;
			
			internal int MaximumBrakeNotch;
			
			internal bool HasHoldBrake;
			
			internal DefaultSafetySystems DefaultSafetySystems;
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
			
			internal double[] DelayPowerUp;
			internal double[] DelayPowerDown;
			internal double[] DelayBrakeUp;
			internal double[] DelayBrakeDown;
			internal PassAlarmType PassAlarm;
			internal DoorMode DoorOpenMode;
			internal DoorMode DoorCloseMode;
			internal DoorInterlockStates DoorInterlockState;
			internal bool DoorClosureAttempted;
			internal EbHandleBehaviour EbHandlesAction;
		}
	}
}
