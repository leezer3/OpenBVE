using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace TrainEditor2.Models.Others
{
	internal class TreeViewItemModel : BindableBase
	{
		private string title;
		private object tag;

		internal TreeViewItemModel Parent
		{
			get;
		}

		internal string Title
		{
			get
			{
				return title;
			}
			set
			{
				SetProperty(ref title, value);
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

		internal ObservableCollection<TreeViewItemModel> Children;

		internal TreeViewItemModel(TreeViewItemModel parent)
		{
			Parent = parent;
			Children = new ObservableCollection<TreeViewItemModel>();
		}
	}
}
