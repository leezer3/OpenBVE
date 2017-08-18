namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal struct CarSounds
		{
			internal MotorSound Motor;
			internal CarSound Adjust;
			internal CarSound Air;
			internal CarSound AirHigh;
			internal CarSound AirZero;
			internal CarSound Brake;
			internal CarSound BrakeHandleApply;
			internal CarSound BrakeHandleRelease;
			internal CarSound BrakeHandleMin;
			internal CarSound BrakeHandleMax;
			internal CarSound BreakerResume;
			internal CarSound BreakerResumeOrInterrupt;
			internal bool BreakerResumed;
			internal CarSound CpEnd;
			internal CarSound CpLoop;
			internal bool CpLoopStarted;
			internal CarSound CpStart;
			internal double CpStartTimeStarted;
			/// <summary>Played once when the left doors close</summary>
			internal CarSound DoorCloseL;
			/// <summary>Played once when the right doors close</summary>
			internal CarSound DoorCloseR;
			/// <summary>Played once when the left doors open</summary>
			internal CarSound DoorOpenL;
			/// <summary>Played once when the right doors open</summary>
			internal CarSound DoorOpenR;
			internal CarSound EmrBrake;
			internal CarSound[] Flange;
			internal double[] FlangeVolume;
			internal CarSound Halt;
			internal Horn[] Horns;
			internal CarSound Loop;
			internal CarSound MasterControllerUp;
			internal CarSound MasterControllerDown;
			internal CarSound MasterControllerMin;
			internal CarSound MasterControllerMax;
			/// <summary>Played once when all doors are closed</summary>
			internal CarSound PilotLampOn;
			/// <summary>Played once when the first door opens</summary>
			internal CarSound PilotLampOff;
			internal CarSound[] PointFrontAxle;
			internal CarSound[] PointRearAxle;
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
			internal int FrontAxleRunIndex;
			internal int RearAxleRunIndex;
			internal int FrontAxleFlangeIndex;
			internal int RearAxleFlangeIndex;
			internal double FlangePitch;
			internal double SpringPlayedAngle;
		}
	}
}
