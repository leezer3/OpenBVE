using System.Drawing;
using System.Reflection;

namespace LibRender2.Cursors
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

			var thisAssembly = Assembly.GetExecutingAssembly();
			using (var stream = thisAssembly.GetManifestResourceStream("OpenBve.plus.png"))
			{
				if (stream != null)
				{
					Bitmap Plus = new Bitmap(stream);
					using (var g = System.Drawing.Graphics.FromImage(Plus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Plus.LockBits(new Rectangle(0, 0, Plus.Width, Plus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorPlus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Plus.UnlockBits(data);
					}
				}
				else
				{
					Bitmap Plus = new Bitmap(OpenBveApi.Path.CombineFile(Renderer.fileSystem.GetDataFolder(), "Cursors\\Symbols\\plus.png"));
					using (var g = System.Drawing.Graphics.FromImage(Plus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Plus.LockBits(new Rectangle(0, 0, Plus.Width, Plus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorPlus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Plus.UnlockBits(data);
					}
				}
			}
			using (var stream = thisAssembly.GetManifestResourceStream("OpenBve.minus.png"))
			{
				if (stream != null)
				{
					Bitmap Minus = new Bitmap(stream);
					using (var g = System.Drawing.Graphics.FromImage(Minus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Minus.LockBits(new Rectangle(0, 0, Minus.Width, Minus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorMinus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Minus.UnlockBits(data);
					}
				}
				else
				{
					Bitmap Minus = new Bitmap(OpenBveApi.Path.CombineFile(Renderer.fileSystem.GetDataFolder(), "Cursors\\Symbols\\minus.png"));
					using (var g = System.Drawing.Graphics.FromImage(Minus))
					{
						g.DrawImage(image, 0.0f, 0.0f, image.Width, image.Height);
						var data = Minus.LockBits(new Rectangle(0, 0, Minus.Width, Minus.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
						MyCursorMinus = new OpenTK.MouseCursor(5, 0, data.Width, data.Height, data.Scan0);
						Minus.UnlockBits(data);
					}
				}
			}

			{
				var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
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
}
