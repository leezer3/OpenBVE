using System;
using System.IO;
using System.Xml;
using OpenBveApi.Math;
using OpenBveApi;

namespace OpenBve
{
	class StationXMLParser
	{
		public static Game.Station ReadStationXML(string fileName, bool PreviewOnly, Textures.Texture[] daytimeTimetableTextures, Textures.Texture[] nighttimeTimetableTextures, int CurrentStation, ref bool passAlarm)
		{
			Game.Station station = new Game.Station();
			station.Stops = new Game.StationStop[] { };
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			currentXML.Load(fileName);
			string Path = System.IO.Path.GetDirectoryName(fileName);
			double[] UnitOfLength = { 1.0 };
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Station");
				//Check this file actually contains OpenBVE light definition nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.HasChildNodes)
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
										if (!c.HasChildNodes)
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
											station.PassengerRatio = ratio;
										}
										break;
									case "departuresound":
										string depSound = string.Empty;
										double depRadius = 30.0;
										if (!c.HasChildNodes)
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
											station.ArrivalSoundBuffer = Sounds.RegisterBuffer(depSound, depRadius);
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
