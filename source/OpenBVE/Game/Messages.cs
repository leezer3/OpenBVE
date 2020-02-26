using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using RouteManager2.MessageManager;

namespace OpenBve
{
	/*
	 * Holds the base routines used to add or remove a message from the in-game display.
	 * NOTE: Messages are rendered to the screen in Graphics\Renderer\Overlays.cs
	 * 
	 */
	internal static partial class Game
	{
		/// <summary>The current size of the plane upon which messages are rendered</summary>
		internal static Vector2 MessagesRendererSize = new Vector2(16.0, 16.0);

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


		
	}
}
