using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.Units;
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

		internal ReactiveProperty<Unit.Length> PositionX_Unit
		{
			get;
		}

		internal ReactiveProperty<string> PositionY
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> PositionY_Unit
		{
			get;
		}

		internal ReactiveProperty<string> PositionZ
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> PositionZ_Unit
		{
			get;
		}

		internal CabViewModel(Cab cab)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			PositionX = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionX,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), cab.PositionX.UnitValue),
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

			PositionX_Unit = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionX,
					x => x.UnitValue,
					x => cab.PositionX.ToNewUnit(x)
				)
				.AddTo(disposable);

			PositionY = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionY,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), cab.PositionY.UnitValue),
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

			PositionY_Unit = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionY,
					x => x.UnitValue,
					x => cab.PositionY.ToNewUnit(x)
				)
				.AddTo(disposable);

			PositionZ = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionZ,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), cab.PositionZ.UnitValue),
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

			PositionZ_Unit = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionZ,
					x => x.UnitValue,
					x => cab.PositionZ.ToNewUnit(x)
				)
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
