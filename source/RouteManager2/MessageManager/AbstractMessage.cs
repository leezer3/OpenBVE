using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace RouteManager2.MessageManager
{
	/// <summary>Defines an abstract textual message</summary>
	public abstract class AbstractMessage
	{
		/// <summary>The object to be rendered</summary>
		/// NOTE: May be either a textual string, or a texture depending on the type
		public object MessageToDisplay;

		/// <summary>The highest game mode in which this message will display</summary>
		public GameMode Mode = GameMode.Expert;

		/// <summary>The track position at which this message is placed</summary>
		public double TrackPosition;

		/// <summary>The timeout in seconds for this message</summary>
		public double Timeout;

		/// <summary>Whether this message will trigger once, or repeatedly (If reversed over etc.)</summary>
		public bool TriggerOnce;

		/// <summary>Whether this message has triggered</summary>
		public bool Triggered;

		/// <summary>The on-screen message width</summary>
		public double Width;

		/// <summary>The on-screen message height</summary>
		public double Height;

		/// <summary>The direction(s) which this message triggers for</summary>
		public MessageDirection Direction;

		/// <summary>A textual key by which this message is referred to in-route</summary>
		public string Key;

		/// <summary>The trains for which this message displays</summary>
		public string[] Trains;

		/// <summary>The color which this message is displayed in</summary>
		public MessageColor Color;

		/// <summary>The on-screen position at which this message is displayed</summary>
		public Vector2 RendererPosition;

		/// <summary>The level of alpha used by the renderer whilst fading out the message</summary>
		public double RendererAlpha;

		/// <summary>Whether this message is queued for removal</summary>
		/// NOTE: May be called either by a track based beacon or by timeout
		public bool QueueForRemoval;

		/// <summary>The function called when the message is added</summary>
		/// <param name="currentTime">The current in-game time</param>
		public abstract void AddMessage(double currentTime);

		/// <summary>Called once a frame to update the message</summary>
		public virtual void Update()
		{
		}
	}
}
