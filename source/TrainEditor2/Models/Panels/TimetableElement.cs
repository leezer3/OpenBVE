using OpenBveApi.Colors;
using System.Text;
using System.Xml.Linq;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class TimetableElement : PanelElement
	{
		private double width;
		private double height;
		private Color24 transparentColor;

		internal double Width
		{
			get => width;
			set => SetProperty(ref width, value);
		}

		internal double Height
		{
			get => height;
			set => SetProperty(ref height, value);
		}

		internal Color24 TransparentColor
		{
			get => transparentColor;
			set => SetProperty(ref transparentColor, value);
		}

		internal TimetableElement()
		{
			Width = 0.0;
			Height = 0.0;
			TransparentColor = Color24.Blue;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[Timetable]");
			Utilities.WriteKey(builder, "Location", Location.X, Location.Y);
			Utilities.WriteKey(builder, "Width", Width);
			Utilities.WriteKey(builder, "Height", Height);
			Utilities.WriteKey(builder, "TransparentColor", TransparentColor.ToString());
			Utilities.WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			parent.Add(new XElement("Timetable",
			new XElement("Location", $"{Location.X}, {Location.Y}"),
			new XElement("Layer", Layer),
				new XElement("Width", Width),
				new XElement("Height", Height),
				new XElement("TransparentColor", TransparentColor)
			));
		}

		public override void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("Timetable",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				new XElement("Width", Width),
				new XElement("Height", Height),
				new XElement("TransparentColor", TransparentColor)
				));
		}
	}
}
