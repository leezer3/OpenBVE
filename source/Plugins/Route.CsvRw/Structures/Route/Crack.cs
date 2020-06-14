namespace CsvRwRouteParser
{
	internal class Crack
	{
		internal Crack(int primaryRail, int secondaryRail, int type)
		{
			PrimaryRail = primaryRail;
			SecondaryRail = secondaryRail;
			Type = type;
		}

		internal readonly int PrimaryRail;
		internal readonly int SecondaryRail;
		internal readonly int Type;
	}
}
