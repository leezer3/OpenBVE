using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibRender2.Overlays;
using OpenBveApi.Colors;

namespace RouteViewer
{
	public partial class formRailPaths : Form
	{
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
				int key = Program.CurrentRoute.Tracks.ElementAt(i).Key;
				RailPath path = Program.Renderer.trackColors[key];
				object[] newRow =
				{
					key.ToString(),
					path.Description,
					string.Empty,
					path.CurrentlyVisible(),
					path.Display,
					key
				};
				dataGridViewPaths.Rows.Add(newRow);
			}
			// setting the back color doesn't take when adding new rows, so do it afterwards
			for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
			{
				int key = Program.CurrentRoute.Tracks.ElementAt(i).Key;
				RailPath path = Program.Renderer.trackColors[key];
				// Drawn path color
				dataGridViewPaths.Rows[i].Cells[2].Style.BackColor = path.Color;
				dataGridViewPaths.Rows[i].Cells[2].Style.SelectionBackColor = path.Color;
			}
			dataGridViewPaths.Columns[5].Visible = false;
			dataGridViewPaths.RowHeadersVisible = false;
			dataGridViewPaths.AllowUserToAddRows = false;
			dataGridViewPaths.AllowUserToDeleteRows = false;
			dataGridViewPaths.AllowUserToOrderColumns = false;
			checkBoxRenderPaths.Checked = Program.Renderer.OptionPaths;
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
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
					Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[5].Value].Display = !Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[5].Value].Display;
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
						Program.Renderer.trackColors[(int)dataGridViewPaths.Rows[dataGridViewPaths.CurrentRow.Index].Cells[5].Value].Color = cd.Color;
						dataGridViewPaths.CurrentCell.Style.BackColor = cd.Color;
						dataGridViewPaths.CurrentCell.Style.SelectionBackColor = cd.Color;
					}
					
				}
			}
		}
	}
}
