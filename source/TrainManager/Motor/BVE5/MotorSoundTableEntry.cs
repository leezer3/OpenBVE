namespace TrainManager.Motor
{
	public class BVE5MotorSoundTableEntry
	{
		/// <summary>The set speed</summary>
		public readonly double Speed;
		/// <summary>The list of sounds to be played</summary>
		public SoundState[] Sounds;

		public BVE5MotorSoundTableEntry(double speed)
		{
			Speed = speed;
		}
	}
}
