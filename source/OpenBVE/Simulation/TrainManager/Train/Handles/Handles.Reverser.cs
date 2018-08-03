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

			internal ReverserHandle()
			{
				Driver = ReverserPosition.Neutral;
				Actual = ReverserPosition.Neutral;
			}
		}
	}
}
