namespace TrainEditor2.Models.Dialogs
{
	internal class SaveFileDialog : BaseDialog
	{
		private string filter;
		private bool overwritePrompt;
		private string fileName;

		internal string Filter
		{
			get => filter;
			set => SetProperty(ref filter, value);
		}

		internal bool OverwritePrompt
		{
			get => overwritePrompt;
			set => SetProperty(ref overwritePrompt, value);
		}

		internal string FileName
		{
			get => fileName;
			set => SetProperty(ref fileName, value);
		}
	}
}
