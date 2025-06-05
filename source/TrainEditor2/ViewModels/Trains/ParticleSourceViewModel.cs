
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Globalization;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class ParticleSourceViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> LocationX
		{
			get;
		}

		internal ReactiveProperty<string> LocationY
		{
			get;
		}

		internal ReactiveProperty<string> LocationZ
		{
			get;
		}

		internal ReactiveProperty<string> InitialSize
		{
			get;
		}

		internal ReactiveProperty<string> MaximumSize
		{
			get;
		}

		internal ReactiveProperty<string> InitialDirectionX
		{
			get;
		}

		internal ReactiveProperty<string> InitialDirectionY
		{
			get;
		}

		internal ReactiveProperty<string> InitialDirectionZ
		{
			get;
		}

		internal ReactiveProperty<string> TextureFile
		{
			get;
		}

		internal ParticleSourceViewModel(ParticleSource particleSource)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			LocationX = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationY = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationZ = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationZ,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			InitialSize = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.InitialSize,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaximumSize = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.MaximiumSize,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationX = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationY = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationZ = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationZ,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			TextureFile = particleSource
				.ToReactivePropertyAsSynchronized(x => x.TextureFile)
				.AddTo(disposable);
		}
	}
}
