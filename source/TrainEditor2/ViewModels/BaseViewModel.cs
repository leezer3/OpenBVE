using System;
using System.Reactive.Disposables;

namespace TrainEditor2.ViewModels
{
	internal abstract class BaseViewModel : IDisposable
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
