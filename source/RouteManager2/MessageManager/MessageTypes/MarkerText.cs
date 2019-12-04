using LibRender2.Texts;
using OpenBveApi.Colors;

namespace RouteManager2.MessageManager.MessageTypes
{
	/// <summary>Defines a marker text (e.g. bridge name) to be displayed in-game</summary>
	public class MarkerText : AbstractMessage
	{
		/// <summary>The font used for this message</summary>
		public OpenGlFont Font;

		/// <summary>Creates a marker text</summary>
		/// <param name="text">The text to be displayed</param>
		public MarkerText(string text)
		{
			MessageToDisplay = text;
			Timeout = double.PositiveInfinity;
			TriggerOnce = false;
			Direction = MessageDirection.Both;
			Color = MessageColor.White;
			RendererAlpha = 1.0;
		}

		/// <summary>Creates a marker text</summary>
		/// <param name="text">The text to be displayed</param>
		/// <param name="Color">The color of the text</param>
		public MarkerText(string text, MessageColor Color)
		{
			MessageToDisplay = text;
			Timeout = double.PositiveInfinity;
			TriggerOnce = false;
			Direction = MessageDirection.Both;
			this.Color = Color;
			RendererAlpha = 1.0;
		}

		public override void AddMessage(double currentTime)
		{
			QueueForRemoval = false;

			if (TriggerOnce && Triggered)
			{
				return;
			}

			Triggered = true;
		}
	}
}
