namespace CsvRwRouteParser
{
	internal class Form
	{
		internal Form(int primaryRail, int secondaryRail, int formType, int roofType)
		{
			PrimaryRail = primaryRail;
			SecondaryRail = secondaryRail;
			FormType = formType;
			RoofType = roofType;
		}
		internal readonly int PrimaryRail;
		internal readonly int SecondaryRail;
		internal readonly int FormType;
		internal readonly int RoofType;
		internal const int SecondaryRailStub = 0;
		internal const int SecondaryRailL = -1;
		internal const int SecondaryRailR = -2;
	}
}
