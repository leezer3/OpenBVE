using System;
using System.Xml;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi.Textures;
using OpenBveApi.Interface;

namespace OpenBve
{
	class MarkerScriptParser
	{
		public static bool ReadMarkerXML(string fileName, ref CsvRwRouteParser.Marker marker)
		{
			
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
			string Path = System.IO.Path.GetDirectoryName(fileName);
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
					Interface.AddMessage(MessageType.Error, false, "No marker nodes defined in XML file " + fileName);
					return false;
				}
				//marker = new CsvRwRouteParser.Marker();
				foreach (XmlNode n in DocumentNodes)
				{
					if (n.ChildNodes.OfType<XmlElement>().Any())
					{
						bool EarlyDefined = false, LateDefined = false;
						string EarlyText = null, Text = null, LateText = null;
						string[] Trains = null;
						Texture EarlyTexture = null, Texture = null, LateTexture = null;
						double EarlyTime = 0.0, LateTime = 0.0, TimeOut = Double.PositiveInfinity, EndingPosition = Double.PositiveInfinity;
						OpenBveApi.Colors.MessageColor EarlyColor = MessageColor.White, OnTimeColor = MessageColor.White, LateColor = MessageColor.White;
						foreach (XmlNode c in n.ChildNodes)
						{
							switch (c.Name.ToLowerInvariant())
							{
								case "early":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false,
											"No paramaters defined for the early message in " + fileName);
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
													f = OpenBveApi.Path.CombineFile(Path, cc.InnerText);
												}
												catch
												{
													Interface.AddMessage(MessageType.Error, false, "MessageEarlyTexture path was malformed in file " + fileName);
													break;
												}
												if (System.IO.File.Exists(f))
												{
													if (!Textures.RegisterTexture(f, out EarlyTexture))
													{
														Interface.AddMessage(MessageType.Error, false, "Loading MessageEarlyTexture " + f + " failed.");
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "MessageEarlyTexture " + f + " does not exist.");
												}
												break;
											case "time":
												if (!Interface.TryParseTime(cc.InnerText, out EarlyTime))
												{
													Interface.AddMessage(MessageType.Error, false, "Early message time invalid in " + fileName);
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
										Interface.AddMessage(MessageType.Error, false,
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
													f = OpenBveApi.Path.CombineFile(Path, cc.InnerText);
												}
												catch
												{
													Interface.AddMessage(MessageType.Error, false, "MessageTexture path was malformed in file " + fileName);
													break;
												}
												if (System.IO.File.Exists(f))
												{
													if (!Textures.RegisterTexture(f, out Texture))
													{
														Interface.AddMessage(MessageType.Error, false, "Loading MessageTexture " + f + " failed.");
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "MessageTexture " + f + " does not exist.");
												}
												break;
											case "color":
												OnTimeColor = ParseColor(cc.InnerText, fileName);
												break;
											case "time":
												Interface.AddMessage(MessageType.Error, false, "OnTime should not contain a TIME declaration in " + fileName);
												break;
										}
									}
									break;
								case "late":
									if (!c.ChildNodes.OfType<XmlElement>().Any())
									{
										Interface.AddMessage(MessageType.Error, false,
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
													f = OpenBveApi.Path.CombineFile(Path, cc.InnerText);
												}
												catch
												{
													Interface.AddMessage(MessageType.Error, false, "MessageLateTexture path was malformed in file " + fileName);
													break;
												}
												if (System.IO.File.Exists(f))
												{
													if (!Textures.RegisterTexture(f, out LateTexture))
													{
														Interface.AddMessage(MessageType.Error, false, "Loading MessageLateTexture " + f + " failed.");
													}
												}
												else
												{
													Interface.AddMessage(MessageType.Error, false, "MessageLateTexture " + f + " does not exist.");
												}
												break;
											case "time":
												if (!Interface.TryParseTime(cc.InnerText, out LateTime))
												{
													Interface.AddMessage(MessageType.Error, false, "Early message time invalid in " + fileName);
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
										Interface.AddMessage(MessageType.Error, false, "Marker timeout invalid in " + fileName);
									}
									break;
								case "distance":
									if (!NumberFormats.TryParseDouble(c.InnerText, new[] {1.0}, out EndingPosition))
									{
										Interface.AddMessage(MessageType.Error, false, "Marker distance invalid in " + fileName);
									}
									break;
								case "trains":
									Trains = c.InnerText.Split(';');
									break;
							}

						}
						//Check this marker is valid
						if (TimeOut == Double.PositiveInfinity && EndingPosition == Double.PositiveInfinity)
						{
							Interface.AddMessage(MessageType.Error, false, "No marker timeout or distance defined in marker XML " + fileName);
							return false;
						}
						if (EndingPosition != Double.PositiveInfinity)
						{
							if (Math.Abs(EndingPosition) == EndingPosition)
							{
								//Positive
								marker.EndingPosition = marker.StartingPosition + EndingPosition;
							}
							else
							{
								//Negative
								marker.EndingPosition = marker.StartingPosition;
								marker.StartingPosition -= EndingPosition;
							}
						}
						MessageManager.TextureMessage t = new MessageManager.TextureMessage();
						MessageManager.GeneralMessage m = new MessageManager.GeneralMessage();
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
									Interface.AddMessage(MessageType.Warning, false, "An early time was defined, but no message was specified in MarkerXML " + fileName);
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
									Interface.AddMessage(MessageType.Warning, false, "An early time was defined, but no message was specified in MarkerXML " + fileName);
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
									Interface.AddMessage(MessageType.Warning, false, "A late time was defined, but no message was specified in MarkerXML " + fileName);
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
									Interface.AddMessage(MessageType.Warning, false, "An early time was defined, but no message was specified in MarkerXML " + fileName);
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
							marker.Message = t;
						}
						else
						{
							marker.Message = m;
						}
					}
				}
				
			}
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
					Interface.AddMessage(MessageType.Error, false, "MessageColor is invalid in MarkerXML " + f);
					return MessageColor.White;
			}
		}
	}

	

}
