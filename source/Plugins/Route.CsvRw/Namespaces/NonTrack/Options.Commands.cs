namespace CsvRwRouteParser
{
	internal enum OptionsCommand
	{
		/// <summary>Sets the length of a block</summary>
		/// <remarks>Grounds, walls, dikes & poles repeat once per block</remarks>
		BlockLength,
		/// <summary>Controls the X Object parser in use</summary>
		XParser,
		/// <summary>Controls the Wavefront OBJ parser in use</summary>
		ObjParser,
		/// <summary>Sets the unit of length relative to 1m</summary>
		UnitOfLength,
		/// <summary>Sets the unit of speed relative to 1km/h</summary>
		UnitOfSpeed,
		/// <summary>Controls how objects are disposed after the camera passes their point of origin</summary>
		ObjectVisibility,
		/// <summary>Controls the behaviour of signalling sections</summary>
		SectionBehavior,
		/// <summary>Controls whether cant is expected to be signed</summary>
		CantBehavior,
		/// <summary>Controls the blending mode used for fog</summary>
		FogBehavior,
		/// <summary>Controls whether BVETS Hacks are forced on / off for this route</summary>
		EnableBveTsHacks,
		/// <summary>Controls whether the route is driven in the reverse direction to construction</summary>
		ReverseDirection,
	}
}
