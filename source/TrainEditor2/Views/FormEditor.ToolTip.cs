using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using Reactive.Bindings.Extensions;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToToolTip(ToolTipViewModel x, IWin32Window window)
		{
			CompositeDisposable toolTipDisposable = new CompositeDisposable();

			ToolTip toolTip = new ToolTip();

			x.Title
				.Subscribe(y => toolTip.ToolTipTitle = y)
				.AddTo(toolTipDisposable);

			x.Icon
				.Subscribe(y => toolTip.ToolTipIcon = (ToolTipIcon)(int)y)
				.AddTo(toolTipDisposable);

			x.IsOpen
				.Subscribe(y =>
				{
					if (y)
					{
						toolTip.Show(x.Text.Value, window, (int)x.X.Value, (int)x.Y.Value);
					}
					else
					{
						toolTip.Hide(window);
					}
				})
				.AddTo(toolTipDisposable);

			x.Text
				.Subscribe(y =>
				{
					if (x.IsOpen.Value)
					{
						toolTip.Hide(window);
						toolTip.Show(y, window, (int)x.X.Value, (int)x.Y.Value);
					}
				})
				.AddTo(toolTipDisposable);

			x.X.Subscribe(y =>
				{
					if (x.IsOpen.Value)
					{
						toolTip.Hide(window);
						toolTip.Show(x.Text.Value, window, (int)y, (int)x.Y.Value);
					}
				})
				.AddTo(toolTipDisposable);

			x.Y.Subscribe(y =>
				{
					if (x.IsOpen.Value)
					{
						toolTip.Hide(window);
						toolTip.Show(x.Text.Value, window, (int)x.X.Value, (int)y);
					}
				})
				.AddTo(toolTipDisposable);

			return toolTipDisposable;
		}
	}
}
