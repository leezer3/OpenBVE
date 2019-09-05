using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToPilotLamp(PilotLampElementViewModel y)
		{
			CompositeDisposable pilotLampDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxPilotLampLocationX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPilotLampLocationX.TextChanged += h,
							h => textBoxPilotLampLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxPilotLampLocationX)
				.AddTo(pilotLampDisposable);

			y.LocationY
				.BindTo(
					textBoxPilotLampLocationY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPilotLampLocationY.TextChanged += h,
							h => textBoxPilotLampLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxPilotLampLocationY)
				.AddTo(pilotLampDisposable);

			y.Layer
				.BindTo(
					numericUpDownPilotLampLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownPilotLampLayer.ValueChanged += h,
							h => numericUpDownPilotLampLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			y.DaytimeImage
				.BindTo(
					textBoxPilotLampDaytimeImage,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPilotLampDaytimeImage.TextChanged += h,
							h => textBoxPilotLampDaytimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			y.NighttimeImage
				.BindTo(
					textBoxPilotLampNighttimeImage,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPilotLampNighttimeImage.TextChanged += h,
							h => textBoxPilotLampNighttimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			y.TransparentColor
				.BindTo(
					textBoxPilotLampTransparentColor,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPilotLampTransparentColor.TextChanged += h,
							h => textBoxPilotLampTransparentColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			y.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxPilotLampTransparentColor)
				.AddTo(pilotLampDisposable);

			return pilotLampDisposable;
		}
	}
}
