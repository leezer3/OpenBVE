using OpenBveApi.Colors;
using System.Text;
using System.Xml.Linq;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class DigitalNumberElement : PanelElement
	{
		private Subject subject;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;
		private int interval;

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

		internal int Interval
		{
			get => interval;
			set => SetProperty(ref interval, value);
		}

		internal DigitalNumberElement()
		{
			Subject = new Subject();
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
			Interval = 0;
		}

		public override object Clone()
		{
			DigitalNumberElement element = (DigitalNumberElement)base.Clone();
			Subject = (Subject)Subject.Clone();
			return element;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[DigitalNumber]");
			Utilities.WriteKey(builder, "Subject", Subject.ToString());
			Utilities.WriteKey(builder, "Location", Location.X, Location.Y);
			Utilities.WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage));
			Utilities.WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage));
			Utilities.WriteKey(builder, "TransparentColor", TransparentColor.ToString());
			Utilities.WriteKey(builder, "Interval", Interval);
			Utilities.WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			XElement digitalNumberNode = new XElement("DigitalNumber",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				new XElement("Subject", Subject)
			);

			if (!string.IsNullOrEmpty(DaytimeImage))
			{
				digitalNumberNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(NighttimeImage))
			{
				digitalNumberNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage)));
			}

			digitalNumberNode.Add(
				new XElement("TransparentColor", TransparentColor),
				new XElement("Interval", Interval)
			);

			parent.Add(digitalNumberNode);
		}

		public override void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("DigitalNumber",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				WriteSubjectNode(Subject),
				new XElement("DaytimeImage", DaytimeImage),
				new XElement("NighttimeImage", NighttimeImage),
				new XElement("TransparentColor", TransparentColor),
				new XElement("Interval", Interval)
				));
		}
	}
}
