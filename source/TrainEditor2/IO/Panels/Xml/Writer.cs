using System.Xml.Linq;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.IO.Panels.Xml
{
	internal static partial class PanelCfgXml
	{
		internal static void Write(string fileName, Panel panel)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement openBVE = new XElement("openBVE");
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")));
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")));
			xml.Add(openBVE);

			XElement panelNode = new XElement("Panel");
			openBVE.Add(panelNode);

			panel.This.WriteXML(fileName, panelNode);

			foreach (Screen screen in panel.Screens)
			{
				screen.WriteXML(fileName, panelNode);
			}

			foreach (PanelElement element in panel.PanelElements)
			{
				element.WriteXML(fileName, panelNode);
			}

			xml.Save(fileName);
		}
		

		

		
	}
}
