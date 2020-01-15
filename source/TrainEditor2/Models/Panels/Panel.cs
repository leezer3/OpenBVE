using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Models.Panels
{
	internal class Panel : BindableBase, ICloneable
	{
		private readonly CultureInfo culture;

		private This _this;

		private TreeViewItemModel selectedTreeItem;

		private ListViewItemModel selectedListItem;

		internal This This
		{
			get
			{
				return _this;
			}
			set
			{
				SetProperty(ref _this, value);
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

		internal ObservableCollection<Screen> Screens;
		internal ObservableCollection<PanelElement> PanelElements;

		internal ObservableCollection<TreeViewItemModel> TreeItems;

		internal ObservableCollection<ListViewColumnHeaderModel> ListColumns;
		internal ObservableCollection<ListViewItemModel> ListItems;

		internal Panel()
		{
			culture = CultureInfo.InvariantCulture;

			This = new This();
			Screens = new ObservableCollection<Screen>();
			PanelElements = new ObservableCollection<PanelElement>();

			TreeItems = new ObservableCollection<TreeViewItemModel>();

			ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			ListItems = new ObservableCollection<ListViewItemModel>();

			CreateTreeItem();
		}

		public object Clone()
		{
			Panel panel = (Panel)MemberwiseClone();

			panel.Screens = new ObservableCollection<Screen>(Screens.Select(s => (Screen)s.Clone()));
			panel.PanelElements = new ObservableCollection<PanelElement>(PanelElements.Select(e => (PanelElement)e.Clone()));

			panel.ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			panel.ListItems = new ObservableCollection<ListViewItemModel>();

			panel.CreateTreeItem();
			panel.SelectedTreeItem = null;
			return panel;
		}

		internal void CreateTreeItem()
		{
			TreeItems.Clear();
			TreeViewItemModel treeItem = new TreeViewItemModel(null) { Title = "Panel" };
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "This" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Screens" });
			treeItem.Children.Add(CreatePanelElementsTreeItem(treeItem));
			treeItem.Children[1].Children = new ObservableCollection<TreeViewItemModel>(Screens.Select(x => CreateScreenTreeItem(treeItem.Children[1], x)));
			TreeItems.Add(treeItem);
		}

		private TreeViewItemModel CreateScreenTreeItem(TreeViewItemModel parent, Screen screen)
		{
			TreeViewItemModel newItem = new TreeViewItemModel(parent) { Title = $"Screen{screen.Number}", Tag = screen };
			newItem.Children.Add(CreatePanelElementsTreeItem(newItem));
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "TouchElements" });
			return newItem;
		}

		private TreeViewItemModel CreatePanelElementsTreeItem(TreeViewItemModel parent)
		{
			TreeViewItemModel newItem = new TreeViewItemModel(parent) { Title = "PanelElements" };
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "PilotLamps" });
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "Needles" });
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "DigitalNumbers" });
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "DigitalGauges" });
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "LinearGauges" });
			newItem.Children.Add(new TreeViewItemModel(newItem) { Title = "Timetables" });
			return newItem;
		}

		internal void RenameScreenTreeItem(TreeViewItemModel item)
		{
			item.Title = $"Screen{((Screen)item.Tag).Number}";
		}

		internal void CreateListColumns()
		{
			ListColumns.Clear();

			if (SelectedTreeItem == TreeItems[0].Children[1])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Number" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[0]) || SelectedTreeItem == TreeItems[0].Children[2].Children[0])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DaytimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NighttimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[1]) || SelectedTreeItem == TreeItems[0].Children[2].Children[1])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Radius" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DaytimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NighttimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Color" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Origin" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "InitialAngle" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "LastAngle" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Minimum" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Maximum" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NaturalFreq" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DampingRatio" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Backstop" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Smoothed" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[2]) || SelectedTreeItem == TreeItems[0].Children[2].Children[2])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DaytimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NighttimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Interval" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[3]) || SelectedTreeItem == TreeItems[0].Children[2].Children[3])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Radius" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Color" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "InitialAngle" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "LastAngle" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Minimum" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Maximum" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Step" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[4]) || SelectedTreeItem == TreeItems[0].Children[2].Children[4])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DaytimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NighttimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Minimum" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Maximum" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Direction" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Width" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[5]) || SelectedTreeItem == TreeItems[0].Children[2].Children[5])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Width" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Height" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[1]))
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Size" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "JumpScreen" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}
		}

		internal void CreateListItems()
		{
			ListItems.Clear();

			if (SelectedTreeItem == TreeItems[0].Children[1])
			{
				foreach (Screen screen in Screens)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = screen };

					for (int i = 0; i < 2; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[0]) || SelectedTreeItem == TreeItems[0].Children[2].Children[0])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<PilotLampElement> pilotLamps = screen != null ? screen.PanelElements.OfType<PilotLampElement>() : PanelElements.OfType<PilotLampElement>();

				foreach (PilotLampElement pilotLamp in pilotLamps)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = pilotLamp };

					for (int i = 0; i < 6; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[1]) || SelectedTreeItem == TreeItems[0].Children[2].Children[1])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<NeedleElement> needles = screen != null ? screen.PanelElements.OfType<NeedleElement>() : PanelElements.OfType<NeedleElement>();

				foreach (NeedleElement needle in needles)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = needle };

					for (int i = 0; i < 17; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[2]) || SelectedTreeItem == TreeItems[0].Children[2].Children[2])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<DigitalNumberElement> digitalNumbers = screen != null ? screen.PanelElements.OfType<DigitalNumberElement>() : PanelElements.OfType<DigitalNumberElement>();

				foreach (DigitalNumberElement digitalNumber in digitalNumbers)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = digitalNumber };

					for (int i = 0; i < 7; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[3]) || SelectedTreeItem == TreeItems[0].Children[2].Children[3])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<DigitalGaugeElement> digitalGauges = screen != null ? screen.PanelElements.OfType<DigitalGaugeElement>() : PanelElements.OfType<DigitalGaugeElement>();

				foreach (DigitalGaugeElement digitalGauge in digitalGauges)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = digitalGauge };

					for (int i = 0; i < 10; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[4]) || SelectedTreeItem == TreeItems[0].Children[2].Children[4])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<LinearGaugeElement> linearGauges = screen != null ? screen.PanelElements.OfType<LinearGaugeElement>() : PanelElements.OfType<LinearGaugeElement>();

				foreach (LinearGaugeElement linearGauge in linearGauges)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = linearGauge };

					for (int i = 0; i < 10; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[5]) || SelectedTreeItem == TreeItems[0].Children[2].Children[5])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<TimetableElement> timetables = screen != null ? screen.PanelElements.OfType<TimetableElement>() : PanelElements.OfType<TimetableElement>();

				foreach (TimetableElement timetable in timetables)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = timetable };

					for (int i = 0; i < 5; i++)
					{
						newItem.SubItems.Add(new ListViewSubItemModel());
					}

					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItems[0].Children[1].Children.Any(y => SelectedTreeItem == y.Children[1]))
			{
				Screen screen = (Screen)SelectedTreeItem.Parent.Tag;

				foreach (TouchElement touch in screen.TouchElements)
				{
					ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = touch };

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
			Screen screen = item.Tag as Screen;
			PilotLampElement pilotLamp = item.Tag as PilotLampElement;
			NeedleElement needle = item.Tag as NeedleElement;
			DigitalNumberElement digitalNumber = item.Tag as DigitalNumberElement;
			DigitalGaugeElement digitalGauge = item.Tag as DigitalGaugeElement;
			LinearGaugeElement linearGauge = item.Tag as LinearGaugeElement;
			TimetableElement timetable = item.Tag as TimetableElement;
			TouchElement touch = item.Tag as TouchElement;

			if (screen != null)
			{
				item.SubItems[0].Text = screen.Number.ToString(culture);
				item.SubItems[1].Text = screen.Layer.ToString(culture);
			}

			if (pilotLamp != null)
			{
				item.SubItems[0].Text = pilotLamp.Subject.ToString();
				item.SubItems[1].Text = $"{pilotLamp.LocationX.ToString(culture)}, {pilotLamp.LocationY.ToString(culture)}";
				item.SubItems[2].Text = pilotLamp.DaytimeImage;
				item.SubItems[3].Text = pilotLamp.NighttimeImage;
				item.SubItems[4].Text = pilotLamp.TransparentColor.ToString();
				item.SubItems[5].Text = pilotLamp.Layer.ToString(culture);
			}

			if (needle != null)
			{
				item.SubItems[0].Text = needle.Subject.ToString();
				item.SubItems[1].Text = $"{needle.LocationX.ToString(culture)}, {needle.LocationY.ToString(culture)}";
				item.SubItems[2].Text = needle.DefinedRadius ? needle.Radius.ToString(culture) : string.Empty;
				item.SubItems[3].Text = needle.DaytimeImage;
				item.SubItems[4].Text = needle.NighttimeImage;
				item.SubItems[5].Text = needle.Color.ToString();
				item.SubItems[6].Text = needle.TransparentColor.ToString();
				item.SubItems[7].Text = needle.DefinedOrigin ? $"{needle.OriginX.ToString(culture)}, {needle.OriginY.ToString(culture)}" : string.Empty;
				item.SubItems[8].Text = needle.InitialAngle.ToDegrees().ToString(culture);
				item.SubItems[9].Text = needle.LastAngle.ToDegrees().ToString(culture);
				item.SubItems[10].Text = needle.Minimum.ToString(culture);
				item.SubItems[11].Text = needle.Maximum.ToString(culture);
				item.SubItems[12].Text = needle.DefinedNaturalFreq ? needle.NaturalFreq.ToString(culture) : string.Empty;
				item.SubItems[13].Text = needle.DefinedDampingRatio ? needle.DampingRatio.ToString(culture) : string.Empty;
				item.SubItems[14].Text = needle.Backstop.ToString();
				item.SubItems[15].Text = needle.Smoothed.ToString();
				item.SubItems[16].Text = needle.Layer.ToString(culture);
			}

			if (digitalNumber != null)
			{
				item.SubItems[0].Text = digitalNumber.Subject.ToString();
				item.SubItems[1].Text = $"{digitalNumber.LocationX.ToString(culture)}, {digitalNumber.LocationY.ToString(culture)}";
				item.SubItems[2].Text = digitalNumber.DaytimeImage;
				item.SubItems[3].Text = digitalNumber.NighttimeImage;
				item.SubItems[4].Text = digitalNumber.TransparentColor.ToString();
				item.SubItems[5].Text = digitalNumber.Interval.ToString(culture);
				item.SubItems[6].Text = digitalNumber.Layer.ToString(culture);
			}

			if (digitalGauge != null)
			{
				item.SubItems[0].Text = digitalGauge.Subject.ToString();
				item.SubItems[1].Text = $"{digitalGauge.LocationX.ToString(culture)}, {digitalGauge.LocationY.ToString(culture)}";
				item.SubItems[2].Text = digitalGauge.Radius.ToString(culture);
				item.SubItems[3].Text = digitalGauge.Color.ToString();
				item.SubItems[4].Text = digitalGauge.InitialAngle.ToDegrees().ToString(culture);
				item.SubItems[5].Text = digitalGauge.LastAngle.ToDegrees().ToString(culture);
				item.SubItems[6].Text = digitalGauge.Minimum.ToString(culture);
				item.SubItems[7].Text = digitalGauge.Maximum.ToString(culture);
				item.SubItems[8].Text = digitalGauge.Step.ToString(culture);
				item.SubItems[9].Text = digitalGauge.Layer.ToString(culture);
			}

			if (linearGauge != null)
			{
				item.SubItems[0].Text = linearGauge.Subject.ToString();
				item.SubItems[1].Text = $"{linearGauge.LocationX.ToString(culture)}, {linearGauge.LocationY.ToString(culture)}";
				item.SubItems[2].Text = linearGauge.DaytimeImage;
				item.SubItems[3].Text = linearGauge.NighttimeImage;
				item.SubItems[4].Text = linearGauge.TransparentColor.ToString();
				item.SubItems[5].Text = linearGauge.Minimum.ToString(culture);
				item.SubItems[6].Text = linearGauge.Maximum.ToString(culture);
				item.SubItems[7].Text = $"{linearGauge.DirectionX.ToString(culture)}, {linearGauge.DirectionY.ToString(culture)}";
				item.SubItems[8].Text = linearGauge.Width.ToString(culture);
				item.SubItems[9].Text = linearGauge.Layer.ToString(culture);
			}

			if (timetable != null)
			{
				item.SubItems[0].Text = $"{timetable.LocationX.ToString(culture)}, {timetable.LocationY.ToString(culture)}";
				item.SubItems[1].Text = timetable.Width.ToString(culture);
				item.SubItems[2].Text = timetable.Height.ToString(culture);
				item.SubItems[3].Text = timetable.TransparentColor.ToString();
				item.SubItems[4].Text = timetable.Layer.ToString(culture);
			}

			if (touch != null)
			{
				item.SubItems[0].Text = $"{touch.LocationX.ToString(culture)}, {touch.LocationY.ToString(culture)}";
				item.SubItems[1].Text = $"{touch.SizeX.ToString(culture)}, {touch.SizeY.ToString(culture)}";
				item.SubItems[2].Text = touch.JumpScreen.ToString(culture);
				item.SubItems[3].Text = touch.Layer.ToString(culture);
			}
		}

		internal void UpScreen()
		{
			int index = Screens.IndexOf((Screen)SelectedListItem.Tag);
			Screens.Move(index, index - 1);

			TreeItems[0].Children[1].Children.Move(index, index - 1);
			ListItems.Move(index, index - 1);
		}

		internal void UpPanelElement<T>() where T : PanelElement
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

			PanelElement currentPanelElement = (PanelElement)SelectedListItem.Tag;
			int currentIndex = screen?.PanelElements.IndexOf(currentPanelElement) ?? PanelElements.IndexOf(currentPanelElement);
			int currentListIndex = ListItems.IndexOf(SelectedListItem);

			PanelElement prevPanelElement = screen != null ? screen.PanelElements.Take(currentIndex).OfType<T>().Last() : PanelElements.Take(currentIndex).OfType<T>().Last();
			int prevIndex = screen?.PanelElements.IndexOf(prevPanelElement) ?? PanelElements.IndexOf(prevPanelElement);

			if (screen != null)
			{
				screen.PanelElements.Move(currentIndex, prevIndex);
			}
			else
			{
				PanelElements.Move(currentIndex, prevIndex);
			}

			ListItems.Move(currentListIndex, currentListIndex - 1);
		}

		internal void UpTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			int index = screen.TouchElements.IndexOf((TouchElement)SelectedListItem.Tag);

			ListItems.Move(index, index - 1);
		}

		internal void DownScreen()
		{
			int index = Screens.IndexOf((Screen)SelectedListItem.Tag);
			Screens.Move(index, index + 1);

			TreeItems[0].Children[1].Children.Move(index, index + 1);
			ListItems.Move(index, index + 1);
		}

		internal void DownPanelElement<T>() where T : PanelElement
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

			PanelElement currentPanelElement = (PanelElement)SelectedListItem.Tag;
			int currentIndex = screen?.PanelElements.IndexOf(currentPanelElement) ?? PanelElements.IndexOf(currentPanelElement);
			int currentListIndex = ListItems.IndexOf(SelectedListItem);

			PanelElement nextPanelElement = screen != null ? screen.PanelElements.Skip(currentIndex + 1).OfType<T>().First() : PanelElements.Skip(currentIndex + 1).OfType<T>().First();
			int nextIndex = screen?.PanelElements.IndexOf(nextPanelElement) ?? PanelElements.IndexOf(nextPanelElement);

			if (screen != null)
			{
				screen.PanelElements.Move(currentIndex, nextIndex);
			}
			else
			{
				PanelElements.Move(currentIndex, nextIndex);
			}

			ListItems.Move(currentListIndex, currentListIndex + 1);
		}

		internal void DownTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			int index = screen.TouchElements.IndexOf((TouchElement)SelectedListItem.Tag);

			ListItems.Move(index, index + 1);
		}

		internal void AddScreen()
		{
			Screen screen = new Screen();

			if (Screens.Any())
			{
				screen.Number = Screens.Last().Number + 1;
			}

			Screens.Add(screen);

			TreeItems[0].Children[1].Children.Add(CreateScreenTreeItem(TreeItems[0].Children[1], Screens.Last()));

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = screen };

			for (int i = 0; i < 2; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddPanelElement<T>(int numberOfColumns) where T : PanelElement, new()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			PanelElement panelElement = new T();

			if (screen != null)
			{
				screen.PanelElements.Add(panelElement);
			}
			else
			{
				PanelElements.Add(panelElement);
			}

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = panelElement };

			for (int i = 0; i < numberOfColumns; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			TouchElement touch = new TouchElement(screen);

			screen.TouchElements.Add(touch);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = touch };

			for (int i = 0; i < 4; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyScreen()
		{
			Screen screen = (Screen)((Screen)SelectedListItem.Tag).Clone();

			if (Screens.Any())
			{
				screen.Number = Screens.Last().Number + 1;
			}

			Screens.Add(screen);

			TreeItems[0].Children[1].Children.Add(CreateScreenTreeItem(TreeItems[0].Children[1], Screens.Last()));

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = screen };

			for (int i = 0; i < 2; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyPanelElement(int numberOfColumns)
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			PanelElement panelElement = (PanelElement)((PanelElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(panelElement);
			}
			else
			{
				PanelElements.Add(panelElement);
			}

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = panelElement };

			for (int i = 0; i < numberOfColumns; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			TouchElement touch = (TouchElement)((TouchElement)SelectedListItem.Tag).Clone();

			screen.TouchElements.Add(touch);

			ListViewItemModel newItem = new ListViewItemModel { SubItems = new ObservableCollection<ListViewSubItemModel>(), Tag = touch };

			for (int i = 0; i < 4; i++)
			{
				newItem.SubItems.Add(new ListViewSubItemModel());
			}

			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void RemoveScreen()
		{
			Screen screen = (Screen)SelectedListItem.Tag;

			Screens.Remove(screen);

			TreeItems[0].Children[1].Children.RemoveAll(x => x.Tag == screen);

			SelectedListItem = null;
		}

		internal void RemovePanelElement()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			PanelElement panelElement = (PanelElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(panelElement);
			}
			else
			{
				PanelElements.Remove(panelElement);
			}

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			TouchElement touch = (TouchElement)SelectedListItem.Tag;

			screen.TouchElements.Remove(touch);

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}
	}
}
