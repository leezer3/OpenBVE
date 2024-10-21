using System;
using System.IO;
using System.Xml;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using RouteManager2;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using Path = OpenBveApi.Path;

namespace CsvRwRouteParser
{
	class StationXMLParser
	{
		public static RouteStation ReadStationXML(CurrentRoute Route, string fileName, bool PreviewOnly, Texture[] daytimeTimetableTextures, Texture[] nighttimeTimetableTextures, int CurrentStation, ref bool passAlarm, ref StopRequest stopRequest)
		{
			RouteStation station = new RouteStation
			{
				Stops = new StationStop[] { }
			};
			stopRequest.Early = new RequestStop();
			stopRequest.OnTime = new RequestStop();
			stopRequest.Late = new RequestStop();
			stopRequest.OnTime.Probability = 75;
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			currentXML.Load(fileName);
			string filePath = Path.GetDirectoryName(fileName);
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Station");
				//Check this file actually contains OpenBVE station nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode c in n.ChildNodes)
							{
								if (!Enum.TryParse(c.Name, true, out StationXmlCommand parsedCommand))
								{
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unknown entry " + c.Name + " encountered in XML file " + fileName);
									continue;
								}
								switch (parsedCommand)
								{
									case StationXmlCommand.Name:
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											station.Name = c.InnerText;
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Station name was empty in XML file " + fileName);
										}
										break;
									case StationXmlCommand.ArrivalTime:
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (!Parser.TryParseTime(c.InnerText, out station.ArrivalTime))
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Station arrival time was invalid in XML file " + fileName);
											}
										}
										break;
									case StationXmlCommand.DepartureTime:
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (!Parser.TryParseTime(c.InnerText, out station.DepartureTime))
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Station arrival time was invalid in XML file " + fileName);
											}
										}
										break;
									case StationXmlCommand.Type:
										switch (c.InnerText.ToLowerInvariant())
										{
											case "c":
											case "changeends":
												station.Type = StationType.ChangeEnds;
												break;
											case "j":
											case "jump":
												station.Type = StationType.Jump;
												break;
											case "t":
											case "terminal":
												station.Type = StationType.Terminal;
												break;
											default:
												station.Type = StationType.Normal;
												break;
										}
										break;
									case StationXmlCommand.JumpIndex:
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											if (!NumberFormats.TryParseIntVb6(c.InnerText, out station.JumpIndex))
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Station jump index was invalid in XML file " + fileName);
												station.Type = StationType.Normal;
											}
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Station jump index was empty in XML file " + fileName);
											station.Type = StationType.Normal;
										}
										break;
									case StationXmlCommand.PassAlarm:
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
									case StationXmlCommand.Doors:
										Direction door = Direction.Both;
										if (!string.IsNullOrEmpty(c.InnerText))
										{
											door = Parser.FindDirection(c.InnerText, "StationXML:Doors", false, -1, Path.GetFileName(fileName));
										}
										station.OpenLeftDoors = door == Direction.Left | door == Direction.Both;
										station.OpenRightDoors = door == Direction.Right | door == Direction.Both;
										break;
									case StationXmlCommand.ForcedRedSignal:
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
									case StationXmlCommand.System:
										switch (c.InnerText.ToLowerInvariant())
										{
											case "0":
											case "ATS":
												station.SafetySystem = SafetySystem.Ats;
												break;
											case "1":
											case "ATC":
												station.SafetySystem = SafetySystem.Atc;
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "An invalid station safety system was specified in XML file " + fileName);
												station.SafetySystem = SafetySystem.Ats;
												break;
										}
										break;
									case StationXmlCommand.ArrivalSound:
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
															arrSound = Path.CombineFile(filePath, cc.InnerText);
														}
														catch
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Arrival sound filename is invalid in XML file " + fileName);
														}
														break;
													case "radius":
														if (!double.TryParse(cc.InnerText, out arrRadius))
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Arrival sound radius was invalid in XML file " + fileName);
														}
														break;
												}
											}
										}
										else
										{
											try
											{
												arrSound = Path.CombineFile(filePath, c.InnerText);
											}
											catch
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Arrival sound filename is invalid in XML file " + fileName);
											}
											
										}
										if (File.Exists(arrSound))
										{
											 Plugin.CurrentHost.RegisterSound(arrSound, arrRadius, out station.ArrivalSoundBuffer);
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Arrival sound file does not exist in XML file " + fileName);
										}
										break;
									case StationXmlCommand.StopDuration:
										if (!double.TryParse(c.InnerText, out double stopDuration))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Stop duration is invalid in XML file " + fileName);
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
									case StationXmlCommand.PassengerRatio:
										if (!double.TryParse(c.InnerText, out double ratio))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Passenger ratio is invalid in XML file " + fileName);
										}
										else
										{
											if (ratio < 0.0)
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Passenger ratio must be non-negative in XML file " + fileName);
												ratio = 100.0;
											}
											station.PassengerRatio = ratio * 0.01;
										}
										break;
									case StationXmlCommand.DepartureSound:
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
															depSound = Path.CombineFile(filePath, cc.InnerText);
														}
														catch
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Departure sound filename is invalid in XML file " + fileName);
														}
														break;
													case "radius":
														if (!double.TryParse(cc.InnerText, out depRadius))
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Departure sound radius was invalid in XML file " + fileName);
														}
														break;
												}
											}
										}
										else
										{
											try
											{
												depSound = Path.CombineFile(filePath, c.InnerText);
											}
											catch
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Departure sound filename is invalid in XML file " + fileName);
											}

										}
										if (File.Exists(depSound))
										{
											Plugin.CurrentHost.RegisterSound(depSound, depRadius, out station.DepartureSoundBuffer);
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Departure sound file does not exist in XML file " + fileName);
										}
										break;
									case StationXmlCommand.TimetableIndex:
										if (!PreviewOnly)
										{
											int ttidx = -1;
											if (!string.IsNullOrEmpty(c.InnerText))
											{
												if(NumberFormats.TryParseIntVb6(c.InnerText, out ttidx))
												{
													if (ttidx < 0)
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Timetable index must be non-negative in XML file " + fileName);
														ttidx = -1;
													}
													else if (ttidx >= daytimeTimetableTextures.Length & ttidx >= nighttimeTimetableTextures.Length)
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Timetable index references a non-loaded texture in XML file " + fileName);
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
													station.TimetableDaytimeTexture = Route.Stations[CurrentStation - 1].TimetableDaytimeTexture;
													station.TimetableNighttimeTexture = Route.Stations[CurrentStation - 1].TimetableNighttimeTexture;
												}
												else if (daytimeTimetableTextures.Length > 0 & nighttimeTimetableTextures.Length > 0)
												{
													station.TimetableDaytimeTexture = daytimeTimetableTextures[0];
													station.TimetableNighttimeTexture = nighttimeTimetableTextures[0];
												}
											}
										}
										break;
									case StationXmlCommand.ReOpenDoor:
										if (!double.TryParse(c.InnerText, out double reopenDoor))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenDoor is invalid in XML file " + fileName);
											reopenDoor = 0.0;
										}
										else
										{
											if (reopenDoor < 0.0)
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenDoor must be non-negative in XML file " + fileName);
												reopenDoor = 0.0;
											}
										}
										station.ReopenDoor = 0.01 * reopenDoor;
										break;
									case StationXmlCommand.ReOpenStationLimit:
										if (!int.TryParse(c.InnerText, out int reopenStationLimit))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenStationLimit is invalid in XML file " + fileName);
											reopenStationLimit = 5;
										}
										else
										{
											if (reopenStationLimit < 0)
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ReopenStationLimit must be non-negative in XML file " + fileName);
												reopenStationLimit = 0;
											}
										}
										station.ReopenStationLimit = reopenStationLimit;
										break;
									case StationXmlCommand.InterferenceInDoor:
										if (!double.TryParse(c.InnerText, out double interferenceInDoor))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "InterferenceInDoor is invalid in XML file " + fileName);
											interferenceInDoor = 0.0;
										}
										else
										{
											if (interferenceInDoor < 0.0)
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "InterferenceInDoor must be non-negative in XML file " + fileName);
												interferenceInDoor = 0.0;
											}
										}
										station.InterferenceInDoor = interferenceInDoor;
										break;
									case StationXmlCommand.MaxInterferingObjectRate:
										if (!int.TryParse(c.InnerText, out int maxInterferingObjectRate))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate is invalid in XML file " + fileName);
											maxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
										}
										else
										{
											if (maxInterferingObjectRate <= 0 || maxInterferingObjectRate >= 100)
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MaxInterferingObjectRate must be positive, less than 100 in XML file " + fileName);
												maxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
											}
										}
										station.MaxInterferingObjectRate = maxInterferingObjectRate;
										break;
									case StationXmlCommand.RequestStop:
										station.Type = StationType.RequestStop;
										station.StopMode = StationStopMode.AllRequestStop;
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
													station.StopMode = StationStopMode.PlayerRequestStop;
													break;
												case "distance":
													if (!string.IsNullOrEmpty(cc.InnerText))
													{
														if (!NumberFormats.TryParseDoubleVb6(cc.InnerText, out double d))
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop distance is invalid in XML file " + fileName);
															break;
														}
														stopRequest.TrackPosition -= Math.Abs(d);
													}
													break;
												case "earlytime":
													if (!string.IsNullOrEmpty(cc.InnerText))
													{
														if (!Parser.TryParseTime(cc.InnerText, out stopRequest.Early.Time))
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop early time was invalid in XML file " + fileName);
														}
													}
													break;
												case "latetime":
													if (!string.IsNullOrEmpty(cc.InnerText))
													{
														if (!Parser.TryParseTime(cc.InnerText, out stopRequest.Late.Time))
														{
															Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop late time was invalid in XML file " + fileName);
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
																case "text":
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
																case "text":
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
																		Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop early probability was invalid in XML file " + fileName);
																	}
																}
																break;
															case "ontime":
																if (!string.IsNullOrEmpty(cd.InnerText))
																{
																	if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.OnTime.Probability))
																	{

																		Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop ontime probability was invalid in XML file " + fileName);
																	}
																}
																break;
															case "late":
																if (!string.IsNullOrEmpty(cd.InnerText))
																{
																	if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.OnTime.Probability))
																	{

																		Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop late probability was invalid in XML file " + fileName);
																	}
																}
																break;
															case "text":
																if (!NumberFormats.TryParseIntVb6(cd.InnerText, out stopRequest.OnTime.Probability))
																{

																	Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop probability was invalid in XML file " + fileName);
																}
																break;
														}
													}
													
													break;
												case "maxcars":
													if (!NumberFormats.TryParseIntVb6(cc.InnerText, out stopRequest.MaxNumberOfCars))
													{
														Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Request stop maximum cars was invalid in XML file " + fileName);
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
