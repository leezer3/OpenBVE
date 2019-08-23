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
		private IDisposable BindToPerformance(PerformanceViewModel z)
		{
			CompositeDisposable performanceDisposable = new CompositeDisposable();

			z.Deceleration
				.BindTo(
					textBoxDeceleration,
					w => w.Text,
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

			z.Deceleration
				.BindToErrorProvider(errorProvider, textBoxDeceleration)
				.AddTo(performanceDisposable);

			z.CoefficientOfStaticFriction
				.BindTo(
					textBoxCoefficientOfStaticFriction,
					w => w.Text,
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

			z.CoefficientOfStaticFriction
				.BindToErrorProvider(errorProvider, textBoxCoefficientOfStaticFriction)
				.AddTo(performanceDisposable);

			z.CoefficientOfRollingResistance
				.BindTo(
					textBoxCoefficientOfRollingResistance,
					w => w.Text,
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

			z.CoefficientOfRollingResistance
				.BindToErrorProvider(errorProvider, textBoxCoefficientOfRollingResistance)
				.AddTo(performanceDisposable);

			z.AerodynamicDragCoefficient
				.BindTo(
					textBoxAerodynamicDragCoefficient,
					w => w.Text,
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

			z.AerodynamicDragCoefficient
				.BindToErrorProvider(errorProvider, textBoxAerodynamicDragCoefficient)
				.AddTo(performanceDisposable);

			return performanceDisposable;
		}
	}
}
