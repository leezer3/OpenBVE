using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;
using TrainEditor2.Models.Panels;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.Models
{
	internal class App : BindableBase
	{
		private readonly CultureInfo culture;

		private string saveLocation;
		private string currentLanguageCode;

		private Train train;
		private Panel panel;

		private MessageBox messageBox;

		private TreeViewItemModel item;
		private TreeViewItemModel selectedItem;

		internal string SaveLocation
		{
			get
			{
				return saveLocation;
			}
			set
			{
				SetProperty(ref saveLocation, value);
			}
		}

		internal string CurrentLanguageCode
		{
			get
			{
				return currentLanguageCode;
			}
			set
			{
				SetProperty(ref currentLanguageCode, value);
			}
		}

		internal Train Train
		{
			get
			{
				return train;
			}
			set
			{
				SetProperty(ref train, value);
			}
		}

		internal Panel Panel
		{
			get
			{
				return panel;
			}
			set
			{
				SetProperty(ref panel, value);
			}
		}

		internal MessageBox MessageBox
		{
			get
			{
				return messageBox;
			}
			set
			{
				SetProperty(ref messageBox, value);
			}
		}

		internal TreeViewItemModel Item
		{
			get
			{
				return item;
			}
			set
			{
				SetProperty(ref item, value);
			}
		}

		internal TreeViewItemModel SelectedItem
		{
			get
			{
				return selectedItem;
			}
			set
			{
				SetProperty(ref selectedItem, value);
			}
		}

		internal App()
		{
			culture = CultureInfo.InvariantCulture;

			CurrentLanguageCode = "en-US";

			MessageBox = new MessageBox();

			CreateNewFile();
		}

		internal void CreateItem()
		{
			item = new TreeViewItemModel { Title = Utilities.GetInterfaceString("tree_cars", "train") };
			item.Children.Add(new TreeViewItemModel { Title = Utilities.GetInterfaceString("tree_cars", "general") });
			item.Children.Add(new TreeViewItemModel { Title = Utilities.GetInterfaceString("tree_cars", "cars") });
			item.Children.Add(new TreeViewItemModel { Title = Utilities.GetInterfaceString("tree_cars", "couplers") });
			item.Children[1].Children = new ObservableCollection<TreeViewItemModel>(Train.Cars.Select((x, i) => new TreeViewItemModel { Title = i.ToString(culture), Tag = x }));
			item.Children[2].Children = new ObservableCollection<TreeViewItemModel>(Train.Couplers.Select((x, i) => new TreeViewItemModel { Title = i.ToString(culture), Tag = x }));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Item)));
		}

		internal void CreateNewFile()
		{
			if (Train != null || Panel != null)
			{
				MessageBox = new MessageBox
				{
					Title = Utilities.GetInterfaceString("menu", "file", "new"),
					Icon = BaseDialog.DialogIcon.Question,
					Button = BaseDialog.DialogButton.YesNo,
					Text = Utilities.GetInterfaceString("menu", "message", "new"),
					IsOpen = true
				};

				if (MessageBox.DialogResult != true)
				{
					return;
				}

				//SaveFile();
			}

			SaveLocation = string.Empty;

			train = new Train();
			train.Cars.Add(new MotorCar());
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(Train)));

			Panel = new Panel();

			CreateItem();

			Interface.LogMessages.Clear();
		}

		private void RenameTreeViewItem(ObservableCollection<TreeViewItemModel> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i].Title = i.ToString(culture);
			}
		}

		internal void UpCar()
		{
			int index = Train.Cars.IndexOf((Car)SelectedItem.Tag);
			Train.Cars.Move(index, index - 1);

			Item.Children[1].Children.Move(index, index - 1);
			RenameTreeViewItem(Item.Children[1].Children);
		}

		internal void DownCar()
		{
			int index = Train.Cars.IndexOf((Car)SelectedItem.Tag);
			Train.Cars.Move(index, index + 1);

			Item.Children[1].Children.Move(index, index + 1);
			RenameTreeViewItem(Item.Children[1].Children);
		}

		internal void AddCar()
		{
			Train.Cars.Add(new TrailerCar());
			Train.Couplers.Add(new Coupler());

			Train.ApplyPowerNotchesToCar();
			Train.ApplyBrakeNotchesToCar();
			Train.ApplyLocoBrakeNotchesToCar();

			Item.Children[1].Children.Add(new TreeViewItemModel { Title = (Train.Cars.Count - 1).ToString(culture), Tag = Train.Cars.Last() });
			Item.Children[2].Children.Add(new TreeViewItemModel { Title = (Train.Couplers.Count - 1).ToString(culture), Tag = Train.Couplers.Last() });
			SelectedItem = Item.Children[1].Children.Last();
		}

		internal void RemoveCar()
		{
			int index = Train.Cars.IndexOf((Car)SelectedItem.Tag);
			Train.Cars.RemoveAt(index);
			Train.Couplers.RemoveAt(index == 0 ? 0 : index - 1);

			Item.Children[1].Children.RemoveAt(index);
			Item.Children[2].Children.RemoveAt(index == 0 ? 0 : index - 1);
			RenameTreeViewItem(Item.Children[1].Children);
			RenameTreeViewItem(Item.Children[2].Children);

			SelectedItem = null;
		}

		internal void CopyCar()
		{
			Train.Cars.Add((Car)((Car)SelectedItem.Tag).Clone());
			Train.Couplers.Add(new Coupler());

			Item.Children[1].Children.Add(new TreeViewItemModel { Title = (Train.Cars.Count - 1).ToString(culture), Tag = Train.Cars.Last() });
			Item.Children[2].Children.Add(new TreeViewItemModel { Title = (Train.Couplers.Count - 1).ToString(culture), Tag = Train.Couplers.Last() });
			SelectedItem = Item.Children[1].Children.Last();
		}

		internal void UpCoupler()
		{
			int index = Train.Couplers.IndexOf((Coupler)SelectedItem.Tag);
			Train.Couplers.Move(index, index - 1);

			Item.Children[2].Children.Move(index, index - 1);
			RenameTreeViewItem(Item.Children[2].Children);
		}

		internal void DownCoupler()
		{
			int index = Train.Couplers.IndexOf((Coupler)SelectedItem.Tag);
			Train.Couplers.Move(index, index + 1);

			Item.Children[2].Children.Move(index, index + 1);
			RenameTreeViewItem(Item.Children[2].Children);
		}

		internal void ChangeCarClass(int carIndex)
		{
			MotorCar motorCar = Train.Cars[carIndex] as MotorCar;
			TrailerCar trailerCar = Train.Cars[carIndex] as TrailerCar;

			if (motorCar != null)
			{
				Train.Cars[carIndex] = new TrailerCar(motorCar);
			}

			if (trailerCar != null)
			{
				Train.Cars[carIndex] = new MotorCar(trailerCar);

				Train.ApplyPowerNotchesToCar();
			}

			Item.Children[1].Children[carIndex].Tag = Train.Cars[carIndex];
		}
	}
}
