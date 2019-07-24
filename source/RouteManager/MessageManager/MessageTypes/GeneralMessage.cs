using System;
using System.Drawing;
using LibRender;
using OpenBveApi.Colors;

namespace OpenBve.RouteManager
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
				this.Timeout = double.PositiveInfinity;
				this.TriggerOnce = true;
				this.Direction = MessageDirection.Forwards;
				this.MessageColor = MessageColor.White;
				this.MessageEarlyColor = MessageColor.White;
				this.MessageLateColor = MessageColor.White;
				this.Font = new OpenGlFont(FontFamily.GenericSansSerif, 12.0f);
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
				if (this.Timeout != double.PositiveInfinity)
				{
					this.Timeout += currentTime;
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
			if(currentlyDisposing)
			{
				Font.Dispose();
			}
		}
		}
}
