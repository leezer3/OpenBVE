using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Models.Sounds
{
	internal class Sound : BindableBase, ICloneable
	{
		private readonly CultureInfo culture;

		private TreeViewItemModel selectedTreeItem;

		private ListViewItemModel selectedListItem;

		internal TreeViewItemModel SelectedTreeItem
		{
			get
			{
				return selectedTreeItem;
			}
			set
			{
				SetProperty(ref selectedTreeItem, value);
			}
		}

		internal ListViewItemModel SelectedListItem
		{
			get
			{
				return selectedListItem;
			}
			set
			{
				SetProperty(ref selectedListItem, value);
			}
		}

		internal ObservableCollection<SoundElement> SoundElements;

		internal ObservableCollection<TreeViewItemModel> TreeItems;

		internal ObservableCollection<ListViewColumnHeaderModel> ListColumns;
		internal ObservableCollection<ListViewItemModel> ListItems;

		internal Sound()
		{
			culture = CultureInfo.InvariantCulture;

			SoundElements = new ObservableCollection<SoundElement>();

			TreeItems = new ObservableCollection<TreeViewItemModel>();

			ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			ListItems = new ObservableCollection<ListViewItemModel>();

			CreateTreeItem();
		}

		public object Clone()
		{
			Sound sound = (Sound)MemberwiseClone();

			sound.SoundElements = new ObservableCollection<SoundElement>(SoundElements.Select(x => (SoundElement)x.Clone()));

			sound.ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			sound.ListItems = new ObservableCollection<ListViewItemModel>();

			sound.CreateTreeItem();
			sound.SelectedTreeItem = null;
			return sound;
		}

		internal void CreateTreeItem()
		{
			TreeItems.Clear();
			TreeViewItemModel treeItem = new TreeViewItemModel(null) { Title = "Sound" };
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Run" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Flange" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Motor" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "FrontSwitch" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "RearSwitch" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Brake" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Compressor" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Suspension" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "PrimaryHorn" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "SecondaryHorn" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "MusicHorn" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Door" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Ats" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Buzzer" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "PilotLamp" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "BrakeHandle" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "MasterController" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Reverser" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Breaker" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "RequestStop" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Touch" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Others" });
			TreeItems.Add(treeItem);
		}

		internal void CreateListColumns()
		{
			ListColumns.Clear();

			if (TreeItems[0].Children.Contains(SelectedTreeItem))
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Key" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "FilePath" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Position" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Radius" });
			}
		}

		internal void CreateListItems()
		{
			ListItems.Clear();

			IEnumerable<SoundElement> elements = null;

			if (SelectedTreeItem == TreeItems[0].Children[0])
			{
				elements = SoundElements.OfType<RunElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[1])
			{
				elements = SoundElements.OfType<FlangeElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[2])
			{
				elements = SoundElements.OfType<MotorElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[3])
			{
				elements = SoundElements.OfType<FrontSwitchElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[4])
			{
				elements = SoundElements.OfType<RearSwitchElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[5])
			{
				elements = SoundElements.OfType<BrakeElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[6])
			{
				elements = SoundElements.OfType<CompressorElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[7])
			{
				elements = SoundElements.OfType<SuspensionElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[8])
			{
				elements = SoundElements.OfType<PrimaryHornElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[9])
			{
				elements = SoundElements.OfType<SecondaryHornElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[10])
			{
				elements = SoundElements.OfType<MusicHornElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[11])
			{
				elements = SoundElements.OfType<DoorElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[12])
			{
				elements = SoundElements.OfType<AtsElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[13])
			{
				elements = SoundElements.OfType<BuzzerElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[14])
			{
				elements = SoundElements.OfType<PilotLampElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[15])
			{
				elements = SoundElements.OfType<BrakeHandleElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[16])
			{
				elements = SoundElements.OfType<MasterControllerElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[17])
			{
				elements = SoundElements.OfType<ReverserElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[18])
			{
				elements = SoundElements.OfType<BreakerElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[19])
			{
				elements = SoundElements.OfType<RequestStopElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[20])
			{
				elements = SoundElements.OfType<TouchElement>();
			}

			if (SelectedTreeItem == TreeItems[0].Children[21])
			{
				elements = SoundElements.OfType<OthersElement>();
			}

			if (elements != null)
			{
				foreach (SoundElement element in elements)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = element };

					for (int i = 0; i < 4; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}
		}

		internal void UpdateListItem(ListViewItemModel item)
		{
			SoundElement element = (SoundElement)item.Tag;
			Enum key = element.Key as Enum;

			item.SubItems[0].Text = key != null ? key.GetStringValues().First() : element.Key.ToString();
			item.SubItems[1].Text = element.FilePath;
			item.SubItems[2].Text = element.DefinedPosition ? $"{element.PositionX.ToString(culture)}, {element.PositionY.ToString(culture)}, {element.PositionZ.ToString(culture)}" : string.Empty;
			item.SubItems[3].Text = element.DefinedRadius ? element.Radius.ToString(culture) : string.Empty;
		}

		internal void UpElement<T>() where T : SoundElement
		{
			SoundElement currentElement = (SoundElement)SelectedListItem.Tag;
			int currentIndex = SoundElements.IndexOf(currentElement);
			int currentListIndex = ListItems.IndexOf(SelectedListItem);

			SoundElement prevElement = SoundElements.Take(currentIndex).OfType<T>().Last();
			int prevIndex = SoundElements.IndexOf(prevElement);

			SoundElements.Move(currentIndex, prevIndex);
			ListItems.Move(currentListIndex, currentListIndex - 1);
		}

		internal void DownElement<T>() where T : SoundElement
		{
			SoundElement currentElement = (SoundElement)SelectedListItem.Tag;
			int currentIndex = SoundElements.IndexOf(currentElement);
			int currentListIndex = ListItems.IndexOf(SelectedListItem);

			SoundElement nextElement = SoundElements.Skip(currentIndex + 1).OfType<T>().First();
			int nextIndex = SoundElements.IndexOf(nextElement);

			SoundElements.Move(currentIndex, nextIndex);
			ListItems.Move(currentListIndex, currentListIndex + 1);
		}

		internal void AddElement<T>() where T : SoundElement<int>, new()
		{
			T element = new T();

			if (SoundElements.OfType<T>().Any())
			{
				element.Key = SoundElements.OfType<T>().OrderBy(x => x.Key).Last().Key + 1;
			}

			SoundElements.Add(element);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = element };

			for (int i = 0; i < 4; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddElement<T, U>() where T : SoundElement<U>, new()
		{
			T element = new T
			{
				Key = Enum.GetValues(typeof(U)).OfType<U>().Except(SoundElements.OfType<T>().Select(x => x.Key)).First()
			};

			SoundElements.Add(element);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = element };

			for (int i = 0; i < 4; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void RemoveElement()
		{
			SoundElement element = (SoundElement)SelectedListItem.Tag;

			SoundElements.Remove(element);

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}
	}
}
