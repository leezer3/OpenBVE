using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Panels;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormSubject : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormSubject(SubjectViewModel subject)
		{
			InitializeComponent();

			disposable = new CompositeDisposable();

			comboBoxBase.Items.AddRange(Enum.GetNames(typeof(SubjectBase)).OfType<object>().ToArray());
			comboBoxSuffix.Items.AddRange(Enum.GetNames(typeof(SubjectSuffix)).OfType<object>().ToArray());

			subject.Base
				.BindTo(
					comboBoxBase,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (SubjectBase)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBase.SelectedIndexChanged += h,
							h => comboBoxBase.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			subject.BaseOption
				.BindTo(
					numericUpDownBaseOption,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownBaseOption.ValueChanged += h,
							h => numericUpDownBaseOption.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			subject.Suffix
				.BindTo(
					comboBoxSuffix,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (SubjectSuffix)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxSuffix.SelectedIndexChanged += h,
							h => comboBoxSuffix.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			subject.SuffixOption
				.BindTo(
					numericUpDownSuffixOption,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownSuffixOption.ValueChanged += h,
							h => numericUpDownSuffixOption.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);
		}

		private void FormSubject_Load(object sender, EventArgs e)
		{
			Icon = FormEditor.GetIcon();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
