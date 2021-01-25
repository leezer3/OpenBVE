namespace TrainManager.SafetySystems
{
	/// <summary>The available modes for the internal ATS system</summary>
	public enum AtsModes
	{
		/// <summary>Not enabled</summary>
		None = -1,
		/// <summary>ATS-SN</summary>
		AtsSn = 0,
		/// <summary>ATS-SN and ATS-P</summary>
		AtsSnP = 1
	}
}
