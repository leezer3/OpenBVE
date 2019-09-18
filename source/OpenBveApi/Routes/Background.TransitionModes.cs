namespace OpenBveApi.Routes
{
	/// <summary>The different methods of transitioning between backgrounds</summary>
	public enum BackgroundTransitionMode
	{
		/// <summary>No transition is performed</summary>
		None = 0,
		/// <summary>The new background fades in</summary>
		FadeIn = 1,
		/// <summary>The old background fades out</summary>
		FadeOut = 2
	}
}
