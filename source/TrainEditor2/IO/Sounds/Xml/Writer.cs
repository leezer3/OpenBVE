using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Sounds;

namespace TrainEditor2.IO.Sounds.Xml
{
	internal static partial class SoundCfgXml
	{
		internal static void Write(string fileName, Sound sound)
		{
			XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

			XElement openBVE = new XElement("openBVE");
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance")));
			openBVE.Add(new XAttribute(XNamespace.Xmlns + "xsd", XNamespace.Get("http://www.w3.org/2001/XMLSchema")));
			xml.Add(openBVE);

			XElement carSounds = new XElement("CarSounds");
			openBVE.Add(carSounds);

			WriteArrayNode(fileName, carSounds, "Run", sound.SoundElements.OfType<RunElement>());
			WriteArrayNode(fileName, carSounds, "Flange", sound.SoundElements.OfType<FlangeElement>());
			WriteArrayNode(fileName, carSounds, "Motor", sound.SoundElements.OfType<MotorElement>());
			WriteArrayNode(fileName, carSounds, "PointFrontAxle", sound.SoundElements.OfType<FrontSwitchElement>());
			WriteArrayNode(fileName, carSounds, "PointRearAxle", sound.SoundElements.OfType<RearSwitchElement>());
			WriteArrayNode(fileName, carSounds, "Brake", sound.SoundElements.OfType<BrakeElement>());
			WriteArrayNode(fileName, carSounds, "Compressor", sound.SoundElements.OfType<CompressorElement>());
			WriteArrayNode(fileName, carSounds, "Suspension", sound.SoundElements.OfType<SuspensionElement>());
			WriteHornNode(fileName, carSounds, sound.SoundElements.OfType<PrimaryHornElement>(), sound.SoundElements.OfType<SecondaryHornElement>(), sound.SoundElements.OfType<MusicHornElement>());
			WriteArrayNode(fileName, carSounds, "Door", sound.SoundElements.OfType<DoorElement>());
			WriteArrayNode(fileName, carSounds, "Ats", sound.SoundElements.OfType<AtsElement>());
			WriteNode(fileName, carSounds, "Buzzer", sound.SoundElements.OfType<BuzzerElement>().FirstOrDefault());
			WriteArrayNode(fileName, carSounds, "PilotLamp", sound.SoundElements.OfType<PilotLampElement>());
			WriteArrayNode(fileName, carSounds, "BrakeHandle", sound.SoundElements.OfType<BrakeHandleElement>());
			WriteArrayNode(fileName, carSounds, "MasterController", sound.SoundElements.OfType<MasterControllerElement>());
			WriteArrayNode(fileName, carSounds, "Reverser", sound.SoundElements.OfType<ReverserElement>());
			WriteArrayNode(fileName, carSounds, "Breaker", sound.SoundElements.OfType<BreakerElement>());
			WriteArrayNode(fileName, carSounds, "RequestStop", sound.SoundElements.OfType<RequestStopElement>());
			WriteArrayNode(fileName, carSounds, "Touch", sound.SoundElements.OfType<TouchElement>());
			WriteNode(fileName, carSounds, "Noise", sound.SoundElements.OfType<OthersElement>().FirstOrDefault(x => x.Key == OthersKey.Noise));
			WriteNode(fileName, carSounds, "Shoe", sound.SoundElements.OfType<OthersElement>().FirstOrDefault(x => x.Key == OthersKey.Shoe));
			WriteNode(fileName, carSounds, "Halt", sound.SoundElements.OfType<OthersElement>().FirstOrDefault(x => x.Key == OthersKey.Halt));

			xml.Save(fileName);
		}

		private static void WriteNode(string fileName, XElement parent, string nodeName, SoundElement element)
		{
			XElement newNode = new XElement(nodeName);

			if (element != null && !string.IsNullOrEmpty(element.FilePath))
			{
				newNode.Add(new XElement("FileName", Utilities.MakeRelativePath(fileName, element.FilePath)));
			}
			else
			{
				return;
			}

			if (element.DefinedPosition)
			{
				newNode.Add(new XElement("Position", $"{element.PositionX}, {element.PositionY}, {element.PositionZ}"));
			}

			if (element.DefinedRadius)
			{
				newNode.Add(new XElement("Radius", element.Radius));
			}

			parent.Add(newNode);
		}

		private static void WriteArrayNode(string fileName, XElement parent, string nodeName, IEnumerable<SoundElement> elements)
		{
			XElement newNode = new XElement(nodeName);

			foreach (SoundElement element in elements)
			{
				WriteNode(fileName, newNode, ((Enum)element.Key).GetStringValues().Last(), element);
			}

			if (newNode.HasElements)
			{
				parent.Add(newNode);
			}
		}

		private static void WriteArrayNode(string fileName, XElement parent, string nodeName, IEnumerable<SoundElement<int>> elements)
		{
			XElement newNode = new XElement(nodeName);

			foreach (SoundElement<int> element in elements)
			{
				if (!string.IsNullOrEmpty(element.FilePath))
				{
					WriteNode(fileName, newNode, "Sound", element);
					newNode.Elements("Sound").Last().AddFirst(new XElement("Index", element.Key));
				}
			}

			if (newNode.HasElements)
			{
				parent.Add(newNode);
			}
		}

		private static void WriteHornNode(string fileName, XElement parent, IEnumerable<PrimaryHornElement> primary, IEnumerable<SecondaryHornElement> secondary, IEnumerable<MusicHornElement> music)
		{
			XElement newNode = new XElement("Horn");

			WriteArrayNode(fileName, newNode, "Primary", primary);
			WriteArrayNode(fileName, newNode, "Secondary", secondary);
			WriteArrayNode(fileName, newNode, "Music", music);

			if (newNode.HasElements)
			{
				parent.Add(newNode);
			}
		}
	}
}
