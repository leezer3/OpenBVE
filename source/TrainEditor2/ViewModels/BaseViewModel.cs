using System;
using System.Reactive.Disposables;
using Prism.Mvvm;

namespace TrainEditor2.ViewModels
{
	internal abstract class BaseViewModel : BindableBase, IDisposable
	{
		protected CompositeDisposable disposable
		{
			get;
		}

		protected BaseViewModel()
		{
			disposable = new CompositeDisposable();
		}

		public void Dispose()
		{
			disposable.Dispose();
		}
	}
}
