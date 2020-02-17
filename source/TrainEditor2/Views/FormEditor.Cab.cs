using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCab(CabViewModel y)
		{
			CompositeDisposable cabDisposable = new CompositeDisposable();
			CompositeDisposable panelDisposable = new CompositeDisposable().AddTo(cabDisposable);
			CompositeDisposable cameraRestrictionDisposable = new CompositeDisposable().AddTo(cabDisposable);

			y.PositionX
				.BindTo(
					textBoxCabX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabX.TextChanged += h,
							h => textBoxCabX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			y.PositionX
				.BindToErrorProvider(errorProvider, textBoxCabX)
				.AddTo(cabDisposable);

			y.PositionY
				.BindTo(
					textBoxCabY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabY.TextChanged += h,
							h => textBoxCabY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			y.PositionY
				.BindToErrorProvider(errorProvider, textBoxCabY)
				.AddTo(cabDisposable);

			y.PositionZ
				.BindTo(
					textBoxCabZ,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabZ.TextChanged += h,
							h => textBoxCabZ.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			y.PositionZ
				.BindToErrorProvider(errorProvider, textBoxCabZ)
				.AddTo(cabDisposable);

			EmbeddedCabViewModel embeddedCab = y as EmbeddedCabViewModel;
			ExternalCabViewModel externalCab = y as ExternalCabViewModel;

			embeddedCab?.Panel
				.Subscribe(z =>
				{
					panelDisposable.Dispose();
					panelDisposable = new CompositeDisposable().AddTo(cabDisposable);

					BindToPanel(z).AddTo(panelDisposable);
				})
				.AddTo(cabDisposable);

			externalCab?.FileName
				.BindTo(
					textBoxCabFileName,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabFileName.TextChanged += h,
							h => textBoxCabFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			externalCab?.CameraRestriction
				.Subscribe(z =>
				{
					cameraRestrictionDisposable.Dispose();
					cameraRestrictionDisposable = new CompositeDisposable().AddTo(cabDisposable);

					BindToCameraRestriction(z).AddTo(cameraRestrictionDisposable);
				})
				.AddTo(cabDisposable);

			return cabDisposable;
		}
	}
}
