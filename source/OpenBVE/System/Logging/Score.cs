using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>Gets the formatted text for an in-game score event</summary>
		/// <param name="textToken">The in-game score event</param>
		internal static string GetScoreText(Game.ScoreTextToken textToken) {
			switch (textToken) {
					case Game.ScoreTextToken.Overspeed: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","overspeed"});
					case Game.ScoreTextToken.PassedRedSignal: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","redsignal"});
					case Game.ScoreTextToken.Toppling: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","toppling"});
					case Game.ScoreTextToken.Derailed: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","derailed"});
					case Game.ScoreTextToken.PassengerDiscomfort: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","discomfort"});
					case Game.ScoreTextToken.DoorsOpened: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","doors"});
					case Game.ScoreTextToken.ArrivedAtStation: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_arrived"});
					case Game.ScoreTextToken.PerfectTimeBonus: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_perfecttime"});
					case Game.ScoreTextToken.Late: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_late"});
					case Game.ScoreTextToken.PerfectStopBonus: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_perfectstop"});
					case Game.ScoreTextToken.Stop: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_stop"});
					case Game.ScoreTextToken.PrematureDeparture: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_departure"});
					case Game.ScoreTextToken.Total: return Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","station_total"});
					default: return "?";
			}
		}

		/// <summary>Exports the current score data to a file</summary>
		/// <param name="file">The file to write</param>
		internal static void ExportScore(string file) {
			CultureInfo culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			string[][] lines = new string[Game.ScoreLogCount + 1][];
			lines[0] = new[] {
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","time"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","position"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","value"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","cumulative"}),
				Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","reason"})
			};
			int columns = lines[0].Length;
			int totalScore = 0;
			for (int i = 0; i < Game.ScoreLogCount; i++) {
				int j = i + 1;
				lines[j] = new string[columns];
				{
					double x = Game.ScoreLogs[i].Time;
					int h = (int)Math.Floor(x / 3600.0);
					x -= h * 3600.0;
					int m = (int)Math.Floor(x / 60.0);
					x -= m * 60.0;
					int s = (int)Math.Floor(x);
					lines[j][0] = h.ToString("00", culture) + ":" + m.ToString("00", culture) + ":" + s.ToString("00", culture);
				}
				lines[j][1] = Game.ScoreLogs[i].Position.ToString("0", culture);
				lines[j][2] = Game.ScoreLogs[i].Value.ToString(culture);
				totalScore += Game.ScoreLogs[i].Value;
				lines[j][3] = totalScore.ToString(culture);
				lines[j][4] = GetScoreText(Game.ScoreLogs[i].TextToken);
			}
			int[] widths = new int[columns];
			for (int i = 0; i < lines.Length; i++) {
				for (int j = 0; j < columns; j++) {
					if (lines[i][j].Length > widths[j]) {
						widths[j] = lines[i][j].Length;
					}
				}
			}
			{ // header rows
				int totalWidth = 0;
				for (int j = 0; j < columns; j++) {
					totalWidth += widths[j] + 2;
				}
				totalWidth += columns - 1;
				builder.Append('╔');
				builder.Append('═', totalWidth);
				builder.Append("╗\n");
				{
					builder.Append('║');
					builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","route"}) + " " + Game.LogRouteName).PadRight(totalWidth, ' '));
					builder.Append("║\n║");
					builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","train"}) + " " + Game.LogTrainName).PadRight(totalWidth, ' '));
					builder.Append("║\n║");
					builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","date"}) + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", culture)).PadRight(totalWidth, ' '));
					builder.Append("║\n");
				}
				builder.Append('╠');
				builder.Append('═', totalWidth);
				builder.Append("╣\n");
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
					builder.Append('║');
					builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","mode"}) + " " + s).PadRight(totalWidth, ' '));
					builder.Append("║\n║");
					builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","score"}) + " " + Game.CurrentScore.CurrentValue.ToString(culture) + " / " + Game.CurrentScore.Maximum.ToString(culture)).PadRight(totalWidth, ' '));
					builder.Append("║\n║");
					builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"log","rating"}) + " " + Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"rating" , index.ToString(culture)}) + " (" + (100.0 * ratio).ToString("0.00") + "%)").PadRight(totalWidth, ' '));
					builder.Append("║\n");
				}
			}
			{ // top border row
				builder.Append('╠');
				for (int j = 0; j < columns; j++) {
					if (j != 0) {
						builder.Append('╤');
					} builder.Append('═', widths[j] + 2);
				} builder.Append("╣\n");
			}
			for (int i = 0; i < lines.Length; i++) {
				// center border row
				if (i != 0) {
					builder.Append('╟');
					for (int j = 0; j < columns; j++) {
						if (j != 0) {
							builder.Append('┼');
						} builder.Append('─', widths[j] + 2);
					} builder.Append("╢\n");
				}
				// cell content
				builder.Append('║');
				for (int j = 0; j < columns; j++) {
					if (j != 0) builder.Append('│');
					builder.Append(' ');
					if (i != 0 & j <= 3) {
						builder.Append(lines[i][j].PadLeft(widths[j], ' '));
					} else {
						builder.Append(lines[i][j].PadRight(widths[j], ' '));
					}
					builder.Append(' ');
				} builder.Append("║\n");
			}
			{ // bottom border row
				builder.Append('╚');
				for (int j = 0; j < columns; j++) {
					if (j != 0) {
						builder.Append('╧');
					} builder.Append('═', widths[j] + 2);
				} builder.Append('╝');
			}
			System.IO.File.WriteAllText(file, builder.ToString(), new System.Text.UTF8Encoding(true));
		}
	}
}
