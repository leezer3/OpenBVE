using OpenBveApi.Textures;

namespace OpenBve.RouteManager
{
	/// <summary>Defines an image based message to be displayed in-game</summary>
	public class TextureMessage : AbstractMessage
	{
		/// <summary>The message texture to be displayed if early</summary>
		public Texture MessageEarlyTexture;
		/// <summary>The message texture to be displayed if on-time </summary>
		public Texture MessageOnTimeTexture;
		/// <summary>The message text to be displayed if late</summary>
		public Texture MessageLateTexture;

		public double MessageEarlyTime;

		public double MessageLateTime;


		/// <summary>Holds the current texture</summary>
		/// NOTE: Not worth creating an enum just for this...
		private int currentTexture;

		public TextureMessage()
		{
			this.Timeout = 10000;
			this.TriggerOnce = true;
			this.Direction = MessageDirection.Forwards;
		}

		public override void AddMessage(double currentTime)
		{
			QueueForRemoval = false;
			if (TriggerOnce && Triggered)
			{
				return;
			}
			Triggered = true;
			if (currentTime <= MessageEarlyTime)
			{
				//We are early
				LibRender.Renderer.AddMarker(MessageEarlyTexture);
				currentTexture = 0;
			}
			else if (currentTime >= MessageLateTime)
			{
				//Late
				LibRender.Renderer.AddMarker(MessageLateTexture);
				currentTexture = 2;
			}
			else
			{
				//On time
				LibRender.Renderer.AddMarker(MessageOnTimeTexture);
				currentTexture = 1;
			}
		}

		public override void Update()
		{
			if (QueueForRemoval == true)
			{
				switch (currentTexture)
				{
					case 0:
						LibRender.Renderer.RemoveMarker(this.MessageEarlyTexture);
						break;
					case 1:
						LibRender.Renderer.RemoveMarker(this.MessageOnTimeTexture);
						break;
					case 2:
						LibRender.Renderer.RemoveMarker(this.MessageLateTexture);
						break;
				}
			}
		}
	}
}
