using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace TrainEditor2.Models.Others
{
	internal class ListViewItemModel : BindableBase
	{
		private object tag;

		internal ObservableCollection<string> Texts;

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
			Texts = new ObservableCollection<string>();
		}
	}
}
