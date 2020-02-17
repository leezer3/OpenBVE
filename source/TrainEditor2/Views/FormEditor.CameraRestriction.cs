using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	partial class FormEditor
	{
		private IDisposable BindToCameraRestriction(CameraRestrictionViewModel x)
		{
			CompositeDisposable cameraRestrictionDisposable = new CompositeDisposable();

			x.DefinedForwards
				.BindTo(
					checkBoxCameraRestrictionDefinedForwards,
					y => y.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxCameraRestrictionDefinedForwards.CheckedChanged += h,
							h => checkBoxCameraRestrictionDefinedForwards.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedForwards
				.BindTo(
					labelCameraRestrictionForwards,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedForwards
				.BindTo(
					textBoxCameraRestrictionForwards,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedForwards
				.BindTo(
					labelCameraRestrictionForwardsUnit,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.Forwards
				.BindTo(
					textBoxCameraRestrictionForwards,
					y => y.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCameraRestrictionForwards.TextChanged += h,
							h => textBoxCameraRestrictionForwards.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedBackwards
				.BindTo(
					checkBoxCameraRestrictionDefinedBackwards,
					y => y.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxCameraRestrictionDefinedBackwards.CheckedChanged += h,
							h => checkBoxCameraRestrictionDefinedBackwards.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedBackwards
				.BindTo(
					labelCameraRestrictionBackwards,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedBackwards
				.BindTo(
					textBoxCameraRestrictionBackwards,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedBackwards
				.BindTo(
					labelCameraRestrictionBackwardsUnit,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.Backwards
				.BindTo(
					textBoxCameraRestrictionBackwards,
					y => y.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCameraRestrictionBackwards.TextChanged += h,
							h => textBoxCameraRestrictionBackwards.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedLeft
				.BindTo(
					checkBoxCameraRestrictionDefinedLeft,
					y => y.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxCameraRestrictionDefinedLeft.CheckedChanged += h,
							h => checkBoxCameraRestrictionDefinedLeft.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedLeft
				.BindTo(
					labelCameraRestrictionLeft,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedLeft
				.BindTo(
					textBoxCameraRestrictionLeft,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedLeft
				.BindTo(
					labelCameraRestrictionLeftUnit,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.Left
				.BindTo(
					textBoxCameraRestrictionLeft,
					y => y.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCameraRestrictionLeft.TextChanged += h,
							h => textBoxCameraRestrictionLeft.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedRight
				.BindTo(
					checkBoxCameraRestrictionDefinedRight,
					y => y.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxCameraRestrictionDefinedRight.CheckedChanged += h,
							h => checkBoxCameraRestrictionDefinedRight.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedRight
				.BindTo(
					labelCameraRestrictionRight,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedRight
				.BindTo(
					textBoxCameraRestrictionRight,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedRight
				.BindTo(
					labelCameraRestrictionRightUnit,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.Right
				.BindTo(
					textBoxCameraRestrictionRight,
					y => y.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCameraRestrictionRight.TextChanged += h,
							h => textBoxCameraRestrictionRight.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedUp
				.BindTo(
					checkBoxCameraRestrictionDefinedUp,
					y => y.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxCameraRestrictionDefinedUp.CheckedChanged += h,
							h => checkBoxCameraRestrictionDefinedUp.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedUp
				.BindTo(
					labelCameraRestrictionUp,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedUp
				.BindTo(
					textBoxCameraRestrictionUp,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedUp
				.BindTo(
					labelCameraRestrictionUpUnit,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.Up
				.BindTo(
					textBoxCameraRestrictionUp,
					y => y.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCameraRestrictionUp.TextChanged += h,
							h => textBoxCameraRestrictionUp.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedDown
				.BindTo(
					checkBoxCameraRestrictionDefinedDown,
					y => y.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxCameraRestrictionDefinedDown.CheckedChanged += h,
							h => checkBoxCameraRestrictionDefinedDown.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedDown
				.BindTo(
					labelCameraRestrictionDown,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedDown
				.BindTo(
					textBoxCameraRestrictionDown,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.DefinedDown
				.BindTo(
					labelCameraRestrictionDownUnit,
					y => y.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			x.Down
				.BindTo(
					textBoxCameraRestrictionDown,
					y => y.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCameraRestrictionDown.TextChanged += h,
							h => textBoxCameraRestrictionDown.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cameraRestrictionDisposable);

			return cameraRestrictionDisposable;
		}
	}
}
