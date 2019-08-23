using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TrainEditor2.Models.Panels
{
	internal class Panel : ICloneable
	{
		internal This This;
		internal ObservableCollection<Screen> Screens;
		internal ObservableCollection<PanelElement> PanelElements;

		internal Panel()
		{
			This = new This();
			Screens = new ObservableCollection<Screen>();
			PanelElements = new ObservableCollection<PanelElement>();
		}

		public object Clone()
		{
			Panel panel = (Panel)MemberwiseClone();
			panel.Screens = new ObservableCollection<Screen>(Screens.Select(s => (Screen)s.Clone()));
			panel.PanelElements = new ObservableCollection<PanelElement>(PanelElements.Select(e => (PanelElement)e.Clone()));
			return panel;
		}
	}
}
