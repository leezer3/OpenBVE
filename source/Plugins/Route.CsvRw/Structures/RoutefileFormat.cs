namespace CsvRwRouteParser
{
	/// <summary>The supported routefile formats by this plugin</summary>
	internal enum RoutefileFormat
	{
		/// <summary>CSV</summary>
		CSV = 0,
		/// <summary>Legacy BVE2 RW</summary>
		RW = 1,
		/// <summary>Hmmsim format CSV</summary>
		Hmmsim = 2
	}
}
