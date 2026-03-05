using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using RouteManager2;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using System;
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

			XMLFile<StationXMLSection, StationXMLKey> xmlFile = new XMLFile<StationXMLSection, StationXMLKey>(fileName, "/openBVE/Station", Plugin.CurrentHost);

			xmlFile.GetValue(StationXMLKey.Name, out station.Name);
			xmlFile.TryGetTime(StationXMLKey.ArrivalTime, ref station.ArrivalTime);
			xmlFile.TryGetTime(StationXMLKey.DepartureTime, ref station.DepartureTime);
			if (xmlFile.GetValue(StationXMLKey.Type, out string type))
			{
				switch (type.ToLowerInvariant())
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
			}

			if (xmlFile.TryGetValue(StationXMLKey.JumpIndex, ref station.JumpIndex, NumberRange.NonNegative))
			{
				station.Type = StationType.Jump;
			}

			xmlFile.GetValue(StationXMLKey.PassAlarm, out passAlarm);
			if (xmlFile.GetValue(StationXMLKey.Doors, out string doorDirection))
			{
				Direction door = Parser.FindDirection(doorDirection, "StationXML:Doors", false, -1, Path.GetFileName(fileName));
				station.OpenLeftDoors = door == Direction.Left || door == Direction.Both;
				station.OpenRightDoors = door == Direction.Right || door == Direction.Both;
			}

			xmlFile.GetValue(StationXMLKey.ForcedRedSignal, out station.ForceStopSignal);
			if (xmlFile.GetValue(StationXMLKey.System, out string safetySystem))
			{
				switch (safetySystem.ToUpperInvariant())
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
			}

			if (xmlFile.GetPath(StationXMLKey.ArrivalSound, Path.GetDirectoryName(fileName), out string arrSound))
			{
				Plugin.CurrentHost.RegisterSound(arrSound, 30, out station.ArrivalSoundBuffer);
			}
			else if (xmlFile.ReadBlock(StationXMLSection.ArrivalSound, out Block<StationXMLSection, StationXMLKey> arrivalSoundBlock))
			{
				arrivalSoundBlock.GetPath(StationXMLKey.Filename, Path.GetDirectoryName(fileName), out arrSound);
				double radius = 30;
				arrivalSoundBlock.TryGetValue(StationXMLKey.Radius, ref radius);
				Plugin.CurrentHost.RegisterSound(arrSound, radius, out station.ArrivalSoundBuffer);
			}

			if (xmlFile.GetPath(StationXMLKey.DepartureSound, Path.GetDirectoryName(fileName), out string depSound))
			{
				Plugin.CurrentHost.RegisterSound(depSound, 30, out station.ArrivalSoundBuffer);
			}
			else if (xmlFile.ReadBlock(StationXMLSection.DepartureSound, out Block<StationXMLSection, StationXMLKey> departureSoundBlock))
			{
				departureSoundBlock.GetPath(StationXMLKey.Filename, Path.GetDirectoryName(fileName), out depSound);
				double radius = 30;
				departureSoundBlock.TryGetValue(StationXMLKey.Radius, ref radius);
				Plugin.CurrentHost.RegisterSound(depSound, radius, out station.DepartureSoundBuffer);
			}

			double stopTime = 5.0;
			if (xmlFile.TryGetValue(StationXMLKey.StopDuration, ref stopTime) && stopTime > 5.0)
			{
				station.StopTime = stopTime;
			}

			if (xmlFile.GetValue(StationXMLKey.PassengerRatio, out double ratio, NumberRange.NonNegative))
			{
				station.PassengerRatio = ratio * 0.01;
			}

			if (xmlFile.GetValue(StationXMLKey.TimetableIndex, out int ttidx))
			{
				if (ttidx >= 0 && (ttidx < daytimeTimetableTextures.Length || ttidx < nighttimeTimetableTextures.Length))
				{
					station.TimetableDaytimeTexture = ttidx < daytimeTimetableTextures.Length ? daytimeTimetableTextures[ttidx] : null;
					station.TimetableNighttimeTexture = ttidx < nighttimeTimetableTextures.Length ? nighttimeTimetableTextures[ttidx] : null;
				}
				else if (ttidx == -1)
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
				else
				{
					if (ttidx < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Timetable index is invalid in XML file " + fileName);
						
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Timetable index references a non-loaded texture in XML file " + fileName);
					}
				}
			}

			if (xmlFile.GetValue(StationXMLKey.ReOpenDoor, out double reopenDoor, NumberRange.NonNegative))
			{
				station.ReopenDoor = 0.01 * reopenDoor;
			}

			xmlFile.TryGetValue(StationXMLKey.ReOpenStationLimit, ref station.ReopenStationLimit, NumberRange.NonNegative);
			xmlFile.TryGetValue(StationXMLKey.InterferenceInDoor, ref station.InterferenceInDoor, NumberRange.NonNegative);
			if (xmlFile.GetValue(StationXMLKey.MaxInterferingObjectRate, out int maxInterferingObjectRate, NumberRange.NonNegative) && maxInterferingObjectRate < 100)
			{
				station.MaxInterferingObjectRate = maxInterferingObjectRate;
			}

			if (xmlFile.ReadBlock(StationXMLSection.RequestStop, out Block<StationXMLSection, StationXMLKey> requestStopBlock))
			{
				station.Type = StationType.RequestStop;
				station.StopMode = StationStopMode.AllStop;
				if (requestStopBlock.GetValue(StationXMLKey.AIBehaviour, out string aiBehaviour))
				{
					switch (aiBehaviour.ToLowerInvariant())
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
				}

				if (requestStopBlock.GetValue(StationXMLKey.PlayerOnly, out bool _))
				{
					station.StopMode = StationStopMode.PlayerRequestStop;
				}

				if (requestStopBlock.GetValue(StationXMLKey.Distance, out double distance))
				{
					stopRequest.TrackPosition -= Math.Abs(distance);
				}

				requestStopBlock.TryGetTime(StationXMLKey.EarlyTime, ref stopRequest.Early.Time);
				requestStopBlock.TryGetTime(StationXMLKey.LateTime, ref stopRequest.Late.Time);
				if (requestStopBlock.ReadBlock(StationXMLSection.StopMessage, out Block<StationXMLSection, StationXMLKey> stopMessageBlock))
				{
					stopMessageBlock.GetValue(StationXMLKey.Early, out stopRequest.Early.StopMessage);
					stopMessageBlock.GetValue(StationXMLKey.OnTime, out stopRequest.OnTime.StopMessage);
					stopMessageBlock.GetValue(StationXMLKey.Late, out stopRequest.Late.StopMessage);
					if (stopMessageBlock.GetValue(StationXMLKey.Text, out string text))
					{
						stopRequest.Early.StopMessage = text;
						stopRequest.OnTime.StopMessage = text;
						stopRequest.Late.StopMessage = text;
					}
				}

				if (requestStopBlock.ReadBlock(StationXMLSection.PassMessage, out Block<StationXMLSection, StationXMLKey> passMessageBlock))
				{
					passMessageBlock.GetValue(StationXMLKey.Early, out stopRequest.Early.PassMessage);
					passMessageBlock.GetValue(StationXMLKey.OnTime, out stopRequest.OnTime.PassMessage);
					passMessageBlock.GetValue(StationXMLKey.Late, out stopRequest.Late.PassMessage);
					if (passMessageBlock.GetValue(StationXMLKey.Text, out string text))
					{
						stopRequest.Early.PassMessage = text;
						stopRequest.OnTime.PassMessage = text;
						stopRequest.Late.PassMessage = text;
					}
				}

				if (requestStopBlock.ReadBlock(StationXMLSection.Probability, out Block<StationXMLSection, StationXMLKey> probabilityBlock))
				{
					probabilityBlock.TryGetValue(StationXMLKey.Early, ref stopRequest.Early.Probability);
					probabilityBlock.TryGetValue(StationXMLKey.OnTime, ref stopRequest.OnTime.Probability);
					probabilityBlock.TryGetValue(StationXMLKey.Late, ref stopRequest.Late.Probability);
				}

				requestStopBlock.TryGetValue(StationXMLKey.MaxCars, ref stopRequest.MaxNumberOfCars, NumberRange.Positive);
			}

			return station;
		}
	}
}
