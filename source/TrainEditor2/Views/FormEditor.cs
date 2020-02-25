using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Security.Permissions;
using System.Windows.Forms;
using OpenBveApi.Interface;
using OpenBveApi.Units;
using OpenTK.Graphics.OpenGL;
using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using TrainEditor2.ViewModels;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor : Form
	{
		private readonly CompositeDisposable disposable;

		private readonly AppViewModel app;

		internal FormEditor(AppViewModel app)
		{
			UIDispatcherScheduler.Initialize();

			disposable = new CompositeDisposable();

			CompositeDisposable trainDisposable = new CompositeDisposable().AddTo(disposable);
			CompositeDisposable soundDisposable = new CompositeDisposable().AddTo(disposable);

			this.app = app;

			InitializeComponent();

			app.SaveLocation
				.BindTo(
					this,
					x => x.Text,
					x => string.IsNullOrEmpty(x) ? $@"NewFile - {Application.ProductName}" : $@"{Path.GetFileName(x)} - {Application.ProductName}"
				)
				.AddTo(disposable);

			app.MessageBox.BindToMessageBox().AddTo(disposable);

			app.OpenFileDialog.BindToOpenFileDialog(this).AddTo(disposable);

			app.SaveFileDialog.BindToSaveFileDialog(this).AddTo(disposable);

			app.Train
				.Subscribe(x =>
				{
					trainDisposable.Dispose();
					trainDisposable = new CompositeDisposable().AddTo(disposable);

					BindToTrain(x).AddTo(trainDisposable);
				})
				.AddTo(disposable);

			app.Sound
				.Subscribe(x =>
				{
					soundDisposable.Dispose();
					soundDisposable = new CompositeDisposable().AddTo(disposable);

					BindToSound(x).AddTo(soundDisposable);
				})
				.AddTo(disposable);

			WinFormsBinders.BindToTreeView(treeViewCars, app.TreeItems, app.SelectedTreeItem).AddTo(disposable);

			app.SelectedTreeItem
				.Subscribe(x =>
				{
					if (x == app.TreeItems[0].Children[0])
					{
						if (!tabControlEditor.TabPages.Contains(tabPageTrain))
						{
							tabControlEditor.TabPages.Add(tabPageTrain);
						}

						if (!tabControlEditor.TabPages.Contains(tabPageSound))
						{
							tabControlEditor.TabPages.Add(tabPageSound);
						}
					}
					else
					{
						tabControlEditor.TabPages.Remove(tabPageTrain);
						tabControlEditor.TabPages.Remove(tabPageSound);
					}

					if (app.TreeItems[0].Children[1].Children.Contains(x))
					{
						if (!tabControlEditor.TabPages.Contains(tabPageCar1))
						{
							tabControlEditor.TabPages.Add(tabPageCar1);
						}

						if (!tabControlEditor.TabPages.Contains(tabPageCar2))
						{
							tabControlEditor.TabPages.Add(tabPageCar2);
						}

						if (x?.Tag.Value is MotorCar)
						{
							if (!tabControlEditor.TabPages.Contains(tabPageAccel))
							{
								tabControlEditor.TabPages.Add(tabPageAccel);
							}

							if (!tabControlEditor.TabPages.Contains(tabPageMotor))
							{
								tabControlEditor.TabPages.Add(tabPageMotor);
							}
						}
						else
						{
							tabControlEditor.TabPages.Remove(tabPageAccel);
							tabControlEditor.TabPages.Remove(tabPageMotor);
						}

						if ((x?.Tag.Value as ControlledMotorCar)?.Cab is EmbeddedCab || (x?.Tag.Value as ControlledTrailerCar)?.Cab is EmbeddedCab)
						{
							if (!tabControlEditor.TabPages.Contains(tabPagePanel))
							{
								tabControlEditor.TabPages.Add(tabPagePanel);
							}
						}
						else
						{
							tabControlEditor.TabPages.Remove(tabPagePanel);
						}
					}
					else
					{
						tabControlEditor.TabPages.Remove(tabPageCar1);
						tabControlEditor.TabPages.Remove(tabPageCar2);
						tabControlEditor.TabPages.Remove(tabPageAccel);
						tabControlEditor.TabPages.Remove(tabPageMotor);
						tabControlEditor.TabPages.Remove(tabPagePanel);
					}

					if (app.TreeItems[0].Children[2].Children.Contains(x))
					{
						if (!tabControlEditor.TabPages.Contains(tabPageCoupler))
						{
							tabControlEditor.TabPages.Add(tabPageCoupler);
						}
					}
					else
					{
						tabControlEditor.TabPages.Remove(tabPageCoupler);
					}
				})
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

			Interface.LogMessages
				.CollectionChangedAsObservable()
				.Subscribe(_ =>
				{
					tabPageStatus.Text = $@"{Utilities.GetInterfaceString("status", "name")} ({Interface.LogMessages.Count})";
					toolStripMenuItemError.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Error || m.Type == MessageType.Critical)} {Utilities.GetInterfaceString("status", "menu", "error")}";
					toolStripMenuItemWarning.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Warning)} {Utilities.GetInterfaceString("status", "menu", "warning")}";
					toolStripMenuItemInfo.Text = $@"{Interface.LogMessages.Count(m => m.Type == MessageType.Information)} {Utilities.GetInterfaceString("status", "menu", "info")}";
				})
				.AddTo(disposable);

			WinFormsBinders.BindToListViewItemCollection(listViewStatus, app.VisibleLogMessages, listViewStatus.Items).AddTo(disposable);

			app.CreateNewFile.BindToButton(toolStripMenuItemNew).AddTo(disposable);
			app.OpenFile.BindToButton(toolStripMenuItemOpen).AddTo(disposable);
			app.SaveFile.BindToButton(toolStripMenuItemSave).AddTo(disposable);
			app.SaveAsFile.BindToButton(toolStripMenuItemSaveAs).AddTo(disposable);

			new[] { app.UpCar, app.UpCoupler }.BindToButton(buttonCarsUp).AddTo(disposable);
			new[] { app.DownCar, app.DownCoupler }.BindToButton(buttonCarsDown).AddTo(disposable);
			app.AddCar.BindToButton(buttonCarsAdd).AddTo(disposable);
			app.CopyCar.BindToButton(buttonCarsCopy).AddTo(disposable);
			app.RemoveCar.BindToButton(buttonCarsRemove).AddTo(disposable);

			app.ChangeBaseCarClass.BindToCheckBox(checkBoxIsMotorCar).AddTo(disposable);
			app.ChangeControlledCarClass.BindToCheckBox(checkBoxIsControlledCar).AddTo(disposable);
			app.ChangeCabClass.BindToCheckBox(checkBoxIsEmbeddedCab).AddTo(disposable);

			app.ChangeVisibleLogMessages.BindToButton(MessageType.Information, toolStripMenuItemInfo).AddTo(disposable);
			app.ChangeVisibleLogMessages.BindToButton(MessageType.Warning, toolStripMenuItemWarning).AddTo(disposable);
			app.ChangeVisibleLogMessages.BindToButton(MessageType.Error, toolStripMenuItemError).AddTo(disposable);
			app.ChangeVisibleLogMessages.BindToButton(MessageType.Critical, toolStripMenuItemError).AddTo(disposable);
			app.ClearLogMessages.BindToButton(toolStripMenuItemClear).AddTo(disposable);
			app.OutputLogs.BindToButton(buttonOutputLogs).AddTo(disposable);
		}

		private void FormEditor_Load(object sender, EventArgs e)
		{
			MinimumSize = Size;

			ComboBox comboBox = toolStripComboBoxIndex.ComboBox;

			if (comboBox != null)
			{
				comboBox.Items.AddRange(Enumerable.Range(-1, 1024).OfType<object>().ToArray());
				comboBox.DrawMode = DrawMode.OwnerDrawFixed;
				comboBox.DrawItem += ToolStripComboBoxIndex_DrawItem;
			}

			string[] massUnits = Enum.GetValues(typeof(Unit.Mass)).OfType<Enum>().Select(x => Unit.GetRewords(x).First()).ToArray();
			string[] lengthUnits = Enum.GetValues(typeof(Unit.Length)).OfType<Enum>().Select(x => Unit.GetRewords(x).First()).ToArray();
			string[] pressureRateUnits = Enum.GetValues(typeof(Unit.PressureRate)).OfType<Enum>().Select(x => Unit.GetRewords(x).First()).ToArray();
			string[] pressureUnits = Enum.GetValues(typeof(Unit.Pressure)).OfType<Enum>().Select(x => Unit.GetRewords(x).First()).ToArray();

			comboBoxMassUnit.Items.AddRange((string[])massUnits.Clone());
			comboBoxLengthUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxWidthUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxHeightUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCenterOfMassHeightUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxFrontAxleUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxRearAxleUnit.Items.AddRange((string[])lengthUnits.Clone());

			comboBoxCabXUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCabYUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCabZUnit.Items.AddRange((string[])lengthUnits.Clone());

			comboBoxCameraRestrictionForwardsUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCameraRestrictionBackwardsUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCameraRestrictionLeftUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCameraRestrictionRightUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCameraRestrictionUpUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCameraRestrictionDownUnit.Items.AddRange((string[])lengthUnits.Clone());

			comboBoxCouplerMinUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxCouplerMaxUnit.Items.AddRange((string[])lengthUnits.Clone());

			comboBoxCompressorRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());

			comboBoxMainReservoirMinimumPressureUnit.Items.AddRange((string[])pressureUnits.Clone());
			comboBoxMainReservoirMaximumPressureUnit.Items.AddRange((string[])pressureUnits.Clone());

			comboBoxAuxiliaryReservoirChargeRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());

			comboBoxEqualizingReservoirChargeRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxEqualizingReservoirServiceRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxEqualizingReservoirEmergencyRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());

			comboBoxBrakePipeNormalPressureUnit.Items.AddRange((string[])pressureUnits.Clone());
			comboBoxBrakePipeChargeRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxBrakePipeServiceRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxBrakePipeEmergencyRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());

			comboBoxStraightAirPipeServiceRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxStraightAirPipeEmergencyRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxStraightAirPipeReleaseRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());

			comboBoxBrakeCylinderServiceMaximumPressureUnit.Items.AddRange((string[])pressureUnits.Clone());
			comboBoxBrakeCylinderEmergencyMaximumPressureUnit.Items.AddRange((string[])pressureUnits.Clone());
			comboBoxBrakeCylinderEmergencyRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());
			comboBoxBrakeCylinderReleaseRateUnit.Items.AddRange((string[])pressureRateUnits.Clone());

			Icon = WinFormsUtilities.GetIcon();

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

			toolStripMenuItemNew.Image = WinFormsUtilities.GetImage("new.png");
			toolStripMenuItemOpen.Image = WinFormsUtilities.GetImage("open.png");
			toolStripMenuItemSave.Image = WinFormsUtilities.GetImage("save.png");

			toolStripButtonUndo.Image = WinFormsUtilities.GetImage("undo.png");
			toolStripButtonRedo.Image = WinFormsUtilities.GetImage("redo.png");
			toolStripButtonDelete.Image = WinFormsUtilities.GetImage("delete.png");
			toolStripButtonCleanup.Image = WinFormsUtilities.GetImage("cleanup.png");
			toolStripButtonSelect.Image = WinFormsUtilities.GetImage("select.png");
			toolStripButtonMove.Image = WinFormsUtilities.GetImage("move.png");
			toolStripButtonDot.Image = WinFormsUtilities.GetImage("draw.png");
			toolStripButtonLine.Image = WinFormsUtilities.GetImage("ruler.png");

			buttonAccelZoomIn.Image = WinFormsUtilities.GetImage("zoomin.png");
			buttonAccelZoomOut.Image = WinFormsUtilities.GetImage("zoomout.png");
			buttonAccelReset.Image = WinFormsUtilities.GetImage("reset.png");

			buttonMotorZoomIn.Image = WinFormsUtilities.GetImage("zoomin.png");
			buttonMotorZoomOut.Image = WinFormsUtilities.GetImage("zoomout.png");
			buttonMotorReset.Image = WinFormsUtilities.GetImage("reset.png");

			buttonDirectDot.Image = WinFormsUtilities.GetImage("draw.png");
			buttonDirectMove.Image = WinFormsUtilities.GetImage("move.png");

			buttonMotorSwap.Image = WinFormsUtilities.GetImage("change.png");
			buttonPlay.Image = WinFormsUtilities.GetImage("play.png");
			buttonPause.Image = WinFormsUtilities.GetImage("pause.png");
			buttonStop.Image = WinFormsUtilities.GetImage("stop.png");

			toolStripMenuItemError.PerformClick();
			toolStripMenuItemWarning.PerformClick();
			toolStripMenuItemInfo.PerformClick();

			Translations.CurrentLanguageCode = Interface.CurrentOptions.LanguageCode;
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			Translations.ListLanguages(toolStripComboBoxLanguage.ComboBox);

			if (app.CurrentLanguageCode.Value == "en-US")
			{
				app.CurrentLanguageCode.ForceNotify();
			}
		}

		private void FormEditor_Resize(object sender, EventArgs e)
		{
			Motor.GlControlWidth = glControlMotor.Width;
			Motor.GlControlHeight = glControlMotor.Height;
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
			}

			if (keyData == Keys.Right)
			{
				if (pictureBoxAccel.Focused)
				{
					car?.Acceleration.Value.MoveRight.Execute();
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
			}

			if (keyData == Keys.Up)
			{
				if (pictureBoxAccel.Focused)
				{
					car?.Acceleration.Value.MoveTop.Execute();
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

		private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			switch (MessageBox.Show(Utilities.GetInterfaceString("menu", "message", "exit"), Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Cancel:
					e.Cancel = true;
					return;
				case DialogResult.Yes:
					if (app.SaveFile.CanExecute())
					{
						app.SaveFile.Execute();
					}
					else
					{
						app.SaveAsFile.Execute();
					}

					glControlMotor.MakeCurrent();
					Program.Renderer.Finalization();
					break;
			}

			Interface.CurrentOptions.LanguageCode = app.CurrentLanguageCode.Value;
		}

		private void ToolStripMenuItemImportTrain_Click(object sender, EventArgs e)
		{
			using (FormImportTrain form = new FormImportTrain(app.ImportTrainFile.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ToolStripMenuItemImportPanel_Click(object sender, EventArgs e)
		{
			using (FormImportPanel form = new FormImportPanel(app.ImportPanelFile.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ToolStripMenuItemImportSound_Click(object sender, EventArgs e)
		{
			using (FormImportSound form = new FormImportSound(app.ImportSoundFile.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ToolStripMenuItemExportTrain_Click(object sender, EventArgs e)
		{
			using (FormExportTrain form = new FormExportTrain(app.ExportTrainFile.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ToolStripMenuItemExportPanel_Click(object sender, EventArgs e)
		{
			using (FormExportPanel form = new FormExportPanel(app.ExportPanelFile.Value, app.Train.Value.Cars))
			{
				form.ShowDialog(this);
			}
		}

		private void ToolStripMenuItemExportSound_Click(object sender, EventArgs e)
		{
			using (FormExportSound form = new FormExportSound(app.ExportSoundFile.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void ToolStripComboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			string currentLanguageCode = app.CurrentLanguageCode.Value;

			if (Translations.SelectedLanguage(ref currentLanguageCode, toolStripComboBoxLanguage.ComboBox))
			{
				ApplyLanguage();
			}

			app.CurrentLanguageCode.Value = currentLanguageCode;
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

		private void ButtonDelayPowerSet_Click(object sender, EventArgs e)
		{
			using (FormDelay form = new FormDelay(app.Train.Value.SelectedCar.Value.Delay.Value.Power))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonDelayBrakeSet_Click(object sender, EventArgs e)
		{
			using (FormDelay form = new FormDelay(app.Train.Value.SelectedCar.Value.Delay.Value.Brake))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonDelayLocoBrakeSet_Click(object sender, EventArgs e)
		{
			using (FormDelay form = new FormDelay(app.Train.Value.SelectedCar.Value.Delay.Value.LocoBrake))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonJerkPowerSet_Click(object sender, EventArgs e)
		{
			using (FormJerk form = new FormJerk(app.Train.Value.SelectedCar.Value.Jerk.Value.Power.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonJerkBrakeSet_Click(object sender, EventArgs e)
		{
			using (FormJerk form = new FormJerk(app.Train.Value.SelectedCar.Value.Jerk.Value.Brake.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonObjectOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxObject);
		}

		private void ButtonLeftDoorSet_Click(object sender, EventArgs e)
		{
			using (FormDoor form = new FormDoor(app.Train.Value.SelectedCar.Value.LeftDoor.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonRightDoorSet_Click(object sender, EventArgs e)
		{
			using (FormDoor form = new FormDoor(app.Train.Value.SelectedCar.Value.RightDoor.Value))
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

			car?.Acceleration.Value.MouseMove.Execute(WinFormsUtilities.MouseEventArgsToModel(e));
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

		private void GlControlMotor_Load(object sender, EventArgs e)
		{
			glControlMotor.MakeCurrent();
			Program.Renderer.Initialize(Program.CurrentHost, Interface.CurrentOptions);
		}

		private void GlControlMotor_Resize(object sender, EventArgs e)
		{
			Motor.GlControlWidth = glControlMotor.Width;
			Motor.GlControlHeight = glControlMotor.Height;
		}

		private void GlControlMotor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Up:
				case Keys.Left:
				case Keys.Right:
				case Keys.Down:
					e.IsInputKey = true;
					break;
			}
		}

		private void GlControlMotor_KeyDown(object sender, KeyEventArgs e)
		{
			MotorViewModel motor = (app.Train.Value.SelectedCar.Value as MotorCarViewModel)?.Motor.Value;

			switch (e.KeyCode)
			{
				case Keys.Left:
					motor?.MoveLeft.Execute();
					break;
				case Keys.Right:
					motor?.MoveRight.Execute();
					break;
				case Keys.Down:
					motor?.MoveBottom.Execute();
					break;
				case Keys.Up:
					motor?.MoveTop.Execute();
					break;
			}
		}

		private void GlControlMotor_MouseEnter(object sender, EventArgs e)
		{
			glControlMotor.Focus();
		}

		private void GlControlMotor_MouseDown(object sender, MouseEventArgs e)
		{
			MotorViewModel.TrackViewModel track = (app.Train.Value.SelectedCar.Value as MotorCarViewModel)?.Motor.Value.SelectedTrack.Value;

			track?.MouseDown.Execute(WinFormsUtilities.MouseEventArgsToModel(e));
		}

		private void GlControlMotor_MouseMove(object sender, MouseEventArgs e)
		{
			MotorViewModel motor = (app.Train.Value.SelectedCar.Value as MotorCarViewModel)?.Motor.Value;

			motor?.MouseMove.Execute(WinFormsUtilities.MouseEventArgsToModel(e));
		}

		private void GlControlMotor_MouseUp(object sender, MouseEventArgs e)
		{
			MotorViewModel.TrackViewModel track = (app.Train.Value.SelectedCar.Value as MotorCarViewModel)?.Motor.Value.SelectedTrack.Value;

			track?.MouseUp.Execute();
		}

		private void GlControlMotor_Paint(object sender, PaintEventArgs e)
		{
			MotorViewModel motor = (app.Train.Value.SelectedCar.Value as MotorCarViewModel)?.Motor.Value;

			glControlMotor.MakeCurrent();

			if (motor != null)
			{
				motor.DrawGlControl.Execute();
			}
			else
			{
				GL.ClearColor(Color.Black);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}

			glControlMotor.SwapBuffers();
		}

		private void ButtonThisDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxThisDaytimeImage);
		}

		private void ButtonThisNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxThisNighttimeImage);
		}

		private void ButtonThisTransparentColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxThisTransparentColor);
		}

		private void ButtonPilotLampSubjectSet_Click(object sender, EventArgs e)
		{
			SetSubject(x => x.SelectedPilotLamp.Value.Subject.Value);
		}

		private void ButtonPilotLampDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxPilotLampDaytimeImage);
		}

		private void ButtonPilotLampNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxPilotLampNighttimeImage);
		}

		private void ButtonPilotLampTransparentColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxPilotLampTransparentColor);
		}

		private void ButtonNeedleSubjectSet_Click(object sender, EventArgs e)
		{
			SetSubject(x => x.SelectedNeedle.Value.Subject.Value);
		}

		private void ButtonNeedleDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxNeedleDaytimeImage);
		}

		private void ButtonNeedleNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxNeedleNighttimeImage);
		}

		private void ButtonNeedleColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxNeedleColor);
		}

		private void ButtonNeedleTransparentColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxNeedleTransparentColor);
		}

		private void ButtonDigitalNumberSubjectSet_Click(object sender, EventArgs e)
		{
			SetSubject(x => x.SelectedDigitalNumber.Value.Subject.Value);
		}

		private void ButtonDigitalNumberDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxDigitalNumberDaytimeImage);
		}

		private void ButtonDigitalNumberNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxDigitalNumberNighttimeImage);
		}

		private void ButtonDigitalNumberTransparentColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxDigitalNumberTransparentColor);
		}

		private void ButtonDigitalGaugeSubjectSet_Click(object sender, EventArgs e)
		{
			SetSubject(x => x.SelectedDigitalGauge.Value.Subject.Value);
		}

		private void ButtonDigitalGaugeColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxDigitalGaugeColor);
		}

		private void ButtonLinearGaugeSubjectSet_Click(object sender, EventArgs e)
		{
			SetSubject(x => x.SelectedLinearGauge.Value.Subject.Value);
		}

		private void ButtonLinearGaugeDaytimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxLinearGaugeDaytimeImage);
		}

		private void ButtonLinearGaugeNighttimeImageOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxLinearGaugeNighttimeImage);
		}

		private void ButtonLinearGaugeTransparentColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxLinearGaugeTransparentColor);
		}

		private void ButtonTimetableTransparentColorSet_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenColorDialog(textBoxTimetableTransparentColor);
		}

		private void ButtonTouchSoundCommand_Click(object sender, EventArgs e)
		{
			EmbeddedCabViewModel embeddedCab = null;
			ControlledMotorCarViewModel controlledMotorCar = app.Train.Value.SelectedCar.Value as ControlledMotorCarViewModel;
			ControlledTrailerCarViewModel controlledTrailerCar = app.Train.Value.SelectedCar.Value as ControlledTrailerCarViewModel;

			if (controlledMotorCar != null)
			{
				embeddedCab = controlledMotorCar.Cab.Value as EmbeddedCabViewModel;
			}

			if (controlledTrailerCar != null)
			{
				embeddedCab = controlledTrailerCar.Cab.Value as EmbeddedCabViewModel;
			}

			if (embeddedCab == null)
			{
				return;
			}

			using (FormTouch form = new FormTouch(embeddedCab.Panel.Value.SelectedTouch.Value))
			{
				form.ShowDialog(this);
			}
		}

		private void ButtonCabFileNameOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxCabFileName);
		}

		private void ButtonCouplerObject_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxCouplerObject);
		}

		private void ButtonSoundFileNameOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxSoundFileName);
		}
	}
}
