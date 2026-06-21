using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
	public class GLNumericUpDown : GLControl
	{
		public float Value = 0f;
		public float Minimum = 0f;
		public float Maximum = 100f;
		public float Increment = 1f;
		public EventHandler ValueChanged;

		public GLNumericUpDown(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
			Size = new Vector2(120, 24);
			BackgroundColor = new Color128(0.15f, 0.15f, 0.15f, 1f);
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X - 30, Size.Y), BackgroundColor);
			Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Value.ToString("0.##"), new Vector2(Location.X + 8, Location.Y + 2), TextAlignment.TopLeft, Color128.White);

			Vector2 upLoc = new Vector2(Location.X + Size.X - 28, Location.Y);
			Vector2 downLoc = new Vector2(Location.X + Size.X - 14, Location.Y);

			Renderer.Rectangle.Draw(null, upLoc, new Vector2(12, Size.Y), new Color128(0.25f, 0.25f, 0.25f, 1f));
			Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, "+", new Vector2(upLoc.X + 2, upLoc.Y + 2), TextAlignment.TopLeft, Color128.White);

			Renderer.Rectangle.Draw(null, downLoc, new Vector2(12, Size.Y), new Color128(0.25f, 0.25f, 0.25f, 1f));
			Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, "-", new Vector2(downLoc.X + 2, downLoc.Y + 2), TextAlignment.TopLeft, Color128.White);
		}

		public override void MouseDown(int x, int y)
		{
			if (!IsVisible) return;
			Vector2 upLoc = new Vector2(Location.X + Size.X - 28, Location.Y);
			Vector2 downLoc = new Vector2(Location.X + Size.X - 14, Location.Y);

			if (x >= upLoc.X && x <= upLoc.X + 12 && y >= upLoc.Y && y <= upLoc.Y + Size.Y)
			{
				Value = System.Math.Min(Maximum, Value + Increment);
				ValueChanged?.Invoke(this, EventArgs.Empty);
				OnClick?.Invoke(this, EventArgs.Empty);
			}
			else if (x >= downLoc.X && x <= downLoc.X + 12 && y >= downLoc.Y && y <= downLoc.Y + Size.Y)
			{
				Value = System.Math.Max(Minimum, Value - Increment);
				ValueChanged?.Invoke(this, EventArgs.Empty);
				OnClick?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
