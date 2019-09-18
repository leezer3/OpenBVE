using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace TrainEditor2.Extensions
{
	internal static class Binders
	{
		internal static IDisposable BindToButton(this ReactiveCommand command, Button button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode:ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton(this IEnumerable<ReactiveCommand> commands, Button button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			ReactiveCommand[] commandsArray = commands.ToArray();

			commandsArray
				.Select(x => x.CanExecuteChangedAsObservable())
				.Merge()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = commandsArray.Select(x => x.CanExecute()).Any(x => x))
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ =>
				{
					foreach (ReactiveCommand command in commandsArray.Where(x => x.CanExecute()).ToArray())
					{
						command.Execute();
					}
				})
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton(this ReactiveCommand command, ToolStripMenuItem button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
				h => (s, e) => h(e),
				h => button.Click += h,
				h => button.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton<T>(this ReactiveCommand<T> command, T value, ToolStripMenuItem button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute(value))
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton(this ReactiveCommand command, ToolStripButton button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton<T>(this ReactiveCommand<T> command, T value, ToolStripButton button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute(value))
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToCheckBox(this ReactiveCommand command, CheckBox checkBox)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => checkBox.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => checkBox.Click += h,
					h => checkBox.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToErrorProvider<T>(this ReactiveProperty<T> property, ErrorProvider errorProvider, Control control)
		{
			return property.ObserveErrorChanged.Subscribe(x => errorProvider.SetError(control, x?.OfType<string>().FirstOrDefault(y => true)));
		}
	}
}
