using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using OpenBveApi.Interface;

namespace LibRender2
{
	public class MouseCursor
	{
		internal readonly BaseRenderer Renderer;
		public readonly string FileName;
		public readonly OpenTK.MouseCursor MyCursor;
		public readonly OpenTK.MouseCursor MyCursorPlus;
		public readonly OpenTK.MouseCursor MyCursorMinus;
		public readonly Image Image;

		public MouseCursor(BaseRenderer renderer, string fileName, Bitmap image)
		{
			Renderer = renderer;
			FileName = fileName;
			Image = image;

			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			using (Stream stream = thisAssembly.GetManifestResourceStream("OpenBve.plus.png"))
			{
				if (stream != null)
				{
					Bitmap Plus = new Bitmap(stream);
					using (Graphics g = Graphics.FromImage(Plus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						BitmapData data = Plus.LockBits(new Rectangle(0, 0, Plus.Width, Plus.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
						MyCursorPlus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Plus.UnlockBits(data);
					}
				}
				else
				{
					Bitmap Plus = new Bitmap(OpenBveApi.Path.CombineFile(Renderer.fileSystem.GetDataFolder(), "Cursors\\Symbols\\plus.png"));
					using (Graphics g = Graphics.FromImage(Plus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						BitmapData data = Plus.LockBits(new Rectangle(0, 0, Plus.Width, Plus.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
						MyCursorPlus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Plus.UnlockBits(data);
					}
				}
			}
			using (Stream stream = thisAssembly.GetManifestResourceStream("OpenBve.minus.png"))
			{
				if (stream != null)
				{
					Bitmap Minus = new Bitmap(stream);
					using (Graphics g = Graphics.FromImage(Minus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						BitmapData data = Minus.LockBits(new Rectangle(0, 0, Minus.Width, Minus.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
						MyCursorMinus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Minus.UnlockBits(data);
					}
				}
				else
				{
					Bitmap Minus = new Bitmap(OpenBveApi.Path.CombineFile(Renderer.fileSystem.GetDataFolder(), "Cursors\\Symbols\\minus.png"));
					using (Graphics g = Graphics.FromImage(Minus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						BitmapData data = Minus.LockBits(new Rectangle(0, 0, Minus.Width, Minus.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
						MyCursorMinus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Minus.UnlockBits(data);
					}
				}
			}

			{
				BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
				MyCursor = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
				image.UnlockBits(data);
			}
		}

		public enum Status
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

	public static class AvailableCursors
	{
		public static readonly List<MouseCursor> CursorList = new List<MouseCursor>();
		public static OpenTK.MouseCursor CurrentCursor;
		public static OpenTK.MouseCursor CurrentCursorPlus;
		public static OpenTK.MouseCursor CurrentCursorMinus;
		public static OpenTK.MouseCursor ScrollCursor;
		public static OpenTK.MouseCursor ResizeCursor;
		internal static BaseRenderer Renderer;
		public static void LoadCursorImages(BaseRenderer renderer, string CursorFolder)
		{
			Renderer = renderer;
			if (!Directory.Exists(CursorFolder))
			{
				Renderer.currentHost.AddMessage(MessageType.Error, true, "Failed to load the default cursor images- Falling back to embedded.");
				LoadEmbeddedCursorImages();
				return;
			}

			string[] CursorImageFiles = Directory.GetFiles(CursorFolder);

			foreach (string File in CursorImageFiles)
			{
				try
				{
					using (FileStream Fs= new FileStream(File, FileMode.Open, FileAccess.Read))
					{
						if (File.EndsWith("scroll.png", StringComparison.InvariantCultureIgnoreCase))
						{
							Bitmap Image = new Bitmap(Fs);
							MouseCursor c = new MouseCursor(Renderer, Path.GetFileName(File), Image);
							ScrollCursor = c.MyCursor;
						}
						else
						{
							Bitmap Image = new Bitmap(Fs);
							CursorList.Add(new MouseCursor(Renderer, Path.GetFileName(File), Image));	
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
			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			using (Stream stream = thisAssembly.GetManifestResourceStream("OpenBve.nk.png"))
			{
				if (stream != null)
				{
					Bitmap Image = new Bitmap(stream);
					CursorList.Add(new MouseCursor(Renderer, "nk.png", Image));
				}
			}
			Renderer.currentOptions.CursorFileName = "nk.png";
		}

		/// <summary>Gets a horizontal (west-east) resize cursor, loading resize_a_horizontal.png on first use</summary>
		public static OpenTK.MouseCursor GetResizeCursor()
		{
			if (ResizeCursor == null)
			{
				ResizeCursor = LoadResizeCursor() ?? CreateResizeCursor();
			}
			return ResizeCursor;
		}

		/// <summary>Loads the resize cursor image from the data folder, or null if unavailable</summary>
		private static OpenTK.MouseCursor LoadResizeCursor()
		{
			try
			{
				if (Renderer?.fileSystem == null) return null;
				string file = OpenBveApi.Path.CombineFile(Renderer.fileSystem.GetDataFolder(), "Cursors\\Symbols\\resize_a_horizontal.png");
				if (!File.Exists(file)) return null;
				using (Bitmap bitmap = new Bitmap(file))
				{
					BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
					OpenTK.MouseCursor cursor = new OpenTK.MouseCursor(data.Width / 2, data.Height / 2, data.Width, data.Height, data.Scan0);
					bitmap.UnlockBits(data);
					return cursor;
				}
			}
			catch
			{
				return null;
			}
		}

		/// <summary>Procedurally draws a double-headed horizontal arrow cursor</summary>
		private static OpenTK.MouseCursor CreateResizeCursor()
		{
			const int size = 32;
			const int cy = size / 2;
			using (Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppPArgb))
			{
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					g.Clear(Color.Transparent);
					g.SmoothingMode = SmoothingMode.AntiAlias;
					Point[] shaft = { new Point(10, cy), new Point(22, cy) };
					Point[] leftHead = { new Point(11, cy - 6), new Point(5, cy), new Point(11, cy + 6) };
					Point[] rightHead = { new Point(21, cy - 6), new Point(27, cy), new Point(21, cy + 6) };
					using (Pen outline = new Pen(Color.Black, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
					using (Pen fill = new Pen(Color.White, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
					{
						foreach (Point[] lines in new[] { shaft, leftHead, rightHead })
						{
							g.DrawLines(outline, lines);
							g.DrawLines(fill, lines);
						}
					}
				}
				BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
				OpenTK.MouseCursor cursor = new OpenTK.MouseCursor(size / 2, size / 2, data.Width, data.Height, data.Scan0);
				bitmap.UnlockBits(data);
				return cursor;
			}
		}
	}
}
