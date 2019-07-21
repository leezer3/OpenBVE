using SoundManager;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represnts a reverser handle</summary>
		internal class ReverserHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal ReverserPosition Driver;
			/// <summary>The actual notch</summary>
			internal ReverserPosition Actual;
			/// <summary>Played when the reverser is moved to F or R</summary>
			internal CarSound EngageSound;
			/// <summary>Played when the reverser is moved to N</summary>
			internal CarSound ReleaseSound;

			internal ReverserHandle()
			{
				Driver = ReverserPosition.Neutral;
				Actual = ReverserPosition.Neutral;
				EngageSound = new CarSound();
				ReleaseSound = new CarSound();
			}
		}
	}
}
