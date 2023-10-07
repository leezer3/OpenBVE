using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace LibRender2.Primitives
{
	/// <summary>An abstract OpenGL based control</summary>
	public abstract class GLControl
	{
		/// <summary>Holds a reference to the base renderer</summary>
		internal readonly BaseRenderer Renderer;
		/// <summary>The background color for the control</summary>
		public Color128 BackgroundColor;
		/// <summary>The texture for the picturebox</summary>
		public Texture Texture;
		/// <summary>The stored location for the control</summary>
		public Vector2 Location;
		/// <summary>The stored size for the control</summary>
		public Vector2 Size;

		protected GLControl(BaseRenderer renderer)
		{
			Renderer = renderer;
		}

		public abstract void Draw();
	}
}
