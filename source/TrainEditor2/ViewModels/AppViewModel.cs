using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Dialogs;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.ViewModels
{
	internal class AppViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> CurrentLanguageCode
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> SaveLocation
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MessageBoxViewModel> MessageBox
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TrainViewModel> Train
		{
			get;
		}

		internal ReactiveProperty<TreeViewItemViewModel> Item
		{
			get;
		}

		internal ReactiveProperty<TreeViewItemViewModel> SelectedItem
		{
			get;
		}

		internal ReactiveCommand CreateNewFileCommand
		{
			get;
		}

		internal ReactiveCommand UpCarCommand
		{
			get;
		}

		internal ReactiveCommand DownCarCommand
		{
			get;
		}

		internal ReactiveCommand AddCarCommand
		{
			get;
		}

		internal ReactiveCommand RemoveCarCommand
		{
			get;
		}

		internal ReactiveCommand CopyCarCommand
		{
			get;
		}

		internal ReactiveCommand UpCouplerCommand
		{
			get;
		}

		internal ReactiveCommand DownCouplerCommand
		{
			get;
		}

		internal ReactiveCommand ChangeCarClass
		{
			get;
		}

		internal AppViewModel()
		{
			CompositeDisposable itemDisposable = new CompositeDisposable();

			App app = new App();

			CurrentLanguageCode = app
				.ToReactivePropertyAsSynchronized(x => x.CurrentLanguageCode)
				.AddTo(disposable);

			CurrentLanguageCode.Subscribe(_ => app.CreateItem()).AddTo(disposable);

			SaveLocation = app
				.ObserveProperty(x => x.SaveLocation)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			MessageBox = app
				.ObserveProperty(x => x.MessageBox)
				.Do(_ => MessageBox?.Value.Dispose())
				.Select(x => new MessageBoxViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Train = app
				.ObserveProperty(x => x.Train)
				.Do(_ => Train?.Value.Dispose())
				.Select(x => new TrainViewModel(x, app))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Item = app
				.ObserveProperty(x => x.Item)
				.Do(_ => Item?.Value.Dispose())
				.Select(x => new TreeViewItemViewModel(x))
				.ToReactiveProperty()
				.AddTo(disposable);

			Item.Subscribe(x =>
				{
					itemDisposable.Dispose();
					itemDisposable = new CompositeDisposable();

					x.PropertyChangedAsObservable()
						.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.None)
						.Subscribe(_ => Item.ForceNotify())
						.AddTo(itemDisposable);
				})
				.AddTo(disposable);

			SelectedItem = app
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedItem,
					x => Item.Value.SearchViewModel(x),
					x => x?.Model
				)
				.AddTo(disposable);

			CreateNewFileCommand = new ReactiveCommand();
			CreateNewFileCommand.Subscribe(() => app.CreateNewFile()).AddTo(disposable);

			UpCarCommand = SelectedItem
				.Select(x => Item.Value.Children[1].Children.IndexOf(x) > 0)
				.ToReactiveCommand()
				.AddTo(disposable);

			UpCarCommand.Subscribe(() => app.UpCar()).AddTo(disposable);

			DownCarCommand = SelectedItem
				.Select(x => Item.Value.Children[1].Children.IndexOf(x))
				.Select(x => x >= 0 && x < Item.Value.Children[1].Children.Count - 1)
				.ToReactiveCommand()
				.AddTo(disposable);

			DownCarCommand.Subscribe(() => app.DownCar()).AddTo(disposable);

			AddCarCommand = SelectedItem
				.Select(x => x == Item.Value.Children[1] || Item.Value.Children[1].Children.Contains(x))
				.ToReactiveCommand()
				.AddTo(disposable);

			AddCarCommand.Subscribe(() => app.AddCar()).AddTo(disposable);

			RemoveCarCommand = SelectedItem
				.Select(x => Item.Value.Children[1].Children.Contains(x) && Item.Value.Children[1].Children.Where(y => y != x).Any(y => y.Tag.Value is MotorCar))
				.ToReactiveCommand()
				.AddTo(disposable);

			RemoveCarCommand.Subscribe(() => app.RemoveCar()).AddTo(disposable);

			CopyCarCommand = SelectedItem
				.Select(x => Item.Value.Children[1].Children.Contains(x))
				.ToReactiveCommand()
				.AddTo(disposable);

			CopyCarCommand.Subscribe(() => app.CopyCar()).AddTo(disposable);

			UpCouplerCommand = SelectedItem
				.Select(x => Item.Value.Children[2].Children.IndexOf(x) > 0)
				.ToReactiveCommand()
				.AddTo(disposable);

			UpCouplerCommand.Subscribe(() => app.UpCoupler()).AddTo(disposable);

			DownCouplerCommand = SelectedItem
				.Select(x => Item.Value.Children[2].Children.IndexOf(x))
				.Select(x => x >= 0 && x < Item.Value.Children[2].Children.Count - 1)
				.ToReactiveCommand()
				.AddTo(disposable);

			DownCouplerCommand.Subscribe(() => app.DownCoupler()).AddTo(disposable);

			ChangeCarClass = SelectedItem
				.Select(x => x?.Tag.Value is TrailerCar || Item.Value.Children[1].Children.Count(y => y?.Tag.Value is MotorCar) > 1)
				.ToReactiveCommand()
				.AddTo(disposable);

			ChangeCarClass
				.Subscribe(() => app.ChangeCarClass(app.Train.Cars.IndexOf((Car)SelectedItem.Value.Tag.Value)))
				.AddTo(disposable);

			itemDisposable.AddTo(disposable);
		}
	}
}
