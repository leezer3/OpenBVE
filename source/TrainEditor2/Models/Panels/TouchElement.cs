using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using OpenBveApi.Interface;
using Prism.Mvvm;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Models.Panels
{
	internal class TouchElement : BindableBase, ICloneable
	{
		internal class SoundEntry : BindableBase, ICloneable
		{
			private int index;

			internal int Index
			{
				get
				{
					return index;
				}
				set
				{
					SetProperty(ref index, value);
				}
			}

			internal SoundEntry()
			{
				Index = -1;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		internal class CommandEntry : BindableBase, ICloneable
		{
			private Translations.CommandInfo info;
			private int option;

			internal Translations.CommandInfo Info
			{
				get
				{
					return info;
				}
				set
				{
					SetProperty(ref info, value);
				}
			}

			internal int Option
			{
				get
				{
					return option;
				}
				set
				{
					SetProperty(ref option, value);
				}
			}

			internal CommandEntry()
			{
				Info = Translations.CommandInfos.TryGetInfo(Translations.Command.None);
				Option = 0;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		private readonly CultureInfo culture;

		private double locationX;
		private double locationY;
		private double sizeX;
		private double sizeY;
		private int jumpScreen;
		private int layer;

		private TreeViewItemModel selectedTreeItem;

		private ListViewItemModel selectedListItem;

		internal ObservableCollection<SoundEntry> SoundEntries;
		internal ObservableCollection<CommandEntry> CommandEntries;

		internal ObservableCollection<TreeViewItemModel> TreeItems;

		internal ObservableCollection<ListViewColumnHeaderModel> ListColumns;
		internal ObservableCollection<ListViewItemModel> ListItems;

		internal double LocationX
		{
			get
			{
				return locationX;
			}
			set
			{
				SetProperty(ref locationX, value);
			}
		}

		internal double LocationY
		{
			get
			{
				return locationY;
			}
			set
			{
				SetProperty(ref locationY, value);
			}
		}

		internal double SizeX
		{
			get
			{
				return sizeX;
			}
			set
			{
				SetProperty(ref sizeX, value);
			}
		}

		internal double SizeY
		{
			get
			{
				return sizeY;
			}
			set
			{
				SetProperty(ref sizeY, value);
			}
		}

		internal int JumpScreen
		{
			get
			{
				return jumpScreen;
			}
			set
			{
				SetProperty(ref jumpScreen, value);
			}
		}

		internal int Layer
		{
			get
			{
				return layer;
			}
			set
			{
				SetProperty(ref layer, value);
			}
		}

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

		internal TouchElement(Screen screen)
		{
			culture = CultureInfo.InvariantCulture;

			LocationX = 0.0;
			LocationY = 0.0;
			SizeX = 0.0;
			SizeY = 0.0;
			JumpScreen = screen.Number;
			SoundEntries = new ObservableCollection<SoundEntry>();
			CommandEntries = new ObservableCollection<CommandEntry>();
			Layer = 0;

			TreeItems = new ObservableCollection<TreeViewItemModel>();

			ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			ListItems = new ObservableCollection<ListViewItemModel>();

			CreateTreeItem();
			SelectedTreeItem = null;
		}

		public object Clone()
		{
			TouchElement touch = (TouchElement)MemberwiseClone();
			touch.SoundEntries = new ObservableCollection<SoundEntry>(SoundEntries.Select(x => (SoundEntry)x.Clone()));
			touch.CommandEntries = new ObservableCollection<CommandEntry>(CommandEntries.Select(x => (CommandEntry)x.Clone()));

			touch.ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			touch.ListItems = new ObservableCollection<ListViewItemModel>();

			touch.CreateTreeItem();
			touch.SelectedTreeItem = null;

			return touch;
		}

		internal void CreateTreeItem()
		{
			TreeItems.Clear();
			TreeViewItemModel treeItem = new TreeViewItemModel(null) { Title = "TouchElement" };
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Sounds" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Commands" });
			TreeItems.Add(treeItem);
		}

		internal void CreateListColumns()
		{
			ListColumns.Clear();

			if (SelectedTreeItem == TreeItems[0].Children[0])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Index" });
			}

			if (SelectedTreeItem == TreeItems[0].Children[1])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Name" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Option" });
			}
		}

		internal void CreateListItems()
		{
			ListItems.Clear();

			if (SelectedTreeItem == TreeItems[0].Children[0])
			{
				foreach (SoundEntry entry in SoundEntries)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel> { new ListViewSubItemModel() }, Tag = entry };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (SelectedTreeItem == TreeItems[0].Children[1])
			{
				foreach (CommandEntry entry in CommandEntries)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = entry };

					for (int i = 0; i < 2; i++)
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
			SoundEntry soundEntry = item.Tag as SoundEntry;
			CommandEntry commandEntry = item.Tag as CommandEntry;

			if (soundEntry != null)
			{
				item.SubItems[0].Text = soundEntry.Index.ToString(culture);
			}

			if (commandEntry != null)
			{
				item.SubItems[0].Text = commandEntry.Info.Name;
				item.SubItems[1].Text = commandEntry.Option.ToString(culture);
			}
		}

		internal void AddSoundEntry()
		{
			SoundEntry entry = new SoundEntry();

			SoundEntries.Add(entry);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel> { new ListViewSubItemModel() }, Tag = entry };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddCommandEntry()
		{
			CommandEntry entry = new CommandEntry();

			CommandEntries.Add(entry);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = entry };

			for (int i = 0; i < 2; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopySoundEntry()
		{
			SoundEntry entry = (SoundEntry)((SoundEntry)SelectedListItem.Tag).Clone();

			SoundEntries.Add(entry);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel> { new ListViewSubItemModel() }, Tag = entry };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyCommandEntry()
		{
			CommandEntry entry = (CommandEntry)((CommandEntry)SelectedListItem.Tag).Clone();

			CommandEntries.Add(entry);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = entry };

			for (int i = 0; i < 2; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void RemoveSoundEntry()
		{
			SoundEntry entry = (SoundEntry)SelectedListItem.Tag;

			SoundEntries.Remove(entry);

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveCommandEntry()
		{
			CommandEntry entry = (CommandEntry)SelectedListItem.Tag;

			CommandEntries.Remove(entry);

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}
	}
}
