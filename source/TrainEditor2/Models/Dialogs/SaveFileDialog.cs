namespace TrainEditor2.Models.Dialogs
{
	internal class SaveFileDialog : BaseDialog
	{
		private string filter;
		private bool overwritePrompt;
		private string fileName;

		internal string Filter
		{
			get
			{
				return filter;
			}
			set
			{
				SetProperty(ref filter, value);
			}
		}

		internal bool OverwritePrompt
		{
			get
			{
				return overwritePrompt;
			}
			set
			{
				SetProperty(ref overwritePrompt, value);
			}
		}

		internal string FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				SetProperty(ref fileName, value);
			}
		}
	}
}
