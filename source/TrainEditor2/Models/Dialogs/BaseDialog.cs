using Prism.Mvvm;

namespace TrainEditor2.Models.Dialogs
{
	internal abstract class BaseDialog : BindableBase
	{
		internal enum DialogIcon
		{
			None,
			Information,
			Question,
			Warning,
			Error
		}

		internal enum DialogButton
		{
			Ok,
			OkCancel,
			YesNo,
			YesNoCancel
		}

		private bool isOpen;
		private bool? dialogResult;
		private string title;

		internal bool IsOpen
		{
			get
			{
				return isOpen;
			}
			set
			{
				if (value)
				{
					DialogResult = null;
				}

				SetProperty(ref isOpen, value);
			}
		}

		internal bool? DialogResult
		{
			get
			{
				return dialogResult;
			}
			set
			{
				SetProperty(ref dialogResult, value);
			}
		}

		internal string Title
		{
			get
			{
				return title;
			}
			set
			{
				SetProperty(ref title, value);
			}
		}
	}
}
