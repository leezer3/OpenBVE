using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Sounds;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToSoundElement(SoundElementViewModel element)
		{
			CompositeDisposable elementDisposable = new CompositeDisposable();

			element.FilePath
				.BindTo(
					textBoxSoundFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundFileName.TextChanged += h,
							h => textBoxSoundFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.DefinedRadius
				.BindTo(
					checkBoxSoundRadius,
					x => x.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxSoundRadius.CheckedChanged += h,
							h => checkBoxSoundRadius.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.DefinedRadius
				.BindTo(
					textBoxSoundRadius,
					x => x.Enabled
				)
				.AddTo(elementDisposable);

			element.Radius
				.BindTo(
					textBoxSoundRadius,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundRadius.TextChanged += h,
							h => textBoxSoundRadius.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.Radius
				.BindToErrorProvider(errorProvider, textBoxSoundRadius)
				.AddTo(elementDisposable);

			element.DefinedPosition
				.BindTo(
					checkBoxSoundDefinedPosition,
					x => x.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxSoundDefinedPosition.CheckedChanged += h,
							h => checkBoxSoundDefinedPosition.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.DefinedPosition
				.BindTo(
					groupBoxSoundPosition,
					x => x.Enabled
				)
				.AddTo(elementDisposable);

			element.PositionX
				.BindTo(
					textBoxSoundPositionX,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundPositionX.TextChanged += h,
							h => textBoxSoundPositionX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.PositionX
				.BindToErrorProvider(errorProvider, textBoxSoundPositionX)
				.AddTo(elementDisposable);

			element.PositionY
				.BindTo(
					textBoxSoundPositionY,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundPositionY.TextChanged += h,
							h => textBoxSoundPositionY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.PositionY
				.BindToErrorProvider(errorProvider, textBoxSoundPositionY)
				.AddTo(elementDisposable);

			element.PositionZ
				.BindTo(
					textBoxSoundPositionZ,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundPositionZ.TextChanged += h,
							h => textBoxSoundPositionZ.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.PositionZ
				.BindToErrorProvider(errorProvider, textBoxSoundPositionZ)
				.AddTo(elementDisposable);

			return elementDisposable;
		}

		private IDisposable BindToSoundElement<T>(T element) where T : SoundElementViewModel<int>
		{
			CompositeDisposable elementDisposable = new CompositeDisposable();

			comboBoxSoundKey.Enabled = false;
			comboBoxSoundKey.Items.Clear();

			numericUpDownSoundKeyIndex.Enabled = true;

			element.Key
				.BindTo(
					numericUpDownSoundKeyIndex,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownSoundKeyIndex.ValueChanged += h,
							h => numericUpDownSoundKeyIndex.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.Key
				.BindToErrorProvider(errorProvider, numericUpDownSoundKeyIndex)
				.AddTo(elementDisposable);

			BindToSoundElement((SoundElementViewModel)element).AddTo(elementDisposable);

			return elementDisposable;
		}

		private IDisposable BindToSoundElement<T, U>(T element) where T : SoundElementViewModel<U>
		{
			CompositeDisposable elementDisposable = new CompositeDisposable();

			comboBoxSoundKey.Enabled = true;
			comboBoxSoundKey.Items.Clear();
			comboBoxSoundKey.Items.AddRange(Enum.GetValues(typeof(U)).OfType<Enum>().Select(x => x.GetStringValues().First()).OfType<object>().ToArray());

			element.Key
				.BindTo(
					comboBoxSoundKey,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)(object)x,
					x => (U)(object)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxSoundKey.SelectedIndexChanged += h,
							h => comboBoxSoundKey.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(elementDisposable);

			element.Key
				.BindToErrorProvider(errorProvider, comboBoxSoundKey)
				.AddTo(elementDisposable);

			numericUpDownSoundKeyIndex.Enabled = false;

			BindToSoundElement((SoundElementViewModel)element).AddTo(elementDisposable);

			return elementDisposable;
		}
	}
}
