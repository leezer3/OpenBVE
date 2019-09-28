using System;
using System.Drawing;
using LibRender2.Texts;
using OpenBveApi.Colors;

namespace RouteManager2.MessageManager.MessageTypes
{
	/// <summary>Defines a textual message to be displayed in-game</summary>
	public class GeneralMessage : AbstractMessage, IDisposable
	{
		/// <summary>The message text to be displayed if early</summary>
		public string MessageEarlyText;

		/// <summary>The message text to be displayed if on-time</summary>
		public string MessageOnTimeText;

		/// <summary>The message text to be displayed if late</summary>
		public string MessageLateText;

		/// <summary>Defines the color of the message</summary>
		public MessageColor MessageEarlyColor;

		public MessageColor MessageColor;

		/// <summary>Defines the color of the message</summary>
		public MessageColor MessageLateColor;

		/// <summary>The font used for this message</summary>
		public OpenGlFont Font;

		public double MessageEarlyTime;

		public double MessageLateTime;

		/// <summary>Creates a general textual message</summary>
		public GeneralMessage()
		{
			Timeout = double.PositiveInfinity;
			TriggerOnce = true;
			Direction = MessageDirection.Forwards;
			MessageColor = MessageColor.White;
			MessageEarlyColor = MessageColor.White;
			MessageLateColor = MessageColor.White;
			Font = new OpenGlFont(FontFamily.GenericSansSerif, 12.0f);
		}

		public override void AddMessage(double currentTime)
		{
			if (TriggerOnce && Triggered)
			{
				return;
			}

			Triggered = true;

			if (currentTime <= MessageEarlyTime)
			{
				//We are early
				if (MessageEarlyText == null)
				{
					QueueForRemoval = true;
					return;
				}

				MessageToDisplay = MessageEarlyText;
				Color = MessageEarlyColor;
			}
			else if (currentTime >= MessageLateTime)
			{
				//Late
				if (MessageLateText == null)
				{
					QueueForRemoval = true;
					return;
				}

				MessageToDisplay = MessageLateText;
				Color = MessageLateColor;

			}
			else
			{
				//On time
				if (MessageOnTimeText == null)
				{
					QueueForRemoval = true;
					return;
				}

				MessageToDisplay = MessageOnTimeText;
				Color = MessageColor;

			}

			if (Timeout != double.PositiveInfinity)
			{
				Timeout += currentTime;
			}

			QueueForRemoval = false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool currentlyDisposing)
		{
			if (currentlyDisposing)
			{
				Font.Dispose();
			}
		}
	}
}
