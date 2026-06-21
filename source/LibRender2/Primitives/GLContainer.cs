using System.Collections.Generic;
using OpenBveApi.Math;

namespace LibRender2.Primitives
{
	public abstract class GLContainer : GLControl
	{
		public readonly List<GLControl> Children = new List<GLControl>();

		protected GLContainer(BaseRenderer renderer) : base(renderer)
		{
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			for (int i = 0; i < Children.Count; i++)
			{
				if (Children[i].IsVisible)
				{
					Children[i].Draw();
				}
			}
		}

		public override void MouseMove(int x, int y)
		{
			if (!IsVisible) return;
			for (int i = 0; i < Children.Count; i++)
			{
				if (Children[i].IsVisible)
				{
					Children[i].MouseMove(x, y);
				}
			}
		}

		public override void MouseDown(int x, int y)
		{
			if (!IsVisible) return;
			for (int i = 0; i < Children.Count; i++)
			{
				if (Children[i].IsVisible)
				{
					Children[i].MouseDown(x, y);
				}
			}
		}
	}
}
