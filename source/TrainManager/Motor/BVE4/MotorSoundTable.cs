using SoundManager;

namespace TrainManager.Motor
{
	/// <summary>Describes a BVE2 / BVE4 MotorSound Table</summary>
	public struct BVEMotorSoundTable
	{
		/// <summary>The motor sound table entries</summary>
		public BVEMotorSoundTableEntry[] Entries;
		/// <summary>The sound buffer</summary>
		public SoundBuffer Buffer;
		/// <summary>The sound source for playback</summary>
		public SoundSource Source;
	}
}
