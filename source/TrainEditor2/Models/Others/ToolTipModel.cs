using Prism.Mvvm;

namespace TrainEditor2.Models.Others
{
	internal class ToolTipModel : BindableBase
	{
		internal enum ToolTipIcon
		{
			None,
			Information,
			Warning,
			Error
		}

		private bool isOpen;
		private string title;
		private ToolTipIcon icon;
		private string text;
		private double x;
		private double y;

		internal bool IsOpen
		{
			get
			{
				return isOpen;
			}
			set
			{
				SetProperty(ref isOpen, value);
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

		internal ToolTipIcon Icon
		{
			get
			{
				return icon;
			}
			set
			{
				SetProperty(ref icon, value);
			}
		}

		internal string Text
		{
			get
			{
				return text;
			}
			set
			{
				SetProperty(ref text, value);
			}
		}

		internal double X
		{
			get
			{
				return x;
			}
			set
			{
				SetProperty(ref x, value);
			}
		}

		internal double Y
		{
			get
			{
				return y;
			}
			set
			{
				SetProperty(ref y, value);
			}
		}
	}
}
