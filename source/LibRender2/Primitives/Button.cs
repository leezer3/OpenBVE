using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
	public class Button : GLControl
	{
		/// <summary>The text displayed on the button</summary>
		public string Text;
		/// <summary>The highlight color of the button</summary>
		public Color128 HighlightColor;
		/// <summary>The color of the text on the button</summary>
		public Color128 TextColor;
		/// <summary>The font for the button</summary>
		public OpenGlFont Font;

		public Button(BaseRenderer renderer) : base(renderer)
		{
		}

		public override void Draw()
		{
			Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
			if (CurrentlySelected)
			{
				Renderer.Rectangle.Draw(Texture, Location + 4, Size - 2, HighlightColor);
			}
			Renderer.OpenGlString.Draw(Font, Text, Location, TextAlignment.CenterLeft, TextColor);
		}

		public override void MouseMove(int x, int y)
		{
			if (x > Location.X && x < Location.X + Size.X && y > Location.Y && y < Location.Y + Size.Y)
			{
				CurrentlySelected = true;
			}
		}
	}
}
