using OpenBveApi.Colors;

namespace TrainEditor2.Models.Panels
{
	internal class DigitalNumberElement : PanelElement
	{
		private Subject subject;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;
		private int interval;

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

		internal int Interval
		{
			get
			{
				return interval;
			}
			set
			{
				SetProperty(ref interval, value);
			}
		}

		internal DigitalNumberElement()
		{
			Subject = new Subject();
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
			Interval = 0;
		}

		public override object Clone()
		{
			DigitalNumberElement element = (DigitalNumberElement)base.Clone();
			element.Subject = (Subject)Subject.Clone();
			return element;
		}
	}
}
