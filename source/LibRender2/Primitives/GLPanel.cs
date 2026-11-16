using OpenBveApi.Math;
using OpenBveApi.Colors;

namespace LibRender2.Primitives
{
	public class GLPanel : GLContainer
	{
		public GLPanel(BaseRenderer renderer) : base(renderer)
		{
			BackgroundColor = Color128.Black;
			IsVisible = true;
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
			base.Draw();
		}
	}
}
