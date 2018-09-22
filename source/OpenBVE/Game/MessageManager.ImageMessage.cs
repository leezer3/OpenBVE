using OpenBveApi.Textures;

namespace OpenBve
{
	partial class MessageManager
	{
		/// <summary>Defines an image based message to be displayed in-game</summary>
		internal class TextureMessage : Message
		{
			/// <summary>The message texture to be displayed if early</summary>
			internal Texture MessageEarlyTexture;
			/// <summary>The message texture to be displayed if on-time </summary>
			internal Texture MessageOnTimeTexture;
			/// <summary>The message text to be displayed if late</summary>
			internal Texture MessageLateTexture;

			internal double MessageEarlyTime;

			internal double MessageLateTime;


			/// <summary>Holds the current texture</summary>
			/// NOTE: Not worth creating an enum just for this...
			private int currentTexture;

			internal TextureMessage()
			{
				this.Timeout = 10000;
				this.TriggerOnce = true;
				this.Direction = MessageDirection.Forwards;
			}

			internal override void AddMessage()
			{
				QueueForRemoval = false;
				ImageMessages.Add(this);
				if (TriggerOnce && Triggered)
				{
					return;
				}
				Triggered = true;
				if (Game.SecondsSinceMidnight <= MessageEarlyTime)
				{
					//We are early
					Game.AddMarker(MessageEarlyTexture);
					currentTexture = 0;
				}
				else if (Game.SecondsSinceMidnight >= MessageLateTime)
				{
					//Late
					Game.AddMarker(MessageLateTexture);
					currentTexture = 2;
				}
				else
				{
					//On time
					Game.AddMarker(MessageOnTimeTexture);
					currentTexture = 1;
				}
			}

			internal override void Update()
			{
				if (QueueForRemoval == true)
				{
					switch (currentTexture)
					{
						case 0:
							Game.RemoveMarker(this.MessageEarlyTexture);
							break;
						case 1:
							Game.RemoveMarker(this.MessageOnTimeTexture);
							break;
						case 2:
							Game.RemoveMarker(this.MessageLateTexture);
							break;
					}
				}
			}
		}

		/// <summary>Defines a legacy marker image, displayed between two points on a route</summary>
		internal class MarkerImage : Message
		{
			/// <summary>The texture to be displayed</summary>
			private readonly Texture Texture;

			internal MarkerImage(Texture texture)
			{
				this.Texture = texture;
			}

			internal override void AddMessage()
			{
				QueueForRemoval = false;
				ImageMessages.Add(this);
				Game.AddMarker(this.Texture);
			}

			internal override void Update()
			{
				if (QueueForRemoval == true)
				{
					Game.RemoveMarker(this.Texture);
				}
			}
		}
	}
}
