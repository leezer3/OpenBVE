using Reactive.Bindings.Binding;
using Reactive.Bindings.Disposables;
using System;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToParticleSource(ParticleSourceViewModel p)
		{
			CompositeDisposable particleDisposable = new CompositeDisposable();
			p.LocationX
				.BindTo(
					textBoxParticleLocationX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleLocationX.TextChanged += h,
							h => textBoxParticleLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.LocationX
				.BindToErrorProvider(errorProvider, textBoxParticleLocationX)
				.AddTo(particleDisposable);

			p.LocationY
				.BindTo(
					textBoxParticleLocationY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleLocationY.TextChanged += h,
							h => textBoxParticleLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.LocationY
				.BindToErrorProvider(errorProvider, textBoxParticleLocationY)
				.AddTo(particleDisposable);

			p.LocationZ
				.BindTo(
					textBoxParticleLocationZ,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleLocationZ.TextChanged += h,
							h => textBoxParticleLocationZ.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.LocationZ
				.BindToErrorProvider(errorProvider, textBoxParticleLocationZ)
				.AddTo(particleDisposable);

			p.InitialSize
				.BindTo(
					textBoxParticleInitialSize,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleInitialSize.TextChanged += h,
							h => textBoxParticleInitialSize.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.InitialSize
				.BindToErrorProvider(errorProvider, textBoxParticleInitialSize)
				.AddTo(particleDisposable);

			p.MaximumSize
				.BindTo(
					textBoxParticleMaxSize,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleMaxSize.TextChanged += h,
							h => textBoxParticleMaxSize.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.MaximumSize
				.BindToErrorProvider(errorProvider, textBoxParticleMaxSize)
				.AddTo(particleDisposable);

			p.InitialDirectionX
				.BindTo(
					textBoxParticleInitialDirectionX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleInitialDirectionX.TextChanged += h,
							h => textBoxParticleInitialDirectionX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.InitialDirectionX
				.BindToErrorProvider(errorProvider, textBoxParticleInitialDirectionX)
				.AddTo(particleDisposable);

			p.InitialDirectionY
				.BindTo(
					textBoxParticleInitialDirectionY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleInitialDirectionY.TextChanged += h,
							h => textBoxParticleInitialDirectionY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.InitialDirectionY
				.BindToErrorProvider(errorProvider, textBoxParticleInitialDirectionY)
				.AddTo(particleDisposable);

			p.InitialDirectionZ
				.BindTo(
					textBoxParticleInitialDirectionZ,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleInitialDirectionZ.TextChanged += h,
							h => textBoxParticleInitialDirectionZ.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			p.InitialDirectionZ
				.BindToErrorProvider(errorProvider, textBoxParticleInitialDirectionZ)
				.AddTo(particleDisposable);

			p.TextureFile
				.BindTo(
					textBoxParticleTexture,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxParticleTexture.TextChanged += h,
							h => textBoxParticleTexture.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(particleDisposable);

			return particleDisposable;
		}
	}
}
