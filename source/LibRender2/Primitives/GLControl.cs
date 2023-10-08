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
		/// <summary>The texture for the control</summary>
		public Texture Texture;
		/// <summary>The stored location for the control</summary>
		public Vector2 Location;
		/// <summary>The stored size for the control</summary>
		public Vector2 Size;
		/// <summary>Whether the control is currently selected by the mouse</summary>
		public bool CurrentlySelected;

		protected GLControl(BaseRenderer renderer)
		{
			Renderer = renderer;
		}

		/// <summary>Draws the control</summary>
		public abstract void Draw();

		/// <summary>Passes a mouse move event to the control</summary>
		/// <param name="x">The absolute screen X co-ordinate</param>
		/// <param name="y">The absolute screen Y co-ordinate</param>
		public virtual void MouseMove(int x, int y)
		{

		}
	}
}
