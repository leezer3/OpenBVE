using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using RouteManager2.Stations;

namespace Bve5RouteParser
{
	internal partial class Parser
	{
		/// <summary>Loads the list of objects for a BVE5 map</summary>
		/// <param name="ObjectList">The absolute on-disk path to the object list</param>
		/// <param name="Data">The route data</param>
		private void LoadObjects(string ObjectList, ref RouteData Data, System.Text.Encoding Encoding, bool PreviewOnly)
		{
			//Read object list file into memory
			string[] Lines = File.ReadAllLines(ObjectList, Encoding);
			string b = String.Empty;
			if (!Lines[0].ToLowerInvariant().StartsWith("bvets structure list"))
			{
				throw new Exception("File " + ObjectList + " is not a BVE5 structure list file");
			}
			for (int i = 21; i < Lines[0].Length; i++)
			{
				if (Char.IsDigit(Lines[0][i]) || Lines[0][i] == '.')
				{
					b = b + Lines[0][i];
				}
				else
				{
					break;
				}
			}
			if (b.Length > 0)
			{
				double version = 0;
				NumberFormats.TryParseDoubleVb6(b, out version);
				if (version > 1.0)
				{
					throw new Exception(version + " is not a supported BVE5 structure list version");
				}
			}
			else
			{
				throw new Exception("File " + ObjectList + " does not contain a BVE5 structure list version");
			}
			
			for (int i = 1; i < Lines.Length; i++)
			{
				//Cycle through the list of objects
				//An object index is formatted as follows:
				// --KEY USED BY ROUTEFILE-- , --PATH TO OBJECT RELATIVE TO STRUCTURE FILE--

				//Remove comments
				Lines[i] = Lines[i].TrimBVE5Comments();
				if (Lines[i].Length == 0)
				{
					continue;
				}

				int a = Lines[i].IndexOf(',');
				string FilePath = Lines[i].Substring(a + 1, Lines[i].Length - a - 1);
				string Name = Lines[i].Substring(0, a);
				try
				{
					FilePath = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(ObjectList), FilePath);
				}
				catch { }
				if (!System.IO.File.Exists(FilePath))
				{
					//If our path does not exist, set it to a null reference in order for the sim to display nothing
					FilePath = null;
				}
				if (Data.UsedObjects >= Data.ObjectList.Length)
				{
					Array.Resize(ref Data.ObjectList, Data.ObjectList.Length + 1);
				}
				Data.ObjectList[Data.UsedObjects] = new ObjectPointer { Name = Name, Path = FilePath };
				
				if (!PreviewOnly)
				{
					if (Data.Structure.Objects.Length <= Data.UsedObjects)
					{
						Array.Resize(ref Data.Structure.Objects, Data.UsedObjects + 1);

					}
					if (FilePath != null)
					{
						Plugin.CurrentHost.LoadObject(FilePath, Encoding, out Data.Structure.Objects[Data.UsedObjects]);
					}
				}
				Data.UsedObjects++;
				
			}
		}

		/// <summary>Loads the list of stations for a BVE5 map</summary>
		/// <param name="StationList">The absolute on-disk path to the station list</param>
		/// <param name="Data">The route data</param>
		/// <param name="Encoding">The text encoding</param>
		/// <param name="PreviewOnly">Whether this is a preview only</param>
		private void LoadStations(string StationList, ref RouteData Data, System.Text.Encoding Encoding, bool PreviewOnly)
		{
			//Read station list file into memory
			string[] Lines = File.ReadAllLines(StationList, Encoding);
			string b = String.Empty;
			if (!Lines[0].ToLowerInvariant().StartsWith("bvets station list"))
			{
				throw new Exception("File " + StationList + " is not a BVE5 station list file");
			}
			for (int i = 19; i < Lines[0].Length; i++)
			{
				if (Char.IsDigit(Lines[0][i]) || Lines[0][i] == '.')
				{
					b = b + Lines[0][i];
				}
				else
				{
					break;
				}
			}
			if (b.Length > 0)
			{
				double version = 0;
				NumberFormats.TryParseDoubleVb6(b, out version);
				if (version > 2.0)
				{
					throw new Exception(version + " is not a supported BVE5 station list version");
				}
			}
			else
			{
				throw new Exception("File " + StationList + " does not contain a BVE5 station list version");
			}
			int CurrentStation = 0;
			for (int i = 1; i < Lines.Length; i++)
			{
				//Remove comments
				Lines[i] = Lines[i].TrimBVE5Comments();
				if (Lines[i].Length == 0)
				{
					continue;
				}

				string SoundPath = string.Empty;
				List<string> Arguments = new List<string>(Lines[i].Split(','));
				if (Arguments[0].Length == 0)
				{
					//stationKey - Any string (Used in map file)
					throw new Exception("BVE5 station key empty");
				}
				Array.Resize(ref CurrentRoute.Stations, CurrentStation + 1);
				CurrentRoute.Stations[CurrentStation] = new RouteStation();
				CurrentRoute.Stations[CurrentStation].Name = string.Empty;
				CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.AllStop;
				CurrentRoute.Stations[CurrentStation].Type = StationType.Normal;
				if (Arguments.Count >= 2 && Arguments[1].Length > 0)
				{
					//stationName - The station name that is displayed in the time table
					CurrentRoute.Stations[CurrentStation].Name = Arguments[1];
				}
				CurrentRoute.Stations[CurrentStation].Key = Arguments[0].ToLowerInvariant();

				//TODO: Sounds are referred to using the key methodology, rather than by a simple filename
				//TODO: Jump time is not implemented in openBVE (?? - CHECK whether it resets to the specified arrival time ??)

				double arr = -1.0, dep = -1.0, jump = -1.0;
				//Cycle through the list of stations
				if (Arguments.Count >= 3 && Arguments[2].Length > 0)
				{
					//Arrival time (HH:mm:ss or P for pass)
					if (string.Equals(Arguments[2], "P", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[1], "L", StringComparison.OrdinalIgnoreCase))
					{
						CurrentRoute.Stations[CurrentStation].StopMode = StationStopMode.AllPass;
					}
					else if (!TryParseBve5Time(Arguments[2], out arr))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ArrivalTime is invalid in Track.Sta at line " + i + " in file " + StationList);
						arr = -1.0;
					}
				}
				if (Arguments.Count >= 4 && Arguments[3].Length > 0)
				{
					//Departure time or passing through time (HH:mm:ss or T for terminal station)
					if (string.Equals(Arguments[3], "T", StringComparison.OrdinalIgnoreCase) | string.Equals(Arguments[3], "=", StringComparison.OrdinalIgnoreCase))
					{
						CurrentRoute.Stations[CurrentStation].Type = StationType.Terminal;
					}
					else if (!TryParseBve5Time(Arguments[3], out dep))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "DepartureTime is invalid in Track.Sta at line " + i + " in file " + StationList);
						dep = -1.0;
					}
				}
				double halt = 15.0;
				if (Arguments.Count >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out halt))
				{
					//Minimum stop duration
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "StopDuration is invalid in Track.Sta at line " + i + " in file " + StationList);
					halt = 15.0;
				}
				else if (halt < 5.0)
				{
					halt = 5.0;
				}
				if (Arguments.Count >= 6 && Arguments[5].Length > 0)
				{
					//Time after having jumped to this station
					if (!TryParseBve5Time(Arguments[5], out dep))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "JumpTime is invalid in Track.Sta at line " + i + " in file " + StationList);
						jump = -1.0;
					}
				}
				int stop = 0;
				if (Arguments.Count >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[6], out stop))
				{
					//Whether the signal is held at red until the departure time
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "ForcedRedSignal is invalid in Track.Sta at line " + i + " in file " + StationList);
					stop = 0;
				}
				double alightTime = 0;
				if (Arguments.Count >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[7], out alightTime))
				{
					//Time required for passengers to alight, must have some relationship to passenger loads?
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "AlightTime is invalid in Track.Sta at line " + i + " in file " + StationList);
					alightTime = 0;
				}
				double jam = 100.0;
				if (!PreviewOnly)
				{
					//The ratio of passengers to the train's capacity
					if (Arguments.Count >= 9 && Arguments[8].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[8], out jam))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PassengerRatio is invalid in Track.Sta at line " + i + " in file " + StationList);
						jam = 100.0;
					}
					else if (jam < 0.0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PassengerRatio is expected to be non-negative in Track.Sta at line " + i + " in file " + StationList);
						jam = 100.0;
					}
				}
				

				if (CurrentRoute.Stations[CurrentStation].Name.Length == 0 & (CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.PlayerStop | CurrentRoute.Stations[CurrentStation].StopMode == StationStopMode.AllStop))
				{
					CurrentRoute.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1);
				}
				CurrentRoute.Stations[CurrentStation].ArrivalTime = arr;
				CurrentRoute.Stations[CurrentStation].DepartureTime = dep;
				CurrentRoute.Stations[CurrentStation].StopTime = halt;
				CurrentRoute.Stations[CurrentStation].ForceStopSignal = stop == 1;
				CurrentRoute.Stations[CurrentStation].Stops = new StationStop[] { };
				CurrentRoute.Stations[CurrentStation].PassengerRatio = 0.01 * jam;
				CurrentRoute.Stations[CurrentStation].DefaultTrackPosition = Data.TrackPosition;
				CurrentStation++;
			}
		}

		private static void LoadSections(string SectionList, ref RouteData Data, System.Text.Encoding Encoding)
		{
			//Read section list file into memory
			string[] Lines = File.ReadAllLines(SectionList, Encoding);
			string b = String.Empty;
			if (!Lines[0].ToLowerInvariant().StartsWith("bvets signal aspects list"))
			{
				throw new Exception("File " + SectionList + " is not a BVE5 signal aspect list file");
			}
			for (int i = 26; i < Lines[0].Length; i++)
			{
				if (Char.IsDigit(Lines[0][i]) || Lines[0][i] == '.')
				{
					b = b + Lines[0][i];
				}
				else
				{
					break;
				}
			}
			if (b.Length > 0)
			{
				double version = 0;
				NumberFormats.TryParseDoubleVb6(b, out version);
				if (version > 2.0)
				{
					throw new Exception(version + " is not a supported BVE5 signal aspect list version");
				}
			}
			else
			{
				throw new Exception("File " + SectionList + " does not contain a BVE5 signal aspect list version");
			}
			bool Glow = false;
			int index = 0;
			for (int i = 1; i < Lines.Length; i++)
			{
				//Remove comments
				Lines[i] = Lines[i].TrimBVE5Comments();
				if (Lines[i].Length == 0)
				{
					continue;
				}

				string[] Arguments = Lines[i].Split(',');
				//If the key is blank, this line defines the glows for the previous line....
				if (Arguments.Length < 0)
				{
					if (Arguments[0].Trim() == String.Empty)
					{
						Glow = true;
					}
				}
				//Load objects and aspects
				int[] Numbers = new int[0];
				StaticObject[] Objects = new StaticObject[0];
				if (Glow == false)
				{
					for (int j = 1; j < Arguments.Length; j++)
					{
						var Key = Arguments[j].Trim();
						if (Key.Length > 0)
						{
							var idx = Numbers.Length;
							//Object key, so we must resize our arrays first
							Array.Resize(ref Numbers, idx + 1);
							Array.Resize(ref Objects, idx + 1);
							//Set the aspect number
							Numbers[idx] = j;
							//Create our object
							string sttype = FindStructurePath(Key, Data);
							Plugin.CurrentHost.LoadStaticObject(sttype, Encoding, false, out Objects[idx]);
						}
					}
					var sigidx = Data.CompatibilitySignalData.Length;
					Array.Resize(ref Data.CompatibilitySignalData, sigidx + 1);
					Data.CompatibilitySignalData[sigidx] = new CompatibilitySignalData(Numbers, Objects, Arguments[0]);
				}
				else
				{
					//TODO: Implement glow......
				}

			}
		}
	}
}
