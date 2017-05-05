using System;
using System.Windows.Forms;

namespace OpenBve {
	internal partial class formMain : Form {
		
		
				// ================
		// review last game
		// ================

		// score save
		private void buttonScoreExport_Click(object sender, EventArgs e) {
		    SaveFileDialog Dialog = new SaveFileDialog
		    {
		        OverwritePrompt = true,
		        Filter =
		            Interface.GetInterfaceString("dialog_textfiles") + @"|*.txt|" +
		            Interface.GetInterfaceString("dialog_allfiles") + @"|*"
		    };
		    if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.ExportScore(Dialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		// score penalties
		private void checkboxScorePenalties_CheckedChanged(object sender, EventArgs e) {
			ShowScoreLog(checkboxScorePenalties.Checked);
		}

		// black box export
		private void buttonBlackBoxExport_Click(object sender, EventArgs e) {
		    SaveFileDialog Dialog = new SaveFileDialog {OverwritePrompt = true};
		    if (comboboxBlackBoxFormat.SelectedIndex == 0) {
				Dialog.Filter = Interface.GetInterfaceString("dialog_csvfiles") + @"|*.txt|" + Interface.GetInterfaceString("dialog_allfiles") + @"|*";
			} else {
				Dialog.Filter = Interface.GetInterfaceString("dialog_textfiles") + @"|*.txt|" + Interface.GetInterfaceString("dialog_allfiles") + @"|*";
			}
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.ExportBlackBox(Dialog.FileName, (Interface.BlackBoxFormat)comboboxBlackBoxFormat.SelectedIndex);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		
				// show score log
		private void ShowScoreLog(bool PenaltiesOnly) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			listviewScore.Items.Clear();
			int sum = 0;
			for (int i = 0; i < Game.ScoreLogCount; i++) {
				sum += Game.ScoreLogs[i].Value;
				if (!PenaltiesOnly | Game.ScoreLogs[i].Value < 0) {
					double x = Game.ScoreLogs[i].Time;
					int h = (int)Math.Floor(x / 3600.0);
					x -= 3600.0 * (double)h;
					int m = (int)Math.Floor(x / 60.0);
					x -= 60.0 * (double)m;
					int s = (int)Math.Floor(x);
					ListViewItem Item = listviewScore.Items.Add(h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture));
					Item.SubItems.Add(Game.ScoreLogs[i].Position.ToString("0", Culture));
					Item.SubItems.Add(Game.ScoreLogs[i].Value.ToString(Culture));
					Item.SubItems.Add(sum.ToString(Culture));
					Item.SubItems.Add(Interface.GetScoreText(Game.ScoreLogs[i].TextToken));
				}
			}
			listviewScore.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		
		
	}
}