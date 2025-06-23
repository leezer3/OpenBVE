using System.Collections.ObjectModel;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Others
{
	internal class TreeViewItemModel : BindableBase
	{
		private string title;
		private object tag;
		private object secondaryTag;

		internal TreeViewItemModel Parent
		{
			get;
		}

		internal string Title
		{
			get => title;
			set => SetProperty(ref title, value);
		}

		internal object Tag
		{
			get => tag;
			set => SetProperty(ref tag, value);
		}

		internal object SecondaryTag
		{
			get => secondaryTag;
			set => SetProperty(ref secondaryTag, value);
		}



		internal ObservableCollection<TreeViewItemModel> Children;

		internal TreeViewItemModel(TreeViewItemModel parent)
		{
			Parent = parent;
			Children = new ObservableCollection<TreeViewItemModel>();
		}
	}
}
