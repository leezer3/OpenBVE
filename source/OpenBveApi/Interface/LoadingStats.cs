namespace OpenBveApi.Interface
{
	/// <summary>Provides statistics about the loading process.</summary>
	public static class LoadingStats
	{
		/// <summary>The time taken to parse the route file in milliseconds.</summary>
		public static double RouteParseTime;
		/// <summary>The time taken to preload objects in milliseconds.</summary>
		public static double ObjectPreloadTime;
		/// <summary>The number of unique objects found during preloading.</summary>
		public static int ObjectsFound;
		/// <summary>The number of objects actually loaded.</summary>
		public static int ObjectsLoaded;
		/// <summary>The total time taken for the loading process in milliseconds.</summary>
		public static double TotalLoadingTime;
	}
}
