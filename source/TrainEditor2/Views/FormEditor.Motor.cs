using System;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToTrack(MotorViewModel.TrackViewModel w)
		{
			CompositeDisposable trackDisposable = new CompositeDisposable();

			w.MessageBox.BindToMessageBox().AddTo(trackDisposable);

			w.ToolTipVertexPitch.BindToToolTip(glControlMotor).AddTo(trackDisposable);

			w.ToolTipVertexVolume.BindToToolTip(glControlMotor).AddTo(trackDisposable);

			w.CurrentCursorType
				.BindTo(
					glControlMotor,
					x => x.Cursor,
					WinFormsUtilities.CursorTypeToCursor
				)
				.AddTo(trackDisposable);

			w.Type
				.BindTo(
					comboBoxTrackType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Motor.TrackType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxTrackType.SelectedIndexChanged += h,
							h => comboBoxTrackType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(trackDisposable);

			w.Type
				.BindTo(
					toolStripStatusLabelType,
					x => x.Text,
					BindingMode.OneWay,
					x =>
					{
						string type = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "type", "name")}:";
						string power = Utilities.GetInterfaceString("motor_sound_settings", "status", "type", "power");
						string brake = Utilities.GetInterfaceString("motor_sound_settings", "status", "type", "brake");

						return $"{type} {(x == Motor.TrackType.Power ? power : brake)}";
					}
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripMenuItemSelect,
					x => x.Checked,
					x => x == Motor.ToolMode.Select
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripMenuItemMove,
					x => x.Checked,
					x => x == Motor.ToolMode.Move
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripMenuItemDot,
					x => x.Checked,
					x => x == Motor.ToolMode.Dot
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripMenuItemLine,
					x => x.Checked,
					x => x == Motor.ToolMode.Line
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripButtonSelect,
					x => x.Checked,
					x => x == Motor.ToolMode.Select
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripButtonMove,
					x => x.Checked,
					x => x == Motor.ToolMode.Move
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripButtonDot,
					x => x.Checked,
					x => x == Motor.ToolMode.Dot
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripButtonLine,
					x => x.Checked,
					x => x == Motor.ToolMode.Line
				)
				.AddTo(trackDisposable);

			w.CurrentToolMode
				.BindTo(
					toolStripStatusLabelTool,
					x => x.Text,
					x =>
					{
						string tool = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "tool", "name")}:";
						string text;

						switch (x)
						{
							case Motor.ToolMode.Select:
								text = $"{tool} {Utilities.GetInterfaceString("motor_sound_settings", "status", "tool", "select")}";
								break;
							case Motor.ToolMode.Move:
								text = $"{tool} {Utilities.GetInterfaceString("motor_sound_settings", "status", "tool", "move")}";
								break;
							case Motor.ToolMode.Dot:
								text = $"{tool} {Utilities.GetInterfaceString("motor_sound_settings", "status", "tool", "dot")}";
								break;
							case Motor.ToolMode.Line:
								text = $"{tool} {Utilities.GetInterfaceString("motor_sound_settings", "status", "tool", "line")}";
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(x), x, null);
						}

						return text;
					}
				)
				.AddTo(trackDisposable);

			w.StoppedSim
				.BindTo(
					groupBoxTrack,
					x => x.Enabled
				)
				.AddTo(trackDisposable);

			w.EnabledDirect
				.BindTo(
					groupBoxDirect,
					x => x.Enabled
				)
				.AddTo(trackDisposable);

			w.DirectX
				.BindTo(
					textBoxDirectX,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDirectX.TextChanged += h,
							h => textBoxDirectX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(trackDisposable);

			w.DirectX
				.BindToErrorProvider(errorProvider, textBoxDirectX)
				.AddTo(trackDisposable);

			w.DirectY
				.BindTo(
					textBoxDirectY,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDirectY.TextChanged += h,
							h => textBoxDirectY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(trackDisposable);

			w.DirectY
				.BindToErrorProvider(errorProvider, textBoxDirectY)
				.AddTo(trackDisposable);

			w.Undo.BindToButton(toolStripMenuItemUndo).AddTo(trackDisposable);
			w.Redo.BindToButton(toolStripMenuItemRedo).AddTo(trackDisposable);
			w.Cleanup.BindToButton(toolStripMenuItemCleanup).AddTo(trackDisposable);
			w.Delete.BindToButton(toolStripMenuItemDelete).AddTo(trackDisposable);

			w.Undo.BindToButton(toolStripButtonUndo).AddTo(trackDisposable);
			w.Redo.BindToButton(toolStripButtonRedo).AddTo(trackDisposable);
			w.Cleanup.BindToButton(toolStripButtonCleanup).AddTo(trackDisposable);
			w.Delete.BindToButton(toolStripButtonDelete).AddTo(trackDisposable);

			w.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripMenuItemSelect).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripMenuItemMove).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripMenuItemDot).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripMenuItemLine).AddTo(trackDisposable);

			w.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripButtonSelect).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripButtonMove).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripButtonDot).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripButtonLine).AddTo(trackDisposable);

			w.DirectDot.BindToButton(buttonDirectDot).AddTo(trackDisposable);
			w.DirectMove.BindToButton(buttonDirectMove).AddTo(trackDisposable);

			return trackDisposable;
		}

		private IDisposable BindToMotor(MotorViewModel z)
		{
			CompositeDisposable motorDisposable = new CompositeDisposable();
			CompositeDisposable trackDisposable = new CompositeDisposable().AddTo(motorDisposable);

			CultureInfo culture = CultureInfo.InvariantCulture;

			WinFormsBinders.BindToTreeView(treeViewMotor, z.TreeItems, z.SelectedTreeItem).AddTo(motorDisposable);

			z.SelectedTreeItem
				.BindTo(
					glControlMotor,
					w => w.Enabled,
					BindingMode.OneWay,
					w => w != null
				)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					toolStripMenuItemEdit,
					w => w.Enabled,
					w => w != null)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					toolStripMenuItemTool,
					w => w.Enabled,
					w => w != null)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					toolStripToolBar,
					w => w.Enabled,
					w => w != null)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					groupBoxTrack,
					w => w.Enabled,
					w => w != null
				)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					groupBoxDirect,
					w => w.Enabled,
					w => w != null
				)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.Subscribe(w =>
				{
					trackDisposable.Dispose();
					trackDisposable = new CompositeDisposable().AddTo(motorDisposable);

					if (w != null)
					{
						BindToTrack(w).AddTo(trackDisposable);
					}
				})
				.AddTo(motorDisposable);

			z.StoppedSim
				.BindTo(
					panelCars,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.StoppedSim
				.BindTo(
					tabPageCar1,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.StoppedSim
				.BindTo(
					tabPageAccel,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.StoppedSim
				.BindTo(
					toolStripMenuItemFile,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.StoppedSim
				.BindTo(
					groupBoxArea,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.MinVelocity
				.BindTo(
					textBoxMotorMinVelocity,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorMinVelocity.TextChanged += h,
							h => textBoxMotorMinVelocity.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.MinVelocity
				.BindToErrorProvider(errorProvider, textBoxMotorMinVelocity)
				.AddTo(motorDisposable);

			z.MaxVelocity
				.BindTo(
					textBoxMotorMaxVelocity,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorMaxVelocity.TextChanged += h,
							h => textBoxMotorMaxVelocity.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.MaxVelocity
				.BindToErrorProvider(errorProvider, textBoxMotorMaxVelocity)
				.AddTo(motorDisposable);

			z.MinPitch
				.BindTo(
					textBoxMotorMinPitch,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorMinPitch.TextChanged += h,
							h => textBoxMotorMinPitch.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.MinPitch
				.BindToErrorProvider(errorProvider, textBoxMotorMinPitch)
				.AddTo(motorDisposable);

			z.MaxPitch
				.BindTo(
					textBoxMotorMaxPitch,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorMaxPitch.TextChanged += h,
							h => textBoxMotorMaxPitch.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.MaxPitch
				.BindToErrorProvider(errorProvider, textBoxMotorMaxPitch)
				.AddTo(motorDisposable);

			z.MinVolume
				.BindTo(
					textBoxMotorMinVolume,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorMinVolume.TextChanged += h,
							h => textBoxMotorMinVolume.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.MinVolume
				.BindToErrorProvider(errorProvider, textBoxMotorMinVolume)
				.AddTo(motorDisposable);

			z.MaxVolume
				.BindTo(
					textBoxMotorMaxVolume,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorMaxVolume.TextChanged += h,
							h => textBoxMotorMaxVolume.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.MaxVolume
				.BindToErrorProvider(errorProvider, textBoxMotorMaxVolume)
				.AddTo(motorDisposable);

			z.NowVelocity
				.BindTo(
					toolStripStatusLabelX,
					w => w.Text,
					w => $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "velocity")}: {w.ToString("0.00", culture)} km/h"
				)
				.AddTo(motorDisposable);

			z.NowPitch
				.BindTo(
					toolStripStatusLabelY,
					w => w.Text,
					w => z.CurrentInputMode.Value == Motor.InputMode.Pitch ? $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "pitch")}: {w.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
				)
				.AddTo(motorDisposable);

			z.NowVolume
				.BindTo(
					toolStripStatusLabelY,
					w => w.Text,
					w => z.CurrentInputMode.Value == Motor.InputMode.Volume ? $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "volume")}: {w.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					toolStripMenuItemPitch,
					w => w.Checked,
					w => w == Motor.InputMode.Pitch
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					toolStripMenuItemVolume,
					w => w.Checked,
					w => w == Motor.InputMode.Volume
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					toolStripMenuItemIndex,
					w => w.Checked,
					w => w == Motor.InputMode.SoundIndex
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					textBoxMotorMinPitch,
					w => w.Enabled,
					w => w == Motor.InputMode.Pitch
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					textBoxMotorMaxPitch,
					w => w.Enabled,
					w => w == Motor.InputMode.Pitch
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					textBoxMotorMinVolume,
					w => w.Enabled,
					w => w == Motor.InputMode.Volume
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					textBoxMotorMaxVolume,
					w => w.Enabled,
					w => w == Motor.InputMode.Volume
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					toolStripStatusLabelMode,
					w => w.Text,
					w =>
					{
						string mode = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "name")}:";
						string text;

						switch (w)
						{
							case Motor.InputMode.Pitch:
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "pitch")}";
								break;
							case Motor.InputMode.Volume:
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "volume")}";
								break;
							case Motor.InputMode.SoundIndex:
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "sound_index")}({z.SelectedSoundIndex.Value})";
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(w), w, null);
						}

						return text;
					})
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					toolStripStatusLabelTool,
					w => w.Enabled,
					w => w != Motor.InputMode.SoundIndex
				)
				.AddTo(motorDisposable);

			z.CurrentInputMode
				.BindTo(
					toolStripStatusLabelY,
					w => w.Enabled,
					w => w != Motor.InputMode.SoundIndex
				)
				.AddTo(motorDisposable);

			z.SelectedSoundIndex
				.BindTo(
					toolStripComboBoxIndex,
					w => w.SelectedIndex,
					BindingMode.TwoWay,
					w => w + 1,
					w => w - 1,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => toolStripComboBoxIndex.SelectedIndexChanged += h,
							h => toolStripComboBoxIndex.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.SelectedSoundIndex
				.BindTo(
					toolStripStatusLabelMode,
					w => w.Text,
					BindingMode.OneWay,
					w =>
					{
						string mode = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "name")}:";
						string text = toolStripStatusLabelMode.Text;

						if (z.CurrentInputMode.Value == Motor.InputMode.SoundIndex)
						{
							text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "sound_index")}({w})";
						}

						return text;
					}
				)
				.AddTo(motorDisposable);

			z.IsRefreshGlControl
				.Where(w => w)
				.Subscribe(_ => glControlMotor.Invalidate())
				.AddTo(motorDisposable);

			z.RunIndex
				.BindTo(
					numericUpDownRunIndex,
					w => w.Value,
					BindingMode.TwoWay,
					null,
					w => (int)w,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownRunIndex.ValueChanged += h,
							h => numericUpDownRunIndex.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.IsLoop
				.BindTo(
					checkBoxMotorLoop,
					w => w.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxMotorLoop.CheckedChanged += h,
							h => checkBoxMotorLoop.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.IsLoop
				.BindTo(
					checkBoxMotorConstant,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.IsConstant
				.BindTo(
					checkBoxMotorConstant,
					w => w.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxMotorConstant.CheckedChanged += h,
							h => checkBoxMotorConstant.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.IsConstant
				.BindTo(
					checkBoxMotorLoop,
					w => w.Enabled,
					BindingMode.OneWay,
					w => !w
				)
				.AddTo(motorDisposable);

			z.Acceleration
				.BindTo(
					textBoxMotorAccel,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorAccel.TextChanged += h,
							h => textBoxMotorAccel.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.Acceleration
				.BindToErrorProvider(errorProvider, textBoxMotorAccel)
				.AddTo(motorDisposable);

			z.StartSpeed
				.BindTo(
					textBoxMotorAreaLeft,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorAreaLeft.TextChanged += h,
							h => textBoxMotorAreaLeft.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.StartSpeed
				.BindToErrorProvider(errorProvider, textBoxMotorAreaLeft)
				.AddTo(motorDisposable);

			z.EndSpeed
				.BindTo(
					textBoxMotorAreaRight,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMotorAreaRight.TextChanged += h,
							h => textBoxMotorAreaRight.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.EndSpeed
				.BindToErrorProvider(errorProvider, textBoxMotorAreaRight)
				.AddTo(motorDisposable);

			z.UpTrack.BindToButton(buttonMotorUp).AddTo(motorDisposable);
			z.DownTrack.BindToButton(buttonMotorDown).AddTo(motorDisposable);
			z.AddTrack.BindToButton(buttonMotorAdd).AddTo(motorDisposable);
			z.RemoveTrack.BindToButton(buttonMotorRemove).AddTo(motorDisposable);
			z.CopyTrack.BindToButton(buttonMotorCopy).AddTo(motorDisposable);

			z.ZoomIn.BindToButton(buttonMotorZoomIn).AddTo(motorDisposable);
			z.ZoomOut.BindToButton(buttonMotorZoomOut).AddTo(motorDisposable);
			z.Reset.BindToButton(buttonMotorReset).AddTo(motorDisposable);

			z.ChangeInputMode.BindToButton(Motor.InputMode.Pitch, toolStripMenuItemPitch).AddTo(motorDisposable);
			z.ChangeInputMode.BindToButton(Motor.InputMode.Volume, toolStripMenuItemVolume).AddTo(motorDisposable);
			z.ChangeInputMode.BindToButton(Motor.InputMode.SoundIndex, toolStripMenuItemIndex).AddTo(motorDisposable);

			z.SwapSpeed.BindToButton(buttonMotorSwap).AddTo(motorDisposable);
			z.StartSimulation.BindToButton(buttonPlay).AddTo(motorDisposable);
			z.PauseSimulation.BindToButton(buttonPause).AddTo(motorDisposable);
			z.StopSimulation.BindToButton(buttonStop).AddTo(motorDisposable);

			return motorDisposable;
		}
	}
}
