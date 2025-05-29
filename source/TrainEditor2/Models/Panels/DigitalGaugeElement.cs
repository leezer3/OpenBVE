using OpenBveApi.Colors;
using System.Text;
using System.Xml.Linq;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class DigitalGaugeElement : PanelElement
	{
		private Subject subject;
		private double radius;
		private Color24 color;
		private double initialAngle;
		private double lastAngle;
		private double minimum;
		private double maximum;
		private double step;

		internal Subject Subject
		{
			get => subject;
			set => SetProperty(ref subject, value);
		}

		internal double Radius
		{
			get => radius;
			set => SetProperty(ref radius, value);
		}

		internal Color24 Color
		{
			get => color;
			set => SetProperty(ref color, value);
		}

		internal double InitialAngle
		{
			get => initialAngle;
			set => SetProperty(ref initialAngle, value);
		}

		internal double LastAngle
		{
			get => lastAngle;
			set => SetProperty(ref lastAngle, value);
		}

		internal double Minimum
		{
			get => minimum;
			set => SetProperty(ref minimum, value);
		}

		internal double Maximum
		{
			get => maximum;
			set => SetProperty(ref maximum, value);
		}

		internal double Step
		{
			get => step;
			set => SetProperty(ref step, value);
		}

		internal DigitalGaugeElement()
		{
			Subject = new Subject();
			Radius = 16.0;
			Color = Color24.Black;
			InitialAngle = -2.0943951023932;
			LastAngle = 2.0943951023932;
			Minimum = 0.0;
			Maximum = 1000.0;
			Step = 0.0;
		}

		public override object Clone()
		{
			DigitalGaugeElement element = (DigitalGaugeElement)base.Clone();
			Subject = (Subject)Subject.Clone();
			return element;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[DigitalGauge]");
			Utilities.WriteKey(builder, "Subject", Subject.ToString());
			Utilities.WriteKey(builder, "Location", Location.X, Location.Y);
			Utilities.WriteKey(builder, "Radius", Radius);
			Utilities.WriteKey(builder, "Color", Color.ToString());
			Utilities.WriteKey(builder, "InitialAngle", InitialAngle.ToDegrees());
			Utilities.WriteKey(builder, "LastAngle", LastAngle.ToDegrees());
			Utilities.WriteKey(builder, "Minimum", Minimum);
			Utilities.WriteKey(builder, "Maximum", Maximum);
			Utilities.WriteKey(builder, "Step", Step);
			Utilities.WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			parent.Add(new XElement("DigitalGauge",
			new XElement("Location", $"{Location.X}, {Location.Y}"),
			new XElement("Layer", Layer),
				new XElement("Subject", Subject),
				new XElement("Radius", Radius),
				new XElement("Color", Color),
				new XElement("InitialAngle", InitialAngle.ToDegrees()),
				new XElement("LastAngle", LastAngle.ToDegrees()),
				new XElement("Minimum", Minimum),
				new XElement("Maximum", Maximum),
				new XElement("Step", Step)
			));
		}

		public override void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("DigitalGauge",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				WriteSubjectNode(Subject),
				new XElement("Radius", Radius),
				new XElement("Color", Color),
				new XElement("InitialAngle", InitialAngle),
				new XElement("LastAngle", LastAngle),
				new XElement("Minimum", Minimum),
				new XElement("Maximum", Maximum),
				new XElement("Step", Step)
				));
		}
	}
}
