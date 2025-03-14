using System.Text;
using System.Windows;
using System.Xml.Linq;
using OpenBveApi.Colors;

namespace TrainEditor2.Models.Panels
{
	internal class TimetableElement : PanelElement
	{
		private double width;
		private double height;
		private Color24 transparentColor;

		internal double Width
		{
			get
			{
				return width;
			}
			set
			{
				SetProperty(ref width, value);
			}
		}

		internal double Height
		{
			get
			{
				return height;
			}
			set
			{
				SetProperty(ref height, value);
			}
		}

		internal Color24 TransparentColor
		{
			get
			{
				return transparentColor;
			}
			set
			{
				SetProperty(ref transparentColor, value);
			}
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
			WriteKey(builder, "Location", LocationX, LocationY);
			WriteKey(builder, "Width", Width);
			WriteKey(builder, "Height", Height);
			WriteKey(builder, "TransparentColor", TransparentColor.ToString());
			WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			parent.Add(new XElement("Timetable",
			new XElement("Location", $"{LocationX}, {LocationY}"),
			new XElement("Layer", Layer),
				new XElement("Width", Width),
				new XElement("Height", Height),
				new XElement("TransparentColor", TransparentColor)
			));
		}
	}
}
