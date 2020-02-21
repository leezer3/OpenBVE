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
	public partial class FormDoor : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormDoor(CarViewModel.DoorViewModel door)
		{
			InitializeComponent();

			disposable = new CompositeDisposable();

			door.Width
				.BindTo(
					textBoxWidth,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxWidth.TextChanged += h,
							h => textBoxWidth.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			door.Width
				.BindToErrorProvider(errorProvider, textBoxWidth)
				.AddTo(disposable);

			door.MaxTolerance
				.BindTo(
					textBoxMaxTolerance,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMaxTolerance.TextChanged += h,
							h => textBoxMaxTolerance.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			door.MaxTolerance
				.BindToErrorProvider(errorProvider, textBoxMaxTolerance)
				.AddTo(disposable);
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
