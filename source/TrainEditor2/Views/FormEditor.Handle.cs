using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToHandle(HandleViewModel y)
		{
			CompositeDisposable handleDisposable = new CompositeDisposable();

			y.HandleType
				.BindTo(
					comboBoxHandleType,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Handle.HandleTypes)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxHandleType.SelectedIndexChanged += h,
							h => comboBoxHandleType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.PowerNotches
				.BindTo(
					numericUpDownPowerNotches,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownPowerNotches.ValueChanged += h,
							h => numericUpDownPowerNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.PowerNotches
				.BindToErrorProvider(errorProvider, numericUpDownPowerNotches)
				.AddTo(handleDisposable);

			y.PowerNotches
				.Subscribe(x =>
				{
					int index = comboBoxNotch.SelectedIndex;

					comboBoxNotch.Items.Clear();
					comboBoxNotch.Items.AddRange(Enumerable.Range(1, x).OfType<object>().ToArray());

					if (index < comboBoxNotch.Items.Count)
					{
						comboBoxNotch.SelectedIndex = index;
					}
					else
					{
						comboBoxNotch.SelectedIndex = comboBoxNotch.Items.Count - 1;
					}
				})
				.AddTo(handleDisposable);

			y.BrakeNotches
				.BindTo(
					numericUpDownBrakeNotches,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownBrakeNotches.ValueChanged += h,
							h => numericUpDownBrakeNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.BrakeNotches
				.BindToErrorProvider(errorProvider, numericUpDownBrakeNotches)
				.AddTo(handleDisposable);

			y.PowerNotchReduceSteps
				.BindTo(
					numericUpDownPowerNotchReduceSteps,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownPowerNotchReduceSteps.ValueChanged += h,
							h => numericUpDownPowerNotchReduceSteps.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.DriverPowerNotches
				.BindTo(
					numericUpDownDriverPowerNotches,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDriverPowerNotches.ValueChanged += h,
							h => numericUpDownDriverPowerNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.DriverPowerNotches
				.BindToErrorProvider(errorProvider, numericUpDownDriverPowerNotches)
				.AddTo(handleDisposable);

			y.DriverBrakeNotches
				.BindTo(
					numericUpDownDriverBrakeNotches,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDriverBrakeNotches.ValueChanged += h,
							h => numericUpDownDriverBrakeNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.DriverBrakeNotches
				.BindToErrorProvider(errorProvider, numericUpDownDriverBrakeNotches)
				.AddTo(handleDisposable);

			y.HandleBehaviour
				.BindTo(
					comboBoxEbHandleBehaviour,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Handle.EbHandleBehaviour)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxEbHandleBehaviour.SelectedIndexChanged += h,
							h => comboBoxEbHandleBehaviour.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.LocoBrake
				.BindTo(
					comboBoxLocoBrakeHandleType,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Handle.LocoBrakeType)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxLocoBrakeHandleType.SelectedIndexChanged += h,
							h => comboBoxLocoBrakeHandleType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			y.LocoBrakeNotches
				.BindTo(
					numericUpDownLocoBrakeNotches,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLocoBrakeNotches.ValueChanged += h,
							h => numericUpDownLocoBrakeNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			return handleDisposable;
		}
	}
}
