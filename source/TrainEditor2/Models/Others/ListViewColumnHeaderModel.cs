using Prism.Mvvm;

namespace TrainEditor2.Models.Others
{
	internal class ListViewColumnHeaderModel : BindableBase
	{
		private string text;

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
