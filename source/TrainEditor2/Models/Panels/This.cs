using System.Text;
using System.Xml.Linq;
using OpenBveApi.Colors;
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
		private double centerX;
		private double centerY;
		private double originX;
		private double originY;

		internal double Resolution
		{
			get
			{
				return resolution;
			}
			set
			{
				SetProperty(ref resolution, value);
			}
		}

		internal double Left
		{
			get
			{
				return left;
			}
			set
			{
				SetProperty(ref left, value);
			}
		}

		internal double Right
		{
			get
			{
				return right;
			}
			set
			{
				SetProperty(ref right, value);
			}
		}

		internal double Top
		{
			get
			{
				return top;
			}
			set
			{
				SetProperty(ref top, value);
			}
		}

		internal double Bottom
		{
			get
			{
				return bottom;
			}
			set
			{
				SetProperty(ref bottom, value);
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

		internal double CenterX
		{
			get
			{
				return centerX;
			}
			set
			{
				SetProperty(ref centerX, value);
			}
		}

		internal double CenterY
		{
			get
			{
				return centerY;
			}
			set
			{
				SetProperty(ref centerY, value);
			}
		}

		internal double OriginX
		{
			get
			{
				return originX;
			}
			set
			{
				SetProperty(ref originX, value);
			}
		}

		internal double OriginY
		{
			get
			{
				return originY;
			}
			set
			{
				SetProperty(ref originY, value);
			}
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
			CenterX = 0.0;
			CenterY = 512.0;
			OriginX = 0.0;
			OriginY = 512.0;
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
			Utilities.WriteKey(builder, "Center", CenterX, CenterY);
			Utilities.WriteKey(builder, "Origin", OriginX, CenterY);
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
				new XElement("Center", $"{CenterX}, {CenterY}"),
				new XElement("Origin", $"{OriginX}, {OriginY}")
			);

			parent.Add(thisNode);
		}
	}
}
