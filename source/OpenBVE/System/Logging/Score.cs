using System;
using System.Globalization;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>Gets the formatted text for an in-game score event</summary>
		/// <param name="TextToken">The in-game score event</param>
		internal static string GetScoreText(Game.ScoreTextToken TextToken) {
			switch (TextToken) {
					case Game.ScoreTextToken.Overspeed: return Translations.GetInterfaceString("score_overspeed");
					case Game.ScoreTextToken.PassedRedSignal: return Translations.GetInterfaceString("score_redsignal");
					case Game.ScoreTextToken.Toppling: return Translations.GetInterfaceString("score_toppling");
					case Game.ScoreTextToken.Derailed: return Translations.GetInterfaceString("score_derailed");
					case Game.ScoreTextToken.PassengerDiscomfort: return Translations.GetInterfaceString("score_discomfort");
					case Game.ScoreTextToken.DoorsOpened: return Translations.GetInterfaceString("score_doors");
					case Game.ScoreTextToken.ArrivedAtStation: return Translations.GetInterfaceString("score_station_arrived");
					case Game.ScoreTextToken.PerfectTimeBonus: return Translations.GetInterfaceString("score_station_perfecttime");
					case Game.ScoreTextToken.Late: return Translations.GetInterfaceString("score_station_late");
					case Game.ScoreTextToken.PerfectStopBonus: return Translations.GetInterfaceString("score_station_perfectstop");
					case Game.ScoreTextToken.Stop: return Translations.GetInterfaceString("score_station_stop");
					case Game.ScoreTextToken.PrematureDeparture: return Translations.GetInterfaceString("score_station_departure");
					case Game.ScoreTextToken.Total: return Translations.GetInterfaceString("score_station_total");
					default: return "?";
			}
		}

		/// <summary>Exports the current score data to a file</summary>
		/// <param name="File">The file to write</param>
		internal static void ExportScore(string File) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			string[][] Lines = new string[Game.ScoreLogCount + 1][];
			Lines[0] = new string[] {
				Translations.GetInterfaceString("log_time"),
				Translations.GetInterfaceString("log_position"),
				Translations.GetInterfaceString("log_value"),
				Translations.GetInterfaceString("log_cumulative"),
				Translations.GetInterfaceString("log_reason")
			};
			int Columns = Lines[0].Length;
			int TotalScore = 0;
			for (int i = 0; i < Game.ScoreLogCount; i++) {
				int j = i + 1;
				Lines[j] = new string[Columns];
				{
					double x = Game.ScoreLogs[i].Time;
					int h = (int)Math.Floor(x / 3600.0);
					x -= (double)h * 3600.0;
					int m = (int)Math.Floor(x / 60.0);
					x -= (double)m * 60.0;
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
					Builder.Append((" " + Translations.GetInterfaceString("log_route") + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString("log_train") + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString("log_date") + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
					Builder.Append("║\n");
				}
				Builder.Append('╠');
				Builder.Append('═', TotalWidth);
				Builder.Append("╣\n");
				{
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.CurrentValue / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Translations.RatingsCount);
					if (index >= Translations.RatingsCount) index = Translations.RatingsCount - 1;
					string s;
					switch (Interface.CurrentOptions.GameMode) {
							case Interface.GameMode.Arcade: s = Translations.GetInterfaceString("mode_arcade"); break;
							case Interface.GameMode.Normal: s = Translations.GetInterfaceString("mode_normal"); break;
							case Interface.GameMode.Expert: s = Translations.GetInterfaceString("mode_expert"); break;
							default: s = Translations.GetInterfaceString("mode_unknown"); break;
					}
					Builder.Append('║');
					Builder.Append((" " + Translations.GetInterfaceString("log_mode") + " " + s).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString("log_score") + " " + Game.CurrentScore.CurrentValue.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture)).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + Translations.GetInterfaceString("log_rating") + " " + Translations.GetInterfaceString("rating_" + index.ToString(Culture)) + " (" + (100.0 * ratio).ToString("0.00") + "%)").PadRight(TotalWidth, ' '));
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
