namespace OpenBveApi.Routes
{
	/// <summary>Represents a handle to an abstract background.</summary>
	public abstract class BackgroundHandle
	{
		/// <summary>Called once a frame to update the current state of the background</summary>
		/// <param name="ElapsedTime">The total elapsed frame time</param>
		/// <param name="Target">Whether this is the target background during a transition (Affects alpha rendering)</param>
		public abstract void UpdateBackground(double ElapsedTime, bool Target);

		/// <summary>Renders the background with the specified level of alpha and scale</summary>
		/// <param name="Alpha">The alpha</param>
		/// <param name="Scale">The scale</param>
		public abstract void RenderBackground(float Alpha, float Scale);

		/// <summary>Renders the background with the specified scale</summary>
		/// <param name="Scale">The scale</param>
		public abstract void RenderBackground(float Scale);

		/// <summary>The current transition mode between backgrounds</summary>
		public BackgroundTransitionMode Mode;

		/// <summary>The current transition countdown</summary>
		public double Countdown = -1;

		/// <summary>The current transition alpha level</summary>
		public float CurrentAlpha = 1.0f;

	}
}
