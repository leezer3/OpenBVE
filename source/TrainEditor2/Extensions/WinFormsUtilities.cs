using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Colors;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Extensions
{
	internal static class WinFormsUtilities
	{
		internal static Icon GetIcon()
		{
			return new Icon(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico"));
		}

		internal static Bitmap GetImage(string path)
		{
			string folder = Program.FileSystem.GetDataFolder("TrainEditor2");
			Bitmap image = new Bitmap(OpenBveApi.Path.CombineFile(folder, path));
			image.MakeTransparent();
			return image;
		}

		internal static void CheckDragEnter(DragEventArgs e, string extension)
		{
			string fileName = string.Empty;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				fileName = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
			}

			if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
			{
				e.Effect = DragDropEffects.None;
			}
			else
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		internal static void OpenFileDialog(TextBox textBox)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = @"All files (*.*)|*";
				dialog.CheckFileExists = true;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				textBox.Text = dialog.FileName;
			}
		}

		internal static void OpenColorDialog(TextBox textBox)
		{
			using (ColorDialog dialog = new ColorDialog())
			{
				Color24 nowColor;
				Color24.TryParseHexColor(textBox.Text, out nowColor);
				dialog.Color = nowColor;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				textBox.Text = ((Color24)dialog.Color).ToString();
			}
		}

		internal static InputEventModel.EventArgs MouseEventArgsToModel(MouseEventArgs e)
		{
			return new InputEventModel.EventArgs
			{
				LeftButton = e.Button == MouseButtons.Left ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				MiddleButton = e.Button == MouseButtons.Middle ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				RightButton = e.Button == MouseButtons.Right ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				XButton1 = e.Button == MouseButtons.XButton1 ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				XButton2 = e.Button == MouseButtons.XButton2 ? InputEventModel.ButtonState.Pressed : InputEventModel.ButtonState.Released,
				X = e.X,
				Y = e.Y
			};
		}

		internal static Cursor CursorTypeToCursor(InputEventModel.CursorType cursorType)
		{
			switch (cursorType)
			{
				case InputEventModel.CursorType.No:
					return Cursors.No;
				case InputEventModel.CursorType.Arrow:
					return Cursors.Arrow;
				case InputEventModel.CursorType.AppStarting:
					return Cursors.AppStarting;
				case InputEventModel.CursorType.Cross:
					return Cursors.Cross;
				case InputEventModel.CursorType.Help:
					return Cursors.Help;
				case InputEventModel.CursorType.IBeam:
					return Cursors.IBeam;
				case InputEventModel.CursorType.SizeAll:
					return Cursors.SizeAll;
				case InputEventModel.CursorType.SizeNESW:
					return Cursors.SizeNESW;
				case InputEventModel.CursorType.SizeNS:
					return Cursors.SizeNS;
				case InputEventModel.CursorType.SizeNWSE:
					return Cursors.SizeNWSE;
				case InputEventModel.CursorType.SizeWE:
					return Cursors.SizeWE;
				case InputEventModel.CursorType.UpArrow:
					return Cursors.UpArrow;
				case InputEventModel.CursorType.Wait:
					return Cursors.WaitCursor;
				case InputEventModel.CursorType.Hand:
					return Cursors.Hand;
				case InputEventModel.CursorType.ScrollNS:
					return Cursors.NoMoveVert;
				case InputEventModel.CursorType.ScrollWE:
					return Cursors.NoMoveHoriz;
				case InputEventModel.CursorType.ScrollAll:
					return Cursors.NoMove2D;
				case InputEventModel.CursorType.ScrollN:
					return Cursors.PanNorth;
				case InputEventModel.CursorType.ScrollS:
					return Cursors.PanSouth;
				case InputEventModel.CursorType.ScrollW:
					return Cursors.PanWest;
				case InputEventModel.CursorType.ScrollE:
					return Cursors.PanEast;
				case InputEventModel.CursorType.ScrollNW:
					return Cursors.PanNW;
				case InputEventModel.CursorType.ScrollNE:
					return Cursors.PanNE;
				case InputEventModel.CursorType.ScrollSW:
					return Cursors.PanSW;
				case InputEventModel.CursorType.ScrollSE:
					return Cursors.PanSE;
				default:
					throw new ArgumentOutOfRangeException(nameof(cursorType), cursorType, null);
			}
		}
	}
}
