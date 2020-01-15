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
			CompositeDisposable messageDisposable = new CompositeDisposable().AddTo(trackDisposable);
			CompositeDisposable toolTipVertexPitchDisposable = new CompositeDisposable().AddTo(trackDisposable);
			CompositeDisposable toolTipVertexVolumeDisposable = new CompositeDisposable().AddTo(trackDisposable);

			CultureInfo culture = CultureInfo.InvariantCulture;

			w.MessageBox
				.Subscribe(x =>
				{
					messageDisposable.Dispose();
					messageDisposable = new CompositeDisposable().AddTo(trackDisposable);

					BindToMessageBox(x).AddTo(messageDisposable);
				})
				.AddTo(trackDisposable);

			w.ToolTipVertexPitch
				.Subscribe(x =>
				{
					toolTipVertexPitchDisposable.Dispose();
					toolTipVertexPitchDisposable = new CompositeDisposable().AddTo(trackDisposable);

					BindToToolTip(x, glControlMotor).AddTo(toolTipVertexPitchDisposable);
				})
				.AddTo(trackDisposable);

			w.ToolTipVertexVolume
				.Subscribe(x =>
				{
					toolTipVertexVolumeDisposable.Dispose();
					toolTipVertexVolumeDisposable = new CompositeDisposable().AddTo(trackDisposable);

					BindToToolTip(x, glControlMotor).AddTo(toolTipVertexVolumeDisposable);
				})
				.AddTo(trackDisposable);

			w.CurrentCursorType
				.BindTo(
					glControlMotor,
					x => x.Cursor,
					CursorTypeToCursor
				)
				.AddTo(trackDisposable);

			w.MinVelocity
				.BindTo(
					textBoxMotorMinVelocity,
					x => x.Text,
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
				.AddTo(trackDisposable);

			w.MinVelocity
				.BindToErrorProvider(errorProvider, textBoxMotorMinVelocity)
				.AddTo(trackDisposable);

			w.MaxVelocity
				.BindTo(
					textBoxMotorMaxVelocity,
					x => x.Text,
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
				.AddTo(trackDisposable);

			w.MaxVelocity
				.BindToErrorProvider(errorProvider, textBoxMotorMaxVelocity)
				.AddTo(trackDisposable);

			w.MinPitch
				.BindTo(
					textBoxMotorMinPitch,
					x => x.Text,
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
				.AddTo(trackDisposable);

			w.MinPitch
				.BindToErrorProvider(errorProvider, textBoxMotorMinPitch)
				.AddTo(trackDisposable);

			w.MaxPitch
				.BindTo(
					textBoxMotorMaxPitch,
					x => x.Text,
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
				.AddTo(trackDisposable);

			w.MaxPitch
				.BindToErrorProvider(errorProvider, textBoxMotorMaxPitch)
				.AddTo(trackDisposable);

			w.MinVolume
				.BindTo(
					textBoxMotorMinVolume,
					x => x.Text,
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
				.AddTo(trackDisposable);

			w.MinVolume
				.BindToErrorProvider(errorProvider, textBoxMotorMinVolume)
				.AddTo(trackDisposable);

			w.MaxVolume
				.BindTo(
					textBoxMotorMaxVolume,
					x => x.Text,
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
				.AddTo(trackDisposable);

			w.MaxVolume
				.BindToErrorProvider(errorProvider, textBoxMotorMaxVolume)
				.AddTo(trackDisposable);

			w.NowVelocity
				.BindTo(
					toolStripStatusLabelX,
					x => x.Text,
					x => $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "velocity")}: {x.ToString("0.00", culture)} km/h"
				)
				.AddTo(trackDisposable);

			w.NowPitch
				.BindTo(
					toolStripStatusLabelY,
					x => x.Text,
					x => w.CurrentInputMode.Value == Motor.InputMode.Pitch ? $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "pitch")}: {x.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
				)
				.AddTo(trackDisposable);

			w.NowVolume
				.BindTo(
					toolStripStatusLabelY,
					x => x.Text,
					x => w.CurrentInputMode.Value == Motor.InputMode.Volume ? $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "volume")}: {x.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
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

			w.CurrentInputMode
				.BindTo(
					toolStripMenuItemPitch,
					x => x.Checked,
					x => x == Motor.InputMode.Pitch
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					toolStripMenuItemVolume,
					x => x.Checked,
					x => x == Motor.InputMode.Volume
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					toolStripMenuItemIndex,
					x => x.Checked,
					x => x == Motor.InputMode.SoundIndex
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					textBoxMotorMinPitch,
					x => x.Enabled,
					x => x == Motor.InputMode.Pitch
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					textBoxMotorMaxPitch,
					x => x.Enabled,
					x => x == Motor.InputMode.Pitch
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					textBoxMotorMinVolume,
					x => x.Enabled,
					x => x == Motor.InputMode.Volume
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					textBoxMotorMaxVolume,
					x => x.Enabled,
					x => x == Motor.InputMode.Volume
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					toolStripStatusLabelMode,
					x => x.Text,
					x =>
					{
						string mode = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "name")}:";
						string text;

						switch (x)
						{
							case Motor.InputMode.Pitch:
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "pitch")}";
								break;
							case Motor.InputMode.Volume:
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "volume")}";
								break;
							case Motor.InputMode.SoundIndex:
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "sound_index")}({w.SelectedSoundIndex.Value})";
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(w), w, null);
						}

						return text;
					})
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					toolStripStatusLabelTool,
					x => x.Enabled,
					x => x != Motor.InputMode.SoundIndex
				)
				.AddTo(trackDisposable);

			w.CurrentInputMode
				.BindTo(
					toolStripStatusLabelY,
					x => x.Enabled,
					x => x != Motor.InputMode.SoundIndex
				)
				.AddTo(trackDisposable);

			w.SelectedSoundIndex
				.BindTo(
					toolStripComboBoxIndex,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => x + 1,
					x => x - 1,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => toolStripComboBoxIndex.SelectedIndexChanged += h,
							h => toolStripComboBoxIndex.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(trackDisposable);

			w.SelectedSoundIndex
				.BindTo(
					toolStripStatusLabelMode,
					x => x.Text,
					BindingMode.OneWay,
					x =>
					{
						string mode = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "name")}:";
						string text = toolStripStatusLabelMode.Text;

						if (w.CurrentInputMode.Value == Motor.InputMode.SoundIndex)
						{
							text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "sound_index")}({x})";
						}

						return text;
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

			w.IsRefreshGlControl
				.Where(x => x)
				.Subscribe(_ => glControlMotor.Invalidate())
				.AddTo(trackDisposable);

			w.Undo.BindToButton(toolStripMenuItemUndo).AddTo(trackDisposable);
			w.Redo.BindToButton(toolStripMenuItemRedo).AddTo(trackDisposable);
			w.Cleanup.BindToButton(toolStripMenuItemCleanup).AddTo(trackDisposable);
			w.Delete.BindToButton(toolStripMenuItemDelete).AddTo(trackDisposable);

			w.Undo.BindToButton(toolStripButtonUndo).AddTo(trackDisposable);
			w.Redo.BindToButton(toolStripButtonRedo).AddTo(trackDisposable);
			w.Cleanup.BindToButton(toolStripButtonCleanup).AddTo(trackDisposable);
			w.Delete.BindToButton(toolStripButtonDelete).AddTo(trackDisposable);

			w.ChangeInputMode.BindToButton(Motor.InputMode.Pitch, toolStripMenuItemPitch).AddTo(trackDisposable);
			w.ChangeInputMode.BindToButton(Motor.InputMode.Volume, toolStripMenuItemVolume).AddTo(trackDisposable);
			w.ChangeInputMode.BindToButton(Motor.InputMode.SoundIndex, toolStripMenuItemIndex).AddTo(trackDisposable);

			w.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripMenuItemSelect).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripMenuItemMove).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripMenuItemDot).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripMenuItemLine).AddTo(trackDisposable);

			w.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripButtonSelect).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripButtonMove).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripButtonDot).AddTo(trackDisposable);
			w.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripButtonLine).AddTo(trackDisposable);

			w.ZoomIn.BindToButton(buttonMotorZoomIn).AddTo(trackDisposable);
			w.ZoomOut.BindToButton(buttonMotorZoomOut).AddTo(trackDisposable);
			w.Reset.BindToButton(buttonMotorReset).AddTo(trackDisposable);

			w.DirectDot.BindToButton(buttonDirectDot).AddTo(trackDisposable);
			w.DirectMove.BindToButton(buttonDirectMove).AddTo(trackDisposable);

			return trackDisposable;
		}

		private IDisposable BindToMotor(MotorViewModel z)
		{
			CompositeDisposable motorDisposable = new CompositeDisposable();
			CompositeDisposable trackDisposable = new CompositeDisposable().AddTo(motorDisposable);

			Binders.BindToTreeView(treeViewMotor, z.TreeItems, z.SelectedTreeItem).AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					menuStripMotor,
					w => w.Enabled,
					w => w != null
				)
				.AddTo(motorDisposable);

			z.SelectedTrack
				.BindTo(
					glControlMotor,
					w => w.Enabled,
					w => w != null
				)
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
					groupBoxView,
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
					tabPageCar,
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

			z.SwapSpeed.BindToButton(buttonMotorSwap).AddTo(motorDisposable);
			z.StartSimulation.BindToButton(buttonPlay).AddTo(motorDisposable);
			z.PauseSimulation.BindToButton(buttonPause).AddTo(motorDisposable);
			z.StopSimulation.BindToButton(buttonStop).AddTo(motorDisposable);

			return motorDisposable;
		}
	}
}
