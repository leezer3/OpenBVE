using OpenBveApi.Colors;

namespace TrainEditor2.Models.Panels
{
	internal class PilotLampElement : PanelElement
	{
		private Subject subject;
		private string daytimeImage;
		private string nighttimeImage;
		private Color24 transparentColor;

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

		internal PilotLampElement()
		{
			Subject = new Subject();
			DaytimeImage = string.Empty;
			NighttimeImage = string.Empty;
			TransparentColor = Color24.Blue;
		}

		public override object Clone()
		{
			PilotLampElement element = (PilotLampElement)base.Clone();
			element.Subject = (Subject)Subject.Clone();
			return element;
		}
	}
}
