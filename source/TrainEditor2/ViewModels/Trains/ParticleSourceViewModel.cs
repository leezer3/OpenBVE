
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Globalization;
using TrainEditor2.Extensions;
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

		internal ParticleSource Model
		{
			get;
		}

		internal CarViewModel ParentCar;

		internal ParticleSourceViewModel(ParticleSource particleSource, CarViewModel parentCar)
		{

			Model = particleSource;

			CultureInfo culture = CultureInfo.InvariantCulture;

			LocationX = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationX,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationY = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationY,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LocationZ = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.LocationZ,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			InitialSize = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.InitialSize,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaximumSize = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.MaximiumSize,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			InitialDirectionX = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.InitialDirectionX,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			InitialDirectionY = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.InitialDirectionY,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			InitialDirectionZ = particleSource
				.ToReactivePropertyAsSynchronized(
					x => x.InitialDirectionZ,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			TextureFile = particleSource
				.ToReactivePropertyAsSynchronized(x => x.TextureFile)
				.AddTo(disposable);
			ParentCar = parentCar;
		}
	}
}
