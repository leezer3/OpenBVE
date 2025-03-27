using System.Text;
using System.Xml.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class LinearGaugeElement : PanelElement
	{
		private Subject subject;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;
		private double minimum;
		private double maximum;
		private Vector2 direction;
		private int width;

		internal Subject Subject
		{
			get
			{
				return subject;
			}
			set
			{
				SetProperty(ref subject, value);
			}
		}

		internal string DaytimeImage
		{
			get
			{
				return daytimeImage;
			}
			set
			{
				SetProperty(ref daytimeImage, value);
			}
		}

		internal string NighttimeImage
		{
			get
			{
				return nighttimeImage;
			}
			set
			{
				SetProperty(ref nighttimeImage, value);
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

		internal double Minimum
		{
			get
			{
				return minimum;
			}
			set
			{
				SetProperty(ref minimum, value);
			}
		}

		internal double Maximum
		{
			get
			{
				return maximum;
			}
			set
			{
				SetProperty(ref maximum, value);
			}
		}

		internal Vector2 Direction
		{
			get
			{
				return direction;
			}
			set
			{
				SetProperty(ref direction, value);
			}
		}

		internal int Width
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

		internal LinearGaugeElement()
		{
			Subject = new Subject();
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
			Minimum = 0.0;
			Maximum = 0.0;
			Direction = Vector2.Null;
			Width = 0;
		}

		public override object Clone()
		{
			LinearGaugeElement element = (LinearGaugeElement)base.Clone();
			Subject = (Subject)Subject.Clone();
			return element;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[LinearGauge]");
			Utilities.WriteKey(builder, "Subject", Subject.ToString());
			Utilities.WriteKey(builder, "Location", Location.X, Location.Y);
			Utilities.WriteKey(builder, "Minimum", Minimum);
			Utilities.WriteKey(builder, "Maximum", Maximum);
			Utilities.WriteKey(builder, "Direction", Direction.X, Direction.Y);
			Utilities.WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage));
			Utilities.WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage));
			Utilities.WriteKey(builder, "Width", Width);
			Utilities.WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			XElement linearGaugeNode = new XElement("LinearGauge",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				new XElement("Subject", Subject)
			);

			if (!string.IsNullOrEmpty(DaytimeImage))
			{
				linearGaugeNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(NighttimeImage))
			{
				linearGaugeNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage)));
			}

			linearGaugeNode.Add(
				new XElement("TransparentColor", TransparentColor),
				new XElement("Minimum", Minimum),
				new XElement("Maximum", Maximum),
				new XElement("Direction", $"{Direction.X}, {Direction.Y}"),
				new XElement("Width", Width)
			);

			parent.Add(linearGaugeNode);
		}
	}
}
