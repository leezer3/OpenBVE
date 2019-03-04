using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using OpenTK;

namespace OpenBve
{
	internal class Cursor
	{
		internal string FileName { get; private set; }
		internal MouseCursor MyCursor { get; private set; }
		internal MouseCursor MyCursorPlus { get; private set; }
		internal MouseCursor MyCursorMinus { get; private set; }
		internal Image Image { get; private set; }

		internal Cursor(string fileName, Bitmap image)
		{
			FileName = fileName;
			Image = image;

			var thisAssembly = Assembly.GetExecutingAssembly();
			using (var stream = thisAssembly.GetManifestResourceStream("OpenBve.plus.png"))
			{
				if (stream != null)
				{
					Bitmap Plus = new Bitmap(stream);
					using (var g = Graphics.FromImage(Plus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Plus.LockBits(new Rectangle(0, 0, Plus.Width, Plus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorPlus = new MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Plus.UnlockBits(data);
					}
				}
				else
				{
					Bitmap Plus = new Bitmap(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "Cursors\\Symbols\\plus.png"));
					using (var g = Graphics.FromImage(Plus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Plus.LockBits(new Rectangle(0, 0, Plus.Width, Plus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorPlus = new MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Plus.UnlockBits(data);
					}
				}
			}
			using (var stream = thisAssembly.GetManifestResourceStream("OpenBve.minus.png"))
			{
				if (stream != null)
				{
					Bitmap Minus = new Bitmap(stream);
					using (var g = Graphics.FromImage(Minus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Minus.LockBits(new Rectangle(0, 0, Minus.Width, Minus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorMinus = new MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Minus.UnlockBits(data);
					}
				}
				else
				{
					Bitmap Minus = new Bitmap(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "Cursors\\Symbols\\minus.png"));
					using (var g = Graphics.FromImage(Minus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Minus.LockBits(new Rectangle(0, 0, Minus.Width, Minus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorMinus = new MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Minus.UnlockBits(data);
					}
				}
			}

			{
				var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
				MyCursor = new MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
				image.UnlockBits(data);
			}
		}

		internal enum Status
		{
			Default,
			Plus,
			Minus
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
		internal static MouseCursor CurrentCursorPlus;
		internal static MouseCursor CurrentCursorMinus;

		internal static void LoadCursorImages(string CursorFolder)
		{
			if (!Directory.Exists(CursorFolder))
			{
				MessageBox.Show(@"The default cursor images have been moved or deleted.");
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
						Bitmap Image = new Bitmap(Fs);
						CursorList.Add(new Cursor(Path.GetFileName(File), Image));
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
					CursorList.Add(new Cursor("nk.png", Image));
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
			Cursor c = comboboxCursor.Items[i] as Cursor;
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
