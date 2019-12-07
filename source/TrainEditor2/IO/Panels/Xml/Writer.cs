using System.Linq;
using System.Xml.Linq;
using TrainEditor2.Extensions;
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

			WriteThisNode(fileName, panelNode, panel.This);

			foreach (Screen screen in panel.Screens)
			{
				WriteScreenNode(fileName, panelNode, screen);
			}

			foreach (PanelElement element in panel.PanelElements)
			{
				WritePanelElementNode(fileName, panelNode, element);
			}

			xml.Save(fileName);
		}

		private static void WriteThisNode(string fileName, XElement parent, This This)
		{
			XElement thisNode = new XElement("This",
				new XElement("Resolution", This.Resolution),
				new XElement("Left", This.Left),
				new XElement("Right", This.Right),
				new XElement("Top", This.Top),
				new XElement("Bottom", This.Bottom)
			);

			if (!string.IsNullOrEmpty(This.DaytimeImage))
			{
				thisNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, This.DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(This.NighttimeImage))
			{
				thisNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, This.NighttimeImage)));
			}

			thisNode.Add(
				new XElement("TransparentColor", This.TransparentColor),
				new XElement("Center", $"{This.CenterX}, {This.CenterY}"),
				new XElement("Origin", $"{This.OriginX}, {This.OriginY}")
			);

			parent.Add(thisNode);
		}

		private static void WriteScreenNode(string fileName, XElement parent, Screen screen)
		{
			XElement screenNode = new XElement("Screen",
				new XElement("Number", screen.Number),
				new XElement("Layer", screen.Layer)
			);

			foreach (PanelElement element in screen.PanelElements)
			{
				WritePanelElementNode(fileName, screenNode, element);
			}

			foreach (TouchElement element in screen.TouchElements)
			{
				WriteTouchElementNode(screenNode, element);
			}

			parent.Add(screenNode);
		}

		private static void WritePanelElementNode(string fileName, XElement parent, PanelElement element)
		{
			if (element is PilotLampElement)
			{
				WritePilotLampElementNode(fileName, parent, (PilotLampElement)element);
			}

			if (element is NeedleElement)
			{
				WriteNeedleElementNode(fileName, parent, (NeedleElement)element);
			}

			if (element is DigitalNumberElement)
			{
				WriteDigitalNumberElementNode(fileName, parent, (DigitalNumberElement)element);
			}

			if (element is DigitalGaugeElement)
			{
				WriteDigitalGaugeElementNode(parent, (DigitalGaugeElement)element);
			}

			if (element is LinearGaugeElement)
			{
				WriteLinearGaugeElementNode(fileName, parent, (LinearGaugeElement)element);
			}

			if (element is TimetableElement)
			{
				WriteTimetableElementNode(parent, (TimetableElement)element);
			}
		}

		private static void WritePilotLampElementNode(string fileName, XElement parent, PilotLampElement element)
		{
			XElement pilotLampNode = new XElement("PilotLamp",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Subject", element.Subject)
			);

			if (!string.IsNullOrEmpty(element.DaytimeImage))
			{
				pilotLampNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(element.NighttimeImage))
			{
				pilotLampNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage)));
			}

			pilotLampNode.Add(new XElement("TransparentColor", element.TransparentColor));

			parent.Add(pilotLampNode);
		}

		private static void WriteNeedleElementNode(string fileName, XElement parent, NeedleElement element)
		{
			XElement needleNode = new XElement("Needle",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Subject", element.Subject)
				);

			if (!string.IsNullOrEmpty(element.DaytimeImage))
			{
				needleNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(element.NighttimeImage))
			{
				needleNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage)));
			}

			needleNode.Add(
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Color", element.Color),
				new XElement("InitialAngle", element.InitialAngle.ToDegrees()),
				new XElement("LastAngle", element.LastAngle.ToDegrees()),
				new XElement("Minimum", element.Minimum),
				new XElement("Maximum", element.Maximum),
				new XElement("Backstop", element.Backstop),
				new XElement("Smoothed", element.Smoothed)
				);

			if (element.DefinedRadius)
			{
				needleNode.Add(new XElement("Radius", element.Radius));
			}

			if (element.DefinedOrigin)
			{
				needleNode.Add(new XElement("Origin", $"{element.OriginX}, {element.OriginY}"));
			}

			if (element.DefinedNaturalFreq)
			{
				needleNode.Add(new XElement("NaturalFreq", element.NaturalFreq));
			}

			if (element.DefinedDampingRatio)
			{
				needleNode.Add(new XElement("DampingRatio", element.DampingRatio));
			}

			parent.Add(needleNode);
		}

		private static void WriteDigitalNumberElementNode(string fileName, XElement parent, DigitalNumberElement element)
		{
			XElement digitalNumberNode = new XElement("DigitalNumber",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Subject", element.Subject)
			);

			if (!string.IsNullOrEmpty(element.DaytimeImage))
			{
				digitalNumberNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(element.NighttimeImage))
			{
				digitalNumberNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage)));
			}

			digitalNumberNode.Add(
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Interval", element.Interval)
				);

			parent.Add(digitalNumberNode);
		}

		private static void WriteDigitalGaugeElementNode(XElement parent, DigitalGaugeElement element)
		{
			parent.Add(new XElement("DigitalGauge",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Subject", element.Subject),
				new XElement("Radius", element.Radius),
				new XElement("Color", element.Color),
				new XElement("InitialAngle", element.InitialAngle.ToDegrees()),
				new XElement("LastAngle", element.LastAngle.ToDegrees()),
				new XElement("Minimum", element.Minimum),
				new XElement("Maximum", element.Maximum),
				new XElement("Step", element.Step)
			));
		}

		private static void WriteLinearGaugeElementNode(string fileName, XElement parent, LinearGaugeElement element)
		{
			XElement linearGaugeNode = new XElement("LinearGauge",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Subject", element.Subject)
			);

			if (!string.IsNullOrEmpty(element.DaytimeImage))
			{
				linearGaugeNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(element.NighttimeImage))
			{
				linearGaugeNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage)));
			}

			linearGaugeNode.Add(
				new XElement("TransparentColor", element.TransparentColor),
				new XElement("Minimum", element.Minimum),
				new XElement("Maximum", element.Maximum),
				new XElement("Direction", $"{element.DirectionX}, {element.DirectionY}"),
				new XElement("Width", element.Width)
				);

			parent.Add(linearGaugeNode);
		}

		private static void WriteTimetableElementNode(XElement parent, TimetableElement element)
		{
			parent.Add(new XElement("Timetable",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Width", element.Width),
				new XElement("Height", element.Height),
				new XElement("TransparentColor", element.TransparentColor)
			));
		}

		private static void WriteTouchElementNode(XElement parent, TouchElement element)
		{
			XElement touchNode = new XElement("Touch",
				new XElement("Location", $"{element.LocationX}, {element.LocationY}"),
				new XElement("Layer", element.Layer),
				new XElement("Size", $"{element.SizeX}, {element.SizeY}"),
				new XElement("JumpScreen", element.JumpScreen)
			);

			if (element.SoundEntries.Any())
			{
				touchNode.Add(new XElement("SoundEntries", element.SoundEntries.Select(WriteTouchElementSoundEntryNode)));
			}

			if (element.CommandEntries.Any())
			{
				touchNode.Add(new XElement("CommandEntries", element.CommandEntries.Select(WriteTouchElementCommandEntryNode)));
			}

			parent.Add(touchNode);
		}

		private static XElement WriteTouchElementSoundEntryNode(TouchElement.SoundEntry entry)
		{
			return new XElement("Entry",
				new XElement("Index", entry.Index)
			);
		}

		private static XElement WriteTouchElementCommandEntryNode(TouchElement.CommandEntry entry)
		{
			return new XElement("Entry",
				new XElement("Name", entry.Info.Name),
				new XElement("Option", entry.Option)
			);
		}
	}
}
