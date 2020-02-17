using System.Globalization;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.ViewModels.Trains
{
	internal abstract class CabViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> PositionX
		{
			get;
		}

		internal ReactiveProperty<string> PositionY
		{
			get;
		}

		internal ReactiveProperty<string> PositionZ
		{
			get;
		}

		internal CabViewModel(Cab cab)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			PositionX = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			PositionY = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			PositionZ = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionZ,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);
		}
	}

	internal class EmbeddedCabViewModel : CabViewModel
	{
		internal ReadOnlyReactivePropertySlim<PanelViewModel> Panel
		{
			get;
		}

		internal EmbeddedCabViewModel(EmbeddedCab cab) : base(cab)
		{
			Panel = cab
				.ObserveProperty(x => x.Panel)
				.Do(_ => Panel?.Value.Dispose())
				.Select(x => new PanelViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}

	internal class ExternalCabViewModel : CabViewModel
	{
		internal ReadOnlyReactivePropertySlim<CameraRestrictionViewModel> CameraRestriction
		{
			get;
		}

		internal ReactiveProperty<string> FileName
		{
			get;
		}

		internal ExternalCabViewModel(ExternalCab cab) : base(cab)
		{
			CameraRestriction = cab
				.ObserveProperty(x => x.CameraRestriction)
				.Do(_ => CameraRestriction?.Value.Dispose())
				.Select(x => new CameraRestrictionViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			FileName = cab
				.ToReactivePropertyAsSynchronized(x => x.FileName)
				.AddTo(disposable);
		}
	}
}
