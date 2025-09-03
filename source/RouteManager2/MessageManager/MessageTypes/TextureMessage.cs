using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace RouteManager2.MessageManager.MessageTypes
{
	/// <summary>Defines an image based message to be displayed in-game</summary>
	public class TextureMessage : AbstractMessage
	{
		private readonly HostInterface currentHost;

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
		
		/// <summary>The size</summary>
		public Vector2 Size = Vector2.Null;

		public TextureMessage(HostInterface Host)
		{
			currentHost = Host;
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
				currentHost.AddMarker(MessageEarlyTexture, Size);
				currentTexture = 0;
			}
			else if (currentTime >= MessageLateTime)
			{
				//Late
				currentHost.AddMarker(MessageLateTexture, Size);
				currentTexture = 2;
			}
			else
			{
				//On time
				currentHost.AddMarker(MessageOnTimeTexture, Size);
				currentTexture = 1;
			}
		}

		public override void Update(double timeElapsed)
		{
			Timeout -= timeElapsed;
			if (QueueForRemoval)
			{
				switch (currentTexture)
				{
					case 0:
						currentHost.RemoveMarker(MessageEarlyTexture);
						break;
					case 1:
						currentHost.RemoveMarker(MessageOnTimeTexture);
						break;
					case 2:
						currentHost.RemoveMarker(MessageLateTexture);
						break;
				}
			}
		}
	}
}
