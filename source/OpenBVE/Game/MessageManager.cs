using System.Collections.Generic;

namespace OpenBve
{
	partial class MessageManager
	{
		/// <summary>Contains the current textual messages</summary>
		internal static List<Message> TextualMessages = new List<Message>();
		/// <summary>Contains the current image based messages</summary>
		internal static List<Message> ImageMessages = new List<Message>();

		/// <summary>Defines the directions in which a message may trigger</summary>
		internal enum MessageDirection
		{
			Forwards = 0,
			Backwards = 1,
			Both = 2
		}

		/// <summary>Adds a message to the in-game display</summary>
		/// <param name="message">The message to add</param>
		internal static void AddMessage(Message message)
		{
			if (message is MarkerImage || message is TextureMessage)
			{
				for (int i = 0; i < ImageMessages.Count; i++)
				{
					if (ImageMessages[i] == message)
					{
						return;
					}
				}
				message.AddMessage();
				ImageMessages.Add(message);
				return;
			}
			var m = message as GameMessage;
			for (int i = 0; i < TextualMessages.Count; i++)
			{
				var c = TextualMessages[i] as GameMessage;
				if (m != null && c != null)
				{
					if (m.Depencency == c.Depencency)
					{
						switch (m.Depencency)
						{
							case MessageDependency.None:
							case MessageDependency.Plugin:
								//Continue down the logic tree, multiple messages allowed
								break;
							case MessageDependency.PointOfInterest:
							case MessageDependency.StationArrival:
							case MessageDependency.CameraView:
								//Show only the latest message of these types
								c.QueueForRemoval = true;
								break;
							default:
								//Show only the current message for all other types
								return;

						}
					}
				}
				else
				{
					if (TextualMessages[i] == message)
					{
						//Other messages display one per message
						return;
					}
				}

			}
			//Prep: May be needed to trigger text replacement etc.
			message.AddMessage();
			if (m != null)
			{
				//This is a game-message: Add to the end of the list
				TextualMessages.Add(message);
			}
			else
			{
				//Add to the start of the list
				TextualMessages.Insert(0, message);
			}
		}

		/// <summary>Updates all current messages</summary>
		internal static void Update()
		{
			for (int i = TextualMessages.Count -1; i >= 0; i--)
			{
				TextualMessages[i].Update();
				if (TextualMessages[i].QueueForRemoval)
				{
					TextualMessages.RemoveAt(i);
				}
			}
			for (int i = ImageMessages.Count - 1; i >= 0; i--)
			{
				ImageMessages[i].Update();
				if (ImageMessages[i].QueueForRemoval)
				{
					ImageMessages.RemoveAt(i);
				}
			}
		}
	}
}
