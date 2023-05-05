using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormDelay : Form
	{
		private readonly CompositeDisposable disposable;

		private ReactiveProperty<DelayViewModel.EntryViewModel> SelectedEntry
		{
			get;
		}

		internal FormDelay(ReadOnlyReactiveCollection<DelayViewModel.EntryViewModel> delay)
		{
			InitializeComponent();

			disposable = new CompositeDisposable();

			string[] timeUnits = Unit.GetAllRewords<Unit.Time>();

			comboBoxUpUnit.Items.AddRange((string[])timeUnits.Clone());
			comboBoxDownUnit.Items.AddRange((string[])timeUnits.Clone());

			listViewDelay.Items.AddRange(delay.Select((x, i) => new ListViewItem(new[] { (i + 1).ToString(CultureInfo.InvariantCulture), x.Up.Value, x.Down.Value }) { Tag = x }).ToArray());

			SelectedEntry = new ReactiveProperty<DelayViewModel.EntryViewModel>();

			SelectedEntry
				.BindTo(
					listViewDelay,
					x => x.FocusedItem,
					BindingMode.OneWayToSource,
					null,
					x => (DelayViewModel.EntryViewModel)x?.Tag,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => listViewDelay.SelectedIndexChanged += h,
							h => listViewDelay.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			CompositeDisposable entryDisposable = new CompositeDisposable().AddTo(disposable);

			SelectedEntry
				.Where(x => x != null)
				.Subscribe(x =>
				{
					entryDisposable.Dispose();
					entryDisposable = new CompositeDisposable().AddTo(disposable);

					x.Up.BindTo(
							textBoxUp,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxUp.TextChanged += h,
									h => textBoxDown.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.Up.BindTo(
							listViewDelay.FocusedItem.SubItems[1],
							y => y.Text
						)
						.AddTo(entryDisposable);

					x.Up.BindToErrorProvider(errorProvider, textBoxUp).AddTo(entryDisposable);

					x.UpUnit
						.BindTo(
							comboBoxUpUnit,
							y => y.SelectedIndex,
							BindingMode.TwoWay,
							y => (int)y,
							y => (Unit.Time)y,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => comboBoxUpUnit.SelectedIndexChanged += h,
									h => comboBoxUpUnit.SelectedIndexChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.Down.BindTo(
							textBoxDown,
							y => y.Text,
							BindingMode.TwoWay,
							null,
							null,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => textBoxDown.TextChanged += h,
									h => textBoxDown.TextChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);

					x.Down.BindTo(
							listViewDelay.FocusedItem.SubItems[2],
							y => y.Text
						)
						.AddTo(entryDisposable);

					x.Down.BindToErrorProvider(errorProvider, textBoxDown).AddTo(entryDisposable);

					x.DownUnit
						.BindTo(
							comboBoxDownUnit,
							y => y.SelectedIndex,
							BindingMode.TwoWay,
							y => (int)y,
							y => (Unit.Time)y,
							Observable.FromEvent<EventHandler, EventArgs>(
									h => (s, e) => h(e),
									h => comboBoxDownUnit.SelectedIndexChanged += h,
									h => comboBoxDownUnit.SelectedIndexChanged -= h
								)
								.ToUnit()
						)
						.AddTo(entryDisposable);
				})
				.AddTo(disposable);

			groupBoxEntry.Enabled = listViewDelay.SelectedIndices.Count == 1;
		}

		private void FormDelay_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void ListViewDelay_SelectedIndexChanged(object sender, EventArgs e)
		{
			groupBoxEntry.Enabled = listViewDelay.SelectedIndices.Count == 1;
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
