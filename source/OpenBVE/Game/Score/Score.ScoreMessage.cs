using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>An on-screen score message to be displayed by the renderer</summary>
	internal class ScoreMessage
	{
		/// <summary>The numeric value of this score message</summary>
		internal readonly int Value;
		/// <summary>The translated text to display</summary>
		internal readonly string Text;
		/// <summary>The timeout for this message to be removed</summary>
		internal double Timeout;
		/// <summary>The text color for this message</summary>
		internal readonly MessageColor Color;
		/// <summary>The on-screen position of this message</summary>
		internal Vector2 RendererPosition;
		/// <summary>The current alpha value (Used when fading the message out)</summary>
		internal double RendererAlpha;

		internal ScoreMessage(int value, Game.ScoreTextToken textToken, double timeout)
		{
			Value = value;
			if (Value < 0.0)
			{
				Color = MessageColor.Red;
			}
			else if (Value > 0.0)
			{
				Color = MessageColor.Green;
			}
			else
			{
				Color = MessageColor.White;
			}
			Text = Interface.GetScoreText(textToken) + ": " + Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			Timeout = timeout;
			RendererPosition = new Vector2(0, 0);
			RendererAlpha = 0;
		}

		internal ScoreMessage(int value, string text, MessageColor color, double timeout)
		{
			Value = value;
			Color = color;
			Text = text.Length != 0 ? text : "══════════";
			Timeout = timeout;
			RendererPosition = new Vector2(0, 0);
			RendererAlpha = 0;
		}
	}
}
