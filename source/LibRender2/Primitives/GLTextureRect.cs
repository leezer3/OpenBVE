using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenBveApi.Textures;

namespace LibRender2.Primitives
{
	public class GLTextureRect : GLControl
	{
		public GLTextureRect(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
			BackgroundColor = Color128.White;
		}

		public override void Draw()
		{
			if (!IsVisible || Texture == null) return;
			Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
		}
	}
}
