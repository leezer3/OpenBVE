using System.Xml.Linq;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.IO.Trains.XML
{
  internal static class TrainXML
  {
		internal static void Write(string fileName, Train train)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement openBVE = new XElement("openBVE");
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")));
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")));
			

			XElement trainNode = new XElement("Train");
			for (int i = 0; i < train.Cars.Count; i++)
			{
				if (i > 0)
				{
					train.Couplers[i - 1].WriteXML(trainNode);
				}
				train.Cars[i].WriteXML(fileName, trainNode, train, i);
			}

			trainNode.Add(new XElement("DriverCar", train.Cab.DriverCar));

			openBVE.Add(trainNode);
			xml.Add(openBVE);
			xml.Save(fileName);
		}
	}
}
