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
		private IDisposable BindToTimetable(TimetableElementViewModel y)
		{
			CompositeDisposable timetableDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxTimetableLocationX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTimetableLocationX.TextChanged += h,
							h => textBoxTimetableLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxTimetableLocationX)
				.AddTo(timetableDisposable);

			y.LocationY
				.BindTo(
					textBoxTimetableLocationY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTimetableLocationY.TextChanged += h,
							h => textBoxTimetableLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxTimetableLocationY)
				.AddTo(timetableDisposable);

			y.Layer
				.BindTo(
					numericUpDownTimetableLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTimetableLayer.ValueChanged += h,
							h => numericUpDownTimetableLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			y.Width
				.BindTo(
					textBoxTimetableWidth,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTimetableWidth.TextChanged += h,
							h => textBoxTimetableWidth.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			y.Width
				.BindToErrorProvider(errorProvider, textBoxTimetableWidth)
				.AddTo(timetableDisposable);

			y.Height
				.BindTo(
					textBoxTimetableHeight,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTimetableHeight.TextChanged += h,
							h => textBoxTimetableHeight.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			y.Height
				.BindToErrorProvider(errorProvider, textBoxTimetableHeight)
				.AddTo(timetableDisposable);

			y.TransparentColor
				.BindTo(
					textBoxTimetableTransparentColor,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTimetableTransparentColor.TextChanged += h,
							h => textBoxTimetableTransparentColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			y.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxTimetableTransparentColor)
				.AddTo(timetableDisposable);

			return timetableDisposable;
		}
	}
}
