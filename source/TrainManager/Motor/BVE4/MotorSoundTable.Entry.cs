using SoundManager;

namespace TrainManager.Motor
{
	/// <summary>Describes a MotorSoundTable entry</summary>
	/// <remarks>A MotorSoundTable contains 1 entry per 0.2 km/h</remarks>
	public struct BVEMotorSoundTableEntry
	{
		/// <summary>The sound buffer to play</summary>
		public SoundBuffer Buffer;
		/// <summary>The index of the sound buffer</summary>
		public int SoundIndex;
		/// <summary>The pitch to play at</summary>
		public float Pitch;
		/// <summary>The gain to play at</summary>
		public float Gain;
	}
}
