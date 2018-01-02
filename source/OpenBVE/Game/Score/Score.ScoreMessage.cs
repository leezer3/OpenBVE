using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Holds all current score messages to be rendered</summary>
		internal static ScoreMessage[] ScoreMessages = new ScoreMessage[] { };
		/// <summary>Holds the current on-screen size in px of the area occupied by score messages</summary>
		internal static Vector2 ScoreMessagesRendererSize = new Vector2(16.0, 16.0);

		/// <summary>An on-screen score message to be displayed by the renderer</summary>
		internal struct ScoreMessage
		{
			/// <summary>The numeric value of this score message</summary>
			internal int Value;
			/// <summary>The translated text to display</summary>
			internal string Text;
			/// <summary>The timeout for this message to be removed</summary>
			internal double Timeout;
			/// <summary>The text color for this message</summary>
			internal MessageColor Color;
			/// <summary>The on-screen position of this message</summary>
			internal Vector2 RendererPosition;
			/// <summary>The current alpha value (Used when fading the message out)</summary>
			internal double RendererAlpha;
		}
	}
}
