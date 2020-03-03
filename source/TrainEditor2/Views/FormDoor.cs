using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using OpenBveApi.Units;
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

			string[] lengthUnits = Unit.GetAllRewords<Unit.Length>();

			comboBoxWidthUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxMaxToleranceUnit.Items.AddRange((string[])lengthUnits.Clone());

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

			door.WidthUnit
				.BindTo(
					comboBoxWidthUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxWidthUnit.SelectedIndexChanged += h,
							h => comboBoxWidthUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
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

			door.MaxToleranceUnit
				.BindTo(
					comboBoxMaxToleranceUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxMaxToleranceUnit.SelectedIndexChanged += h,
							h => comboBoxMaxToleranceUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
