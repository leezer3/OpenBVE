namespace CsvRwRouteParser
{
	/// <summary>Holds the base structures for a route: These are cloned and transformed for final world placement</summary>
	internal class StructureData
	{
		/// <summary>All currently defined Structure.Rail objects</summary>
		internal ObjectDictionary RailObjects;
		/// <summary>All currently defined Structure.Pole objects</summary>
		internal PoleDictionary Poles;
		/// <summary>All currently defined Structure.Ground objects</summary>
		internal ObjectDictionary Ground;
		/// <summary>All currently defined Structure.WallL objects</summary>
		internal ObjectDictionary WallL;
		/// <summary>All currently defined Structure.WallR objects</summary>
		internal ObjectDictionary WallR;
		/// <summary>All currently defined Structure.DikeL objects</summary>
		internal ObjectDictionary DikeL;
		/// <summary>All currently defined Structure.DikeR objects</summary>
		internal ObjectDictionary DikeR;
		/// <summary>All currently defined Structure.FormL objects</summary>
		internal ObjectDictionary FormL;
		/// <summary>All currently defined Structure.FormR objects</summary>
		internal ObjectDictionary FormR;
		/// <summary>All currently defined Structure.FormCL objects</summary>
		internal ObjectDictionary FormCL;
		/// <summary>All currently defined Structure.FormCR objects</summary>
		internal ObjectDictionary FormCR;
		/// <summary>All currently defined Structure.RoofL objects</summary>
		internal ObjectDictionary RoofL;
		/// <summary>All currently defined Structure.RoofR objects</summary>
		internal ObjectDictionary RoofR;
		/// <summary>All currently defined Structure.RoofCL objects</summary>
		internal ObjectDictionary RoofCL;
		/// <summary>All currently defined Structure.RoofCR objects</summary>
		internal ObjectDictionary RoofCR;
		/// <summary>All currently defined Structure.CrackL objects</summary>
		internal ObjectDictionary CrackL;
		/// <summary>All currently defined Structure.CrackR objects</summary>
		internal ObjectDictionary CrackR;
		/// <summary>All currently defined Structure.FreeObj objects</summary>
		internal ObjectDictionary FreeObjects;
		/// <summary>All currently defined Structure.Beacon objects</summary>
		internal ObjectDictionary Beacon;
		/// <summary>All currenly defined Structure.Weather objects</summary>
		internal ObjectDictionary WeatherObjects;
		/// <summary>All currently defined cycles</summary>
		internal int[][] Cycles;
		/// <summary>All currently defined RailCycles</summary>
		internal int[][] RailCycles;
		/// <summary>The Run sound index to be played for each railtype idx</summary>
		internal int[] Run;
		/// <summary>The flange sound index to be played for each railtype idx</summary>
		internal int[] Flange;
	}
}
