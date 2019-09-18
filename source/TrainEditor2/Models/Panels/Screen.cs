using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;

namespace TrainEditor2.Models.Panels
{
	internal class Screen : BindableBase,ICloneable
	{
		private int number;
		private int layer;

		internal int Number
		{
			get
			{
				return number;
			}
			set
			{
				SetProperty(ref number, value);
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

		internal ObservableCollection<PanelElement> PanelElements;
		internal ObservableCollection<TouchElement> TouchElements;

		internal Screen()
		{
			Number = 0;
			Layer = 0;
			PanelElements = new ObservableCollection<PanelElement>();
			TouchElements = new ObservableCollection<TouchElement>();
		}

		public object Clone()
		{
			Screen screen = (Screen)MemberwiseClone();
			screen.PanelElements = new ObservableCollection<PanelElement>(PanelElements.Select(e => (PanelElement)e.Clone()));
			screen.TouchElements = new ObservableCollection<TouchElement>(TouchElements.Select(e => (TouchElement)e.Clone()));
			return screen;
		}
	}
}
