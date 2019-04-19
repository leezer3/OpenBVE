using OpenBveApi.Interface;

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
			string fileHash = GetChecksum(FileName);
			switch (fileHash)
			{
				case "F0D6AC84D94F63144F9ED5497CDF7697BDB45FF11223E2C001CF8BDA943D4E66":
					//Jundiai-Francisco Morato.rw
					Interface.AddMessage(MessageType.Warning, false, "Jundiai - Francisco Morato routefile detected- Applying fix to line endings.");
					Data.LineEndingFix = true;
					break;
				case "7C21D03D487E36CCA2D9D1003732614BEEF682E421CB861E8632418CCD8D9D41":
					//kurra_fine1.csv
					Data.IgnorePitchRoll = true;
					Interface.AddMessage(MessageType.Warning, false, "Richmond- Kurrajong routefile detected- Applying fix to yaw / roll.");
					break;
				case "FD99B78D5A1847070A3ED3DFEE3E3B6BD56CE578DE1CEB056AFDA799989BF14B":
					//FVES3.rw
					Data.IgnorePitchRoll = true;
					Interface.AddMessage(MessageType.Warning, false, "Linie S3 (FVE) routefile detected- Applying fix to yaw / roll.");
					break;
				case "FF2B19C253C09CB541E57AB144CE67792E069F6FC2022D918ED7CBD9B59A1994":
					//kurrajong.csv
					Data.IgnorePitchRoll = true;
					Interface.AddMessage(MessageType.Warning, false, "Richmond- Kurrajong routefile detected- Applying fix to yaw / roll.");
					break;
				case "6BFDD2746C56A64FCB5BE116C64530FFA0AD7C2889B8F77DCFFBFAD526070704":
					//camden_17.csv
					Data.IgnorePitchRoll = true;
					Interface.AddMessage(MessageType.Warning, false, "Campbelltown- Camden routefile detected- Applying fix to yaw / roll.");
					break;
				case "83EE400BA3A9FE0112AD5146D12968B8BE981B28E5D27449027EFBBB6583B68A":
					//Zwolle-Vlissingen.rw
					Data.IgnorePitchRoll = true;
					Interface.AddMessage(MessageType.Warning, false, "Zwolle - Vlissingen routefile detected- Applying fix to yaw / roll.");
					break;
				case "54281BEA1964A11925E3B1E9F6CF8DBFF39156CBE6272977150A2B7F08799DD1":
				case "D8B88EE63CF98D271EC8A75577539752E84A8B74A46EF1B49D57FCA6A53BDBB4":
				case "FD63747CDD10E501D68E7A3FAB193B191F818CE950FB714B2FF79439A11AD427":
				case "661C2E3EEE663192529A761631F02C0F0288340DCF69AB80622CA7E1B37BB126":
				case "18A7BD8BB969E79D140112C4246B62EED8111A82509720C466174248674B20B6":
				case "A29A91698F03A3C0F258A4E6C6DC72C4A15F3828849C929F25AF67FD0747A3FF":
					//Sanbie-663-bve4.csv
					//Sanbie-663-nonstop-bve4.csv
					//Sanbie-663-rain-nonstop-bve4.csv
					//Sanbie-773-bve4.csv
					//Sanbie-773-nonstop-bve4.csv
					//Sanbie-773-rain-nonstop-bve4.csv
					Data.IgnorePitchRoll = true;
					Interface.AddMessage(MessageType.Warning, false, "Sanbie routefile detected- Applying fix to yaw / roll.");
					break;
				case "DDBE5CFDE20F0AD7D03AFC187F70B1B6044B637109758B392B9EBA61FA169F69":
					//目蒲線普.csv
					Expressions[596].Text = ".rail 1;7.5;0";
					Expressions[600].Text = ".rail 1;5.5;0";
					break;
				case "2654DAF8B0FEEAAB928C8CC9E1D68279E361C7C2150D82860CCBB31DFD48A7C8":
				case "D2382ED7D67DF6B9D6F32E8A89B1C21A7689EA18AEADED6BFE8EFE53BD78D2BD":
				case "9062F55797622F66E2FED2C7E2A6EB6F661F6C8C6EE89A249AEAC0E6CB6B1FF9":
				case "C2AC8A277D08AED9505A339759EDD5BBB0E4C541E0226EFFD8615DCBDCE8A446":
				case "1175ABDC20934050DBF3DF6E186874D9A7315358235D2CE4CC3096512BF17514":
				case "84F32BB8CFAB5D63E611C261822A55F22AE444401A04B826E2529449E3431728":
					//Elephant to Harrow 1972Mk2TS.csv
					//Elephant to Queen's Park 1972Mk2TS.csv
					//Queen's Park to Harrow SILVERLINK Cl313.csv
					//Followed by BVE4 versions, same filenames
					CylinderHack = true;
					Interface.AddMessage(MessageType.Warning, false, "Bakerloo v3 routefile detected- Applying cylinder hack.");
					break;
				case "AA6528402BE457A20DF77A8B7CAFBC2580F3F8326BB56B9A691A4558174AE152":
				case "2EB087770AEC2C6A0F2AADC0A8D531117502B1AB33E94CBBC11940DA4FFF4A30":
					//V.2.1.1 Aldwych BVE4- no fog.csv
					//V.2.1.1 Aldwych BVE4.csv
					CylinderHack = true;
					Interface.AddMessage(MessageType.Warning, false, "Aldwych v2.1.1 routefile detected- Applying cylinder hack.");
					break;
				case "9D87539BAC426DE5B1ECB638A935EF8AC37B31AD6B16D645B1D3F2A7E2D1B23F":
				case "078324311EC9048F313513849B5EBDCF3C3CAF193520F947258324FEDDD6A2BD":
				case "939E18B5AB5645FF25CF5C4CE72D0F0567C9A0C9ABDF6D21AB5DC3278D180032":
				case "3E673AAD446439A42FA81AA37D6C51FE69DC32C93D2FDC1EE3B8DA73EDC61B81":
				case "60F9CF7F40E21808072DAEF3F316ECB0BEA2AA224F5AD414BCA084F6AE0BD1E8":
				case "179EB3C4F653DBC8FF98CF1F6F44693DE5AE6EB8A196CE89D2B5681BA6D972EB":
					//Barons Ct to Arnos 38ts.csv
					//Barons Ct to Arnos Peak.csv
					//Barons Ct to Arnos Weedkill.csv
					//Barons Ct to Arnos.csv
					//Barons Ct to Wood Green.csv
					//Hyde Pk Cnr to Wood Green.csv
					CylinderHack = true;
					Interface.AddMessage(MessageType.Warning, false, "Picadilly v5.2 routefile detected- Applying cylinder hack.");
					break;
				case "C4061E0E53862BA4951A76746B2AB1AFA3A7D4126F375D5EF96E4DEBDFC969EF":
					//citta2.csv
					Expressions[737].Text = "2550";
					Expressions[767].Text = "2925";
					break;
				case "CCF88CCB0C1345746406F7E98798D8924FDA93888788E596601C55E3C7C35C8A":
					//B1649ﾃｼiﾄ・ﾄｺJﾃｼﾄ佚架・ｹﾃ嘉ｼj.csv
					Expressions[1441].Text = ".curve 1000;20";
					break;
				case "BBB3E085320FDC847A36E80C34A6820EEE4E7A76F9FCDC2F411911B61E0CE222":
				case "48B0870AEEA7D8779AFF77E06D0C43D01CDC6B1C2A2ED822055CFE84DAC2ABB0":
				case "72108E9FBA951DEB47E18CFE11F336145E4E7C3DAA0BD0C2F3911B509254D13A":
					//Amsterdam-Schiphol.rw
					//Amsterdam-Antwerpen-v3.rw
					//Amsterdam-Antwerpen (Thalys PBKA).rw
					Data.IgnorePitchRoll = true;
					break;
				case "FBF4399E04B769BDD416CEB6CC3D3C5EC4803E158997D5D75A76C423B6DA45ED":
				case "49630A77BE1AB513BD58962795DC38846451A3E44C4E6770DF78EE71C5EF226E":
					//737 Test Flight-
					//Arrive Night.csv
					//Arrive.csv
					Interface.CurrentOptions.Derailments = false;
					break;
				case "471C1CC06E55D1B40E7B992028FA25200E480A37778E51B1B0F36BE475B250AF":
					//Iiyama 2060-
					//9001.csv
					Expressions[434].Text = ".section 0; 103; 104; 105; 106";
					if (Interface.CurrentOptions.CurrentXParser == Interface.XParsers.Original)
					{
						//Various broken stuff with the original parser, either Assimp or new will do
						Interface.CurrentOptions.CurrentXParser = Interface.XParsers.NewXParser;
					}
					break;
				case "B97E93C1A21B43CCDA2C504F254E6F8FF099FD51E502E2EE875E6B05FBDE5326":
					//NYCT-L.csv
					Interface.CurrentOptions.Toppling = false;
					break;
				case "B44150CA3636B904E2A0058902CA739DE3F7B24D5D551C7FFD53597F3157FB79":
					//M4.csv
					Expressions[206].Text = string.Empty;
					Expressions[207].Text = string.Empty;
					Expressions[208].Text = string.Empty;
					break;
				case "5E976BA173F3267271746FB161730FFC205FAC85A38DB369BDC2FB25297B05E1":
					//spaceroute1.csv
					Data.IgnorePitchRoll = true;
					Interface.CurrentOptions.Derailments = false;
					Interface.CurrentOptions.Toppling = false;
					break;
				case "64F6EECA5B5976F271DC9686C585026A1A5732E743A1BA1DB6DC57C6672E9F3B":
				case "FEFAF4332990CDDBC8F563010B539320AD00FB49CC6EFB14546FE30A65752752":
					//TR_203_A6.rw
					//TR_3_A5.rw
					Expressions[803].Text = "Freeobj(0,50,0,0,168.075)";
					break;
				case "E734848C90A964904907AB3FCFAB2BE01F352D44B5D3C66A65E9A28AB70B920B":
					//TR_207_A9.rw
					Expressions[802].Text = "Freeobj(0,50,0,0,168.075)";
					break;
				case "C49103E985BD256ACC38E8373652F9B963F2AA7144FE66CCAC769FDD16053B79":
					//TR_217_A1.rw
					Expressions[799].Text = "Freeobj(0,50,0,0,168.075)";
					break;
				case "AFC9586954ED7867860EB167A1CE2718D9D49AA9AE4905B2A24E49F49295F07D":
					//TR_3_A11.csv
					Expressions[1316].Text = "5025,";
					break;
				case "A2FA01331674992B4E1AB935F496590B4CD7D083BF659109C28B0BF603AB93DB":
					//TR_207_A1.csv
					Expressions[1329].Text = "5025,";
					break;
				case "9EC666169C5B5E1AFFEAD1C7753FD67F54632E51C7FB64450D4B9A0329CFF9F9":
					//TR_207_A9.csv
					Expressions[1323].Text = "5025,";
					break;
				case "097B3ACC33FA9CB33A9D35023EE24F977657875D9C79DCFE1CC8655A2EEB67CA":
					//TR_spec_L29.csv
					Expressions[1318].Text = "5025,";
					break;
			}
		}
	}
}
