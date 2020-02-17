using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
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
		internal class TrackViewModel : BaseViewModel
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

			internal ReadOnlyReactivePropertySlim<bool> StoppedSim
			{
				get;
			}

			internal ReadOnlyReactivePropertySlim<Motor.ToolMode> CurrentToolMode
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

			internal ReactiveProperty<Motor.TrackType> Type
			{
				get;
			}

			internal ReactiveCommand<Motor.ToolMode> ChangeToolMode
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

			internal TrackViewModel(Motor.Track track)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				MessageBox = track
					.ObserveProperty(x => x.MessageBox)
					.Do(_ => MessageBox?.Value.Dispose())
					.Select(x => new MessageBoxViewModel(x))
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				ToolTipVertexPitch = track
					.ObserveProperty(x => x.ToolTipVertexPitch)
					.Do(_ => ToolTipVertexPitch?.Value.Dispose())
					.Select(x => new ToolTipViewModel(x))
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				ToolTipVertexVolume = track
					.ObserveProperty(x => x.ToolTipVertexVolume)
					.Do(_ => ToolTipVertexVolume?.Value.Dispose())
					.Select(x => new ToolTipViewModel(x))
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				CurrentModifierKeys = track
					.ToReactivePropertyAsSynchronized(x => x.CurrentModifierKeys)
					.AddTo(disposable);

				CurrentCursorType = track
					.ObserveProperty(x => x.CurrentCursorType)
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				StoppedSim = track.BaseMotor
					.ObserveProperty(x => x.CurrentSimState)
					.Select(x => x == Motor.SimulationState.Disable || x == Motor.SimulationState.Stopped)
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				ReadOnlyReactivePropertySlim<Motor.InputMode> CurrentInputMode = track.BaseMotor
					.ObserveProperty(x => x.CurrentInputMode)
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				CurrentInputMode
					.Subscribe(_ =>
					{
						track.ResetSelect();
						track.BaseMotor.IsRefreshGlControl = true;
					})
					.AddTo(disposable);

				CurrentToolMode = track
					.ObserveProperty(x => x.CurrentToolMode)
					.ToReadOnlyReactivePropertySlim()
					.AddTo(disposable);

				CurrentToolMode
					.Subscribe(_ =>
					{
						track.ResetSelect();
						track.BaseMotor.IsRefreshGlControl = true;
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

				Type = track
					.ToReactivePropertyAsSynchronized(x => x.Type)
					.AddTo(disposable);

				track.ObserveProperty(x => x.Type)
					.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
					.Subscribe(_ => track.BaseMotor.ApplyTrackType())
					.AddTo(disposable);

				ChangeToolMode = new[]
					{
						CurrentInputMode.Select(x => x != Motor.InputMode.SoundIndex),
						StoppedSim
					}
					.CombineLatestValuesAreAllTrue()
					.ToReactiveCommand<Motor.ToolMode>()
					.WithSubscribe(x => track.CurrentToolMode = x)
					.AddTo(disposable);

				Undo = new[]
					{
						track.PrevStates.CollectionChangedAsObservable().Select(_ => track.PrevStates.Any()),
						StoppedSim
					}
					.CombineLatestValuesAreAllTrue()
					.ToReactiveCommand(false)
					.WithSubscribe(track.Undo)
					.AddTo(disposable);

				Redo = new[]
					{
						track.NextStates.CollectionChangedAsObservable().Select(_ => track.NextStates.Any()),
						StoppedSim
					}
					.CombineLatestValuesAreAllTrue()
					.ToReactiveCommand(false)
					.WithSubscribe(track.Redo)
					.AddTo(disposable);

				Cleanup = StoppedSim
					.ToReactiveCommand()
					.WithSubscribe(track.Cleanup)
					.AddTo(disposable);

				Delete = StoppedSim
					.ToReactiveCommand()
					.WithSubscribe(track.Delete)
					.AddTo(disposable);

				MouseDown = new ReactiveCommand<InputEventModel.EventArgs>().WithSubscribe(track.MouseDown).AddTo(disposable);

				MouseUp = new ReactiveCommand().WithSubscribe(track.MouseUp).AddTo(disposable);

				DirectDot = new[]
					{
						StoppedSim,
						CurrentToolMode.Select(x => x == Motor.ToolMode.Dot),
						DirectX.ObserveHasErrors.Select(x => !x),
						DirectY.ObserveHasErrors.Select(x => !x)
					}
					.CombineLatestValuesAreAllTrue()
					.ToReactiveCommand()
					.WithSubscribe(() => track.DirectDot(double.Parse(DirectX.Value), double.Parse(DirectY.Value)))
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
					.WithSubscribe(() => track.DirectMove(double.Parse(DirectX.Value), double.Parse(DirectY.Value)))
					.AddTo(disposable);
			}
		}

		internal ReadOnlyReactiveCollection<TreeViewItemViewModel> TreeItems
		{
			get;
		}

		internal ReactiveProperty<TreeViewItemViewModel> SelectedTreeItem
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TrackViewModel> SelectedTrack
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<bool> StoppedSim
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

		internal ReadOnlyReactivePropertySlim<Motor.InputMode> CurrentInputMode
		{
			get;
		}

		internal ReactiveProperty<int> SelectedSoundIndex
		{
			get;
		}

		internal ReactiveProperty<bool> IsRefreshGlControl
		{
			get;
		}

		internal ReactiveProperty<int> RunIndex
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

		internal ReactiveCommand UpTrack
		{
			get;
		}

		internal ReactiveCommand DownTrack
		{
			get;
		}

		internal ReactiveCommand AddTrack
		{
			get;
		}

		internal ReactiveCommand RemoveTrack
		{
			get;
		}

		internal ReactiveCommand CopyTrack
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

		internal ReactiveCommand<Motor.InputMode> ChangeInputMode
		{
			get;
		}

		internal ReactiveCommand<InputEventModel.EventArgs> MouseMove
		{
			get;
		}

		internal ReactiveCommand DrawGlControl
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

		internal MotorViewModel(Motor motor)
		{
			CompositeDisposable treeItemDisposable = new CompositeDisposable().AddTo(disposable);

			CultureInfo culture = CultureInfo.InvariantCulture;

			TreeItems = motor.TreeItems.ToReadOnlyReactiveCollection(x => new TreeViewItemViewModel(x, null)).AddTo(disposable);

			SelectedTreeItem = motor
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItems.Select(y => y.SearchViewModel(x)).FirstOrDefault(y => y != null),
					x => x?.Model
				)
				.AddTo(disposable);

			SelectedTreeItem
				.Subscribe(x =>
				{
					treeItemDisposable.Dispose();
					treeItemDisposable = new CompositeDisposable();

					motor.IsRefreshGlControl = true;

					x?.Checked.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(treeItemDisposable);
				})
				.AddTo(disposable);

			SelectedTrack = SelectedTreeItem
				.Select(x => x?.Tag.Value as Motor.Track)
				.Do(_ => SelectedTrack?.Value?.Dispose())
				.Select(x => x != null ? new TrackViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			StoppedSim = motor
				.ObserveProperty(x => x.CurrentSimState)
				.Select(x => x == Motor.SimulationState.Disable || x == Motor.SimulationState.Stopped)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			MinVelocity = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MinVelocity,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinVelocity.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MaxVelocity = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MaxVelocity,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxVelocity.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MinPitch = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MinPitch,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinPitch.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MaxPitch = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MaxPitch,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxPitch.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MinVolume = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MinVolume,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinVolume.Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			MaxVolume = motor
				.ToReactivePropertyAsSynchronized(
					x => x.MaxVolume,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			CurrentInputMode = motor
				.ObserveProperty(x => x.CurrentInputMode)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentInputMode.Where(_ => SelectedTrack.Value == null).Subscribe(_ => motor.IsRefreshGlControl = true).AddTo(disposable);

			SelectedSoundIndex = motor
				.ToReactivePropertyAsSynchronized(x => x.SelectedSoundIndex)
				.AddTo(disposable);

			IsRefreshGlControl = motor
				.ToReactivePropertyAsSynchronized(x => x.IsRefreshGlControl)
				.AddTo(disposable);

			RunIndex = motor
				.ToReactivePropertyAsSynchronized(x => x.RunIndex)
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
					x => double.Parse(x, NumberStyles.Float, culture),
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
					x => double.Parse(x, NumberStyles.Float, culture),
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
					x => double.Parse(x, NumberStyles.Float, culture),
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

			UpTrack = new[]
				{
					SelectedTreeItem.Select(x => TreeItems[0].Children.SelectMany(y => y.Children).Contains(x) && x.Parent.Children.IndexOf(x) > 0),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(motor.UpTrack)
				.AddTo(disposable);

			DownTrack = new[]
				{
					SelectedTreeItem.Select(x => TreeItems[0].Children.SelectMany(y => y.Children).Contains(x) && x.Parent.Children.IndexOf(x) >= 0 && x.Parent.Children.IndexOf(x) < x.Parent.Children.Count - 1),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(motor.DownTrack)
				.AddTo(disposable);

			AddTrack = new[]
				{
					SelectedTreeItem.Select(x => TreeItems[0].Children.Contains(x) || TreeItems[0].Children.SelectMany(y=>y.Children).Contains(x)),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(motor.AddTrack)
				.AddTo(disposable);

			RemoveTrack = new[]
				{
					SelectedTreeItem.Select(x => TreeItems[0].Children.SelectMany(y=>y.Children).Contains(x)),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(motor.RemoveTrack)
				.AddTo(disposable);

			CopyTrack = new[]
				{
					SelectedTreeItem.Select(x => TreeItems[0].Children.SelectMany(y=>y.Children).Contains(x)),
					StoppedSim
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(motor.CopyTrack)
				.AddTo(disposable);

			ZoomIn = new ReactiveCommand().WithSubscribe(motor.ZoomIn).AddTo(disposable);

			ZoomOut = new ReactiveCommand().WithSubscribe(motor.ZoomOut).AddTo(disposable);

			Reset = new ReactiveCommand().WithSubscribe(motor.Reset).AddTo(disposable);

			MoveLeft = new ReactiveCommand().WithSubscribe(motor.MoveLeft).AddTo(disposable);

			MoveRight = new ReactiveCommand().WithSubscribe(motor.MoveRight).AddTo(disposable);

			MoveBottom = new ReactiveCommand().WithSubscribe(motor.MoveBottom).AddTo(disposable);

			MoveTop = new ReactiveCommand().WithSubscribe(motor.MoveTop).AddTo(disposable);

			ChangeInputMode = new ReactiveCommand<Motor.InputMode>().WithSubscribe(x => motor.CurrentInputMode = x).AddTo(disposable);

			MouseMove = new ReactiveCommand<InputEventModel.EventArgs>().WithSubscribe(motor.MouseMove).AddTo(disposable);

			DrawGlControl = new ReactiveCommand()
				.WithSubscribe(motor.DrawGlControl)
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
