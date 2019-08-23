using System;
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
		private IDisposable BindToBrake(BrakeViewModel z)
		{
			CompositeDisposable brakeDisposable = new CompositeDisposable();

			z.BrakeType
				.BindTo(
					comboBoxBrakeType,
					w => w.SelectedIndex,
					BindingMode.TwoWay,
					w => (int)w,
					w => (Brake.BrakeTypes)w,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeType.SelectedIndexChanged += h,
							h => comboBoxBrakeType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			z.LocoBrakeType
				.BindTo(
					comboBoxLocoBrakeType,
					w => w.SelectedIndex,
					BindingMode.TwoWay,
					w => (int)w,
					w => (Brake.LocoBrakeTypes)w,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxLocoBrakeType.SelectedIndexChanged += h,
							h => comboBoxLocoBrakeType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			z.BrakeControlSystem
				.BindTo(
					comboBoxBrakeControlSystem,
					w => w.SelectedIndex,
					BindingMode.TwoWay,
					w => (int)w,
					w => (Brake.BrakeControlSystems)w,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeControlSystem.SelectedIndexChanged += h,
							h => comboBoxBrakeControlSystem.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeDisposable);

			z.BrakeControlSpeed
				.BindTo(
					textBoxBrakeControlSpeed,
					w => w.Text,
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

			z.BrakeControlSpeed
				.BindToErrorProvider(errorProvider, textBoxBrakeControlSpeed)
				.AddTo(brakeDisposable);

			return brakeDisposable;
		}
	}
}
