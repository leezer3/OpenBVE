using OpenBveApi.Colors;

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
			get
			{
				return subject;
			}
			set
			{
				SetProperty(ref subject, value);
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

		internal double Step
		{
			get
			{
				return step;
			}
			set
			{
				SetProperty(ref step, value);
			}
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
			element.Subject = (Subject)Subject.Clone();
			return element;
		}
	}
}
