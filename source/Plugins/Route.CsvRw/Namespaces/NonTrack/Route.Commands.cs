namespace CsvRwRouteParser
{
	internal enum RouteCommand
	{
		/// <summary>Used by BVE to allow for debugging, unused by OpenBVE</summary>
		DeveloperID,
		/// <summary>A textual description of the route to be displayed in the main menu</summary>
		Comment,
		/// <summary>An image of the route to be displayed in the main menu</summary>
		Image,
		/// <summary>The timetable image to be displayed in-cab</summary>
		TimeTable,
		/// <summary>The mode for thew train's safety system to start in</summary>
		Change,
		/// <summary>The rail gauge</summary>
		Gauge,
		/// <summary>Sets a speed limit for each signal aspect</summary>
		Signal,
		/// <summary>The acceleration due to gravity</summary>
		AccelerationDueToGravity,
		/// <summary>The game starting time</summary>
		StartTime,
		/// <summary>Sets the background to be displayed on loading screens</summary>
		LoadingScreen,
		/// <summary>Sets a custom unit of speed to be displayed in in-game messages</summary>
		DisplaySpeed,
		/// <summary>Sets briefing data</summary>
		Briefing,
		/// <summary>Sets the initial elevation above sea-level</summary>
		Elevation,
		/// <summary>Sets the initial air temperature</summary>
		Temperature,
		/// <summary>Sets the initial air pressure</summary>
		Pressure,
		/// <summary>Sets the ambient light color</summary>
		AmbientLight,
		/// <summary>Sets the directional light color</summary>
		DirectionalLight,
		/// <summary>Sets the position of the directional light</summary>
		LightDirection,
		/// <summary>Adds dynamic lighting</summary>
		DynamicLight,
		/// <summary>Sets the initial viewpoint for the camera</summary>
		InitialViewPoint,
		/// <summary>Adds AI trains</summary>
		TfoXML
	}
}
