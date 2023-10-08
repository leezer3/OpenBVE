using System;
using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
	public class Button : GLControl
	{
		/// <summary>The text displayed on the button</summary>
		public readonly string Text;
		/// <summary>The highlight color of the button</summary>
		public Color128 HighlightColor;
		/// <summary>The color of the text on the button</summary>
		public Color128 TextColor;
		/// <summary>The font for the button</summary>
		public OpenGlFont Font;

		public Button(BaseRenderer renderer, string text) : base(renderer)
		{
			Text = text;
			Font = Renderer.Fonts.LargeFont;
			Size = Font.MeasureString(Text) * 1.5;
			// default colors to match GLMenu
			BackgroundColor = Color128.Black;
			HighlightColor = Color128.Orange;
			TextColor = Color128.White;
		}

		public override void Draw()
		{
			Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
			if (CurrentlySelected)
			{
				Renderer.Rectangle.Draw(Texture, Location + Size * 0.1, Size - (Size * 0.2), HighlightColor);
			}
			Renderer.OpenGlString.Draw(Font, Text, Location + (Size * 0.15), TextAlignment.TopLeft, TextColor);
		}

		public override void MouseMove(int x, int y)
		{
			CurrentlySelected = x > Location.X && x < Location.X + Size.X && y > Location.Y && y < Location.Y + Size.Y;
		}

		public override void MouseDown(int x, int y)
		{
			MouseMove(x, y);
			if (CurrentlySelected)
			{
				OnClick?.Invoke(this, EventArgs.Empty);
			}
		}

		public EventHandler OnClick;
	}
}
