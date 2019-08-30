using System;
using System.Drawing;
using System.Globalization;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class AccelerationViewModel : BaseViewModel
	{
		internal class EntryViewModel : BaseViewModel
		{
			internal ReactiveProperty<string> A0
			{
				get;
			}

			internal ReactiveProperty<string> A1
			{
				get;
			}

			internal ReactiveProperty<string> V1
			{
				get;
			}

			internal ReactiveProperty<string> V2
			{
				get;
			}

			internal ReactiveProperty<string> E
			{
				get;
			}

			internal EntryViewModel(Acceleration.Entry entry)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				A0 = entry
					.ToReactivePropertyAsSynchronized(
						x => x.A0,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.Positive, out result, out message);

						return message;
					})
					.AddTo(disposable);

				A1 = entry
					.ToReactivePropertyAsSynchronized(
						x => x.A1,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.Positive, out result, out message);

						return message;
					})
					.AddTo(disposable);

				V1 = entry
					.ToReactivePropertyAsSynchronized(
						x => x.V1,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.Positive, out result, out message);

						return message;
					})
					.AddTo(disposable);

				V2 = entry
					.ToReactivePropertyAsSynchronized(
						x => x.V2,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.Positive, out result, out message);

						return message;
					})
					.AddTo(disposable);

				E = entry
					.ToReactivePropertyAsSynchronized(
						x => x.E,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.Positive, out result, out message);

						return message;
					})
					.AddTo(disposable);
			}
		}

		internal ReadOnlyReactiveCollection<EntryViewModel> Entries
		{
			get;
		}

		internal ReactiveProperty<int> SelectedEntryIndex
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<EntryViewModel> SelectedEntry
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

		internal ReactiveProperty<string> MinAcceleration
		{
			get;
		}

		internal ReactiveProperty<string> MaxAcceleration
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> NowVelocity
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> NowAcceleration
		{
			get;
		}

		internal ReactiveProperty<bool> Resistance
		{
			get;
		}

		internal ReactiveProperty<int> ImageWidth
		{
			get;
		}

		internal ReactiveProperty<int> ImageHeight
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<Bitmap> Image
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

		internal ReactiveCommand<InputEventModel.EventArgs> MouseMove
		{
			get;
		}

		internal AccelerationViewModel(Acceleration acceleration, Train train, MotorCar car)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Entries = acceleration.Entries
				.ToReadOnlyReactiveCollection(x => new EntryViewModel(x))
				.AddTo(disposable);

			SelectedEntryIndex = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedEntryIndex,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => x < 0 ? string.Empty : null)
				.AddTo(disposable);

			SelectedEntry = SelectedEntryIndex
				.Select(x => x < 0 ? null : Entries[x])
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			MinVelocity = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.MinVelocity,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxVelocity = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.MaxVelocity,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinAcceleration = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.MinAcceleration,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxAcceleration = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.MaxAcceleration,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			NowVelocity = acceleration
				.ObserveProperty(x => x.NowVelocity)
				.Select(x => $"{x} km/h")
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			NowAcceleration = acceleration
				.ObserveProperty(x => x.NowAcceleration)
				.Select(x => $"{x} km/h/s")
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Resistance = acceleration
				.ToReactivePropertyAsSynchronized(x => x.Resistance)
				.AddTo(disposable);

			ImageWidth = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.ImageWidth,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => x <= 0 ? string.Empty : null)
				.AddTo(disposable);

			ImageHeight = acceleration
				.ToReactivePropertyAsSynchronized(
					x => x.ImageHeight,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => x <= 0 ? string.Empty : null)
				.AddTo(disposable);

			Image = acceleration
				.ObserveProperty(x => x.Image)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			new[]
				{
					acceleration
						.PropertyChangedAsObservable()
						.Where(x => x.PropertyName != nameof(acceleration.NowVelocity)
									&& x.PropertyName != nameof(acceleration.NowAcceleration)
									&& x.PropertyName != nameof(acceleration.Image)
						)
						.OfType<object>(),
					acceleration.Entries.ObserveElementPropertyChanged().OfType<object>()
				}
				.Merge()
				.ToReadOnlyReactivePropertySlim()
				.Subscribe(_ => train.DrawAccelerationImage(car))
				.AddTo(disposable);

			ZoomIn = new ReactiveCommand();
			ZoomIn.Subscribe(acceleration.ZoomIn).AddTo(disposable);

			ZoomOut = new ReactiveCommand();
			ZoomOut.Subscribe(acceleration.ZoomOut).AddTo(disposable);

			Reset = new ReactiveCommand();
			Reset.Subscribe(acceleration.Reset).AddTo(disposable);

			MoveLeft = new ReactiveCommand();
			MoveLeft.Subscribe(acceleration.MoveLeft).AddTo(disposable);

			MoveRight = new ReactiveCommand();
			MoveRight.Subscribe(acceleration.MoveRight).AddTo(disposable);

			MoveBottom = new ReactiveCommand();
			MoveBottom.Subscribe(acceleration.MoveBottom).AddTo(disposable);

			MoveTop = new ReactiveCommand();
			MoveTop.Subscribe(acceleration.MoveTop).AddTo(disposable);

			MouseMove = new ReactiveCommand<InputEventModel.EventArgs>();
			MouseMove.Subscribe(acceleration.MouseMove).AddTo(disposable);

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

			MinAcceleration
				.SetValidateNotifyError(x =>
				{
					double min;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out min, out message))
					{
						double max;

						if (Utilities.TryParse(MaxAcceleration.Value, NumberRange.NonNegative, out max) && min >= max)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MaxAcceleration.ForceValidate())
				.AddTo(disposable);

			MinAcceleration.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MinAcceleration.ForceNotify())
				.AddTo(disposable);

			MaxAcceleration
				.SetValidateNotifyError(x =>
				{
					double max;
					string message;

					if (Utilities.TryParse(x, NumberRange.NonNegative, out max, out message))
					{
						double min;

						if (Utilities.TryParse(MinAcceleration.Value, NumberRange.NonNegative, out min) && max <= min)
						{
							message = "MinはMax未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => MinAcceleration.ForceValidate())
				.AddTo(disposable);

			MaxAcceleration.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => MaxAcceleration.ForceNotify())
				.AddTo(disposable);
		}
	}
}
