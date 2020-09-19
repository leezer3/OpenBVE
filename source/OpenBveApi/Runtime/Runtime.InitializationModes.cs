namespace OpenBveApi.Runtime
{
	/// <summary>Represents the mode in which the plugin should initialize.</summary>
	public enum InitializationModes 
	{
		/// <summary>The safety system should be enabled. The train has its service brakes applied. The numerical value of this constant is -1.</summary>
		OnService = -1,
		/// <summary>The safety system should be enabled. The train has its emergency brakes applied. The numerical value of this constant is 0.</summary>
		OnEmergency = 0,
		/// <summary>The safety system should be disabled. The train has its emergency brakes applied. The numerical value of this constant is 1.</summary>
		OffEmergency = 1
	}
}
