using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenBveApi.Interface;
using OpenTK;

namespace OpenBve
{
	internal static class Cursors
	{
		internal static readonly List<LibRender2.Cursors.MouseCursor> CursorList = new List<LibRender2.Cursors.MouseCursor>();
		internal static MouseCursor CurrentCursor;
		internal static MouseCursor CurrentCursorPlus;
		internal static MouseCursor CurrentCursorMinus;
		internal static MouseCursor ScrollCursor;

		internal static void LoadCursorImages(string CursorFolder)
		{
			if (!Directory.Exists(CursorFolder))
			{
				MessageBox.Show(@"The default cursor images have been moved or deleted.", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				LoadEmbeddedCursorImages();
				return;
			}

			string[] CursorImageFiles = Directory.GetFiles(CursorFolder);

			foreach (var File in CursorImageFiles)
			{
				try
				{
					using (var Fs= new FileStream(File, FileMode.Open, FileAccess.Read))
					{
						if (File.EndsWith("scroll.png", StringComparison.InvariantCultureIgnoreCase))
						{
							Bitmap Image = new Bitmap(Fs);
							LibRender2.Cursors.MouseCursor c = new LibRender2.Cursors.MouseCursor(Program.Renderer, Path.GetFileName(File), Image);
							ScrollCursor = c.MyCursor;
						}
						else
						{
							Bitmap Image = new Bitmap(Fs);
							CursorList.Add(new LibRender2.Cursors.MouseCursor(Program.Renderer, Path.GetFileName(File), Image));	
						}
						
						
					}
				}
				catch
				{
					// ignored
				}
			}
		}

		private static void LoadEmbeddedCursorImages()
		{
			var thisAssembly = Assembly.GetExecutingAssembly();
			using (var stream = thisAssembly.GetManifestResourceStream("OpenBve.nk.png"))
			{
				if (stream != null)
				{
					Bitmap Image = new Bitmap(stream);
					CursorList.Add(new LibRender2.Cursors.MouseCursor(Program.Renderer, "nk.png", Image));
				}
			}
			Interface.CurrentOptions.CursorFileName = "nk.png";
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
			LibRender2.Cursors.MouseCursor c = comboboxCursor.Items[i] as LibRender2.Cursors.MouseCursor;
			if (c == null)
			{
				return;
			}
			Interface.CurrentOptions.CursorFileName = c.FileName;
			CurrentCursor = c.MyCursor;
			CurrentCursorPlus = c.MyCursorPlus;
			CurrentCursorMinus = c.MyCursorMinus;
			pictureboxCursor.Image = c.Image;
		}
	}
}
