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
		private IDisposable BindToTimetable(TimetableElementViewModel timetable)
		{
			CompositeDisposable timetableDisposable = new CompositeDisposable();

			timetable.LocationX
				.BindTo(
					textBoxTimetableLocationX,
					x => x.Text,
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

			timetable.LocationX
				.BindToErrorProvider(errorProvider, textBoxTimetableLocationX)
				.AddTo(timetableDisposable);

			timetable.LocationY
				.BindTo(
					textBoxTimetableLocationY,
					x => x.Text,
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

			timetable.LocationY
				.BindToErrorProvider(errorProvider, textBoxTimetableLocationY)
				.AddTo(timetableDisposable);

			timetable.Layer
				.BindTo(
					numericUpDownTimetableLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTimetableLayer.ValueChanged += h,
							h => numericUpDownTimetableLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(timetableDisposable);

			timetable.Width
				.BindTo(
					textBoxTimetableWidth,
					x => x.Text,
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

			timetable.Width
				.BindToErrorProvider(errorProvider, textBoxTimetableWidth)
				.AddTo(timetableDisposable);

			timetable.Height
				.BindTo(
					textBoxTimetableHeight,
					x => x.Text,
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

			timetable.Height
				.BindToErrorProvider(errorProvider, textBoxTimetableHeight)
				.AddTo(timetableDisposable);

			timetable.TransparentColor
				.BindTo(
					textBoxTimetableTransparentColor,
					x => x.Text,
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

			timetable.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxTimetableTransparentColor)
				.AddTo(timetableDisposable);

			return timetableDisposable;
		}
	}
}
