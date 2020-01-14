using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using OpenBveApi.Interface;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormTouch : Form
	{
		private readonly CompositeDisposable disposable;

		private readonly TouchElementViewModel touch;

		internal FormTouch(TouchElementViewModel touch)
		{
			disposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable().AddTo(disposable);
			CompositeDisposable entryDisposable = new CompositeDisposable().AddTo(disposable);

			this.touch = touch;

			InitializeComponent();

			Binders.BindToTreeView(treeViewTouch, touch.TreeItems, touch.SelectedTreeItem).AddTo(disposable);

			touch.SelectedTreeItem
				.BindTo(
					listViewTouch,
					x => x.Enabled,
					BindingMode.OneWay,
					x => touch.TreeItems[0].Children.Contains(x)
				)
				.AddTo(disposable);

			Binders.BindToListView(listViewTouch, touch.ListColumns, touch.ListItems, touch.SelectedListItem).AddTo(disposable);

			touch.SelectedSoundEntry
				.BindTo(
					groupBoxTouchSound,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(disposable);

			touch.SelectedSoundEntry
				.Where(x => x != null)
				.Subscribe(x =>
				{
					entryDisposable.Dispose();
					entryDisposable = new CompositeDisposable().AddTo(disposable);

					BindToSoundEntry(x).AddTo(entryDisposable);
				})
				.AddTo(disposable);

			touch.SelectedCommandEntry
				.BindTo(
					groupBoxTouchCommand,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(disposable);

			touch.SelectedCommandEntry
				.Where(x => x != null)
				.Subscribe(x =>
				{
					entryDisposable.Dispose();
					entryDisposable = new CompositeDisposable().AddTo(disposable);

					BindToCommandEntry(x).AddTo(entryDisposable);
				})
				.AddTo(disposable);

			comboBoxTouchCommandName.Items
				.AddRange(
					Enum.GetValues(typeof(Translations.Command))
						.OfType<Translations.Command>()
						.Select(c => Translations.CommandInfos.TryGetInfo(c).Name)
						.OfType<object>()
						.ToArray()
				);

			new[] { touch.AddSoundEntry, touch.AddCommandEntry }
				.BindToButton(buttonTouchAdd)
				.AddTo(disposable);

			new[] { touch.CopySoundEntry, touch.CopyCommandEntry }
				.BindToButton(buttonTouchCopy)
				.AddTo(disposable);

			new[] { touch.RemoveSoundEntry, touch.RemoveCommandEntry }
				.BindToButton(buttonTouchRemove)
				.AddTo(disposable);
		}

		private void FormTouch_Load(object sender, EventArgs e)
		{
			Icon = FormEditor.GetIcon();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		private IDisposable BindToSoundEntry(TouchElementViewModel.SoundEntryViewModel entry)
		{
			CompositeDisposable entryDisposable = new CompositeDisposable();

			entry.Index
				.BindTo(
					numericUpDownTouchSoundIndex,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchSoundIndex.ValueChanged += h,
							h => numericUpDownTouchSoundIndex.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(entryDisposable);

			return entryDisposable;
		}

		private IDisposable BindToCommandEntry(TouchElementViewModel.CommandEntryViewModel entry)
		{
			CompositeDisposable entryDisposable = new CompositeDisposable();

			entry.Info
				.BindTo(
					comboBoxTouchCommandName,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x.Command,
					x => Translations.CommandInfos.TryGetInfo((Translations.Command)x),
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxTouchCommandName.SelectedIndexChanged += h,
							h => comboBoxTouchCommandName.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(entryDisposable);

			entry.Option
				.BindTo(
					numericUpDownTouchCommandOption,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchCommandOption.ValueChanged += h,
							h => numericUpDownTouchCommandOption.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(entryDisposable);

			return entryDisposable;
		}
	}
}
