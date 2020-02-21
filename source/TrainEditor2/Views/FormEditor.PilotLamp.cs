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
		private IDisposable BindToPilotLamp(PilotLampElementViewModel pilotLamp)
		{
			CompositeDisposable pilotLampDisposable = new CompositeDisposable();

			pilotLamp.LocationX
				.BindTo(
					textBoxPilotLampLocationX,
					x => x.Text,
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

			pilotLamp.LocationX
				.BindToErrorProvider(errorProvider, textBoxPilotLampLocationX)
				.AddTo(pilotLampDisposable);

			pilotLamp.LocationY
				.BindTo(
					textBoxPilotLampLocationY,
					x => x.Text,
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

			pilotLamp.LocationY
				.BindToErrorProvider(errorProvider, textBoxPilotLampLocationY)
				.AddTo(pilotLampDisposable);

			pilotLamp.Layer
				.BindTo(
					numericUpDownPilotLampLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownPilotLampLayer.ValueChanged += h,
							h => numericUpDownPilotLampLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pilotLampDisposable);

			pilotLamp.DaytimeImage
				.BindTo(
					textBoxPilotLampDaytimeImage,
					x => x.Text,
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

			pilotLamp.NighttimeImage
				.BindTo(
					textBoxPilotLampNighttimeImage,
					x => x.Text,
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

			pilotLamp.TransparentColor
				.BindTo(
					textBoxPilotLampTransparentColor,
					x => x.Text,
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

			pilotLamp.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxPilotLampTransparentColor)
				.AddTo(pilotLampDisposable);

			return pilotLampDisposable;
		}
	}
}
