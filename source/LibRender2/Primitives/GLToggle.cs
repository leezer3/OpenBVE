using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
	public class GLToggle : GLControl
	{
		public bool Checked;
		public string Text = "";
		public EventHandler ValueChanged;

		public GLToggle(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
			Size = new Vector2(120, 24);
			BackgroundColor = new Color128(0.1f, 0.1f, 0.1f, 1f);
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			Renderer.Rectangle.Draw(null, Location, new Vector2(20, 20), BackgroundColor);
			if (Checked)
			{
				Renderer.Rectangle.Draw(null, new Vector2(Location.X + 4, Location.Y + 4), new Vector2(12, 12), Color128.Orange);
			}
			Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Text, new Vector2(Location.X + 28, Location.Y + 2), TextAlignment.TopLeft, Color128.White);
		}

		public override void MouseDown(int x, int y)
		{
			if (!IsVisible) return;
			if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
			{
				Checked = !Checked;
				ValueChanged?.Invoke(this, EventArgs.Empty);
				OnClick?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
