namespace OpenBveApi.Routes
{
	/// <summary>The different types of route loading mode</summary>
    public enum LoadingMode
    {
		/// <summary>The route is being loaded in preview (map) mode</summary>
	    Preview,
	    /// <summary>The route is being loaded by an in-game player</summary>
		InGame,
		// For future use
		/// <summary>The route is being loaded by a multiplayer server host</summary>
		ServerHost
	}
}
