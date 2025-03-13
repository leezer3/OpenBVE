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
			for (int i = 0; i < AvailableCursors.CursorList.Count; i++)
			{
				comboBoxCursor.Items.Add(AvailableCursors.CursorList[i]);
				if (AvailableCursors.CursorList[i].FileName == Interface.CurrentOptions.CursorFileName)
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
			if (!(comboboxCursor.Items[i] is MouseCursor c))
			{
				return;
			}
			Interface.CurrentOptions.CursorFileName = c.FileName;
			AvailableCursors.CurrentCursor = c.MyCursor;
			AvailableCursors.CurrentCursorPlus = c.MyCursorPlus;
			AvailableCursors.CurrentCursorMinus = c.MyCursorMinus;
			pictureboxCursor.Image = c.Image;
		}
	}
}
