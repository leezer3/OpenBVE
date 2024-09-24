using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>Gets the formatted text for an in-game score event</summary>
		/// <param name="TextToken">The in-game score event</param>
		internal static string GetScoreText(ScoreEventToken TextToken) {
			switch (TextToken) {
					case ScoreEventToken.Overspeed: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","overspeed"});
					case ScoreEventToken.PassedRedSignal: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","redsignal"});
					case ScoreEventToken.Toppling: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","toppling"});
					case ScoreEventToken.Derailed: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","derailed"});
					case ScoreEventToken.PassengerDiscomfort: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","discomfort"});
					case ScoreEventToken.DoorsOpened: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","doors"});
					case ScoreEventToken.ArrivedAtStation: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_arrived"});
					case ScoreEventToken.PerfectTimeBonus: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_perfecttime"});
					case ScoreEventToken.Late: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_late"});
					case ScoreEventToken.PerfectStopBonus: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_perfectstop"});
					case ScoreEventToken.Stop: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_stop"});
					case ScoreEventToken.PrematureDeparture: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_departure"});
					case ScoreEventToken.Total: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_total"});
					default: return "?";
			}
		}

		/// <summary>Exports the current score data to a file</summary>
		/// <param name="File">The file to write</param>
		internal static void ExportScore(string File) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			string[][] Lines = new string[Game.ScoreLogCount + 1][];
			Lines[0] = new[] {
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","time"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","position"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","value"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","cumulative"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","reason"})
			};
			int Columns = Lines[0].Length;
			int TotalScore = 0;
			for (int i = 0; i < Game.ScoreLogCount; i++) {
				int j = i + 1;
				Lines[j] = new string[Columns];
				{
					double x = Game.ScoreLogs[i].Time;
					int h = (int)Math.Floor(x / 3600.0);
					x -= h * 3600.0;
					int m = (int)Math.Floor(x / 60.0);
					x -= m * 60.0;
					int s = (int)Math.Floor(x);
					Lines[j][0] = h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture);
				}
				Lines[j][1] = Game.ScoreLogs[i].Position.ToString("0", Culture);
				Lines[j][2] = Game.ScoreLogs[i].Value.ToString(Culture);
				TotalScore += Game.ScoreLogs[i].Value;
				Lines[j][3] = TotalScore.ToString(Culture);
				Lines[j][4] = GetScoreText(Game.ScoreLogs[i].TextToken);
			}
			int[] Widths = new int[Columns];
			for (int i = 0; i < Lines.Length; i++) {
				for (int j = 0; j < Columns; j++) {
					if (Lines[i][j].Length > Widths[j]) {
						Widths[j] = Lines[i][j].Length;
					}
				}
			}
			{ // header rows
				int TotalWidth = 0;
				for (int j = 0; j < Columns; j++) {
					TotalWidth += Widths[j] + 2;
				}
				TotalWidth += Columns - 1;
				Builder.Append('╔');
				Builder.Append('═', TotalWidth);
				Builder.Append("╗\n");
				{
					Builder.Append('║');
					Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","route"}) + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","train"}) + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","date"}) + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
					Builder.Append("║\n");
				}
				Builder.Append('╠');
				Builder.Append('═', TotalWidth);
				Builder.Append("╣\n");
				{
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : Game.CurrentScore.CurrentValue / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * Translations.RatingsCount);
					if (index >= Translations.RatingsCount) index = Translations.RatingsCount - 1;
					string s;
					switch (Interface.CurrentOptions.PreviousGameMode) {
							case GameMode.Arcade: s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","arcade"}); break;
							case GameMode.Normal: s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","normal"}); break;
							case GameMode.Expert: s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","expert"}); break;
							default: s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","unknown"}); break;
					}
					Builder.Append('║');
					Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","mode"}) + " " + s).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","score"}) + " " + Game.CurrentScore.CurrentValue.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture)).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"log","rating"}) + " " + Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"rating" , index.ToString(Culture)}) + " (" + (100.0 * ratio).ToString("0.00") + "%)").PadRight(TotalWidth, ' '));
					Builder.Append("║\n");
				}
			}
			{ // top border row
				Builder.Append('╠');
				for (int j = 0; j < Columns; j++) {
					if (j != 0) {
						Builder.Append('╤');
					} Builder.Append('═', Widths[j] + 2);
				} Builder.Append("╣\n");
			}
			for (int i = 0; i < Lines.Length; i++) {
				// center border row
				if (i != 0) {
					Builder.Append('╟');
					for (int j = 0; j < Columns; j++) {
						if (j != 0) {
							Builder.Append('┼');
						} Builder.Append('─', Widths[j] + 2);
					} Builder.Append("╢\n");
				}
				// cell content
				Builder.Append('║');
				for (int j = 0; j < Columns; j++) {
					if (j != 0) Builder.Append('│');
					Builder.Append(' ');
					if (i != 0 & j <= 3) {
						Builder.Append(Lines[i][j].PadLeft(Widths[j], ' '));
					} else {
						Builder.Append(Lines[i][j].PadRight(Widths[j], ' '));
					}
					Builder.Append(' ');
				} Builder.Append("║\n");
			}
			{ // bottom border row
				Builder.Append('╚');
				for (int j = 0; j < Columns; j++) {
					if (j != 0) {
						Builder.Append('╧');
					} Builder.Append('═', Widths[j] + 2);
				} Builder.Append('╝');
			}
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}
	}
}
