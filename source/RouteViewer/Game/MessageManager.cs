using System.Collections.Generic;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using RouteManager2.MessageManager.MessageTypes;

namespace OpenBve
{
	/*
	 * NOTE: Cut-down version of the message manager for Route Viewer
	 *
	 */

	class MessageManager
	{
		/// <summary>Contains the current textual messages</summary>
		internal static readonly List<AbstractMessage> TextualMessages = new List<AbstractMessage>();
		/// <summary>Contains the current image based messages</summary>
		internal static readonly List<AbstractMessage> ImageMessages = new List<AbstractMessage>();
		
		/// <summary>Adds a message to the in-game display</summary>
		/// <param name="message">The message to add</param>
		internal static void AddMessage(AbstractMessage message)
		{
			if (TrainManager.PlayerTrain.StationState == TrainStopState.Jumping)
			{
				//Ignore messages triggered during a jump
				return;
			}
			if (message is MarkerImage || message is TextureMessage)
			{
				for (int i = 0; i < ImageMessages.Count; i++)
				{
					if (ImageMessages[i] == message)
					{
						return;
					}
				}
				message.AddMessage(Game.SecondsSinceMidnight);
				ImageMessages.Add(message);
				return;
			}
			
			for (int i = 0; i < TextualMessages.Count; i++)
			{

				if (TextualMessages[i] == message)
				{
					//Other messages display one per message
					return;
				}


			}
			//Prep: May be needed to trigger text replacement etc.
			message.AddMessage(Game.SecondsSinceMidnight);
			//Add to the start of the list
			TextualMessages.Insert(0, message);
		}

		/// <summary>Updates all current messages</summary>
		internal static void UpdateMessages()
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
