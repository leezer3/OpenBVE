namespace OpenBveApi
{
	/// <summary>The sections of an options file</summary>
	public enum OptionsSection
	{
		/// <summary>Unknown section</summary>
		Unknown = 0,
		/// <summary>Contains language options</summary>
		Language = 1,
		/// <summary>Contains UI options</summary>
		Interface = 2,
		/// <summary>Contains display options</summary>
		Display = 3,
		/// <summary>Contains graphics quality options</summary>
		Quality = 4,
		/// <summary>Contains object optimization options</summary>
		ObjectOptimization = 5,
		/// <summary>Contains object optimization options</summary>
		Simulation = 6,
		/// <summary>Contains control settings</summary>
		Controls = 7,
		/// <summary>Contains control settings</summary>
		Keys = 7,
		/// <summary>Contains sound options</summary>
		Sound = 8,
		/// <summary>Contains error logging options</summary>
		Verbosity = 9,
		/// <summary>Contains folder search options</summary>
		Folders = 10,
		/// <summary>Contains package install and creation options</summary>
		Packages = 11,
		/// <summary>Contains the list of recently used routes</summary>
		RecentlyUsedRoutes = 12,
		/// <summary>Contains the list of recently used trains</summary>
		RecentlyUsedTrains = 13,
		/// <summary>Contains the list of encodings for the recently used routes</summary>
		RouteEncodings = 14,
		/// <summary>Contains the list of encodings for the recently used trains</summary>
		TrainEncodings = 15,
		/// <summary>Enables input device plugins</summary>
		EnableInputDevicePlugins = 16,
		/// <summary>Contains parser options</summary>
		Parsers = 17,
		/// <summary>Contains touch control options</summary>
		Touch = 18,
		/// <summary>Contains loading related options</summary>
		Loading = 19
	}
}
