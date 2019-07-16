using LibRender;
using OpenBveApi.Colors;

namespace OpenBve.RouteManager
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
			this.MessageToDisplay = text;
			this.Timeout = double.PositiveInfinity;
			this.TriggerOnce = false;
			this.Direction = MessageDirection.Both;
			this.Color = MessageColor.White;
			this.RendererAlpha = 1.0;
		}

		/// <summary>Creates a marker text</summary>
		/// <param name="text">The text to be displayed</param>
		/// <param name="Color">The color of the text</param>
		public MarkerText(string text, MessageColor Color)
		{
			this.MessageToDisplay = text;
			this.Timeout = double.PositiveInfinity;
			this.TriggerOnce = false;
			this.Direction = MessageDirection.Both;
			this.Color = Color;
			this.RendererAlpha = 1.0;
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
