namespace TrainManager.TractionModels
{
	public enum BypassValveType
	{
		/// <summary>No bypass valve is fitted</summary>
		None,
		/// <summary>A manual bypass valve</summary>
		Manual,
		/// <summary>A fully automatic bypass valve</summary>
		Automatic,
		/// <summary>An automatic bypass valve, which triggers when regulator is applied</summary>
		AutomaticRegulatorTriggered
	}
}
