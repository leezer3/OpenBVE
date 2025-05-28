namespace TrainEditor2.Models.Dialogs
{
	internal class MessageBox : BaseDialog
	{
		private DialogIcon icon;
		private DialogButton button;
		private string text;

		internal DialogIcon Icon
		{
			get => icon;
			set => SetProperty(ref icon, value);
		}

		internal DialogButton Button
		{
			get => button;
			set => SetProperty(ref button, value);
		}

		internal string Text
		{
			get => text;
			set => SetProperty(ref text, value);
		}
	}
}
