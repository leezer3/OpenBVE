using OpenBveApi.Colors;

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
		private int directionX;
		private int directionY;
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

		internal int DirectionX
		{
			get
			{
				return directionX;
			}
			set
			{
				SetProperty(ref directionX, value);
			}
		}

		internal int DirectionY
		{
			get
			{
				return directionY;
			}
			set
			{
				SetProperty(ref directionY, value);
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
			DirectionX = 0;
			DirectionY = 0;
			Width = 0;
		}

		public override object Clone()
		{
			LinearGaugeElement element = (LinearGaugeElement)base.Clone();
			element.Subject = (Subject)Subject.Clone();
			return element;
		}
	}
}
