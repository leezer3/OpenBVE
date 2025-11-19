using System;
using System.Linq;
using System.Windows.Forms;
using LibRender2.Overlays;

namespace RouteViewer
{
	public partial class formRailPaths : Form
	{
		private readonly int keyColumn = 5;

		public formRailPaths()
		{
			InitializeComponent();
			if (!Program.CurrentHost.MonoRuntime)
			{
				// Mono is broken in this mode
				dataGridViewPaths.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			}
			for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
			{
				int railIndex = Program.CurrentRoute.Tracks.ElementAt(i).Key;
				RailPath path = Program.Renderer.trackColors[railIndex];
				object[] newRow =
				{
					railIndex.ToString(),
					path.Description,
					string.Empty,
					path.CurrentlyVisible(),
					path.Display,
					railIndex
				};
				dataGridViewPaths.Rows.Add(newRow);
			}
			// setting the back color doesn't take when adding new rows, so do it afterwards
			for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
			{
				int trackKey = Program.CurrentRoute.Tracks.ElementAt(i).Key;
				RailPath path = Program.Renderer.trackColors[trackKey];
				// Drawn path color
				dataGridViewPaths.Rows[i].Cells[2].Style.BackColor = path.Color;
				dataGridViewPaths.Rows[i].Cells[2].Style.SelectionBackColor = path.Color;
			}
			dataGridViewPaths.Columns[keyColumn].Visible = false;
			dataGridViewPaths.RowHeadersVisible = false;
			dataGridViewPaths.AllowUserToAddRows = false;
			dataGridViewPaths.AllowUserToDeleteRows = false;
			dataGridViewPaths.AllowUserToOrderColumns = false;
			checkBoxRenderPaths.Checked = Program.Renderer.OptionPaths;
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
			if (Program.CurrentHost.MonoRuntime)
			{
				// HACK: On some machines, Mono doesn't appear to call CurrentCellDirtyStateChanged
				// https://github.com/leezer3/OpenBVE/issues/1025
				for (int i = 0; i < dataGridViewPaths.RowCount; i++)
				{
					Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[i].Cells[keyColumn].Value].Display = (bool)dataGridViewPaths.Rows[i].Cells[4].Value;
				}
			}
			Program.pathForm = null;
			this.Close();
		}

		private void dataGridViewPaths_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dataGridViewPaths.CurrentCell.OwningColumn == dataGridViewPaths.Columns[4] && dataGridViewPaths.IsCurrentCellDirty)
			{
				dataGridViewPaths.CommitEdit(DataGridViewDataErrorContexts.Commit);
				if (dataGridViewPaths.CurrentRow != null)
				{
					Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[keyColumn].Value].Display = !Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[keyColumn].Value].Display;
				}
			}
		}

		private void checkBoxRenderPaths_CheckedChanged(object sender, EventArgs e)
		{
			Program.Renderer.OptionPaths = checkBoxRenderPaths.Checked;
		}

		private void dataGridViewPaths_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (dataGridViewPaths.CurrentCell.OwningColumn == dataGridViewPaths.Columns[2])
			{
				if (dataGridViewPaths.CurrentRow != null)
				{
					ColorDialog cd = new ColorDialog();
					if (cd.ShowDialog() == DialogResult.OK)
					{
						Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[keyColumn].Value].Color = cd.Color;
						dataGridViewPaths.CurrentCell.Style.BackColor = cd.Color;
						dataGridViewPaths.CurrentCell.Style.SelectionBackColor = cd.Color;
					}
					
				}
			}
		}

		private void dataGridViewPaths_CellValidated(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGridViewPaths.CurrentRow != null && e.ColumnIndex ==  1 && dataGridViewPaths.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
			{
				Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[keyColumn].Value].Description = dataGridViewPaths.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
			}
		}

		private void buttonSelectAll_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < dataGridViewPaths.Rows.Count; i++)
			{
				dataGridViewPaths.Rows[i].Cells[4].Value = true;
				Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[i].Cells[keyColumn].Value].Display = true;
			}
		}

		private void buttonSelectNone_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < dataGridViewPaths.Rows.Count; i++)
			{
				dataGridViewPaths.Rows[i].Cells[4].Value = false;
				Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[i].Cells[keyColumn].Value].Display = false;
			}
		}
	}
}
