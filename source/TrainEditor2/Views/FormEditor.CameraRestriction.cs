using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.Units;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCameraRestriction(CameraRestrictionViewModel restriction)
		{
			CompositeDisposable restrictionDisposable = new CompositeDisposable();

			restriction.DefinedForwards
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
				.AddTo(restrictionDisposable);

			restriction.DefinedForwards
				.BindTo(
					labelCameraRestrictionForwards,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedForwards
				.BindTo(
					textBoxCameraRestrictionForwards,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedForwards
				.BindTo(
					comboBoxCameraRestrictionForwardsUnit,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.Forwards
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
				.AddTo(restrictionDisposable);

			restriction.Forwards
				.BindToErrorProvider(errorProvider, textBoxCameraRestrictionForwards)
				.AddTo(restrictionDisposable);

			restriction.ForwardsUnit
				.BindTo(
					comboBoxCameraRestrictionForwardsUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCameraRestrictionForwardsUnit.SelectedIndexChanged += h,
							h => comboBoxCameraRestrictionForwardsUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedBackwards
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
				.AddTo(restrictionDisposable);

			restriction.DefinedBackwards
				.BindTo(
					labelCameraRestrictionBackwards,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedBackwards
				.BindTo(
					textBoxCameraRestrictionBackwards,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedBackwards
				.BindTo(
					comboBoxCameraRestrictionBackwardsUnit,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.Backwards
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
				.AddTo(restrictionDisposable);

			restriction.Backwards
				.BindToErrorProvider(errorProvider, textBoxCameraRestrictionBackwards)
				.AddTo(restrictionDisposable);

			restriction.BackwardsUnit
				.BindTo(
					comboBoxCameraRestrictionBackwardsUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCameraRestrictionBackwardsUnit.SelectedIndexChanged += h,
							h => comboBoxCameraRestrictionBackwardsUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedLeft
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
				.AddTo(restrictionDisposable);

			restriction.DefinedLeft
				.BindTo(
					labelCameraRestrictionLeft,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedLeft
				.BindTo(
					textBoxCameraRestrictionLeft,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedLeft
				.BindTo(
					comboBoxCameraRestrictionLeftUnit,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.Left
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
				.AddTo(restrictionDisposable);

			restriction.Left
				.BindToErrorProvider(errorProvider, textBoxCameraRestrictionLeft)
				.AddTo(restrictionDisposable);

			restriction.LeftUnit
				.BindTo(
					comboBoxCameraRestrictionLeftUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCameraRestrictionLeftUnit.SelectedIndexChanged += h,
							h => comboBoxCameraRestrictionLeftUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedRight
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
				.AddTo(restrictionDisposable);

			restriction.DefinedRight
				.BindTo(
					labelCameraRestrictionRight,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedRight
				.BindTo(
					textBoxCameraRestrictionRight,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedRight
				.BindTo(
					comboBoxCameraRestrictionRightUnit,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.Right
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
				.AddTo(restrictionDisposable);

			restriction.Right
				.BindToErrorProvider(errorProvider, textBoxCameraRestrictionRight)
				.AddTo(restrictionDisposable);

			restriction.RightUnit
				.BindTo(
					comboBoxCameraRestrictionRightUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCameraRestrictionRightUnit.SelectedIndexChanged += h,
							h => comboBoxCameraRestrictionRightUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedUp
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
				.AddTo(restrictionDisposable);

			restriction.DefinedUp
				.BindTo(
					labelCameraRestrictionUp,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedUp
				.BindTo(
					textBoxCameraRestrictionUp,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedUp
				.BindTo(
					comboBoxCameraRestrictionUpUnit,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.Up
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
				.AddTo(restrictionDisposable);

			restriction.Up
				.BindToErrorProvider(errorProvider, textBoxCameraRestrictionUp)
				.AddTo(restrictionDisposable);

			restriction.UpUnit
				.BindTo(
					comboBoxCameraRestrictionUpUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCameraRestrictionUpUnit.SelectedIndexChanged += h,
							h => comboBoxCameraRestrictionUpUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedDown
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
				.AddTo(restrictionDisposable);

			restriction.DefinedDown
				.BindTo(
					labelCameraRestrictionDown,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedDown
				.BindTo(
					textBoxCameraRestrictionDown,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.DefinedDown
				.BindTo(
					comboBoxCameraRestrictionDownUnit,
					x => x.Enabled
				)
				.AddTo(restrictionDisposable);

			restriction.Down
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
				.AddTo(restrictionDisposable);

			restriction.Down
				.BindToErrorProvider(errorProvider, textBoxCameraRestrictionDown)
				.AddTo(restrictionDisposable);

			restriction.DownUnit
				.BindTo(
					comboBoxCameraRestrictionDownUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCameraRestrictionDownUnit.SelectedIndexChanged += h,
							h => comboBoxCameraRestrictionDownUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(restrictionDisposable);

			return restrictionDisposable;
		}
	}
}
