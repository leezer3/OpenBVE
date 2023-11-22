namespace TrainEditor2.Models.Dialogs
{
	internal class OpenFolderDialog : BaseDialog
	{
		private string folder;

		internal string Folder
		{
			get
			{
				return folder;
			}
			set
			{
				SetProperty(ref folder, value);
			}
		}
	}
}
