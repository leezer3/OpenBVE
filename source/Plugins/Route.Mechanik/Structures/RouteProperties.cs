using System.Collections.Generic;
using OpenBveApi.Runtime;

namespace MechanikRouteParser
{
	internal partial class Parser
	{
		/// <summary>Holds the list of known complete Mechanik routes</summary>
		internal static Dictionary<string, RouteProperties> KnownRoutes;
		/// <summary>Holds the list of known Mechanik modules</summary>
		/// <remarks>Any number of modules could be combined using the Mechnik route generator
		/// to generate a 'complete' routefile. They are not useful by themselves, but
		/// structurally resemble a complete route, and should be ignored.</remarks>
		internal static List<string> KnownModules;

		internal static void GetProperties(string hash)
		{
			if (KnownRoutes == null || !KnownRoutes.ContainsKey(hash))
			{
				return;
			}

			RouteProperties routeProperties = KnownRoutes[hash];
			for (int i = 0; i < Plugin.CurrentRoute.Stations.Length; i++)
			{
				if (routeProperties.StationNames.Length == Plugin.CurrentRoute.Stations.Length)
				{
					Plugin.CurrentRoute.Stations[i].Name = routeProperties.StationNames[i];
				}
				if (routeProperties.DepartureTimes.Length == Plugin.CurrentRoute.Stations.Length)
				{
					Plugin.CurrentRoute.Stations[i].DepartureTime = routeProperties.DepartureTimes[i];
				}
				if (routeProperties.Doors.Length == Plugin.CurrentRoute.Stations.Length)
				{
					Plugin.CurrentRoute.Stations[i].OpenLeftDoors = (routeProperties.Doors[i] & DoorStates.Left) != 0;
					Plugin.CurrentRoute.Stations[i].OpenRightDoors = (routeProperties.Doors[i] & DoorStates.Right) != 0;
				}
			}

			Plugin.CurrentRoute.Comment = routeProperties.Description;
			Plugin.CurrentOptions.TrainName = routeProperties.DefaultTrain;
		}
	}

	/// <summary>Contains the properties of a known Mechanik route</summary>
	/// <remarks>As these are not set in file, and have not set format,
	/// we provide a list (Station names etc.) to make players life easier</remarks>
	internal class RouteProperties
	{
		/// <summary>The station names</summary>
		internal string[] StationNames;
		/// <summary>The description to be shown in the in-game menu</summary>
		internal string Description;
		/// <summary>The default train to be used</summary>
		internal string DefaultTrain;
		/// <summary>The station departure times</summary>
		internal double[] DepartureTimes;
		/// <summary>The doors to be opened at this station</summary>
		internal DoorStates[] Doors;

		internal RouteProperties()
		{
			StationNames = new string[] { };
			DepartureTimes = new double[] { };
			Doors = new DoorStates[] { };
		}
	}
}
