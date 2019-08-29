using System;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Colors;
using TrainEditor2.Models.Others;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private TreeNode TreeViewItemViewModelToTreeNode(TreeViewItemViewModel item)
		{
			return new TreeNode(item.Title.Value, item.Children.Select(TreeViewItemViewModelToTreeNode).ToArray()) { Tag = item };
		}

		private TreeNode SearchTreeNode(TreeViewItemViewModel item, TreeNode node)
		{
			return node.Tag == item ? node : node.Nodes.OfType<TreeNode>().Select(x => SearchTreeNode(item, x)).FirstOrDefault(x => x != null);
		}

		private ColumnHeader ListViewColumnHeaderViewModelToColumnHeader(ListViewColumnHeaderViewModel column)
		{
			return new ColumnHeader { Text = column.Text.Value, Tag = column };
		}

		private ListViewItem ListViewItemViewModelToListViewItem(ListViewItemViewModel item)
		{
			return new ListViewItem(item.Texts.ToArray()) { Tag = item };
		}

		private void UpdateListViewItem(ListViewItem item, ListViewItemViewModel viewModel)
		{
			for (int i = 0; i < viewModel.Texts.Count; i++)
			{
				item.SubItems[i].Text = viewModel.Texts[i];
			}
		}

		private InputEventModel.EventArgs MouseEventArgsToModel(MouseEventArgs e)
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

		private Cursor CursorTypeToCursor(InputEventModel.CursorType cursorType)
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

		private void ModifierKeysDownUp(KeyEventArgs e)
		{
			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			if (pictureBoxDrawArea.Focused)
			{
				if (car != null)
				{
					car.Motor.Value.CurrentModifierKeys.Value = InputEventModel.ModifierKeys.None;

					if (e.Alt)
					{
						car.Motor.Value.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Alt;
					}

					if (e.Control)
					{
						car.Motor.Value.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Control;
					}

					if (e.Shift)
					{
						car.Motor.Value.CurrentModifierKeys.Value |= InputEventModel.ModifierKeys.Shift;
					}
				}
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

		private void OpenColorDialog(TextBox textBox)
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
	}
}
