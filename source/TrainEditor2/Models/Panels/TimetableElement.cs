using OpenBveApi.Colors;

namespace TrainEditor2.Models.Panels
{
	internal class TimetableElement : PanelElement
	{
		private double width;
		private double height;
		private Color24 transparentColor;

		internal double Width
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

		internal double Height
		{
			get
			{
				return height;
			}
			set
			{
				SetProperty(ref height, value);
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

		internal TimetableElement()
		{
			Width = 0.0;
			Height = 0.0;
			TransparentColor = Color24.Blue;
		}
	}
}
