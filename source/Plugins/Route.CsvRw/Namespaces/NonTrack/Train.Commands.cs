namespace CsvRwRouteParser
{
	internal enum TrainCommand
	{
		/// <summary>Sets the interval between preceding AI trains</summary>
		Interval,
		/// <summary>Sets the max speed of an AI train</summary>
		Velocity,
		/// <summary>Sets the folder used for the player train</summary>
		Folder,
		/// <summary>Sets the folder used for the player train</summary>
		File,
		/// <summary>Sets the run sound played for Rail with the structure index N</summary>
		Run,
		/// <summary>Sets the run sound played for Rail with the structure index N</summary>
		Rail,
		/// <summary>Sets the flange sound played for Rail with the structure index N</summary>
		Flange,
		/// <summary>Sets the daytime timetable image</summary>
		TimetableDay,
		/// <summary>Sets the nighttime timetable image</summary>
		TimetableNight,
		/// <summary>Sets the initial destination value</summary>
		Destination,
		/*
		 * RW commands, currently unsupported
		 */
		/// <summary>Sets the stopping frequency of the train in front</summary>
		/// <remarks>Unsure as to effects at present</remarks>
		Station,
		/// <summary>Sets the acceleration of the train in front</summary>
		/// <remarks>Appears to be a constant acceleration value at all times.
		/// As we simulate the train fully, it's not really useful.</remarks>
		Acceleration,
		/// <summary>Contains a URL where the train may be downloaded if not currently possessed by the player</summary>
		DownloadLocation
	}
}
