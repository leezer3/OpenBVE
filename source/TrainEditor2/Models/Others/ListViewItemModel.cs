using System.Collections.ObjectModel;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Others
{
	internal class ListViewItemModel : BindableBase
	{
		private object tag;
		private int imageIndex;

		internal ObservableCollection<string> Texts;

		internal object Tag
		{
			get => tag;
			set => SetProperty(ref tag, value);
		}

		internal int ImageIndex
		{
			get => imageIndex;
			set => SetProperty(ref imageIndex, value);
		}

		internal ListViewItemModel()
		{
			Texts = new ObservableCollection<string>();
			ImageIndex = -1;
		}
	}
}
