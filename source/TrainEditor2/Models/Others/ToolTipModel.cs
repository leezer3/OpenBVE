using TrainEditor2.Extensions;

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
			get => isOpen;
			set => SetProperty(ref isOpen, value);
		}

		internal string Title
		{
			get => title;
			set => SetProperty(ref title, value);
		}

		internal ToolTipIcon Icon
		{
			get => icon;
			set => SetProperty(ref icon, value);
		}

		internal string Text
		{
			get => text;
			set => SetProperty(ref text, value);
		}

		internal double X
		{
			get => x;
			set => SetProperty(ref x, value);
		}

		internal double Y
		{
			get => y;
			set => SetProperty(ref y, value);
		}
	}
}
