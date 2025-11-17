namespace CsvRwRouteParser
{
	/// <summary>The commands in the Track namespace</summary>
	internal enum TrackCommand
	{
		/// <summary>Controls the position and type of a rail</summary>
		Rail,
		/// <summary>Starts a rail</summary>
		RailStart,
		/// <summary>Ends a rail</summary>
		RailEnd,
		/// <summary>Changes the RailType of a rail</summary>
		RailType,
		/// <summary>Changes the accuracy (bounce / spread) value</summary>
		Accuracy,
		/// <summary>Changes the pitch of Rail 0</summary>
		Pitch,
		/// <summary>Changes the curve of Rail 0</summary>
		Curve,
		/// <summary>Adds a turn on Rail 0</summary>
		Turn,
		/// <summary>Changes the adhesion value of Rail 0</summary>
		Adhesion,
		/// <summary>Changes the overall brightness value</summary>
		Brightness,
		/// <summary>Controls a region of fog</summary>
		Fog,
		/// <summary>Adds a signalling section</summary>
		Section,
		/// <summary>Adds a signalling section</summary>
		SectionS,
		/// <summary>Adds a permissive signalling section</summary>
		/// <remarks>Must be value based</remarks>
		SectionP,
		/// <summary>Adds a signal object</summary>
		SigF,
		/// <summary>Adds a signal object</summary>
		Signal,
		/// <summary>Adds a signal object</summary>
		Sig,
		/// <summary>Adds a relay</summary>
		Relay,
		/// <summary>Adds a destination change event</summary>
		Destination,
		/// <summary>Adds a beacon</summary>
		Beacon,
		/// <summary>Adds a transponder</summary>
		Transponder,
		/// <summary>Adds a transponder</summary>
		Tr,
		/// <summary>Adds an ATS-SN transponder</summary>
		ATSSn,
		/// <summary>Adds an ATS-P transponder</summary>
		ATSP,
		/// <summary>Adds an ATS-P pattern transponder</summary>
		Pattern,
		/// <summary>Adds an ATS-P limit transponder</summary>
		PLimit,
		/// <summary>Adds a speed limit</summary>
		Limit,
		/// <summary>Adds a station stop point</summary>
		Stop,
		/// <summary>Adds a station</summary>
		Sta,
		/// <summary>Adds a station</summary>
		Station,
		/// <summary>Adds a station defined in an external XML file</summary>
		StationXML,
		/// <summary>Adds a buffer stop to Rail 0</summary>
		Buffer,
		/// <summary>Adds a platform</summary>
		Form,
		/// <summary>Starts OHLE poles on the selected rail(s)</summary>
		Pole,
		/// <summary>Ends OHLE poles on the selected rail(s)</summary>
		PoleEnd,
		/// <summary>Starts a wall on the selected rail</summary>
		Wall,
		/// <summary>Ends a wall on the selected rail</summary>
		WallEnd,
		/// <summary>Starts a dike on the selected rail</summary>
		Dike,
		/// <summary>Ends a dike on the selected rail</summary>
		DikeEnd,
		/// <summary>Adds a graphical marker</summary>
		Marker,
		/// <summary>Adds a textual marker</summary>
		TextMarker,
		/// <summary>Changes the height of Rail 0 above the ground</summary>
		Height,
		/// <summary>Changes the ground object</summary>
		Ground,
		/// <summary>Creates a crack fill object between two rails</summary>
		Crack,
		/// <summary>Adds a FreeObject</summary>
		FreeObj,
		/// <summary>Changes the displayed background</summary>
		Back,
		/// <summary>Changes the displayed background</summary>
		Background,
		/// <summary>Adds an announcement played in the cab</summary>
		Announce,
		/// <summary>Adds an announcement played in all cars</summary>
		AnnounceAll,
		/// <summary>Adds a doppler sound played in the cab</summary>
		Doppler,
		/// <summary>Adds a doppler sound played in all cars</summary>
		DopplerAll,
		/// <summary>Adds a reigon where an announcement can be played in-game using the microphone</summary>
		MicSound,
		/// <summary>Controls the calling time of the PreTrain</summary>
		PreTrain,
		/// <summary>Adds a PointOfInterest</summary>
		PointOfInterest,
		/// <summary>Adds a PointOfInterest</summary>
		POI,
		/// <summary>Adds a horn blow event</summary>
		HornBlow,
		/// <summary>Sets the rain intensity</summary>
		Rain,
		/// <summary>Sets the snow intensity</summary>
		Snow,
		/// <summary>Changes the ambient lighting</summary>
		/// <remarks>Ignored when dynamic lighting is active</remarks>
		AmbientLight,
		/// <summary>Changes the directional light</summary>
		/// <remarks>Ignored when directional lighting is active</remarks>
		DirectionalLight,
		/// <summary>Changes the light direction</summary>
		/// <remarks>Ignored when directional lighting is active</remarks>
		LightDirection,
		/// <summary>Changes the dynamic lighting set in use</summary>
		DynamicLight,
		/// <summary>Creates a facing switch</summary>
		Switch,
		/// <summary>Creates a trailing switch</summary>
		SwitchT,
		/// <summary>Sets the speed limit for a rail</summary>
		RailLimit,
		/// <summary>Adds a buffer stop</summary>
		RailBuffer,
		/// <summary>Sets the accuracy value for a rail</summary>
		RailAccuracy,
		/// <summary>Sets the adhesion value for a rail</summary>
		RailAdhesion,
		/// <summary>Adds an announcement played in the cab on a non-zero rail</summary>
		RailAnnounce,
		/// <summary>Adds an announcement played in all cars on a non-zero rail</summary>
		RailAnnounceAll,
		/// <summary>Changes the player path</summary>
		PlayerPath,
		/// <summary>Adds or changes the properties of a power supply</summary>
		PowerSupply,
		/// <summary>Ends a power supply</summary>
		PowerSupplyEnd,
		/*
		 * HMMSIM
		 */
		/// <summary>Adds a station stop point</summary>
		/// <remarks>Eqivilant of .Stop</remarks>
		StopPos,
		/// <summary>Starts a repeating object cycle</summary>
		PatternObj,
		/// <summary>Ends a repeating object cycle</summary>
		PatternEnd
	}
}
