using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.World;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToMotor(MotorViewModel motor)
		{
			CompositeDisposable motorDisposable = new CompositeDisposable();
			CompositeDisposable trackDisposable = new CompositeDisposable().AddTo(motorDisposable);

			CultureInfo culture = CultureInfo.InvariantCulture;

			WinFormsBinders.BindToTreeView(treeViewMotor, motor.TreeItems, motor.SelectedTreeItem).AddTo(motorDisposable);

			motor.SelectedTreeItem
				.BindTo(
					glControlMotor,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x != null
				)
				.AddTo(motorDisposable);

			motor.SelectedTrack
				.BindTo(
					toolStripMenuItemEdit,
					x => x.Enabled,
					x => x != null)
				.AddTo(motorDisposable);

			motor.SelectedTrack
				.BindTo(
					toolStripMenuItemTool,
					x => x.Enabled,
					x => x != null)
				.AddTo(motorDisposable);

			motor.SelectedTrack
				.BindTo(
					toolStripToolBar,
					x => x.Enabled,
					x => x != null)
				.AddTo(motorDisposable);

			motor.SelectedTrack
				.BindTo(
					groupBoxTrack,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(motorDisposable);

			motor.SelectedTrack
				.BindTo(
					groupBoxDirect,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(motorDisposable);

			motor.SelectedTrack
				.Subscribe(x =>
				{
					trackDisposable.Dispose();
					trackDisposable = new CompositeDisposable().AddTo(motorDisposable);

					if (x != null)
					{
						BindToTrack(x).AddTo(trackDisposable);
					}
				})
				.AddTo(motorDisposable);

			motor.StoppedSim
				.BindTo(
					panelCars,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.StoppedSim
				.BindTo(
					tabPageCar1,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.StoppedSim
				.BindTo(
					tabPageAccel,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.StoppedSim
				.BindTo(
					toolStripMenuItemFile,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.StoppedSim
				.BindTo(
					groupBoxUnit,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.StoppedSim
				.BindTo(
					groupBoxArea,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.VelocityUnit
				.BindTo(
					comboBoxMotorXUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Velocity)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxMotorXUnit.SelectedIndexChanged += h,
							h => comboBoxMotorXUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			motor.VelocityUnit
				.BindTo(
					labelMotorMinVelocityUnit,
					x => x.Text,
					BindingMode.OneWay,
					x => Unit.GetRewords(x).First()
				)
				.AddTo(motorDisposable);

			motor.VelocityUnit
				.BindTo(
					labelMotorMaxVelocityUnit,
					x => x.Text,
					BindingMode.OneWay,
					x => Unit.GetRewords(x).First()
				)
				.AddTo(motorDisposable);

			motor.VelocityUnit
				.BindTo(
					labelDirectXUnit,
					x => x.Text,
					BindingMode.OneWay,
					x => Unit.GetRewords(x).First()
				)
				.AddTo(motorDisposable);

			motor.VelocityUnit
				.BindTo(
					labelMotorAreaUnit,
					x => x.Text,
					BindingMode.OneWay,
					x => Unit.GetRewords(x).First()
				)
				.AddTo(motorDisposable);

			motor.MinVelocity
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
				.AddTo(motorDisposable);

			motor.MinVelocity
				.BindToErrorProvider(errorProvider, textBoxMotorMinVelocity)
				.AddTo(motorDisposable);

			motor.MaxVelocity
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
				.AddTo(motorDisposable);

			motor.MaxVelocity
				.BindToErrorProvider(errorProvider, textBoxMotorMaxVelocity)
				.AddTo(motorDisposable);

			motor.MinPitch
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
				.AddTo(motorDisposable);

			motor.MinPitch
				.BindToErrorProvider(errorProvider, textBoxMotorMinPitch)
				.AddTo(motorDisposable);

			motor.MaxPitch
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
				.AddTo(motorDisposable);

			motor.MaxPitch
				.BindToErrorProvider(errorProvider, textBoxMotorMaxPitch)
				.AddTo(motorDisposable);

			motor.MinVolume
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
				.AddTo(motorDisposable);

			motor.MinVolume
				.BindToErrorProvider(errorProvider, textBoxMotorMinVolume)
				.AddTo(motorDisposable);

			motor.MaxVolume
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
				.AddTo(motorDisposable);

			motor.MaxVolume
				.BindToErrorProvider(errorProvider, textBoxMotorMaxVolume)
				.AddTo(motorDisposable);

			motor.NowVelocity
				.BindTo(
					toolStripStatusLabelX,
					x => x.Text,
					x => $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "velocity")}: {x.ToString(culture)} {Unit.GetRewords(motor.VelocityUnit.Value)}"
				)
				.AddTo(motorDisposable);

			motor.NowPitch
				.BindTo(
					toolStripStatusLabelY,
					x => x.Text,
					x => motor.CurrentInputMode.Value == Motor.InputMode.Pitch ? $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "pitch")}: {x.ToString(culture)} " : toolStripStatusLabelY.Text
				)
				.AddTo(motorDisposable);

			motor.NowVolume
				.BindTo(
					toolStripStatusLabelY,
					x => x.Text,
					x => motor.CurrentInputMode.Value == Motor.InputMode.Volume ? $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "xy", "volume")}: {x.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					toolStripMenuItemPitch,
					x => x.Checked,
					x => x == Motor.InputMode.Pitch
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					toolStripMenuItemVolume,
					x => x.Checked,
					x => x == Motor.InputMode.Volume
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					toolStripMenuItemIndex,
					x => x.Checked,
					x => x == Motor.InputMode.SoundIndex
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					textBoxMotorMinPitch,
					x => x.Enabled,
					x => x == Motor.InputMode.Pitch
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					textBoxMotorMaxPitch,
					x => x.Enabled,
					x => x == Motor.InputMode.Pitch
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					textBoxMotorMinVolume,
					x => x.Enabled,
					x => x == Motor.InputMode.Volume
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					textBoxMotorMaxVolume,
					x => x.Enabled,
					x => x == Motor.InputMode.Volume
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
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
								text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "sound_index")}({motor.SelectedSoundIndex.Value})";
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(x), x, null);
						}

						return text;
					})
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					toolStripStatusLabelTool,
					x => x.Enabled,
					x => x != Motor.InputMode.SoundIndex
				)
				.AddTo(motorDisposable);

			motor.CurrentInputMode
				.BindTo(
					toolStripStatusLabelY,
					x => x.Enabled,
					x => x != Motor.InputMode.SoundIndex
				)
				.AddTo(motorDisposable);

			motor.SelectedSoundIndex
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
				.AddTo(motorDisposable);

			motor.SelectedSoundIndex
				.BindTo(
					toolStripStatusLabelMode,
					x => x.Text,
					BindingMode.OneWay,
					x =>
					{
						string mode = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "name")}:";
						string text = toolStripStatusLabelMode.Text;

						if (motor.CurrentInputMode.Value == Motor.InputMode.SoundIndex)
						{
							text = $"{mode} {Utilities.GetInterfaceString("motor_sound_settings", "status", "mode", "sound_index")}({x})";
						}

						return text;
					}
				)
				.AddTo(motorDisposable);

			motor.IsRefreshGlControl
				.Where(x => x)
				.Subscribe(_ => glControlMotor.Invalidate())
				.AddTo(motorDisposable);

			motor.RunIndex
				.BindTo(
					numericUpDownRunIndex,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownRunIndex.ValueChanged += h,
							h => numericUpDownRunIndex.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			motor.IsLoop
				.BindTo(
					checkBoxMotorLoop,
					x => x.Checked,
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

			motor.IsLoop
				.BindTo(
					checkBoxMotorConstant,
					x => x.Enabled
				)
				.AddTo(motorDisposable);

			motor.IsConstant
				.BindTo(
					checkBoxMotorConstant,
					x => x.Checked,
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

			motor.IsConstant
				.BindTo(
					checkBoxMotorLoop,
					x => x.Enabled,
					BindingMode.OneWay,
					x => !x
				)
				.AddTo(motorDisposable);

			motor.Acceleration
				.BindTo(
					textBoxMotorAccel,
					x => x.Text,
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

			motor.Acceleration
				.BindToErrorProvider(errorProvider, textBoxMotorAccel)
				.AddTo(motorDisposable);

			motor.AccelerationUnit
				.BindTo(
					comboBoxMotorAccelUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Acceleration)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxMotorAccelUnit.SelectedIndexChanged += h,
							h => comboBoxMotorAccelUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			motor.StartSpeed
				.BindTo(
					textBoxMotorAreaLeft,
					x => x.Text,
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

			motor.StartSpeed
				.BindToErrorProvider(errorProvider, textBoxMotorAreaLeft)
				.AddTo(motorDisposable);

			motor.EndSpeed
				.BindTo(
					textBoxMotorAreaRight,
					x => x.Text,
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

			motor.EndSpeed
				.BindToErrorProvider(errorProvider, textBoxMotorAreaRight)
				.AddTo(motorDisposable);

			motor.UpTrack.BindToButton(buttonMotorUp).AddTo(motorDisposable);
			motor.DownTrack.BindToButton(buttonMotorDown).AddTo(motorDisposable);
			motor.AddTrack.BindToButton(buttonMotorAdd).AddTo(motorDisposable);
			motor.RemoveTrack.BindToButton(buttonMotorRemove).AddTo(motorDisposable);
			motor.CopyTrack.BindToButton(buttonMotorCopy).AddTo(motorDisposable);

			motor.ZoomIn.BindToButton(buttonMotorZoomIn).AddTo(motorDisposable);
			motor.ZoomOut.BindToButton(buttonMotorZoomOut).AddTo(motorDisposable);
			motor.Reset.BindToButton(buttonMotorReset).AddTo(motorDisposable);

			motor.ChangeInputMode.BindToButton(Motor.InputMode.Pitch, toolStripMenuItemPitch).AddTo(motorDisposable);
			motor.ChangeInputMode.BindToButton(Motor.InputMode.Volume, toolStripMenuItemVolume).AddTo(motorDisposable);
			motor.ChangeInputMode.BindToButton(Motor.InputMode.SoundIndex, toolStripMenuItemIndex).AddTo(motorDisposable);

			motor.SwapSpeed.BindToButton(buttonMotorSwap).AddTo(motorDisposable);
			motor.StartSimulation.BindToButton(buttonPlay).AddTo(motorDisposable);
			motor.PauseSimulation.BindToButton(buttonPause).AddTo(motorDisposable);
			motor.StopSimulation.BindToButton(buttonStop).AddTo(motorDisposable);

			return motorDisposable;
		}

		private IDisposable BindToTrack(MotorViewModel.TrackViewModel track)
		{
			CompositeDisposable trackDisposable = new CompositeDisposable();

			track.MessageBox.BindToMessageBox().AddTo(trackDisposable);

			track.ToolTipVertexPitch.BindToToolTip(glControlMotor).AddTo(trackDisposable);

			track.ToolTipVertexVolume.BindToToolTip(glControlMotor).AddTo(trackDisposable);

			track.CurrentCursorType
				.BindTo(
					glControlMotor,
					x => x.Cursor,
					WinFormsUtilities.CursorTypeToCursor
				)
				.AddTo(trackDisposable);

			track.Type
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

			track.Type
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

			track.CurrentToolMode
				.BindTo(
					toolStripMenuItemSelect,
					x => x.Checked,
					x => x == Motor.ToolMode.Select
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripMenuItemMove,
					x => x.Checked,
					x => x == Motor.ToolMode.Move
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripMenuItemDot,
					x => x.Checked,
					x => x == Motor.ToolMode.Dot
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripMenuItemLine,
					x => x.Checked,
					x => x == Motor.ToolMode.Line
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripButtonSelect,
					x => x.Checked,
					x => x == Motor.ToolMode.Select
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripButtonMove,
					x => x.Checked,
					x => x == Motor.ToolMode.Move
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripButtonDot,
					x => x.Checked,
					x => x == Motor.ToolMode.Dot
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
				.BindTo(
					toolStripButtonLine,
					x => x.Checked,
					x => x == Motor.ToolMode.Line
				)
				.AddTo(trackDisposable);

			track.CurrentToolMode
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

			track.StoppedSim
				.BindTo(
					groupBoxTrack,
					x => x.Enabled
				)
				.AddTo(trackDisposable);

			track.EnabledDirect
				.BindTo(
					groupBoxDirect,
					x => x.Enabled
				)
				.AddTo(trackDisposable);

			track.DirectX
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

			track.DirectX
				.BindToErrorProvider(errorProvider, textBoxDirectX)
				.AddTo(trackDisposable);

			track.DirectY
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

			track.DirectY
				.BindToErrorProvider(errorProvider, textBoxDirectY)
				.AddTo(trackDisposable);

			track.Undo.BindToButton(toolStripMenuItemUndo).AddTo(trackDisposable);
			track.Redo.BindToButton(toolStripMenuItemRedo).AddTo(trackDisposable);
			track.Cleanup.BindToButton(toolStripMenuItemCleanup).AddTo(trackDisposable);
			track.Delete.BindToButton(toolStripMenuItemDelete).AddTo(trackDisposable);

			track.Undo.BindToButton(toolStripButtonUndo).AddTo(trackDisposable);
			track.Redo.BindToButton(toolStripButtonRedo).AddTo(trackDisposable);
			track.Cleanup.BindToButton(toolStripButtonCleanup).AddTo(trackDisposable);
			track.Delete.BindToButton(toolStripButtonDelete).AddTo(trackDisposable);

			track.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripMenuItemSelect).AddTo(trackDisposable);
			track.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripMenuItemMove).AddTo(trackDisposable);
			track.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripMenuItemDot).AddTo(trackDisposable);
			track.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripMenuItemLine).AddTo(trackDisposable);

			track.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripButtonSelect).AddTo(trackDisposable);
			track.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripButtonMove).AddTo(trackDisposable);
			track.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripButtonDot).AddTo(trackDisposable);
			track.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripButtonLine).AddTo(trackDisposable);

			track.DirectDot.BindToButton(buttonDirectDot).AddTo(trackDisposable);
			track.DirectMove.BindToButton(buttonDirectMove).AddTo(trackDisposable);

			return trackDisposable;
		}
	}
}
