using LibRender2;
using OpenBveApi.Textures;

namespace RouteManager2.MessageManager.MessageTypes
{
	/// <summary>Defines an image based message to be displayed in-game</summary>
	public class TextureMessage : AbstractMessage
	{
		private readonly BaseRenderer renderer;

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

		public TextureMessage(BaseRenderer Renderer)
		{
			renderer = Renderer;

			Timeout = 10000;
			TriggerOnce = true;
			Direction = MessageDirection.Forwards;
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
				renderer.Marker.AddMarker(MessageEarlyTexture);
				currentTexture = 0;
			}
			else if (currentTime >= MessageLateTime)
			{
				//Late
				renderer.Marker.AddMarker(MessageLateTexture);
				currentTexture = 2;
			}
			else
			{
				//On time
				renderer.Marker.AddMarker(MessageOnTimeTexture);
				currentTexture = 1;
			}
		}

		public override void Update()
		{
			if (QueueForRemoval)
			{
				switch (currentTexture)
				{
					case 0:
						renderer.Marker.RemoveMarker(MessageEarlyTexture);
						break;
					case 1:
						renderer.Marker.RemoveMarker(MessageOnTimeTexture);
						break;
					case 2:
						renderer.Marker.RemoveMarker(MessageLateTexture);
						break;
				}
			}
		}
	}
}
