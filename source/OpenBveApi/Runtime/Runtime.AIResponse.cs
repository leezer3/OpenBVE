namespace OpenBveApi.Runtime
{
	/// <summary>Represents responses by the AI.</summary>
	public enum AIResponse 
	{
		/// <summary>No action was performed by the plugin.</summary>
		None = 0,
		/// <summary>The action performed took a short time.</summary>
		Short = 1,
		/// <summary>The action performed took an average amount of time.</summary>
		Medium = 2,
		/// <summary>The action performed took a long time.</summary>
		Long = 3
	}
}
