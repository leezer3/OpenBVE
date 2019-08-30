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
using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
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
				treeViewCars.SelectedNode = treeViewCars.Nodes
					.OfType<TreeNode>()
					.Select(y => SearchTreeNode(app.SelectedItem.Value, y))
					.FirstOrDefault(y => y != null);
				app.SelectedItem.ForceNotify();
			}
		}

		private TreeNode TreeViewPanelTopNode
		{
			get
			{
				if (treeViewPanel.Nodes.Count == 0)
				{
					treeViewPanel.Nodes.Add(new TreeNode());
				}

				return treeViewPanel.Nodes[0];
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				treeViewPanel.Nodes.Clear();
				treeViewPanel.Nodes.Add(value);
				treeViewPanel.ExpandAll();
				treeViewPanel.SelectedNode = treeViewPanel.Nodes
					.OfType<TreeNode>()
					.Select(z => SearchTreeNode(app.Panel.Value.SelectedTreeItem.Value, z))
					.FirstOrDefault(z => z != null);
				app.Panel.Value.SelectedTreeItem.ForceNotify();
			}
		}

		private ListViewItem ListViewPanelSelectedItem
		{
			get
			{
				if (listViewPanel.SelectedItems.Count == 1)
				{
					return listViewPanel.SelectedItems[0];
				}

				return null;
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				foreach (ListViewItem item in listViewPanel.Items.OfType<ListViewItem>().Where(x => x.Selected))
				{
					item.Selected = false;
				}

				if (value != null)
				{
					value.Selected = true;
				}
			}
		}

		private TreeNode TreeViewSoundTopNode
		{
			get
			{
				if (treeViewSound.Nodes.Count == 0)
				{
					treeViewSound.Nodes.Add(new TreeNode());
				}

				return treeViewSound.Nodes[0];
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				treeViewSound.Nodes.Clear();
				treeViewSound.Nodes.Add(value);
				treeViewSound.ExpandAll();
				treeViewSound.SelectedNode = treeViewSound.Nodes
					.OfType<TreeNode>()
					.Select(z => SearchTreeNode(app.Sound.Value.SelectedTreeItem.Value, z))
					.FirstOrDefault(z => z != null);
				app.Sound.Value.SelectedTreeItem.ForceNotify();
			}
		}

		private ListViewItem ListViewSoundSelectedItem
		{
			get
			{
				if (listViewSound.SelectedItems.Count == 1)
				{
					return listViewSound.SelectedItems[0];
				}

				return null;
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				foreach (ListViewItem item in listViewSound.Items.OfType<ListViewItem>().Where(x => x.Selected))
				{
					item.Selected = false;
				}

				if (value != null)
				{
					value.Selected = true;
				}
			}
		}

		public FormEditor()
		{
			disposable = new CompositeDisposable();
			CompositeDisposable messageDisposable = new CompositeDisposable();
			CompositeDisposable trainDisposable = new CompositeDisposable();
			CompositeDisposable panelDisposable = new CompositeDisposable();
			CompositeDisposable soundDisposable = new CompositeDisposable();

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

			app.Panel
				.Subscribe(x =>
				{
					panelDisposable.Dispose();
					panelDisposable = new CompositeDisposable();

					BindToPanel(x).AddTo(panelDisposable);
				})
				.AddTo(disposable);

			app.Sound
				.Subscribe(x =>
				{
					soundDisposable.Dispose();
					soundDisposable = new CompositeDisposable();

					BindToSound(x).AddTo(soundDisposable);
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

			app.SelectedItem
				.BindTo(
					tabControlEditor,
					x => x.SelectedTab,
					BindingMode.OneWay,
					_ =>
					{
						TabPage tabPage;

						if (tabControlEditor.SelectedTab.Enabled)
						{
							tabPage = tabControlEditor.SelectedTab;
						}
						else
						{
							tabPage = tabControlEditor.TabPages.OfType<TabPage>().FirstOrDefault(x => x.Enabled);
						}

						return tabPage;
					}
				)
				.AddTo(disposable);

			app.IsVisibleInfo
				.ToReadOnlyReactivePropertySlim()
				.BindTo(
					toolStripMenuItemInfo,
					x => x.Checked
				)
				.AddTo(disposable);

			app.IsVisibleInfo
				.ToReadOnlyReactivePropertySlim()
				.BindTo(
					toolStripMenuItemInfo,
					x => x.BackColor,
					x => x ? SystemColors.GradientActiveCaption : SystemColors.Control
				)
				.AddTo(disposable);

			app.IsVisibleWarning
				.ToReadOnlyReactivePropertySlim()
				.BindTo(
					toolStripMenuItemWarning,
					x => x.Checked
				)
				.AddTo(disposable);

			app.IsVisibleWarning
				.ToReadOnlyReactivePropertySlim()
				.BindTo(
					toolStripMenuItemWarning,
					x => x.BackColor,
					x => x ? SystemColors.GradientActiveCaption : SystemColors.Control
				)
				.AddTo(disposable);

			app.IsVisibleError
				.ToReadOnlyReactivePropertySlim()
				.BindTo(
					toolStripMenuItemError,
					x => x.Checked
				)
				.AddTo(disposable);

			app.IsVisibleError
				.ToReadOnlyReactivePropertySlim()
				.BindTo(
					toolStripMenuItemError,
					x => x.BackColor,
					x => x ? SystemColors.GradientActiveCaption : SystemColors.Control
				)
				.AddTo(disposable);

			app.VisibleLogMessages
				.ObserveAddChanged()
				.Subscribe(y =>
				{
					listViewStatus.Items.Add(ListViewItemViewModelToListViewItem(y));
					listViewStatus.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			app.VisibleLogMessages
				.ObserveRemoveChanged()
				.Subscribe(y =>
				{
					foreach (ListViewItem item in listViewStatus.Items.OfType<ListViewItem>().Where(z => z.Tag == y).ToArray())
					{
						listViewStatus.Items.Remove(item);
					}

					listViewStatus.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			app.CreateNewFileCommand.BindToButton(toolStripMenuItemNew).AddTo(disposable);

			new[] { app.UpCarCommand, app.UpCouplerCommand }.BindToButton(buttonCarsUp).AddTo(disposable);
			new[] { app.DownCarCommand, app.DownCouplerCommand }.BindToButton(buttonCarsDown).AddTo(disposable);
			app.AddCarCommand.BindToButton(buttonCarsAdd).AddTo(disposable);
			app.CopyCarCommand.BindToButton(buttonCarsCopy).AddTo(disposable);
			app.RemoveCarCommand.BindToButton(buttonCarsRemove).AddTo(disposable);

			app.ChangeCarClass.BindToCheckBox(checkBoxIsMotorCar).AddTo(disposable);

			app.ChangeVisibleLogMessages.BindToButton(MessageType.Information, toolStripMenuItemInfo).AddTo(disposable);
			app.ChangeVisibleLogMessages.BindToButton(MessageType.Warning, toolStripMenuItemWarning).AddTo(disposable);
			app.ChangeVisibleLogMessages.BindToButton(MessageType.Error, toolStripMenuItemError).AddTo(disposable);
			app.ChangeVisibleLogMessages.BindToButton(MessageType.Critical, toolStripMenuItemError).AddTo(disposable);
			app.ClearLogMessages.BindToButton(toolStripMenuItemClear).AddTo(disposable);

			messageDisposable.AddTo(disposable);
			trainDisposable.AddTo(disposable);
			panelDisposable.AddTo(disposable);
			soundDisposable.AddTo(disposable);
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

			comboBoxTouchCommand.Items
				.AddRange(
					Enum.GetValues(typeof(Translations.Command))
						.OfType<Translations.Command>()
						.Select(c => Translations.CommandInfos.TryGetInfo(c).Name)
						.OfType<object>()
						.ToArray()
				);

			Icon = GetIcon();

			toolStripMenuItemError.Image = Bitmap.FromHicon(SystemIcons.Error.Handle);
			toolStripMenuItemWarning.Image = Bitmap.FromHicon(SystemIcons.Warning.Handle);
			toolStripMenuItemInfo.Image = Bitmap.FromHicon(SystemIcons.Information.Handle);

			listViewStatus.SmallImageList = new ImageList();
			listViewStatus.SmallImageList.Images.AddRange(new Image[]
			{
				Bitmap.FromHicon(SystemIcons.Information.Handle),
				Bitmap.FromHicon(SystemIcons.Warning.Handle),
				Bitmap.FromHicon(SystemIcons.Error.Handle),
				Bitmap.FromHicon(SystemIcons.Error.Handle)
			});

			toolStripMenuItemNew.Image = GetImage("new.png");
			toolStripMenuItemOpen.Image = GetImage("open.png");
			toolStripMenuItemSave.Image = GetImage("save.png");

			toolStripButtonUndo.Image = GetImage("undo.png");
			toolStripButtonRedo.Image = GetImage("redo.png");
			toolStripButtonTearingOff.Image = GetImage("cut.png");
			toolStripButtonCopy.Image = GetImage("copy.png");
			toolStripButtonPaste.Image = GetImage("paste.png");
			toolStripButtonDelete.Image = GetImage("delete.png");
			toolStripButtonCleanup.Image = GetImage("cleanup.png");
			toolStripButtonSelect.Image = GetImage("select.png");
			toolStripButtonMove.Image = GetImage("move.png");
			toolStripButtonDot.Image = GetImage("draw.png");
			toolStripButtonLine.Image = GetImage("ruler.png");

			buttonAccelZoomIn.Image = GetImage("zoomin.png");
			buttonAccelZoomOut.Image = GetImage("zoomout.png");
			buttonAccelReset.Image = GetImage("reset.png");

			buttonMotorZoomIn.Image = GetImage("zoomin.png");
			buttonMotorZoomOut.Image = GetImage("zoomout.png");
			buttonMotorReset.Image = GetImage("reset.png");

			buttonDirectDot.Image = GetImage("draw.png");
			buttonDirectMove.Image = GetImage("move.png");

			buttonMotorSwap.Image = GetImage("change.png");
			buttonPlay.Image = GetImage("play.png");
			buttonPause.Image = GetImage("pause.png");
			buttonStop.Image = GetImage("stop.png");

			toolStripMenuItemError.PerformClick();
			toolStripMenuItemWarning.PerformClick();
			toolStripMenuItemInfo.PerformClick();

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

		private void ButtonObjectOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxObject);
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

		private void ButtonThisDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxThisDaytimeImage);
		}

		private void ButtonThisNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxThisNighttimeImage);
		}

		private void ButtonThisTransparentColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxThisTransparentColor);
		}

		private void ButtonPilotLampSubjectSet_Click(object sender, EventArgs e)
		{
			using (FormSubject form = new FormSubject(app.Panel.Value.SelectedPilotLamp.Value.Subject.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonPilotLampDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxPilotLampDaytimeImage);
		}

		private void ButtonPilotLampNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxPilotLampNighttimeImage);
		}

		private void ButtonPilotLampTransparentColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxPilotLampTransparentColor);
		}

		private void ButtonNeedleSubjectSet_Click(object sender, EventArgs e)
		{
			using (FormSubject form = new FormSubject(app.Panel.Value.SelectedNeedle.Value.Subject.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonNeedleDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxNeedleDaytimeImage);
		}

		private void ButtonNeedleNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxNeedleNighttimeImage);
		}

		private void ButtonNeedleColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxNeedleColor);
		}

		private void ButtonNeedleTransparentColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxNeedleTransparentColor);
		}

		private void ButtonDigitalNumberSubjectSet_Click(object sender, EventArgs e)
		{
			using (FormSubject form = new FormSubject(app.Panel.Value.SelectedDigitalNumber.Value.Subject.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonDigitalNumberDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxDigitalNumberDaytimeImage);
		}

		private void ButtonDigitalNumberNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxDigitalNumberNighttimeImage);
		}

		private void ButtonDigitalNumberTransparentColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxDigitalNumberTransparentColor);
		}

		private void ButtonDigitalGaugeSubjectSet_Click(object sender, EventArgs e)
		{
			using (FormSubject form = new FormSubject(app.Panel.Value.SelectedDigitalGauge.Value.Subject.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonDigitalGaugeColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxDigitalGaugeColor);
		}

		private void ButtonLinearGaugeSubjectSet_Click(object sender, EventArgs e)
		{
			using (FormSubject form = new FormSubject(app.Panel.Value.SelectedLinearGauge.Value.Subject.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonLinearGaugeDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxLinearGaugeDaytimeImage);
		}

		private void ButtonLinearGaugeNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxLinearGaugeNighttimeImage);
		}

		private void ButtonLinearGaugeTransparentColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxLinearGaugeTransparentColor);
		}

		private void ButtonTimetableTransparentColorSet_Click(object sender, EventArgs e)
		{
			OpenColorDialog(textBoxTimetableTransparentColor);
		}

		private void ButtonSoundFileNameOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog(textBoxSoundFileName);
		}
	}
}
