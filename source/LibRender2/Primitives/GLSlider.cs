using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;

namespace LibRender2.Primitives
{
	public class GLSlider : GLControl
	{
		public double Value = 0.0;
		public double Minimum = 0.0;
		public double Maximum = 100.0;
		public EventHandler ValueChanged;

		private bool dragging = false;

		public GLSlider(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
			Size = new Vector2(160, 20);
			BackgroundColor = new Color128(0.2f, 0.2f, 0.2f, 1f);
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			double centerY = Location.Y + Size.Y / 2.0 - 2.0;
			Renderer.Rectangle.Draw(null, new Vector2(Location.X, centerY), new Vector2(Size.X, 4.0), BackgroundColor);
			
			double ratio = (Value - Minimum) / (Maximum - Minimum);
			double knobX = Location.X + ratio * (Size.X - 12.0);
			Renderer.Rectangle.Draw(null, new Vector2(knobX, Location.Y + 2.0), new Vector2(12.0, Size.Y - 4.0), Color128.Orange);
		}

		public override void MouseMove(int x, int y)
		{
			if (!IsVisible) return;
			if (dragging)
			{
				double localX = System.Math.Max(0.0, System.Math.Min(Size.X - 12.0, x - Location.X - 6.0));
				double ratio = localX / (Size.X - 12.0);
				Value = Minimum + ratio * (Maximum - Minimum);
				ValueChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public override void MouseDown(int x, int y)
		{
			if (!IsVisible) return;
			if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
			{
				dragging = true;
				MouseMove(x, y);
			}
		}
	}
}
