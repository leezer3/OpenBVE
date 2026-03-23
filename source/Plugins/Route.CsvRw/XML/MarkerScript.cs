using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using RouteManager2.MessageManager;
using RouteManager2.MessageManager.MessageTypes;
using System;

namespace CsvRwRouteParser
{
	internal class MarkerScriptParser
	{
		public static bool ReadMarkerXML(string fileName, double StartingPosition, out Marker marker)
		{
			marker = null;
			double EndingPosition = double.PositiveInfinity;
			AbstractMessage Message = null;
			XMLFile<MarkerScriptSection, MarkerScriptKey> xmlFile = new XMLFile<MarkerScriptSection, MarkerScriptKey>(fileName, "/openBVE", Plugin.CurrentHost);

			bool EarlyDefined = false, LateDefined = false;
			string EarlyText = null, Text = null, LateText = null;
			string[] Trains = null;
			Texture EarlyTexture = null, Texture = null, LateTexture = null;
			double EarlyTime = 0.0, LateTime = 0.0, TimeOut = double.PositiveInfinity;
			MessageColor EarlyColor = MessageColor.White, OnTimeColor = MessageColor.White, LateColor = MessageColor.White;
			Vector2 messageSize = Vector2.Null;

			while (xmlFile.RemainingSubBlocks > 0)
			{
				xmlFile.ReadBlock(new[] { MarkerScriptSection.TextMarker, MarkerScriptSection.ImageMarker }, out Block<MarkerScriptSection, MarkerScriptKey> markerBlock);
				while (markerBlock.RemainingSubBlocks > 0)
				{
					Block<MarkerScriptSection, MarkerScriptKey> subBlock = markerBlock.ReadNextBlock();
					switch (subBlock.Key)
					{
						case MarkerScriptSection.Early:
							subBlock.TryGetValue(MarkerScriptKey.Text, ref EarlyText);
							if (subBlock.GetPath(MarkerScriptKey.Texture, Path.GetDirectoryName(fileName), out string textureFile))
							{
								if (!Plugin.CurrentHost.RegisterTexture(textureFile, TextureParameters.NoChange, out EarlyTexture))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loading MessageEarlyTexture " + textureFile + " failed.");
								}
							}

							if (subBlock.TryGetTime(MarkerScriptKey.Time, ref EarlyTime))
							{
								EarlyDefined = true;
							}

							if (subBlock.GetValue(MarkerScriptKey.Color, out string color))
							{
								EarlyColor = ParseColor(color, fileName);
							}
							break;
						case MarkerScriptSection.OnTime:
							subBlock.TryGetValue(MarkerScriptKey.Text, ref Text);
							if (subBlock.GetPath(MarkerScriptKey.Texture, Path.GetDirectoryName(fileName), out textureFile))
							{
								if (!Plugin.CurrentHost.RegisterTexture(textureFile, TextureParameters.NoChange, out Texture))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loading MessageTexture " + textureFile + " failed.");
								}
							}

							if (subBlock.GetValue(MarkerScriptKey.Color, out color))
							{
								OnTimeColor = ParseColor(color, fileName);
							}
							break;
						case MarkerScriptSection.Late:
							subBlock.TryGetValue(MarkerScriptKey.Text, ref LateText);
							if (subBlock.GetPath(MarkerScriptKey.Texture, Path.GetDirectoryName(fileName), out textureFile))
							{
								if (!Plugin.CurrentHost.RegisterTexture(textureFile, TextureParameters.NoChange, out LateTexture))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loading MessageLateTexture " + textureFile + " failed.");
								}
							}

							if (subBlock.TryGetTime(MarkerScriptKey.Time, ref LateTime))
							{
								LateDefined = true;
							}

							if (subBlock.GetValue(MarkerScriptKey.Color, out color))
							{
								LateColor = ParseColor(color, fileName);
							}
							break;
					}

					markerBlock.TryGetValue(MarkerScriptKey.Timeout, ref TimeOut);
					markerBlock.TryGetValue(MarkerScriptKey.Distance, ref EndingPosition);
					markerBlock.TryGetStringArray(MarkerScriptKey.Trains, ',', ref Trains);
					if (markerBlock.Key == MarkerScriptSection.ImageMarker)
					{
						markerBlock.TryGetVector2(MarkerScriptKey.Size, ',', ref messageSize);
					}

					//Check this marker is valid
					if (TimeOut == Double.PositiveInfinity && EndingPosition == Double.PositiveInfinity)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "No marker timeout or distance defined in marker XML " + fileName);
						return false;
					}
					if (EndingPosition != Double.PositiveInfinity)
					{
						if (Math.Abs(EndingPosition) == EndingPosition)
						{
							//Positive
							EndingPosition = StartingPosition + EndingPosition;
						}
						else
						{
							//Negative
							EndingPosition = StartingPosition;
							StartingPosition -= EndingPosition;
						}
					}
					TextureMessage t = new TextureMessage(Plugin.CurrentHost);
					GeneralMessage m = new GeneralMessage();
					//Add variants

					if (EarlyDefined)
					{
						if (markerBlock.Key == MarkerScriptSection.ImageMarker)
						{
							if (EarlyTexture != null)
							{
								t.MessageEarlyTime = EarlyTime;
								t.MessageEarlyTexture = EarlyTexture;
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "An early time was defined, but no message was specified in MarkerXML " + fileName);
							}

						}
						else
						{
							if (EarlyText != null)
							{
								m.MessageEarlyTime = EarlyTime;
								m.MessageEarlyText = EarlyText;
								m.MessageEarlyColor = EarlyColor;
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "An early time was defined, but no message was specified in MarkerXML " + fileName);
							}
						}
					}
					if (LateDefined)
					{
						if (markerBlock.Key == MarkerScriptSection.ImageMarker)
						{
							if (LateTexture != null)
							{
								t.MessageLateTime = LateTime;
								t.MessageLateTexture = LateTexture;
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "A late time was defined, but no message was specified in MarkerXML " + fileName);
							}

						}
						else
						{
							if (LateText != null)
							{
								m.MessageLateTime = LateTime;
								m.MessageLateText = LateText;
								m.MessageLateColor = LateColor;
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "An early time was defined, but no message was specified in MarkerXML " + fileName);
							}
						}
					}
					//Final on-time message
					if (markerBlock.Key == MarkerScriptSection.ImageMarker)
					{
						t.Trains = Trains;
						if (Texture != null)
						{
							t.MessageOnTimeTexture = Texture;
						}
					}
					else
					{
						m.Trains = Trains;
						if (Text != null)
						{
							m.MessageOnTimeText = Text;
							m.Color = OnTimeColor;
						}
					}
					if (markerBlock.Key == MarkerScriptSection.ImageMarker)
					{
						t.Size = messageSize;
						Message = t;
					}
					else
					{
						Message = m;
					}
				}
			}
			marker = new Marker(StartingPosition, EndingPosition, Message);
			return true;
		}

		/// <summary>Parses a color string into a message color</summary>
		/// <param name="s">The string to parse</param>
		/// <param name="f">The filename (Use in errors)</param>
		/// <returns></returns>
		private static MessageColor ParseColor(string s, string f)
		{
			switch (s.ToLowerInvariant())
			{
				case "black":
				case "1":
					return MessageColor.Black;
				case "gray":
				case "2":
					return MessageColor.Gray;
				case "white":
				case "3":
					return MessageColor.White;
				case "red":
				case "4":
					return MessageColor.Red;
				case "orange":
				case "5":
					return MessageColor.Orange;
				case "green":
				case "6":
					return MessageColor.Green;
				case "blue":
				case "7":
					return MessageColor.Blue;
				case "magenta":
				case "8":
					return MessageColor.Magenta;
				default:
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageColor is invalid in MarkerXML " + f);
					return MessageColor.White;
			}
		}
	}

	

}
