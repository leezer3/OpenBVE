using System.IO;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void CheckRouteSpecificFixes(string FileName, ref RouteData Data)
		{
			if (Interface.CurrentOptions.EnableBveTsHacks == false)
			{
				return;
			}
			FileInfo f = new FileInfo(FileName);
			switch (f.Length)
			{
				case 67729:
					//kurra_fine1.csv
					if (Game.RouteComment == "Kurrajong Line, 1963\r\nLocal\r\n2 Cars\r\nRichmond - Kurrajong\r\n\r\n(C) 2001 Spot")
					{
						Data.IgnorePitchRoll = true;
						Interface.AddMessage(Interface.MessageType.Warning, false, "Richmond- Kurrajong routefile detected- Applying fix to yaw / roll.");
					}
					break;
				case 73262:
					//kurrajong.csv
					if (Game.RouteComment == "Kurrajong Line, 1953\r\nLocal\r\n2 Cars\r\nRichmond - Kurrajong\r\n\r\n(C) 2001 Spot")
					{
						Data.IgnorePitchRoll = true;
						Interface.AddMessage(Interface.MessageType.Warning, false, "Richmond- Kurrajong routefile detected- Applying fix to yaw / roll.");
					}
					break;
				case 75400:
					//camden_17.csv
					if (Game.RouteComment == "Camden Line, 1963\r\nLocal\r\n2 Cars\r\nCampbelltown - Camden\r\n\r\n(C) 2001 Spot")
					{
						Data.IgnorePitchRoll = true;
						Interface.AddMessage(Interface.MessageType.Warning, false, "Campbelltown- Camden routefile detected- Applying fix to yaw / roll.");
					}
					break;
			}
		}
	}
}
