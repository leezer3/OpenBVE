using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Permissions;
using System.Windows.Forms;
using OpenBveApi.Interface;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using TrainEditor2.ViewModels;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor : Form
	{
		private readonly CompositeDisposable disposable;

		private readonly AppViewModel app;

		private TreeNode TreeViewCarsTopNode
		{
			get
			{
				if (treeViewCars.Nodes.Count == 0)
				{
					treeViewCars.Nodes.Add(new TreeNode());
				}

				return treeViewCars.Nodes[0];
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				treeViewCars.Nodes.Clear();
				treeViewCars.Nodes.Add(value);
				treeViewCars.ExpandAll();
				app.SelectedItem.ForceNotify();
			}
		}

		public FormEditor()
		{
			disposable = new CompositeDisposable();
			CompositeDisposable messageDisposable = new CompositeDisposable();
			CompositeDisposable trainDisposable = new CompositeDisposable();

			app = new AppViewModel();

			InitializeComponent();

			app.SaveLocation
				.BindTo(
					this,
					x => x.Text,
					x => string.IsNullOrEmpty(x) ? $@"NewFile - {Application.ProductName}" : $@"{Path.GetFileName(x)} - {Application.ProductName}"
				)
				.AddTo(disposable);

			app.MessageBox
				.Subscribe(x =>
				{
					messageDisposable.Dispose();
					messageDisposable = new CompositeDisposable();

					BindToMessageBox(x).AddTo(messageDisposable);
				})
				.AddTo(disposable);

			app.Train
				.Subscribe(x =>
				{
					trainDisposable.Dispose();
					trainDisposable = new CompositeDisposable();

					BindToTrain(x).AddTo(trainDisposable);
				})
				.AddTo(disposable);

			app.Item
				.BindTo(
					this,
					x => x.TreeViewCarsTopNode,
					BindingMode.OneWay,
					TreeViewItemViewModelToTreeNode
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					treeViewCars,
					x => x.SelectedNode,
					BindingMode.TwoWay,
					x => treeViewCars.Nodes.OfType<TreeNode>().Select(y => SearchTreeNode(x, y)).FirstOrDefault(y => y != null),
					x => (TreeViewItemViewModel)x.Tag,
					Observable.FromEvent<TreeViewEventHandler, TreeViewEventArgs>(
							h => (s, e) => h(e),
							h => treeViewCars.AfterSelect += h,
							h => treeViewCars.AfterSelect -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPageTrain,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == app.Item.Value.Children[0]
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPageCar,
					x => x.Enabled,
					BindingMode.OneWay,
					x => app.Item.Value.Children[1].Children.Contains(x)
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPageAccel,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x?.Tag.Value is MotorCar && app.Item.Value.Children[1].Children.Contains(x)
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPageMotor,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x?.Tag.Value is MotorCar && app.Item.Value.Children[1].Children.Contains(x)
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPageCoupler,
					x => x.Enabled,
					BindingMode.OneWay,
					x => app.Item.Value.Children[2].Children.Contains(x)
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPagePanel,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == app.Item.Value.Children[0]
				)
				.AddTo(disposable);

			app.SelectedItem
				.BindTo(
					tabPageSound,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == app.Item.Value.Children[0]
				)
				.AddTo(disposable);

			app.CreateNewFileCommand.BindToButton(toolStripMenuItemNew).AddTo(disposable);

			new[] { app.UpCarCommand, app.UpCouplerCommand }.BindToButton(buttonCarsUp).AddTo(disposable);
			new[] { app.DownCarCommand, app.DownCouplerCommand }.BindToButton(buttonCarsDown).AddTo(disposable);
			app.AddCarCommand.BindToButton(buttonCarsAdd).AddTo(disposable);
			app.CopyCarCommand.BindToButton(buttonCarsCopy).AddTo(disposable);
			app.RemoveCarCommand.BindToButton(buttonCarsRemove).AddTo(disposable);

			app.ChangeCarClass.BindToCheckBox(checkBoxIsMotorCar).AddTo(disposable);

			messageDisposable.AddTo(disposable);
			trainDisposable.AddTo(disposable);
		}

		private void FormEditor_Load(object sender, EventArgs e)
		{
			MinimumSize = Size;

			ComboBox comboBox = toolStripComboBoxIndex.ComboBox;

			if (comboBox != null)
			{
				comboBox.DrawMode = DrawMode.OwnerDrawFixed;
				comboBox.DrawItem += ToolStripComboBoxIndex_DrawItem;
			}

			Translations.CurrentLanguageCode = Interface.CurrentOptions.LanguageCode;
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			Translations.ListLanguages(toolStripComboBoxLanguage.ComboBox);
		}

		[UIPermission(SecurityAction.Demand, Window = UIPermissionWindow.AllWindows)]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			bool ret = false;

			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			if (keyData == Keys.Left)
			{
				if (pictureBoxAccel.Focused)
				{
					car?.Acceleration.Value.MoveLeft.Execute();
					ret = true;
				}

				if (pictureBoxDrawArea.Focused)
				{
					car?.Motor.Value.MoveLeft.Execute();
					ret = true;
				}
			}

			if (keyData == Keys.Right)
			{
				if (pictureBoxAccel.Focused)
				{
					car?.Acceleration.Value.MoveRight.Execute();
					ret = true;
				}

				if (pictureBoxDrawArea.Focused)
				{
					car?.Motor.Value.MoveRight.Execute();
					ret = true;
				}
			}

			if (keyData == Keys.Down)
			{
				if (pictureBoxAccel.Focused)
				{
					car?.Acceleration.Value.MoveBottom.Execute();
					ret = true;
				}

				if (pictureBoxDrawArea.Focused)
				{
					car?.Motor.Value.MoveBottom.Execute();
					ret = true;
				}
			}

			if (keyData == Keys.Up)
			{
				if (pictureBoxAccel.Focused)
				{
					car?.Acceleration.Value.MoveTop.Execute();
					ret = true;
				}

				if (pictureBoxDrawArea.Focused)
				{
					car?.Motor.Value.MoveTop.Execute();
					ret = true;
				}
			}

			return ret;
		}

		private void FormEditor_KeyDown(object sender, KeyEventArgs e)
		{
			ModifierKeysDownUp(e);
		}

		private void FormEditor_KeyUp(object sender, KeyEventArgs e)
		{
			ModifierKeysDownUp(e);
		}

		private void ToolStripComboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			string currentLanguageCode = app.CurrentLanguageCode.Value;

			if (Translations.SelectedLanguage(ref currentLanguageCode, toolStripComboBoxLanguage.ComboBox))
			{
				//ApplyLanguage();
			}

			app.CurrentLanguageCode.Value = currentLanguageCode;
		}

		private void ButtonDelayPowerSet_Click(object sender, EventArgs e)
		{
			using (FormDelay form = new FormDelay(app.Train.Value.SelectedCar.Value.Delay.Value.DelayPower))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonDelayBrakeSet_Click(object sender, EventArgs e)
		{
			using (FormDelay form = new FormDelay(app.Train.Value.SelectedCar.Value.Delay.Value.DelayBrake))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonDelayLocoBrakeSet_Click(object sender, EventArgs e)
		{
			using (FormDelay form = new FormDelay(app.Train.Value.SelectedCar.Value.Delay.Value.DelayLocoBrake))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonFrontBogieSet_Click(object sender, EventArgs e)
		{
			using (FormBogie form = new FormBogie(app.Train.Value.SelectedCar.Value.FrontBogie.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonRearBogieSet_Click(object sender, EventArgs e)
		{
			using (FormBogie form = new FormBogie(app.Train.Value.SelectedCar.Value.RearBogie.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void PictureBoxAccel_MouseEnter(object sender, EventArgs e)
		{
			pictureBoxAccel.Focus();
		}

		private void PictureBoxAccel_MouseMove(object sender, MouseEventArgs e)
		{
			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			car?.Acceleration.Value.MouseMove.Execute(MouseEventArgsToModel(e));
		}

		private void ToolStripComboBoxIndex_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();

			if (e.Index == -1)
			{
				return;
			}

			Color c;

			if (e.Index - 1 >= 0)
			{
				double hue = Utilities.HueFactor * (e.Index - 1);
				hue -= Math.Floor(hue);
				c = Utilities.GetColor(hue, false);
			}
			else
			{
				c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
			}

			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			e.Graphics.FillRectangle(new SolidBrush(c), e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
			e.Graphics.DrawString(toolStripComboBoxIndex.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + e.Bounds.Height + 10, e.Bounds.Y);
		}

		private void PictureBoxDrawArea_MouseEnter(object sender, EventArgs e)
		{
			pictureBoxDrawArea.Focus();
		}

		private void PictureBoxDrawArea_MouseDown(object sender, MouseEventArgs e)
		{
			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			car?.Motor.Value.MouseDown.Execute(MouseEventArgsToModel(e));
		}

		private void PictureBoxDrawArea_MouseMove(object sender, MouseEventArgs e)
		{
			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			car?.Motor.Value.MouseMove.Execute(MouseEventArgsToModel(e));
		}

		private void PictureBoxDrawArea_MouseUp(object sender, MouseEventArgs e)
		{
			MotorCarViewModel car = app.Train.Value.SelectedCar.Value as MotorCarViewModel;

			car?.Motor.Value.MouseUp.Execute();
		}
	}
}
