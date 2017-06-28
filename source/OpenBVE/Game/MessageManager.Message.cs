using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	/*
	 * This class holds the root abstract definition for a message
	 */
	partial class MessageManager
	{
		/// <summary>Defines an abstract textual message</summary>
		internal abstract class Message
		{
			/// <summary>The object to be rendered</summary>
			/// NOTE: May be either a textual string, or a texture depending on the type
			internal object MessageToDisplay;
			/// <summary>The track position at which this message is placed</summary>
			internal double TrackPosition;
			/// <summary>The timeout in seconds for this message</summary>
			internal double Timeout;
			/// <summary>Whether this message will trigger once, or repeatedly (If reversed over etc.)</summary>
			internal bool TriggerOnce;
			/// <summary>Whether this message has triggered</summary>
			internal bool Triggered;
			/// <summary>The on-screen message width</summary>
			internal double Width;
			/// <summary>The on-screen message height</summary>
			internal double Height;
			/// <summary>The direction(s) which this message triggers for</summary>
			internal MessageDirection Direction;
			/// <summary>A textual key by which this message is referred to in-route</summary>
			internal string Key;
			/// <summary>The trains for which this message displays</summary>
			internal string[] Trains;
			/// <summary>The color which this message is displayed in</summary>
			internal MessageColor Color;
			/// <summary>The on-screen position at which this message is displayed</summary>
			internal Vector2 RendererPosition;
			/// <summary>The level of alpha used by the renderer whilst fading out the message</summary>
			internal double RendererAlpha;
			/// <summary>Whether this message is queued for removal</summary>
			/// NOTE: May be called either by a track based beacon or by timeout
			internal bool QueueForRemoval;
			/// <summary>The function called when the message is added</summary>
			internal abstract void AddMessage();
			/// <summary>Called once a frame to update the message</summary>
			internal abstract void Update();
		}	
	}
}
