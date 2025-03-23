namespace Formats.OpenBve
{
	/// <summary>The keys in a sound.cfg file</summary>
	public enum SoundCfgKey
	{
		Unknown = 0,
		// brake
		BcReleaseHigh,
		BcRelease,
		BcReleaseFull,
		Emergency,
		EmergencyRelease,
		BpDecomp,
		// compressor
		Attack,
		Loop,
		Release,
		// suspension
		Left,
		Right,
		// horn
		PrimaryStart,
		// duplicated indicies used for enum parsing
		PrimaryEnd = 13,
		PrimaryRelease = 13,
		PrimaryLoop,
		Primary,
		SecondaryStart,
		SecondaryEnd = 17,
		SecondaryRelease = 17,
		SecondaryLoop,
		Secondary,
		MusicStart,
		MusicEnd = 21,
		MusicRelease = 21,
		MusicLoop,
		Music,
		// door
		OpenLeft,
		OpenRight,
		CloseLeft,
		CloseRight,
		// buzzer
		Correct,
		// pilot lamp
		On,
		Off,
		// brake handle
		Apply,
		ApplyFast,
		// **release** duplicated above
		ReleaseFast,
		Min,
		Max,
		// master controller
		Up,
		UpFast,
		Down,
		DownFast,
		// **min** and **max** duplicated above
		// reverser + breaker **on** and **off** duplicated above
		//others
		Noise,
		Shoe,
		Halt,
		// windscreen
		RainDrop,
		WetWipe,
		DryWipe,
		Switch,
		// request stop
		Stop,
		Pass,
		Ignored
	}
}
