using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Dialogs;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.ViewModels.Trains
{
	internal class MotorViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<MessageBoxViewModel> MessageBox
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ToolTipViewModel> ToolTipVertexPitch
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ToolTipViewModel> ToolTipVertexVolume
		{
			get;
		}

		internal ReactiveProperty<InputEventModel.ModifierKeys> CurrentModifierKeys
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<InputEventModel.CursorType> CurrentCursorType
		{
			get;
		}

		internal ReactiveProperty<string> MinVelocity
		{
			get;
		}

		internal ReactiveProperty<string> MaxVelocity
		{
			get;
		}

		internal ReactiveProperty<string> MinPitch
		{
			get;
		}

		internal ReactiveProperty<string> MaxPitch
		{
			get;
		}

		internal ReactiveProperty<string> MinVolume
		{
			get;
		}

		internal ReactiveProperty<string> MaxVolume
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<double> NowVelocity
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<double> NowPitch
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<double> NowVolume
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<Motor.TrackInfo> CurrentSelectedTrack
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<Motor.InputMode> CurrentInputMode
		{
			get;
		}

		internal ReactiveProperty<int> SelectedSoundIndex
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<Motor.ToolMode> CurrentToolMode
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<bool> StoppedSim
		{
			get;
		}

		internal ReactiveProperty<int> RunIndex
		{
			get;
		}

		internal ReactiveProperty<bool> IsPlayTrack1
		{
			get;
		}

		internal ReactiveProperty<bool> IsPlayTrack2
		{
			get;
		}

		internal ReactiveProperty<bool> IsLoop
		{
			get;
		}

		internal ReactiveProperty<bool> IsConstant
		{
			get;
		}

		internal ReactiveProperty<string> Acceleration
		{
			get;
		}

		internal ReactiveProperty<string> StartSpeed
		{
			get;
		}

		internal ReactiveProperty<string> EndSpeed
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<bool> EnabledDirect
		{
			get;
		}

		internal ReactiveProperty<string> DirectX
		{
			get;
		}

		internal ReactiveProperty<string> DirectY
		{
			get;
		}

		internal ReactiveProperty<int> GlControlWidth
		{
			get;
		}

		internal ReactiveProperty<int> GlControlHeight
		{
			get;
		}

		internal ReactiveProperty<bool> IsRefreshGlControl
		{
			get;
		}

		internal ReactiveCommand<Motor.TrackInfo> ChangeSelectedTrack
		{
			get;
		}

		internal ReactiveCommand<Motor.InputMode> ChangeInputMode
		{
			get;
		}

		internal ReactiveCommand<Motor.ToolMode> ChangeToolMode
		{
			get;
		}

		internal ReactiveCommand ZoomIn
		{
			get;
		}

		internal ReactiveCommand ZoomOut
		{
			get;
		}

		internal ReactiveCommand Reset
		{
			get;
		}

		internal ReactiveCommand MoveLeft
		{
			get;
		}

		internal ReactiveCommand MoveRight
		{
			get;
		}

		internal ReactiveCommand MoveBottom
		{
			get;
		}

		internal ReactiveCommand MoveTop
		{
			get;
		}

		internal ReactiveCommand Undo
		{
			get;
		}

		internal ReactiveCommand Redo
		{
			get;
		}

		internal ReactiveCommand TearingOff
		{
			get;
		}

		internal ReactiveCommand Copy
		{
			get;
		}

		internal ReactiveCommand Paste
		{
			get;
		}

		internal ReactiveCommand Cleanup
		{
			get;
		}

		internal ReactiveCommand Delete
		{
			get;
		}

		internal ReactiveCommand<InputEventModel.EventArgs> MouseDown
		{
			get;
		}

		internal ReactiveCommand<InputEventModel.EventArgs> MouseMove
		{
			get;
		}

		internal ReactiveCommand MouseUp
		{
			get;
		}

		internal ReactiveCommand DirectDot
		{
			get;
		}

		internal ReactiveCommand DirectMove
		{
			get;
		}

		internal ReactiveCommand SwapSpeed
		{
			get;
		}

		internal ReactiveTimer SimulationTimer
		{
			get;
		}

		internal ReactiveCommand StartSimulation
		{
			get;
		}

		internal ReactiveCommand PauseSimulation
		{
			get;
		}

		internal ReactiveCommand StopSimulation
		{
			get;
		}

		internal ReactiveCommand DrawGlControl
		{
			get;
		}

		internal MotorViewModel(Motor motor)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			MessageBox = motor
				.ObserveProperty(x => x.MessageBox)
				.Do(_ => MessageBox?.Value.Dispose())
				.Select(x => new MessageBoxViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ToolTipVertexPitch = motor
				.ObserveProperty(x => x.ToolTipVertexPitch)
				.Do(_ => ToolTipVertexPitch?.Value.Dispose())
				.Select(x => new ToolTipViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ToolTipVertexVolume = motor
				.ObserveProperty(x => x.ToolTipVertexVolume)
				.Do(_ => ToolTipVertexVolume?.Value.Dispose())
				.Select(x => new ToolTipViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentModifierKeys = motor
				.ToReactivePropertyAsSynchronized(x => x.CurrentModifierKeys)
				.AddTo(disposable);

			CurrentCursorType = motor
				.ObserveProperty(x => x.CurrentCursorType)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			MinVelocity = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MinVelocity,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinVelocity.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MaxVelocity = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MaxVelocity,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxVelocity.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MinPitch = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MinPitch,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinPitch.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MaxPitch = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MaxPitch,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxPitch.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MinVolume = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MinVolume,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinVolume.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MaxVolume = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MaxVolume,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxVolume.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			NowVelocity = motor
				.ObserveProperty(x => x.NowVelocity)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			NowPitch = motor
				.ObserveProperty(x => x.NowPitch)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			NowVolume = motor
				.ObserveProperty(x => x.NowVolume)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentSelectedTrack = motor
				.ObserveProperty(x => x.SelectedTrackInfo)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentSelectedTrack
				.Subscribe(_ =>
				{
					motor.ResetSelect();
					motor.IsRefreshGlControl = true;
				})
				.AddTo(disposable);

			CurrentInputMode = motor
				.ObserveProperty(x => x.CurrentInputMode)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentInputMode
				.Subscribe(_ =>
				{
					motor.ResetSelect();
					motor.IsRefreshGlControl = true;
				})
				.AddTo(disposable);

			SelectedSoundIndex = motor
				.ToReactivePropertyAsSynchronized(x => x.SelectedSoundIndex)
				.AddTo(disposable);

			CurrentToolMode = motor
				.ObserveProperty(x => x.CurrentToolMode)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentToolMode
				.Subscribe(_ =>
				{
					motor.ResetSelect();
					motor.IsRefreshGlControl = true;
				})
				.AddTo(disposable);

			StoppedSim = motor
				.ObserveProperty(x => x.CurrentSimState)
				.Select(x => x == Motor.SimulationState.Disable || x == Motor.SimulationState.Stopped)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			RunIndex = motor
				.ToReactivePropertyAsSynchronized(x => x.RunIndex)
				.AddTo(disposable);

			IsPlayTrack1 = motor
				.ToReactivePropertyAsSynchronized(x => x.IsPlayTrack1)
				.AddTo(disposable);

			IsPlayTrack2 = motor
				.ToReactivePropertyAsSynchronized(x => x.IsPlayTrack2)
				.AddTo(disposable);

			IsLoop = motor
				.ToReactivePropertyAsSynchronized(x => x.IsLoop)
				.AddTo(disposable);

			IsConstant = motor
				.ToReactivePropertyAsSynchronized(x => x.IsConstant)
				.AddTo(disposable);

			Acceleration = motor
				.ToReactivePropertyAsSynchronized(
					x => x.Acceleration,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			StartSpeed = motor
				.ToReactivePropertyAsSynchronized(
					x => x.StartSpeed,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			EndSpeed = motor
				.ToReactivePropertyAsSynchronized(
					x => x.EndSpeed,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			EnabledDirect = new[]
				{
					CurrentInputMode.Select(x => x != Motor.InputMode.SoundIndex),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			DirectX = new ReactiveProperty<string>(0.0.ToString(culture))
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					switch (CurrentToolMode.Value)
					{
						case Motor.ToolMode.Move:
							Utilities.TryParse(x, NumberRange.Any, out result, out message);
							break;
						case Motor.ToolMode.Dot:
							Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);
							break;
						default:
							message = null;
							break;
					}

					return message;
				})
				.AddTo(disposable);

			DirectY = new ReactiveProperty<string>(0.0.ToString(culture))
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					switch (CurrentToolMode.Value)
					{
						case Motor.ToolMode.Move:
							Utilities.TryParse(x, NumberRange.Any, out result, out message);
							break;
						case Motor.ToolMode.Dot:
							Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);
							break;
						default:
							message = null;
							break;
					}

					return message;
				})
				.AddTo(disposable);

			CurrentToolMode
				.Subscribe(_ =>
				{
					DirectX.ForceValidate();
					DirectY.ForceValidate();
				})
				.AddTo(disposable);

			GlControlWidth = motor
				.ToReactivePropertyAsSynchronized(
					x => x.GlControlWidth,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => x <= 0 ? string.Empty : null)
				.AddTo(disposable);

			GlControlWidth.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			GlControlHeight = motor
				.ToReactivePropertyAsSynchronized(
					x => x.GlControlHeight,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => x <= 0 ? string.Empty : null)
				.AddTo(disposable);

			GlControlHeight.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			IsRefreshGlControl = motor
				.ToReactivePropertyAsSynchronized(x => x.IsRefreshGlControl)
				.AddTo(disposable);

			ChangeSelectedTrack = new ReactiveCommand<Motor.TrackInfo>().WithSubscribe(x => motor.SelectedTrackInfo = x).AddTo(disposable);

			ChangeInputMode = new ReactiveCommand<Motor.InputMode>().WithSubscribe(x => motor.CurrentInputMode = x).AddTo(disposable);

			ChangeToolMode = new[]
				{
					CurrentInputMode.Select(x => x != Motor.InputMode.SoundIndex),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand<Motor.ToolMode>()
				.WithSubscribe(x => motor.CurrentToolMode = x)
				.AddTo(disposable);

			ZoomIn = new ReactiveCommand().WithSubscribe(motor.ZoomIn).AddTo(disposable);

			ZoomOut = new ReactiveCommand().WithSubscribe(motor.ZoomOut).AddTo(disposable);

			Reset = new ReactiveCommand().WithSubscribe(motor.Reset).AddTo(disposable);

			MoveLeft = new ReactiveCommand().WithSubscribe(motor.MoveLeft).AddTo(disposable);

			MoveRight = new ReactiveCommand().WithSubscribe(motor.MoveRight).AddTo(disposable);

			MoveBottom = new ReactiveCommand().WithSubscribe(motor.MoveBottom).AddTo(disposable);

			MoveTop = new ReactiveCommand().WithSubscribe(motor.MoveTop).AddTo(disposable);

			Undo = new[]
				{
					new[]
						{
							motor.PropertyChangedAsObservable().Where(x => x.PropertyName == nameof(motor.SelectedTrackInfo)).OfType<object>(),
							motor.PrevTrackStates.CollectionChangedAsObservable().OfType<object>()
						}
						.Merge()
						.Select(_ => motor.PrevTrackStates.Any(x => x.Info == motor.SelectedTrackInfo)),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand(false)
				.WithSubscribe(motor.Undo)
				.AddTo(disposable);

			Redo = new[]
				{
					new[]
						{
							motor.PropertyChangedAsObservable().Where(x => x.PropertyName == nameof(motor.SelectedTrackInfo)).OfType<object>(),
							motor.NextTrackStates.CollectionChangedAsObservable().OfType<object>()
						}
						.Merge()
						.Select(_ => motor.NextTrackStates.Any(x => x.Info == motor.SelectedTrackInfo)),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand(false)
				.WithSubscribe(motor.Redo)
				.AddTo(disposable);

			TearingOff = StoppedSim
				.ToReactiveCommand()
				.WithSubscribe(motor.TearingOff)
				.AddTo(disposable);

			Copy = StoppedSim
				.ToReactiveCommand()
				.WithSubscribe(motor.Copy)
				.AddTo(disposable);

			Paste = new[]
				{
					motor.ObserveProperty(x => x.CopyTrack).Select(x => x != null),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(motor.Paste)
				.AddTo(disposable);

			Cleanup = StoppedSim
				.ToReactiveCommand()
				.WithSubscribe(motor.Cleanup)
				.AddTo(disposable);

			Delete = StoppedSim
				.ToReactiveCommand()
				.WithSubscribe(motor.Delete)
				.AddTo(disposable);

			MouseDown = new ReactiveCommand<InputEventModel.EventArgs>().WithSubscribe(motor.MouseDown).AddTo(disposable);

			MouseMove = new ReactiveCommand<InputEventModel.EventArgs>().WithSubscribe(motor.MouseMove).AddTo(disposable);

			MouseUp = new ReactiveCommand().WithSubscribe(motor.MouseUp).AddTo(disposable);

			DirectDot = new[]
				{
					StoppedSim,
					CurrentToolMode.Select(x => x == Motor.ToolMode.Dot),
					DirectX.ObserveHasErrors.Select(x => !x),
					DirectY.ObserveHasErrors.Select(x => !x)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(() => motor.DirectDot(double.Parse(DirectX.Value), double.Parse(DirectY.Value)))
				.AddTo(disposable);

			DirectMove = new[]
				{
					StoppedSim,
					CurrentToolMode.Select(x => x == Motor.ToolMode.Move),
					DirectX.ObserveHasErrors.Select(x => !x),
					DirectY.ObserveHasErrors.Select(x => !x)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(() => motor.DirectMove(double.Parse(DirectX.Value), double.Parse(DirectY.Value)))
				.AddTo(disposable);

			SwapSpeed = StoppedSim
				.ToReactiveCommand()
				.WithSubscribe(() =>
				{
					string tmp = StartSpeed.Value;
					StartSpeed.Value = EndSpeed.Value;
					EndSpeed.Value = tmp;
				})
				.AddTo(disposable);

			SimulationTimer = new ReactiveTimer(TimeSpan.FromMilliseconds(1000.0 / 30.0));

			SimulationTimer
				.Subscribe(_ =>
				{
					SimulationTimer.Stop();
					Observable.Start(motor.RunSimulation, UIDispatcherScheduler.Default).Wait();
					SimulationTimer.Start(TimeSpan.FromMilliseconds(1000.0 / 30.0));
				})
				.AddTo(disposable);

			motor.ObserveProperty(x => x.CurrentSimState)
				.ToReadOnlyReactivePropertySlim()
				.Subscribe(x =>
				{
					switch (x)
					{
						case Motor.SimulationState.Started:
							SimulationTimer.Start();
							break;
						default:
							SimulationTimer.Stop();
							break;
					}
				})
				.AddTo(disposable);

			StartSimulation = motor
				.ObserveProperty(x => x.CurrentSimState)
				.Select(x => x == Motor.SimulationState.Paused || x == Motor.SimulationState.Stopped)
				.ToReactiveCommand()
				.WithSubscribe(motor.StartSimulation)
				.AddTo(disposable);

			PauseSimulation = motor
				.ObserveProperty(x => x.CurrentSimState)
				.Select(x => x == Motor.SimulationState.Started)
				.ToReactiveCommand()
				.WithSubscribe(motor.PauseSimulation)
				.AddTo(disposable);

			StopSimulation = motor
				.ObserveProperty(x => x.CurrentSimState)
				.Select(x => x == Motor.SimulationState.Paused || x == Motor.SimulationState.Started)
				.ToReactiveCommand()
				.WithSubscribe(motor.StopSimulation)
				.AddTo(disposable);

			DrawGlControl = new ReactiveCommand()
				.WithSubscribe(motor.DrawGlControl)
				.AddTo(disposable);

			MinVelocity
				.SetValidateNotifyError(x =>
				{
					double min;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out min, out message))
					{
						double max;

						if (Utilities.TryParse(MaxVelocity.Value, NumberRange.NonNegative, out max) && min >= max)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MaxVelocity.ForceValidate())
				.AddTo(disposable);

			MinVelocity.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MinVelocity.ForceNotify())
				.AddTo(disposable);

			MaxVelocity
				.SetValidateNotifyError(x =>
				{
					double max;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out max, out message))
					{
						double min;

						if (Utilities.TryParse(MinVelocity.Value, NumberRange.NonNegative, out min) && max <= min)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MinVelocity.ForceValidate())
				.AddTo(disposable);

			MaxVelocity.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MaxVelocity.ForceNotify())
				.AddTo(disposable);

			MinPitch
				.SetValidateNotifyError(x =>
				{
					double min;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out min, out message))
					{
						double max;

						if (Utilities.TryParse(MaxPitch.Value, NumberRange.NonNegative, out max) && min >= max)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MaxPitch.ForceValidate())
				.AddTo(disposable);

			MinPitch.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MinPitch.ForceNotify())
				.AddTo(disposable);

			MaxPitch
				.SetValidateNotifyError(x =>
				{
					double max;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out max, out message))
					{
						double min;

						if (Utilities.TryParse(MinPitch.Value, NumberRange.NonNegative, out min) && max <= min)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MinPitch.ForceValidate())
				.AddTo(disposable);

			MaxPitch.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MaxPitch.ForceNotify())
				.AddTo(disposable);

			MinVolume
				.SetValidateNotifyError(x =>
				{
					double min;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out min, out message))
					{
						double max;

						if (Utilities.TryParse(MaxVolume.Value, NumberRange.NonNegative, out max) && min >= max)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MaxVolume.ForceValidate())
				.AddTo(disposable);

			MinVolume.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MinVolume.ForceNotify())
				.AddTo(disposable);

			MaxVolume
				.SetValidateNotifyError(x =>
				{
					double max;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out max, out message))
					{
						double min;

						if (Utilities.TryParse(MinVolume.Value, NumberRange.NonNegative, out min) && max <= min)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MinVolume.ForceValidate())
				.AddTo(disposable);

			MaxVolume.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MaxVolume.ForceNotify())
				.AddTo(disposable);
		}
	}
}
