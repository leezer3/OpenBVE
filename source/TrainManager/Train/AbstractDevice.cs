using OpenBveApi.Math;
using SoundManager;

namespace TrainManager.Trains
{
	/// <summary>An abstract device attached to a train</summary>
	public class AbstractDevice
	{
		/// <summary>The sound source for this device</summary>
		public SoundSource Source;
		/// <summary>The sound buffer to be played once when playback commences</summary>
		public SoundBuffer StartSound;
		/// <summary>The loop sound</summary>
		public SoundBuffer LoopSound;
		/// <summary>The idle loop sound</summary>
		public SoundBuffer IdleLoopSound;
		/// <summary>The sound buffer to be played once when playback ends</summary>
		public SoundBuffer EndSound;
		/// <summary>The position of the sound within the train car</summary>
		public Vector3 SoundPosition;
		/// <summary>Whether this device has start and end sounds, or uses the legacy loop/ stretch method</summary>
		public bool StartEndSounds;
		/// <summary>Stores whether the sound is looped or stretched when using the legacy method</summary>
		public bool Loop;
	}
}
