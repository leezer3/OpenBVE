﻿using System;
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
		private IDisposable BindToAcceleration(AccelerationViewModel acceleration)
		{
			CompositeDisposable accelerationDisposable = new CompositeDisposable();
			CompositeDisposable entryDisposable = new CompositeDisposable().AddTo(accelerationDisposable);

			acceleration.SelectedEntryIndex
				.BindTo(
					comboBoxNotch,
					x => x.SelectedIndex,
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

			acceleration.SelectedEntryIndex.Value = comboBoxNotch.SelectedIndex;

			acceleration.SelectedEntry
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

			acceleration.MinVelocity
				.BindTo(
					textBoxAccelXmin,
					x => x.Text,
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

			acceleration.MinVelocity
				.BindToErrorProvider(errorProvider, textBoxAccelXmin)
				.AddTo(accelerationDisposable);

			acceleration.MaxVelocity
				.BindTo(
					textBoxAccelXmax,
					x => x.Text,
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

			acceleration.MaxVelocity
				.BindToErrorProvider(errorProvider, textBoxAccelXmax)
				.AddTo(accelerationDisposable);

			acceleration.MinAcceleration
				.BindTo(
					textBoxAccelYmin,
					x => x.Text,
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

			acceleration.MinAcceleration
				.BindToErrorProvider(errorProvider, textBoxAccelYmin)
				.AddTo(accelerationDisposable);

			acceleration.MaxAcceleration
				.BindTo(
					textBoxAccelYmax,
					x => x.Text,
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

			acceleration.MaxAcceleration
				.BindToErrorProvider(errorProvider, textBoxAccelYmax)
				.AddTo(accelerationDisposable);

			acceleration.NowVelocity
				.BindTo(
					labelAccelXValue,
					x => x.Text
				)
				.AddTo(accelerationDisposable);

			acceleration.NowAcceleration
				.BindTo(
					labelAccelYValue,
					x => x.Text
				)
				.AddTo(accelerationDisposable);

			acceleration.Resistance
				.BindTo(
					checkBoxSubtractDeceleration,
					x => x.Checked,
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

			acceleration.ImageWidth
				.BindTo(
					pictureBoxAccel,
					x => x.Width,
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

			acceleration.ImageWidth.Value = pictureBoxAccel.Width;

			acceleration.ImageHeight
				.BindTo(
					pictureBoxAccel,
					x => x.Height,
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

			acceleration.ImageHeight.Value = pictureBoxAccel.Height;

			acceleration.Image
				.Subscribe(x =>
				{
					pictureBoxAccel.Image = x;
					pictureBoxAccel.Refresh();
				})
				.AddTo(accelerationDisposable);

			acceleration.ZoomIn.BindToButton(buttonAccelZoomIn).AddTo(accelerationDisposable);
			acceleration.ZoomOut.BindToButton(buttonAccelZoomOut).AddTo(accelerationDisposable);
			acceleration.Reset.BindToButton(buttonAccelReset).AddTo(accelerationDisposable);

			return accelerationDisposable;
		}
	}
}
