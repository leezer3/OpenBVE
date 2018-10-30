namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>The mode the viewport should change to</summary>
		internal enum ViewPortChangeMode
		{
			ChangeToScenery = 0,
			ChangeToCab = 1,
			NoChange = 2
		}

		/// <summary>The viewport modes</summary>
		private enum ViewPortMode
		{
			Scenery = 0,
			Cab = 1
		}
	}

}
