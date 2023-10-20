// ReSharper disable UnusedMember.Global
namespace OpenBveApi.Sounds
{
	/// <summary>Represents the different types of sound</summary>
	public enum SoundType
	{
		/// <summary>The sound source is attached to the car of a train</summary>
		TrainCar = 0,
		/// <summary>The sound source is emitted when triggered from a track location</summary>
		TrackSound = 1,
		/// <summary>The sound source is emitted in the train cab when triggered from a track location</summary>
		/// <remarks>Playback speed is not effected by train speed (e.g. announcement)</remarks>
		TrainStatic = 2,
		/// <summary>The sound source is emitted by each car when triggered from a track location</summary>
		/// <remarks>Playback speed is not effected by train speed (e.g. announcement)</remarks>
		TrainAllCarStatic = 3,
		/// <summary>The sound source is emitted in the train cab when triggered from a track location</summary>
		/// <remarks>Playback speed is effected by train speed (e.g. passing train sound)</remarks>
		TrainDynamic = 4,
		/// <summary>The sound source is emitted by each car when triggered from a track location</summary>
		/// <remarks>Playback speed is effected by train speed (e.g. passing train sound)</remarks>
		TrainAllCarDynamic = 5,
		/// <summary>The sound source is ambient</summary>
		Ambient = 6,
		/// <summary>The sound source is emitted from a fixed position (Placed via the routefile)</summary>
		FixedPosition = 7,
		/// <summary>The sound source is emitted by a static object</summary>
		StaticObject = 8,
		/// <summary>The sound source is emitted by an animated object, and may move</summary>
		AnimatedObject = 9,
		/// <summary>The sound source is undefined</summary>
		Undefined = -1
	}
}
