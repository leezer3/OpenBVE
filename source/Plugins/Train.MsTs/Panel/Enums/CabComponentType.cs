namespace Train.MsTs
{
	internal enum CabComponentType
	{
		/// <summary>None</summary>
		None = 0,
		/// <summary>Dial based control</summary>
		Dial = 1,
		/// <summary>Lever based control</summary>
		Lever = 2,
		/// <summary>Two-state based control</summary>
		TwoState = 3,
		/// <summary>Tri-state based control</summary>
		TriState = 4,
		/// <summary>A display capable of displaying N states</summary>
		MultiStateDisplay = 5,
		/// <summary>In cab signalling / safety system (e.g. AWS)</summary>
		CabSignalDisplay = 6
	}
}
