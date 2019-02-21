using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;

namespace OpenBve
{
	internal class Cursor
	{
		internal string FileName { get; private set; }
		internal MouseCursor MyCursor { get; private set; }
		internal Image Image { get; private set; }

		internal Cursor(string fileName, MouseCursor myCursor, Image image)
		{
			FileName = fileName;
			MyCursor = myCursor;
			Image = image;
		}

		public override string ToString()
		{
			return FileName;
		}
	}

	internal static class Cursors
	{
		internal static List<Cursor> CursorList = new List<Cursor>();
		internal static MouseCursor CurrentCursor;

		internal static void LoadCursorImages(string CursorFolder)
		{
			if (!Directory.Exists(CursorFolder))
			{
				MessageBox.Show(@"The default language files have been moved or deleted.");
				return;
			}

			string[] CursorImageFiles = Directory.GetFiles(CursorFolder);

			foreach (var File in CursorImageFiles)
			{
				try
				{
					using (var Fs= new FileStream(File, FileMode.Open, FileAccess.Read))
					{
						Bitmap Image = new Bitmap(Fs);
						var data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						CursorList.Add(new Cursor(Path.GetFileName(File), new MouseCursor(0, 0, data.Width, data.Height, data.Scan0), Image));
						Image.UnlockBits(data);
					}
				}
				catch
				{
					// ignored
				}
			}
		}

		internal static void ListCursors(ComboBox comboBoxCursor)
		{
			comboBoxCursor.Items.Clear();

			//Load all available cursors
			int idx = -1;
			for (int i = 0; i < CursorList.Count; i++)
			{
				comboBoxCursor.Items.Add(CursorList[i]);
				if (CursorList[i].FileName == Interface.CurrentOptions.CursorFileName)
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
			Cursor c = comboboxCursor.Items[i] as Cursor;
			if (c == null)
			{
				return;
			}
			Interface.CurrentOptions.CursorFileName = c.FileName;
			CurrentCursor = c.MyCursor;
			pictureboxCursor.Image = c.Image;
		}
	}
}
