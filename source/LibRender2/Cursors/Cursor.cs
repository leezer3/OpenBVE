using System;
using System.Collections.Generic;
using System.Drawing;
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
	}
}
