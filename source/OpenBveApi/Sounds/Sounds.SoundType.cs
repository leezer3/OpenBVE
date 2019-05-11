namespace OpenBveApi.Sounds
{
	/// <summary>Represents the different types of sound</summary>
	public enum SoundType
	{
		/// <summary>The sound source is attached to the car of a train</summary>
		TrainCar,
		/// <summary>The sound source is emitted when triggered from a track location</summary>
		TrackSound,
		/// <summary>The sound source is ambient</summary>
		Ambient,
		/// <summary>The sound source is emitted from a fixed position (Placed via the routefile)</summary>
		FixedPosition,
		/// <summary>The sound source is emitted by a static object</summary>
		StaticObject,
		/// <summary>The sound source is emitted by an animated object, and may move</summary>
		AnimatedObject,
		/// <summary>The sound source is undefined</summary>
		Undefined
	}
}
