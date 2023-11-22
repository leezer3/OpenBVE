using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.World;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToPerformance(PerformanceViewModel performance)
		{
			CompositeDisposable performanceDisposable = new CompositeDisposable();

			performance.Deceleration
				.BindTo(
					textBoxDeceleration,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDeceleration.TextChanged += h,
							h => textBoxDeceleration.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(performanceDisposable);

			performance.Deceleration
				.BindToErrorProvider(errorProvider, textBoxDeceleration)
				.AddTo(performanceDisposable);

			performance.DecelerationUnit
				.BindTo(
					comboBoxDecelerationUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Acceleration)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxDecelerationUnit.SelectedIndexChanged += h,
							h => comboBoxDecelerationUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(performanceDisposable);

			performance.CoefficientOfStaticFriction
				.BindTo(
					textBoxCoefficientOfStaticFriction,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCoefficientOfStaticFriction.TextChanged += h,
							h => textBoxCoefficientOfStaticFriction.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(performanceDisposable);

			performance.CoefficientOfStaticFriction
				.BindToErrorProvider(errorProvider, textBoxCoefficientOfStaticFriction)
				.AddTo(performanceDisposable);

			performance.CoefficientOfRollingResistance
				.BindTo(
					textBoxCoefficientOfRollingResistance,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCoefficientOfRollingResistance.TextChanged += h,
							h => textBoxCoefficientOfRollingResistance.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(performanceDisposable);

			performance.CoefficientOfRollingResistance
				.BindToErrorProvider(errorProvider, textBoxCoefficientOfRollingResistance)
				.AddTo(performanceDisposable);

			performance.AerodynamicDragCoefficient
				.BindTo(
					textBoxAerodynamicDragCoefficient,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxAerodynamicDragCoefficient.TextChanged += h,
							h => textBoxAerodynamicDragCoefficient.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(performanceDisposable);

			performance.AerodynamicDragCoefficient
				.BindToErrorProvider(errorProvider, textBoxAerodynamicDragCoefficient)
				.AddTo(performanceDisposable);

			return performanceDisposable;
		}
	}
}
