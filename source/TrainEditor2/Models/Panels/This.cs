using System.Text;
using System.Xml.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using TrainEditor2.Extensions;

namespace TrainEditor2.Models.Panels
{
	internal class This : PanelElement
	{
		private double resolution;
		private double left;
		private double right;
		private double top;
		private double bottom;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;
		private Vector2 center;
		private Vector2 origin;

		internal double Resolution
		{
			get => resolution;
			set => SetProperty(ref resolution, value);
		}

		internal double Left
		{
			get => left;
			set => SetProperty(ref left, value);
		}

		internal double Right
		{
			get => right;
			set => SetProperty(ref right, value);
		}

		internal double Top
		{
			get => top;
			set => SetProperty(ref top, value);
		}

		internal double Bottom
		{
			get => bottom;
			set => SetProperty(ref bottom, value);
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

		internal Vector2 Center
		{
			get => center;
			set => SetProperty(ref center, value);
		}

		internal Vector2 Origin
		{
			get => origin;
			set => SetProperty(ref origin, value);
		}

		internal This()
		{
			Resolution = 1024.0;
			Left = 0.0;
			Right = 1024.0;
			Top = 0.0;
			Bottom = 1024.0;
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
			Center = new Vector2(0.0, 512.0);
			Origin = new Vector2(0.0, 512.0);
		}

		public override void WriteCfg(string fileName, StringBuilder builder)
		{
			builder.AppendLine("[This]");
			Utilities.WriteKey(builder, "Resolution", Resolution);
			Utilities.WriteKey(builder, "Left", Left);
			Utilities.WriteKey(builder, "Right", Right);
			Utilities.WriteKey(builder, "Top", Top);
			Utilities.WriteKey(builder, "Bottom", Bottom);
			Utilities.WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage));
			Utilities.WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage));
			Utilities.WriteKey(builder, "TransparentColor", TransparentColor.ToString());
			Utilities.WriteKey(builder, "Center", Center.X, Center.Y);
			Utilities.WriteKey(builder, "Origin", Origin.X, Center.Y);
		}

		public override void WriteXML(string fileName, XElement parent)
		{
			XElement thisNode = new XElement("This",
				new XElement("Resolution", Resolution),
				new XElement("Left", Left),
				new XElement("Right", Right),
				new XElement("Top", Top),
				new XElement("Bottom", Bottom)
			);

			if (!string.IsNullOrEmpty(DaytimeImage))
			{
				thisNode.Add(new XElement("DaytimeImage", Utilities.MakeRelativePath(fileName, DaytimeImage)));
			}

			if (!string.IsNullOrEmpty(NighttimeImage))
			{
				thisNode.Add(new XElement("NighttimeImage", Utilities.MakeRelativePath(fileName, NighttimeImage)));
			}

			thisNode.Add(
				new XElement("TransparentColor", TransparentColor),
				new XElement("Center", $"{Center.X}, {Center.Y}"),
				new XElement("Origin", $"{Origin.X}, {Origin.Y}")
			);

			parent.Add(thisNode);
		}

		public override void WriteIntermediate(XElement parent)
		{
			parent.Add(new XElement("This",
				new XElement("Resolution", Resolution),
				new XElement("Left", Left),
				new XElement("Right", Right),
				new XElement("Top", Top),
				new XElement("Bottom", Bottom),
				new XElement("DaytimeImage", DaytimeImage),
				new XElement("NighttimeImage", NighttimeImage),
				new XElement("TransparentColor", TransparentColor),
				new XElement("Center", $"{Center.X}, {Center.Y}"),
				new XElement("Origin", $"{Origin.X}, {Origin.Y}")
				));
		}
	}
}
