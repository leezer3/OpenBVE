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
		private IDisposable BindToMotor(MotorViewModel z)
		{
			CompositeDisposable motorDisposable = new CompositeDisposable();
			CompositeDisposable messageDisposable = new CompositeDisposable().AddTo(motorDisposable);
			CompositeDisposable toolTipVertexPitchDisposable = new CompositeDisposable().AddTo(motorDisposable);
			CompositeDisposable toolTipVertexVolumeDisposable = new CompositeDisposable().AddTo(motorDisposable);

			CultureInfo culture = CultureInfo.InvariantCulture;

			z.MessageBox
				.Subscribe(w =>
				{
					messageDisposable.Dispose();
					messageDisposable = new CompositeDisposable().AddTo(motorDisposable);

					BindToMessageBox(w).AddTo(messageDisposable);
				})
				.AddTo(motorDisposable);

			z.ToolTipVertexPitch
				.Subscribe(w =>
				{
					toolTipVertexPitchDisposable.Dispose();
					toolTipVertexPitchDisposable = new CompositeDisposable().AddTo(motorDisposable);

					BindToToolTip(w, glControlMotor).AddTo(toolTipVertexPitchDisposable);
				})
				.AddTo(motorDisposable);

			z.ToolTipVertexVolume
				.Subscribe(w =>
				{
					toolTipVertexVolumeDisposable.Dispose();
					toolTipVertexVolumeDisposable = new CompositeDisposable().AddTo(motorDisposable);

					BindToToolTip(w, glControlMotor).AddTo(toolTipVertexVolumeDisposable);
				})
				.AddTo(motorDisposable);

			z.CurrentCursorType
				.BindTo(
					glControlMotor,
					w => w.Cursor,
					CursorTypeToCursor
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

			z.CurrentSelectedTrack
				.BindTo(
					toolStripMenuItemPowerTrack1,
					w => w.Checked,
					w => w == Motor.TrackInfo.Power1
				)
				.AddTo(motorDisposable);

			z.CurrentSelectedTrack
				.BindTo(
					toolStripMenuItemPowerTrack2,
					w => w.Checked,
					w => w == Motor.TrackInfo.Power2
				)
				.AddTo(motorDisposable);

			z.CurrentSelectedTrack
				.BindTo(
					toolStripMenuItemBrakeTrack1,
					w => w.Checked,
					w => w == Motor.TrackInfo.Brake1
				)
				.AddTo(motorDisposable);

			z.CurrentSelectedTrack
				.BindTo(
					toolStripMenuItemBrakeTrack2,
					w => w.Checked,
					w => w == Motor.TrackInfo.Brake2
				)
				.AddTo(motorDisposable);

			z.CurrentSelectedTrack
				.BindTo(
					toolStripStatusLabelType,
					w => w.Text,
					w =>
					{
						string type = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "type", "name")}:";
						string power = Utilities.GetInterfaceString("motor_sound_settings", "status", "type", "power");
						string brake = Utilities.GetInterfaceString("motor_sound_settings", "status", "type", "brake");

						return $"{type} {(w == Motor.TrackInfo.Power1 || w == Motor.TrackInfo.Power2 ? power : brake)}";
					}
				)
				.AddTo(motorDisposable);

			z.CurrentSelectedTrack
				.BindTo(
					toolStripStatusLabelTrack,
					w => w.Text,
					w =>
					{
						string track = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "track", "name")}:";

						return $"{track} {(w == Motor.TrackInfo.Power1 || w == Motor.TrackInfo.Brake1 ? 1 : 2)}";
					}
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

			z.CurrentToolMode
				.BindTo(
					toolStripMenuItemSelect,
					w => w.Checked,
					w => w == Motor.ToolMode.Select
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripMenuItemMove,
					w => w.Checked,
					w => w == Motor.ToolMode.Move
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripMenuItemDot,
					w => w.Checked,
					w => w == Motor.ToolMode.Dot
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripMenuItemLine,
					w => w.Checked,
					w => w == Motor.ToolMode.Line
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripButtonSelect,
					w => w.Checked,
					w => w == Motor.ToolMode.Select
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripButtonMove,
					w => w.Checked,
					w => w == Motor.ToolMode.Move
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripButtonDot,
					w => w.Checked,
					w => w == Motor.ToolMode.Dot
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripButtonLine,
					w => w.Checked,
					w => w == Motor.ToolMode.Line
				)
				.AddTo(motorDisposable);

			z.CurrentToolMode
				.BindTo(
					toolStripStatusLabelTool,
					w => w.Text,
					w =>
					{
						string tool = $"{Utilities.GetInterfaceString("motor_sound_settings", "status", "tool", "name")}:";
						string text;

						switch (w)
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
								throw new ArgumentOutOfRangeException(nameof(w), w, null);
						}

						return text;
					}
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
					panelCars,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.StoppedSim
				.BindTo(
					groupBoxArea,
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

			z.IsPlayTrack1
				.BindTo(
					checkBoxTrack1,
					w => w.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxTrack1.CheckedChanged += h,
							h => checkBoxTrack1.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.IsPlayTrack2
				.BindTo(
					checkBoxTrack2,
					w => w.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxTrack2.CheckedChanged += h,
							h => checkBoxTrack2.CheckedChanged -= h
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

			z.EnabledDirect
				.BindTo(
					groupBoxDirect,
					w => w.Enabled
				)
				.AddTo(motorDisposable);

			z.DirectX
				.BindTo(
					textBoxDirectX,
					w => w.Text,
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
				.AddTo(motorDisposable);

			z.DirectX
				.BindToErrorProvider(errorProvider, textBoxDirectX)
				.AddTo(motorDisposable);

			z.DirectY
				.BindTo(
					textBoxDirectY,
					w => w.Text,
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
				.AddTo(motorDisposable);

			z.DirectY
				.BindToErrorProvider(errorProvider, textBoxDirectY)
				.AddTo(motorDisposable);

			z.GlControlWidth
				.BindTo(
					glControlMotor,
					w => w.Width,
					BindingMode.OneWayToSource,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => Resize += h,
							h => Resize -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.GlControlWidth.Value = glControlMotor.Width;

			z.GlControlHeight
				.BindTo(
					glControlMotor,
					w => w.Height,
					BindingMode.OneWayToSource,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => Resize += h,
							h => Resize -= h
						)
						.ToUnit()
				)
				.AddTo(motorDisposable);

			z.GlControlHeight.Value = glControlMotor.Height;

			z.IsRefreshGlControl
				.Where(x => x)
				.Subscribe(_ => glControlMotor.Invalidate())
				.AddTo(motorDisposable);

			z.Undo.BindToButton(toolStripMenuItemUndo).AddTo(motorDisposable);
			z.Redo.BindToButton(toolStripMenuItemRedo).AddTo(motorDisposable);
			z.TearingOff.BindToButton(toolStripMenuItemTearingOff).AddTo(motorDisposable);
			z.Copy.BindToButton(toolStripMenuItemCopy).AddTo(motorDisposable);
			z.Paste.BindToButton(toolStripMenuItemPaste).AddTo(motorDisposable);
			z.Cleanup.BindToButton(toolStripMenuItemCleanup).AddTo(motorDisposable);
			z.Delete.BindToButton(toolStripMenuItemDelete).AddTo(motorDisposable);

			z.Undo.BindToButton(toolStripButtonUndo).AddTo(motorDisposable);
			z.Redo.BindToButton(toolStripButtonRedo).AddTo(motorDisposable);
			z.TearingOff.BindToButton(toolStripButtonTearingOff).AddTo(motorDisposable);
			z.Copy.BindToButton(toolStripButtonCopy).AddTo(motorDisposable);
			z.Paste.BindToButton(toolStripButtonPaste).AddTo(motorDisposable);
			z.Cleanup.BindToButton(toolStripButtonCleanup).AddTo(motorDisposable);
			z.Delete.BindToButton(toolStripButtonDelete).AddTo(motorDisposable);

			z.ChangeSelectedTrack.BindToButton(Motor.TrackInfo.Power1, toolStripMenuItemPowerTrack1).AddTo(motorDisposable);
			z.ChangeSelectedTrack.BindToButton(Motor.TrackInfo.Power2, toolStripMenuItemPowerTrack2).AddTo(motorDisposable);
			z.ChangeSelectedTrack.BindToButton(Motor.TrackInfo.Brake1, toolStripMenuItemBrakeTrack1).AddTo(motorDisposable);
			z.ChangeSelectedTrack.BindToButton(Motor.TrackInfo.Brake2, toolStripMenuItemBrakeTrack2).AddTo(motorDisposable);

			z.ChangeInputMode.BindToButton(Motor.InputMode.Pitch, toolStripMenuItemPitch).AddTo(motorDisposable);
			z.ChangeInputMode.BindToButton(Motor.InputMode.Volume, toolStripMenuItemVolume).AddTo(motorDisposable);
			z.ChangeInputMode.BindToButton(Motor.InputMode.SoundIndex, toolStripMenuItemIndex).AddTo(motorDisposable);

			z.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripMenuItemSelect).AddTo(motorDisposable);
			z.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripMenuItemMove).AddTo(motorDisposable);
			z.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripMenuItemDot).AddTo(motorDisposable);
			z.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripMenuItemLine).AddTo(motorDisposable);

			z.ChangeToolMode.BindToButton(Motor.ToolMode.Select, toolStripButtonSelect).AddTo(motorDisposable);
			z.ChangeToolMode.BindToButton(Motor.ToolMode.Move, toolStripButtonMove).AddTo(motorDisposable);
			z.ChangeToolMode.BindToButton(Motor.ToolMode.Dot, toolStripButtonDot).AddTo(motorDisposable);
			z.ChangeToolMode.BindToButton(Motor.ToolMode.Line, toolStripButtonLine).AddTo(motorDisposable);

			z.ZoomIn.BindToButton(buttonMotorZoomIn).AddTo(motorDisposable);
			z.ZoomOut.BindToButton(buttonMotorZoomOut).AddTo(motorDisposable);
			z.Reset.BindToButton(buttonMotorReset).AddTo(motorDisposable);

			z.DirectDot.BindToButton(buttonDirectDot).AddTo(motorDisposable);
			z.DirectMove.BindToButton(buttonDirectMove).AddTo(motorDisposable);

			z.SwapSpeed.BindToButton(buttonMotorSwap).AddTo(motorDisposable);
			z.StartSimulation.BindToButton(buttonPlay).AddTo(motorDisposable);
			z.PauseSimulation.BindToButton(buttonPause).AddTo(motorDisposable);
			z.StopSimulation.BindToButton(buttonStop).AddTo(motorDisposable);

			return motorDisposable;
		}
	}
}
