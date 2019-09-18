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
		private IDisposable BindToAcceleration(AccelerationViewModel z)
		{
			CompositeDisposable accelerationDisposable = new CompositeDisposable();
			CompositeDisposable entryDisposable = new CompositeDisposable().AddTo(accelerationDisposable);

			z.SelectedEntryIndex
				.BindTo(
					comboBoxNotch,
					w => w.SelectedIndex,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxNotch.SelectedIndexChanged += h,
							h => comboBoxNotch.SelectedIndexChanged -= h
						)
						.ToUnit(),
					-1
				)
				.AddTo(accelerationDisposable);

			z.SelectedEntryIndex.Value = comboBoxNotch.SelectedIndex;

			z.SelectedEntry
				.Where(x => x != null)
				.Subscribe(x =>
				{
					entryDisposable.Dispose();
					entryDisposable = new CompositeDisposable().AddTo(accelerationDisposable);

					x.A0.BindTo(
							textBoxAccelA0,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxAccelA0.TextChanged += h,
									h => textBoxAccelA0.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.A0.BindToErrorProvider(errorProvider, textBoxAccelA0)
						.AddTo(entryDisposable);

					x.A1.BindTo(
							textBoxAccelA1,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxAccelA1.TextChanged += h,
									h => textBoxAccelA1.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.A1.BindToErrorProvider(errorProvider, textBoxAccelA1)
						.AddTo(entryDisposable);

					x.V1.BindTo(
							textBoxAccelV1,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxAccelV1.TextChanged += h,
									h => textBoxAccelV1.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.V1.BindToErrorProvider(errorProvider, textBoxAccelV1)
						.AddTo(entryDisposable);

					x.V2.BindTo(
							textBoxAccelV2,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxAccelV2.TextChanged += h,
									h => textBoxAccelV2.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.V2.BindToErrorProvider(errorProvider, textBoxAccelV2)
						.AddTo(entryDisposable);

					x.E.BindTo(
							textBoxAccelE,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxAccelE.TextChanged += h,
									h => textBoxAccelE.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.E.BindToErrorProvider(errorProvider, textBoxAccelE)
						.AddTo(entryDisposable);
				})
				.AddTo(accelerationDisposable);

			z.MinVelocity
				.BindTo(
					textBoxAccelXmin,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxAccelXmin.TextChanged += h,
							h => textBoxAccelXmin.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.MinVelocity
				.BindToErrorProvider(errorProvider, textBoxAccelXmin)
				.AddTo(accelerationDisposable);

			z.MaxVelocity
				.BindTo(
					textBoxAccelXmax,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxAccelXmax.TextChanged += h,
							h => textBoxAccelXmax.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.MaxVelocity
				.BindToErrorProvider(errorProvider, textBoxAccelXmax)
				.AddTo(accelerationDisposable);

			z.MinAcceleration
				.BindTo(
					textBoxAccelYmin,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxAccelYmin.TextChanged += h,
							h => textBoxAccelYmin.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.MinAcceleration
				.BindToErrorProvider(errorProvider, textBoxAccelYmin)
				.AddTo(accelerationDisposable);

			z.MaxAcceleration
				.BindTo(
					textBoxAccelYmax,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxAccelYmax.TextChanged += h,
							h => textBoxAccelYmax.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.MaxAcceleration
				.BindToErrorProvider(errorProvider, textBoxAccelYmax)
				.AddTo(accelerationDisposable);

			z.NowVelocity
				.BindTo(
					labelAccelXValue,
					w => w.Text
				)
				.AddTo(accelerationDisposable);

			z.NowAcceleration
				.BindTo(
					labelAccelYValue,
					w => w.Text
				)
				.AddTo(accelerationDisposable);

			z.Resistance
				.BindTo(
					checkBoxSubtractDeceleration,
					w => w.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxSubtractDeceleration.CheckedChanged += h,
							h => checkBoxSubtractDeceleration.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.ImageWidth
				.BindTo(
					pictureBoxAccel,
					w => w.Width,
					BindingMode.OneWayToSource,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => Resize += h,
							h => Resize -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.ImageWidth.Value = pictureBoxAccel.Width;

			z.ImageHeight
				.BindTo(
					pictureBoxAccel,
					w => w.Height,
					BindingMode.OneWayToSource,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => Resize += h,
							h => Resize -= h
						)
						.ToUnit()
				)
				.AddTo(accelerationDisposable);

			z.ImageHeight.Value = pictureBoxAccel.Height;

			z.Image
				.Subscribe(x =>
				{
					pictureBoxAccel.Image = x;
					pictureBoxAccel.Refresh();
				})
				.AddTo(accelerationDisposable);

			z.ZoomIn.BindToButton(buttonAccelZoomIn).AddTo(accelerationDisposable);
			z.ZoomOut.BindToButton(buttonAccelZoomOut).AddTo(accelerationDisposable);
			z.Reset.BindToButton(buttonAccelReset).AddTo(accelerationDisposable);

			return accelerationDisposable;
		}
	}
}
