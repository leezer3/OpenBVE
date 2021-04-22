using System.Collections.Generic;

namespace MechanikRouteParser
{
	internal partial class Parser
	{
		internal static Dictionary<string, RouteProperties> knownRoutes;
		internal static List<string> knownModules;

		internal static void GetProperties(string hash)
		{
			if (knownRoutes == null || !knownRoutes.ContainsKey(hash))
			{
				return;
			}

			RouteProperties routeProperties = knownRoutes[hash];
			for (int i = 0; i < routeProperties.StationNames.Length; i++)
			{
				Plugin.CurrentRoute.Stations[i].Name = routeProperties.StationNames[i];
				if (routeProperties.DepartureTimes.Length == routeProperties.StationNames.Length)
				{
					Plugin.CurrentRoute.Stations[i].DepartureTime = routeProperties.DepartureTimes[i];
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

		internal RouteProperties()
		{
			StationNames = new string[] { };
			DepartureTimes = new double[] { };
		}
	}
}
