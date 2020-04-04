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
		private IDisposable BindToHandle(HandleViewModel handle)
		{
			CompositeDisposable handleDisposable = new CompositeDisposable();

			handle.HandleType
				.BindTo(
					comboBoxHandleType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Handle.HandleTypes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxHandleType.SelectedIndexChanged += h,
							h => comboBoxHandleType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.PowerNotches
				.BindTo(
					numericUpDownPowerNotches,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownPowerNotches.ValueChanged += h,
							h => numericUpDownPowerNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.PowerNotches
				.BindToErrorProvider(errorProvider, numericUpDownPowerNotches)
				.AddTo(handleDisposable);

			handle.PowerNotches
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

			handle.BrakeNotches
				.BindTo(
					numericUpDownBrakeNotches,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownBrakeNotches.ValueChanged += h,
							h => numericUpDownBrakeNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.BrakeNotches
				.BindToErrorProvider(errorProvider, numericUpDownBrakeNotches)
				.AddTo(handleDisposable);

			handle.PowerNotchReduceSteps
				.BindTo(
					numericUpDownPowerNotchReduceSteps,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownPowerNotchReduceSteps.ValueChanged += h,
							h => numericUpDownPowerNotchReduceSteps.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.DriverPowerNotches
				.BindTo(
					numericUpDownDriverPowerNotches,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDriverPowerNotches.ValueChanged += h,
							h => numericUpDownDriverPowerNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.DriverPowerNotches
				.BindToErrorProvider(errorProvider, numericUpDownDriverPowerNotches)
				.AddTo(handleDisposable);

			handle.DriverBrakeNotches
				.BindTo(
					numericUpDownDriverBrakeNotches,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDriverBrakeNotches.ValueChanged += h,
							h => numericUpDownDriverBrakeNotches.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.DriverBrakeNotches
				.BindToErrorProvider(errorProvider, numericUpDownDriverBrakeNotches)
				.AddTo(handleDisposable);

			handle.HandleBehaviour
				.BindTo(
					comboBoxEbHandleBehaviour,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Handle.EbHandleBehaviour)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxEbHandleBehaviour.SelectedIndexChanged += h,
							h => comboBoxEbHandleBehaviour.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.LocoBrake
				.BindTo(
					comboBoxLocoBrakeHandleType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Handle.LocoBrakeType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxLocoBrakeHandleType.SelectedIndexChanged += h,
							h => comboBoxLocoBrakeHandleType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(handleDisposable);

			handle.LocoBrakeNotches
				.BindTo(
					numericUpDownLocoBrakeNotches,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
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
