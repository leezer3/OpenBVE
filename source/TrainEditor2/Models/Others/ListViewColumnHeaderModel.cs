using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Others
{
	internal class ListViewColumnHeaderModel : BindableBase
	{
		private string text;

		internal string Text
		{
			get => text;
			set => SetProperty(ref text, value);
		}
	}
}
