using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormJerk : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormJerk(JerkViewModel.EntryViewModel entry)
		{
			InitializeComponent();

			disposable = new CompositeDisposable();

			entry.Up
				.BindTo(
					textBoxUp,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxUp.TextChanged += h,
							h => textBoxUp.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			entry.Up
				.BindToErrorProvider(errorProvider, textBoxUp)
				.AddTo(disposable);

			entry.Down
				.BindTo(
					textBoxDown,
					w => w.Text,
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
				.AddTo(disposable);

			entry.Down
				.BindToErrorProvider(errorProvider, textBoxDown)
				.AddTo(disposable);
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
