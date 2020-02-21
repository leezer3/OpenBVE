using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCameraRestriction(CameraRestrictionViewModel cameraRestriction)
		{
			CompositeDisposable cameraRestrictionDisposable = new CompositeDisposable();

			cameraRestriction.DefinedForwards
				.BindTo(
					checkBoxCameraRestrictionDefinedForwards,
					x => x.Checked,
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

			cameraRestriction.DefinedForwards
				.BindTo(
					labelCameraRestrictionForwards,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedForwards
				.BindTo(
					textBoxCameraRestrictionForwards,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedForwards
				.BindTo(
					labelCameraRestrictionForwardsUnit,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.Forwards
				.BindTo(
					textBoxCameraRestrictionForwards,
					x => x.Text,
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

			cameraRestriction.DefinedBackwards
				.BindTo(
					checkBoxCameraRestrictionDefinedBackwards,
					x => x.Checked,
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

			cameraRestriction.DefinedBackwards
				.BindTo(
					labelCameraRestrictionBackwards,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedBackwards
				.BindTo(
					textBoxCameraRestrictionBackwards,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedBackwards
				.BindTo(
					labelCameraRestrictionBackwardsUnit,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.Backwards
				.BindTo(
					textBoxCameraRestrictionBackwards,
					x => x.Text,
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

			cameraRestriction.DefinedLeft
				.BindTo(
					checkBoxCameraRestrictionDefinedLeft,
					x => x.Checked,
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

			cameraRestriction.DefinedLeft
				.BindTo(
					labelCameraRestrictionLeft,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedLeft
				.BindTo(
					textBoxCameraRestrictionLeft,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedLeft
				.BindTo(
					labelCameraRestrictionLeftUnit,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.Left
				.BindTo(
					textBoxCameraRestrictionLeft,
					x => x.Text,
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

			cameraRestriction.DefinedRight
				.BindTo(
					checkBoxCameraRestrictionDefinedRight,
					x => x.Checked,
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

			cameraRestriction.DefinedRight
				.BindTo(
					labelCameraRestrictionRight,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedRight
				.BindTo(
					textBoxCameraRestrictionRight,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedRight
				.BindTo(
					labelCameraRestrictionRightUnit,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.Right
				.BindTo(
					textBoxCameraRestrictionRight,
					x => x.Text,
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

			cameraRestriction.DefinedUp
				.BindTo(
					checkBoxCameraRestrictionDefinedUp,
					x => x.Checked,
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

			cameraRestriction.DefinedUp
				.BindTo(
					labelCameraRestrictionUp,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedUp
				.BindTo(
					textBoxCameraRestrictionUp,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedUp
				.BindTo(
					labelCameraRestrictionUpUnit,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.Up
				.BindTo(
					textBoxCameraRestrictionUp,
					x => x.Text,
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

			cameraRestriction.DefinedDown
				.BindTo(
					checkBoxCameraRestrictionDefinedDown,
					x => x.Checked,
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

			cameraRestriction.DefinedDown
				.BindTo(
					labelCameraRestrictionDown,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedDown
				.BindTo(
					textBoxCameraRestrictionDown,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.DefinedDown
				.BindTo(
					labelCameraRestrictionDownUnit,
					x => x.Enabled
				)
				.AddTo(cameraRestrictionDisposable);

			cameraRestriction.Down
				.BindTo(
					textBoxCameraRestrictionDown,
					x => x.Text,
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
