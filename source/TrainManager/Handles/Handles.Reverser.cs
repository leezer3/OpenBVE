using SoundManager;

namespace TrainManager.Handles
{
	/// <summary>Represnts a reverser handle</summary>
	public class ReverserHandle
	{
		/// <summary>The notch set by the driver</summary>
		public ReverserPosition Driver;
		/// <summary>The actual notch</summary>
		public ReverserPosition Actual;
		/// <summary>Played when the reverser is moved to F or R</summary>
		public CarSound EngageSound;
		/// <summary>Played when the reverser is moved to N</summary>
		public CarSound ReleaseSound;
		/// <summary>Contains the notch descriptions to be displayed on the in-game UI</summary>
		public string[] NotchDescriptions;
		/// <summary>The max width used in px for the reverser HUD string</summary>
		public int MaxWidth = 48;

		public ReverserHandle()
		{
			Driver = ReverserPosition.Neutral;
			Actual = ReverserPosition.Neutral;
			EngageSound = new CarSound();
			ReleaseSound = new CarSound();
		}
	}
}
