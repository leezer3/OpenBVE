using System.IO;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private static void CheckRouteSpecificFixes(string FileName, ref RouteData Data, ref Expression[] Expressions)
		{
			if (Interface.CurrentOptions.EnableBveTsHacks == false)
			{
				return;
			}
			FileInfo f = new FileInfo(FileName);
			switch (f.Length)
			{
				case 63652:
					//Jundiai-Francisco Morato.rw
					if (Game.RouteComment == "Jundiai-Francisco Morato\nExtensão Operacional Linha A\nCPTM - Cia. Paulista de Trens Metropolitanos")
					{
						Interface.AddMessage(Interface.MessageType.Warning, false, "Jundiai - Francisco Morato routefile detected- Applying fix to line endings.");
						Data.LineEndingFix = true;
					}
					break;
				case 67729:
					//kurra_fine1.csv
					if (Game.RouteComment == "Kurrajong Line, 1963\r\nLocal\r\n2 Cars\r\nRichmond - Kurrajong\r\n\r\n(C) 2001 Spot")
					{
						Data.IgnorePitchRoll = true;
						Interface.AddMessage(Interface.MessageType.Warning, false, "Richmond- Kurrajong routefile detected- Applying fix to yaw / roll.");
					}
					break;
				case 70625:
					//FVES3.rw
					if (Game.RouteComment == "Linie S3 der FVE\nFarge - Vegesack\nHöchstgeschwindigkeit\nder Strecke 80Km/h\nvon Hans-Martin Finken")
					{
						Data.IgnorePitchRoll = true;
						Interface.AddMessage(Interface.MessageType.Warning, false, "Linie S3 (FVE) routefile detected- Applying fix to yaw / roll.");
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
				case 86723:
					//Zwolle-Vlissingen.rw
					if (Game.RouteComment == "Zwolle - Vlissingen \n(Part Two of Groningen-Vlissingen)\nEarly Evening Express")
					{
						Data.IgnorePitchRoll = true;
						Interface.AddMessage(Interface.MessageType.Warning, false, "Zwolle - Vlissingen routefile detected- Applying fix to yaw / roll.");
					}
					break;
				case 14297:
					//目蒲線普.csv
					//Trackwork on exit to second station is broken without this
					if (Game.RouteComment == "東急目蒲線\r\n奥沢-多摩川園\r\nver1.01\r\n\r\n(C)2004　こば")
					{
						if (Expressions[596].Text == ".rail 1;7.5:0")
						{
							Expressions[596].Text = ".rail 1;7.5;0";
						}

						if (Expressions[600].Text == ".rail 1;5.5:0")
						{
							Expressions[600].Text = ".rail 1;5.5;0";
						}
					}
					break;
			}
		}
	}
}
