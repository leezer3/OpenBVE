using System;
using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using RouteManager2.MessageManager.MessageTypes;

namespace OpenBve
{
	partial class MessageManager
	{
		/// <summary>Contains the current textual messages</summary>
		internal static readonly List<AbstractMessage> TextualMessages = new List<AbstractMessage>();
		/// <summary>Contains the current image based messages</summary>
		internal static readonly List<AbstractMessage> ImageMessages = new List<AbstractMessage>();
		
		/// <summary>Adds a message to the in-game interface render queue</summary>
		/// <param name="Text">The text of the message</param>
		/// <param name="Depencency"></param>
		/// <param name="Mode"></param>
		/// <param name="Color">The color of the message text</param>
		/// <param name="Timeout">The time this message will display for</param>
		/// <param name="key">The textual key identifiying this message</param>
		internal static void AddMessage(string Text, MessageDependency Depencency, GameMode Mode, MessageColor Color, double Timeout, string key)
		{
			if (Interface.CurrentOptions.GameMode <= Mode)
			{
				MessageManager.GameMessage message = new MessageManager.GameMessage
				{
					InternalText = Text,
					MessageToDisplay = String.Empty,
					Depencency = Depencency,
					Color = Color,
					Timeout = Timeout,
					Key = key
				};
				MessageManager.AddMessage(message);
			}
		}

		/// <summary>Adds a message to the in-game display</summary>
		/// <param name="message">The message to add</param>
		internal static void AddMessage(AbstractMessage message)
		{
			if (TrainManager.PlayerTrain.StationState == TrainStopState.Jumping || message.Mode < Interface.CurrentOptions.GameMode)
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
				message.AddMessage(Program.CurrentRoute.SecondsSinceMidnight);
				ImageMessages.Add(message);
				return;
			}
			var m = message as GameMessage;
			if (m != null && m.Depencency == MessageDependency.PassedRedSignal)
			{
				for (int i = TextualMessages.Count -1; i >= 0; i--)
				{
					var c = TextualMessages[i] as GameMessage;
					if (c != null && (c.Depencency == MessageDependency.SectionLimit || c.Depencency == MessageDependency.RouteLimit))
					{
						TextualMessages.RemoveAt(i);
					}
				}
			}
			for (int i = 0; i < TextualMessages.Count; i++)
			{
				var c = TextualMessages[i] as GameMessage;
				if (m != null && c != null)
				{
					if ((m.Depencency == MessageDependency.SectionLimit || m.Depencency == MessageDependency.RouteLimit) && c.Depencency == MessageDependency.PassedRedSignal)
					{
						//Don't add a new section limit message whilst the passed red signal message is active
						return;
					}
					if (m.Depencency == c.Depencency)
					{
						switch (m.Depencency)
						{
							case MessageDependency.None:
							case MessageDependency.Plugin:
								//Continue down the logic tree, multiple messages allowed
								break;
							case MessageDependency.SectionLimit:
							case MessageDependency.RouteLimit:
							case MessageDependency.PointOfInterest:
							case MessageDependency.StationArrival:
							case MessageDependency.CameraView:
							case MessageDependency.AccessibilityHelper:
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
			message.AddMessage(Program.CurrentRoute.SecondsSinceMidnight);
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
