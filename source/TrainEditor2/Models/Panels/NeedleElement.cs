using System.Text;
using System.Xml.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class NeedleElement : PanelElement
	{
		private Subject subject;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;
		private bool definedRadius;
		private double radius;
		private Color24 color;
		private bool definedOrigin;
		private Vector2 origin;
		private double initialAngle;
		private double lastAngle;
		private double minimum;
		private double maximum;
		private bool definedNaturalFreq;
		private double naturalFreq;
		private bool definedDampingRatio;
		private double dampingRatio;
		private bool backstop;
		private bool smoothed;

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

		internal bool DefinedRadius
		{
			get => definedRadius;
			set => SetProperty(ref definedRadius, value);
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

		internal bool DefinedOrigin
		{
			get => definedOrigin;
			set => SetProperty(ref definedOrigin, value);
		}

		internal Vector2 Origin
		{
			get => origin;
			set => SetProperty(ref origin, value);
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

		internal bool DefinedNaturalFreq
		{
			get => definedNaturalFreq;
			set => SetProperty(ref definedNaturalFreq, value);
		}

		internal double NaturalFreq
		{

			get => naturalFreq;
			set => SetProperty(ref naturalFreq, value);
		}

		internal bool DefinedDampingRatio
		{
			get => definedDampingRatio;
			set => SetProperty(ref definedDampingRatio, value);
		}

		internal double DampingRatio
		{
			get => dampingRatio;
			set => SetProperty(ref dampingRatio, value);
		}

		internal bool Backstop
		{
			get => backstop;
			set => SetProperty(ref backstop, value);
		}

		internal bool Smoothed
		{
			get => smoothed;
			set => SetProperty(ref smoothed, value);
		}

		internal NeedleElement()
		{
			Subject = new Subject();
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
			DefinedRadius = false;
			Radius = 0.0;
			Color = Color24.White;
			DefinedOrigin = false;
			Origin = Vector2.Null;
			InitialAngle = -2.0943951023932;
			LastAngle = 2.0943951023932;
			Minimum = 0.0;
			Maximum = 1000.0;
			DefinedNaturalFreq = false;
			NaturalFreq = 0.0;
			DefinedDampingRatio = false;
			DampingRatio = 0.0;
			Backstop = false;
			Smoothed = false;
		}

		public override object Clone()
		{
			NeedleElement element = (NeedleElement)base.Clone();
			Subject = (Subject)Subject.Clone();
			return element;
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[Needle]");
			Utilities.WriteKey(builder, "Subject", Subject.ToString());
			Utilities.WriteKey(builder, "Location", Location.X, Location.Y);

			if (DefinedRadius)
			{
				Utilities.WriteKey(builder, "Radius", Radius);
			}

			Utilities.WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage));
			Utilities.WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage));
			Utilities.WriteKey(builder, "Color", Color.ToString());
			Utilities.WriteKey(builder, "TransparentColor", TransparentColor.ToString());

			if (DefinedOrigin)
			{
				Utilities.WriteKey(builder, "Origin", Origin.X, Origin.Y);
			}

			Utilities.WriteKey(builder, "InitialAngle", InitialAngle.ToDegrees());
			Utilities.WriteKey(builder, "LastAngle", LastAngle.ToDegrees());
			Utilities.WriteKey(builder, "Minimum", Minimum);
			Utilities.WriteKey(builder, "Maximum", Maximum);

			if (DefinedNaturalFreq)
			{
				Utilities.WriteKey(builder, "NaturalFreq", NaturalFreq);
			}

			if (DefinedDampingRatio)
			{
				Utilities.WriteKey(builder, "DampingRatio", DampingRatio);
			}

			Utilities.WriteKey(builder, "Backstop", Backstop.ToString());
			Utilities.WriteKey(builder, "Smoothed", Smoothed.ToString());
			Utilities.WriteKey(builder, "Layer", Layer);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			XElement needleNode = new XElement("Needle",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				new XElement("Subject", Subject)
			);

			if (!string.IsNullOrEmpty(DaytimeImage))
			{
				needleNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(NighttimeImage))
			{
				needleNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage)));
			}

			needleNode.Add(
				new XElement("TransparentColor", TransparentColor),
				new XElement("Color", Color),
				new XElement("InitialAngle", InitialAngle.ToDegrees()),
				new XElement("LastAngle", LastAngle.ToDegrees()),
				new XElement("Minimum", Minimum),
				new XElement("Maximum", Maximum),
				new XElement("Backstop", Backstop),
				new XElement("Smoothed", Smoothed)
			);

			if (DefinedRadius)
			{
				needleNode.Add(new XElement("Radius", Radius));
			}

			if (DefinedOrigin)
			{
				needleNode.Add(new XElement("Origin", $"{Origin.X}, {Origin.Y}"));
			}

			if (DefinedNaturalFreq)
			{
				needleNode.Add(new XElement("NaturalFreq", NaturalFreq));
			}

			if (DefinedDampingRatio)
			{
				needleNode.Add(new XElement("DampingRatio", DampingRatio));
			}

			parent.Add(needleNode);
		}

		public override void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("Needle",
				new XElement("Location", $"{Location.X}, {Location.Y}"),
				new XElement("Layer", Layer),
				WriteSubjectNode(Subject),
				new XElement("DaytimeImage", DaytimeImage),
				new XElement("NighttimeImage", NighttimeImage),
				new XElement("TransparentColor", TransparentColor),
				new XElement("DefinedRadius", DefinedRadius),
				new XElement("Radius", Radius),
				new XElement("Color", Color),
				new XElement("DefinedOrigin", DefinedOrigin),
				new XElement("Origin", $"{Origin.X}, {Origin.Y}"),
				new XElement("InitialAngle", InitialAngle),
				new XElement("LastAngle", LastAngle),
				new XElement("Minimum", Minimum),
				new XElement("Maximum", Maximum),
				new XElement("DefinedNaturalFreq", DefinedNaturalFreq),
				new XElement("NaturalFreq", NaturalFreq),
				new XElement("DefinedDampingRatio", DefinedDampingRatio),
				new XElement("DampingRatio", DampingRatio),
				new XElement("Backstop", Backstop),
				new XElement("Smoothed", Smoothed)
				));
		}
	}
}
