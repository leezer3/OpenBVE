namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The set of sounds attached to a car</summary>
		internal struct CarSounds
		{
			internal MotorSound Motor;
			internal CarSound Adjust;
			internal CarSound Air;
			internal CarSound AirHigh;
			internal CarSound AirZero;
			internal CarSound Brake;
			internal CarSound BrakeHandleApply;
			internal CarSound BrakeHandleApplyFast;
			internal CarSound BrakeHandleRelease;
			internal CarSound BrakeHandleReleaseFast;
			internal CarSound BrakeHandleMin;
			internal CarSound BrakeHandleMax;
			internal CarSound BreakerResume;
			internal CarSound BreakerResumeOrInterrupt;
			/// <summary>Whether the brake handle is being moved continuously</summary>
			internal bool BrakeHandleFast;
			/// <summary>Whether the power handle is being moved continuously</summary>
			internal bool PowerHandleFast;
			internal bool BreakerResumed;
			internal CarSound CpEnd;
			internal CarSound CpLoop;
			internal bool CpLoopStarted;
			internal CarSound CpStart;
			internal double CpStartTimeStarted;
			internal CarSound EmrBrake;
			internal CarSound[] Flange;
			internal double[] FlangeVolume;
			internal CarSound Halt;
			
			internal CarSound Loop;
			internal CarSound MasterControllerUp;
			internal CarSound MasterControllerUpFast;
			internal CarSound MasterControllerDown;
			internal CarSound MasterControllerDownFast;
			internal CarSound MasterControllerMin;
			internal CarSound MasterControllerMax;
			/// <summary>Played once when all doors are closed</summary>
			internal CarSound PilotLampOn;
			/// <summary>Played once when the first door opens</summary>
			internal CarSound PilotLampOff;
			internal CarSound Rub;
			internal CarSound ReverserOn;
			internal CarSound ReverserOff;
			internal CarSound[] Run;
			internal double[] RunVolume;
			internal double RunNextReasynchronizationPosition;
			internal CarSound SpringL;
			internal CarSound SpringR;
			internal CarSound[] Plugin;
			internal CarSound[] RequestStop;
			internal double FlangePitch;
			internal double SpringPlayedAngle;
			internal CarSound[] Touch;
		}
	}
}
