namespace OpenBveApi.Trains
{
	/// <summary>The different types of train</summary>
    public enum TrainType
    {
		/// <summary>The train is controlled by the local player</summary>
		LocalPlayerTrain,
		/// <summary>The train is controlled by a remote player</summary>
		RemotePlayerTrain,
		/// <summary>The train is controlled by legacy BVE PreTrain / RunInterval commands</summary>
		PreTrain,
		/// <summary>The train is controlled by a TFO script</summary>
		ScriptedTrain,
		/// <summary>The train consists of static cars and does not move independantly</summary>
		StaticCars
    }
}
