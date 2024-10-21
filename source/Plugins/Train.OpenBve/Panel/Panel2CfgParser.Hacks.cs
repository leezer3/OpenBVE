namespace Train.OpenBve
{
	internal partial class Panel2CfgParser
	{
		/// <summary>Applies various hacks to 'fix' some broken trains</summary>
		internal void ApplyGlobalHacks()
		{
			if (!Plugin.CurrentOptions.EnableBveTsHacks)
			{
				return;
			}
			switch ((int)PanelRight)
			{
				case 1696:
					if (PanelResolution == 1024 && trainName == "TOQ2000CN1EXP10" || trainName == "TOQ8500CS8EXP10")
					{
						PanelRight = 1024;
					}
					break;
			}

			switch ((int)PanelCenter.Y)
			{
				case 180:
					switch (trainName.ToUpperInvariant())
					{
						case "LT_C69_77":
						case "LT_C69_77_V2":
							// Broken initial zoom
							PanelCenter.Y = 350;
							break;
					}
					break;
				case 200:
					switch (trainName.ToUpperInvariant())
					{
						case "HM05":
							// Broken initial zoom
							PanelCenter.Y = 350;
							break;
					}
					break;
				case 229:
					if (PanelBottom == 768 && PanelResolution == 1024)
					{
						// Martin Finken's BVE4 trams: Broken initial zoom
						PanelCenter.Y = 350;
					}
					break;
				case 255:
					if (PanelBottom == 1024 && PanelResolution == 1024)
					{
						switch (trainName.ToUpperInvariant())
						{
							case "PARIS_MF67":
							case "PARIS_MF88":
							case "PARIS_MP73":
							case "PARIS_MP89":
							case "PARIS_MP89AUTO":
							case "LT1938":
							case "LT1973 UNREFURB":
								// Broken initial zoom
								PanelCenter.Y = 350;
								break;
							case "LT_A60_62":
							case "LT1972 MKII":
								// Broken initial zoom and black patch at bottom of panel
								PanelCenter.Y = 350;
								PanelBottom = 792;
								break;
						}
					}
					break;
				case 483:
					switch (trainName.ToUpperInvariant())
					{
						case "[HOSHIRAIL] KCIC CR400-AF":
							PanelCenter.X = 517;
							PanelCenter.Y = 300;
							break;
					}
					break;
			}

			switch (trainName)
			{
				case "8171BETA":
					if (PanelResolution == 768 && PanelOrigin.Y == 256)
					{
						// 81-71: Bust panel origin means a flying cab....
						PanelOrigin.Y = 0;
					}
					break;
				case "[HOSHIRAIL] KCIC CR400-AF":
					if (PanelResolution == 826 && PanelOrigin.X == 350)
					{
						PanelOrigin.X = 517;
						PanelOrigin.Y = 300;
					}
					break;
			}
		}
	}
}
