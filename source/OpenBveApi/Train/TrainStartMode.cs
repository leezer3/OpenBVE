namespace OpenBveApi.Trains
{
	/// <summary>The startup state of the train</summary>
	public enum TrainStartMode
	{
		/// <summary>The train will start with the service brakes applied, and the safety-system plugin initialised</summary>
		ServiceBrakesAts = -1,
		/// <summary>The train will start with the EB brakes applied, and the safety-system plugin initialised</summary>
		EmergencyBrakesAts = 0,
		/// <summary>The train will start with the EB brakes applied, and the safety-system plugin inactive</summary>
		EmergencyBrakesNoAts = 1
	}
}
