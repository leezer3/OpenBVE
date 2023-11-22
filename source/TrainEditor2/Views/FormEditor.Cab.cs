using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.World;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCab(CabViewModel cab)
		{
			CompositeDisposable cabDisposable = new CompositeDisposable();
			CompositeDisposable panelDisposable = new CompositeDisposable().AddTo(cabDisposable);
			CompositeDisposable restrictionDisposable = new CompositeDisposable().AddTo(cabDisposable);

			cab.PositionX
				.BindTo(
					textBoxCabX,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabX.TextChanged += h,
							h => textBoxCabX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			cab.PositionX
				.BindToErrorProvider(errorProvider, textBoxCabX)
				.AddTo(cabDisposable);

			cab.PositionX_Unit
				.BindTo(
					comboBoxCabXUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCabXUnit.SelectedIndexChanged += h,
							h => comboBoxCabXUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			cab.PositionY
				.BindTo(
					textBoxCabY,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabY.TextChanged += h,
							h => textBoxCabY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			cab.PositionY
				.BindToErrorProvider(errorProvider, textBoxCabY)
				.AddTo(cabDisposable);

			cab.PositionY_Unit
				.BindTo(
					comboBoxCabYUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCabYUnit.SelectedIndexChanged += h,
							h => comboBoxCabYUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			cab.PositionZ
				.BindTo(
					textBoxCabZ,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabZ.TextChanged += h,
							h => textBoxCabZ.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			cab.PositionZ
				.BindToErrorProvider(errorProvider, textBoxCabZ)
				.AddTo(cabDisposable);

			cab.PositionZ_Unit
				.BindTo(
					comboBoxCabZUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCabZUnit.SelectedIndexChanged += h,
							h => comboBoxCabZUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			EmbeddedCabViewModel embeddedCab = cab as EmbeddedCabViewModel;
			ExternalCabViewModel externalCab = cab as ExternalCabViewModel;

			embeddedCab?.Panel
				.Subscribe(x =>
				{
					panelDisposable.Dispose();
					panelDisposable = new CompositeDisposable().AddTo(cabDisposable);

					BindToPanel(x).AddTo(panelDisposable);
				})
				.AddTo(cabDisposable);

			externalCab?.FileName
				.BindTo(
					textBoxCabFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabFileName.TextChanged += h,
							h => textBoxCabFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			externalCab?.CameraRestriction
				.Subscribe(x =>
				{
					restrictionDisposable.Dispose();
					restrictionDisposable = new CompositeDisposable().AddTo(cabDisposable);

					BindToCameraRestriction(x).AddTo(restrictionDisposable);
				})
				.AddTo(cabDisposable);

			return cabDisposable;
		}
	}
}
