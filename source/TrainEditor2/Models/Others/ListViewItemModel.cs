using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace TrainEditor2.Models.Others
{
	internal class ListViewItemModel : BindableBase
	{
		private bool _checked;
		private int imageIndex;
		private object tag;

		internal ObservableCollection<ListViewSubItemModel> SubItems;

		internal bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				SetProperty(ref _checked, value);
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

		internal ListViewItemModel()
		{
			SubItems = new ObservableCollection<ListViewSubItemModel>();
			ImageIndex = -1;
		}
	}
}
