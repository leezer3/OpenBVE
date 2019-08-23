namespace TrainEditor2.Models.Dialogs
{
	internal class MessageBox : BaseDialog
	{
		private DialogIcon icon;
		private DialogButton button;
		private string text;

		internal DialogIcon Icon
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

		internal DialogButton Button
		{
			get
			{
				return button;
			}
			set
			{
				SetProperty(ref button, value);
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
	}
}
