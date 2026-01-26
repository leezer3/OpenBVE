using System;
using System.Xml;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using RouteManager2.MessageManager.MessageTypes;

namespace CsvRwRouteParser
{
	internal class MarkerScriptParser
	{
		public static bool ReadMarkerXML(string fileName, double StartingPosition, out Marker Marker)
		{
			Marker = null;
			double EndingPosition = Double.PositiveInfinity;
			AbstractMessage Message = null;
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			string filePath = Path.GetDirectoryName(fileName);
			if (currentXML.DocumentElement != null)
			{
				bool iM = false;
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/TextMarker");

				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/ImageMarker");
					iM = true;
				}
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "No marker nodes defined in XML file " + fileName);
					return false;
				}
				foreach (XmlNode n in DocumentNodes)
				{
					if (n.ChildNodes.OfType<XmlElement>().Any())
					{
						bool EarlyDefined = false, LateDefined = false;
						string EarlyText = null, Text = null, LateText = null;
						string[] Trains = null;
						Texture EarlyTexture = null, Texture = null, LateTexture = null;
						double EarlyTime = 0.0, LateTime = 0.0, TimeOut = double.PositiveInfinity;
						MessageColor EarlyColor = MessageColor.White, OnTimeColor = MessageColor.White, LateColor = MessageColor.White;
						Vector2 messageSize = Vector2.Null;
						foreach (XmlNode c in n.ChildNodes)
						{
							switch (c.Name.ToLowerInvariant())
							{
								case "early":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "No paramaters defined for the early message in " + fileName);
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "text":
												EarlyText = cc.InnerText;
												break;
											case "texture":
											case "image":
												string f;
												try
												{
													f = Path.CombineFile(filePath, cc.InnerText);
												}
												catch
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageEarlyTexture path was malformed in file " + fileName);
													break;
												}
												if (System.IO.File.Exists(f))
												{
													if (!Plugin.CurrentHost.RegisterTexture(f, TextureParameters.NoChange, out EarlyTexture))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loading MessageEarlyTexture " + f + " failed.");
													}
												}
												else
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageEarlyTexture " + f + " does not exist.");
												}
												break;
											case "time":
												if (!Parser.TryParseTime(cc.InnerText, out EarlyTime))
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Early message time invalid in " + fileName);
												}
												EarlyDefined = true;
												break;
											case "color":
												EarlyColor = ParseColor(cc.InnerText, fileName);
												break;
										}
									}
									break;
								case "ontime":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false,
											"No paramaters defined for the on-time message in " + fileName);
									}
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "text":
												Text = cc.InnerText;
												break;
											case "texture":
											case "image":
												string f;
												try
												{
													f = Path.CombineFile(filePath, cc.InnerText);
												}
												catch
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageTexture path was malformed in file " + fileName);
													break;
												}
												if (System.IO.File.Exists(f))
												{
													if (!Plugin.CurrentHost.RegisterTexture(f, TextureParameters.NoChange, out Texture))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loading MessageTexture " + f + " failed.");
													}
												}
												else
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageTexture " + f + " does not exist.");
												}
												break;
											case "color":
												OnTimeColor = ParseColor(cc.InnerText, fileName);
												break;
											case "time":
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "OnTime should not contain a TIME declaration in " + fileName);
												break;
										}
									}
									break;
								case "late":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false,
											"No paramaters defined for the late message in " + fileName);
									}
									
									foreach (XmlNode cc in c.ChildNodes)
									{
										switch (cc.Name.ToLowerInvariant())
										{
											case "text":
												LateText = cc.InnerText;
												break;
											case "texture":
											case "image":
												string f;
												try
												{
													f = Path.CombineFile(filePath, cc.InnerText);
												}
												catch
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageLateTexture path was malformed in file " + fileName);
													break;
												}
												if (System.IO.File.Exists(f))
												{
													if (!Plugin.CurrentHost.RegisterTexture(f, TextureParameters.NoChange, out LateTexture))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loading MessageLateTexture " + f + " failed.");
													}
												}
												else
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MessageLateTexture " + f + " does not exist.");
												}
												break;
											case "time":
												if (!Parser.TryParseTime(cc.InnerText, out LateTime))
												{
													Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Early message time invalid in " + fileName);
												}
												LateDefined = true;
												break;
											case "color":
												LateColor = ParseColor(cc.InnerText, fileName);
												break;
										}
									}
									break;
								case "timeout":
									if (!NumberFormats.TryParseDouble(c.InnerText, new[] {1.0}, out TimeOut))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Marker timeout invalid in " + fileName);
									}
									break;
								case "distance":
									if (!NumberFormats.TryParseDouble(c.InnerText, new[] {1.0}, out EndingPosition))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Marker distance invalid in " + fileName);
									}
									break;
								case "trains":
									Trains = c.InnerText.Split(';');
									break;
								case "size":
									if (iM)
									{
										string[] Arguments = c.InnerText.Split(',');
										if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out messageSize.X))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Message size X " + Arguments[0] + " is invalid.");
											messageSize.X = 0.0;
										}
										if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out messageSize.Y))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Message size X " + Arguments[1] + " is invalid.");
											messageSize.Y = 0.0;
										}
									}
									break;
							}

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
							if (iM)
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
							if (iM)
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
						if (iM)
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
						if (iM)
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
				
			}
			Marker = new Marker(StartingPosition, EndingPosition, Message);
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
