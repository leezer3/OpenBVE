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
	public partial class FormBogie : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormBogie(CarViewModel.BogieViewModel bogie)
		{
			InitializeComponent();

			disposable = new CompositeDisposable();

			string[] lengthUnits = Enum.GetValues(typeof(Unit.Length)).OfType<Enum>().Select(x => Unit.GetRewords(x).First()).ToArray();

			comboBoxFrontAxleUnit.Items.AddRange((string[])lengthUnits.Clone());
			comboBoxRearAxleUnit.Items.AddRange((string[])lengthUnits.Clone());

			bogie.DefinedAxles
				.BindTo(
					checkBoxDefinedAxles,
					x => x.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxDefinedAxles.CheckedChanged += h,
							h => checkBoxDefinedAxles.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			bogie.DefinedAxles
				.BindTo(
					groupBoxAxles,
					x => x.Enabled
				)
				.AddTo(disposable);

			bogie.FrontAxle
				.BindTo(
					textBoxFrontAxle,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxFrontAxle.TextChanged += h,
							h => textBoxFrontAxle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			bogie.FrontAxle
				.BindToErrorProvider(errorProvider, textBoxFrontAxle)
				.AddTo(disposable);

			bogie.FrontAxleUnit
				.BindTo(
					comboBoxFrontAxleUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxFrontAxleUnit.SelectedIndexChanged += h,
							h => comboBoxFrontAxleUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			bogie.RearAxle
				.BindTo(
					textBoxRearAxle,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxRearAxle.TextChanged += h,
							h => textBoxRearAxle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			bogie.RearAxle
				.BindToErrorProvider(errorProvider, textBoxRearAxle)
				.AddTo(disposable);

			bogie.RearAxleUnit
				.BindTo(
					comboBoxRearAxleUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Length)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxRearAxleUnit.SelectedIndexChanged += h,
							h => comboBoxRearAxleUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			bogie.Reversed
				.BindTo(
					checkBoxReversed,
					x => x.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxReversed.CheckedChanged += h,
							h => checkBoxReversed.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			bogie.Object
				.BindTo(
					textBoxObject,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxObject.TextChanged += h,
							h => textBoxObject.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);
		}

		private void FormBogie_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();

			labelDefinedAxles.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "defined_axles")}:";
			groupBoxAxles.Text = Utilities.GetInterfaceString("car_settings", "general", "axles", "name");
			labelFrontAxle.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "axles", "front")}:";
			labelRearAxle.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "axles", "rear")}:";
			labelReversed.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "reversed")}:";
			labelObject.Text = $@"{Utilities.GetInterfaceString("car_settings", "general", "object")}:";
			buttonOpen.Text = Utilities.GetInterfaceString("navigation", "open");
		}

		private void ButtonOpen_Click(object sender, EventArgs e)
		{
			WinFormsUtilities.OpenFileDialog(textBoxObject);
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void checkBoxDefinedAxles_CheckedChanged(object sender, EventArgs e)
		{

		}
	}
}
