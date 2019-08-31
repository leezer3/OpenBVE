using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using OpenBveApi.Math;
using Prism.Mvvm;
using SoundManager;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Simulation.TrainManager;

namespace TrainEditor2.Models.Sounds
{
	internal class Sound : BindableBase, ICloneable
	{
		private readonly CultureInfo culture;

		private TreeViewItemModel treeItem;
		private TreeViewItemModel selectedTreeItem;

		private ListViewItemModel selectedListItem;

		internal TreeViewItemModel TreeItem
		{
			get
			{
				return treeItem;
			}
			set
			{
				SetProperty(ref treeItem, value);
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

		internal ObservableCollection<SoundElement> SoundElements;

		internal ObservableCollection<ListViewColumnHeaderModel> ListColumns;
		internal ObservableCollection<ListViewItemModel> ListItems;

		internal Sound()
		{
			culture = CultureInfo.InvariantCulture;

			SoundElements = new ObservableCollection<SoundElement>();

			ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			ListItems = new ObservableCollection<ListViewItemModel>();

			CreateTreeItem();
			SelectedTreeItem = TreeItem;
		}

		public object Clone()
		{
			Sound sound = (Sound)MemberwiseClone();

			sound.SoundElements = new ObservableCollection<SoundElement>(SoundElements.Select(x => (SoundElement)x.Clone()));

			sound.ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			sound.ListItems = new ObservableCollection<ListViewItemModel>();

			sound.CreateTreeItem();
			sound.SelectedTreeItem = sound.TreeItem;

			return sound;
		}

		internal void CreateTreeItem()
		{
			treeItem = new TreeViewItemModel(null) { Title = "Sound" };
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Run" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Flange" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Motor" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "FrontSwitch" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "RearSwitch" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Brake" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Compressor" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Suspension" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "PrimaryHorn" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "SecondaryHorn" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "MusicHorn" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Door" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Ats" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Buzzer" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "PilotLamp" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "BrakeHandle" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "MasterController" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Reverser" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Breaker" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "RequestStop" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Touch" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Others" });
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(TreeItem)));
		}

		internal void CreateListColumns()
		{
			ListColumns.RemoveAll(_ => true);

			if (TreeItem.Children.Contains(SelectedTreeItem))
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Key" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "FilePath" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Position" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Radius" });
			}
		}

		internal void CreateListItems()
		{
			ListItems.RemoveAll(_ => true);

			IEnumerable<SoundElement> elements = null;

			if (SelectedTreeItem == TreeItem.Children[0])
			{
				elements = SoundElements.OfType<RunElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[1])
			{
				elements = SoundElements.OfType<FlangeElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[2])
			{
				elements = SoundElements.OfType<MotorElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[3])
			{
				elements = SoundElements.OfType<FrontSwitchElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[4])
			{
				elements = SoundElements.OfType<RearSwitchElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[5])
			{
				elements = SoundElements.OfType<BrakeElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[6])
			{
				elements = SoundElements.OfType<CompressorElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[7])
			{
				elements = SoundElements.OfType<SuspensionElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[8])
			{
				elements = SoundElements.OfType<PrimaryHornElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[9])
			{
				elements = SoundElements.OfType<SecondaryHornElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[10])
			{
				elements = SoundElements.OfType<MusicHornElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[11])
			{
				elements = SoundElements.OfType<DoorElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[12])
			{
				elements = SoundElements.OfType<AtsElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[13])
			{
				elements = SoundElements.OfType<BuzzerElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[14])
			{
				elements = SoundElements.OfType<PilotLampElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[15])
			{
				elements = SoundElements.OfType<BrakeHandleElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[16])
			{
				elements = SoundElements.OfType<MasterControllerElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[17])
			{
				elements = SoundElements.OfType<ReverserElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[18])
			{
				elements = SoundElements.OfType<BreakerElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[19])
			{
				elements = SoundElements.OfType<RequestStopElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[20])
			{
				elements = SoundElements.OfType<TouchElement>();
			}

			if (SelectedTreeItem == TreeItem.Children[21])
			{
				elements = SoundElements.OfType<OthersElement>();
			}

			if (elements != null)
			{
				foreach (SoundElement element in elements)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[4]), Tag = element };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}
		}

		internal void UpdateListItem(ListViewItemModel item)
		{
			SoundElement element = (SoundElement)item.Tag;
			Enum key = element.Key as Enum;

			item.Texts[0] = key != null ? key.GetStringValues().First() : element.Key.ToString();
			item.Texts[1] = element.FilePath;
			item.Texts[2] = element.DefinedPosition ? $"{element.PositionX.ToString(culture)}, {element.PositionY.ToString(culture)}, {element.PositionZ.ToString(culture)}" : string.Empty;
			item.Texts[3] = element.DefinedRadius ? element.Radius.ToString(culture) : string.Empty;
		}

		internal void AddElement<T>() where T : SoundElement<int>, new()
		{
			T t = new T();

			if (SoundElements.OfType<T>().Any())
			{
				t.Key = SoundElements.OfType<T>().Last().Key + 1;
			}

			SoundElements.Add(t);

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[4]), Tag = t };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddElement<T, U>() where T : SoundElement<U>, new()
		{
			T t = new T
			{
				Key = Enum.GetValues(typeof(U)).OfType<U>().Except(SoundElements.OfType<T>().Select(x => x.Key)).First()
			};

			SoundElements.Add(t);

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[4]), Tag = t };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void RemoveElement<T>() where T : SoundElement
		{
			T t = (T)SelectedListItem.Tag;

			SoundElements.Remove(t);

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void ApplySounds(TrainManager.Car car)
		{
			car.InitializeCarSounds();

			//Default sound positions and radii
			double mediumRadius = 10.0;

			//3D center of the car
			Vector3 center = Vector3.Zero;

			// run sound
			foreach (var element in SoundElements.OfType<RunElement>())
			{
				int n = car.Sounds.Run.Length;

				if (element.Key >= n)
				{
					Array.Resize(ref car.Sounds.Run, element.Key + 1);

					for (int h = n; h < element.Key; h++)
					{
						car.Sounds.Run[h] = new CarSound();
					}
				}

				car.Sounds.Run[element.Key] = new CarSound(Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius), center);
			}

			car.Sounds.RunVolume = new double[car.Sounds.Run.Length];


			// motor sound
			car.Sounds.Motor.Position = center;

			for (int i = 0; i < car.Sounds.Motor.Tables.Length; i++)
			{
				car.Sounds.Motor.Tables[i].Buffer = null;
				car.Sounds.Motor.Tables[i].Source = null;

				for (int j = 0; j < car.Sounds.Motor.Tables[i].Entries.Length; j++)
				{
					MotorElement element = SoundElements.OfType<MotorElement>().FirstOrDefault(x => x.Key == car.Sounds.Motor.Tables[i].Entries[j].SoundIndex);

					if (element !=null)
					{
						car.Sounds.Motor.Tables[i].Entries[j].Buffer = Program.SoundApi.RegisterBuffer(element.FilePath, mediumRadius);
					}
				}
			}
		}
	}
}
