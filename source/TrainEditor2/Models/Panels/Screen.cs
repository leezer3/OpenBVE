using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TrainEditor2.Models.Panels
{
	internal class Screen : PanelElement
	{
		private int number;

		internal int Number
		{
			get => number;
			set => SetProperty(ref number, value);
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

		public override object Clone()
		{
			Screen screen = (Screen)MemberwiseClone();
			screen.PanelElements = new ObservableCollection<PanelElement>(PanelElements.Select(e => (PanelElement)e.Clone()));
			screen.TouchElements = new ObservableCollection<TouchElement>(TouchElements.Select(e => (TouchElement)e.Clone()));
			return screen;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			throw new System.NotImplementedException();
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			XElement screenNode = new XElement("Screen",
				new XElement("Number", Number),
				new XElement("Layer", Layer)
			);

			foreach (PanelElement element in PanelElements)
			{
				element.WriteXML(fileName, screenNode);
			}

			foreach (TouchElement element in TouchElements)
			{
				element.WriteXML(fileName, screenNode);
			}

			parent.Add(screenNode);
		}

		public override void WriteIntermediate(XElement parent)
		{
			XElement screenNode = new XElement("Screen",
				new XElement("Number", Number),
				new XElement("Layer", Layer)
				);
			parent.Add(screenNode);

			XElement panelElementsNode = new XElement("PanelElements");
			screenNode.Add(panelElementsNode);

			foreach (PanelElement element in PanelElements)
			{
				element.WriteIntermediate(panelElementsNode);
			}

			XElement touchElementsNode = new XElement("TouchElements");
			screenNode.Add(touchElementsNode);

			foreach (TouchElement element in TouchElements)
			{
				element.WriteIntermediate(touchElementsNode);
			}
		}
	}
}
