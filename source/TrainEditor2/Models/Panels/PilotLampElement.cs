using OpenBveApi.Colors;
using System.Text;
using System.Xml.Linq;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class PilotLampElement : PanelElement
	{
		private Subject subject;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;

		internal Subject Subject
		{
			get => subject;
			set => SetProperty(ref subject, value);
		}

		internal string DaytimeImage
		{
			get => daytimeImage;
			set => SetProperty(ref daytimeImage, value);
		}

		internal string NighttimeImage
		{
			get => nighttimeImage;
			set => SetProperty(ref nighttimeImage, value);
		}

		internal Color24 TransparentColor
		{
			get => transparentColor;
			set => SetProperty(ref transparentColor, value);
		}

		internal PilotLampElement()
		{
			Subject = new Subject();
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
		}

		public override object Clone()
		{
			PilotLampElement element = (PilotLampElement)base.Clone();
			Subject = (Subject)Subject.Clone();
			return element;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[PilotLamp]");
			Utilities.WriteKey(builder, "Subject", Subject.ToString());
			Utilities.WriteKey(builder, "Location", Location.X, Location.Y);
			Utilities.WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage));
			Utilities.WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage));
			Utilities.WriteKey(builder, "TransparentColor", TransparentColor.ToString());
			Utilities.WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			XElement pilotLampNode = new XElement("PilotLamp",
			new XElement("Location", $"{Location.X}, {Location.Y}"),
			new XElement("Layer", Layer),
				new XElement("Subject", Subject)
			);

			if (!string.IsNullOrEmpty(DaytimeImage))
			{
				pilotLampNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(NighttimeImage))
			{
				pilotLampNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage)));
			}

			pilotLampNode.Add(new XElement("TransparentColor", TransparentColor));

			parent.Add(pilotLampNode);
		}

		public override void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("PilotLamp",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer",Layer),
				WriteSubjectNode(Subject),
				new XElement("DaytimeImage", DaytimeImage),
				new XElement("NighttimeImage", NighttimeImage),
				new XElement("TransparentColor", TransparentColor)
				));
		}
	}
}
