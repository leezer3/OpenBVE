using System;
using System.IO;
using System.Xml;
using OpenBveApi.Math;
using System.Linq;

namespace OpenBve
{
	class StationXMLParser
	{
		public static Game.Station ReadStationXML(string fileName, bool PreviewOnly, Textures.Texture[] daytimeTimetableTextures, Textures.Texture[] nighttimeTimetableTextures, int CurrentStation, ref bool passAlarm, ref CsvRwRouteParser.StopRequest stopRequest)
		{
			Game.Station station = new Game.Station
			{
				Stops = new Game.StationStop[] { }
			};
			stopRequest.Early = new TrackManager.RequestStop();
			stopRequest.OnTime = new TrackManager.RequestStop();
			stopRequest.Late = new TrackManager.RequestStop();
			stopRequest.OnTime.Probability = 75;
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			currentXML.Load(fileName);
			string Path = System.IO.Path.GetDirectoryName(fileName);
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Station");
				//Check this file actually contains OpenBVE light definition nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode c in n.ChildNodes)
							{

								//string[] Arguments = c.InnerText.Split(',');
								switch (c.Name.ToLowerInvariant())
								{
									case "name":
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											station.Name = c.InnerText;
										}
										else
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Station name was empty in XML file " + fileName);
										}
										break;
									case "arrivaltime":
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (!Interface.TryParseTime(c.InnerText, out station.ArrivalTime))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "Station arrival time was invalid in XML file " + fileName);
											}
										}
										break;
									case "departuretime":
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (!Interface.TryParseTime(c.InnerText, out station.DepartureTime))
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "Station arrival time was invalid in XML file " + fileName);
											}
										}
										break;
									case "type":
										switch (c.InnerText.ToLowerInvariant())
										{
											case "c":
											case "changeends":
												station.StationType = Game.StationType.ChangeEnds;
												break;
											case "t":
											case "terminal":
												station.StationType = Game.StationType.Terminal;
												break;
											default:
												station.StationType = Game.StationType.Normal;
												break;
										}
										break;
									case "passalarm":
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
											{
												passAlarm = true;
											}
											else
											{
												passAlarm = false;
											}
										}
										break;
									case "doors":
										int door = 0;
										bool doorboth = false;
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											switch (c.InnerText.ToLowerInvariant())
											{
												case "l":
												case "left":
													door = -1;
													break;
												case "r":
												case "right":
													door = 1;
													break;
												case "n":
												case "none":
												case "neither":
													door = 0;
													break;
												case "b":
												case "both":
													doorboth = true;
													break;
												default:
													if (!NumberFormats.TryParseIntVb6(c.InnerText, out door))
													{
														Interface.AddMessage(Interface.MessageType.Error, false, "Door side was invalid in XML file " + fileName);
														door = 0;
													}
													break;
											}
										}
										station.OpenLeftDoors = door < 0.0 | doorboth;
										station.OpenRightDoors = door > 0.0 | doorboth;
										break;
									case "forcedredsignal":
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
											{
												station.ForceStopSignal = true;
											}
											else
											{
												station.ForceStopSignal = false;
											}
										}
										break;
									case "system":
										switch (c.InnerText.ToLowerInvariant())
										{
											case "0":
											case "ATS":
												station.SafetySystem = Game.SafetySystem.Ats;
												break;
											case "1":
											case "ATC":
												station.SafetySystem = Game.SafetySystem.Atc;
												break;
											default:
												Interface.AddMessage(Interface.MessageType.Error, false, "An invalid station safety system was specified in XML file " + fileName);
												station.SafetySystem = Game.SafetySystem.Ats;
												break;
										}
										break;
									case "arrivalsound":
										string arrSound = string.Empty;
										double arrRadius = 30.0;
										if (!c.ChildNodes.OfType<XmlElement>().Any())
										{
											foreach (XmlNode cc in c.ChildNodes)
											{
												switch (c.Name.ToLowerInvariant())
												{
													case "filename":
														try
														{
															arrSound = OpenBveApi.Path.CombineFile(Path, cc.InnerText);
														}
														catch
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Arrival sound filename is invalid in XML file " + fileName);
														}
														break;
													case "radius":
														if (!double.TryParse(cc.InnerText, out arrRadius))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Arrival sound radius was invalid in XML file " + fileName);
														}
														break;
												}
											}
										}
										else
										{
											try
											{
												arrSound = OpenBveApi.Path.CombineFile(Path, c.InnerText);
											}
											catch
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "Arrival sound filename is invalid in XML file " + fileName);
											}
											
										}
										if (File.Exists(arrSound))
										{
											station.ArrivalSoundBuffer = Sounds.RegisterBuffer(arrSound, arrRadius);
										}
										else
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Arrival sound file does not exist in XML file " + fileName);
										}
										break;
									case "stopduration":
										double stopDuration;
										if (!double.TryParse(c.InnerText, out stopDuration))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Stop duration is invalid in XML file " + fileName);
										}
										else
										{
											if (stopDuration < 5.0)
											{
												stopDuration = 5.0;
											}
											station.StopTime = stopDuration;
										}
										break;
									case "passengerratio":
										double ratio;
										if (!double.TryParse(c.InnerText, out ratio))
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Passenger ratio is invalid in XML file " + fileName);
										}
										else
										{
											if (ratio < 0.0)
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "Passenger ratio must be non-negative in XML file " + fileName);
												ratio = 100.0;
											}
											station.PassengerRatio = ratio * 0.01;
										}
										break;
									case "departuresound":
										string depSound = string.Empty;
										double depRadius = 30.0;
										if (!c.ChildNodes.OfType<XmlElement>().Any())
										{
											foreach (XmlNode cc in c.ChildNodes)
											{
												switch (c.Name.ToLowerInvariant())
												{
													case "filename":
														try
														{
															depSound = OpenBveApi.Path.CombineFile(Path, cc.InnerText);
														}
														catch
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Departure sound filename is invalid in XML file " + fileName);
														}
														break;
													case "radius":
														if (!double.TryParse(cc.InnerText, out depRadius))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Departure sound radius was invalid in XML file " + fileName);
														}
														break;
												}
											}
										}
										else
										{
											try
											{
												depSound = OpenBveApi.Path.CombineFile(Path, c.InnerText);
											}
											catch
											{
												Interface.AddMessage(Interface.MessageType.Error, false, "Departure sound filename is invalid in XML file " + fileName);
											}

										}
										if (File.Exists(depSound))
										{
											station.DepartureSoundBuffer = Sounds.RegisterBuffer(depSound, depRadius);
										}
										else
										{
											Interface.AddMessage(Interface.MessageType.Error, false, "Departure sound file does not exist in XML file " + fileName);
										}
										break;
									case "timetableindex":
										if (!PreviewOnly)
										{
											int ttidx = -1;
											if (!string.IsNullOrEmpty(c.InnerText))
											{
												if(NumberFormats.TryParseIntVb6(c.InnerText, out ttidx))
												{
													if (ttidx < 0)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, "Timetable index must be non-negative in XML file " + fileName);
														ttidx = -1;
													}
													else if (ttidx >= daytimeTimetableTextures.Length & ttidx >= nighttimeTimetableTextures.Length)
													{
														Interface.AddMessage(Interface.MessageType.Error, false, "Timetable index references a non-loaded texture in XML file " + fileName);
														ttidx = -1;
													}
													station.TimetableDaytimeTexture = ttidx >= 0 & ttidx < daytimeTimetableTextures.Length ? daytimeTimetableTextures[ttidx] : null;
													station.TimetableNighttimeTexture = ttidx >= 0 & ttidx < nighttimeTimetableTextures.Length ? nighttimeTimetableTextures[ttidx] : null;
													break;
												}
											}
											if (ttidx == -1)
											{
												if (CurrentStation > 0)
												{
													station.TimetableDaytimeTexture = Game.Stations[CurrentStation - 1].TimetableDaytimeTexture;
													station.TimetableNighttimeTexture = Game.Stations[CurrentStation - 1].TimetableNighttimeTexture;
												}
												else if (daytimeTimetableTextures.Length > 0 & nighttimeTimetableTextures.Length > 0)
												{
													station.TimetableDaytimeTexture = daytimeTimetableTextures[0];
													station.TimetableNighttimeTexture = nighttimeTimetableTextures[0];
												}
											}
										}
										break;
									case "requeststop":
										station.StationType = Game.StationType.RequestStop;
										station.StopMode = Game.StationStopMode.AllRequestStop;
										foreach (XmlNode cc in c.ChildNodes)
										{
											switch (cc.Name.ToLowerInvariant())
											{
												case "aibehaviour":
													switch (cc.InnerText.ToLowerInvariant())
													{
														case "fullspeed":
														case "0":
															//With this set, the AI driver will not attempt to brake, but pass through at linespeed
															stopRequest.FullSpeed = true;
															break;
														case "normalbrake":
														case "1":
															//With this set, the AI driver breaks to a near stop whilst passing through the station
															stopRequest.FullSpeed = false;
															break;
													}
													break;
												case "playeronly":
													station.StopMode = Game.StationStopMode.PlayerRequestStop;
													break;
												case "distance":
													if (!string.IsNullOrEmpty(cc.InnerText))
													{
														double d;
														if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out d))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Request stop distance is invalid in XML file " + fileName);
															break;
														}
														stopRequest.TrackPosition -= Math.Abs(d);
													}
													break;
												case "earlytime":
													if (!string.IsNullOrEmpty(cc.InnerText))
													{
														if (!Interface.TryParseTime(cc.InnerText, out stopRequest.Early.Time))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Request stop early time was invalid in XML file " + fileName);
														}
													}
													break;
												case "latetime":
													if (!string.IsNullOrEmpty(cc.InnerText))
													{
														if (!Interface.TryParseTime(cc.InnerText, out stopRequest.Late.Time))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Request stop late time was invalid in XML file " + fileName);
														}
													}
													break;
												case "stopmessage":
													if (cc.ChildNodes.OfType<XmlElement>().Any())
													{
														foreach (XmlNode cd in cc.ChildNodes)
														{
															switch (cd.Name.ToLowerInvariant())
															{
																case "early":
																	if (!string.IsNullOrEmpty(cd.InnerText))
																	{
																		stopRequest.Early.StopMessage = cd.InnerText;
																	}
																	break;
																case "ontime":
																	if (!string.IsNullOrEmpty(cd.InnerText))
																	{
																		stopRequest.OnTime.StopMessage = cd.InnerText;
																	}
																	break;
																case "late":
																	if (!string.IsNullOrEmpty(cd.InnerText))
																	{
																		stopRequest.Late.StopMessage = cd.InnerText;
																	}
																	break;
																case "#text":
																	stopRequest.Early.StopMessage = cc.InnerText;
																	stopRequest.OnTime.StopMessage = cc.InnerText;
																	stopRequest.Late.StopMessage = cc.InnerText;
																	break;
															}
														}
													}
													break;
												case "passmessage":
													if (cc.ChildNodes.OfType<XmlElement>().Any())
													{
														foreach (XmlNode cd in cc.ChildNodes)
														{
															switch (cd.Name.ToLowerInvariant())
															{
																case "early":
																	if (!string.IsNullOrEmpty(cd.InnerText))
																	{
																		stopRequest.Early.PassMessage = cd.InnerText;
																	}
																	break;
																case "ontime":
																	if (!string.IsNullOrEmpty(cd.InnerText))
																	{
																		stopRequest.OnTime.PassMessage = cd.InnerText;
																	}
																	break;
																case "late":
																	if (!string.IsNullOrEmpty(cd.InnerText))
																	{
																		stopRequest.Late.PassMessage = cd.InnerText;
																	}
																	break;
																case "#text":
																	stopRequest.Early.PassMessage = cc.InnerText;
																	stopRequest.OnTime.PassMessage = cc.InnerText;
																	stopRequest.Late.PassMessage = cc.InnerText;
																	break;
															}
														}
													}
													break;
												case "probability":
													foreach (XmlNode cd in cc.ChildNodes)
													{
														switch (cd.Name.ToLowerInvariant())
														{
															case "early":
																if (!string.IsNullOrEmpty(cd.InnerText))
																{
																	if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.Early.Probability))
																	{
																		Interface.AddMessage(Interface.MessageType.Error, false, "Request stop early probability was invalid in XML file " + fileName);
																	}
																}
																break;
															case "ontime":
																if (!string.IsNullOrEmpty(cd.InnerText))
																{
																	if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.OnTime.Probability))
																	{

																		Interface.AddMessage(Interface.MessageType.Error, false, "Request stop ontime probability was invalid in XML file " + fileName);
																	}
																}
																break;
															case "late":
																if (!string.IsNullOrEmpty(cd.InnerText))
																{
																	if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.OnTime.Probability))
																	{

																		Interface.AddMessage(Interface.MessageType.Error, false, "Request stop late probability was invalid in XML file " + fileName);
																	}
																}
																break;
															case "#text":
																if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.OnTime.Probability))
																{

																	Interface.AddMessage(Interface.MessageType.Error, false, "Request stop probability was invalid in XML file " + fileName);
																}
																break;
														}
													}
													
													break;
												case "maxcars":
													if (!NumberFormats.TryParseIntVb6(cc.InnerText, out stopRequest.MaxNumberOfCars))
													{
														Interface.AddMessage(Interface.MessageType.Error, false, "Request stop maximum cars was invalid in XML file " + fileName);
													}
													break;
											}
										}
										
										break;
								}
							}
							

						}
					}
					return station;
				}
			}
			//We couldn't find any valid XML, so return false
			throw new InvalidDataException();
		}
	}
}
