namespace TrainEditor2.Models.Dialogs
{
	internal class OpenFileDialog : BaseDialog
	{
		private string filter;
		private bool checkFileExists;
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

		internal bool CheckFileExists
		{
			get
			{
				return checkFileExists;
			}
			set
			{
				SetProperty(ref checkFileExists, value);
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
