using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

		private TreeViewItemModel treeItem;
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

		internal ObservableCollection<Screen> Screens;
		internal ObservableCollection<PanelElement> PanelElements;

		internal ObservableCollection<ListViewColumnHeaderModel> ListColumns;
		internal ObservableCollection<ListViewItemModel> ListItems;

		internal Panel()
		{
			culture = CultureInfo.InvariantCulture;

			This = new This();
			Screens = new ObservableCollection<Screen>();
			PanelElements = new ObservableCollection<PanelElement>();

			ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			ListItems = new ObservableCollection<ListViewItemModel>();

			CreateTreeItem();
			SelectedTreeItem = TreeItem;
		}

		public object Clone()
		{
			Panel panel = (Panel)MemberwiseClone();

			panel.Screens = new ObservableCollection<Screen>(Screens.Select(s => (Screen)s.Clone()));
			panel.PanelElements = new ObservableCollection<PanelElement>(PanelElements.Select(e => (PanelElement)e.Clone()));

			panel.ListColumns = new ObservableCollection<ListViewColumnHeaderModel>();
			panel.ListItems = new ObservableCollection<ListViewItemModel>();

			panel.CreateTreeItem();
			panel.SelectedTreeItem = panel.TreeItem;
			return panel;
		}

		internal void CreateTreeItem()
		{
			treeItem = new TreeViewItemModel(null) { Title = "Panel" };
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "This" });
			treeItem.Children.Add(new TreeViewItemModel(TreeItem) { Title = "Screens" });
			treeItem.Children.Add(CreatePanelElementsTreeItem(TreeItem));
			treeItem.Children[1].Children = new ObservableCollection<TreeViewItemModel>(Screens.Select(x => CreateScreenTreeItem(treeItem.Children[1], x)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(TreeItem)));
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

			if (SelectedTreeItem == TreeItem.Children[1])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Number" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[0]) || SelectedTreeItem == TreeItem.Children[2].Children[0])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DaytimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NighttimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[1]) || SelectedTreeItem == TreeItem.Children[2].Children[1])
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

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[2]) || SelectedTreeItem == TreeItem.Children[2].Children[2])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Subject" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "DaytimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "NighttimeImage" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Interval" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[3]) || SelectedTreeItem == TreeItem.Children[2].Children[3])
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

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[4]) || SelectedTreeItem == TreeItem.Children[2].Children[4])
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

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[5]) || SelectedTreeItem == TreeItem.Children[2].Children[5])
			{
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Location" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Width" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Height" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "TransparentColor" });
				ListColumns.Add(new ListViewColumnHeaderModel { Text = "Layer" });
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[1]))
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

			if (SelectedTreeItem == TreeItem.Children[1])
			{
				foreach (Screen screen in Screens)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[2]), Tag = screen };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[0]) || SelectedTreeItem == TreeItem.Children[2].Children[0])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<PilotLampElement> pilotLamps = screen != null ? screen.PanelElements.OfType<PilotLampElement>() : PanelElements.OfType<PilotLampElement>();

				foreach (PilotLampElement pilotLamp in pilotLamps)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[6]), Tag = pilotLamp };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[1]) || SelectedTreeItem == TreeItem.Children[2].Children[1])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<NeedleElement> needles = screen != null ? screen.PanelElements.OfType<NeedleElement>() : PanelElements.OfType<NeedleElement>();

				foreach (NeedleElement needle in needles)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[17]), Tag = needle };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[2]) || SelectedTreeItem == TreeItem.Children[2].Children[2])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<DigitalNumberElement> digitalNumbers = screen != null ? screen.PanelElements.OfType<DigitalNumberElement>() : PanelElements.OfType<DigitalNumberElement>();

				foreach (DigitalNumberElement digitalNumber in digitalNumbers)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[7]), Tag = digitalNumber };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[3]) || SelectedTreeItem == TreeItem.Children[2].Children[3])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<DigitalGaugeElement> digitalGauges = screen != null ? screen.PanelElements.OfType<DigitalGaugeElement>() : PanelElements.OfType<DigitalGaugeElement>();

				foreach (DigitalGaugeElement digitalGauge in digitalGauges)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[10]), Tag = digitalGauge };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[4]) || SelectedTreeItem == TreeItem.Children[2].Children[4])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<LinearGaugeElement> linearGauges = screen != null ? screen.PanelElements.OfType<LinearGaugeElement>() : PanelElements.OfType<LinearGaugeElement>();

				foreach (LinearGaugeElement linearGauge in linearGauges)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[10]), Tag = linearGauge };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[0].Children[5]) || SelectedTreeItem == TreeItem.Children[2].Children[5])
			{
				Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;

				IEnumerable<TimetableElement> timetables = screen != null ? screen.PanelElements.OfType<TimetableElement>() : PanelElements.OfType<TimetableElement>();

				foreach (TimetableElement timetable in timetables)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[5]), Tag = timetable };
					UpdateListItem(newItem);
					ListItems.Add(newItem);
				}
			}

			if (TreeItem.Children[1].Children.Any(y => SelectedTreeItem == y.Children[1]))
			{
				Screen screen = (Screen)SelectedTreeItem.Parent.Tag;

				foreach (TouchElement touch in screen.TouchElements)
				{
					ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[4]), Tag = touch };
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
				item.Texts[0] = screen.Number.ToString(culture);
				item.Texts[1] = screen.Layer.ToString(culture);
			}

			if (pilotLamp != null)
			{
				item.Texts[0] = pilotLamp.Subject.ToString();
				item.Texts[1] = $"{pilotLamp.LocationX.ToString(culture)}, {pilotLamp.LocationY.ToString(culture)}";
				item.Texts[2] = pilotLamp.DaytimeImage;
				item.Texts[3] = pilotLamp.NighttimeImage;
				item.Texts[4] = pilotLamp.TransparentColor.ToString();
				item.Texts[5] = pilotLamp.Layer.ToString(culture);
			}

			if (needle != null)
			{
				item.Texts[0] = needle.Subject.ToString();
				item.Texts[1] = $"{needle.LocationX.ToString(culture)}, {needle.LocationY.ToString(culture)}";
				item.Texts[2] = needle.DefinedRadius ? needle.Radius.ToString(culture) : string.Empty;
				item.Texts[3] = needle.DaytimeImage;
				item.Texts[4] = needle.NighttimeImage;
				item.Texts[5] = needle.Color.ToString();
				item.Texts[6] = needle.TransparentColor.ToString();
				item.Texts[7] = needle.DefinedOrigin ? $"{needle.OriginX.ToString(culture)}, {needle.OriginY.ToString(culture)}" : string.Empty;
				item.Texts[8] = needle.InitialAngle.ToDegrees().ToString(culture);
				item.Texts[9] = needle.LastAngle.ToDegrees().ToString(culture);
				item.Texts[10] = needle.Minimum.ToString(culture);
				item.Texts[11] = needle.Maximum.ToString(culture);
				item.Texts[12] = needle.DefinedNaturalFreq ? needle.NaturalFreq.ToString(culture) : string.Empty;
				item.Texts[13] = needle.DefinedDampingRatio ? needle.DampingRatio.ToString(culture) : string.Empty;
				item.Texts[14] = needle.Backstop.ToString();
				item.Texts[15] = needle.Smoothed.ToString();
				item.Texts[16] = needle.Layer.ToString(culture);
			}

			if (digitalNumber != null)
			{
				item.Texts[0] = digitalNumber.Subject.ToString();
				item.Texts[1] = $"{digitalNumber.LocationX.ToString(culture)}, {digitalNumber.LocationY.ToString(culture)}";
				item.Texts[2] = digitalNumber.DaytimeImage;
				item.Texts[3] = digitalNumber.NighttimeImage;
				item.Texts[4] = digitalNumber.TransparentColor.ToString();
				item.Texts[5] = digitalNumber.Interval.ToString(culture);
				item.Texts[6] = digitalNumber.Layer.ToString(culture);
			}

			if (digitalGauge != null)
			{
				item.Texts[0] = digitalGauge.Subject.ToString();
				item.Texts[1] = $"{digitalGauge.LocationX.ToString(culture)}, {digitalGauge.LocationY.ToString(culture)}";
				item.Texts[2] = digitalGauge.Radius.ToString(culture);
				item.Texts[3] = digitalGauge.Color.ToString();
				item.Texts[4] = digitalGauge.InitialAngle.ToDegrees().ToString(culture);
				item.Texts[5] = digitalGauge.LastAngle.ToDegrees().ToString(culture);
				item.Texts[6] = digitalGauge.Minimum.ToString(culture);
				item.Texts[7] = digitalGauge.Maximum.ToString(culture);
				item.Texts[8] = digitalGauge.Step.ToString(culture);
				item.Texts[9] = digitalGauge.Layer.ToString(culture);
			}

			if (linearGauge != null)
			{
				item.Texts[0] = linearGauge.Subject.ToString();
				item.Texts[1] = $"{linearGauge.LocationX.ToString(culture)}, {linearGauge.LocationY.ToString(culture)}";
				item.Texts[2] = linearGauge.DaytimeImage;
				item.Texts[3] = linearGauge.NighttimeImage;
				item.Texts[4] = linearGauge.TransparentColor.ToString();
				item.Texts[5] = linearGauge.Minimum.ToString(culture);
				item.Texts[6] = linearGauge.Maximum.ToString(culture);
				item.Texts[7] = $"{linearGauge.DirectionX.ToString(culture)}, {linearGauge.DirectionY.ToString(culture)}";
				item.Texts[8] = linearGauge.Width.ToString(culture);
				item.Texts[9] = linearGauge.Layer.ToString(culture);
			}

			if (timetable != null)
			{
				item.Texts[0] = $"{timetable.LocationX.ToString(culture)}, {timetable.LocationY.ToString(culture)}";
				item.Texts[1] = timetable.Width.ToString(culture);
				item.Texts[2] = timetable.Height.ToString(culture);
				item.Texts[3] = timetable.TransparentColor.ToString();
				item.Texts[4] = timetable.Layer.ToString(culture);
			}

			if (touch != null)
			{
				item.Texts[0] = $"{touch.LocationX.ToString(culture)}, {touch.LocationY.ToString(culture)}";
				item.Texts[1] = $"{touch.SizeX.ToString(culture)}, {touch.SizeY.ToString(culture)}";
				item.Texts[2] = touch.JumpScreen.ToString(culture);
				item.Texts[3] = touch.Layer.ToString(culture);
			}
		}

		internal void AddScreen()
		{
			Screen screen = new Screen();

			if (Screens.Any())
			{
				screen.Number = Screens.Last().Number + 1;
			}

			Screens.Add(screen);

			TreeItem.Children[1].Children.Add(CreateScreenTreeItem(TreeItem.Children[1], Screens.Last()));

			SelectedListItem = ListItems.Last();
		}

		internal void AddPilotLamp()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			PilotLampElement pilotLamp = new PilotLampElement();

			if (screen != null)
			{
				screen.PanelElements.Add(pilotLamp);
			}
			else
			{
				PanelElements.Add(pilotLamp);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[6]), Tag = pilotLamp };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddNeedle()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			NeedleElement needle = new NeedleElement();

			if (screen != null)
			{
				screen.PanelElements.Add(needle);
			}
			else
			{
				PanelElements.Add(needle);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[17]), Tag = needle };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddDigitalNumber()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			DigitalNumberElement digitalNumber = new DigitalNumberElement();

			if (screen != null)
			{
				screen.PanelElements.Add(digitalNumber);
			}
			else
			{
				PanelElements.Add(digitalNumber);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[7]), Tag = digitalNumber };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddDigitalGauge()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			DigitalGaugeElement digitalGauge = new DigitalGaugeElement();

			if (screen != null)
			{
				screen.PanelElements.Add(digitalGauge);
			}
			else
			{
				PanelElements.Add(digitalGauge);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[10]), Tag = digitalGauge };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddLinearGauge()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			LinearGaugeElement linearGauge = new LinearGaugeElement();

			if (screen != null)
			{
				screen.PanelElements.Add(linearGauge);
			}
			else
			{
				PanelElements.Add(linearGauge);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[10]), Tag = linearGauge };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddTimetable()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			TimetableElement timetable = new TimetableElement();

			if (screen != null)
			{
				screen.PanelElements.Add(timetable);
			}
			else
			{
				PanelElements.Add(timetable);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[5]), Tag = timetable };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void AddTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			TouchElement touch = new TouchElement(screen);

			screen.TouchElements.Add(touch);

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[4]), Tag = touch };
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

			TreeItem.Children[1].Children.Add(CreateScreenTreeItem(TreeItem.Children[1], Screens.Last()));

			SelectedListItem = ListItems.Last();
		}

		internal void CopyPilotLamp()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			PilotLampElement pilotLamp = (PilotLampElement)((PilotLampElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(pilotLamp);
			}
			else
			{
				PanelElements.Add(pilotLamp);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[6]), Tag = pilotLamp };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyNeedle()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			NeedleElement needle = (NeedleElement)((NeedleElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(needle);
			}
			else
			{
				PanelElements.Add(needle);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[17]), Tag = needle };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyDigitalNumber()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			DigitalNumberElement digitalNumber = (DigitalNumberElement)((DigitalNumberElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(digitalNumber);
			}
			else
			{
				PanelElements.Add(digitalNumber);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[7]), Tag = digitalNumber };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyDigitalGauge()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			DigitalGaugeElement digitalGauge = (DigitalGaugeElement)((DigitalGaugeElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(digitalGauge);
			}
			else
			{
				PanelElements.Add(digitalGauge);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[10]), Tag = digitalGauge };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyLinearGauge()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			LinearGaugeElement linearGauge = (LinearGaugeElement)((LinearGaugeElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(linearGauge);
			}
			else
			{
				PanelElements.Add(linearGauge);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[10]), Tag = linearGauge };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyTimetable()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			TimetableElement timetable = (TimetableElement)((TimetableElement)SelectedListItem.Tag).Clone();

			if (screen != null)
			{
				screen.PanelElements.Add(timetable);
			}
			else
			{
				PanelElements.Add(timetable);
			}

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[5]), Tag = timetable };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void CopyTouch()
		{
			Screen screen = (Screen)SelectedTreeItem.Parent.Tag;
			TouchElement touch = (TouchElement)((TouchElement)SelectedListItem.Tag).Clone();

			screen.TouchElements.Add(touch);

			ListViewItemModel newItem = new ListViewItemModel { Texts = new ObservableCollection<string>(new string[4]), Tag = touch };
			UpdateListItem(newItem);
			ListItems.Add(newItem);

			SelectedListItem = ListItems.Last();
		}

		internal void RemoveScreen()
		{
			Screen screen = (Screen)SelectedListItem.Tag;

			Screens.Remove(screen);

			TreeItem.Children[1].Children.RemoveAll(x => x.Tag == screen);

			SelectedListItem = null;
		}

		internal void RemovePilotLamp()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			PilotLampElement pilotLamp = (PilotLampElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(pilotLamp);
			}
			else
			{
				PanelElements.Remove(pilotLamp);
			}

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveNeedle()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			NeedleElement needle = (NeedleElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(needle);
			}
			else
			{
				PanelElements.Remove(needle);
			}

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveDigitalNumber()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			DigitalNumberElement digitalNumber = (DigitalNumberElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(digitalNumber);
			}
			else
			{
				PanelElements.Remove(digitalNumber);
			}

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveDigitalGauge()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			DigitalGaugeElement digitalGauge = (DigitalGaugeElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(digitalGauge);
			}
			else
			{
				PanelElements.Remove(digitalGauge);
			}

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveLinearGauge()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			LinearGaugeElement linearGauge = (LinearGaugeElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(linearGauge);
			}
			else
			{
				PanelElements.Remove(linearGauge);
			}

			ListItems.Remove(SelectedListItem);

			SelectedListItem = null;
		}

		internal void RemoveTimetable()
		{
			Screen screen = SelectedTreeItem.Parent.Parent.Tag as Screen;
			TimetableElement timetable = (TimetableElement)SelectedListItem.Tag;

			if (screen != null)
			{
				screen.PanelElements.Remove(timetable);
			}
			else
			{
				PanelElements.Remove(timetable);
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
