using System;

namespace Train.MsTs
{
	[Flags]
	public enum ResetCondition
	{
		None = 0,
		ZeroSpeed = 1,
		ZeroThrottle= 2,
		DirectionNeutral = 4,
		EngineAtIdle = 8,
		BrakeOff = 16,
		BrakeFullyOn = 32,
		DynamicBrakeOff = 64,
		DynamicBrakeOn = 128,
		ResetButton = 256,
	}
}
