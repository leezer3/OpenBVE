//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void LoadStationList(string FileName, MapData ParseData, RouteData RouteData)
		{
			RouteData.StationList = new Dictionary<string, Station>();
			// Everything breaks if no station list
			if (string.IsNullOrEmpty(ParseData.StationListPath))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, true, "No BVE5 Station List file was specified.");
				return;
			}

			string stationList = ParseData.StationListPath;

			if (!File.Exists(stationList))
			{
				stationList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), stationList);

				if (!File.Exists(stationList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Station List file " + stationList + " was not found.");
					return;
				}
			}

			System.Text.Encoding Encoding = Text.DetermineBVE5FileEncoding(stationList);
			string[] Lines = File.ReadAllLines(stationList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();

			for (int currentLine = 1; currentLine < Lines.Length; currentLine++)
			{
				Lines[currentLine] = Lines[currentLine].TrimBVE5Comments();
				if (string.IsNullOrEmpty(Lines[currentLine]))
				{
					continue;
				}

				
				string[] splitLine = Lines[currentLine].Split(',');
				if (splitLine.Length < 2)
				{
					// Need at least the key and name for this to be a sensibly valid station
					continue;
				}

				string stationKey = string.Empty;
				Station newStation = new Station();
				for (int i = 0; i < splitLine.Length; i++)
				{
					splitLine[i] = splitLine[i].Trim();
					switch (i)
					{
						case 0:
							stationKey = splitLine[i].ToLowerInvariant();
							break;
						case 1:
							newStation.Name = splitLine[i];
							break;
						case 2:
							if (splitLine[i].Equals("p", StringComparison.InvariantCultureIgnoreCase))
							{
								newStation.StopMode = StationStopMode.AllPass;
							}

							if (!TryParseBve5Time(splitLine[i], out newStation.ArrivalTime))
							{
								newStation.ArrivalTime = -1.0;
							}

							break;
						case 3:
							if (splitLine[i].Equals("t", StringComparison.InvariantCultureIgnoreCase))
							{
								newStation.StationType = StationType.Terminal;
							}

							if (!TryParseBve5Time(splitLine[i], out newStation.DepartureTime))
							{
								newStation.DepartureTime = -1.0;
							}

							break;
						case 4:
							{
								if (!NumberFormats.TryParseDoubleVb6(splitLine[i], out double stopTime))
								{
									stopTime = 15.0;
								}
								else if (stopTime < 5.0)
								{
									stopTime = 5.0;
								}

								newStation.StopTime = stopTime;
							}
							break;
						case 5:
							if (!TryParseBve5Time(splitLine[i], out newStation.DefaultTime))
							{
								newStation.DefaultTime = -1.0;
							}

							break;
						case 6:
							{
								if (!NumberFormats.TryParseIntVb6(splitLine[i], out int signalFlag))
								{
									signalFlag = 0;
								}

								newStation.ForceStopSignal = signalFlag == 1;
							}
							break;
						case 7:
							{
								if (!NumberFormats.TryParseDoubleVb6(splitLine[i], out double alightingTime))
								{
									alightingTime = 0.0;
								}

								newStation.AlightingTime = alightingTime;
							}
							break;
						case 8:
							{
								if (!NumberFormats.TryParseDoubleVb6(splitLine[i], out double passengerRatio))
								{
									passengerRatio = 100.0;
								}

								newStation.PassengerRatio = passengerRatio / 100.0;
							}
							break;
						case 9:
							newStation.ArrivalSoundKey = splitLine[i];
							break;
						case 10:
							newStation.DepartureSoundKey = splitLine[i];
							break;
						case 11:
							{
								if (!NumberFormats.TryParseDoubleVb6(splitLine[i], out double reopenDoor) || reopenDoor < 0.0)
								{
									reopenDoor = 0.0;
								}

								newStation.ReopenDoor = reopenDoor / 100.0;
							}
							break;
						case 12:
							{
								if (!NumberFormats.TryParseDoubleVb6(splitLine[i], out double interferenceInDoor) || interferenceInDoor < 0.0)
								{
									interferenceInDoor = 0.0;
								}

								newStation.InterferenceInDoor = interferenceInDoor;
							}
							break;
					}
				}

				if (!string.IsNullOrEmpty(stationKey) && !string.IsNullOrEmpty(newStation.Name))
				{
					// Key and name *must* be set, everything else can be ignored
					if (RouteData.StationList.ContainsKey(stationKey))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The station with key " + stationKey + " has been declared twice in BVE5 station list file " + ParseData.StationListPath);
						RouteData.StationList[stationKey] = newStation;
					}
					else
					{
						RouteData.StationList.Add(stationKey, newStation);
					}
					
				}

			}

		}
	}
}
