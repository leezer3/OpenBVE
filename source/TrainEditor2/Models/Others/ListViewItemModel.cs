using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace TrainEditor2.Models.Others
{
	internal class ListViewItemModel : BindableBase
	{
		private object tag;
		private int imageIndex;

		internal ObservableCollection<ListViewSubItemModel> SubItems;

		internal object Tag
		{
			get
			{
				return tag;
			}
			set
			{
				SetProperty(ref tag, value);
			}
		}

		internal int ImageIndex
		{
			get
			{
				return imageIndex;
			}
			set
			{
				SetProperty(ref imageIndex, value);
			}
		}

		internal ListViewItemModel()
		{
			SubItems = new ObservableCollection<ListViewSubItemModel>();
			ImageIndex = -1;
		}
	}
}
