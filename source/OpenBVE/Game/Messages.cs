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
		internal enum MessageDependency
		{
			None = 0,
			RouteLimit = 1,
			SectionLimit = 2,
			Station = 3
		}

		/// <summary>Defines an in-game message</summary>
		internal struct Message
		{
			/// <summary>The internal (non-translated) string</summary>
			internal string InternalText;
			/// <summary>The translated message string to be displayed</summary>
			internal string DisplayText;
			/// <summary>The action which triggered this message (route, speed limit etc.)</summary>
			internal MessageDependency Depencency;
			/// <summary>The time in seconds after midnight at which this message will time out, and be removed</summary>
			internal double Timeout;
			/// <summary>The color which this message is displayed in</summary>
			internal MessageColor Color;
			/// <summary>The on-screen position at which this message is displayed</summary>
			internal Vector2 RendererPosition;
			/// <summary>The level of alpha used by the renderer whilst fading out the message</summary>
			internal double RendererAlpha;
		}

		/// <summary>The current in-game messages</summary>
		internal static Message[] Messages = new Message[] { };
		/// <summary>The current size of the plane upon which messages are rendered</summary>
		internal static Vector2 MessagesRendererSize = new Vector2(16.0, 16.0);

		/// <summary>Adds a message to the in-game interface render queue</summary>
		/// <param name="Text">The text of the message</param>
		/// <param name="Depencency"></param>
		/// <param name="Mode"></param>
		/// <param name="Color">The color of the message text</param>
		/// <param name="Timeout">The time this message will display for</param>
		internal static void AddMessage(string Text, MessageDependency Depencency, Interface.GameMode Mode, MessageColor Color, double Timeout)
		{
			if (Interface.CurrentOptions.GameMode <= Mode)
			{
				if (Depencency == MessageDependency.RouteLimit | Depencency == MessageDependency.SectionLimit)
				{
					for (int i = 0; i < Messages.Length; i++)
					{
						if (Messages[i].Depencency == Depencency) return;
					}
				}
				int n = Messages.Length;
				Array.Resize<Message>(ref Messages, n + 1);
				Messages[n].InternalText = Text;
				Messages[n].DisplayText = "";
				Messages[n].Depencency = Depencency;
				Messages[n].Timeout = Timeout;
				Messages[n].Color = Color;
				Messages[n].RendererPosition = new Vector2(0.0, 0.0);
				Messages[n].RendererAlpha = 0.0;
			}
		}
		internal static void AddDebugMessage(string text, double duration)
		{
			Game.AddMessage(text, Game.MessageDependency.None, Interface.GameMode.Expert, MessageColor.Magenta, Game.SecondsSinceMidnight + duration);
		}

		/// <summary>The number 1km/h must be multiplied by to produce your desired speed units, or 0.0 to disable this</summary>
		internal static double SpeedConversionFactor = 0.0;
		/// <summary>The unit of speed displayed in in-game messages</summary>
		internal static string UnitOfSpeed = "km/h";

		internal static void UpdateMessages()
		{
			for (int i = 0; i < Messages.Length; i++)
			{
				bool remove = SecondsSinceMidnight >= Messages[i].Timeout;
				switch (Messages[i].Depencency)
				{
					case MessageDependency.RouteLimit:
						{
							double spd = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed);
							double lim = TrainManager.PlayerTrain.CurrentRouteLimit;
							//Get the speed and limit in km/h
							spd = Math.Round(spd * 3.6);
							lim = Math.Round(lim * 3.6);
							remove = spd <= lim;
							string s = Messages[i].InternalText, t;
							if (SpeedConversionFactor != 0.0)
							{
								spd = Math.Round(spd * SpeedConversionFactor);
								lim = Math.Round(lim * SpeedConversionFactor);
							}
							t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[speed]", t);
							t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[limit]", t);
							s = s.Replace("[unit]", UnitOfSpeed);
							Messages[i].DisplayText = s;
						} break;
					case MessageDependency.SectionLimit:
						{
							double spd = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed);
							double lim = TrainManager.PlayerTrain.CurrentSectionLimit;
							spd = Math.Round(spd * 3.6);
							lim = Math.Round(lim * 3.6);
							remove = spd <= lim;
							string s = Messages[i].InternalText, t;
							if (SpeedConversionFactor != 0.0)
							{
								spd = Math.Round(spd * SpeedConversionFactor);
								lim = Math.Round(lim * SpeedConversionFactor);
							}
							t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[speed]", t);
							t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[limit]", t);
							s = s.Replace("[unit]", UnitOfSpeed);
							Messages[i].DisplayText = s;
						} break;
					case MessageDependency.Station:
						{
							int j = TrainManager.PlayerTrain.Station;
							if (j >= 0 & TrainManager.PlayerTrain.StationState != TrainManager.TrainStopState.Completed)
							{
								double d = TrainManager.PlayerTrain.StationDepartureTime - SecondsSinceMidnight + 1.0;
								if (d < 0.0) d = 0.0;
								string s = Messages[i].InternalText;
								TimeSpan a = TimeSpan.FromSeconds(d);
								System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
								string t = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
								s = s.Replace("[time]", t);
								s = s.Replace("[name]", Stations[j].Name);
								Messages[i].DisplayText = s;
								if (d > 0.0) remove = false;
							}
							else
							{
								remove = true;
							}
						} break;
					default:
						Messages[i].DisplayText = Messages[i].InternalText;
						break;
				}
				if (remove)
				{
					if (Messages[i].Timeout == double.PositiveInfinity)
					{
						Messages[i].Timeout = SecondsSinceMidnight - 1.0;
					}
					if (SecondsSinceMidnight >= Messages[i].Timeout & Messages[i].RendererAlpha == 0.0)
					{
						for (int j = i; j < Messages.Length - 1; j++)
						{
							Messages[j] = Messages[j + 1];
						}
						i--;
						Array.Resize<Message>(ref Messages, Messages.Length - 1);
					}
				}
			}
		}
	}
}
