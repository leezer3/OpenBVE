using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;

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
		internal static void AddMessage(string Text, MessageManager.MessageDependency Depencency, Interface.GameMode Mode, MessageColor Color, double Timeout, string key)
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


		/// <summary>The number 1km/h must be multiplied by to produce your desired speed units, or 0.0 to disable this</summary>
		internal static double SpeedConversionFactor = 0.0;
		/// <summary>The unit of speed displayed in in-game messages</summary>
		internal static string UnitOfSpeed = "km/h";

		/// <summary>Called once a frame to update the messages displayed on-screen</summary>
		internal static void UpdateMessages()
		{
			MessageManager.Update();
			
		}
	}
}
