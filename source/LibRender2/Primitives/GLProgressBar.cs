using OpenBveApi.Math;
using OpenBveApi.Colors;

namespace LibRender2.Primitives
{
	public class GLProgressBar : GLControl
	{
		public float Value = 0f; // 0 to 1
		public Color128 FillColor = Color128.Orange;

		public GLProgressBar(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
			BackgroundColor = new Color128(0.2f, 0.2f, 0.2f, 1f);
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			Renderer.Rectangle.Draw(null, Location, Size, BackgroundColor);
			double fillWidth = Size.X * System.Math.Max(0.0, System.Math.Min(1.0, Value));
			Renderer.Rectangle.Draw(null, Location, new Vector2(fillWidth, Size.Y), FillColor);
		}
	}
}
