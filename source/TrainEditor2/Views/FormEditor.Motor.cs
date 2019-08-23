using System;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
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
			CompositeDisposable messageDisposable = new CompositeDisposable();
			CompositeDisposable toolTipVertexPitchDisposable = new CompositeDisposable();
			CompositeDisposable toolTipVertexVolumeDisposable = new CompositeDisposable();

			var culture = CultureInfo.InvariantCulture;

			z.MessageBox
				.Subscribe(w =>
				{
					messageDisposable.Dispose();
					messageDisposable = new CompositeDisposable();

					BindToMessageBox(w).AddTo(messageDisposable);
				})
				.AddTo(motorDisposable);

			z.ToolTipVertexPitch
				.Subscribe(w =>
				{
					toolTipVertexPitchDisposable.Dispose();
					toolTipVertexPitchDisposable = new CompositeDisposable();

					BindToToolTip(w, pictureBoxDrawArea).AddTo(toolTipVertexPitchDisposable);
				})
				.AddTo(motorDisposable);

			z.ToolTipVertexVolume
				.Subscribe(w =>
				{
					toolTipVertexVolumeDisposable.Dispose();
					toolTipVertexVolumeDisposable = new CompositeDisposable();

					BindToToolTip(w, pictureBoxDrawArea).AddTo(toolTipVertexVolumeDisposable);
				})
				.AddTo(motorDisposable);

			z.CurrentCursorType
				.BindTo(
					pictureBoxDrawArea,
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
					w => $"Velocity: {w.ToString("0.00", culture)} km/h"
				)
				.AddTo(motorDisposable);

			z.NowPitch
				.BindTo(
					toolStripStatusLabelY,
					w => w.Text,
					w => z.CurrentInputMode.Value == Motor.InputMode.Pitch ? $"Pitch: {w.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
				)
				.AddTo(motorDisposable);

			z.NowVolume
				.BindTo(
					toolStripStatusLabelY,
					w => w.Text,
					w => z.CurrentInputMode.Value == Motor.InputMode.Volume ? $"Volume: {w.ToString("0.00", culture)} " : toolStripStatusLabelY.Text
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
						string type = "Type:";
						string power = "Power";
						string brake = "Brake";

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
						string track = "Track:";

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
						string mode = "Mode:";
						string text;

						switch (w)
						{
							case Motor.InputMode.Pitch:
								text = $"{mode} Pitch";
								break;
							case Motor.InputMode.Volume:
								text = $"{mode} Volume";
								break;
							case Motor.InputMode.SoundIndex:
								text = $"{mode} Sound source index({z.SelectedSoundIndex.Value})";
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
						string mode = "Mode:";
						string text = toolStripStatusLabelMode.Text;

						if (z.CurrentInputMode.Value == Motor.InputMode.SoundIndex)
						{
							text = $"{mode} Sound source index({w})";
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
						string tool = "Tool:";
						string text;

						switch (w)
						{
							case Motor.ToolMode.Select:
								text = $"{tool} Select";
								break;
							case Motor.ToolMode.Move:
								text = $"{tool} Move";
								break;
							case Motor.ToolMode.Dot:
								text = $"{tool} Dot";
								break;
							case Motor.ToolMode.Line:
								text = $"{tool} Line";
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(w), w, null);
						}

						return text;
					}
				)
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

			z.ImageWidth
				.BindTo(
					pictureBoxDrawArea,
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

			z.ImageWidth.Value = pictureBoxDrawArea.Width;

			z.ImageHeight
				.BindTo(
					pictureBoxDrawArea,
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

			z.ImageHeight.Value = pictureBoxDrawArea.Height;

			z.Image
				.Subscribe(x =>
				{
					pictureBoxDrawArea.Image = x;
					pictureBoxDrawArea.Refresh();
				})
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

			messageDisposable.AddTo(motorDisposable);
			toolTipVertexPitchDisposable.AddTo(motorDisposable);
			toolTipVertexVolumeDisposable.AddTo(motorDisposable);

			return motorDisposable;
		}
	}
}
