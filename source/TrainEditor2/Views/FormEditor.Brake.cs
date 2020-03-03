using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.Units;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToBrake(BrakeViewModel brake)
		{
			CompositeDisposable brakeDisposable = new CompositeDisposable();

			brake.BrakeType
				.BindTo(
					comboBoxBrakeType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Brake.BrakeTypes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeType.SelectedIndexChanged += h,
							h => comboBoxBrakeType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			brake.LocoBrakeType
				.BindTo(
					comboBoxLocoBrakeType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Brake.LocoBrakeTypes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxLocoBrakeType.SelectedIndexChanged += h,
							h => comboBoxLocoBrakeType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			brake.BrakeControlSystem
				.BindTo(
					comboBoxBrakeControlSystem,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Brake.BrakeControlSystems)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeControlSystem.SelectedIndexChanged += h,
							h => comboBoxBrakeControlSystem.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			brake.BrakeControlSpeed
				.BindTo(
					textBoxBrakeControlSpeed,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeControlSpeed.TextChanged += h,
							h => textBoxBrakeControlSpeed.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			brake.BrakeControlSpeed
				.BindToErrorProvider(errorProvider, textBoxBrakeControlSpeed)
				.AddTo(brakeDisposable);

			brake.BrakeControlSpeedUnit
				.BindTo(
					comboBoxBrakeControlSpeedUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Velocity)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeControlSpeedUnit.SelectedIndexChanged += h,
							h => comboBoxBrakeControlSpeedUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			return brakeDisposable;
		}
	}
}
