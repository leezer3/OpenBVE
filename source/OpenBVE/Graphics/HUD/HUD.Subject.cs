namespace OpenBve
{
	/// <summary>Describes the subject for a HUD Element</summary>
	internal enum HUDSubject
	{
		// Elements not shown in viewer mode
		Unknown = 0,
		Reverser,
		Power,
		Single,
		Brake,
		LocoBrake,
		DoorsLeft,
		DoorsRight,
		Score,
		ScoreMessages,
		ATS,
		// Elements shown in all game modes if appropriate
		StopLeft = 100,
		StopRight,
		StopNone,
		StopLeftTick,
		StopRightTick,
		StopNoneTick,
		Clock,
		Gradient,
		Speed,
		DistNextStation,
		DistNextStation2,
		FPS,
		AI,
		Messages
	}
}
