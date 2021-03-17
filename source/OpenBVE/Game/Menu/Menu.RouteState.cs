namespace OpenBve
{
	public partial class Menu
	{
		private enum RouteState
		{
			/// <summary>No routefile is currently selected</summary>
			NoneSelected,
			/// <summary>The background thread is currently loading the routefile data</summary>
			Loading,
			/// <summary>The background thread has processed the route data</summary>
			Processed

		}
	}
	
}
