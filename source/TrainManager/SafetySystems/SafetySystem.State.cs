
namespace TrainManager.SafetySystems
{
	/// <summary>The possible states of a SafetySystem</summary>
	public enum SafetySystemState
	{
		/// <summary>The device is monitoring</summary>
		Monitoring = 0,
		/// <summary>The device is in alarm state, but no penalty has yet been triggered</summary>
		Alarm = 1,
		/// <summary>A penalty has been triggered</summary>
		Triggered = 2,
	}
}
