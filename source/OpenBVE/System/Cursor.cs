using System.Windows.Forms;
using LibRender2;

namespace OpenBve
{
	internal static class Cursors
	{
		internal static void ListCursors(ComboBox comboBoxCursor)
		{
			comboBoxCursor.Items.Clear();

			//Load all available cursors
			int idx = -1;
			for (int i = 0; i < LibRender2.AvailableCursors.CursorList.Count; i++)
			{
				comboBoxCursor.Items.Add(LibRender2.AvailableCursors.CursorList[i]);
				if (LibRender2.AvailableCursors.CursorList[i].FileName == Interface.CurrentOptions.CursorFileName)
				{
					idx = i;
				}
			}

			if (idx != -1)
			{
				comboBoxCursor.SelectedIndex = idx;
			}
		}

		internal static void SelectedCursor(ComboBox comboboxCursor, PictureBox pictureboxCursor)
		{
			int i = comboboxCursor.SelectedIndex;
			if (i == -1)
			{
				return;
			}
			MouseCursor c = comboboxCursor.Items[i] as MouseCursor;
			if (c == null)
			{
				return;
			}
			Interface.CurrentOptions.CursorFileName = c.FileName;
			LibRender2.AvailableCursors.CurrentCursor = c.MyCursor;
			LibRender2.AvailableCursors.CurrentCursorPlus = c.MyCursorPlus;
			LibRender2.AvailableCursors.CurrentCursorMinus = c.MyCursorMinus;
			pictureboxCursor.Image = c.Image;
		}
	}
}
