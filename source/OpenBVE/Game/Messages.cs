namespace OpenBve
{
	/*
	 * Holds the base routines used to add or remove a message from the in-game display.
	 * NOTE: Messages are rendered to the screen in Graphics\Renderer\Overlays.cs
	 * 
	 */
	internal static partial class Game
	{
		/// <summary>The number 1km/h must be multiplied by to produce your desired speed units, or 0.0 to disable this</summary>
		internal static double SpeedConversionFactor = 0.0;
		/// <summary>The unit of speed displayed in in-game messages</summary>
		internal static string UnitOfSpeed = "km/h";
	}
}
