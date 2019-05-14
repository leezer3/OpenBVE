namespace OpenBve.SignalManager
{
	/// <summary>Defines the possible safety system to be used in the upcoming section</summary>
	/// <remarks>Currently only applies to the default JA safety systems</remarks>
	public enum SafetySystem
	{
		/// <summary>Any available safety system should be used (Either that from the previous station if defined or NONE)</summary>
		Any = -1,
		/// <summary>ATS should be used- The track is NOT fitted with ATC</summary>
		Ats = 0,
		/// <summary>ATC should be used</summary>
		Atc = 1
	}
}
