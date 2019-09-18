using OpenBveApi.Colors;

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
		private double originX;
		private double originY;
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

		internal bool DefinedRadius
		{
			get
			{
				return definedRadius;
			}
			set
			{
				SetProperty(ref definedRadius, value);
			}
		}

		internal double Radius
		{
			get
			{
				return radius;
			}
			set
			{
				SetProperty(ref radius, value);
			}
		}

		internal Color24 Color
		{
			get
			{
				return color;
			}
			set
			{
				SetProperty(ref color, value);
			}
		}

		internal bool DefinedOrigin
		{
			get
			{
				return definedOrigin;
			}
			set
			{
				SetProperty(ref definedOrigin, value);
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

		internal double InitialAngle
		{
			get
			{
				return initialAngle;
			}
			set
			{
				SetProperty(ref initialAngle, value);
			}
		}

		internal double LastAngle
		{
			get
			{
				return lastAngle;
			}
			set
			{
				SetProperty(ref lastAngle, value);
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

		internal bool DefinedNaturalFreq
		{
			get
			{
				return definedNaturalFreq;
			}
			set
			{
				SetProperty(ref definedNaturalFreq, value);
			}
		}

		internal double NaturalFreq
		{

			get
			{
				return naturalFreq;
			}
			set
			{
				SetProperty(ref naturalFreq, value);
			}
		}

		internal bool DefinedDampingRatio
		{
			get
			{
				return definedDampingRatio;
			}
			set
			{
				SetProperty(ref definedDampingRatio, value);
			}
		}

		internal double DampingRatio
		{
			get
			{
				return dampingRatio;
			}
			set
			{
				SetProperty(ref dampingRatio, value);
			}
		}

		internal bool Backstop
		{
			get
			{
				return backstop;
			}
			set
			{
				SetProperty(ref backstop, value);
			}
		}

		internal bool Smoothed
		{
			get
			{
				return smoothed;
			}
			set
			{
				SetProperty(ref smoothed, value);
			}
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
			OriginX = 0.0;
			OriginY = 0.0;
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
			element.Subject = (Subject)Subject.Clone();
			return element;
		}
	}
}
